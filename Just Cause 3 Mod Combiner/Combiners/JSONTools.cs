using Newtonsoft.Json.Linq;

namespace Just_Cause_3_Mod_Combiner
{
	public static class JSONTools
	{
		public static object GetID(JToken token)
		{
			if (token.GetType() == typeof(JObject))
			{
				var obj = (JObject)token;
				if (obj.GetValue("id") != null)
					return obj.GetValue("id");
				else
					return obj.GetValue("name");
			}
			else if (token.GetType() == typeof(JValue))
			{
				var value = (JValue)token;

			}
			return null;
		}
	}
}
