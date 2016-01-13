using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Just_Cause_3_Mod_Combiner
{
	/// <summary>
	/// Interaction logic for DragDropBox.xaml
	/// </summary>
	public partial class DragDropBox : UserControl
	{

		public ObservableCollection<Item> Items
		{
			get { return (ObservableCollection<Item>)GetValue(ItemsProperty); }
			set
			{
				SetValue(ItemsProperty, value);
				fileList.ItemsSource = value;
			}
		}
		// Using a DependencyProperty as the backing store for Property1.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(ObservableCollection<Item>), typeof(DragDropBox), new PropertyMetadata(new ObservableCollection<Item>()));

		private Point _dragStartPoint;


		public DragDropBox()
		{
			InitializeComponent();
			fileList.ItemsSource = Items;

			fileList.AllowDrop = true;
			fileList.Drop += ListBox_Drop;

			fileList.DisplayMemberPath = "File";
			fileList.ItemsSource = Items;

			fileList.PreviewMouseMove += ListBox_PreviewMouseMove;

			var style = new Style(typeof(ListBoxItem));
			style.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
			style.Setters.Add(
				new EventSetter(
					ListBoxItem.PreviewMouseLeftButtonDownEvent,
					new MouseButtonEventHandler(ListBoxItem_PreviewMouseLeftButtonDown)));
			style.Setters.Add(
					new EventSetter(
						ListBoxItem.DropEvent,
						new DragEventHandler(ListBoxItem_Drop)));
			fileList.ItemContainerStyle = style;
		}

		public void AddFileToList(string file)
		{
			if (FileFormats.GetFileFormat(file) == FileFormat.Unknown)
			{
				Errors.Alert("Can't combine " + Path.GetExtension(file) + " files. If you need to combine these let me know at jc3mods.com");
				return;
			}
			if (File.Exists(file))
			{
				foreach (Item item in Items)
				{
					if (Path.Equals(item.File, file))
						return;
				}
			}
			Items.Add(new Item(file));
		}

		public void RemoveSelectedItems()
		{
			var selectedItems = fileList.SelectedItems;
			while (selectedItems.Count > 0)
			{
				Items.Remove(selectedItems[0] as Item);
			}
		}

		private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
		{
			var parentObject = VisualTreeHelper.GetParent(child);
			if (parentObject == null)
				return null;
			T parent = parentObject as T;
			if (parent != null)
				return parent;
			return FindVisualParent<T>(parentObject);
		}

		private void FileListMouseEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Move;
		}

		private void AddFile(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog();
			dialog.CheckFileExists = true;
			dialog.Filter = "Bin files|*.bin";
			dialog.Multiselect = true;
			if (dialog.ShowDialog() == true)
			{
				var files = dialog.FileNames;
				foreach (var file in files)
				{
					AddFileToList(file);
				}
			}

		}

		private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (Items.Count == 0)
				return;
			Point point = e.GetPosition(null);
			Vector diff = _dragStartPoint - point;
			if (e.LeftButton == MouseButtonState.Pressed &&
				(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
			{
				var lb = sender as ListBox;
				var lbi = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
				if (lbi != null)
				{
					DragDrop.DoDragDrop(lbi, lbi.DataContext, DragDropEffects.Move);
				}
			}
		}
		private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_dragStartPoint = e.GetPosition(this);
		}

		private void ListBoxItem_Drop(object sender, DragEventArgs e)
		{

		}

		private void ListBox_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (string file in files)
				{
					AddFileToList(file);
				}
			}
			else if (e.Data.GetData(typeof(Item)) != null)
			{
				var source = e.Data.GetData(typeof(Item)) as Item;
				//var target = null;

				int sourceIndex = fileList.Items.IndexOf(source);
				int targetIndex = GetCurrentIndex(e.GetPosition);

				Move(source, sourceIndex, targetIndex);
			}
		}

		private void Move(Item source, int sourceIndex, int targetIndex)
		{
			if (targetIndex > Items.Count - 1)
				targetIndex = Items.Count - 1;
			if (targetIndex < 0)
				targetIndex = 0;
			if (sourceIndex < targetIndex)
			{
				Items.Insert(targetIndex + 1, source);
				Items.RemoveAt(sourceIndex);
			}
			else
			{
				int removeIndex = sourceIndex + 1;
				if (Items.Count + 1 > removeIndex)
				{
					Items.Insert(targetIndex, source);
					Items.RemoveAt(removeIndex);
				}
			}
		}

		delegate Point GetPositionDelegate(IInputElement element);

		private int GetCurrentIndex(GetPositionDelegate getPosition)
		{
			int index = Items.Count;
			for (int i = 0; i < fileList.Items.Count; ++i)
			{
				ListViewItem item = GetListViewItem(i);
				if (item != null && this.IsMouseOverTarget(item, getPosition))
				{
					index = i;
					break;
				}
			}
			return index;
		}

		private bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
		{
			Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
			Point mousePos = getPosition((IInputElement)target);
			return bounds.Contains(mousePos);
		}

		ListViewItem GetListViewItem(int index)
		{
			if (fileList.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
				return null;

			return fileList.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
		}
	}
}
