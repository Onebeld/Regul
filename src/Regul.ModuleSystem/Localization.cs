using System.Collections.Generic;
using Avalonia.Styling;

namespace Regul.ModuleSystem;

public class Localization
{
    /// <summary>
    /// Necessary to dynamically change the language.
    /// </summary>
    public IStyle? LanguageStyle { get; set; }

    /// <summary>
    /// The languages supported by the module.
    /// </summary>
    public IReadOnlyList<Language>? SupportedLanguages { get; set; }

    /// <summary>
    /// The path in which all your languages are stored.
    /// </summary>
    public string? PathToLocalization { get; set; }
}