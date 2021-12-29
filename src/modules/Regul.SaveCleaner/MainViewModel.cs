using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Collections;
using Avalonia.Controls;
using Onebeld.Extensions;
using PleasantUI.Structures;
using PleasantUI.Windows;
using Regul.Base;
using Regul.Base.Views.Windows;
using Regul.S3PI.Interfaces;
using Regul.S3PI.Package;
using Regul.SaveCleaner.Controls;
using Regul.SaveCleaner.Structures;

namespace Regul.SaveCleaner
{
    public class MainViewModel : ViewModelBase
    {
        private string _pathBackup;
        private bool _isLoading;
        
        private AvaloniaList<SaveFilePortrait> _saveFilePortraits = new AvaloniaList<SaveFilePortrait>();
        private AvaloniaList<SaveFilePortrait> _selectSaves = new AvaloniaList<SaveFilePortrait>();

        private List<string> ClearFiles = new List<string>();

        private Loading Loading;

        [AccessedThroughProperty("bgwClean")] private BackgroundWorker _bgwClean;
        
        #region Propertys

        private string PathBackup
        {
            get => _pathBackup;
            set => RaiseAndSetIfChanged(ref _pathBackup, value);
        }

        private bool IsLoading
        {
            get => _isLoading;
            set => RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private AvaloniaList<SaveFilePortrait> SaveFilePortraits
        {
            get => _saveFilePortraits;
            set => RaiseAndSetIfChanged(ref _saveFilePortraits, value);
        }

        private AvaloniaList<SaveFilePortrait> SelectSaves
        {
            get => _selectSaves;
            set => RaiseAndSetIfChanged(ref _selectSaves, value);
        }

        #endregion
        
        public MainViewModel()
        {
            bgwClean = new BackgroundWorker();
        }
        
        private void ChooseAll()
        {
            SaveCleanerSettings.Settings.DeletingCharacterPortraits = true;
            SaveCleanerSettings.Settings.RemovingLotThumbnails = true;
            SaveCleanerSettings.Settings.RemovingPhotos = true;
            SaveCleanerSettings.Settings.RemovingFamilyPortraits = true;
            SaveCleanerSettings.Settings.RemovingGeneratedImages = true;
        }

        private void CancelAll()
        {
            SaveCleanerSettings.Settings.DeletingCharacterPortraits = false;
            SaveCleanerSettings.Settings.RemovingLotThumbnails = false;
            SaveCleanerSettings.Settings.RemovingPhotos = false;
            SaveCleanerSettings.Settings.RemovingTextures = false;
            SaveCleanerSettings.Settings.RemovingFamilyPortraits = false;
            SaveCleanerSettings.Settings.RemovingGeneratedImages = false;
            SaveCleanerSettings.Settings.RemoveOtherTypes = false;
        }

        private async void OpenSettingsWindow()
        {
            Views.SettingsWindow foundWindow = WindowsManager.FindModalWindow<Views.SettingsWindow>();
            
            if (foundWindow != null && foundWindow.CanOpen)
                return;

            Views.SettingsWindow window = new Views.SettingsWindow();
            WindowsManager.OtherModalWindows.Add(window);
            await window.Show(WindowsManager.MainWindow);
            
            bgwClean = new BackgroundWorker();
        }
        
        private void CloseWindow()
        {
            MainModalWindow foundWindow = WindowsManager.FindModalWindow<MainModalWindow>();
            
            foundWindow?.Close();

            WindowsManager.OtherModalWindows.Remove(foundWindow);
        }
        
        public async void LoadingSaves()
        {
            if (IsLoading) return;

            List<string> saves = new List<string>();

            for (int i = 0; i < SelectSaves.Count; i++) saves.Add(SelectSaves[i].SaveName.Text);

            SelectSaves.Clear();
            SaveFilePortraits.Clear();

            if (!Directory.Exists(SaveCleanerSettings.Settings.PathToTheSims3Document))
            {
                await MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Information"),
                    App.GetResource<string>("NotFindFolderTheSims3"),
                    new List<MessageBoxButton>
                    {
                        new MessageBoxButton
                        {
                            IsKeyDown = true,
                            Default = true,
                            Result = "OK",
                            Text = App.GetResource<string>("OK")
                        }
                    }, MessageBox.MessageBoxIcon.Information);

                OpenFolderDialog dialog = new OpenFolderDialog
                {
                    Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };
                string path = await dialog.ShowAsync(WindowsManager.MainWindow);
                if (!string.IsNullOrEmpty(path))
                {
                    SaveCleanerSettings.Settings.PathToTheSims3Document = path;

                    SaveCleanerSettings.Settings.PathToSaves = Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "Saves");

                    LoadingSaves();
                }

                return;
            }

