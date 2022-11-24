namespace Regul.ModuleSystem.Internal;

internal class RuntimeOptions
{
    public string Tfm { get; set; } = string.Empty;

    public string[] AdditionalProbingPaths { get; set; } = null!;
}