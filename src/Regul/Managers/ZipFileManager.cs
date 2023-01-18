using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Regul.Enums;

namespace Regul.Managers;

public static class ZipFileManager
{
    public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, ZipFileOverwrite overwrite = ZipFileOverwrite.IfNewer)
    {
        using ZipArchive archive = ZipFile.OpenRead(sourceArchiveFileName);

        foreach (ZipArchiveEntry entry in archive.Entries)
            ExtractToFile(entry, destinationDirectoryName, overwrite);
    }

    public static List<string> ExtractToDirectoryWithPaths(string sourceArchiveFileName, string destinationDirectoryName, ZipFileOverwrite overwrite = ZipFileOverwrite.IfNewer)
    {
        List<string> list = new();

        using ZipArchive archive = ZipFile.OpenRead(sourceArchiveFileName);

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string? path = ExtractToFileWithPath(entry, destinationDirectoryName, overwrite);
            if (!string.IsNullOrWhiteSpace(path))
                list.Add(path);
        }

        return list;
    }

    public static void ExtractToFile(ZipArchiveEntry entry, string destinationPath, ZipFileOverwrite overwrite = ZipFileOverwrite.IfNewer)
    {
        //Gets the complete path for the destination file, including any
        //relative paths that were in the zip file
        string destinationFileName = Path.GetFullPath(Path.Combine(destinationPath, entry.FullName));
        string fullDestDirPath = Path.GetFullPath(destinationPath + Path.DirectorySeparatorChar);
        if (!destinationFileName.StartsWith(fullDestDirPath))
            throw new InvalidOperationException("Entry is outside the target dir: " + destinationFileName);

        //Gets just the new path, minus the file name so we can create the
        //directory if it does not exist
        string destinationFilePath = Path.GetDirectoryName(destinationFileName)!;

        //Creates the directory (if it doesn't exist) for the new path
        Directory.CreateDirectory(destinationFilePath);

        if (string.IsNullOrWhiteSpace(Path.GetFileName(destinationFileName))) return;

        //Determines what to do with the file based upon the
        //method of overwriting chosen
        switch (overwrite)
        {
            case ZipFileOverwrite.Always:
                //Just put the file in and overwrite anything that is found
                entry.ExtractToFile(destinationFileName, true);
                break;
            case ZipFileOverwrite.IfNewer:
                //Checks to see if the file exists, and if so, if it should
                //be overwritten
                if (!File.Exists(destinationFileName) ||
                    File.GetLastWriteTime(destinationFileName) < entry.LastWriteTime)
                    //Either the file didn't exist or this file is newer, so
                    //we will extract it and overwrite any existing file
                    entry.ExtractToFile(destinationFileName, true);
                break;
            case ZipFileOverwrite.Never:
                //Put the file in if it is new but ignores the 
                //file if it already exists
                if (!File.Exists(destinationFileName)) entry.ExtractToFile(destinationFileName);
                break;
        }
    }

    public static string? ExtractToFileWithPath(ZipArchiveEntry entry, string destinationPath, ZipFileOverwrite overwrite = ZipFileOverwrite.IfNewer)
    {
        //Gets the complete path for the destination file, including any
        //relative paths that were in the zip file
        string destinationFileName = Path.GetFullPath(Path.Combine(destinationPath, entry.FullName));
        string fullDestDirPath = Path.GetFullPath(destinationPath + Path.DirectorySeparatorChar);
        if (!destinationFileName.StartsWith(fullDestDirPath))
            throw new InvalidOperationException("Entry is outside the target dir: " + destinationFileName);

        //Gets just the new path, minus the file name so we can create the
        //directory if it does not exist
        string destinationFilePath = Path.GetDirectoryName(destinationFileName)!;

        //Creates the directory (if it doesn't exist) for the new path
        Directory.CreateDirectory(destinationFilePath);

        if (string.IsNullOrWhiteSpace(Path.GetFileName(destinationFileName))) return null;

        //Determines what to do with the file based upon the
        //method of overwriting chosen
        switch (overwrite)
        {
            case ZipFileOverwrite.Always:
                //Just put the file in and overwrite anything that is found
                entry.ExtractToFile(destinationFileName, true);
                return destinationFileName;
            case ZipFileOverwrite.IfNewer:
                //Checks to see if the file exists, and if so, if it should
                //be overwritten
                if (!File.Exists(destinationFileName) ||
                    File.GetLastWriteTime(destinationFileName) < entry.LastWriteTime)
                {
                    //Either the file didn't exist or this file is newer, so
                    //we will extract it and overwrite any existing file
                    entry.ExtractToFile(destinationFileName, true);
                    return destinationFileName;
                }

                break;
            case ZipFileOverwrite.Never:
                //Put the file in if it is new but ignores the 
                //file if it already exists
                if (!File.Exists(destinationFileName))
                {
                    entry.ExtractToFile(destinationFileName);
                    return destinationFileName;
                }

                break;
        }

        return null;
    }
}