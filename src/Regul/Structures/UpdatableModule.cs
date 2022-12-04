using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Regul.Structures;

[DataContract]
public class UpdatableModule
{
    [XmlAttribute]
    public string Path { get; set; } = string.Empty;

    [XmlAttribute]
    public string PathToModule { get; set; } = string.Empty;
}