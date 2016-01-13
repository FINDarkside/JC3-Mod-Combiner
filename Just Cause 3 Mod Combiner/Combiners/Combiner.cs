using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

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
				throw new Exception("Couldn't find default file");
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
				int combineCount = 0;

				var originalXml = GibbedsTools.ConvertProperty(originalFile, Path.Combine(Settings.tempFolder, Path.GetFileNameWithoutExtension(originalFile) + "_" + combineCount + ".xml"));
				combineCount++;

				var xmlFiles = new List<string>();
				foreach (string file in files)
				{
					xmlFiles.Add(GibbedsTools.ConvertProperty(file, Path.Combine(Settings.tempFolder, Path.GetFileNameWithoutExtension(file) + "_" + combineCount + ".xml")));
					combineCount++;
				}

				var combiner = new XmlCombiner(originalXml, xmlFiles, rootFiles, notifyCollissions);
				combiner.Combine(originalXml);
				GibbedsTools.ConvertProperty(originalXml, outputPath);
			}
			else if (fileFormat == FileFormat.Adf)
			{
				//TODO: notify collissions
				var originalBytes = File.ReadAllBytes(originalFile);
				var replacingBytes = originalBytes;
				bool useReplace = false;
				foreach (string file in files)
				{
					var bytes = File.ReadAllBytes(file);
					if (!Enumerable.SequenceEqual(bytes, originalBytes))
						replacingBytes = bytes;
					if (bytes.Length != originalBytes.Length)
						useReplace = true;
				}
				if (useReplace)
				{
					File.WriteAllBytes(outputPath, replacingBytes);
				}
				else
				{
					BinaryCombiner.Combine(originalFile, files, outputPath, notifyCollissions);
				}

			}
			else if (fileFormat == FileFormat.Xml)
			{
				var combiner = new XmlCombiner(originalFile, files, rootFiles, notifyCollissions);
				combiner.Combine(originalFile);
			}
			else if (fileFormat == FileFormat.Unknown)
			{
				var originalBytes = File.ReadAllBytes(originalFile);
				var replacingBytes = originalBytes;
				foreach (string file in files)
				{
					var bytes = File.ReadAllBytes(file);
					if (!Enumerable.SequenceEqual(bytes, originalBytes))
						replacingBytes = bytes;
				}
				File.WriteAllBytes(originalFile, replacingBytes);
			}
			else if (fileFormat == FileFormat.SmallArchive)
			{
				Settings.SetBusyContent("Unpacking " + Path.GetFileName(originalFile));
				int combineCount = 0;

				var originalUnpacked = GibbedsTools.SmallUnpack(originalFile, Path.Combine(Settings.tempFolder, Path.GetFileNameWithoutExtension(originalFile) + "_" + combineCount));
				combineCount++;

				var unpackedFiles = new List<string>();
				foreach (string file in files)
				{
					unpackedFiles.Add(GibbedsTools.SmallUnpack(file, Path.Combine(Settings.tempFolder, Path.GetFileNameWithoutExtension(file) + "_" + combineCount)));
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


	}
}
