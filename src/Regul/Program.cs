#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.Rendering;
using Onebeld.Logging;
using PleasantUI;
using Regul.Base;
using Regul.Base.Other;
using Regul.Base.Views.Windows;

#endregion

namespace Regul
{
    public class Program
    {
        private static FileStream _lockFile;

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                _lockFile = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".lock"),
                    FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _lockFile.Lock(0, 0);
            }
            catch
            {
                if (!Directory.Exists(RegulPaths.Cache))
                    Directory.CreateDirectory(RegulPaths.Cache);
                if (!Directory.Exists(Path.Combine(RegulPaths.Cache, "OpenFiles")))
                    Directory.CreateDirectory(Path.Combine(RegulPaths.Cache, "OpenFiles"));

                Guid guid = Guid.NewGuid();

                string newArgs = string.Join("|", args);

                File.WriteAllText(Path.Combine(RegulPaths.Cache, "OpenFiles", guid + ".cache"), newArgs);

                EventWaitHandle bytesWritten =
                    new EventWaitHandle(false, EventResetMode.AutoReset,
                        "Onebeld-Regul-MemoryMap-dG17tr7Nv3_BytesWritten");
                bytesWritten.Set();

                return;
            }

            Logger.Current = new Logger();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            GeneralSettings.Settings = GeneralSettings.Load();
            PleasantSettings.Settings = PleasantSettings.Load();

            BuildAvaloniaApp().Start(AppMain, args);

            _lockFile.Unlock(0, 0);
            _lockFile.Dispose();
            File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".lock"));
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_lockFile != null)
            {
                _lockFile.Unlock(0, 0);
                _lockFile.Dispose();
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".lock"));
            }

            string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            if (!Directory.Exists(pathToLog)) Directory.CreateDirectory(pathToLog);

            if (e.ExceptionObject is Exception ex)
            {
                string filename = $"{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now:dd.MM.yyy}.log";

                Logger.Current.WriteLog(Log.Fatal,
                    $"[{ex.TargetSite?.DeclaringType}.{ex.TargetSite?.Name}()] {ex}\r\n");
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

            appBuilder.UseSkia();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                appBuilder
                    .UseWin32()
                    .With(new AngleOptions
                    {
                        AllowedPlatformApis = new List<AngleOptions.PlatformApi> { AngleOptions.PlatformApi.DirectX11 }
                    });

                if (DwmIsCompositionEnabled(out bool dwmEnabled) == 0 && dwmEnabled)
                {
                    Action wp = appBuilder.WindowingSubsystemInitializer;
                    appBuilder.UseWindowingSubsystem(() =>
                    {
                        wp();
                        AvaloniaLocator.CurrentMutable.Bind<IRenderTimer>().ToConstant(new WindowsDwmRenderTimer());
                    });
                }
            }
            else
            {
                appBuilder.UsePlatformDetect();
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
                .With(new MacOSPlatformOptions
                {
                    DisableDefaultApplicationMenuItems = true,
                    ShowInDock = false
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
            WindowsManager.MainWindow = new MainWindow(args);

            app.Run(WindowsManager.MainWindow);
        }

        [DllImport("Dwmapi.dll")]
        private static extern int DwmIsCompositionEnabled(out bool enabled);
    }
}