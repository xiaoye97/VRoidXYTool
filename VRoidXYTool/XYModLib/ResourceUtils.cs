using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace XYModLib
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

		/// <summary>
		/// 从程序集加载资源
		/// </summary>
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

		/// <summary>
		/// 转换图片
		/// </summary>
		public static Texture2D LoadTexture(byte[] texData)
		{
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			MethodInfo method = typeof(Texture2D).GetMethod("LoadImage", new Type[]
			{
				typeof(byte[])
			});
			if (method != null)
			{
				method.Invoke(texture2D, new object[]
				{
					texData
				});
			}
			else
			{
				Type type = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule");
				if (type == null)
				{
					throw new ArgumentNullException("converter");
				}
				MethodInfo method2 = type.GetMethod("LoadImage", new Type[]
				{
					typeof(Texture2D),
					typeof(byte[])
				});
				if (method2 == null)
				{
					throw new ArgumentNullException("converterMethod");
				}
				method2.Invoke(null, new object[]
				{
					texture2D,
					texData
				});
			}
			return texture2D;
		}

		private static Dictionary<string, Texture2D> texDict = new Dictionary<string, Texture2D>();

		/// <summary>
		/// 加载材质
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Texture2D GetTex(string name)
		{
			if (texDict.ContainsKey(name))
			{
				return texDict[name];
			}
			byte[] embeddedResource = GetEmbeddedResource(name, null);
			var tex = LoadTexture(embeddedResource);
			UnityEngine.Object.DontDestroyOnLoad(tex);
			texDict.Add(name, tex);
			return tex;
		}
	}
}
