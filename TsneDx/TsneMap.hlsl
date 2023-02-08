// Copyright (C) 2020 VisuMap Technologies Inc.
//
// File: TsneMap.hlsl
// Description: Shaders for the tSNE mapping algorithm.
//=====================================================================================

#pragma warning( disable : 4714)

//======== Mcros =======================================

#define GROUP_SIZE 1024

#define ForLoop0(gIdx, loopSize, _k, _SZ) for(uint _k=gIdx; _k<(loopSize); _k+=_SZ)

#define ForLoop(gIdx, loopSize, _k) ForLoop0(gIdx, loopSize, _k, GROUP_SIZE)

#define GroupMax0(gIdx, groupMem)  \
	{for(uint offset2=GROUP_SIZE/2; offset2>0; offset2>>=1) { \
	    if ( gIdx < offset2 ) { \
			groupMem[gIdx] = max(groupMem[gIdx], groupMem[gIdx+offset2]); \
		} \
		GroupMemoryBarrierWithGroupSync(); \
	}}

#define GroupSum0(gIdx, groupMem)  \
	{for(uint offset=GROUP_SIZE/2; offset>0; offset>>=1) { \
	    if ( gIdx < offset ) { \
			groupMem[gIdx] += groupMem[gIdx+offset]; \
		} \
		GroupMemoryBarrierWithGroupSync(); \
	}}

#define GroupSum(gIdx, groupMem, result)  GroupSum0(gIdx, groupMem); \
    if ( gIdx == 0 ) result[0] = groupMem[0];

// Sum up a variable value of in threads of a group. The sum will be stored in grShared[0]
#define GROUP_SUM(grShared, grIdx, grVar, grSize)  \
	grShared[grIdx] = grVar; \
	GroupMemoryBarrierWithGroupSync(); \
	for (uint k = grSize / 2; k > 0; k >>= 1) { \
		if (grIdx < k) { \
			grShared[grIdx] += grShared[grIdx + k]; \
		} \
		GroupMemoryBarrierWithGroupSync(); \
	} \

#define IsEpsilon(x) (abs(x)<2.22e-14f) 

//============= structs ====================================================

cbuffer GlobalConstants : register(b0) {
	float targetH;   // the taget entropy for each point.
	uint outDim;
    float PFactor;
	float mom;
	bool cachedP;
	int blockIdx; // needed to split the CalculateP() into multiple batchs to avoid timeout monitor.
	int cmd;      // optional command flag
	uint groupNumber; // optional parameter for the number of dispatched thread groups.
    uint columns;
    uint N;
    int metricType; // 0: Euclidean; 1: correlation;
}

struct VariableStates3 {
	float3 dY;
	float3 gain;
};

struct VariableStates2 {
	float2 dY;
	float2 gain;
};

//============= Buffers ====================================================

StructuredBuffer<float> dataTable : register(t0); // The training data table.
RWStructuredBuffer<float> distanceMatrix : register(u0); // NxN triangle matrix without diagonal elements.

RWStructuredBuffer<float> P_ : register(u1);	// The NxN matrix for P and Q: Upper-right and lower-left traingle part.
RWStructuredBuffer<float> result : register(u2); 
RWStructuredBuffer<float2> Y2 : register(u3);
RWStructuredBuffer<float3> Y3 : register(u4);	
RWStructuredBuffer<VariableStates2> v2 : register(u5);	
RWStructuredBuffer<VariableStates3> v3 : register(u6);
RWStructuredBuffer<float> groupMax : register(u7); 
StructuredBuffer<float> Pcpu : register(t7);   // Needed to for OnCpu Caching mode.	

groupshared float groupValue[GROUP_SIZE];	// to store various accumulated by one group thread.

//=========== misc ===========================================================

#define FLT_MAX 1e38
#define FLT_MIN -1e38
#define TOLERANCE 1e-5
#define eps 2.22e-16f
#define epsilon 500
#define newtonRepeats 25
#define DistanceScale 100.0

#define P(i, j) P_[j*N+i]    // The symmetrical metric P.

// For calculate P on-fly. We will only allocate (2+G_P3_SIZE)*N floats for P_.
#define betaList(i) P_[i<<1]  // i<<1 == 2*i
#define affinityFactor(i) P_[(i<<1)+1]
#define R(tIdx, j) P_[(2+tIdx)*N + j] // auxilary vector for ToAffinity2().

#define minGain 0.01
#define UpdateGain0(v, g) (sign(g) == sign(v.dY)) ? (0.8 * v.gain) : (0.2 + v.gain)
#define UpdateGain2(v, g) v.gain = max(float2(minGain, minGain), UpdateGain0(v, g))
#define UpdateGain3(v, g) v.gain = max(float3(minGain, minGain, minGain), UpdateGain0(v, g))

