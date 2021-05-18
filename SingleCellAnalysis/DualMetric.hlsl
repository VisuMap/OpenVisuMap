// File: DualMetric.hlsl
#define G_SIZE 64

cbuffer GlobalConstants : register(b0) {
	uint N;			 // the number of data bodies, e.g. the number of data rows.
	uint columns;    // the number of data columns.
	uint iBlock;
}

StructuredBuffer<float> dataMatrix : register(t0);
RWStructuredBuffer<float> distMatrix : register(u0);

#define DT(row, col) dataMatrix[col*N+row]

[numthreads(G_SIZE, 1, 1)]
void CorrelationDistance(uint3 gid : SV_GroupId, uint gidx : SV_GroupIndex) {
	uint i = iBlock + gid.x; 
	if (i < N) {
		uint offset = (i - iBlock) * (i + iBlock - 1) / 2;
		for (uint j = gidx; j < i; j += G_SIZE) {
			float prod = 0.0;
			for (uint col = 0; col < columns; col++)
				prod += DT(i, col) * DT(j, col);
            distMatrix[offset + j] = 1.0 - prod;
        }
	}
}

[numthreads(G_SIZE, 1, 1)]
void DotProduct(uint3 gid : SV_GroupId, uint gidx : SV_GroupIndex) {
	uint i = iBlock + gid.x;
	if (i < N) {
		uint offset = (i - iBlock) * (i + iBlock - 1) / 2;
		for (uint j = gidx; j < i; j += G_SIZE) {
			float prod = 0.0;
			for (uint col = 0; col < columns; col++)
				prod += DT(i, col) * DT(j, col);
			distMatrix[offset + j] = prod;
		}
	}
}

[numthreads(G_SIZE, 1, 1)]
void EuclideanDistance(uint3 gid : SV_GroupId, uint gidx : SV_GroupIndex) {
	uint i = iBlock + gid.x;
	if (i < N) {
		uint offset = (i - iBlock) * (i + iBlock - 1) / 2;
		for (uint j = gidx; j < i; j += G_SIZE) {
			float dist = 0.0;
			for (uint col = 0; col < columns; col++) {
				float d = DT(i, col) - DT(j, col);
				dist += d * d;
			}
			distMatrix[offset + j] = sqrt(dist);
		}
	}
}
