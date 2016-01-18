using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Just_Cause_3_Mod_Combiner
{
	public static class TempFolder
	{
		public static string path = Path.Combine(Settings.files, "temp");
		private static char[] chars = "qwertyuiopasdfghjklzxcvbnm1234567890-_".ToCharArray();
		private static string[] illegal = new string[] { "con", "prn", "aux", "nul", "com1", "com2", "com3", "com4", "com5", "com6", "com7", "com8", "com9", "lpt1", "lpt2", "lpt3", "lpt4", "lpt5", "lpt6", "lpt7", "lpt8", "lpt9" };
		private static int[] charIndexes = new int[1];


		public static string GetTempFile()
		{
			Directory.CreateDirectory(path);
			string result = null;
			do
			{
				StringBuilder sb = new StringBuilder();
				foreach (int i in charIndexes)
				{
					sb.Append(chars[i]);
				}
				result = Path.Combine(path, sb.ToString());
				Next();
			} while (Array.IndexOf(illegal, result) != -1 || File.Exists(result));
			return result;
		}

		private static void Next()
		{
			charIndexes[charIndexes.Length - 1]++;
			for (var j = charIndexes.Length - 1; j >= 0; j--)
			{
				if (charIndexes[j] >= chars.Length)
				{
					if (j == 0)
					{
						charIndexes = new int[charIndexes.Length + 1];
						return;
					}
					charIndexes[j] = 0;
					charIndexes[j - 1]++;
				}
			}
		}

		public static async Task ClearAsync()
		{
			await Task.Run(() =>
			   {
				   var di = new DirectoryInfo(path);
				   foreach (FileInfo file in di.GetFiles())
					   file.Delete();
				   foreach (DirectoryInfo dir in di.GetDirectories())
					   dir.Delete(true);

				   charIndexes = new int[1];
			   });
		}
	}
}
