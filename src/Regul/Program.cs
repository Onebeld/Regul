using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.OpenGL;
using Onebeld.Logging;
using PleasantUI;
using PleasantUI.Controls.Custom;
using Regul.Base;
using Regul.Base.Views.Windows;

namespace Regul
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Logger.Current = new Logger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            GeneralSettings.Settings = GeneralSettings.Load();
            PleasantSettings.Settings = PleasantSettings.Load();

            BuildAvaloniaApp().Start(AppMain, args);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            if (!Directory.Exists(pathToLog)) Directory.CreateDirectory(pathToLog);

            if (e.ExceptionObject is Exception ex)
            {
                string filename = $"{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now:dd.MM.yyy}.log";

                Logger.Current.WriteLog(Log.Fatal, $"[{ex.TargetSite?.DeclaringType}.{ex.TargetSite?.Name}()] {ex}\r\n");
                Logger.Current.SaveLog(Path.Combine(pathToLog, filename));

                Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(pathToLog, filename),
                    UseShellExecute = true
                });
            }
        }

        private static AppBuilder BuildAvaloniaApp()
        {
            AppBuilder appBuilder = AppBuilder.Configure<App>();

            // Checking a running OS. This is required in the future for cross-platform functionality.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                appBuilder
                    .UseWin32()
                    .UseSkia()
                    .With(new AngleOptions
                    {
                        AllowedPlatformApis = new List<AngleOptions.PlatformApi>
                        {
                            AngleOptions.PlatformApi.DirectX11
                        }
                    });
            }
            else
            {
                appBuilder.UsePlatformDetect()
                    .UseManagedSystemDialogs<AppBuilder, PleasantWindow>();
            }

            return appBuilder
                .LogToTrace()
                .With(new Win32PlatformOptions
                {
                    AllowEglInitialization = GeneralSettings.Settings.HardwareAcceleration,
                    UseDeferredRendering = true,
                    OverlayPopups = false,
                    UseWgl = false
                })
                .With(new AvaloniaNativePlatformOptions
                {
                    UseGpu = GeneralSettings.Settings.HardwareAcceleration,
                    UseDeferredRendering = true,
                    OverlayPopups = false
                });
        }

        private static void AppMain(Application app, string[] args)
        {
            WindowsManager.MainWindow = new MainWindow();

			app.Run(WindowsManager.MainWindow);
        }
    }
}