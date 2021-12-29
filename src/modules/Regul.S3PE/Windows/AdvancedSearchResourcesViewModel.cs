using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Onebeld.Extensions;
using Regul.Base;
using Regul.S3PE.Structures;
using Regul.S3PI;
using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Windows
{
	public class AdvancedSearchResourcesViewModel : ViewModelBase
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        private string _keyedWords;
        private ObservableCollection<Resource> _foundResources = new ObservableCollection<Resource>();
        private Resource _selectedResoruce;
        private bool _isIndeterminate;
        private bool _isRunningSearching;

        public ObservableCollection<Resource> FoundResources
        {
            get => _foundResources;
            set => RaiseAndSetIfChanged(ref _foundResources, value);
        }

        private ObservableCollection<Resource> ViewedResource
		{
            get => FoundResources;
		}

        public Resource SelectedResource
        {
            get => _selectedResoruce;
            set
            {
                RaiseAndSetIfChanged(ref _selectedResoruce, value);
                
                CloseWindowWithResult(value);
            }
        }

        private bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => RaiseAndSetIfChanged(ref _isIndeterminate, value);
        }

        public AvaloniaList<Resource> Resources;
        public IPackage Package;

        public string KeyedWords
        {
            get => _keyedWords;
            set
            {
                RaiseAndSetIfChanged(ref _keyedWords, value);

                if (_isRunningSearching)
                    _cancellationTokenSource.Cancel();

                Task.Run(() => SearchResources(_cancellationTokenSource.Token));
            }
        }

        public void CloseWindow()
        {
            AdvancedSearchResources foundAdvancedSearchResources = WindowsManager.FindModalWindow<AdvancedSearchResources>();
            WindowsManager.OtherModalWindows.Remove(foundAdvancedSearchResources);

            foundAdvancedSearchResources?.Close(null);
        }
        
        private void CloseWindowWithResult(Resource resource)
        {
            if (resource == null) return;
            
            AdvancedSearchResources foundAdvancedSearchResources = WindowsManager.FindModalWindow<AdvancedSearchResources>();
            WindowsManager.OtherModalWindows.Remove(foundAdvancedSearchResources);

            foundAdvancedSearchResources?.Close(resource);
        }

        private async void UpdateList()
		{
            while (_isRunningSearching)
			{
                await Task.Delay(500);
                RaisePropertyChanged(nameof(ViewedResource));
			}
		}

        private async Task SearchResources(CancellationToken token)
        {
            _isRunningSearching = true;

            try
            {
                await Task.Delay(1000, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            
            FoundResources.Clear();

            IsIndeterminate = true;
            
            string[] keys = KeyedWords.Split(',');

            UpdateList();

			foreach (Resource resource in Resources)
			{
                token.ThrowIfCancellationRequested();

                IResource res = WrapperDealer.GetResource(Package, resource.IndexEntry);

                string text = new StreamReader(res.Stream).ReadToEnd();

                foreach (string key in keys)
                {
                    if (text.Contains(key))
                    {
                        FoundResources.Add(resource);

                        break;
                    }
                }
            }

            IsIndeterminate = false;
            _isRunningSearching = false;

            RaisePropertyChanged(nameof(ViewedResource));
        }
    }
}