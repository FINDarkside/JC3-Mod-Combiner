using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Just_Cause_3_Mod_Combiner
{
	public class Settings
	{
		public static int revision = 3;
		public static string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		public static string files = Path.Combine(currentPath, "Files");
		public static string tempFolder = Path.Combine(files, "temp");
		public static string defaultFiles = Path.Combine(files, @"Default files");
		public static string gibbedsTools = Path.Combine(files, @"Gibbedstools");

		public static MainWindow mainWindow;

		public static UserSettings user;
		public static LocalSettings local;

		static Settings()
		{
			Directory.CreateDirectory(tempFolder);
			Directory.CreateDirectory(defaultFiles);

			var settingsPath = Path.Combine(files, "settings.json");
			if (File.Exists(settingsPath))
			{
				try
				{
					user = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(settingsPath));
				}
				catch (Exception ex)
				{
					Errors.Handle(ex);
					user = new UserSettings();
				}
			}
			else
			{
				user = new UserSettings();
			}

			var localPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "JC3 Mod Combiner");
			Directory.CreateDirectory(localPath);
			var localDataPath = Path.Combine(localPath, "data.json");
			if (File.Exists(localDataPath))
			{
				local = JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText(localDataPath));
			}
			else
			{
				local = new LocalSettings();
			}


		}


		public static void SetBusyContent(string text)
		{
			Settings.mainWindow.Dispatcher.BeginInvoke((Action)delegate
			{
				Settings.mainWindow.busyIndicator.BusyContent = text;
			});
		}

	}

	public class UserSettings
	{
		public string JC3Folder;
		public bool checkForUpdates = true;
		public List<string> smallArchiveExtensions = new List<string>();
		public List<string> propertyExtensions = new List<string>();
		public List<string> adfExtensions = new List<string>();
		public List<string> unknownExtensions = new List<string>();

		public void Save()
		{
			var json = JsonConvert.SerializeObject(this);
			File.WriteAllText(Path.Combine(Settings.files, "settings.json"), json);
		}
	}

	public class LocalSettings
	{
		public string lastInstallPath = Settings.currentPath;
		public int lastRevision = Settings.revision;

		public void Save()
		{
			var json = JsonConvert.SerializeObject(this);
			var localPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), "JC3 Mod Combiner");
			var localDataPath = Path.Combine(localPath, "data.json");
			File.WriteAllText(localDataPath, json);
		}
	}
}
