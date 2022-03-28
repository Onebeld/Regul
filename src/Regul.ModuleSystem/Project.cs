using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Onebeld.Extensions;

namespace Regul.ModuleSystem;

public class Project : ViewModelBase
{
    private string _idEditor;
    private string _path;

    public Project(string idEditor, string path)
    {
        if (string.IsNullOrEmpty(idEditor) && string.IsNullOrEmpty(path))
            throw new NullReferenceException();

        IdEditor = idEditor;
        Path = path;
    }

    public Project()
    {
        
    }

    [XmlAttribute]
    [DataMember]
    public string Path
    {
        get => _path;
        set => RaiseAndSetIfChanged(ref _path, value);
    }

    [XmlAttribute]
    [DataMember]
    public string IdEditor
    {
        get => _idEditor;
        set => RaiseAndSetIfChanged(ref _idEditor, value);
    }
}