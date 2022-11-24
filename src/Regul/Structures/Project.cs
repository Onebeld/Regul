using System.Runtime.Serialization;
using System.Xml.Serialization;
using PleasantUI;

namespace Regul.Structures;

public class Project : ViewModelBase
{
    private ulong _idEditor;
    private string _path = string.Empty;
    private string _dateTime = string.Empty;

    [XmlAttribute]
    [DataMember]
    public string Path
    {
        get => _path;
        set => RaiseAndSetIfChanged(ref _path, value);
    }

    [XmlAttribute]
    [DataMember]
    public ulong IdEditor
    {
        get => _idEditor;
        set => RaiseAndSetIfChanged(ref _idEditor, value);
    }

    [XmlAttribute]
    [DataMember]
    public string DateTime
    {
        get => _dateTime;
        set => RaiseAndSetIfChanged(ref _dateTime, value);
    }

    public Project()
    {
        
    }

    public Project(ulong idEditor, string path, string dateTime)
    {
        IdEditor = idEditor;
        Path = path;
        DateTime = dateTime;
    }
}