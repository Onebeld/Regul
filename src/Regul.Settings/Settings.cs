using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Avalonia.Collections;
using Onebeld.Extensions;
using Regul.ModuleSystem;
using Regul.Settings;

namespace Regul
{
	[DataContract]
	public class GeneralSettings : ViewModelBase
	{
		private bool _hardwareAcceleration;
		private bool _showFullscreenButton;
		private bool _compactMode;
		private bool _showCustomTitleBar;
		private string _creatorName;
		private AvaloniaList<Project> _projects;
		private AvaloniaList<CorrespondingExtensionEditor> _correspondingExtensionEditors = new AvaloniaList<CorrespondingExtensionEditor>();
		private AvaloniaList<ModuleForUpdate> _modulesForUpdate = new AvaloniaList<ModuleForUpdate>();

		public GeneralSettings()
		{
			if (FirstRun)
			{
				Theme = "Light";
				Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
				ShowCustomTitleBar = true;
				HardwareAcceleration = true;

				FirstRun = false;
			}
		}

		public static GeneralSettings Settings { get; set; }

		public static GeneralSettings Load()
		{
			if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Settings"))
				Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Settings");

			try
			{
				using (FileStream fileStream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "Settings/" + "general.xml"))
					return (GeneralSettings)new XmlSerializer(typeof(GeneralSettings)).Deserialize(fileStream);
			}
			catch
			{
				return new GeneralSettings();
			}
		}

		public static void Save()
		{
			using (FileStream fileStream = File.Create(AppDomain.CurrentDomain.BaseDirectory + "Settings/" + "general.xml"))
				new XmlSerializer(typeof(GeneralSettings)).Serialize(fileStream, Settings);
		}


		[DataMember]
		public string Theme { get; set; }
		[XmlAttribute]
		[DataMember]
		public string Language { get; set; }

		[XmlAttribute]
		[DataMember]
		public bool FirstRun { get; set; } = true;

		[DataMember]
		public bool HardwareAcceleration
		{
			get => _hardwareAcceleration;
			set => RaiseAndSetIfChanged(ref _hardwareAcceleration, value);
		}
		[DataMember]
		public bool ShowFullscreenButton
		{
			get => _showFullscreenButton;
			set => RaiseAndSetIfChanged(ref _showFullscreenButton, value);
		}
		[DataMember]
		public bool CompactMode
		{
			get => _compactMode;
			set => RaiseAndSetIfChanged(ref _compactMode, value);
		}
		[DataMember]
		public bool ShowCustomTitleBar
		{
			get => _showCustomTitleBar;
			set => RaiseAndSetIfChanged(ref _showCustomTitleBar, value);
		}
		[XmlAttribute]
		[DataMember]
		public string CreatorName
		{
			get => _creatorName;
			set => RaiseAndSetIfChanged(ref _creatorName, value);
		}
		[DataMember]
		public AvaloniaList<Project> Projects
		{
			get => _projects;
			set => RaiseAndSetIfChanged(ref _projects, value);
		}

		[DataMember]
		public AvaloniaList<ModuleForUpdate> ModulesForUpdate
		{
			get => _modulesForUpdate;
			set => RaiseAndSetIfChanged(ref _modulesForUpdate, value);
		}
		[DataMember]
		public AvaloniaList<CorrespondingExtensionEditor> CorrespondingExtensionEditors
		{
			get => _correspondingExtensionEditors;
			set => RaiseAndSetIfChanged(ref _correspondingExtensionEditors, value);
		}
	}
}