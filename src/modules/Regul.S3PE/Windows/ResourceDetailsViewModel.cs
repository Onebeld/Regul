using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Onebeld.Extensions;
using Regul.Base;
using Regul.S3PE.Structures;
using Regul.S3PI.Extensions;
using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Windows
{
    public class ResourceDetailsViewModel : ViewModelBase, IResourceKey
    {
        private AvaloniaList<ResourceType> _resourceTypes = new AvaloniaList<ResourceType>();
        private ResourceType _selectedResourceType;

        private string _resourceName;
        private ulong _instance;
        private uint _group;
        private string _fileName;

        private bool _internalChg;
        private bool _importedFile;

        private bool _compress;
        private bool _useResourceName;
        private bool _allowRename;

        #region Properties

        public string ResourceName
        {
            get => _resourceName;
            set
            {
                RaiseAndSetIfChanged(ref _resourceName, value); 
                UpdateTGIN();
            }
        }

        public ulong Instance
        {
            get => _instance;
            set
            {
                RaiseAndSetIfChanged(ref _instance, value);
                UpdateTGIN();
            }
        }

        public uint Group
        {
            get => _group;
            set
            {
                RaiseAndSetIfChanged(ref _group, value);
                UpdateTGIN();
            }
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                RaiseAndSetIfChanged(ref _fileName, value);
                ImportedFile = !string.IsNullOrEmpty(value);
            }
        }

        public bool Compress
        {
            get => _compress;
            set => RaiseAndSetIfChanged(ref _compress, value);
        }

        public bool UseResourceName
        {
            get => _useResourceName;
            set => RaiseAndSetIfChanged(ref _useResourceName, value);
        }

        private bool ImportedFile
        {
            get => _importedFile;
            set => RaiseAndSetIfChanged(ref _importedFile, value);
        }

        public bool AllowRename
        {
            get => _allowRename;
            set => RaiseAndSetIfChanged(ref _allowRename, value);
        }

        public AvaloniaList<ResourceType> ResourceTypes
        {
            get => _resourceTypes;
            set => RaiseAndSetIfChanged(ref _resourceTypes, value);
        }

        public ResourceType SelectedResourceType
        {
            get => _selectedResourceType;
            set => RaiseAndSetIfChanged(ref _selectedResourceType, value);
        }
        
        public uint ResourceType
        {
            get => SelectedResourceType.Type;
            set => throw new NotImplementedException();
        }

        public uint ResourceGroup
        {
            get => Group;
            set => throw new NotImplementedException();
        }

        public bool Replace { get; set; }

        #endregion

        public static implicit operator TGIN(ResourceDetailsViewModel viewModel)
        {
            return viewModel._details;
        }

        private TGIN _details;
        
        public ResourceDetailsViewModel() : this(true) {}

        public ResourceDetailsViewModel(bool useName)
        {
            UseResourceName = useName;
            Compress = true;

            foreach (KeyValuePair<string,List<string>> keyValuePair in ExtList.Ext)
            {
                try
                {
                    ResourceTypes.Add(new ResourceType
                    {
                        Type = Convert.ToUInt32(keyValuePair.Key, keyValuePair.Key.StartsWith("0x") ? 16 : 10),
                        Tag = keyValuePair.Value[0]
                    });
                }
                catch {}
            }

            ResourceType resourceType = new ResourceType { Type = 0x00000000, Tag = "UNKN" };

            ResourceTypes = new AvaloniaList<ResourceType>(ResourceTypes.OrderBy(x => x.Tag));
            ResourceTypes.Insert(0, resourceType);

            SelectedResourceType = resourceType;
        }

        public ResourceDetailsViewModel(bool useName, IResourceKey key) : this(useName)
        {
            _internalChg = true;

            try
            {
                SelectedResourceType = ResourceTypes.FirstOrDefault(x => x.Type == key.ResourceType);

                Group = key.ResourceGroup;
                Instance = key.Instance;
            }
            finally
            {
                _internalChg = false;
                UpdateTGIN();
            }
        }

        private void UpdateTGIN()
        {
            if (_internalChg) return;

            _details = new TGIN
            {
                ResType = SelectedResourceType.Type,
                ResGroup = Group,
                ResInstance = Instance,
                ResName = ResourceName
            };
        }

        private void CopyResourceKeyDetails()
        {
            Application.Current.Clipboard.SetTextAsync((AResourceKey)_details );
        }

        private async void PasteResourceKeyDetails()
        {
            TGIBlock tgiBlock = new TGIBlock(null);
            if (!AResourceKey.TryParse(await Application.Current.Clipboard.GetTextAsync(), tgiBlock)) return;

            Group = tgiBlock.ResourceGroup;
            Instance = tgiBlock.Instance;
            SelectedResourceType = ResourceTypes.FirstOrDefault(x => x.Type == tgiBlock.ResourceType);
        }

        private async void ImportFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter {Extensions = {"*"}, Name = App.GetResource<string>("AllFiles")});
            string[] files = await dialog.ShowAsync(WindowsManager.MainWindow);
            
            if (files.Length == 0) return;
            
            FileName = files[0];
        }

        private void OK()
        {
            ResourceDetails foundResourceDetails = WindowsManager.FindModalWindow<ResourceDetails>();
            WindowsManager.OtherModalWindows.Remove(foundResourceDetails);

            foundResourceDetails?.Close(true);
        }

        private void Cancel()
        {
            ResourceDetails foundResourceDetails = WindowsManager.FindModalWindow<ResourceDetails>();
            WindowsManager.OtherModalWindows.Remove(foundResourceDetails);

            foundResourceDetails?.Close();
        }

        private void FNV64Convert() => Instance = FNV.Hash64(ResourceName, false);
        private void CLIPIIDConvert() => Instance = FNV.Hash64(ResourceName, true);
        private void FNV32Convert() => Instance = FNV.Hash32(ResourceName, false);
        
        public bool Equals(IResourceKey x, IResourceKey y) => x.Equals(y);
        public bool Equals(IResourceKey other) => CompareTo(other) == 0;
        public int GetHashCode(IResourceKey obj) => obj.GetHashCode();
        public int CompareTo(IResourceKey other)
        {
            int res = ResourceType.CompareTo(other.ResourceType);
            if (res != 0) return res;

            res = ResourceGroup.CompareTo(other.ResourceGroup);
            if (res != 0) return res;

            return Instance.CompareTo(other.Instance);
        }
    }
}