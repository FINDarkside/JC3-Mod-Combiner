using System;
using System.Threading.Tasks;
using System.Windows;

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
				ErrorDialog.Show(e.Message, e.ToString());
			Console.Error.WriteLine(e.ToString());
		}

		public static void Handle(string message, Exception e)
		{
			ErrorDialog.Show(message, e.Message + "\n" + e.ToString());
			Console.Error.WriteLine(e.ToString());
		}

		public static void Alert(string text)
		{
			MessageBox.Show(text, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
