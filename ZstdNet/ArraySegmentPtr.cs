using System;
using System.Runtime.InteropServices;

namespace ZstdNet
{
	// Token: 0x02000002 RID: 2
	internal class ArraySegmentPtr : IDisposable
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public ArraySegmentPtr(ArraySegment<byte> segment)
		{
			this.handle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
			this.arr = segment.Array;
			this.offset = segment.Offset;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002085 File Offset: 0x00000285
		public static implicit operator IntPtr(ArraySegmentPtr pinner)
		{
			return Marshal.UnsafeAddrOfPinnedArrayElement(pinner.arr, pinner.offset);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002098 File Offset: 0x00000298
		public void Dispose()
		{
			this.handle.Free();
		}

		// Token: 0x04000001 RID: 1
		private GCHandle handle;

		// Token: 0x04000002 RID: 2
		private readonly byte[] arr;

		// Token: 0x04000003 RID: 3
		private readonly int offset;
	}
}
