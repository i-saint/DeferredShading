using UnityEngine;
using System.Collections;


public class GPUSort
{
	public struct KIP
	{
		public uint key;
		public uint index;
	}
	public struct SortCB
	{
		public uint level;
		public uint levelMask;
		public uint width;
		public uint height;
	}

	static ComputeBuffer consts;

	static void Sort(ComputeShader sh, ComputeBuffer data, ComputeBuffer data_tmp, uint num)
	{
		//const uint BITONIC_BLOCK_SIZE = 512;
		//const uint TRANSPOSE_BLOCK_SIZE = 16;

		//uint NUM_ELEMENTS = num;
		//uint MATRIX_WIDTH = BITONIC_BLOCK_SIZE;
		//uint MATRIX_HEIGHT = NUM_ELEMENTS / BITONIC_BLOCK_SIZE;

		//for (uint level = 2; level <= BITONIC_BLOCK_SIZE; level <<= 1)
		//{
		//	SortCB constants = new SortCB {
		//		level = level,
		//		levelMask = level,
		//		width = MATRIX_HEIGHT, // not mistake 
		//		height = MATRIX_WIDTH  // 
		//	};

		//	// Sort the row data
		//	uint UAVInitialCounts = 0;
		//	//pd3dImmediateContext->CSSetUnorderedAccessViews(0, 1, &inUAV, &UAVInitialCounts);
		//	sh.Dispatch(0, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);
		//}

		//// Then sort the rows and columns for the levels > than the block size
		//// Transpose. Sort the Columns. Transpose. Sort the Rows.
		//for (uint level = (BITONIC_BLOCK_SIZE << 1); level <= NUM_ELEMENTS; level <<= 1)
		//{
		//	SortCB constants1 = new SortCB {
		//		level = (level / BITONIC_BLOCK_SIZE),
		//		levelMask = (level & ~NUM_ELEMENTS) / BITONIC_BLOCK_SIZE,
		//		width = MATRIX_WIDTH,
		//		height = MATRIX_HEIGHT
		//	};

		//	// Transpose the data from buffer 1 into buffer 2
		//	uint UAVInitialCounts = 0;
		//	pd3dImmediateContext->CSSetShaderResources(0, 1, &pViewNULL);
		//	pd3dImmediateContext->CSSetUnorderedAccessViews(0, 1, &tempUAV, &UAVInitialCounts);
		//	pd3dImmediateContext->CSSetShaderResources(0, 1, &inSRV);
		//	sh.Dispatch(1, (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), 1);

		//	// Sort the transposed column data
		//	sh.Dispatch(0, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);

		//	SortCB constants2 = new SortCB {
		//		level = BITONIC_BLOCK_SIZE,
		//		levelMask = level,
		//		width = MATRIX_HEIGHT,
		//		height = MATRIX_WIDTH
		//	};
		//	pd3dImmediateContext->UpdateSubresource(g_pSortCB, 0, NULL, &constants2, 0, 0);

		//	// Transpose the data from buffer 2 back into buffer 1
		//	pd3dImmediateContext->CSSetShaderResources(0, 1, &pViewNULL);
		//	pd3dImmediateContext->CSSetUnorderedAccessViews(0, 1, &inUAV, &UAVInitialCounts);
		//	pd3dImmediateContext->CSSetShaderResources(0, 1, &tempSRV);
		//	sh.Dispatch(1, (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), 1);

		//	// Sort the row data
		//	sh.Dispatch(0, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);
		//}
	}
}
