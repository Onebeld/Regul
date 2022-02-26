#region

using System.Xml.Serialization;

#endregion

namespace Regul.Settings
{
    public class CorrespondingExtensionEditor
    {
        [XmlAttribute] public string Extension { get; set; }

        [XmlAttribute] public string IdEditor { get; set; }
    }
}