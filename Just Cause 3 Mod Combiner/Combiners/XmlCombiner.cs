using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Just_Cause_3_Mod_Combiner
{
	public class XmlCombiner
	{
		private XmlDocument originalDoc;
		private XmlDocument[] docs;
		private string[] fileNames;
		private bool notifyCollissions;

		public XmlCombiner(string originalFile, IList<string> files, IList<string> binFiles, bool notifyCollissions)
		{
			this.fileNames = binFiles.ToArray<string>();
			docs = new XmlDocument[files.Count];
			int i = 0;
			foreach (string file in files)
			{
				docs[i] = XmlTools.LoadDocument(file);
				i++;
			}
			this.originalDoc = XmlTools.LoadDocument(originalFile);
			this.notifyCollissions = notifyCollissions;
		}

		public XmlCombiner(XmlDocument originalDoc, IEnumerable<XmlDocument> docs, bool notifyCollissions)
		{
			this.originalDoc = originalDoc;
			this.docs = docs.ToArray<XmlDocument>();
			this.notifyCollissions = notifyCollissions;
		}

		public void Combine(string newFileLocation)
		{

			var docElems = new XmlNode[docs.Length];
			for (var i = 0; i < docs.Length; i++)
			{
				docElems[i] = docs[i].DocumentElement;
			}
			RecursiveCombine2(originalDoc.DocumentElement, docElems);
			System.Diagnostics.Debug.WriteLine(XmlTools.GetOuterXml(originalDoc.DocumentElement));

			var dir = Path.GetDirectoryName(newFileLocation);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			originalDoc.Save(newFileLocation);
		}

		private void RecursiveCombine2(XmlNode originalNode, IList<XmlNode> nodes)
		{

			var nodeDict = new List<NodeDictionary>();
			foreach (var node in nodes)
			{
				nodeDict.Add(new NodeDictionary(node));
			}

			if (originalNode.ChildNodes.Count >= 1 && originalNode.ChildNodes[0].NodeType == XmlNodeType.Text)
			{
				var overridingNode = GetOverridingNode(originalNode, nodes);
				ChangeNode(originalNode, overridingNode);
			}
			else if (originalNode.NodeType == XmlNodeType.Element)
			{
				var used = new HashSet<XmlNode>();
				var nodesToRemove = new List<XmlNode>();
				foreach (XmlNode originalChildNode in originalNode.ChildNodes)
				{
					var correspodingNodes = GetCorrespondingNodes(originalChildNode, nodes, nodeDict);
					used.Add(originalChildNode);
					foreach (XmlNode node in correspodingNodes)
					{
						used.Add(node);
					}

					var changeIDs = GetChangeIDs(originalChildNode, correspodingNodes);
					var changeNodes = new List<XmlNode>();
					var files = new List<string>();
					var uniqueChanges = new HashSet<string>();

					var nullFound = false;
					var textNodeFound = false;
					var nonNullChangeFound = false;
					foreach (int index in changeIDs)
					{
						var node = correspodingNodes[index];
						if (node == null)
							uniqueChanges.Add(null);
						else
							uniqueChanges.Add(node.OuterXml);
						if (index >= this.fileNames.Length)
							files.Add("Failed to get file name");
						else
							files.Add(this.fileNames[index]);
						changeNodes.Add(node);
						if (node == null)
							nullFound = true;
						else if (node.NodeType == XmlNodeType.Text)
							textNodeFound = true;
						if (node != null && node.OuterXml != originalChildNode.OuterXml)
							nonNullChangeFound = true;
					}

					var items = new List<SelectionItem>();
					items.Add(new SelectionItem() { Name = "Default", Description = XmlTools.GetOuterXml(originalChildNode), Value = originalChildNode });
					foreach (int index in changeIDs)
					{
						var changedNode = correspodingNodes[index];
						for (var i = 1; i < items.Count; i++)
						{
							var found = false;
							if (changedNode.OuterXml == originalChildNode.OuterXml)
							{
								items[i].Name += "\n" + fileNames[index];
								found = true;
								break;
							}
							if (!found)
								items.Add(new SelectionItem() { Name = fileNames[index], Description = XmlTools.GetOuterXml(changedNode), Value = changedNode });
						}
					}



					//

					//
					if (textNodeFound)
					{
						//TODO:Clean ^?
						var overridingNode = changeNodes[changeNodes.Count - 1];
						object result = null;
						if (uniqueChanges.Count > 1 && notifyCollissions && SelectionDialog.Show(items, out result, out notifyCollissions))
						{
							overridingNode = result as XmlNode;
						}

						if (overridingNode == null)
						{
							nodesToRemove.Add(originalChildNode);
							foreach (XmlNode node in correspodingNodes)
							{
								if (node != null)
									node.ParentNode.RemoveChild(node);
							}
						}
						else if (overridingNode.NodeType == XmlNodeType.Text)
						{
							if (used.Contains(overridingNode))
								nodesToRemove.Add(originalChildNode);
							else
								ChangeNode(originalChildNode, overridingNode);
							foreach (XmlNode node in correspodingNodes)
							{
								if (node != null)
									node.ParentNode.RemoveChild(node);
							}
						}
						else
						{
							foreach (int index in changeIDs)
							{
								if (correspodingNodes[index] == null || correspodingNodes[index].NodeType == XmlNodeType.Text)
								{
									var parent = nodes[index];
									var child = parent.OwnerDocument.ImportNode(originalChildNode, true);
									if (correspodingNodes[index] != null)
										parent.RemoveChild(correspodingNodes[index]);
									used.Add(child);
									parent.AppendChild(child);
									correspodingNodes[index] = child;
								}
							}
							RecursiveCombine2(originalChildNode, correspodingNodes);
						}
					}
					else if (nullFound && !nonNullChangeFound)
					{
						nodesToRemove.Add(originalChildNode);
						foreach (XmlNode node in correspodingNodes)
						{
							if (node != null)
								ChangeNode(node, null);
						}

					}
					else if (nullFound && nonNullChangeFound)
					{
						//Fix to ask if continue combine or not
						var continueCombining = true;
						object result = null;
						if (notifyCollissions && SelectionDialog.Show(items, out result, out notifyCollissions))
						{
							if (result == null)
								continueCombining = false;
						}
						if (continueCombining)
						{
							foreach (int index in changeIDs)
							{
								if (correspodingNodes[index] == null || correspodingNodes[index].NodeType == XmlNodeType.Text)
								{
									var parent = nodes[index];
									var child = parent.OwnerDocument.ImportNode(originalChildNode, true);
									if (correspodingNodes[index] != null)
										parent.RemoveChild(correspodingNodes[index]);
									parent.AppendChild(child);
									used.Add(child);
									correspodingNodes[index] = child; //Used fucks up
								}
							}
							RecursiveCombine2(originalChildNode, correspodingNodes);
						}
						else
						{
							nodesToRemove.Add(originalChildNode);
							foreach (XmlNode node in correspodingNodes)
							{
								if (node != null)
									node.ParentNode.RemoveChild(node);
							}
						}
					}
					else
					{
						RecursiveCombine2(originalChildNode, correspodingNodes);
					}
				}

				foreach (XmlNode node in nodes)
				{
					var originalDict = new NodeDictionary(originalNode);

					if (node == null || node.ChildNodes == null)
						continue;

					foreach (XmlNode childNode in node.ChildNodes)
					{
						var found = false;

						var id = XmlTools.GetID(childNode);
						if (id == null)
						{
							found = node.ChildNodes.Count == 1 && originalNode.ChildNodes.Count == 1;
						}
						else
						{
							if (originalDict.ContainsKey(id))
								found = true;
						}

						if (childNode.NodeType == XmlNodeType.Element && !found && !used.Contains(childNode)) //If only one textnode in each do the combine? Fix GetID(originalNode) == null)
						{
							used.Add(childNode);
							var correspondingNodes = GetCorrespondingNodes(childNode, nodes, nodeDict);
							foreach (XmlNode usedNode in correspondingNodes)
							{
								used.Add(usedNode);
							}
							var overridingNode = GetOverridingNode(null, correspondingNodes);
							if (overridingNode != null)
								originalNode.AppendChild(originalNode.OwnerDocument.ImportNode(overridingNode, true));
						}
					}
				}

				foreach (XmlNode node in nodesToRemove)
				{
					ChangeNode(node, null);
				}
			}
		}

		private XmlNode GetOverridingNode(XmlNode originalNode, IList<XmlNode> nodes)
		{
			if (nodes.Count == 0)
				return originalNode;

			XmlNode overridingNode = originalNode;

			var changeIDs = GetChangeIDs(originalNode, nodes);
			if (changeIDs.Count == 0)
				return originalNode;


			var set = new HashSet<string>();
			foreach (int index in changeIDs)
			{
				var node = nodes[index];
				if (node == null)
					set.Add(null);
				else
					set.Add(node.OuterXml);
			}
			var allSame = set.Count <= 1;

			if (allSame)
			{
				overridingNode = nodes[changeIDs[0]];
			}
			else
			{
				var changeNodes = new List<XmlNode>();
				var files = new List<string>();
				foreach (int index in changeIDs)
				{
					changeNodes.Add(nodes[index]);
					if (index >= this.fileNames.Length)
						files.Add("Failed to get file name");
					else
						files.Add(this.fileNames[index]);
				}

				var items = new List<SelectionItem>();
				items.Add(new SelectionItem() { Name = "Default", Description = XmlTools.GetOuterXml(originalNode), Value = originalNode });
				foreach (int index in changeIDs)
				{
					var changedNode = nodes[index];
					var found = false;
					for (var i = 1; i < items.Count; i++)
					{
						if (changedNode.OuterXml == originalNode.OuterXml)
						{
							items[i].Name += "\n" + fileNames[index];
							found = true;
							break;
						}
					}
					if (!found)
						items.Add(new SelectionItem() { Name = fileNames[index], Description = XmlTools.GetOuterXml(changedNode), Value = changedNode });

				}

				object result = null;
				if (notifyCollissions && SelectionDialog.Show(items, out result, out notifyCollissions))
				{
					overridingNode = result as XmlNode;
				}
				else
				{
					if (changeNodes.Count > 0)
						overridingNode = changeNodes[changeNodes.Count - 1];
				}
			}
			return overridingNode;
		}


		private void ChangeNode(XmlNode originalNode, XmlNode newNode)
		{
			if (originalNode == newNode)
				return;
			var parentNode = originalNode.ParentNode;
			if (newNode != null)
			{

				foreach (XmlNode node in originalNode.ChildNodes)
				{
					originalNode.RemoveChild(node);
				}
				foreach (XmlNode node in newNode.ChildNodes)
				{
					var newChild = originalNode.OwnerDocument.ImportNode(node, true);
					originalNode.AppendChild(newChild);
				}
				//Move to nodesToChange

			}
			else
			{
				parentNode.RemoveChild(originalNode);
			}
		}


		private IList<XmlNode> GetCorrespondingNodes(XmlNode originalChildNode, IList<XmlNode> nodes, IList<NodeDictionary> nodeDict)
		{
			string id = XmlTools.GetID(originalChildNode);

			XmlNode[] correspondingNodes = new XmlNode[nodes.Count];

			if (id == null)
			{
				if (originalChildNode.ParentNode.ChildNodes.Count == 1)
				{
					for (var i = 0; i < nodes.Count; i++)
					{
						var node = nodes[i];
						if (node == null)
							throw new ArgumentNullException("asdasdasd"); //TODO DELETE?
						if (node.ChildNodes.Count == 1)
							correspondingNodes[i] = node.ChildNodes[0];
						else
							throw new NotImplementedException("");
					}
				}
				else
				{
					throw new NotImplementedException("Finding corresponding nodes for nodes without id not implemented.");
				}
			}
			else
			{
				for (var i = 0; i < nodes.Count; i++)
				{
					var node = nodes[i];
					if (node != null)
					{
						if (nodeDict[i].ContainsKey(id))
						{
							correspondingNodes[i] = nodeDict[i][id];
						}
					}
				}
			}

			return correspondingNodes;
		}

		private IList<int> GetChangeIDs(XmlNode originalNode, IList<XmlNode> nodes)
		{
			var changeIDs = new List<int>();
			for (var i = 0; i < nodes.Count; i++)
			{
				var node = nodes[i];
				if (!(node == null && originalNode == null))
					if ((originalNode == null && node != null) || (originalNode != null && node == null) || (node.OuterXml != originalNode.OuterXml))
						changeIDs.Add(i);
			}
			return changeIDs;
		}

		private void AddittiveCombine(IList<XmlNode> nodes)
		{

		}



	}
}
