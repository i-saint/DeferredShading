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

	ComputeBuffer[] cbConsts = new ComputeBuffer[2];
	SortCB[] consts = new SortCB[1];

	public void Start()
	{
		cbConsts[0] = new ComputeBuffer(16, 1);
		cbConsts[1] = new ComputeBuffer(16, 1);
	}

	public void OnDisable()
	{
		cbConsts[0].Release();
		cbConsts[1].Release();
	}

	public void BitonicSort(ComputeShader sh, ComputeBuffer kip, ComputeBuffer kip_tmp, uint num)
	{
		uint BITONIC_BLOCK_SIZE = 512;
		uint TRANSPOSE_BLOCK_SIZE = 16;


		uint NUM_ELEMENTS = num;
		uint MATRIX_WIDTH = BITONIC_BLOCK_SIZE;
		uint MATRIX_HEIGHT = NUM_ELEMENTS / BITONIC_BLOCK_SIZE;

		GPUSort.KIP[] dbgSourData = new GPUSort.KIP[num];
	//	kip.GetData(dbgSourData);

		for (uint level = 2; level <= BITONIC_BLOCK_SIZE; level <<= 1)
		{
			consts[0].level = level;
			consts[0].levelMask = level;
			consts[0].width = MATRIX_HEIGHT; // not a mistake!
			consts[0].height = MATRIX_WIDTH; // 
			cbConsts[0].SetData(consts);

			sh.SetBuffer(0, "consts", cbConsts[0]);
			sh.SetBuffer(0, "kip_rw", kip);
			sh.Dispatch(0, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);
		}
		kip.GetData(dbgSourData);

		// Then sort the rows and columns for the levels > than the block size
		// Transpose. Sort the Columns. Transpose. Sort the Rows.
		for (uint level = (BITONIC_BLOCK_SIZE << 1); level <= NUM_ELEMENTS; level <<= 1)
		{
			consts[0].level = (level / BITONIC_BLOCK_SIZE);
			consts[0].levelMask = (level & ~NUM_ELEMENTS) / BITONIC_BLOCK_SIZE;
			consts[0].width = MATRIX_WIDTH;
			consts[0].height = MATRIX_HEIGHT;
			cbConsts[0].SetData(consts);

			// Transpose the data from buffer 1 into buffer 2
			sh.SetBuffer(1, "consts", cbConsts[0]);
			sh.SetBuffer(1, "kip", kip);
			sh.SetBuffer(1, "kip_rw", kip_tmp);
			sh.Dispatch(1, (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), 1);

			// Sort the transposed column data
			sh.SetBuffer(0, "consts", cbConsts[0]);
			sh.SetBuffer(0, "kip_rw", kip_tmp);
			sh.Dispatch(0, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);


			consts[0].level = BITONIC_BLOCK_SIZE;
			consts[0].levelMask = level;
			consts[0].width = MATRIX_HEIGHT;
			consts[0].height = MATRIX_WIDTH;
			cbConsts[1].SetData(consts);

			// Transpose the data from buffer 2 back into buffer 1
			sh.SetBuffer(1, "consts", cbConsts[1]);
			sh.SetBuffer(1, "kip", kip_tmp);
			sh.SetBuffer(1, "kip_rw", kip);
			sh.Dispatch(1, (int)(MATRIX_HEIGHT / TRANSPOSE_BLOCK_SIZE), (int)(MATRIX_WIDTH / TRANSPOSE_BLOCK_SIZE), 1);

			// Sort the row data
			sh.SetBuffer(0, "consts", cbConsts[1]);
			sh.SetBuffer(0, "kip_rw", kip);
			sh.Dispatch(0, (int)(NUM_ELEMENTS / BITONIC_BLOCK_SIZE), 1, 1);
		}
		kip.GetData(dbgSourData);
		Debug.Log("BitonicSort()");
	}
}
