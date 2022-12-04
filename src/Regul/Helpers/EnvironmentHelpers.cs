using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Regul.Logging;

namespace Regul.Helpers;

public static class EnvironmentHelpers
{
    public static async Task ShellExecAsync(string cmd, bool waitForExit = true)
    {
        string escapedArgs = cmd.Replace("\"", "\\\"");

        using Process? process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        );
        if (waitForExit && process is not null)
        {
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                Logger.Instance.WriteLog(LogType.Error, $"{nameof(ShellExecAsync)} command: {cmd} exited with exit code: {exitCode}, instead of 0.");
            }
        }
    }
}