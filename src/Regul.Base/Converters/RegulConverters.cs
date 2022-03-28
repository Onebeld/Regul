using System;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Regul.ModuleSystem;

namespace Regul.Base.Converters;

public static class RegulConverters
{
    public static readonly IValueConverter StringToGeometry =
        new FuncValueConverter<string, Geometry>(Geometry.Parse);

    public static readonly IValueConverter PathToFileName =
        new FuncValueConverter<string, string>(Path.GetFileName);

    public static readonly IValueConverter IdEditorToNameEditor =
        new FuncValueConverter<string?, string?>(id =>
        {
            //Editor editor = ModuleManager.Editors.FirstOrDefault(x => x.Id == id);
            Editor? editor = null;
            for (int i = 0; i < ModuleManager.Editors.Count; i++)
            {
                Editor item = ModuleManager.Editors[i] ?? throw new NullReferenceException();

                if (item.Id == id)
                {
                    editor = item;
                    break;
                }
            }
            //

            return editor is not null ? editor.Name : "Undefined";
        });

    public static readonly IValueConverter IdEditorToGeometry =
        new FuncValueConverter<string?, Geometry?>(id =>
        {
            //Editor editor = ModuleManager.Editors.FirstOrDefault(x => x.Id == id);
            Editor? editor = null;
            for (int i = 0; i < ModuleManager.Editors.Count; i++)
            {
                Editor item = ModuleManager.Editors[i] ?? throw new NullReferenceException();

                if (item.Id == id)
                {
                    editor = item;
                    break;
                }
            }
            //

            return App.GetResource<Geometry>(editor is not null ? editor.IconKey : "UnknownIcon");
        });
        
    public static readonly IValueConverter IconKeyToGeometry =
        new FuncValueConverter<string, Geometry>(App.GetResource<Geometry>);
}