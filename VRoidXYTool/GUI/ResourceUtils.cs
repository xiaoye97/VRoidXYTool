using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VRoidXYTool
{
	public static class ResourceUtils
	{
		public static byte[] ReadAllBytes(this Stream input)
		{
			byte[] array = new byte[16384];
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				int count;
				while ((count = input.Read(array, 0, array.Length)) > 0)
				{
					memoryStream.Write(array, 0, count);
				}
				result = memoryStream.ToArray();
			}
			return result;
		}

		public static byte[] GetEmbeddedResource(string resourceFileName, Assembly containingAssembly = null)
		{
			if (containingAssembly == null)
			{
				containingAssembly = Assembly.GetCallingAssembly();
			}
			string name = containingAssembly.GetManifestResourceNames().Single((string str) => str.EndsWith(resourceFileName));
			byte[] result;
			using (Stream manifestResourceStream = containingAssembly.GetManifestResourceStream(name))
			{
				Stream stream = manifestResourceStream;
				if (stream == null)
				{
					throw new InvalidOperationException("The resource " + resourceFileName + " was not found");
				}
				result = stream.ReadAllBytes();
			}
			return result;
		}
	}
}
