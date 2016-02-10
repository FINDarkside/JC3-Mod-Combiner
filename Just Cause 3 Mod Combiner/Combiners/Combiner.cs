using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace Just_Cause_3_Mod_Combiner
{
	public class Combiner
	{

		public static List<string> rootFiles;
		public static bool notifyCollissions;

		public static void Combine(List<string> files, bool notifyCollissions)
		{
			Settings.SetBusyContent("Combining...");
			rootFiles = files;
			Combiner.notifyCollissions = notifyCollissions;

			var fileFormat = FileFormats.GetFileFormat(files[0]);
			if (fileFormat == FileFormat.Unknown)
			{
				throw new ArgumentException("Can't combine " + Path.GetExtension(files[0]) + " files. If you need to combine these files, let me know at jc3mods.com");
			}

			var originalFiles = DefaultFiles.GetFile(Path.GetFileName(files[0]));
			if (originalFiles.Count == 0)
			{
				throw new Exception("Couldn't find default files for " + Path.GetFileName(files[0]));
			}
			var outputPath = Path.Combine(Settings.user.JC3Folder, "dropzone", originalFiles[0].Substring(Settings.defaultFiles.Length + 1));
			var name = Path.GetFileNameWithoutExtension(outputPath);
			outputPath = Path.GetDirectoryName(outputPath) + "\\" + name.Substring(0, name.LastIndexOf('_')) + Path.GetExtension(outputPath);
			Combine(originalFiles, files, FileFormats.GetFileFormat(originalFiles[0]), outputPath);
		}

		private static void Combine(List<string> originalFiles, List<string> files, FileFormat fileFormat, string outputPath)
		{

			Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

			if (fileFormat == FileFormat.Property)
			{
				Settings.SetBusyContent("Combining " + Path.GetFileName(originalFiles[0]));

				List<string> originalXml = originalFiles.Select(originalFile => GibbedsTools.ConvertProperty(originalFile, TempFolder.GetTempFile(), GibbedsTools.ConvertMode.Export)).ToList();
				List<string> xmlFiles = files.Select(originalFile => GibbedsTools.ConvertProperty(originalFile, TempFolder.GetTempFile(), GibbedsTools.ConvertMode.Export)).ToList();

				var xmlOutputPath = TempFolder.GetTempFile();
				Combine(originalXml, xmlFiles, FileFormat.Xml, xmlOutputPath);

				GibbedsTools.ConvertProperty(xmlOutputPath, outputPath, GibbedsTools.ConvertMode.Import);
			}
			else if (fileFormat == FileFormat.Adf)
			{
				Settings.SetBusyContent("Combining " + Path.GetFileName(originalFiles[0]));

				OverrideCombine(originalFiles, files, outputPath, true);
			}
			else if (fileFormat == FileFormat.Xml)
			{
				var fileNames = rootFiles;
				if (files != rootFiles)
				{
					fileNames = rootFiles.Select(item => Path.Combine(item, Path.GetFileName(originalFiles[0]))).ToList<string>();
				}
				XmlCombiner.Combine(originalFiles, files, fileNames, notifyCollissions, outputPath);
			}
			else if (fileFormat == FileFormat.Unknown)
			{
				Settings.SetBusyContent("Combining " + Path.GetFileName(originalFiles[0]));

				OverrideCombine(originalFiles, files, outputPath, false);
			}
			else if (fileFormat == FileFormat.SmallArchive)
			{
				Settings.SetBusyContent("Unpacking " + Path.GetFileName(originalFiles[0]));

				var originalUnpacked = originalFiles.Select(file => GibbedsTools.SmallUnpack(file, TempFolder.GetTempFile())).ToList();
				var unpackedFiles = files.Select(file => GibbedsTools.SmallUnpack(file, TempFolder.GetTempFile())).ToList();

				foreach (string file in Directory.EnumerateFiles(originalUnpacked[originalUnpacked.Count - 1], "*", SearchOption.AllDirectories))
				{
					Settings.SetBusyContent("Combining " + Path.GetFileName(file));
					var correspondingOriginals = new List<string>();
					foreach (var unpackedFile in originalUnpacked)
					{
						string path = Path.Combine(unpackedFile, file.Substring(originalUnpacked[0].Length + 1));
						correspondingOriginals.Add(path);
					}
					var correspondingFiles = new List<string>();
					foreach (string unpackedFile in unpackedFiles)
					{
						string path = Path.Combine(unpackedFile, file.Substring(originalUnpacked[0].Length + 1));
						correspondingFiles.Add(path);
					}

					Combine(correspondingOriginals, correspondingFiles, FileFormats.GetFileFormat(file), file);
				}
				Settings.SetBusyContent("Packing " + Path.GetFileName(originalFiles[0]));
				GibbedsTools.SmallPack(originalUnpacked[originalUnpacked.Count - 1], outputPath);
			}

		}

		private static void OverrideCombine(List<string> originalFiles, List<string> files, string outputPath, bool binaryCombine)
		{
			bool allSameSize = true;

			var originalHashes = originalFiles.Select(file => SHA256.ComputeHash(file)).ToList();
			var modifiedFileHashes = new Dictionary<string, List<int>>();
			var size = new FileInfo(files[0]).Length;
			for (var i = 0; i < files.Count; i++)
			{
				var file = files[i];
				var hash = SHA256.ComputeHash(file);
				if (!originalHashes.Contains(hash))
				{
					if (modifiedFileHashes.ContainsKey(hash))
						modifiedFileHashes[hash].Add(i);
					else
						modifiedFileHashes.Add(hash, new List<int>() { i });
					if (new FileInfo(file).Length != size)
						allSameSize = false;
				}
			}

			if (modifiedFileHashes.Count == 0)
				return;

			if (modifiedFileHashes.Count == 1)
			{
				if (File.Exists(outputPath))
					File.Delete(outputPath);
				File.Copy(files[modifiedFileHashes.First().Value[0]], outputPath);
				return;
			}

			if (binaryCombine && allSameSize)
			{
				BinaryCombiner.Combine(originalFiles, files, outputPath, notifyCollissions);
			}
			else
			{
				var items = new List<SelectionItem>();
				items.Add(new SelectionItem() { Name = "Original", Value = originalFiles[originalFiles.Count - 1] + "\\" + Path.GetFileName(originalFiles[0]) });
				foreach (KeyValuePair<string, List<int>> pair in modifiedFileHashes)
				{
					var item = new SelectionItem()
					{
						Name = pair.Value.Count == 1 ? rootFiles[pair.Value[0]] + "\\" + Path.GetFileName(originalFiles[0]) : "",
						Description = pair.Value.Count == 1 ? null : rootFiles[pair.Value[0]] + "\\" + Path.GetFileName(originalFiles[0]),
						Value = files[pair.Value[0]]
					};
					items.Add(item);
				}

				string replacingFile = (string)items[items.Count - 1].Value;
				object result = null;
				if (notifyCollissions && SelectionDialog.Show("Select overriding file", items, out result, out notifyCollissions))
				{
					replacingFile = (string)result;
				}
				if (outputPath == replacingFile)
					return;
				if (File.Exists(outputPath))
					File.Delete(outputPath);
				File.Copy(replacingFile, outputPath);
			}
		}

	}
}
