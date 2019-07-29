using System;

namespace ZstdNet
{
	// Token: 0x02000005 RID: 5
	public class DecompressionOptions : IDisposable
	{
		// Token: 0x06000014 RID: 20 RVA: 0x00002380 File Offset: 0x00000580
		public DecompressionOptions(byte[] dict)
		{
			this.Dictionary = dict;
			if (dict != null)
			{
				this.Ddict = ExternMethods.ZSTD_createDDict(dict, (UIntPtr)((ulong)((long)dict.Length))).EnsureZstdSuccess();
				return;
			}
			GC.SuppressFinalize(this);
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000023B4 File Offset: 0x000005B4
		~DecompressionOptions()
		{
			this.Dispose(false);
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000023E4 File Offset: 0x000005E4
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000023F3 File Offset: 0x000005F3
		private void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}
			if (this.Ddict != IntPtr.Zero)
			{
				ExternMethods.ZSTD_freeDDict(this.Ddict);
			}
			this.disposed = true;
		}

		// Token: 0x0400000C RID: 12
		private bool disposed;

		// Token: 0x0400000D RID: 13
		public readonly byte[] Dictionary;

		// Token: 0x0400000E RID: 14
		internal readonly IntPtr Ddict;
	}
}
