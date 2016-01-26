using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using Ookii.Dialogs;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;

namespace Just_Cause_3_Mod_Combiner
{
	public class Item
	{
		public string File { get; set; }
		public Item(string name)
		{
			this.File = name;
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public ObservableCollection<Item> Items { get; set; }
		public bool AdvancedCombine { get; set; }

		private Task runningTask;

		public MainWindow()
		{
			Settings.mainWindow = this;

			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += new UnhandledExceptionEventHandler((object sender, UnhandledExceptionEventArgs args) =>
			{
				File.WriteAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "crash.txt"), args.ExceptionObject.ToString());
				Application.Current.Shutdown();
			});

#if !DEBUG
			if (Settings.user.checkForUpdates && System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
			{
				try
				{
					WebClient webClient = new WebClient();
					webClient.DownloadStringCompleted += (DownloadStringCompletedEventHandler)((sender, e) =>
					{
						if (e.Error != null)
							return;
						string result = e.Result;
						string match = Regex.Match(result, @"<b>Version</b>r[0-9]+<").Value;
						int newestRevision = int.Parse(Regex.Match(match, "r[0-9]+").Value.Substring(1));
						Debug.WriteLine(newestRevision + "");
						if (newestRevision > Settings.revision && System.Windows.MessageBox.Show("Current version: r" + Settings.revision + "\nNewest version: r" + newestRevision + "\nOpen justcause3mods.com mod page?", "New version available", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							Process.Start("http://justcause3mods.com/mods/mod-combiner/");
					});
					webClient.DownloadStringTaskAsync("http://justcause3mods.com/mods/mod-combiner/");
				}
				catch (Exception e)
				{
					Errors.Handle("Failed to check for new version", e);
				}
			}
#endif

			var oldSettings = Path.Combine(Settings.local.lastInstallPath, @"Files\settings.json");
			if (File.Exists(oldSettings))
			{
				Settings.user = JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(oldSettings));
			}
			InitializeComponent();
			RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Linear);
			Title += " r" + Settings.revision;

			if (Settings.user.JC3Folder == null || !Directory.Exists(Settings.user.JC3Folder))
			{
				try
				{
					RegistryKey regKey = Registry.CurrentUser;
					regKey = regKey.OpenSubKey(@"Software\Valve\Steam");

					if (regKey != null)
					{
						var installpath = regKey.GetValue("SteamPath").ToString() + @"/SteamApps/common/Just Cause 3";
						if (Directory.Exists(installpath))
							if (MessageBox.Show("Just Cause 3 folder found at\n" + installpath, "Set as JC3 path?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								Settings.user.JC3Folder = installpath;
							}
					}
				}
				catch (Exception ex)
				{
					Errors.Handle(ex);
				}

				while (Settings.user.JC3Folder == null || !Directory.Exists(Settings.user.JC3Folder))
				{
					var dialog = new VistaFolderBrowserDialog();
					dialog.Description = "Select Just Cause 3 folder";
					dialog.UseDescriptionForTitle = true;
					var result = dialog.ShowDialog();
					if (result == System.Windows.Forms.DialogResult.OK)
					{
						if (Directory.Exists(dialog.SelectedPath))
							Settings.user.JC3Folder = dialog.SelectedPath;
					}
					else if (result == System.Windows.Forms.DialogResult.Cancel)
					{
						Application.Current.Shutdown();
					}
				}
			}


			Directory.CreateDirectory(Path.Combine(Settings.user.JC3Folder, "dropzone"));


			Items = new ObservableCollection<Item>();
			fileList.Items = Items;

		}

		private async void AddFile(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.CheckFileExists = true;
			dialog.Multiselect = true;
			if (dialog.ShowDialog() == true)
			{
				var files = dialog.FileNames;
				foreach (var file in files)
				{
					if (Directory.Exists(file))
					{
						ErrorDialog.Show("Can't combine directories");
						continue;
					}
					if (await FileFormats.IsKnownFormat(file))
						fileList.AddFileToList(file);
					else
						ErrorDialog.Show("Can't combine " + Path.GetExtension(file) + " files. If you need to combine these let me know at jc3mods.com");
				}
			}
		}

		private void DeleteSelectedItems(object sender, RoutedEventArgs e)
		{
			fileList.RemoveSelectedItems();
		}

		private async void CombineClicked(object sender, RoutedEventArgs e)
		{

			var result2 = new XmlDocument();

			var originalDocs = new List<XmlDocument>();
			var docs = new List<XmlDocument>();
			var breadcrumbs = new List<string>() { "TESTSHIT", "Test2" };

			var doc = new XmlDocument();
			doc.LoadXml(@"<note>
							<value id=""A"">Tove</value>
							<value id=""B"">Jani</value>
							<value id=""C"">Reminder</value>
							<value id=""D"">Don't forget me this weekend!</value>
						</note>");
			result2 = doc;

			doc = new XmlDocument();
			doc.LoadXml(@"<note>
							<value id=""A"">Tove</value>
							<value id=""B"">Jani</value>
							<value id=""C"">Reminder</value>
							<value id=""D"">Don't forget me this weekend!</value>
						</note>");

			originalDocs.Add(doc);

			doc = new XmlDocument();
			doc.LoadXml(@"<note>
							<value id=""A"">Test sdf</value>
							<value id=""B"">Jani</value>
							<value id=""C"">Reminder</value>
							<value id=""D"">Don't forget me this weekend!</value>
						</note>");

			docs.Add(doc);

			doc = new XmlDocument();
			doc.LoadXml(@"<note>
							<value id=""A"">test2</value>
							<value id=""B"">Jani</value>
							<value id=""D"">Don't forget me this weekend!</value>
						</note>");

			docs.Add(doc);

			XmlCombiner2.Combine(result2, originalDocs, docs, breadcrumbs, true);


			//

			if (runningTask != null && !runningTask.IsCompleted)
			{
				busyIndicator.IsBusy = true;
				await Task.Run(() =>
				{
					runningTask.Wait();
				});
				busyIndicator.IsBusy = false;
			}


			if (fileList.Items.Count < 2)
			{
				ErrorDialog.Show("You need to select at least 2 files");
				return;
			}

			var fileName = Path.GetFileName(fileList.Items[0].File);
			foreach (Item item in fileList.Items)
			{
				if (Path.GetFileName(item.File) != fileName)
				{
					ErrorDialog.Show("All files must have the same name");
				}
			}

			busyIndicator.IsBusy = true;
			try
			{
				var items = fileList.Items.Select(item => item.File).ToArray<string>();
				var alertCollissions = rbAdvancedCombine.IsChecked == true;
				await Task.Run(() =>
				{
					Combiner.Combine(items, alertCollissions);
				});

				busyIndicator.IsBusy = false;
				MessageBox.Show(this, "Combined mod can be found in dropzone folder", "Success", MessageBoxButton.OK, MessageBoxImage.None);
			}
			catch (Exception ex)
			{
				busyIndicator.IsBusy = false;
				Errors.Handle("Failed to combine mods", ex);
			}
			busyIndicator.BusyContent = "Deleting temporary files";
			runningTask = TempFolder.ClearAsync();

		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Settings.user.Save();
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Settings.local.lastRevision >= 4 && Settings.local.lastRevision < Settings.revision && Settings.local.lastInstallPath != Settings.currentPath)
			{
				var defaultFilesPath = Path.Combine(Settings.local.lastInstallPath, @"Files\Default files");

				if (Directory.Exists(defaultFilesPath))
				{
					busyIndicator.IsBusy = true;
					busyIndicator.BusyContent = "Moving default file cache from " + defaultFilesPath;
					await Task.Run(() =>
					{
						foreach (var file in Directory.EnumerateFiles(defaultFilesPath, "*", SearchOption.AllDirectories))
						{
							var relativePath = file.Substring(defaultFilesPath.Length + 1);
							var newPath = Path.Combine(Settings.defaultFiles, relativePath);
							if (!File.Exists(newPath))
							{
								File.Move(file, newPath);
							}
						}
					});
					busyIndicator.IsBusy = false;
				}
			}

			Settings.local.Save();
		}


	}

}
