using System.Collections.Generic;
using System.Xml;

namespace Just_Cause_3_Mod_Combiner
{
	public class NodeDictionary : Dictionary<string, XmlNode>
	{
		public NodeDictionary() : base()
		{

		}

		public NodeDictionary(XmlNode e) : base()
		{
			if (e == null)
				return;
			foreach (XmlNode node in e.ChildNodes)
			{
				//TODO: Check what happens if there are nodes without attributes!!
				if (node != null && node.Attributes != null)
				{
					if (node.Attributes["id"] != null)
						this.Add(node.Attributes["id"].Value, node);
					else if (node.Attributes["name"] != null)
						this.Add(node.Attributes["name"].Value, node);
				}
			}
		}
	}
}
