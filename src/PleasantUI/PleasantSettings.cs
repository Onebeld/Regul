using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Onebeld.Extensions;

namespace PleasantUI
{
    [DataContract]
	public class PleasantSettings : ViewModelBase
	{
        private bool _enableShadowing = true;

        public static PleasantSettings Settings { get; set; }

        public static PleasantSettings Load()
        {
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Settings"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Settings");

            try
            {
                using (FileStream fileStream = File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "Settings/" + "pleasantUI.xml"))
                    return (PleasantSettings)new XmlSerializer(typeof(PleasantSettings)).Deserialize(fileStream);
            }
            catch
            {
                return new PleasantSettings();
            }
        }

        public static void Save()
        {
            using (FileStream fileStream = File.Create(AppDomain.CurrentDomain.BaseDirectory + "Settings/" + "pleasantUI.xml"))
                new XmlSerializer(typeof(PleasantSettings)).Serialize(fileStream, Settings);
        }

        [DataMember]
        public bool EnableShadowing
		{
            get => _enableShadowing;
            set => RaiseAndSetIfChanged(ref _enableShadowing, value);
		}
    }
}
