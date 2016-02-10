using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Just_Cause_3_Mod_Combiner
{
	public class BinaryCombiner
	{
		public static void Combine(List<string> originalFiles, List<string> files, bool notifyCollissions)
		{
			Combine(originalFiles, files, originalFiles[originalFiles.Count - 1], notifyCollissions);
		}

		public static void Combine(List<string> originalFiles, List<string> files, string outputPath, bool notifyCollissions)
		{
			var originalFileBytes = originalFiles.Select(file => File.ReadAllBytes(file)).ToList();
			var fileBytes = files.Select(file => File.ReadAllBytes(file)).ToList();

			for (var i = 0; i < originalFileBytes[0].Length; i++)
			{
				for (var j = fileBytes.Count - 1; j >= 0; j--)
				{
					var bytes = fileBytes[j];
					var equalBytesFound = false;
					foreach (var originalFile in originalFileBytes)
					{
						if (bytes[i] == originalFile[i])
						{
							equalBytesFound = true;
							break;
						}
					}
					if (!equalBytesFound)
					{
						originalFileBytes[originalFileBytes.Count - 1][i] = bytes[i];
						break;
					}
				}
			}

			File.WriteAllBytes(outputPath, originalFileBytes[originalFileBytes.Count - 1]);
		}
	}
}
