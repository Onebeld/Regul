using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PleasantUI.Windows;
using Regul.Helpers;
using Regul.Managers;
using VirusTotalNet;
using VirusTotalNet.Exceptions;
using VirusTotalNet.Objects;
using VirusTotalNet.ResponseCodes;
using VirusTotalNet.Results;

namespace Regul.Other;

public static class VirusScanner
{
    public static async Task<bool> Scan(string path)
    {
        if (WindowsManager.MainWindow is null || !WindowsManager.MainWindow.LoadingIsOpened())
            return false;
        
        FileInfo fileInfo = new(path);
        
        VirusTotal virusTotal = new(AesEncryption.DecryptString(ApplicationSettings.Current.Key, ApplicationSettings.Current.VirusTotalApiKey))
        {
            UseTLS = true
        };
        
        WindowsManager.MainWindow.ChangeLoadingProgress(0, $"{Path.GetFileName(path)}\n{App.GetString("GettingInformationM")}", true);

        FileReport fileReport;
        
        try
        {
            fileReport = await virusTotal.GetFileReportAsync(fileInfo);
        }
        catch (RateLimitException)
        {
            string result = await MessageBox.Show(
                WindowsManager.MainWindow,
                "CheckLimitExceeded",
                $"{App.GetString("FileName")}: {fileInfo.Name}\n\n{App.GetString("YouReallyWantToContinueInstallation")}",
                MessageBoxButtons.YesNo);

            return result == "Yes";
        }

        if (fileReport.ResponseCode == FileReportResponseCode.NotPresent)
        {
            WindowsManager.MainWindow.ChangeLoadingProgress(0, $"{Path.GetFileName(path)}\n{App.GetString("CheckForVirusesM")}", true);

            ScanResult scanResult;

            try
            {
                scanResult = await virusTotal.ScanFileAsync(fileInfo);
            }
            catch (RateLimitException)
            {
                string result = await MessageBox.Show(
                    WindowsManager.MainWindow,
                    "CheckLimitExceeded",
                    $"{App.GetString("FileName")}: {fileInfo.Name}\n\n{App.GetString("YouReallyWantToContinueInstallation")}",
                    MessageBoxButtons.YesNo);

                return result == "Yes";
            }

            if (scanResult.ResponseCode == ScanFileResponseCode.Queued)
            {
                do
                {
                    await Task.Delay(21000);

                    try
                    {
                        fileReport = await virusTotal.GetFileReportAsync(fileInfo);
                    }
                    catch (RateLimitException)
                    {
                        string result = await MessageBox.Show(
                            WindowsManager.MainWindow,
                            "CheckLimitExceeded",
                            $"{App.GetString("FileName")}: {fileInfo.Name}\n\n{App.GetString("YouReallyWantToContinueInstallation")}",
                            MessageBoxButtons.YesNo);

                        return result == "Yes";
                    }
                    
                } while (fileReport.ResponseCode != FileReportResponseCode.Present);
            }
            else
            {
                string result = await MessageBox.Show(
                    WindowsManager.MainWindow,
                    "FailedToCheckForViruses",
                    $"{App.GetString("FileName")}: {fileInfo.Name}\n\n{App.GetString("YouReallyWantToContinueInstallation")}",
                    MessageBoxButtons.YesNo);

                return result == "Yes";
            }
        }
        else if (fileReport.ResponseCode == FileReportResponseCode.Queued)
        {
            WindowsManager.MainWindow.ChangeLoadingProgress(0, $"{Path.GetFileName(path)}\n{App.GetString("CheckForVirusesM")}", true);
            
            do
            {
                await Task.Delay(21000);
                
                try
                {
                    fileReport = await virusTotal.GetFileReportAsync(fileInfo);
                }
                catch (RateLimitException)
                {
                    string result = await MessageBox.Show(
                        WindowsManager.MainWindow,
                        "CheckLimitExceeded",
                        $"{App.GetString("FileName")}: {fileInfo.Name}\n\n{App.GetString("YouReallyWantToContinueInstallation")}",
                        MessageBoxButtons.YesNo);

                    return result == "Yes";
                }
            } while (fileReport.ResponseCode != FileReportResponseCode.Present);
        }
        
        int countUsedAntivirus = fileReport.Scans.Count(pair => pair.Value.Detected);

        if (countUsedAntivirus > 0)
        {
            string scans = string.Empty;

            foreach (KeyValuePair<string, ScanEngine> scan in fileReport.Scans)
            {
                if (string.IsNullOrWhiteSpace(scan.Value.Result))
                    continue;

                scans += $"{scan.Key} - {scan.Value.Result}\n";
            }

            string result = await MessageBox.Show(
                WindowsManager.MainWindow,
                "ThreatDetected",
                $"{App.GetString("FileName")}: {fileInfo.Name}\n\n{App.GetString("YouReallyWantToContinueInstallation")}",
                MessageBoxButtons.YesNo,
                scans);

            return result == "Yes";
        }

        return true;
    }
}