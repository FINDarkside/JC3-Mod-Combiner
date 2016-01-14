using System.Text;
using System.Xml;

namespace Just_Cause_3_Mod_Combiner
{
	public static class XmlTools
	{
		public static string GetID(XmlNode node)
		{
			if (node.Attributes == null)
				return null;
			if (node.Attributes["id"] != null)
				return node.Attributes["id"].Value;
			else if (node.Attributes["name"] != null)
				return node.Attributes["name"].Value;
			return null;
		}

		public static XmlDocument LoadDocument(string file)
		{
			var doc = new XmlDocument();
			doc.Load(file);
			return doc;
		}

		public static string GetOuterXml(XmlNode node)
		{
			if (node == null)
				return "null";
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "   ",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				ConformanceLevel = ConformanceLevel.Auto
			};
			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				node.WriteTo(writer);
			}
			return sb.ToString();
		}

		public static string GetInnerXml(XmlNode node)
		{
			if (node == null)
				return "null";
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "   ",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				ConformanceLevel = ConformanceLevel.Auto
			};
			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				node.WriteContentTo(writer);
			}
			return sb.ToString();
		}

		public static string GetPath(XmlNode node)
		{
			var id = GetID(node.ParentNode);
			string path = (id != null ? id : node.ParentNode.Name);
			var n = node.ParentNode.ParentNode;

			while (n.ParentNode != null)
			{
				id = GetID(n);
				path = (id != null ? id : n.Name) + "." + path;
				n = n.ParentNode;
			}
			return path;
		}
	}
}
