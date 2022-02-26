#region

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;

#endregion

namespace Onebeld.Logging
{
    public enum Log
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public class Logger
    {
        public Logger()
        {
            Logs = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Logs { get; set; }

        /// <summary>
        ///     Gets the current instance of the logger
        /// </summary>
        public static Logger Current { get; set; }

        public event EventHandler SavedLog;

        public event EventHandler Writed;

        public void WriteLog(Log log, string value)
        {
            string text = $"[{DateTime.Now:dd.MM.yyy HH:mm:ss.fff}] | {log} | " + value;

            Logs.Add(text);

            Writed?.Invoke(text, EventArgs.Empty);
        }

        public void WriteLog(Log log, string value, Assembly assembly)
        {
            string text = $"[{DateTime.Now:dd.MM.yyy HH:mm:ss.fff}] | [{assembly?.GetName()}] | {log} | " + value;

            Logs.Add($"[{DateTime.Now:dd.MM.yyy HH:mm:ss.fff}] | [{assembly?.GetName()}] | {log} | " + value);

            Writed?.Invoke(text, EventArgs.Empty);
        }

        public void SaveLog()
        {
            string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            string filename = $"{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now:dd.MM.yyy}.log";

            string path = Path.Combine(pathToLog, filename);

            new FileInfo(path).Directory.Create();

            File.AppendAllText(path, string.Join("\n", Logs), Encoding.UTF8);

            SavedLog?.Invoke(this, EventArgs.Empty);
        }

        public void SaveLog(string path)
        {
            File.AppendAllText(path, string.Join("\n", Logs), Encoding.UTF8);

            SavedLog?.Invoke(this, EventArgs.Empty);
        }
    }
}