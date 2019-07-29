using ExportWadLol.ZstdNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ZstdNet
{
	// Token: 0x02000008 RID: 8
	internal static class ExternMethods
	{
		// Token: 0x06000024 RID: 36 RVA: 0x00002769 File Offset: 0x00000969
		static ExternMethods()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				ExternMethods.SetWinDllDirectory();
			}
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002780 File Offset: 0x00000980
		private static void SetWinDllDirectory()
		{
			string location = Assembly.GetExecutingAssembly().Location;
			string text;
			if (string.IsNullOrEmpty(location) || (text = Path.GetDirectoryName(location)) == null)
			{
				Trace.TraceWarning(string.Format("{0}: Failed to get executing assembly location", "ZstdNet"));
				return;
			}
			if (Path.GetFileName(text).StartsWith("net", StringComparison.Ordinal) && Path.GetFileName(Path.GetDirectoryName(text)) == "lib" && File.Exists(Path.Combine(text, "../../zstdnet.nuspec")))
			{
				text = Path.Combine(text, "../../build");
			}
            /*string path = Environment.Is64BitProcess ? "x64" : "x86";
			if (!ExternMethods.SetDllDirectory(Path.Combine(text, path)))
			{
				Trace.TraceWarning(string.Format("{0}: Failed to set DLL directory to '{1}'", "ZstdNet", text));
			}*/
            var dllLoader = new DynamicDllLoader();
           

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ExportWadLol.libzstd.dll";

            
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (!File.Exists(text + "/libzstd.dll"))
                {
                    var fileStream = new FileStream(text + "/libzstd.dll", FileMode.Create, FileAccess.Write);
                    stream.CopyTo(fileStream);
                    fileStream.Dispose();
                }
            }

            if (!ExternMethods.SetDllDirectory(text))
            {
                Trace.TraceWarning(string.Format("{0}: Failed to set DLL directory to '{1}'", "ZstdNet", text));
            }
            /*
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    if (!dllLoader.LoadLibrary(memoryStream.ToArray()))
                    {
                        Trace.TraceWarning(string.Format("{0}: Failed to set DLL directory to '{1}'", "ZstdNet", text));
                    }
                }
                
            }*/

        }

        // Token: 0x06000026 RID: 38
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool SetDllDirectory(string path);

		// Token: 0x06000027 RID: 39
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZDICT_trainFromBuffer(byte[] dictBuffer, UIntPtr dictBufferCapacity, byte[] samplesBuffer, UIntPtr[] samplesSizes, uint nbSamples);

		// Token: 0x06000028 RID: 40
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ZDICT_isError(UIntPtr code);

		// Token: 0x06000029 RID: 41
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ZDICT_getErrorName(UIntPtr code);

		// Token: 0x0600002A RID: 42
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ZSTD_createCCtx();

		// Token: 0x0600002B RID: 43
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZSTD_freeCCtx(IntPtr cctx);

		// Token: 0x0600002C RID: 44
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ZSTD_createDCtx();

		// Token: 0x0600002D RID: 45
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZSTD_freeDCtx(IntPtr cctx);

		// Token: 0x0600002E RID: 46
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZSTD_compressCCtx(IntPtr ctx, IntPtr dst, UIntPtr dstCapacity, IntPtr src, UIntPtr srcSize, int compressionLevel);

		// Token: 0x0600002F RID: 47
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZSTD_decompressDCtx(IntPtr ctx, IntPtr dst, UIntPtr dstCapacity, IntPtr src, UIntPtr srcSize);

		// Token: 0x06000030 RID: 48
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ZSTD_createCDict(byte[] dict, UIntPtr dictSize, int compressionLevel);

		// Token: 0x06000031 RID: 49
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZSTD_freeCDict(IntPtr cdict);

		// Token: 0x06000032 RID: 50
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZSTD_compress_usingCDict(IntPtr cctx, IntPtr dst, UIntPtr dstCapacity, IntPtr src, UIntPtr srcSize, IntPtr cdict);

		// Token: 0x06000033 RID: 51
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ZSTD_createDDict(byte[] dict, UIntPtr dictSize);

		// Token: 0x06000034 RID: 52
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZSTD_freeDDict(IntPtr ddict);

		// Token: 0x06000035 RID: 53
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZSTD_decompress_usingDDict(IntPtr dctx, IntPtr dst, UIntPtr dstCapacity, IntPtr src, UIntPtr srcSize, IntPtr ddict);

		// Token: 0x06000036 RID: 54
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong ZSTD_getDecompressedSize(IntPtr src, UIntPtr srcSize);

		// Token: 0x06000037 RID: 55
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern int ZSTD_maxCLevel();

		// Token: 0x06000038 RID: 56
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern UIntPtr ZSTD_compressBound(UIntPtr srcSize);

		// Token: 0x06000039 RID: 57
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint ZSTD_isError(UIntPtr code);

		// Token: 0x0600003A RID: 58
		[DllImport("libzstd", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr ZSTD_getErrorName(UIntPtr code);

		// Token: 0x04000013 RID: 19
		private const string DllName = "libzstd";
	}
}
