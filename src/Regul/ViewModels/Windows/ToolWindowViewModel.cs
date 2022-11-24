using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using PleasantUI;
using PleasantUI.Controls;
using PleasantUI.Extensions;
using Regul.ModuleSystem;
using Regul.ModuleSystem.Structures;

namespace Regul.ViewModels.Windows;

public class ToolWindowViewModel : ViewModelBase
{
    private Module? _selectedModule;
    private Instrument? _selectedInstrument;

    private string _moduleNameSearching = string.Empty;
    private string _instrumentNameSearching = string.Empty;
    private bool _invertModuleList;
    private bool _invertInstrumentList;

    public AvaloniaList<Module> SortedModules { get; } = new();
    public AvaloniaList<Instrument> SortedInstruments { get; } = new();

    public Module? SelectedModule
    {
        get => _selectedModule;
        set => RaiseAndSetIfChanged(ref _selectedModule, value);
    }
    public Instrument? SelectedInstrument
    {
        get => _selectedInstrument;
        set => RaiseAndSetIfChanged(ref _selectedInstrument, value);
    }

    public string ModuleNameSearching
    {
        get => _moduleNameSearching;
        set => RaiseAndSetIfChanged(ref _moduleNameSearching, value);
    }
    public string InstrumentNameSearching
    {
        get => _instrumentNameSearching;
        set => RaiseAndSetIfChanged(ref _instrumentNameSearching, value);
    }
    public bool InvertModuleList
    {
        get => _invertModuleList;
        set => RaiseAndSetIfChanged(ref _invertModuleList, value);
    }
    public bool InvertInstrumentList
    {
        get => _invertInstrumentList;
        set => RaiseAndSetIfChanged(ref _invertInstrumentList, value);
    }
    
    public int? ModulesWithInstrumentsCount { get; set; }
    public int InstrumentsInModuleCount { get; set; }
    
    
    public ToolWindowViewModel()
    {
        OnSearchModules(ModuleManager.Modules);

        this.WhenAnyValue(x => x.ModuleNameSearching, x => x.InvertModuleList)
            .Subscribe(_ => OnSearchModules(ModuleManager.Modules));

        this.WhenAnyValue(x => x.SelectedModule, x => x.InstrumentNameSearching, x => x.InvertInstrumentList)
            .Skip(1)
            .Subscribe(items =>
            {
                OnSearchInstruments(items.Item1?.Instance.Instruments);
            });
        this.WhenAnyValue(x => x.SelectedModule)
            .Skip(1)
            .Subscribe(module =>
            {
                if (module?.Instance.Instruments is not null)
                {
                    InstrumentsInModuleCount = module.Instance.Instruments.Count;
                    RaisePropertyChanged(nameof(InstrumentsInModuleCount));
                }
            });
    }

    private void OnSearchModules(AvaloniaList<Module> modules)
    {
        SortedModules.Clear();

        List<Module> list = new(modules);

        list = list.FindAll(x => x.Instance.Instruments is { Count: > 0 });

        if (ModulesWithInstrumentsCount is null)
        {
            ModulesWithInstrumentsCount = list.Count;
            RaisePropertyChanged(nameof(ModulesWithInstrumentsCount));
        }
        
        if (!string.IsNullOrWhiteSpace(ModuleNameSearching))
            list = list.FindAll(x =>
            {
                if (Application.Current is not null && Application.Current.TryFindResource(x.Instance.Name, out object? name) && name is string s)
                {
                    return s.ToLower().Contains(ModuleNameSearching);
                }

                return x.Instance.Name.ToLower().Contains(ModuleNameSearching);
            });
        
        if (InvertModuleList)
            SortedModules.AddRange(list.OrderByDescending(x => x.Instance.Name));
        else SortedModules.AddRange(list.OrderBy(x => x.Instance.Name));
    }

    private void OnSearchInstruments(AvaloniaList<Instrument>? instruments)
    {
        SortedInstruments.Clear();
        
        if (instruments is null) return;

        List<Instrument> list = new(instruments);

        if (!string.IsNullOrWhiteSpace(InstrumentNameSearching))
            list = list.FindAll(x =>
            {
                if (x.Name is null) return false;

                if (Application.Current is not null && Application.Current.TryFindResource(x.Name, out object? name) && name is string s)
                {
                    return s.ToLower().Contains(InstrumentNameSearching);
                }

                return x.Name.ToLower().Contains(InstrumentNameSearching);
            });
        
        if (InvertInstrumentList)
            SortedInstruments.AddRange(list.OrderByDescending(x => x.Name));
        else SortedInstruments.AddRange(list.OrderBy(x => x.Name));
    }
    
    public void CloseWithInstrument(ContentDialog contentDialog)
    {
        SelectedInstrument?.Execute.Invoke();
        
        contentDialog.Close();
    }
    
    public void Close(ContentDialog contentDialog)
    {
        contentDialog.Close();
    }
}