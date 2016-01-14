using System;
using System.Globalization;
using System.Windows.Data;

namespace Just_Cause_3_Mod_Combiner
{
	public class ObjectToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
