using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace Just_Cause_3_Mod_Combiner
{
	/// <summary>
	/// Interaction logic for SelectionDialog.xaml
	/// </summary>
	///

	public class SelectionItem
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public bool Selected { get; set; }
		public object Value { get; set; }
	}

	public partial class SelectionDialog : Window
	{

		public bool DontShowAgain { get; set; }
		public IList<SelectionItem> Items { get; set; }
		public object SelectedItem;

		public SelectionDialog()
		{
			InitializeComponent();
		}


		public SelectionDialog(IList<SelectionItem> items)
		{
			InitializeComponent();

			foreach (var item in items)
				item.Selected = false;
			this.MaxHeight = System.Windows.SystemParameters.PrimaryScreenWidth;
			this.Items = items;

			this.DataContext = this;
		}

		public static bool Show(IList<SelectionItem> items, out object selectedValue, out bool notifyCollissions)
		{
			Debug.WriteLine(items.Count);
			object result = null;
			bool notifyColl = false;
			bool dialogResult = false;
			Settings.mainWindow.Dispatcher.Invoke((System.Windows.Forms.MethodInvoker)delegate
			{
				var dialog = new SelectionDialog(items);
				dialog.ShowDialog();
				result = dialog.SelectedItem;
				notifyColl = dialog.DontShowAgain;
				dialogResult = dialog.DialogResult == true;
			});
			selectedValue = result;
			notifyCollissions = notifyColl;
			return dialogResult;
		}

		private void SelectClicked(object sender, RoutedEventArgs e)
		{
			bool selectedValue = false;

			foreach (SelectionItem item in Items)
			{
				if (item.Selected)
				{
					this.SelectedItem = item.Value;
					selectedValue = true;
					break;
				}
			}

			if (selectedValue)
			{
				this.DialogResult = true;
			}
			else
			{
				MessageBox.Show("Select value");
			}
		}
	}
}
