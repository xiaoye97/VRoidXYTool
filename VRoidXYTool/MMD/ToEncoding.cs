using System;
using System.Collections.Generic;
using System.Text;

namespace VRoidXYTool.MMD
{
	public class ToEncoding
	{
		public static string ToUnicode(byte[] sjis_bytes)
		{
			List<byte> list = new List<byte>();
			for (int i = 0; i < sjis_bytes.Length; i++)
			{
				ushort num;
				if ((sjis_bytes[i] >= 129 && sjis_bytes[i] <= 159) || (sjis_bytes[i] >= 224 && sjis_bytes[i] <= 234))
				{
					num = (ushort)(sjis_bytes[i] << 8);
					num += (ushort)sjis_bytes[++i];
				}
				else
				{
					num = (ushort)sjis_bytes[i];
				}
				ushort code = SJISToUnicode.GetCode(num);
				byte item = (byte)(code >> 8);
				byte item2 = (byte)(code & 255);
				list.Add(item2);
				list.Add(item);
			}
			return Encoding.Unicode.GetString(list.ToArray());
		}
	}
}
