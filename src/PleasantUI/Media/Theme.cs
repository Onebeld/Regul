using Avalonia.Collections;

namespace PleasantUI.Media;

public class Theme : ViewModelBase, ICloneable
{
    private string _name = string.Empty;
    private AvaloniaDictionary<string, uint> _colors = null!;

    public string Name
    {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public AvaloniaDictionary<string, uint> Colors
    {
        get => _colors;
        set => RaiseAndSetIfChanged(ref _colors, value);
    }

    public static Theme LoadFromText(string s)
    {
        char[] separators =
        {
            '\r', '\n'
        };
        string[] strings = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        Theme theme = new()
        {
            Name = strings[0], Colors = new AvaloniaDictionary<string, uint>()
        };

        for (int i = 1; i < strings.Length; i++)
        {
            if (strings[i] == string.Empty) continue;

            string[] keyValue = strings[i].Split(';');
            theme.Colors.Add(keyValue[0], uint.Parse(keyValue[1]));
        }

        return theme;
    }

    public object Clone()
    {
        Theme theme = (Theme)MemberwiseClone();
        theme.Colors = new AvaloniaDictionary<string, uint>();

        foreach (KeyValuePair<string, uint> color in Colors)
            theme.Colors.Add(color.Key, color.Value);

        return theme;
    }
}