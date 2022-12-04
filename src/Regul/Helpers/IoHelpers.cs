using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Regul.Helpers;

public static class IoHelpers
{
    public static async Task OpenBrowserAsync(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // If no associated application/json MimeType is found xdg-open opens return error
            // but it tries to open it anyway using the console editor (nano, vim, other..)
            await EnvironmentHelpers.ShellExecAsync($"xdg-open {url}", waitForExit: false).ConfigureAwait(false);
        }
        else
        {
            using Process? process = Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {url}" : "",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }
    }

    public static void OpenFileInFileExplorer(string dirPath)
    {
        using Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "explorer.exe"
                : (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? "open -R"
                    : "xdg-open file"),
            Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"/select, {dirPath}" : dirPath
        });
    }
}