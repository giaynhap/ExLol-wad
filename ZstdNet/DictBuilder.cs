using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZstdNet
{
	// Token: 0x02000007 RID: 7
	public static class DictBuilder
	{
		// Token: 0x06000023 RID: 35 RVA: 0x000026F8 File Offset: 0x000008F8
		public static byte[] TrainFromBuffer(IEnumerable<byte[]> samples, int dictCapacity = 112640)
		{
			MemoryStream ms = new MemoryStream();
			UIntPtr[] array = samples.Select(delegate(byte[] sample)
			{
				ms.Write(sample, 0, sample.Length);
				return (UIntPtr)((ulong)((long)sample.Length));
			}).ToArray<UIntPtr>();
			byte[] array2 = new byte[dictCapacity];
			int num = (int)((uint)ExternMethods.ZDICT_trainFromBuffer(array2, (UIntPtr)((ulong)((long)dictCapacity)), ms.ToArray(), array, (uint)array.Length).EnsureZdictSuccess());
			if (dictCapacity != num)
			{
				Array.Resize<byte>(ref array2, num);
			}
			return array2;
		}

		// Token: 0x04000012 RID: 18
		public const int DefaultDictCapacity = 112640;
	}
}
