#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Regul.ModuleSystem.Models;

#endregion

namespace Regul.ModuleSystem
{
    public static class ModuleManagerExtensions
    {
        public static void ChangeLanguage(this Module module, string newLanguage, Styles modulesLanguage)
        {
            if (module.Source.Languages == null ||
                module.Source.Languages.Length == 0 ||
                string.IsNullOrEmpty(module.Source.PathToLocalization))
                return;

            Language lang = Array.Find(module.Source.Languages, x => x.Key == newLanguage) ??
                            Array.Find(module.Source.Languages, x => x.Key == "en");

            module.Source.LanguageStyle = new StyleInclude(new Uri("resm:Styles?assembly=Regul"))
            {
                Source = new Uri($"avares://{module.Source.PathToLocalization}{lang.Key}.axaml")
            };

            if (modulesLanguage.Contains(module.Source.LanguageStyle) == false)
                modulesLanguage.Add(module.Source.LanguageStyle);
        }

        public static bool CheckUpdate(this Module module)
        {
            try
            {
                if (string.IsNullOrEmpty(module.Source.LinkForCheckUpdates))
                    return false;

                string file;

                using (WebClient webClient = new WebClient())
                {
                    file = webClient.DownloadString(
                        new Uri(module.Source.LinkForCheckUpdates));
                }

                List<string> lines = file.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                //foreach (string myString in lines)
                for (int i = 0; i < lines.Count; i++)
                {
                    string myString = lines[i];

                    if (lines.IndexOf(myString) == 0)
                        module.InfoForUpdate.NewVersion = Version.Parse(myString);
                    else if (lines.IndexOf(myString) == 1)
                        module.InfoForUpdate.MinRegulVersion = Version.Parse(myString);
                    else if (lines.IndexOf(myString) == 2)
                        module.InfoForUpdate.LinkForDownload = myString;
                }

                if (Version.Parse(module.Source.Version) < module.InfoForUpdate.NewVersion)
                {
                    module.Source.ThereIsAnUpdate = true;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}