using System;

namespace ZstdNet
{
	// Token: 0x02000006 RID: 6
	public class Decompressor : IDisposable
	{
		// Token: 0x06000018 RID: 24 RVA: 0x00002423 File Offset: 0x00000623
		public Decompressor() : this(new DecompressionOptions(null))
		{
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002431 File Offset: 0x00000631
		public Decompressor(DecompressionOptions options)
		{
			this.Options = options;
			this.dctx = ExternMethods.ZSTD_createDCtx().EnsureZstdSuccess();
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002450 File Offset: 0x00000650
		~Decompressor()
		{
			this.Dispose(false);
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002480 File Offset: 0x00000680
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000248F File Offset: 0x0000068F
		private void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			ExternMethods.ZSTD_freeDCtx(this.dctx);
			this.disposed = true;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000024AD File Offset: 0x000006AD
		public byte[] Unwrap(byte[] src, int maxDecompressedSize = 2147483647)
		{
			return this.Unwrap(new ArraySegment<byte>(src), maxDecompressedSize);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000024BC File Offset: 0x000006BC
		public byte[] Unwrap(ArraySegment<byte> src, int maxDecompressedSize = 2147483647)
		{
			if (src.Count == 0)
			{
				return new byte[0];
			}
			ulong decompressedSize = Decompressor.GetDecompressedSize(src);
			if (decompressedSize == 0UL)
			{
				throw new ZstdException("Can't create buffer for data with unspecified decompressed size (provide your own buffer to Unwrap instead)");
			}
			if (decompressedSize > (ulong)((long)maxDecompressedSize))
			{
				throw new ArgumentOutOfRangeException(string.Format("Decompressed size is too big ({0} bytes > authorized {1} bytes)", decompressedSize, maxDecompressedSize));
			}
			byte[] array = new byte[decompressedSize];
			int num;
			try
			{
				num = this.Unwrap(src, array, 0, false);
			}
			catch (InsufficientMemoryException)
			{
				throw new ZstdException("Invalid decompressed size");
			}
			if ((int)decompressedSize != num)
			{
				throw new ZstdException("Invalid decompressed size specified in the data");
			}
			return array;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002554 File Offset: 0x00000754
		public static ulong GetDecompressedSize(byte[] src)
		{
			return Decompressor.GetDecompressedSize(new ArraySegment<byte>(src));
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002564 File Offset: 0x00000764
		public static ulong GetDecompressedSize(ArraySegment<byte> src)
		{
			ulong result;
			using (ArraySegmentPtr arraySegmentPtr = new ArraySegmentPtr(src))
			{
				result = ExternMethods.ZSTD_getDecompressedSize(arraySegmentPtr, (UIntPtr)((ulong)((long)src.Count)));
			}
			return result;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000025B0 File Offset: 0x000007B0
		public int Unwrap(byte[] src, byte[] dst, int offset)
		{
			return this.Unwrap(new ArraySegment<byte>(src), dst, offset, true);
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000025C4 File Offset: 0x000007C4
		public int Unwrap(ArraySegment<byte> src, byte[] dst, int offset, bool bufferSizePrecheck = true)
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
			int result;
			using (ArraySegmentPtr arraySegmentPtr = new ArraySegmentPtr(src))
			{
				if (bufferSizePrecheck && (int)ExternMethods.ZSTD_getDecompressedSize(arraySegmentPtr, (UIntPtr)((ulong)((long)src.Count))) > num)
				{
					throw new InsufficientMemoryException("Buffer size is less than specified decompressed data size");
				}
				UIntPtr uintPtr;
				using (ArraySegmentPtr arraySegmentPtr2 = new ArraySegmentPtr(new ArraySegment<byte>(dst, offset, num)))
				{
					if (this.Options.Ddict == IntPtr.Zero)
					{
						uintPtr = ExternMethods.ZSTD_decompressDCtx(this.dctx, arraySegmentPtr2, (UIntPtr)((ulong)((long)num)), arraySegmentPtr, (UIntPtr)((ulong)((long)src.Count)));
					}
					else
					{
						uintPtr = ExternMethods.ZSTD_decompress_usingDDict(this.dctx, arraySegmentPtr2, (UIntPtr)((ulong)((long)num)), arraySegmentPtr, (UIntPtr)((ulong)((long)src.Count)), this.Options.Ddict);
					}
				}
				uintPtr.EnsureZstdSuccess();
				result = (int)((uint)uintPtr);
			}
			return result;
		}

		// Token: 0x0400000F RID: 15
		private bool disposed;

		// Token: 0x04000010 RID: 16
		public readonly DecompressionOptions Options;

		// Token: 0x04000011 RID: 17
		private readonly IntPtr dctx;
	}
}
