using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Onebeld.Extensions;

namespace Regul.SaveCleaner
{
	[DataContract]
	public class SaveCleanerSettings : ViewModelBase
	{
		private bool _deletingCharacterPortraits;
		private bool _removingLotThumbnails;
		private bool _removingPhotos;
		private bool _removingTextures;
		private bool _removingGeneratedImages;
		private bool _removingFamilyPortraits;
		private bool _createABackup;
		private bool _clearCache;
		private bool _removeOtherTypes;
		private string _pathToTheSims3Document;
		private string _pathToSaves;

		public SaveCleanerSettings()
		{
			if (FirstRun)
			{
				DeletingCharacterPortraits = true;
				RemovingFamilyPortraits = true;
				PathToTheSims3Document = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Electronic Arts\\The Sims 3";
				PathToSaves = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Electronic Arts\\The Sims 3", "Saves");

				FirstRun = false;
			}
		}

		public static SaveCleanerSettings Settings { get; set; }

		public static SaveCleanerSettings Load()
		{
			if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Settings"))
				Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Settings");

			try
			{
				using (FileStream fileStream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "Settings/" + "regulSaveCleaner.xml"))
					return (SaveCleanerSettings)new XmlSerializer(typeof(SaveCleanerSettings)).Deserialize(fileStream);
			}
			catch
			{
				return new SaveCleanerSettings();
			}
		}

		public static void Save()
		{
			using (FileStream fileStream = File.Create(AppDomain.CurrentDomain.BaseDirectory + "Settings/" + "regulSaveCleaner.xml"))
				new XmlSerializer(typeof(SaveCleanerSettings)).Serialize(fileStream, Settings);
		}

		[DataMember]
		public bool FirstRun { get; set; } = true;

		[DataMember]
		public bool DeletingCharacterPortraits
		{
			get => _deletingCharacterPortraits;
			set => RaiseAndSetIfChanged(ref _deletingCharacterPortraits, value);
		}
		[DataMember]
		public bool RemovingLotThumbnails
		{
			get => _removingLotThumbnails;
			set => RaiseAndSetIfChanged(ref _removingLotThumbnails, value);
		}
		[DataMember]
		public bool RemovingPhotos
		{
			get => _removingPhotos;
			set => RaiseAndSetIfChanged(ref _removingPhotos, value);
		}
		[DataMember]
		public bool RemovingTextures
		{
			get => _removingTextures;
			set => RaiseAndSetIfChanged(ref _removingTextures, value);
		}
		[DataMember]
		public bool RemovingGeneratedImages
		{
			get => _removingGeneratedImages;
			set => RaiseAndSetIfChanged(ref _removingGeneratedImages, value);
		}
		[DataMember]
		public bool RemovingFamilyPortraits
		{
			get => _removingFamilyPortraits;
			set => RaiseAndSetIfChanged(ref _removingFamilyPortraits, value);
		}
		[DataMember]
		public bool CreateABackup
		{
			get => _createABackup;
			set => RaiseAndSetIfChanged(ref _createABackup, value);
		}
		[DataMember]
		public bool ClearCache
		{
			get => _clearCache;
			set => RaiseAndSetIfChanged(ref _clearCache, value);
		}

		[DataMember]
		public bool RemoveOtherTypes
		{
			get => _removeOtherTypes;
			set => RaiseAndSetIfChanged(ref _removeOtherTypes, value);
		}

		[DataMember]
		public string PathToTheSims3Document
		{
			get => _pathToTheSims3Document;
			set => RaiseAndSetIfChanged(ref _pathToTheSims3Document, value);
		}
		[DataMember]
		public string PathToSaves
		{
			get => _pathToSaves;
			set => RaiseAndSetIfChanged(ref _pathToSaves, value);
		}
	}
}