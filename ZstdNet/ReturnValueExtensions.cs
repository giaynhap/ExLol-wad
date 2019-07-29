using System;
using System.Runtime.InteropServices;

namespace ZstdNet
{
	// Token: 0x02000009 RID: 9
	internal static class ReturnValueExtensions
	{
		// Token: 0x0600003B RID: 59 RVA: 0x0000283E File Offset: 0x00000A3E
		public static UIntPtr EnsureZdictSuccess(this UIntPtr returnValue)
		{
			if (ExternMethods.ZDICT_isError(returnValue) != 0u)
			{
				ReturnValueExtensions.ThrowException(returnValue, Marshal.PtrToStringAnsi(ExternMethods.ZDICT_getErrorName(returnValue)));
			}
			return returnValue;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x0000285A File Offset: 0x00000A5A
		public static UIntPtr EnsureZstdSuccess(this UIntPtr returnValue)
		{
			if (ExternMethods.ZSTD_isError(returnValue) != 0u)
			{
				ReturnValueExtensions.ThrowException(returnValue, Marshal.PtrToStringAnsi(ExternMethods.ZSTD_getErrorName(returnValue)));
			}
			return returnValue;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002876 File Offset: 0x00000A76
		private static void ThrowException(UIntPtr returnValue, string message)
		{
			if (0u - (uint)((ulong)returnValue) == 70u)
			{
				throw new InsufficientMemoryException(message);
			}
			throw new ZstdException(message);
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00002892 File Offset: 0x00000A92
		public static IntPtr EnsureZstdSuccess(this IntPtr returnValue)
		{
			if (returnValue == IntPtr.Zero)
			{
				throw new ZstdException("Failed to create a structure");
			}
			return returnValue;
		}

		// Token: 0x04000014 RID: 20
		private const int ZSTD_error_dstSize_tooSmall = 70;
	}
}
