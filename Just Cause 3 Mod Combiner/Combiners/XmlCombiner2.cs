using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Just_Cause_3_Mod_Combiner
{
	public class XmlCombiner2
	{
		private static bool notifyCollissions;

		public static void Combine(List<string> originalFiles, List<string> files, List<string> breadcrumbs, bool notifyCollissions, string outputPath)
		{

			XmlCombiner2.notifyCollissions = notifyCollissions;


			var result = XmlTools.LoadDocument(originalFiles[originalFiles.Count - 1]);
			var originalDocElems = new List<XmlNode>();

			var docElems = new List<XmlNode>(files.Count);
			foreach (string file in files)
			{
				docElems.Add(XmlTools.LoadDocument(file).DocumentElement);
			}
			foreach (string file in originalFiles)
			{
				originalDocElems.Add(XmlTools.LoadDocument(file).DocumentElement);
			}

			RecursiveCombine(result.DocumentElement, originalDocElems, docElems, breadcrumbs);

			File.Delete(outputPath);
			Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
			result.Save(outputPath);
		}

		public static void Combine(XmlDocument result, List<XmlDocument> originalDocs, List<XmlDocument> docs, List<string> breadcrumbs, bool notifyCollissions)
		{
			var docElems = new List<XmlNode>(docs.Count);
			var originalDocElems = new List<XmlNode>(originalDocs.Count);

			foreach (XmlDocument file in docs)
			{
				docElems.Add(file.DocumentElement);
			}
			foreach (XmlDocument file in originalDocs)
			{
				originalDocElems.Add(file.DocumentElement);
			}
			XmlCombiner2.notifyCollissions = notifyCollissions;
			RecursiveCombine(result.DocumentElement, originalDocElems, docElems, breadcrumbs);
			System.Diagnostics.Debug.WriteLine(XmlTools.GetOuterXml(result.DocumentElement));
		}

		private static void RecursiveCombine(XmlNode result, List<XmlNode> originalNodes, List<XmlNode> nodes, List<string> breadcrumbs)
		{

			if (result.NodeType == XmlNodeType.Text)
			{
				System.Diagnostics.Debug.WriteLine(result.Value);
				foreach (var node in originalNodes)
					if (node.NodeType != XmlNodeType.Text)
						throw new Exception("All original nodes are not text nodes. Post a comment at jc3mods telling what mods you were trying to combine.");
				foreach (var node in nodes)
					if (node.NodeType != XmlNodeType.Text)
						throw new Exception("All nodes are not text nodes. Post a comment at jc3mods telling what mods you were trying to combine.");

				var selectionItems = GetSelectionItems(originalNodes, nodes, breadcrumbs);

				if (selectionItems.Count == 0)
					return;
				if (selectionItems.Count == 1)
				{
					if (selectionItems[0].Name != "Original")
						ChangeNode(result, (XmlNode)selectionItems[0].Value);
					return;
				}
				if (selectionItems[0].Name == "Original" && selectionItems.Count == 2)
				{
					ChangeNode(result, (XmlNode)selectionItems[1].Value);
				}
				var replacingNode = (XmlNode)selectionItems[selectionItems.Count - 1].Value;
				object selectionResult = null;
				if (notifyCollissions && SelectionDialog.Show("Collission at " + XmlTools.GetPath(result), selectionItems, out selectionResult, out notifyCollissions))
				{
					replacingNode = (XmlNode)selectionResult;
				}
				ChangeNode(result, replacingNode);

			}
			else if (result.NodeType == XmlNodeType.Element)
			{

				foreach (var node in originalNodes)
					if (node.NodeType != XmlNodeType.Element)
						throw new Exception("All original nodes are not Elements. Post a comment at jc3mods telling what mods you were trying to combine.");
				foreach (var node in nodes)
					if (node.NodeType != XmlNodeType.Element)
						throw new Exception("All nodes are not Elements. Post a comment at jc3mods telling what mods you were trying to combine.");

				var nodesToRemove = new List<XmlNode>();
				foreach (XmlNode resultChildNode in result)
				{
					var correspondingOriginalNodes = GetCorrespondingNodes(resultChildNode, originalNodes);
					var correspondingNodes = GetCorrespondingNodes(resultChildNode, nodes);

					var changeIds = GetChangedIds(correspondingOriginalNodes, correspondingNodes);
					if (changeIds.Count == 0)
						break;
					if (changeIds.Count == 1)
					{
						ChangeNode(resultChildNode, correspondingNodes[changeIds[0][0]]);
						break;
					}

					var nullFound = false;
					var nonNullChangeFound = false;

					foreach (List<int> l in changeIds)
					{
						foreach (int i in l)
						{
							var node = nodes[i];
							if (node == null)
								nullFound = true;
							else
								nonNullChangeFound = true;
						}
					}

					if (nullFound && !nonNullChangeFound)
					{
						nodesToRemove.Add(resultChildNode);
					}
					else
					{
						var nullIds = new List<int>();
						var continueIds = new List<int>();

						foreach (List<int> l in changeIds)
						{
							foreach (int i in l)
							{
								if (correspondingNodes[i] == null)
									nullIds.Add(i);
								else
									continueIds.Add(i);
							}
						}

						var continueCombining = correspondingNodes[changeIds[changeIds.Count - 1][0]] != null;
						if (notifyCollissions && nullIds.Count > 0)
						{
							var selectionItems = new List<SelectionItem>();
							selectionItems.Add(new SelectionItem() { Name = "Null", Description = "", Value = false });
							selectionItems.Add(new SelectionItem() { Name = "Continue combining", Description = "", Value = true });
							foreach (int index in nullIds)
								selectionItems[1].Description += "\n" + breadcrumbs[index];
							foreach (int index in continueIds)
								selectionItems[1].Description += "\n" + breadcrumbs[index];
							object selectionResult = null;
							if (SelectionDialog.Show("Collission at " + XmlTools.GetPath(resultChildNode), selectionItems, out selectionResult, out notifyCollissions))
							{
								continueCombining = (bool)selectionResult;
							}
						}
						if (!continueCombining)
						{
							nodesToRemove.Add(resultChildNode);
						}
						else
						{
							var originalChildNodes2 = correspondingOriginalNodes;
							originalChildNodes2.RemoveAll(item => item == null);
							var childNodes2 = new List<XmlNode>(continueIds.Count);
							var breadcrumbs2 = new List<string>(continueIds.Count);
							foreach (int index in continueIds)
							{
								childNodes2.Add(correspondingNodes[index]);
								breadcrumbs2.Add(breadcrumbs[index]);
							}
							RecursiveCombine(resultChildNode, originalChildNodes2, childNodes2, breadcrumbs2);
						}
					}




				}
				foreach (XmlNode node in nodesToRemove)
				{
					result.RemoveChild(node);
				}
			}
			else
			{
				throw new Exception("Cant combine nodes of type " + result.NodeType);
			}

			//Doo add thing
		}

		private static void AddittiveCombine(XmlNode originalNode, List<XmlNode> nodes)
		{
			var nodeDicts = new List<NodeDictionary>(nodes.Count);
			foreach (XmlNode node in nodes)
			{
				nodeDicts.Add(new NodeDictionary(node));
			}

			for (var i = nodes.Count - 1; i >= 0; i--)
			{
				foreach (XmlNode childNode in nodes[i])
				{
					var id = XmlTools.GetID(childNode);

				}
			}
		}

		private static void ChangeNode(XmlNode originalNode, XmlNode replacingNode)
		{
			if (originalNode.NodeType != replacingNode.NodeType)
			{
				throw new Exception("Nodes don't have the same type. Node 1: " + originalNode.NodeType + " Node 2: " + replacingNode.NodeType);
			}
			if (originalNode.NodeType == XmlNodeType.Element)
			{
				if (originalNode == replacingNode)
					return;

				foreach (XmlNode node in originalNode)
				{
					originalNode.RemoveChild(node);
				}
				if (replacingNode != null)
				{
					foreach (XmlNode node in replacingNode)
					{
						var newChild = originalNode.OwnerDocument.ImportNode(node, true);
						originalNode.AppendChild(newChild);
					}
				}
			}
			else if (originalNode.NodeType == XmlNodeType.Text)
			{
				originalNode.Value = replacingNode.Value;
			}
			else
			{
				throw new Exception("Can't replace nodes type of " + originalNode.NodeType);
			}
		}

		private static List<SelectionItem> GetSelectionItems(List<XmlNode> originalNodes, List<XmlNode> nodes, List<string> breadcrumbs)
		{
			var changeIds = GetChangedIds(originalNodes, nodes);
			var selectionItems = new List<SelectionItem>();
			if (originalNodes.Count != 0)
			{
				var originalNode = originalNodes[originalNodes.Count - 1];
				selectionItems.Add(new SelectionItem() { Name = "Original", Description = XmlTools.GetOuterXml(originalNode), Value = originalNode });
			}
			foreach (List<int> l in changeIds)
			{
				var item = new SelectionItem() { Name = breadcrumbs[l[0]], Description = XmlTools.GetOuterXml(nodes[l[0]]), Value = nodes[l[0]] };
				for (var i = 1; i < l.Count; i++)
				{
					item.Name += "\n" + breadcrumbs[l[i]];
				}
				selectionItems.Add(item);
			}
			return selectionItems;
		}

		private static List<List<int>> GetChangedIds(List<XmlNode> originalNodes, List<XmlNode> nodes)
		{
			var changeIds = new List<List<int>>();
			for (var i = 0; i < nodes.Count; i++)
			{
				var node = nodes[i];
				bool isChanged = true;
				foreach (XmlNode originalNode in originalNodes)
				{
					if ((node == null || originalNode == null) && node == originalNode)
					{
						isChanged = false;
						break;
					}
					else if (node.OuterXml == originalNode.OuterXml)
					{
						isChanged = false;
						break;
					}
				}
				if (isChanged)
				{
					var equalNodeFound = false;
					foreach (List<int> l in changeIds)
					{
						var node2 = nodes[l[0]];
						if (node == null && node2 == null || (node != null && node2 != null && node.OuterXml == node2.OuterXml))
						{
							equalNodeFound = true;
							l.Add(i);
							break;
						}
					}
					if (!equalNodeFound)
						changeIds.Add(new List<int>() { i });
				}
			}
			return changeIds;
		}

		private static List<XmlNode> GetCorrespondingNodes(XmlNode originalNode, List<XmlNode> nodes)
		{
			var correspondingNodes = new List<XmlNode>(nodes.Count);
			var originalId = XmlTools.GetID(originalNode);
			if (originalId == null)
			{
				foreach (var node in nodes)
				{
					XmlNode result = null;
					if (originalNode.ParentNode.ChildNodes.Count == 1 && node.ChildNodes.Count == 1)
					{
						result = node.ChildNodes[0];
					}
					correspondingNodes.Add(result);
				}
			}
			else
			{
				foreach (var node in nodes)
				{
					XmlNode result = null;
					foreach (XmlNode childNode in node)
					{
						var id = XmlTools.GetID(childNode);
						if (id == originalId)
						{
							result = childNode;
							break;
						}
					}
					correspondingNodes.Add(result);
				}
			}

			return correspondingNodes;
		}



	}
}