//=======================================================================================
// returns squared distance between i and j.
float DistanceSquared(uint i, uint j) {
	float sum = 0;
	int kN = 0;
	if (metricType == 0)
	{
		for (uint k = 0; k < columns; k++) {
			float d = dataTable[kN + i] - dataTable[kN + j];
			sum += d * d;
			kN += N;
		}
		return abs(sum);
	}
	else {
		for (uint k = 0; k < columns; k++) {
			sum += dataTable[kN + i] * dataTable[kN + j];
			kN += N;
		}
		float d = 1 - sum;
		return d * d;
	}
}

#define G_SIZE_CACHE 64
[numthreads(G_SIZE_CACHE, 1, 1)]
void CreateDistanceCache(uint3 gid : SV_GroupId, uint gidx : SV_GroupIndex){
    uint i = blockIdx + gid.x; 
    if (i < N) {
        uint offset = i * (i - 1) / 2;
        for (uint j = gidx; j < i; j += G_SIZE_CACHE)
            distanceMatrix[offset + j] = DistanceSquared(i, j);
    }
}

#define G_SIZE_P 64
[numthreads(G_SIZE_P, 1, 1)]
void PartialDistance2(uint3 gid : SV_GroupId, uint gidx : SV_GroupIndex) {
	uint i = blockIdx + gid.x;
	if (i < N) {
		uint offset = (i * (i - 1) - blockIdx * (blockIdx - 1)) / 2;
		for (uint j = gidx; j < i; j += G_SIZE_P) {
			if (metricType == 0) { // for squared euclidean distance
				float s = 0.0;
				for (uint k = 0; k < columns; k++) {
					float d = dataTable[k * N + i] - dataTable[k * N + j];
					s += d * d;
				}
				distanceMatrix[offset + j] = s;
			} else { // for squared correlation distance.
				float s = 0.0;
				for (uint k = 0; k < columns; k++) {
					s += dataTable[k * N + i] * dataTable[k * N + j];
				}
				distanceMatrix[offset + j] = (1 - s) * (1 - s);
			}
		}
	}
}

//=======================================================================================

// Calculate the the entropy of rowIdx-th row of matrix P.
float Entropy(uint rowIdx, float beta) {
    float sumP = 0;
    float h = 0;
    for (uint j = 0; j < N; j++)
        if (j != rowIdx) {
			float Pij = P(rowIdx, j);
			float aff = exp(-Pij * beta);
            sumP += aff;
            h += Pij * aff;
        }
    return IsEpsilon(sumP) ? 0 : (log(sumP) + beta * h / sumP);
}

void ToAffinity(uint rowIdx) {
	float betaLeft = FLT_MIN;
	float betaRight = FLT_MAX;
	float fctLeft = 0;   // function's value at left end of the bracket. Always positive.
	float fctRight = 0;  // function's value at right end of the bracket. Always negative;
	float beta = 1.0;
	for(int tries=newtonRepeats; tries>=0; tries--) {
		float fctZero = Entropy(rowIdx, beta) - targetH;
		if ( abs(fctZero) < TOLERANCE ) break;
		if (fctZero > 0) {
			fctLeft = fctZero;
			betaLeft = beta;
			if (betaRight == FLT_MAX) {
				beta *= 2;
			} else {
				float r = fctLeft / (fctLeft - fctRight);
				beta = (1 - r) * beta + r * betaRight;
			}
		} else {
			fctRight = fctZero;
			betaRight = beta;
			if (betaLeft == FLT_MIN) {
				beta /= 2;
			} else {
				float r = fctLeft / (fctLeft - fctRight);
				beta = (1 - r) * betaLeft + r * beta;
			}
		}
	}

	// Convert rowIdx-th row to affinity with the final beta; and normalize it.
	float sum = 0;
	{
		for(uint j=0; j<N; j++) {
			if ( j != rowIdx ) {
				float aff = exp(-P(rowIdx, j) * beta);
				P(rowIdx, j) = aff;
				sum += aff;
			}
		}
	}

	if ( sum != 0 ) {
		for(uint j=0; j<N; j++) 
			if ( j != rowIdx ) 
				P(rowIdx, j) /= sum;
	}
}

[numthreads(64, 1, 1)]
void CalculatePFromCache(uint3 id : SV_DispatchThreadId) {
	ForLoop0(id.x, N, i, 64*64) {
		for(uint j=i+1; j<N; j++) {
            P(i, j) = distanceMatrix[j * (j - 1) / 2 + i];
        }
	}
}


