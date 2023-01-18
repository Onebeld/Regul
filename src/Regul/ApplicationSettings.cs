using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using PleasantUI;
using PleasantUI.Other;
using Regul.Enums;
using Regul.Managers;
using Regul.Structures;

namespace Regul;

[DataContract]
public class ApplicationSettings : ViewModelBase
{
    public static ApplicationSettings Current = new();

    private string _language = null!;
    private string _creatorName = string.Empty;
    private string _virusTotalApiKey = string.Empty;
    private string _key = Guid.NewGuid().ToString("N");
    private bool _hardwareAcceleration = true;
    private bool _scanForVirus;

    private CheckUpdateInterval _checkUpdateInterval = CheckUpdateInterval.EveryWeek;

    private AvaloniaList<Project> _projects = new();
    private AvaloniaList<UpdatableModule> _updatableModules = new();
    private AvaloniaList<EditorRelatedExtension> _editorRelatedExtensions = new();

    [DataMember]
    public string Key
    {
        get => _key;
        set => RaiseAndSetIfChanged(ref _key, value);
    }

    [DataMember]
    public string Language
    {
        get => _language;
        set => RaiseAndSetIfChanged(ref _language, value);
    }

    [DataMember]
    public bool HardwareAcceleration
    {
        get => _hardwareAcceleration;
        set => RaiseAndSetIfChanged(ref _hardwareAcceleration, value);
    }

    [DataMember]
    public AvaloniaList<Project> Projects
    {
        get => _projects;
        set => RaiseAndSetIfChanged(ref _projects, value);
    }

    [DataMember]
    public AvaloniaList<UpdatableModule> UpdatableModules
    {
        get => _updatableModules;
        set => RaiseAndSetIfChanged(ref _updatableModules, value);
    }

    [DataMember]
    public AvaloniaList<EditorRelatedExtension> EditorRelatedExtensions
    {
        get => _editorRelatedExtensions;
        set => RaiseAndSetIfChanged(ref _editorRelatedExtensions, value);
    }

    [DataMember]
    public string CreatorName
    {
        get => _creatorName;
        set => RaiseAndSetIfChanged(ref _creatorName, value);
    }

    [DataMember]
    public string VirusTotalApiKey
    {
        get => _virusTotalApiKey;
        set => RaiseAndSetIfChanged(ref _virusTotalApiKey, value);
    }

    [DataMember]
    public CheckUpdateInterval CheckUpdateInterval
    {
        get => _checkUpdateInterval;
        set => RaiseAndSetIfChanged(ref _checkUpdateInterval, value);
    }
    
    [DataMember]
    public bool ScanForVirus
    {
        get => _scanForVirus;
        set => RaiseAndSetIfChanged(ref _scanForVirus, value);
    }

    [DataMember]
    public string? DateOfLastUpdateCheck { get; set; }

    [JsonIgnore]
    internal bool ExceptionCalled { get; set; }

    [JsonIgnore]
    internal bool RestartingApp { get; set; }

    [JsonIgnore]
    internal string ExceptionText { get; set; } = string.Empty;

    public ApplicationSettings() => Setup();

    private void Setup() => Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    public static void Load()
    {
        if (!Directory.Exists(Directories.Settings))
            Directory.CreateDirectory(Directories.Settings);

        string appSettings = Path.Combine(Directories.Settings, "settings.json");
        if (!File.Exists(appSettings)) return;

        using FileStream fileStream = File.OpenRead(appSettings);
        try
        {
            Current = JsonSerializer.Deserialize<ApplicationSettings>(fileStream)!;
        }
        catch
        {
            // ignored
        }
    }

    public static void Save()
    {
        using FileStream fileStream = File.Create(Path.Combine(Directories.Settings, "settings.json"));
        JsonSerializer.Serialize(fileStream, Current);
    }

    public static void Reset()
    {
        Current.Setup();

        Language? language = App.Languages.FirstOrDefault(x =>
            x.Key == Current.Language ||
            x.AdditionalKeys.Any(lang => lang == Current.Language));

        string? key = language.Value.Key;

        if (string.IsNullOrWhiteSpace(key))
            key = "en";

        Current.Language = key;

        Current.HardwareAcceleration = true;
    }
}