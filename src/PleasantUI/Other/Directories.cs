namespace PleasantUI.Other;

public static class Directories
{
    public static readonly string Settings = 
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");

    public static readonly string Themes =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
}