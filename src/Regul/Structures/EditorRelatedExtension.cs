using System.Xml.Serialization;

namespace Regul.Structures;

public class EditorRelatedExtension
{
    [XmlAttribute]
    public string Extension { get; set; } = string.Empty;

    [XmlAttribute]
    public ulong IdEditor { get; set; }
}