using Avalonia.Collections;
using Onebeld.Extensions;
using Regul.S3PI.Interfaces;
using System;
using System.Reflection;

namespace Regul.S3PE.Windows.Editors
{
	public class DataGridViewModel : ViewModelBase
	{
		private AvaloniaList<PropertyViewModel> _properties = new AvaloniaList<PropertyViewModel>();

		public object ResourceObject { get; set; }

		private Type _typeObject;

		public DataGridViewModel(object resource)
		{
			ResourceObject = resource;
			_typeObject = resource.GetType();

			foreach (PropertyInfo property in _typeObject.GetProperties())
			{
				foreach (CustomAttributeData item in property.CustomAttributes)
				{
					if (item.AttributeType == typeof(ElementPriorityAttribute))
					{

					}
				}
			}
		}
	}
}
