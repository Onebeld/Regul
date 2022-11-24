namespace PleasantUI.Other;

public class KeyColor : ViewModelBase
{
    private string _key = null!;
    private uint _value;

    public KeyColor(string key, uint value)
    {
        Key = key;
        Value = value;
    }

    public string Key
    {
        get => _key;
        set => RaiseAndSetIfChanged(ref _key, value);
    }

    public uint Value
    {
        get => _value;
        set => RaiseAndSetIfChanged(ref _value, value);
    }
}