using System.Collections.Generic;
using System.IO;

namespace Just_Cause_3_Mod_Combiner
{
	public class BinaryCombiner
	{
		public static void Combine(string originalFile, IList<string> files, bool notifyCollissions)
		{
			Combine(originalFile, files, originalFile, notifyCollissions);
		}

		public static void Combine(string originalFile, IList<string> files,string outputPath, bool notifyCollissions)
		{
			byte[] originalBytes = File.ReadAllBytes(originalFile); 
			var fileBytes = new List<byte[]>();
			foreach (string file in files)
			{
				var bytes = File.ReadAllBytes(file);
				fileBytes.Add(bytes);
			}

			for (var i = 0; i < originalBytes.Length; i++)
			{
				for (var j = fileBytes.Count - 1; j >= 0; j--)
				{
					var bytes = fileBytes[j];
					if (bytes[i] != originalBytes[i])
					{
						originalBytes[i] = bytes[i];
						break;
					}
				}
			}

			File.WriteAllBytes(outputPath, originalBytes);

		}
	}
}
