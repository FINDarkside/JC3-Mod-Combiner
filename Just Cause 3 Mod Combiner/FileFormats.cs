using System;
using System.Collections.Generic;
using System.IO;

namespace Just_Cause_3_Mod_Combiner
{
	public enum FileFormat
	{
		Property,
		SmallArchive,
		Adf,
		Xml,
		Unknown
	}

	public static class FileFormats
	{

		private static List<string> smallArchiveExtensions = new List<string> { "ee", "nl", "fl", "bl" };
		private static List<string> propertyExtensions = new List<string> { "blo", "bin", "epe", "asb", "afsmb", "brdd" };
		private static List<string> adfExtensions = new List<string> { "xvmc", "vocalsc", "onlinec", "stringlookup", "gdc", "etunec", "ftunec", "wtunec", "environc", "watertunec", "trim", "effc", "roadgraphc", "statisticsc", "revolutionc", "occludersc", "light_infoc", "worldaudioinfo_xmlc", "shudc", "collectionc", "routec", "vocals_settingsc", "aisystunec", "gadfc", "mtunec", "gvc", "bak", "world" };
		private static List<string> unknownExtensions = new List<string> { "dds", "ddsc", "rbm", "binc", "wavc", "ttfc", "gfx", "hmddsc", "streampatch", "navmeshc", "ban", "fmod_guids", "lod", "pfxc"};

		static FileFormats()
		{
			Settings.user.smallArchiveExtensions.RemoveAll(item => smallArchiveExtensions.Contains(item));
			Settings.user.propertyExtensions.RemoveAll(item => propertyExtensions.Contains(item));
			Settings.user.adfExtensions.RemoveAll(item => adfExtensions.Contains(item));
			Settings.user.unknownExtensions.RemoveAll(item => unknownExtensions.Contains(item));

			smallArchiveExtensions.AddRange(Settings.user.smallArchiveExtensions);
			propertyExtensions.AddRange(Settings.user.propertyExtensions);
			adfExtensions.AddRange(Settings.user.adfExtensions);
			unknownExtensions.AddRange(Settings.user.unknownExtensions);
		}

		public static FileFormat GetFileFormat(string file)
		{
			string extension = Path.GetExtension(file).Substring(1);

			if (extension == "xml")
				return FileFormat.Xml;
			else if (smallArchiveExtensions.Contains(extension))
				return FileFormat.SmallArchive;
			else if (propertyExtensions.Contains(extension))
				return FileFormat.Property;
			else if (adfExtensions.Contains(extension))
				return FileFormat.Adf;
			else if (unknownExtensions.Contains(extension))
				return FileFormat.Unknown;

			System.Diagnostics.Debug.WriteLine("Defining file format of ." + extension);

			if (GibbedsTools.CanConvert(file, GibbedsTools.convertAdf))
			{
				adfExtensions.Add(extension);
				Settings.user.adfExtensions.Add(extension);
				return FileFormat.Adf;
			}
			else if (GibbedsTools.CanConvert(file, GibbedsTools.convertProperty))
			{
				propertyExtensions.Add(extension);
				Settings.user.propertyExtensions.Add(extension);
				return FileFormat.Property;
			}
			else if (GibbedsTools.CanConvert(file, GibbedsTools.smallUnpack))
			{
				smallArchiveExtensions.Add(extension);
				Settings.user.smallArchiveExtensions.Add(extension);
				return FileFormat.SmallArchive;
			}

			unknownExtensions.Add(extension);
			Settings.user.unknownExtensions.Add(extension);

			System.Diagnostics.Debug.WriteLine(String.Join("\", \"", unknownExtensions));

			return FileFormat.Unknown;
		}
	}
}
