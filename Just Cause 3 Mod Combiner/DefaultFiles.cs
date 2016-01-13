using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Just_Cause_3_Mod_Combiner
{
	public static class DefaultFiles
	{
		public static string GetFile(string fileName)
		{

			var matches = Directory.GetFiles(Settings.defaultFiles, fileName, SearchOption.AllDirectories);
			if (matches.Length != 0)
			{
				return matches[0];
			}

			//Find file from jc3 folders


			Settings.SetBusyContent("Extracting files from Just Cause 3 archives...");

			var fileLists = new List<string>();
			var patchFileLists = Directory.EnumerateFiles(Path.Combine(Settings.user.JC3Folder, "patch_win64")).Where(name => Regex.IsMatch(name, "game_hash_names[0-9]+\\.txt")).ToList<string>();
			patchFileLists = patchFileLists.OrderBy(s => -int.Parse(Path.GetFileNameWithoutExtension(s).Substring(4))).ToList<string>(); //Sort descending order, to take the file from the latest patch
			fileLists.AddRange(patchFileLists);
			fileLists.AddRange(Directory.EnumerateFiles(Path.Combine(Settings.user.JC3Folder, "dlc"), "*", SearchOption.AllDirectories).Where(name => Regex.IsMatch(name, "game_hash_names[0-9]+\\.txt")));
			fileLists.AddRange(Directory.EnumerateFiles(Path.Combine(Settings.user.JC3Folder, "archives_win64")).Where(name => Regex.IsMatch(name, "game_hash_names[0-9]+\\.txt")));



			string rightFileList = null;
			foreach (string fileList in fileLists)
			{
				string[] lines = File.ReadAllLines(fileList);
				foreach (string line in lines)
				{
					if (line.Contains(fileName))
					{
						rightFileList = fileList;
						break;
					}
				}
				if (rightFileList != null)
					break;
			}

			if (rightFileList == null)
			{
				return null;
			}


			string num = Path.GetFileName(rightFileList).Substring(15, Path.GetFileName(rightFileList).Length - 15 - 4);
			string tabFile = Path.Combine(Path.GetDirectoryName(rightFileList), "game" + num + ".tab");

			//TODO: handle?
			var outputPath = Path.Combine(Settings.tempFolder, Path.GetFileNameWithoutExtension(tabFile));
			string extractedFolder = GibbedsTools.Unpack(tabFile, outputPath);

			if (extractedFolder == null)
				return null;

			foreach (string file in Directory.GetFiles(extractedFolder, "*", SearchOption.AllDirectories))
			{
				string newPath = Path.Combine(Settings.defaultFiles, file.Substring(outputPath.Length + 1));
				if (!Directory.Exists(Path.GetDirectoryName(newPath)))
					Directory.CreateDirectory(Path.GetDirectoryName(newPath));
				if (File.Exists(newPath))
					File.Delete(newPath);
				File.Move(file, newPath);
			}

			matches = Directory.GetFiles(Settings.defaultFiles, fileName, SearchOption.AllDirectories);
			if (matches.Length != 0)
			{
				return matches[0];
			}
			return null;
		}

	}
}
