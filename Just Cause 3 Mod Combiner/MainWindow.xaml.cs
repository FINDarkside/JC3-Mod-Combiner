using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using Ookii.Dialogs;
using System.Threading.Tasks;
using WForms = System.Windows.Forms;

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

		public MainWindow()
		{
			Settings.mainWindow = this;
			InitializeComponent();

			Settings.local.Save();

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
							if (WForms.MessageBox.Show("Just Cause 3 folder found at\n" + installpath, "Set as JC3 path?", WForms.MessageBoxButtons.YesNo, WForms.MessageBoxIcon.Question) == WForms.DialogResult.Yes)
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

			if (!Directory.Exists(Path.Combine(Settings.user.JC3Folder, "dropzone")))
			{
				Directory.CreateDirectory(Path.Combine(Settings.user.JC3Folder, "dropzone"));
			}

			Items = new ObservableCollection<Item>();
			fileList.Items = Items;

			RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.Linear);



		}

		private void AddFile(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.CheckFileExists = true;
			dialog.Multiselect = true;
			if (dialog.ShowDialog() == true)
			{
				var files = dialog.FileNames;
				foreach (var file in files)
				{
					fileList.AddFileToList(file);
				}
			}
		}

		private void DeleteSelectedItems(object sender, RoutedEventArgs e)
		{
			fileList.RemoveSelectedItems();
		}

		private async void CombineClicked(object sender, RoutedEventArgs e)
		{
			if (fileList.Items.Count < 2)
			{
				WForms.MessageBox.Show("You need to select at least 2 files", "", WForms.MessageBoxButtons.OK, WForms.MessageBoxIcon.Error);
				return;
			}

			var fileName = Path.GetFileName(fileList.Items[0].File);
			foreach (Item item in fileList.Items)
			{
				if (Path.GetFileName(item.File) != fileName)
				{

					WForms.MessageBox.Show("All files must have the same name", "Filenames don't match", WForms.MessageBoxButtons.OK, WForms.MessageBoxIcon.Error);

				}
				if (Path.GetExtension(item.File) != Path.GetExtension(fileName))
				{
					WForms.MessageBox.Show("Files don't have the same extension", "", WForms.MessageBoxButtons.OK, WForms.MessageBoxIcon.Error);
					return;
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
				WForms.MessageBox.Show("File combined", "Success", WForms.MessageBoxButtons.OK, WForms.MessageBoxIcon.None);

			}
			catch (Exception ex)
			{
				busyIndicator.IsBusy = false;
				Errors.Handle(ex);
			}


			var di = new DirectoryInfo(Settings.tempFolder);
			foreach (FileInfo file in di.GetFiles())
				file.Delete();
			foreach (DirectoryInfo dir in di.GetDirectories())
				dir.Delete(true);


		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Settings.user.Save();
		}


	}

}