[numthreads(GROUP_SIZE, 1, 1)]
void CalculateP(uint3 id : SV_DispatchThreadId) {
	if ( cmd == 1 ) {
		ForLoop0(id.x, N, i, 2*GROUP_SIZE) {
			for(uint j=i+1; j<N; j++) {
				P(i, j) = DistanceSquared(i, j);
            }
		}
	}

	if ( cmd == 4 ) {
		float maxV = 0;
		{  // Fill the upper-right triangle.
			ForLoop(id.x, N, i)
				for(uint j=i+1; j<N; j++) 
					maxV = max(P(i,j), maxV);
			groupValue[id.x] = maxV;
			GroupMemoryBarrierWithGroupSync();
		}
		GroupMax0(id.x, groupValue);
	
		// normalize the triangle matrix
        maxV = DistanceScale / groupValue[0];
		ForLoop(id.x, N, i) {
			for(uint j=i+1; j<N; j++) {
				P(i, j) *= maxV;
				P(j, i) = P(i, j);
			}
		}
	}

	if (cmd == 2) {
		uint rowIdx = id.x + blockIdx;
		if (rowIdx < N)
			ToAffinity(rowIdx);
	}

	if ( cmd == 3 ) {	
		// Symmetrize the whole matrix.
		float sum = 0;
		ForLoop(id.x, N, i) {
			for(uint j=i+1; j<N; j++) {
				P(i, j) += P(j,i);
				sum += P(i, j);
			}
		}
		groupValue[id.x] = sum;
		GroupMemoryBarrierWithGroupSync();
		GroupSum0(id.x, groupValue);
	
		// Normalize again the upper right triangle matrix.
		sum = 2*groupValue[0];
		ForLoop(id.x, N, ii) {
			for(uint j=ii+1; j<N; j++) {
				P(j, ii) = P(ii, j) = max(P(ii,j)/sum, eps);	
			}
		}
	}
}


//=======================================================================================

float Entropy2(uint rowIdx, uint tIdx, float beta) {
    float sumP = 0;
    float h = 0;
    for (uint j = 0; j < N; j++)
        if (j != rowIdx) {
			float Pij = R(tIdx, j);
			float aff = exp(-Pij * beta);
            sumP += aff;
            h += Pij * aff;
        }
    return IsEpsilon(sumP) ? 0 : (log(sumP) + beta * h / sumP);
}

float2 ToAffinity2(uint rowIdx, uint gid) {
	float betaLeft = FLT_MIN;
	float betaRight = FLT_MAX;
	float fctLeft = 0;   // function's value at left end of the bracket. Always positive.
	float fctRight = 0;  // function's value at right end of the bracket. Always negative;
	float beta = 1.0;

	for (int tries = newtonRepeats; tries >= 0; tries--) {
		float fctZero = Entropy2(rowIdx, gid, beta) - targetH;
		if (abs(fctZero) < TOLERANCE) break;
		if (fctZero > 0) {
			fctLeft = fctZero;
			betaLeft = beta;
			if (betaRight == FLT_MAX) {
				beta *= 2;
			}
			else {
				float r = fctLeft / (fctLeft - fctRight);
				beta = (1 - r) * beta + r * betaRight;
			}
		}
		else {
			fctRight = fctZero;
			betaRight = beta;
			if (betaLeft == FLT_MIN) {
				beta /= 2;
			}
			else {
				float r = fctLeft / (fctLeft - fctRight);
				beta = (1 - r) * betaLeft + r * beta;
			}
		}
	}

	// Convert rowIdx-th row to affinity with the final beta; and normalize it.
	float sum = 0;
	{
		for (uint j = 0; j < N; j++) {
			if (j != rowIdx) {
				sum += exp(-R(gid, j) * beta);
			}
		}
	}
	return float2(beta, sum);
}


