using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Regul.Base.Other
{
    public enum Overwrite
    {
        IfNewer,
        Always,
        Never
    }
    
    public static class Extensions
    {
        /// <summary>
        /// Gets an <typeparamref name="T"/> with the given binding
        /// </summary>
        public static T Bind<T>(this IAvaloniaObject control, AvaloniaProperty property, IBinding binding, object anchor = null)
		{
            control.Bind(property, binding, anchor);
            return (T)control;
		}
        /// <summary>
        /// Gets an <typeparamref name="T"/> with the given converter
        /// </summary>
        public static Binding Converter(this Binding binding, IValueConverter converter)
        {
            binding.Converter = converter;
            return binding;
        }
        
        public static void ImprovedExtractToDirectory(string sourceArchiveFileName, 
            string destinationDirectoryName, 
            Overwrite overwriteMethod = Overwrite.IfNewer)
        {
            //Opens the zip file up to be read
            using (ZipArchive archive = ZipFile.OpenRead(sourceArchiveFileName))
            {
                //Loops through each file in the zip file
                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    ImprovedExtractToFile(file, destinationDirectoryName, overwriteMethod);
                }
            }
        }

        public static List<string> ImprovedExtractToDirectoryWithList(string sourceArchiveFileName,
            string destinationDirectoryName,
            Overwrite overwriteMethod = Overwrite.IfNewer)
        {
            List<string> list = new List<string>();
            
            using (ZipArchive archive = ZipFile.OpenRead(sourceArchiveFileName))
            {
                //Loops through each file in the zip file
                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    string path = ImprovedExtractToFileWithPath(file, destinationDirectoryName, overwriteMethod);
                    if (!string.IsNullOrEmpty(path))
                        list.Add(path);
                }
            }

            return list;
        }

        public static void ImprovedExtractToFile(ZipArchiveEntry file, 
            string destinationPath, 
            Overwrite overwriteMethod = Overwrite.IfNewer)
        {
            //Gets the complete path for the destination file, including any
            //relative paths that were in the zip file
            string destinationFileName = Path.Combine(destinationPath, file.FullName);
 
            //Gets just the new path, minus the file name so we can create the
            //directory if it does not exist
            string destinationFilePath = Path.GetDirectoryName(destinationFileName);
 
            //Creates the directory (if it doesn't exist) for the new path
            Directory.CreateDirectory(destinationFilePath);
 
            //Determines what to do with the file based upon the
            //method of overwriting chosen
            switch (overwriteMethod)
            {
                case Overwrite.Always:
                    //Just put the file in and overwrite anything that is found
                    file.ExtractToFile(destinationFileName, true);
                    break;
                case Overwrite.IfNewer:
                    //Checks to see if the file exists, and if so, if it should
                    //be overwritten
                    if (!File.Exists(destinationFileName) || File.GetLastWriteTime(destinationFileName) < file.LastWriteTime)
                    {
                        //Either the file didn't exist or this file is newer, so
                        //we will extract it and overwrite any existing file
                        file.ExtractToFile(destinationFileName, true);
                    }
                    break;
                case Overwrite.Never:
                    //Put the file in if it is new but ignores the 
                    //file if it already exists
                    if (!File.Exists(destinationFileName))
                    {
                        file.ExtractToFile(destinationFileName);
                    }
                    break;
            }
        }
        
        public static string ImprovedExtractToFileWithPath(ZipArchiveEntry file, 
            string destinationPath, 
            Overwrite overwriteMethod = Overwrite.IfNewer)
        {
            //Gets the complete path for the destination file, including any
            //relative paths that were in the zip file
            string destinationFileName = Path.Combine(destinationPath, file.FullName);
 
            //Gets just the new path, minus the file name so we can create the
            //directory if it does not exist
            string destinationFilePath = Path.GetDirectoryName(destinationFileName);
 
            //Creates the directory (if it doesn't exist) for the new path
            Directory.CreateDirectory(destinationFilePath);
 
            //Determines what to do with the file based upon the
            //method of overwriting chosen
            switch (overwriteMethod)
            {
                case Overwrite.Always:
                    //Just put the file in and overwrite anything that is found
                    file.ExtractToFile(destinationFileName, true);
                    return destinationFileName;
                case Overwrite.IfNewer:
                    //Checks to see if the file exists, and if so, if it should
                    //be overwritten
                    if (!File.Exists(destinationFileName) || File.GetLastWriteTime(destinationFileName) < file.LastWriteTime)
                    {
                        //Either the file didn't exist or this file is newer, so
                        //we will extract it and overwrite any existing file
                        file.ExtractToFile(destinationFileName, true);
                        return destinationFileName;
                    }
                    break;
                case Overwrite.Never:
                    //Put the file in if it is new but ignores the 
                    //file if it already exists
                    if (!File.Exists(destinationFileName))
                    {
                        file.ExtractToFile(destinationFileName);
                        return destinationFileName;
                    }
                    break;
            }

            return null;
        }
    }
}