using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Win32;
using PleasantUI;
using Regul.Logging;
using Regul.Managers;
using Regul.Other;
using Regul.Structures;

namespace Regul;

public static class Program
{
    private static FileStream? _lockFile;

    public static string[] Arguments { get; private set; } = null!;

    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            _lockFile = File.Open(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".lock"),
                FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            _lockFile.Lock(0, 0);
        }
        catch
        {
            if (!Directory.Exists(RegulDirectories.Cache))
                Directory.CreateDirectory(RegulDirectories.Cache);
            if (!Directory.Exists(Path.Combine(RegulDirectories.Cache, "OpenFiles")))
                Directory.CreateDirectory(Path.Combine(RegulDirectories.Cache, "OpenFiles"));

            Guid guid = Guid.NewGuid();

            string newArgs = string.Join("|", args);

            File.WriteAllText(Path.Combine(RegulDirectories.Cache, "OpenFiles", guid + ".cache"), newArgs);

            EventWaitHandle eventWaitHandle = new(false, EventResetMode.AutoReset, "Onebeld-Regul-MemoryMap-dG17tr7Nv3_BytesWritten");
            eventWaitHandle.Set();

            return 0;
        }

        Arguments = args;

        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;

        ApplicationSettings.Load();

        if (string.IsNullOrWhiteSpace(ApplicationSettings.Current.VirusTotalApiKey) 
            || ApplicationSettings.Current.VirusTotalApiKey.Length < 64)
            ApplicationSettings.Current.ScanForVirus = false;

        AppBuilder mainAppBuilder = BuildAvaloniaApp();
        mainAppBuilder.SetupWithoutStarting();

        while (true)
        {
            try
            {
                ClassicDesktopStyleApplicationLifetime lifeTime = mainAppBuilder.CreateLifeTime();
                lifeTime.Start(args);
                lifeTime.Dispose();

                App.UnloadModules();
                break;
            }
            catch (Exception exception)
            {
                Logger.Instance.WriteLog(LogType.Error, $"[{exception.TargetSite?.DeclaringType}.{exception.TargetSite?.Name}()] {exception}\r\n");

                ApplicationSettings.Current.ExceptionCalled = true;
                ApplicationSettings.Current.ExceptionText = exception.ToString();

                foreach (Workbench workbench in WindowsManager.MainWindow?.ViewModel.Workbenches!)
                {
                    if (workbench.PathToFile is null || !workbench.IsDirty) continue;

                    try
                    {
                        workbench.EditorViewModel?.Save();
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.WriteLog(LogType.Error, $"[{e.TargetSite?.DeclaringType}.{e.TargetSite?.Name}()] {e}\r\n");
                    }
                }

                ClassicDesktopStyleApplicationLifetime currentLifeTime = (ClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!;
                currentLifeTime.Shutdown();
                currentLifeTime.Dispose();

                ApplicationSettings.Save();
                PleasantUiSettings.Save();

                ClassicDesktopStyleApplicationLifetime lifeTime = mainAppBuilder.CreateLifeTime();
                lifeTime.Start(args);

                lifeTime.Dispose();

                if (ApplicationSettings.Current.RestartingApp)
                {
                    ApplicationSettings.Current.ExceptionCalled = false;
                    ApplicationSettings.Current.RestartingApp = false;
                    ApplicationSettings.Current.ExceptionText = string.Empty;
                    continue;
                }

                App.UnloadModules();
                return 1;
            }
        }

        return 0;
    }
    private static void CurrentDomainOnProcessExit(object? sender, EventArgs e)
    {
        _lockFile?.Unlock(0, 0);
        _lockFile?.Dispose();
    }

    private static ClassicDesktopStyleApplicationLifetime CreateLifeTime(this AppBuilder appBuilder)
    {
        ClassicDesktopStyleApplicationLifetime lifeTime = new()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose
        };
        appBuilder.Instance!.ApplicationLifetime = lifeTime;
        appBuilder.Instance.OnFrameworkInitializationCompleted();

        return lifeTime;
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _lockFile?.Unlock(0, 0);
        _lockFile?.Dispose();

        string path = Logger.Instance.SaveLogs();

        Process.Start(new ProcessStartInfo
        {
            FileName = path, UseShellExecute = true
        });
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder appBuilder = AppBuilder.Configure<App>();
        return appBuilder.ConfigureAppBuilder();
    }

    public static AppBuilder ConfigureAppBuilder(this AppBuilder appBuilder)
    {
        appBuilder.UseSkia();

#if Windows
        appBuilder.UseWin32()
            .With(new AngleOptions
            {
                AllowedPlatformApis = new List<AngleOptions.PlatformApi>
                {
                    AngleOptions.PlatformApi.DirectX11
                }
            });
#else
            appBuilder.UsePlatformDetect();
#endif

        appBuilder
#if Windows
            .With(new Win32PlatformOptions
            {
                AllowEglInitialization = ApplicationSettings.Current.HardwareAcceleration,
                UseDeferredRendering = true,
                OverlayPopups = true,
                UseWgl = false,
                UseWindowsUIComposition = true,
                UseCompositor = true
            });
#elif OSX
            .With(new MacOSPlatformOptions
            {
                DisableDefaultApplicationMenuItems = true,
                ShowInDock = false
            });
#else
            .With(new AvaloniaNativePlatformOptions
			{
                UseDeferredRendering = true,
                UseGpu = ApplicationSettings.Current.HardwareAcceleration,
			});
#endif

#if DEBUG
        return appBuilder.LogToTrace();
#else
        return appBuilder;
#endif
    }
}