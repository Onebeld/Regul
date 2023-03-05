using Avalonia.Collections;
using PleasantUI;
using PleasantUI.Controls;
using PleasantUI.Extensions;
using PleasantUI.Reactive;
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

        this.WhenAnyValue(x => x.SelectedModule, x => x.InstrumentNameSearching)
            .Skip(1)
            .Subscribe(items =>
            {
                OnSearchInstruments(items.Item1?.Instance.Instruments);
            });
        this.WhenAnyValue(x => x.InvertInstrumentList)
            .Skip(1)
            .Subscribe(items => OnSearchInstruments(SelectedModule.Instance.Instruments));
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
            list = list.FindAll(x => App.GetString(x.Instance.Name).ToLower().Contains(ModuleNameSearching));

        list = new List<Module>(list.OrderBy(x => x.Instance.Name));

        if (InvertModuleList)
            list.Reverse();

        SortedModules.AddRange(list);
    }

    private void OnSearchInstruments(AvaloniaList<Instrument>? instruments)
    {
        SortedInstruments.Clear();

        if (instruments is null) return;

        List<Instrument> list = new(instruments);

        if (!string.IsNullOrWhiteSpace(InstrumentNameSearching))
            list = list.FindAll(x => x.Name is not null && App.GetString(x.Name).ToLower().Contains(InstrumentNameSearching));

        list = new List<Instrument>(list.OrderBy(x => x.Name));

        if (InvertInstrumentList)
            list.Reverse();

        SortedInstruments.AddRange(list);
    }

    public void CloseWithInstrument(ContentDialog contentDialog)
    {
        SelectedInstrument?.Execute.Invoke();

        contentDialog.Close();
    }

    public void Close(ContentDialog contentDialog) => contentDialog.Close();
}