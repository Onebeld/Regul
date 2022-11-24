using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Regul.Logging;

public class Logger
{
    public static Logger Instance = new();
    
    public ObservableCollection<string> Logs { get; }

    public event EventHandler? SavedLogs;
    public event EventHandler? WrittenLog;

    public Logger()
    {
        Logs = new ObservableCollection<string>();
    }

    public void WriteLog(LogType logType, string value, Assembly? assembly = null)
    {
        string text;
        
        if (assembly is not null)
            text = $"[{DateTime.Now:dd.MM.yyy HH:mm:ss.fff}] | [{assembly.GetName()}] | {logType} | " + value;
        else
            text = $"[{DateTime.Now:dd.MM.yyy HH:mm:ss.fff}] | {logType} | " + value;

        Logs.Add(text);
        
        WrittenLog?.Invoke(text, EventArgs.Empty);
    }

    public string SaveLogs()
    {
        string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
        string filename = $"{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now:dd.MM.yyy}.log";

        string path = Path.Combine(pathToLog, filename);
        
        new FileInfo(path).Directory!.Create();
        
        string logs = string.Join("\n", Logs);
        AddMoreInformation(ref logs);
        
        File.AppendAllText(path, logs, Encoding.UTF8);
        
        SavedLogs?.Invoke(this, EventArgs.Empty);

        return path;
    }

    public void SaveLogsToFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;

        string logs = string.Join("\n", Logs);
        AddMoreInformation(ref logs);
        
        File.AppendAllText(path, logs, Encoding.UTF8);
        
        SavedLogs?.Invoke(this, EventArgs.Empty);
    }

    private void AddMoreInformation(ref string logs)
    {
        StringBuilder stringBuilder = new(logs);
        
        OperatingSystem os = Environment.OSVersion;
        stringBuilder.AppendLine("\nOperating System information:");
        stringBuilder.AppendLine("OS name: " + os.VersionString);
        stringBuilder.AppendLine("OS platform: " + os.Platform);
        stringBuilder.AppendLine("OS architecture: " + RuntimeInformation.OSArchitecture + "\n");
            
        CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
        stringBuilder.AppendLine("Default language information:");
        stringBuilder.AppendLine("Name: " + cultureInfo.Name);
        stringBuilder.AppendLine("Display name: " + cultureInfo.DisplayName);
        stringBuilder.AppendLine("English name: " + cultureInfo.EnglishName);
        stringBuilder.AppendLine("2-letter ISO name: " + cultureInfo.TwoLetterISOLanguageName);
        stringBuilder.AppendLine("3-letter ISO name: " + cultureInfo.ThreeLetterISOLanguageName + "\n");
            
        stringBuilder.AppendLine("Other information:");
        stringBuilder.AppendLine(".NET version: " + RuntimeInformation.FrameworkDescription);
        stringBuilder.AppendLine("Process architecture: " + RuntimeInformation.ProcessArchitecture + "\r\n");

        logs = stringBuilder.ToString();
    }
}