//Calculate the sumQ and store it into result[1].
#define G_P2_SIZE 64
[numthreads(G_P2_SIZE, 1, 1)]
void CalculateSumQ(uint3 gid : SV_GroupId, uint gidx : SV_GroupIndex) {
	// gid.x is the group index; gidx is the thread index within group.
	// Each thread group will calculate sum of one line and add it to groupMax[gid.x].
	if (blockIdx >= 0) {
		uint i = blockIdx + gid.x;
		if (i < N) {
			float sum = 0;
			if (outDim == 3) {
                float3 Y3i = Y3[i];
                for (uint j = gidx; j < i; j += G_P2_SIZE) {
                    float3 d = Y3i - Y3[j];
                    sum += 1.0 / (1.0 + dot(d, d));
                }
			}
			else {
                float2 Y2i = Y2[i];
                for (uint j = gidx; j < i; j += G_P2_SIZE) {
                    float2 d = Y2i - Y2[j];
                    sum += 1.0 / (1.0 + dot(d, d));
                }
			}

			GROUP_SUM(groupValue, gidx, sum, G_P2_SIZE);
			if (gidx == 0) {
				if (blockIdx == 0)
					groupMax[gid.x] = groupValue[0];
				else
					groupMax[gid.x] += groupValue[0];
			}
		}
	}
	else {
		// sum up the partial sums in groupMax[0..groupNumber]. 
		if ( (gid.x == 0) && (gidx==0)) {
			float sum = 0;
			for (uint i = 0; i < groupNumber; i++)
				sum += groupMax[i];
			result[1] = sum;
		}
	}
}

#define G_P3_SIZE 32
[numthreads(G_P3_SIZE, 1, 1)]
void InitializeP3(uint3 id : SV_DispatchThreadId, uint3 gid : SV_GroupId, uint gIdx : SV_GroupIndex) {
	if (cmd == 1) {
		// Calculate the max of sequared distance and store them into groupMax[].
		float maxDist2 = (blockIdx == 0) ? 0 : groupMax[gid.x];
		uint i = blockIdx + gid.x;
		if (i < N) {
			for (uint j = i + 1; j < N; j += G_P3_SIZE) {
                maxDist2 = max(DistanceSquared(i, j), maxDist2);
            }
		}
		groupValue[gIdx] = maxDist2;
		GroupMemoryBarrierWithGroupSync();
		for (uint k = G_P3_SIZE / 2; k > 0; k >>= 1) {
			if (gIdx < k) {
				groupValue[gIdx] = max(groupValue[gIdx], groupValue[gIdx + k]);
			}
			GroupMemoryBarrierWithGroupSync();
		}
		if (gIdx == 0) 
			groupMax[gid.x] = groupValue[0];
		DeviceMemoryBarrier();
	} else if (cmd == 2) {
		if (id.x == 0) {
			//Calculate the maximal value of groupMax[] into maxV and set result[0] to DistanceScale/maxV.
			float maxV = eps;
			for (uint i = 0; i < groupNumber; i++)
				maxV = max(maxV, groupMax[i]);
            result[0] = DistanceScale / maxV; // save the distanceFactor into result[0] for latter use.
        }
		DeviceMemoryBarrier();
	} else if (cmd == 3) {
		uint i = blockIdx + gid.x;
		if (i < N) {
            // result[0] is the distanceFactor calculated in the initial call.
			float distanceFactor = result[0];
			// Initialize the squared distances to the rowIdx-th element in to R vector.
			for (uint j = gIdx; j < N; j += G_P3_SIZE) {
				if (j != i) {
					R(gid.x, j) = distanceFactor * DistanceSquared(i, j);
                }
			}
			groupValue[gIdx] = 1.0;
			GroupMemoryBarrierWithGroupSync();

			if (gIdx == 0) {
				float2 ret = ToAffinity2(i, gid.x); 
				betaList(i) = distanceFactor * ret[0];
				float v = (ret[1] == 0) ? 1e22 : 1.0 / ret[1];
				affinityFactor(i) = isinf(v) ? 1e22 : v;
			}
		}
		DeviceMemoryBarrier();
	} else if (cmd == 4) {		
		uint i = gid.x + blockIdx;
		if (i < N) {
			float sum = 0;
			for (uint j = gIdx; j < i; j+=G_P3_SIZE) {
				float v = DistanceSquared(i, j);
				sum += exp(-v * betaList(i)) * affinityFactor(i) + exp(-v * betaList(j)) * affinityFactor(j);
			}
			GROUP_SUM(groupValue, gIdx, sum, G_P3_SIZE);
			if (gIdx == 0) {
				if (blockIdx == 0)
					groupMax[gid.x] = groupValue[0];
				else
					groupMax[gid.x] += groupValue[0];
			}
		}
		DeviceMemoryBarrier();
	}
}

//Notice: groupNumber must be smaller than GROUP_SIZE.
[numthreads(GROUP_SIZE, 1, 1)]
void InitializeP(uint3 id : SV_DispatchThreadId, uint gidx : SV_GroupIndex) {
	groupValue[id.x] = (id.x < groupNumber) ? groupMax[id.x] : 0.0;
	GroupMemoryBarrierWithGroupSync();
	GroupSum0(id.x, groupValue);
	float fct = 1.0 / (2 * groupValue[0]);

	ForLoop(id.x, N, k) {
		affinityFactor(k) *= fct;
	}
}

