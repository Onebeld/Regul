using System.Runtime.Serialization;
using System.Text.Json;
using Avalonia.Collections;
using PleasantUI.Other;

namespace PleasantUI.Media;

[DataContract]
public class Theme : ViewModelBase, ICloneable
{
    private string _name = string.Empty;
    private AvaloniaList<KeyColor> _colors = null!;

    [DataMember]
    public string Name
    {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    [DataMember]
    public AvaloniaList<KeyColor> Colors
    {
        get => _colors;
        set => RaiseAndSetIfChanged(ref _colors, value);
    }

    public static Theme LoadFromJson(Stream stream) => JsonSerializer.Deserialize<Theme>(stream) ?? throw new NullReferenceException();
    public static Theme LoadFromJson(string? jsonText) => JsonSerializer.Deserialize<Theme>(jsonText ?? throw new ArgumentNullException(nameof(jsonText))) ?? throw new NullReferenceException();

    public object Clone()
    {
        Theme theme = (Theme)MemberwiseClone();
        theme.Colors = new AvaloniaList<KeyColor>();

        foreach (KeyColor color in Colors)
            theme.Colors.Add(color);

        return theme;
    }
}