// Copyright (C) 2020 VisuMap Technologies Inc.
//
// File: FastPca.hlsl
//
// shader to calculates eigen-decoposition with power method.

cbuffer GlobalConstants : register(b0) {
	uint rows;		 
	uint columns;
	uint iBlock; // Needed to split the matrix into blocks to avoid timeout problems.
	uint eigenCount;
	uint eigenIdx;
	uint groupNumber;
	float covFactor; // constant to covert dot-product to covariance.
}

StructuredBuffer<float> dataTable : register(t0);   // The training data table.
RWStructuredBuffer<float> covMatrix : register(u0); // rowsxrows triangle matrix without diagonal elements.
RWStructuredBuffer<float> result : register(u1);
RWStructuredBuffer<float> eVector : register(u2);   // The iterated eigen vector.
RWStructuredBuffer<float> eVector2 : register(u3);  // The iterated eigen vector.

StructuredBuffer<float> eigenList1 : register(t1);   // 
RWStructuredBuffer<float> eigenList2 : register(u4); //  used to store the transposed eigen-vectors.

#define G_SIZE 32
#define G_SIZE2 64
#define dataT(row, col) dataTable[(row) + (col)*(rows)]    // Using the column-major layout for better performance.
#define covM(row, col)  covMatrix[(row) + (rows) * (col)]

//SEC_SIZE is the size of a section in a row of dataT() table.
#define SEC_SIZE 1024
groupshared float gValues[SEC_SIZE];

[numthreads(G_SIZE2, 1, 1)]
void PcaCreateCovMatrix(uint3 gid : SV_GroupId, uint gidx: SV_GroupIndex) {
	uint i = iBlock + gid.x;  // each thread group will be calculate one line. 
	if (i < rows) {
		// Calculate covM(i, 0:i+1) with the current thread-group:
		for (uint p=0; p<columns; p+=SEC_SIZE) {
			uint q = min(columns, p+SEC_SIZE);
			// The first thread of each group load dataT(i, p:q) into sumCov[]
			if (gidx == 0) {
				for (uint k = p; k < q; k++)
					gValues[k - p] = dataT(i, k);
			}
			GroupMemoryBarrierWithGroupSync();

			// Each thread dot-product sumCov[] the corresponding section in j-th row of dataT().
			for (uint j = gidx; j <= i; j += G_SIZE2) {
				float sum = 0.0;
				for (uint k = p; k < q; k++)
					sum += gValues[k - p] * dataT(j, k);
				covM(i, j) = ((p == 0) ? 0 : covM(i, j)) + sum;
			}
			DeviceMemoryBarrier();
		}

		for (uint j = gidx; j <= i; j += G_SIZE2) {
			covM(i, j) *= covFactor;
			if ( i != j)
				covM(j, i) = covM(i, j);
		}
	}
}

[numthreads(1, 1, 1)]
void PcaInitIteration()
{
	float nr = 0.0;
	for (uint k = 0; k < rows; k++) {
		eVector[k] = covM(eigenIdx, k);
		nr += eVector[k] * eVector[k];
	}
	nr = sqrt(nr);
	if (nr <= 0) {
		float e = sqrt(1.0 / rows);
		for (uint k = 0; k < rows; k++)
			eVector[k] = e;
	}
	else {
		for (uint k = 0; k < rows; k++)
			eVector[k] /= nr;
	}
	result[0] = nr;
}

[numthreads(G_SIZE2, 1, 1)]
void PcaIterateOneStep(uint3 id : SV_DispatchThreadId, uint3 gid : SV_GroupId, uint gidx : SV_GroupIndex)
{
	for (uint row = id.x; row < rows; row += groupNumber * G_SIZE2) {
		float s = 0.0;
		for (uint k = 0; k < rows; k++)
			s += covM(row, k) * eVector[k];
		eVector2[row] = s;
	}
}

#define GroupSum(tId, G, SZ)  \
	for(uint offset=SZ/2; offset>0; offset>>=1) { \
	    if ( tId < offset ) \
			G[tId] += G[tId+offset]; \
		GroupMemoryBarrierWithGroupSync(); }

#define G_SIZE3 1024
groupshared float sumNormal[G_SIZE3];
[numthreads(G_SIZE3, 1, 1)]
void PcaCalculateNormal(uint3 id : SV_DispatchThreadId)
{
	sumNormal[id.x] = 0;
	for (uint row = id.x; row < rows; row += G_SIZE3)
		sumNormal[id.x] += eVector2[row] * eVector2[row];
	GroupMemoryBarrierWithGroupSync();
	GroupSum(id.x, sumNormal, G_SIZE3);
	if (id.x == 0)
		result[0] = sqrt(sumNormal[0]);	
	DeviceMemoryBarrier();
	for (row = id.x; row < rows; row += G_SIZE3)
		eVector[row] = eVector2[row] / result[0];
}


[numthreads(G_SIZE, 1, 1)]
void PcaAdjustCovMatrix(uint3 id : SV_DispatchThreadId)
{
	for (uint row = id.x; row < rows; row += groupNumber * G_SIZE) {
		float c = result[0] * eVector[row];
		for (uint col = 0; col < rows; col++)
			covM(row, col) -= c * eVector[col] ;
	}
}

#define G_SIZE4 128

[numthreads(G_SIZE4, 1, 1)]
void PcaTransposeEigenvectors(uint3 gid : SV_GroupId, uint gidx : SV_GroupIndex)
{
	for (uint row = gid.x; row < eigenCount; row += groupNumber)
	for (uint col = gidx; col < columns; col += G_SIZE4) {
		float v = 0.0;
		int offset = row * rows;
		for (uint k = 0; k < rows; k++)
			v += eigenList1[offset + k] * dataT(k, col);
		eigenList2[row * columns + col] = v;
	}
}