//=================================================================================================

float PP(uint i, uint j) {
    float d = DistanceSquared(i, j);
	return max(eps, affinityFactor(i) * exp(-d * betaList(i)) + affinityFactor(j) * exp(-d * betaList(j)));
}

// Large groupshared dataRow[][] will significantly reduce the performance.
#define GR_SIZE 64
#define MAX_DIMENSION  64
#define MAX_DIMENSIONs 32
groupshared float dataRow[MAX_DIMENSION][GR_SIZE];
groupshared float dataRowS[MAX_DIMENSIONs][GR_SIZE];

float PP2(uint gidx, float a_i, float b_i, uint j) {
	float p = 0;
	if (metricType == 0) {
		for (uint k = 0; k < columns; k++) {
			float d = dataRow[k][gidx] - dataTable[k*N + j];
			p += d * d;
		}
		p = abs(p);
	} else {
		for (uint k = 0; k < columns; k++)
			p += dataRow[k][gidx] * dataTable[k*N + j];
		p = 1 - p;
		p *= p;
	}
	return max(eps, a_i * exp(-p * b_i) + affinityFactor(j) * exp(-p * betaList(j)));
}

float PP2s(uint gidx, float a_i, float b_i, uint j) {
	float p = 0;
	if (metricType == 0) {
		for (uint k = 0; k < columns; k++) {
			float d = dataRowS[k][gidx] - dataTable[k*N + j];
			p += d * d;
		}
		p = abs(p);
	} else {
		for (uint k = 0; k < columns; k++)
			p += dataRowS[k][gidx] * dataTable[k*N + j];
		p = 1 - p;
		p *= p;
	}
	return max(eps, a_i * exp(-p * b_i) + affinityFactor(j) * exp(-p * betaList(j)));
}

//==================================================================================================

// Returns the image distance between i and j-th data point.
float Q(uint i, uint j) {
	if (outDim == 3) {
		float3 d = Y3[i] - Y3[j];
		return 1 / (1 + dot(d, d));
	}
	else {
		float2 d = Y2[i] - Y2[j];
		return 1 / (1 + dot(d, d));
	}
}

[numthreads(GROUP_SIZE, 1, 1)]
void CurrentCost(uint3 id : SV_DispatchThreadId) {
	float sumQ = result[1];
	float costAux = 0;
	ForLoop(id.x, N, i) {
		for (uint j = 0; j < i; j++) {
			float Pij = P(i,j);
			costAux += Pij * log(Pij * sumQ / Q(i, j));
		}
	}
	groupValue[id.x] = costAux;
	GroupMemoryBarrierWithGroupSync();
	GroupSum(id.x, groupValue, result);
}

#define G_COST_SZ 32
[numthreads(G_COST_SZ, 1, 1)]
void CurrentCostLarge(uint3 gid : SV_GroupId, uint gIdx : SV_GroupIndex) {
	if (cmd == 1) {
		float sumQ = result[1];
		float costAux = 0;   
		uint i = blockIdx + gid.x;
		if (i < N) {
			for (uint j = gIdx; j < i; j+= G_COST_SZ) {
				float Pji = PP(j, i);
				costAux += Pji * log(Pji * sumQ / Q(i, j));
			}
		}		
		GROUP_SUM(groupValue, gIdx, costAux, G_COST_SZ);
		if (gIdx == 0) {
			if (blockIdx==0)
				groupMax[gid.x] = groupValue[0];
			else
				groupMax[gid.x] += groupValue[0];
		}
	} else if (cmd==2) {
		if (gIdx == 0) {
			float costAux = 0;
			for (uint i = 0; i < groupNumber; i++)
				costAux += groupMax[i];
			result[0] = costAux;
		}
	}
}

//=======================================================================================

