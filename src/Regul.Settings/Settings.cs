using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Avalonia.Collections;
using Onebeld.Extensions;
using Regul.ModuleSystem;
using Regul.Settings;

namespace Regul;

[DataContract]
public class GeneralSettings : ViewModelBase
{
    private static readonly string SettingsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
    
    private bool _compactMode;

    private AvaloniaList<CorrespondingExtensionEditor> _correspondingExtensionEditors = new();

    private string _creatorName;
    private bool _hardwareAcceleration = true;
    private AvaloniaList<ModuleForUpdate> _modulesForUpdate = new();
    private AvaloniaList<Project> _projects;
    private bool _showCustomTitleBar = true;
    private bool _showFullscreenButton;

    public static GeneralSettings Instance;
    
    [DataMember] 
    public string Theme { get; set; } = "Light";

    [XmlAttribute]
    [DataMember]
    public string Language { get; set; } = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    [DataMember]
    public bool HardwareAcceleration
    {
        get => _hardwareAcceleration;
        set => RaiseAndSetIfChanged(ref _hardwareAcceleration, value);
    }

    [DataMember]
    public bool ShowFullscreenButton
    {
        get => _showFullscreenButton;
        set => RaiseAndSetIfChanged(ref _showFullscreenButton, value);
    }

    [DataMember]
    public bool CompactMode
    {
        get => _compactMode;
        set => RaiseAndSetIfChanged(ref _compactMode, value);
    }

    [DataMember]
    public bool ShowCustomTitleBar
    {
        get => _showCustomTitleBar;
        set => RaiseAndSetIfChanged(ref _showCustomTitleBar, value);
    }

    [XmlAttribute]
    [DataMember]
    public string CreatorName
    {
        get => _creatorName;
        set => RaiseAndSetIfChanged(ref _creatorName, value);
    }

    [DataMember]
    public AvaloniaList<Project> Projects
    {
        get => _projects;
        set => RaiseAndSetIfChanged(ref _projects, value);
    }

    [DataMember]
    public AvaloniaList<ModuleForUpdate> ModulesForUpdate
    {
        get => _modulesForUpdate;
        set => RaiseAndSetIfChanged(ref _modulesForUpdate, value);
    }

    [DataMember]
    public AvaloniaList<CorrespondingExtensionEditor> CorrespondingExtensionEditors
    {
        get => _correspondingExtensionEditors;
        set => RaiseAndSetIfChanged(ref _correspondingExtensionEditors, value);
    }

    public static GeneralSettings Load()
    {
        if (!Directory.Exists(SettingsFolder))
            Directory.CreateDirectory(SettingsFolder);

        try
        {
            using FileStream fileStream = File.OpenRead(Path.Combine(SettingsFolder, "general.xml"));
            return (GeneralSettings)new XmlSerializer(typeof(GeneralSettings)).Deserialize(fileStream);
        }
        catch
        {
            return new GeneralSettings();
        }
    }

    public static void Save()
    {
        using FileStream fileStream = File.Create(Path.Combine(SettingsFolder, "general.xml"));
        new XmlSerializer(typeof(GeneralSettings)).Serialize(fileStream, Instance);
    }
}