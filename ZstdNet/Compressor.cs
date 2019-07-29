using System;

namespace ZstdNet
{
	// Token: 0x02000004 RID: 4
	public class Compressor : IDisposable
	{
		// Token: 0x0600000A RID: 10 RVA: 0x00002162 File Offset: 0x00000362
		public Compressor() : this(new CompressionOptions(3))
		{
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002170 File Offset: 0x00000370
		public Compressor(CompressionOptions options)
		{
			this.Options = options;
			this.cctx = ExternMethods.ZSTD_createCCtx().EnsureZstdSuccess();
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002190 File Offset: 0x00000390
		~Compressor()
		{
			this.Dispose(false);
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000021C0 File Offset: 0x000003C0
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000021CF File Offset: 0x000003CF
		private void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			ExternMethods.ZSTD_freeCCtx(this.cctx);
			this.disposed = true;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000021ED File Offset: 0x000003ED
		public byte[] Wrap(byte[] src)
		{
			return this.Wrap(new ArraySegment<byte>(src));
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000021FC File Offset: 0x000003FC
		public byte[] Wrap(ArraySegment<byte> src)
		{
			if (src.Count == 0)
			{
				return new byte[0];
			}
			int compressBound = Compressor.GetCompressBound(src.Count);
			byte[] array = new byte[compressBound];
			int num = this.Wrap(src, array, 0);
			if (compressBound == num)
			{
				return array;
			}
			byte[] array2 = new byte[num];
			Array.Copy(array, array2, num);
			return array2;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x0000224B File Offset: 0x0000044B
		public static int GetCompressBound(int size)
		{
			return (int)((uint)ExternMethods.ZSTD_compressBound((UIntPtr)((ulong)((long)size))));
		}

		// Token: 0x06000012 RID: 18 RVA: 0x0000225E File Offset: 0x0000045E
		public int Wrap(byte[] src, byte[] dst, int offset)
		{
			return this.Wrap(new ArraySegment<byte>(src), dst, offset);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002270 File Offset: 0x00000470
		public int Wrap(ArraySegment<byte> src, byte[] dst, int offset)
		{
			if (offset < 0 || offset >= dst.Length)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (src.Count == 0)
			{
				return 0;
			}
			int num = dst.Length - offset;
			UIntPtr uintPtr;
			using (ArraySegmentPtr arraySegmentPtr = new ArraySegmentPtr(src))
			{
				using (ArraySegmentPtr arraySegmentPtr2 = new ArraySegmentPtr(new ArraySegment<byte>(dst, offset, num)))
				{
					if (this.Options.Cdict == IntPtr.Zero)
					{
						uintPtr = ExternMethods.ZSTD_compressCCtx(this.cctx, arraySegmentPtr2, (UIntPtr)((ulong)((long)num)), arraySegmentPtr, (UIntPtr)((ulong)((long)src.Count)), this.Options.CompressionLevel);
					}
					else
					{
						uintPtr = ExternMethods.ZSTD_compress_usingCDict(this.cctx, arraySegmentPtr2, (UIntPtr)((ulong)((long)num)), arraySegmentPtr, (UIntPtr)((ulong)((long)src.Count)), this.Options.Cdict);
					}
				}
			}
			uintPtr.EnsureZstdSuccess();
			return (int)((uint)uintPtr);
		}

		// Token: 0x04000009 RID: 9
		private bool disposed;

		// Token: 0x0400000A RID: 10
		public readonly CompressionOptions Options;

		// Token: 0x0400000B RID: 11
		private readonly IntPtr cctx;
	}
}
