#region

using System.Xml.Serialization;

#endregion

namespace Regul.Settings
{
    public class ModuleForUpdate
    {
        [XmlAttribute] public string Path { get; set; }

        [XmlAttribute] public string PathToModule { get; set; }
    }
}