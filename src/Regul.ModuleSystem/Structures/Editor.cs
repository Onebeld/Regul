using System;
using System.Collections.Generic;
using Avalonia.Platform.Storage;

namespace Regul.ModuleSystem.Structures;

public class Editor
{
    /// <summary>
    /// Editor's ID. It is used to identify the instance of the editor.
    /// </summary>
    public ulong Id { get; }
    /// <summary>
    /// Editor's name. Can also be a key for localization.
    /// </summary>
    public string? Name { get; }
    /// <summary>
    /// The type of class that is ViewModule 
    /// </summary>
    public Type Type { get; }
    /// <summary>
    /// The key that points the resource to the editor icon.
    /// </summary>
    public string? IconKey { get; }
    /// <summary>
    /// The file types that your editor supports.
    /// </summary>
    public IReadOnlyList<FilePickerFileType> FileTypes { get; }

    public Editor(ulong id, string name, Type editorType, string? iconKey, List<FilePickerFileType> fileTypes)
    {
        Id = id;
        Name = name;
        Type = editorType;
        IconKey = iconKey;
        FileTypes = fileTypes;
    }
}