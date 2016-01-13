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
	public partial class SelectionDialog : Window
	{

		public CollissionData Original { get; set; }
		public List<CollissionData> Collissions { get; set; }
		public bool DontShowAgain { get; set; }
		public XmlNode SelectedNode;

		public SelectionDialog()
		{
			InitializeComponent();
		}

		public SelectionDialog(XmlNode originalNode, IList<XmlNode> collidingNodes, IList<string> fileNames)
		{
			InitializeComponent();
			this.MaxHeight = System.Windows.SystemParameters.PrimaryScreenWidth;

			var data = new CollissionData();
			data.OuterXml = XmlTools.GetOuterXml(originalNode); ;
			data.Node = originalNode;
			Original = data;

			if(originalNode != null)
				tbHeader.Text = "Collission in " + XmlTools.GetPath(originalNode);
			else
			{
				foreach(XmlNode node in collidingNodes){
					if (node != null)
					{
						tbHeader.Text = "Collission in " + XmlTools.GetPath(node);
						break;
					}
				}
			}

			var collissions = new List<CollissionData>();

			for (int i = 0; i < collidingNodes.Count; i++)
			{
				data = new CollissionData();
				var node = collidingNodes[i];
				data.OuterXml = XmlTools.GetOuterXml(node);
				data.FilePath = fileNames[i];
				data.Node = node;
				collissions.Add(data);
			}
			collissions[collissions.Count - 1].Selected = true;
			Collissions = collissions;

			this.DataContext = this;

			
		}


		public class CollissionData
		{
			public string FilePath { get; set; }
			public string OuterXml { get; set; }
			public XmlNode Node { get; set; }
			public bool Selected { get; set; }
		}

		private void SelectClicked(object sender, RoutedEventArgs e)
		{
			bool selectedValue = false;

			if (Original.Selected)
			{
				this.SelectedNode = Original.Node;
				selectedValue = true;
			}

			foreach (CollissionData data in Collissions)
			{
				if (data.Selected)
				{
					this.SelectedNode = data.Node;
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
