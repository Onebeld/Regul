using System.Reflection;
using Avalonia.Media;
using PleasantUI.Enums;

namespace PleasantUI.Media;

internal static class KnownColors
{
    private static readonly IReadOnlyDictionary<string, KnownColor> KnownColorNames;
    private static readonly IReadOnlyDictionary<uint, string> KnownColorsDictionary;
#if !BUILDTASK
    private static readonly Dictionary<KnownColor, ISolidColorBrush> KnownBrushes;
#endif

    static KnownColors()
    {
        Dictionary<string, KnownColor> knownColorNames = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<uint, string> knownColors = new();

        foreach (FieldInfo field in typeof(KnownColor).GetRuntimeFields())
        {
            if (field.FieldType != typeof(KnownColor)) continue;
            KnownColor knownColor = (KnownColor)field.GetValue(null)!;
            if (knownColor == KnownColor.None) continue;

            knownColorNames.Add(field.Name, knownColor);

            // some known colors have the same value, so use the first
            if (!knownColors.ContainsKey((uint)knownColor))
            {
                knownColors.Add((uint)knownColor, field.Name);
            }
        }

        KnownColorNames = knownColorNames;
        KnownColorsDictionary = knownColors;

#if !BUILDTASK
        KnownBrushes = new Dictionary<KnownColor, ISolidColorBrush>();
#endif
    }

#if !BUILDTASK
    public static ISolidColorBrush? GetKnownBrush(string s)
    {
        KnownColor color = GetKnownColor(s);
        return color != KnownColor.None ? color.ToBrush() : null;
    }
#endif

    public static KnownColor GetKnownColor(string s)
    {
        if (KnownColorNames.TryGetValue(s, out KnownColor color))
        {
            return color;
        }

        return KnownColor.None;
    }

    public static string? GetKnownColorName(uint rgb)
    {
        return KnownColorsDictionary.TryGetValue(rgb, out string? name) ? name : null;
    }

    public static Color ToColor(this KnownColor color)
    {
        return Color.FromUInt32((uint)color);
    }

#if !BUILDTASK
    public static ISolidColorBrush ToBrush(this KnownColor color)
    {
        lock (KnownBrushes)
        {
            if (!KnownBrushes.TryGetValue(color, out ISolidColorBrush? brush))
            {
                brush = new Avalonia.Media.Immutable.ImmutableSolidColorBrush(color.ToColor());
                KnownBrushes.Add(color, brush);
            }

            return brush;
        }
    }
#endif
}