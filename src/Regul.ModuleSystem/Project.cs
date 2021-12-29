using System.Runtime.Serialization;
using Onebeld.Extensions;

namespace Regul.ModuleSystem
{
	public class Project : ViewModelBase
	{
		private string _name;
		private string _path;
		private string _editorIcon;
		private string _editorName;

		[DataMember]
		public string Name
		{
			get => _name;
			set => RaiseAndSetIfChanged(ref _name, value);
		}

		[DataMember]
		public string Path
		{
			get => _path;
			set => RaiseAndSetIfChanged(ref _path, value);
		}

		[DataMember]
		public string EditorIcon
		{
			get => _editorIcon;
			set => RaiseAndSetIfChanged(ref _editorIcon, value);
		}

		[DataMember]
		public string EditorName
		{
			get => _editorName;
			set => RaiseAndSetIfChanged(ref _editorName, value);
		}
	}
}