#define GROUP_SZ 128
[numthreads(GROUP_SZ, 1, 1)]
void OneStep(uint3 id : SV_DispatchThreadId) {   
	float sumQ = result[1];
	float sumQ_next = (blockIdx==0) ? 0 : groupMax[id.x];

	uint i = id.x + blockIdx;
	if ( i < N ) {
		if (outDim == 3 ) {
			float3 gradient = float3(0, 0, 0);
			for (uint j = 0; j < N; j++) {
				if (j != i ) {
					float3 d = Y3[i] - Y3[j];
					float Qij = 1/(1+dot(d,d));
					sumQ_next += Qij;
					Qij /= sumQ;
					gradient += mad(PFactor, P(i, j), -Qij) * Qij * d;
				}
			}
			gradient *= sumQ * 4 * epsilon;
			UpdateGain3(v3[i], gradient);
			v3[i].dY = mom * v3[i].dY - v3[i].gain * gradient;
			groupMax[id.x] = sumQ_next;
		} else {  
			float2 gradient = float2(0, 0);
			for (uint j = 0; j < N; j++) {
				if (j != i) {
					float2 d = Y2[i] - Y2[j];
					float Qij = 1/(1+dot(d,d));
					sumQ_next += Qij;
					Qij /= sumQ;
					gradient += mad(PFactor, P(i, j), -Qij) * Qij * d;
				}
			}
			gradient *= sumQ * 4 * epsilon;
			UpdateGain2(v2[i], gradient);
			v2[i].dY = mom * v2[i].dY - v2[i].gain * gradient;
			groupMax[id.x] = sumQ_next;
		}
	}
}

#define G_SIZE 128
#define groupSumQ groupValue
#define groupSumQ_next groupMax
groupshared float3 groupGradient[G_SIZE];	// to store various accumulated by one group thread.

[numthreads(G_SIZE, 1, 1)]
void OneStepNoCache(uint3 gid : SV_GroupId, uint gIdx : SV_GroupIndex) {
	uint i = gid.x + blockIdx;
	float sumQ = result[1];
	if ((blockIdx == 0) && (gIdx == 0)) {
		groupSumQ_next[gid.x] = 0;
	}

	if (i < N) {
		if (outDim == 3) {
			float3 gradient = float3(0, 0, 0);
			float sQ = 0;
			for (uint j = gIdx; j < N; j += G_SIZE) {
				if (j != i) {
					float3 d = Y3[i] - Y3[j];
					float Qij = 1 / (1 + dot(d, d));
					sQ += Qij;
					Qij /= sumQ;
					gradient += mad(PFactor, PP(i, j), -Qij) * Qij * d;
				}
			}
			groupGradient[gIdx] = gradient;
			groupSumQ[gIdx] = sQ;
			GroupMemoryBarrierWithGroupSync();
			for (uint k = G_SIZE / 2; k > 0; k >>= 1) {
				if (gIdx < k) {
					groupSumQ[gIdx] += groupSumQ[gIdx + k];
					groupGradient[gIdx] += groupGradient[gIdx + k];
				}
				GroupMemoryBarrierWithGroupSync();
			}
			if (gIdx == 0) { //let the first thread of the group finish the updating and store sumQ into groupSumQ_next[]
				groupSumQ_next[gid.x] += groupSumQ[0];
				gradient = groupGradient[0];
				gradient *= sumQ * 4 * epsilon;
				UpdateGain3(v3[i], gradient);
				v3[i].dY = mom * v3[i].dY - v3[i].gain * gradient;
			}
		}
		else {
			float2 gradient = float2(0, 0);
			float sQ = 0;
			for (uint j = gIdx; j < N; j += G_SIZE) {
				if (j != i) {
					float2 d = Y2[i] - Y2[j];
					float Qij = 1 / (1 + dot(d, d));
					sQ += Qij;
					Qij /= sumQ;
					gradient += mad(PFactor, PP(i, j), -Qij) * Qij * d;
				}
			}
			groupGradient[gIdx].xy = gradient;
			groupSumQ[gIdx] = sQ;
			GroupMemoryBarrierWithGroupSync();
			for (uint k = G_SIZE / 2; k > 0; k >>= 1) {
				if (gIdx < k) {
					groupSumQ[gIdx] += groupSumQ[gIdx + k];
					groupGradient[gIdx].xy += groupGradient[gIdx + k].xy;
				}
				GroupMemoryBarrierWithGroupSync();
			}
			if (gIdx == 0) { //let the first thread of the group finish the updating and store sumQ into groupSumQ_next[]
				groupSumQ_next[gid.x] += groupSumQ[0];
				gradient = groupGradient[0].xy;
				gradient *= sumQ * 4 * epsilon;
				UpdateGain2(v2[i], gradient);
				v2[i].dY = mom * v2[i].dY - v2[i].gain * gradient;
			}
		}
	}
}


