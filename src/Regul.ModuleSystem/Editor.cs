using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace Regul.ModuleSystem;

public class Editor
{
    public Editor(string id, 
        string name, 
        string? iconKey, 
        List<FileDialogFilter> dialogFilters,
        Func<IEditor> creatingAnEditor)
    {
        Id = id;
        Name = name;
        IconKey = iconKey;
        CreatingAnEditor = creatingAnEditor;
        DialogFilters = dialogFilters;
    }
    
    public string Id { get; }
    public string Name { get; set; }
    public Func<IEditor> CreatingAnEditor { get; }
    public string? IconKey { get; set; }
    public List<FileDialogFilter> DialogFilters { get; }
}