            try
            {
                if (!Directory.Exists(SaveCleanerSettings.Settings.PathToSaves))
                {
                    await MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Information"),
                        App.GetResource<string>("SaveFilesNotFound"),
                        new List<MessageBoxButton>
                        {
                            new MessageBoxButton
                            {
                                IsKeyDown = true,
                                Default = true,
                                Result = "OK",
                                Text = App.GetResource<string>("OK")
                            }
                        }, MessageBox.MessageBoxIcon.Information);

                    OpenFolderDialog dialog = new OpenFolderDialog
                    {
                        Directory = SaveCleanerSettings.Settings.PathToTheSims3Document
                    };
                    string path = await dialog.ShowAsync(WindowsManager.MainWindow);
                    if (!string.IsNullOrEmpty(path))
                    {
                        SaveCleanerSettings.Settings.PathToSaves = path;

                        LoadingSaves();
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Error"),
                    App.GetResource<string>("AnErrorHasOccurred"),
                    new List<MessageBoxButton>
                    {
                        new MessageBoxButton
                        {
                            IsKeyDown = true, 
                            Default = true,
                            Result = "OK", 
                            Text = App.GetResource<string>("OK")
                        }
                    }, MessageBox.MessageBoxIcon.Error, ex.ToString());

                return;
            }

            foreach (string directory in Directory.EnumerateDirectories(SaveCleanerSettings.Settings.PathToSaves, "*.sims3",
                SearchOption.TopDirectoryOnly))
            {
                try
                {
                    foreach (string file in Directory.EnumerateFiles(directory, "*.nhd", SearchOption.TopDirectoryOnly))
                    {
                        if (SaveFilePortraits.FirstOrDefault(x => x.SaveDir == directory) != null)
                            continue;

                        Save save = new Save(file);

                        SaveFilePortrait saveFilePortrait = new SaveFilePortrait(directory, save);

                        if (file.IndexOf(saveFilePortrait.Location, StringComparison.Ordinal) != -1)
                        {
                            saveFilePortrait.IconFamily.Source = save.FamilyIcon;
                            saveFilePortrait.SaveFamily.Text = save.WorldName;
                            saveFilePortrait.ImgInstance = save.ImgInstance;
                            SaveFilePortraits.Add(saveFilePortrait);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Error"),
                        App.GetResource<string>("AnErrorHasOccurred"),
                        new List<MessageBoxButton>
                        {
                            new MessageBoxButton
                            {
                                IsKeyDown = true, 
                                Default = true,
                                Result = "OK",
                                Text = App.GetResource<string>("OK")
                            }
                        }, MessageBox.MessageBoxIcon.Error, ex.ToString());
                }
            }

            if (SaveFilePortraits.Count == 0)
            {
                await MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Information"),
                    App.GetResource<string>("SaveFilesNotFound"),
                    new List<MessageBoxButton>
                    {
                        new MessageBoxButton
                        {
                            IsKeyDown = true, 
                            Default = true,
                            Result = "OK",
                            Text = App.GetResource<string>("OK")
                        }
                    }, MessageBox.MessageBoxIcon.Information);
            }

            foreach (string item in saves)
                SelectSaves.Add(SaveFilePortraits.FirstOrDefault(x => x.SaveName.Text == item));
        }
        
        private async void SelectPath() => PathBackup = await new OpenFolderDialog().ShowAsync(WindowsManager.MainWindow) + "\\";
        
