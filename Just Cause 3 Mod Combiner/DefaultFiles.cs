using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Just_Cause_3_Mod_Combiner
{
	public static class DefaultFiles
	{
		public class DefaultFileInformation
		{
			public string relativePath;
			public string hash;
			public int size;
			public string tabFile;

			public DefaultFileInformation(string line, string tabFile)
			{
				string[] splitLine = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				size = Convert.ToInt32(splitLine[2], 16);
				hash = splitLine[0].Substring(2);
				relativePath = splitLine[4];
				this.tabFile = tabFile;
			}
		}

		public static string GetFile(string fileName)
		{
			Settings.SetBusyContent("Finding default files for " + fileName + "...");

			var result = new List<string>();
			var cachedFiles = Directory.EnumerateFiles(Settings.defaultFiles, Path.GetFileNameWithoutExtension(fileName) + "_*" + Path.GetExtension(fileName), SearchOption.AllDirectories);
			foreach (var file in cachedFiles)
			{
				result.Add(file);
			}
			//Find file from jc3 folders

			var fileLists = new List<string>();
			fileLists.AddRange(Directory.EnumerateFiles(Path.Combine(Settings.user.JC3Folder, "archives_win64"), "*" , SearchOption.AllDirectories).Where(name => Regex.IsMatch(name, "game_hash_names[0-9]+\\.txt")));
			fileLists = fileLists.OrderBy(s => int.Parse(Path.GetFileNameWithoutExtension(s).Substring(15))).ToList<string>();
			if (Directory.Exists(Path.Combine(Settings.user.JC3Folder, "dlc")))
			{
				var dlcFileLists = Directory.EnumerateFiles(Path.Combine(Settings.user.JC3Folder, "dlc"), "*", SearchOption.AllDirectories).Where(name => Regex.IsMatch(name, "game_hash_names[0-9]+\\.txt")).ToList<string>();
				dlcFileLists = dlcFileLists.OrderBy(s => int.Parse(Path.GetFileNameWithoutExtension(s).Substring(15))).ToList<string>();
				fileLists.AddRange(dlcFileLists);
			}
			if (Directory.Exists(Path.Combine(Settings.user.JC3Folder, "patch_win64")))
			{
				var patchFileLists = Directory.EnumerateFiles(Path.Combine(Settings.user.JC3Folder, "patch_win64")).Where(name => Regex.IsMatch(name, "game_hash_names[0-9]+\\.txt")).ToList<string>();
				patchFileLists = patchFileLists.OrderBy(s => int.Parse(Path.GetFileNameWithoutExtension(s).Substring(15))).ToList<string>();
				fileLists.AddRange(patchFileLists);
			}


			var fileInfos = new List<DefaultFileInformation>();
			foreach (string fileList in fileLists)
			{
				string[] lines = File.ReadAllLines(fileList);
				foreach (string line in lines)
				{
					if (line.Contains(fileName))
					{

						string num = Path.GetFileName(fileList).Substring(15, Path.GetFileName(fileList).Length - 15 - 4);
						string tabFile = Path.Combine(Path.GetDirectoryName(fileList), "game" + num + ".tab");
						fileInfos.Add(new DefaultFileInformation(line, tabFile));
					}
				}
			}

			if (result.Count == fileInfos.Count)
			{
				return result[0]; //return result;
			}

			foreach (string file in result)
			{
				File.Delete(file);
			}

			Settings.SetBusyContent("Extracting default files from Just Cause 3 archives...");

			foreach (var fileInfo in fileInfos)
			{
				var outputPath = TempFolder.GetTempFile();
				string extractedFolder = GibbedsTools.Unpack(fileInfo.tabFile, outputPath, fileInfo.hash + "\\.*");

				if (extractedFolder == null)
					continue;

				var files = Directory.EnumerateFiles(extractedFolder, fileInfo.hash + ".*", SearchOption.AllDirectories);
				foreach (string file in files)
				{
					if (new FileInfo(file).Length != fileInfo.size)
						continue;
					string newPath = Path.Combine(Settings.defaultFiles, fileInfo.relativePath);
					newPath = Util.GetUniqueFile(newPath);

					if (!Directory.Exists(Path.GetDirectoryName(newPath)))
						Directory.CreateDirectory(Path.GetDirectoryName(newPath));
					File.Move(file, newPath);
					if (File.Exists(newPath))
						result.Add(newPath);
					break;
				}
			}
			return result[0];
		}
	}
}
