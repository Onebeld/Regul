﻿using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace Regul.Converters;

public class PathToFileNameConverter : IValueConverter
{
    private static readonly Lazy<PathToFileNameConverter> _lazy = new(() => new PathToFileNameConverter());
    public static PathToFileNameConverter Instance
    {
        get => _lazy.Value;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? path = value as string;

        return !string.IsNullOrWhiteSpace(path) ? Path.GetFileName(path) : App.GetString("NoName");
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}