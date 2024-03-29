﻿using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Regul.ModuleSystem.Structures;

namespace Regul.ModuleSystem;

public static class ModuleExtensions
{
    public static void ChangeLanguage(this Module module, string newLanguage, Styles modulesLanguage)
    {
        if (module.Instance.Localization?.SupportedLanguages is null || module.Instance.Localization?.SupportedLanguages?.Count < 1 ||
            string.IsNullOrWhiteSpace(module.Instance.Localization.PathToLocalization))
            return;

        Language? language = null;

        if (module.Instance.Localization.SupportedLanguages is { } languages)
        {
            language = languages.FirstOrDefault(x => x.Key == newLanguage);

            if (language.Value.Key is null)
                language = languages.First(x => x.Key == "en");
        }
        else return;

        module.Instance.Localization.LanguageStyle = new StyleInclude(new Uri("avares://Regul/App.axaml"))
        {
            Source = new Uri($"{module.Instance.Localization.PathToLocalization}/{language.Value.Key}.axaml")
        };

        if (!modulesLanguage.Contains(module.Instance.Localization.LanguageStyle))
            modulesLanguage.Add(module.Instance.Localization.LanguageStyle);
    }

    public static void RemoveStyleLanguage(this Module module, Styles modulesLanguage)
    {
        if (module.Instance.Localization?.LanguageStyle is null)
            return;

        modulesLanguage.Remove(module.Instance.Localization.LanguageStyle);
    }
}