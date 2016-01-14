using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Just_Cause_3_Mod_Combiner
{
	public class JSONCombiner
	{
		private string[] fileNames = new string[] { "Test1", "Test2" };
		private JObject originalDoc;
		private JObject[] docs;
		private bool notifyCollissions;

		/*public JSONCombiner(string originalFile, IList<string> files, bool notifyCollissions)
		{

			docs = new JObject[files.Count];
			int i = 0;
			foreach (string file in files)
			{
				docs[i] = JObject.Parse(File.ReadAllText(file));
			}
			this.originalDoc = JObject.Parse(File.ReadAllText(originalFile));
			
			this.notifyCollissions = notifyCollissions;
		}*/

		public JSONCombiner(string originalJSON, IList<string> json, bool notifyCollissions)
		{
			this.originalDoc = JObject.Parse(originalJSON);
			docs = new JObject[json.Count];
			for (var i = 0; i < json.Count; i++)
			{
				docs[i] = JObject.Parse(json[i]);
			}
			this.notifyCollissions = notifyCollissions;
		}

		private class StackItem
		{
			public JToken originalToken;
			public JToken[] tokens;
			public string[] files;

			public StackItem(JToken originalToken, JToken[] tokens, string[] files)
			{
				this.originalToken = originalToken;
				this.tokens = tokens;
				this.files = files;
			}
		}

		public void Combine()
		{

			var stack = new Stack<StackItem>();
			stack.Push(new StackItem(originalDoc, docs, fileNames));

			while (stack.Count != 0)
			{
				var stackItem = stack.Pop();
				var originalToken = stackItem.originalToken;
				var tokens = stackItem.tokens;
				var files = stackItem.files;
		

				if (originalToken.GetType() == typeof(JObject))
				{
					var originalObject = (JObject)originalToken;
					var objects = tokens.Cast<JObject>();

					var used = new HashSet<JToken>();

					foreach (var item in originalObject)
					{


						if (item.Value.GetType() == typeof(JObject))
						{
							var originalChildObject = (JObject)item.Value;
							//var childObjects = GetCorrespondingObjects(originalChildObject, tokens);

						}
						else if (item.Value.GetType() == typeof(JArray))
						{
							var originalArray = (JArray)item.Value;
						}
						else if (item.Value.GetType() == typeof(JValue))
						{
							var originalValue = (JValue)item.Value;
							
						}
					}
				}
			}


		}


		private object[] GetCorrespondingObjects(JValue originalObject, JContainer[] objects)
		{
			var correspondingObjects = new object[objects.Length];
			var id = JSONTools.GetID(originalObject);
			if (id == null)
			{
				if (originalObject.Children().Count() == 1)
				{
					for (var i = 0; i < objects.Length; i++)
					{
						var obj = objects[i];
						if (obj.Count == 1)
							correspondingObjects[i] = obj[0];
						else
							throw new NotImplementedException("Finding corresponding objects for nodes without id not implemented.");
					}
				}
				else
				{
					throw new NotImplementedException("Finding corresponding nodes objects nodes without id not implemented.");
				}
			}
			else
			{
				for (var i = 0; i < objects.Length; i++)
				{
					var obj = objects[i];
					var objID = JSONTools.GetID(obj);
					if (objID == id)
						correspondingObjects[i] = obj;
				}
			}
			return correspondingObjects;
		}

		/*private IList<int> GetChangeIDs(JToken originalToken, JToken[] tokens)
		{

		}*/
	}
}
