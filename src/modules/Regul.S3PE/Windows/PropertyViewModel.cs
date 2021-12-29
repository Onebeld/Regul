using Onebeld.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Regul.S3PE.Windows
{
	public class PropertyViewModel : ViewModelBase
	{
		private object _resourceObject;

		private PropertyInfo _property;

		private string Key
		{
			get
			{
				return _property.Name;
			}
		}

		private 

		public PropertyViewModel(object resource, PropertyInfo property)
		{

		}

		private string ConvertToString(object value)
		{
			if (value is null)
				return "(null)";

			TypeConverter converter = TypeDescriptor.GetConverter(value);

			if (!converter.CanConvertTo(typeof(string)) || converter.GetType() == typeof(CollectionConverter))
			{
				return value.ToString() ?? "(null)";
			}

			return converter.ConvertToString(value);
		}

		private object ConvertFromString(string s, Type targetType)
		{
			if (string.IsNullOrEmpty(s))
				return null;

			TypeConverter converter = TypeDescriptor.GetConverter(targetType);

			if (converter.CanConvertFrom(typeof(string)))
			{
				return converter.ConvertFrom(null, CultureInfo.InvariantCulture, s);
			}

			return InvokeParse(s, targetType);
		}

		private object InvokeParse(string s, Type targetType)
		{

		}
	}
}
