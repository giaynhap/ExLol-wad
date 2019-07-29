using System;

namespace ZstdNet
{
	// Token: 0x02000003 RID: 3
	public class CompressionOptions : IDisposable
	{
		// Token: 0x06000004 RID: 4 RVA: 0x000020A5 File Offset: 0x000002A5
		public CompressionOptions(byte[] dict, int compressionLevel = 3) : this(compressionLevel)
		{
			this.Dictionary = dict;
			if (dict != null)
			{
				this.Cdict = ExternMethods.ZSTD_createCDict(dict, (UIntPtr)((ulong)((long)dict.Length)), compressionLevel).EnsureZstdSuccess();
				return;
			}
			GC.SuppressFinalize(this);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000020DA File Offset: 0x000002DA
		public CompressionOptions(int compressionLevel)
		{
			this.CompressionLevel = compressionLevel;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000020EC File Offset: 0x000002EC
		~CompressionOptions()
		{
			this.Dispose(false);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000211C File Offset: 0x0000031C
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000212B File Offset: 0x0000032B
		private void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			if (this.Cdict != IntPtr.Zero)
			{
				ExternMethods.ZSTD_freeCDict(this.Cdict);
			}
			this.disposed = true;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000009 RID: 9 RVA: 0x0000215B File Offset: 0x0000035B
		public static int MaxCompressionLevel
		{
			get
			{
				return ExternMethods.ZSTD_maxCLevel();
			}
		}

		// Token: 0x04000004 RID: 4
		private bool disposed;

		// Token: 0x04000005 RID: 5
		public const int DefaultCompressionLevel = 3;

		// Token: 0x04000006 RID: 6
		public readonly int CompressionLevel;

		// Token: 0x04000007 RID: 7
		public readonly byte[] Dictionary;

		// Token: 0x04000008 RID: 8
		internal readonly IntPtr Cdict;
	}
}
