using System;
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
				MessageBox.Show(e.ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			Console.Error.WriteLine(e.ToString());
		}

		public static void Handle(string message, Exception e)
		{
			string s = message + "\n" + e.ToString();
			MessageBox.Show(s, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
			Console.Error.WriteLine(s);
		}

		public static void Alert(string text)
		{
			MessageBox.Show(text, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