        private async void Clear()
        {
            try
            {
                if (SelectSaves.Count == 0)
                {
                    await MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Error"),
                        App.GetResource<string>("NoSelectedSaveFile"),
                        new List<MessageBoxButton>
                        {
                            new MessageBoxButton
                            {
                                IsKeyDown = true, 
                                Default = true,
                                Result = "OK",
                                Text = App.GetResource<string>("OK")
                            }
                        }, MessageBox.MessageBoxIcon.Error);
                    return;
                }

                WindowsManager.MainWindow.CancelClose = true;
                Loading = new Loading();
                Loading.Show(WindowsManager.MainWindow);
                
                bgwClean.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                await MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Error"),
                    App.GetResource<string>("AnErrorHasOccurred"),
                    new List<MessageBoxButton>
                    {
                        new MessageBoxButton
                        {
                            IsKeyDown = true, 
                            Default = true,
                            Result = "OK",
                            Text = App.GetResource<string>("OK")
                        }
                    }, MessageBox.MessageBoxIcon.Error, ex.ToString());
            }
        }
        
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            for (int index = 0; index < files.Length; index++)
            {
                FileInfo file = files[index];
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    DirectoryInfo subdir = dirs[i];
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, true);
                }
            }
        }
        
        internal BackgroundWorker bgwClean
        {
            get => _bgwClean;
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                ProgressChangedEventHandler changedEventHandler = bgwClean_ProgressChanged;
                RunWorkerCompletedEventHandler completedEventHandler = bgwClean_RunWorkerCompleted;
                DoWorkEventHandler workEventHandler = bgwClean_DoWork;

                if (_bgwClean != null)
                {
                    _bgwClean.ProgressChanged -= changedEventHandler;
                    _bgwClean.RunWorkerCompleted -= completedEventHandler;
                    _bgwClean.DoWork -= workEventHandler;
                }

                _bgwClean = value;

                if (bgwClean == null) return;

                _bgwClean.ProgressChanged += changedEventHandler;
                _bgwClean.RunWorkerCompleted += completedEventHandler;
                _bgwClean.DoWork += workEventHandler;

                _bgwClean.WorkerReportsProgress = true;
            }
        }
        
        private void bgwClean_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Loading.ProgressBar.Value = e.ProgressPercentage;
            Loading.TextBlock.Text = (string)e.UserState;
        }

        private void bgwClean_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WindowsManager.MainWindow.CancelClose = false;
            Loading.Close();
            GC.Collect();

            List<CleanerResult> results = (List<CleanerResult>) e.Result;
            string s = "";

            for (int index = 0; index < results.Count; index++)
            {
                CleanerResult cleanerResult = results[index];
                s +=
                    $"{App.GetResource<string>("SaveNameC")} {cleanerResult.Save}" +
                    $"\n{App.GetResource<string>("TimePassedC")} {cleanerResult.TotalSecond} {App.GetResource<string>("Sec")}" +
                    $"\n\n{App.GetResource<string>("OldSizeC")} {cleanerResult.OldSize} MB" +
                    $"\n{App.GetResource<string>("NewSizeC")} {cleanerResult.NewSize} MB" +
                    $"\n{App.GetResource<string>("PercentC")} {(cleanerResult.OldSize - cleanerResult.NewSize) / cleanerResult.OldSize:P}";

                if (index + 1 < results.Count)
                    s += "\n-----------------------\n";
            }

            if (ClearFiles.Count > 0)
            {
                s += "\n-----------------------\n";
                s += $"{App.GetResource<string>("DeletedFilesC")}\n";
                for (int i = 0; i < ClearFiles.Count; i++) s += ClearFiles[i] + "\n";
            }

            if (WindowsManager.MainWindow.WindowState == WindowState.Minimized) 
                WindowsManager.MainWindow.WindowState = WindowState.Normal;

            MessageBox.Show(WindowsManager.MainWindow, App.GetResource<string>("Successfully"),
                App.GetResource<string>("SaveFilesCleanedSuccessfully"), new List<MessageBoxButton>
                {
                    new MessageBoxButton
                    {
                        IsKeyDown = true,
                        Default = true,
                        Result = "OK", 
                        Text = App.GetResource<string>("OK")
                    }
                }, MessageBox.MessageBoxIcon.Information, s);
        }

        private void bgwClean_DoWork(object sender, DoWorkEventArgs e)
        {
            List<CleanerResult> cleanerResults = new List<CleanerResult>();
            for (int index1 = 0; index1 < SelectSaves.Count; index1++)
            {
                SaveFilePortrait selectSave = SelectSaves[index1];

                long num1 = 0;
                foreach (string file in Directory.EnumerateFiles(selectSave.SaveDir, "*.*", SearchOption.AllDirectories))
                    num1 += new FileInfo(file).Length;

                Stopwatch w = new Stopwatch();
                w.Start();

                switch (SaveCleanerSettings.Settings.CreateABackup)
                {
                    case true:
                    {
                        bgwClean.ReportProgress(20, $"{selectSave.SaveName.Text}\n" + App.GetResource<string>("ProcessingCreateBackup"));
                        switch (string.IsNullOrEmpty(PathBackup))
                        {
                            case false:
                                DirectoryCopy(selectSave.SaveDir, PathBackup + selectSave.SaveName.Text + ".sims3", true);
                                break;
                        }
                        break;
                    }
                }

                bgwClean.ReportProgress(40, $"{selectSave.SaveName.Text}\n" + App.GetResource<string>("ProcessingCompressingSave"));
                foreach (string file in Directory.EnumerateFiles(selectSave.SaveDir, "*.package", SearchOption.AllDirectories))
                {
                    if (file == Path.Combine(selectSave.SaveDir, "TravelDB.package")) continue;

                    IPackage pkg = Package.OpenPackage(file, true);

                    foreach (IResourceIndexEntry getResource in pkg.GetResourceList)
                    {
                        switch (SaveCleanerSettings.Settings.RemoveOtherTypes)
                        {
                            case true when getResource.Memsize == 174904U:
                                pkg.DeleteResource(getResource);
                                continue;
                        }

                        if (getResource.Filesize != getResource.Memsize && getResource.Compressed == 0)
                            getResource.Compressed = ushort.MaxValue;
                    }

                    pkg.SavePackage();
                    Package.ClosePackage(pkg);
                }

                bgwClean.ReportProgress(70, $"{selectSave.SaveName.Text}\n" + App.GetResource<string>("ProcessingClearingSave"));

                if (File.Exists(Path.Combine(selectSave.SaveDir, "TravelDB.package")) && SaveCleanerSettings.Settings.RemovingGeneratedImages || SaveCleanerSettings.Settings.RemovingPhotos)
                {
                    IPackage pkg1 = Package.OpenPackage(Path.Combine(selectSave.SaveDir, "TravelDB.package"), true);

                    foreach (IResourceIndexEntry getResource in pkg1.GetResourceList)
                    {
                        switch (SaveCleanerSettings.Settings.RemoveOtherTypes)
                        {
                            case true when getResource.Memsize == 174904U:
                                pkg1.DeleteResource(getResource);
                                continue;
                        }

                        if (SaveCleanerSettings.Settings.RemovingGeneratedImages && getResource.ResourceType == 11720834U &&
                            (getResource.ResourceGroup == 38510538U) | (getResource.ResourceGroup == 41034393U) |
                            (getResource.ResourceGroup == 45967776U) ||
                            SaveCleanerSettings.Settings.RemovingPhotos && getResource.ResourceType == 11720834U &&
                            getResource.ResourceGroup == 40488965U)
                        {
                            pkg1.DeleteResource(getResource);
                            continue;
                        }

                        if (getResource.Filesize != getResource.Memsize && getResource.Compressed == 0)
                            getResource.Compressed = ushort.MaxValue;
                    }

                    pkg1.SavePackage();
                    Package.ClosePackage(pkg1);
                }

                foreach (string file in Directory.EnumerateFiles(selectSave.SaveDir, "*.nhd", SearchOption.AllDirectories))
                {
                    IPackage pkg2 = Package.OpenPackage(file, true);

                    foreach (IResourceIndexEntry getResource in pkg2.GetResourceList)
                    {
                        switch (SaveCleanerSettings.Settings.RemoveOtherTypes)
                        {
                            case true when getResource.Memsize == 174904U:
                                pkg2.DeleteResource(getResource);
                                continue;
                        }

                        switch (SaveCleanerSettings.Settings.RemovingFamilyPortraits)
                        {
                            case true when getResource.Instance != selectSave.ImgInstance && getResource.ResourceType == 1802339198U:
                                pkg2.DeleteResource(getResource);
                                continue;
                        }

                        switch (SaveCleanerSettings.Settings.RemovingGeneratedImages)
                        {
                            case true when getResource.ResourceType == 11720834U && (getResource.ResourceGroup == 38510538U) | (getResource.ResourceGroup == 41034393U) |
                                (getResource.ResourceGroup == 45967776U):
                                pkg2.DeleteResource(getResource);
                                continue;
                        }

                        switch (SaveCleanerSettings.Settings.RemovingPhotos)
                        {
                            case true when getResource.ResourceType == 11720834U && getResource.ResourceGroup == 40488965U:
                                pkg2.DeleteResource(getResource);
                                continue;
                        }

                        switch (SaveCleanerSettings.Settings.RemovingTextures)
                        {
                            case true when getResource.ResourceType == 11720834U && (getResource.ResourceGroup == 11584775U) | (getResource.ResourceGroup == 1U) |
                                (getResource.ResourceGroup == 12328524U) | (getResource.ResourceGroup == 1943529U) |
                                (getResource.ResourceGroup == 16441714U) | (getResource.ResourceGroup == 12328532U) |
                                (getResource.ResourceGroup == 8287573U):
                                pkg2.DeleteResource(getResource);
                                continue;
                        }

                        switch (SaveCleanerSettings.Settings.RemovingLotThumbnails)
                        {
                            case true when getResource.ResourceType == 3629023174U:
                                pkg2.DeleteResource(getResource);
                                continue;
                        }

                        switch (SaveCleanerSettings.Settings.DeletingCharacterPortraits)
                        {
                            case true when (getResource.ResourceType == 92316365U) |
                                           (getResource.ResourceType == 92316366U) | (getResource.ResourceType == 92316367U):
                                pkg2.DeleteResource(getResource);
                                continue;
                        }

                        if (getResource.Filesize != getResource.Memsize && getResource.Compressed == 0)
                            getResource.Compressed = ushort.MaxValue;
                    }

                    pkg2.SavePackage();
                    Package.ClosePackage(pkg2);
                }

                w.Stop();

                long num2 = 0;
                foreach (string file in Directory.EnumerateFiles(selectSave.SaveDir, "*.*", SearchOption.AllDirectories))
                    num2 += new FileInfo(file).Length;

                cleanerResults.Add(new CleanerResult(num1 * 0.0009765625f * 0.0009765625f,
                    num2 * 0.0009765625f * 0.0009765625f, w.Elapsed.TotalSeconds, selectSave.SaveName.Text));
            }


            ClearFiles.Clear();
            switch (SaveCleanerSettings.Settings.ClearCache)
            {
                case true:
                {
                    try
                    {
                        bgwClean.ReportProgress(85, App.GetResource<string>("ProcessingClearingCache"));

                        if (File.Exists(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "CASPartCache.package")))
                            ClearFiles.Add(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "CASPartCache.package"));

                        if (File.Exists(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "compositorCache.package")))
                            ClearFiles.Add(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document,
                                "compositorCache.package"));

                        if (File.Exists(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "scriptCache.package")))
                            ClearFiles.Add(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "scriptCache.package"));

                        if (File.Exists(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document,
                            "simCompositorCache.package")))
                            ClearFiles.Add(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document,
                                "simCompositorCache.package"));

                        if (File.Exists(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "socialCache.package")))
                            ClearFiles.Add(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "socialCache.package"));

                        foreach (string file in Directory.EnumerateFiles(
                            Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "WorldCaches"), "*"))
                            ClearFiles.Add(file);
                        foreach (string file in Directory.EnumerateFiles(
                            Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "IGACache"), "*"))
                            ClearFiles.Add(file);
                        foreach (string file in Directory.EnumerateFiles(
                            Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "Thumbnails"), "*.package"))
                            ClearFiles.Add(file);
                        try
                        {
                            foreach (string file in Directory.EnumerateFiles(
                                Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "FeaturedItems"), "*"))
                                ClearFiles.Add(file);
                        }
                        catch { }

                        foreach (string file in Directory.EnumerateFiles(SaveCleanerSettings.Settings.PathToTheSims3Document, "*.xml",
                            SearchOption.TopDirectoryOnly))
                            ClearFiles.Add(file);

                        if (File.Exists(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "DCCache\\missingdeps.idx")))
                            ClearFiles.Add(
                                Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "DCCache\\missingdeps.idx"));

                        if (File.Exists(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "DCCache\\dcc.ent")))
                            ClearFiles.Add(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "DCCache\\dcc.ent"));

                        foreach (string file in Directory.EnumerateFiles(
                            Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "Downloads"), "*.bin"))
                            ClearFiles.Add(file);

                        foreach (string file in Directory.EnumerateFiles(
                            Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document, "SigsCache"), "*.bin"))
                            ClearFiles.Add(file);

                        if (File.Exists(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document,
                            "SavedSims\\Downloadedsims.index")))
                            ClearFiles.Add(Path.Combine(SaveCleanerSettings.Settings.PathToTheSims3Document,
                                "SavedSims\\Downloadedsims.index"));
                    }
                    catch { }

                    for (int i = 0; i < ClearFiles.Count; i++)
                    {
                        try
                        {
                            File.Delete(ClearFiles[i]);
                        }
                        catch { }
                    }

                    break;
                }
            }

            e.Result = cleanerResults;
        }
    }
}