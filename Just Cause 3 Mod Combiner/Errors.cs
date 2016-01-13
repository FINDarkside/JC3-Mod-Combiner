using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WForms = System.Windows.Forms;

namespace Just_Cause_3_Mod_Combiner
{
	public static class Errors
	{

		public static void Handle(Exception e)
		{
			Handle(e, true);
		}

		public static void Handle(Exception e, bool showDialog)
		{
			if (showDialog)
				WForms.MessageBox.Show(e.ToString(), "Exception", WForms.MessageBoxButtons.OK, WForms.MessageBoxIcon.Error);
			Console.Error.WriteLine(e.ToString());
		}

		public static void Alert(string text)
		{
			WForms.MessageBox.Show(text, "Alert", WForms.MessageBoxButtons.OK, WForms.MessageBoxIcon.Error);
		}
	}
}
