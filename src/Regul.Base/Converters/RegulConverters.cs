using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Regul.Base.Converters
{
    public static class RegulConverters
    {
        public static readonly IValueConverter StringToGeometry =
            new FuncValueConverter<string, Geometry>(Geometry.Parse);
    }
}