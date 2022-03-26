using System.Xml.Serialization;

namespace Regul.Settings
{
    public class CorrespondingExtensionEditor
    {
        [XmlAttribute] public string Extension { get; set; }

        [XmlAttribute] public string IdEditor { get; set; }
    }
}