[numthreads(GR_SIZE, 1, 1)]
void FastStep(uint3 id : SV_DispatchThreadId, uint gidx : SV_GroupIndex) {   
	float sumQ = result[1];
	float sumQ_next = (blockIdx==0) ? 0 : groupMax[id.x];

	uint i = id.x + blockIdx;
	if ( i < N ) {
		for(uint k=0; k<columns; k++) 
			dataRow[k][gidx] = dataTable[k*N+i];
		float a_i = affinityFactor(i);  // to save some global memory access.
		float b_i = betaList(i);

		if (outDim == 3 ) {
			float3 gradient = float3(0, 0, 0);
			for (uint j = 0; j < N; j++) {
				if (j != i ) {
					float3 d = Y3[i] - Y3[j];
					float Qij = 1/(1+dot(d,d));
					sumQ_next += Qij;
					Qij /= sumQ;
					gradient += (PFactor * PP2(gidx, a_i, b_i, j) - Qij ) * Qij * d;
				}
			}
			gradient *= sumQ * 4 * epsilon;
			UpdateGain3(v3[i], gradient);
			v3[i].dY = mom * v3[i].dY - v3[i].gain * gradient;
			groupMax[id.x] = sumQ_next;
		} else {
			float2 gradient = float2(0, 0);
			for (uint j = 0; j < N; j++) {
				if (j != i) {
					float2 d = Y2[i] - Y2[j];
					float Qij = 1/(1+dot(d,d));
					sumQ_next += Qij;
					Qij /= sumQ;
					gradient += (PFactor * PP2(gidx, a_i, b_i, j) - Qij) * Qij * d;
				}
			}
			gradient *= sumQ * 4 * epsilon;
			UpdateGain2(v2[i], gradient);
			v2[i].dY = mom * v2[i].dY - v2[i].gain * gradient;
			groupMax[id.x] = sumQ_next;
		}
	}
}

[numthreads(GR_SIZE, 1, 1)]
void FastStepS(uint3 id : SV_DispatchThreadId, uint gidx : SV_GroupIndex) {
	float sumQ = result[1];
	float sumQ_next = (blockIdx == 0) ? 0 : groupMax[id.x];

	uint i = id.x + blockIdx;
	if (i < N) {
		for (uint k = 0; k < columns; k++)
			dataRowS[k][gidx] = dataTable[k*N + i];
		float a_i = affinityFactor(i);  // to save some global memory access.
		float b_i = betaList(i);

		if (outDim == 3) {
			float3 gradient = float3(0, 0, 0);
			for (uint j = 0; j < N; j++) {
				if (j != i) {
					float3 d = Y3[i] - Y3[j];
					float Qij = 1 / (1 + dot(d, d));
					sumQ_next += Qij;
					Qij /= sumQ;
					gradient += (PFactor * PP2s(gidx, a_i, b_i, j) - Qij) * Qij * d;
				}
			}
			gradient *= sumQ * 4 * epsilon;
			UpdateGain3(v3[i], gradient);
			v3[i].dY = mom * v3[i].dY - v3[i].gain * gradient;
			groupMax[id.x] = sumQ_next;
		}
		else {
			float2 gradient = float2(0, 0);
			for (uint j = 0; j < N; j++) {
				if (j != i) {
					float2 d = Y2[i] - Y2[j];
					float Qij = 1 / (1 + dot(d, d));
					sumQ_next += Qij;
					Qij /= sumQ;
					gradient += (PFactor * PP2s(gidx, a_i, b_i, j) - Qij) * Qij * d;
				}
			}
			gradient *= sumQ * 4 * epsilon;
			UpdateGain2(v2[i], gradient);
			v2[i].dY = mom * v2[i].dY - v2[i].gain * gradient;
			groupMax[id.x] = sumQ_next;
		}
	}
}


//=================================================================================================

#define G_SumUp_SZ 32
[numthreads(G_SumUp_SZ, 1, 1)]
void OneStepSumUp(uint gidx : SV_GroupIndex) {
	float changes = 0;
	if (outDim == 3) {
		for (uint i = gidx; i < N; i += G_SumUp_SZ) {
			Y3[i] += v3[i].dY;
			changes += length(v3[i].dY);
		}
	}
	else {
		for (uint i = gidx; i < N; i += G_SumUp_SZ) {
            if (outDim == 1)
                Y2[i].x += v2[i].dY.x;
            else
			    Y2[i] += v2[i].dY;
			changes += length(v2[i].dY);
		}
	}

	GROUP_SUM(groupValue, gidx, changes, G_SumUp_SZ)
		if (gidx == 0) {
			result[2] = groupValue[0];
			float sum = 0;
			for (uint i = 0; i < groupNumber; i++)
				sum += groupMax[i];
			result[1] = sum;  // set sumQ for the next loop.
		}
}

// ================================================================================
#define GROUP_SZ_HYP 32

#define PPtr(i, j) P_[(i)*N + (j)]     // Access P in normal matrix order.
#define PP2tr(i, j) Pcpu[(i)*N+(j)]    // Used when caching P on CPU.

