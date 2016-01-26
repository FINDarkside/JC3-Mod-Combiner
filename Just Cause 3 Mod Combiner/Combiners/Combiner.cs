using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace Just_Cause_3_Mod_Combiner
{
	public class Combiner
	{

		public static IList<string> rootFiles;
		public static bool notifyCollissions;

		public static void Combine(IList<string> files, bool notifyCollissions)
		{
			Settings.SetBusyContent("Combining...");
			rootFiles = files;
			Combiner.notifyCollissions = notifyCollissions;

			var fileFormat = FileFormats.GetFileFormat(files[0]);
			if (fileFormat == FileFormat.Unknown)
			{
				throw new ArgumentException("Can't combine " + Path.GetExtension(files[0]) + " files. If you need to combine these files, let me know at jc3mods.com");
			}

			string originalFile = DefaultFiles.GetFile(Path.GetFileName(files[0]));
			if (originalFile == null)
			{
				throw new Exception("Couldn't find default file for " + Path.GetFileName(files[0]));
			}
			var outputPath = Path.Combine(Settings.user.JC3Folder, "dropzone", originalFile.Substring(Settings.defaultFiles.Length + 1));
			Combine(originalFile, fileFormat, files, outputPath);

		}

		private static void Combine(string originalFile, FileFormat fileFormat, IList<string> files, string outputPath)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
			Settings.SetBusyContent("Combining " + Path.GetFileName(originalFile));
			if (fileFormat == FileFormat.Property)
			{

				var originalXml = GibbedsTools.ConvertProperty(originalFile, TempFolder.GetTempFile(), GibbedsTools.ConvertMode.Export);

				var xmlFiles = new List<string>();
				foreach (string file in files)
				{
					xmlFiles.Add(GibbedsTools.ConvertProperty(file, TempFolder.GetTempFile(), GibbedsTools.ConvertMode.Export));
				}

				var combiner = new XmlCombiner(originalXml, xmlFiles, rootFiles, notifyCollissions);
				combiner.Combine(originalXml);
				GibbedsTools.ConvertProperty(originalXml, outputPath, GibbedsTools.ConvertMode.Import);
			}
			else if (fileFormat == FileFormat.Adf)
			{
				OverrideCombine(originalFile, files, outputPath, true);
			}
			else if (fileFormat == FileFormat.Xml)
			{
				var fileNames = rootFiles;
				if (files != rootFiles)
				{
					fileNames = rootFiles.Select(item => Path.Combine(item, Path.GetFileName(originalFile))).ToList<string>();
				}
				var combiner = new XmlCombiner(originalFile, files, fileNames, notifyCollissions);
				combiner.Combine(outputPath);
			}
			else if (fileFormat == FileFormat.Unknown)
			{
				OverrideCombine(originalFile, files, outputPath, false);
			}
			else if (fileFormat == FileFormat.SmallArchive)
			{
				Settings.SetBusyContent("Unpacking " + Path.GetFileName(originalFile));
				int combineCount = 0;

				var originalUnpacked = GibbedsTools.SmallUnpack(originalFile, TempFolder.GetTempFile());
				combineCount++;

				var unpackedFiles = new List<string>();
				foreach (string file in files)
				{
					unpackedFiles.Add(GibbedsTools.SmallUnpack(file, TempFolder.GetTempFile()));
					combineCount++;
				}
				foreach (string file in Directory.GetFiles(originalUnpacked, "*", SearchOption.AllDirectories))
				{
					Settings.SetBusyContent("Combining " + Path.GetFileName(file));
					var correspondingFiles = new List<string>();
					foreach (string unpackedFile in unpackedFiles)
					{
						string path = Path.Combine(unpackedFile, file.Substring(originalUnpacked.Length + 1));
						correspondingFiles.Add(path);
					}

					var fileFormat2 = FileFormats.GetFileFormat(file);
					Combine(file, fileFormat2, correspondingFiles, file);
				}
				Settings.SetBusyContent("Packing " + Path.GetFileName(originalFile));
				GibbedsTools.SmallPack(originalUnpacked, outputPath);
			}

		}

		private static void OverrideCombine(string originalFile, IList<string> files, string outputPath, bool binaryCombine)
		{
			var items = new List<SelectionItem>();
			items.Add(new SelectionItem() { Name = "Original", Value = originalFile });

			bool allSameSize = true;
			var originalBytes = File.ReadAllBytes(originalFile);
			byte[] replacingBytes = null;
			for (var i = 0; i < files.Count; i++)
			{
				var file = files[i];
				var bytes = File.ReadAllBytes(file);
				if (!Enumerable.SequenceEqual(bytes, originalBytes))
				{
					replacingBytes = bytes;
					var alreadyFound = false;
					for (var j = 1; j < items.Count; j++)
					{
						if (Enumerable.SequenceEqual(File.ReadAllBytes((string)items[j].Value), bytes))
						{
							items[j].Name += "\n" + Path.Combine(rootFiles[i], Path.GetFileName(originalFile));
						}
					}
					if (!alreadyFound)
						items.Add(new SelectionItem() { Name = Path.Combine(rootFiles[i], Path.GetFileName(originalFile)), Value = file });
				}
				if (bytes.Length != originalBytes.Length)
					allSameSize = false;
			}
			if (items.Count == 1)
				return;
			if (items.Count == 2)
			{
				File.WriteAllBytes(outputPath, replacingBytes);
				return;
			}

			if (binaryCombine && allSameSize)
			{
				BinaryCombiner.Combine(originalFile, files, outputPath, notifyCollissions);
			}
			else
			{
				object result = null;
				if (notifyCollissions && SelectionDialog.Show("Select overriding file",items, out result, out notifyCollissions))
				{
					replacingBytes = File.ReadAllBytes((string)result);
				}
				File.WriteAllBytes(outputPath, replacingBytes);
			}
		}

	}
}