float Entropy3(uint rowIdx, uint refJ, float beta) {
	float sumP = 0;
	float h = 0;
	for (uint j = 0; j < N; j++) {
		if (j != refJ) {
			float Pij = PPtr(rowIdx, j);
			float aff = exp(-Pij * beta);
			sumP += aff;
			h += Pij * aff;
		}
	}
	return IsEpsilon(sumP) ? 0 : (log(sumP) + beta * h / sumP);
}

// Convert a block of symmetrized and scaled distance matrix distanceMatrix[] to affinity.
[numthreads(GROUP_SZ_HYP, 1, 1)]
void Dist2Affinity(uint3 id : SV_DispatchThreadId) {
	uint rowIdx = id.x;
	uint refJ = blockIdx + id.x;
	if (refJ < N) {
		float betaLeft = FLT_MIN;
		float betaRight = FLT_MAX;
		float fctLeft = 0; // function's value at left end of the bracket. Always positive.
		float fctRight = 0; // function's value at right end of the bracket. Always negative;
		float beta = 1.0;
		for (int tries = newtonRepeats; tries >= 0; tries--) {
			float fctZero = Entropy3(rowIdx, refJ, beta) - targetH;
			if (abs(fctZero) < TOLERANCE)
				break;
			if (fctZero > 0) {
				fctLeft = fctZero;
				betaLeft = beta;
				if (betaRight == FLT_MAX) {
					beta *= 2;
				}
				else {
					float r = fctLeft / (fctLeft - fctRight);
					beta = (1 - r) * beta + r * betaRight;
				}
			}
			else {
				fctRight = fctZero;
				betaRight = beta;
				if (betaLeft == FLT_MIN) {
					beta /= 2;
				}
				else {
					float r = fctLeft / (fctLeft - fctRight);
					beta = (1 - r) * betaLeft + r * beta;
				}
			}
		}

		// Convert rowIdx-th row to affinity with the final beta; and normalize it.
		float sum = 0;
		for (uint j = 0; j < N; j++) {
			if (j != refJ) {
				float aff = exp(-PPtr(rowIdx, j) * beta);
				PPtr(rowIdx, j) = aff;
				sum += aff;
			}
		}

		if (sum != 0) {
			for (uint j = 0; j < N; j++)
				if (j != refJ)
					PPtr(rowIdx, j) /= sum;
		}
	}
}

[numthreads(GROUP_SZ_HYP, 1, 1)]
void OneStepCpuCache(uint3 id : SV_DispatchThreadId) {
	float sumQ = result[1];
	float sumQ_next = (blockIdx == 0) ? 0 : groupMax[id.x];
	uint i = id.x + blockIdx;
	if (i >= N) return;

	if (outDim != 3) {
		float2 gradient = float2(0, 0);
		for (uint j = 0; j < i; j++) {
			float2 d = Y2[i] - Y2[j];
			float Qij = 1 / (1 + dot(d, d));
			sumQ_next += Qij;
			Qij /= sumQ;
			gradient += mad(PFactor, PP2tr(id.x, j), -Qij) * Qij * d;
		}
		for (j = i + 1; j < N; j++) {
			float2 d = Y2[i] - Y2[j];
			float Qij = 1 / (1 + dot(d, d));
			sumQ_next += Qij;
			Qij /= sumQ;
			gradient += mad(PFactor, PP2tr(id.x, j), -Qij) * Qij * d;
		}

		gradient *= sumQ * 4 * epsilon;
		UpdateGain2(v2[i], gradient);
		v2[i].dY = mom * v2[i].dY - v2[i].gain * gradient;
		groupMax[id.x] = sumQ_next;
	}
	else {
		float3 gradient = float3(0, 0, 0);
		for (uint j = 0; j < i; j++) {
			float3 d = Y3[i] - Y3[j];
			float Qij = 1 / (1 + dot(d, d));
			sumQ_next += Qij;
			Qij /= sumQ;
			gradient += mad(PFactor, PP2tr(id.x, j), -Qij) * Qij * d;
		}
		for (j = i + 1; j < N; j++) {
			float3 d = Y3[i] - Y3[j];
			float Qij = 1 / (1 + dot(d, d));
			sumQ_next += Qij;
			Qij /= sumQ;
			gradient += mad(PFactor, PP2tr(id.x, j), -Qij) * Qij * d;
		}
		gradient *= sumQ * 4 * epsilon;
		UpdateGain3(v3[i], gradient);
		v3[i].dY = mom * v3[i].dY - v3[i].gain * gradient;
		groupMax[id.x] = sumQ_next;
	}
}
