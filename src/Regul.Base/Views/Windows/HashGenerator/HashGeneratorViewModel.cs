using System.Security.Cryptography;
using Onebeld.Extensions;

namespace Regul.Base.Views.Windows;

public class HashGeneratorViewModel : ViewModelBase
{
    private bool _dec;
    private bool _hex = true;
    private bool _highBit;

    private ulong _number;

    private string _text = string.Empty;

    public ulong Number
    {
        get => _number;
        set => RaiseAndSetIfChanged(ref _number, value);
    }

    public string Text
    {
        get => _text;
        set
        {
            if (!RaiseAndSetIfChanged(ref _text, value)) return;

            RaisePropertyChanged(nameof(Fnv24));
            RaisePropertyChanged(nameof(Fnv32));
            RaisePropertyChanged(nameof(Fnv56));
            RaisePropertyChanged(nameof(Fnv64));
        }
    }

    public bool Hex
    {
        get => _hex;
        set
        {
            if (!RaiseAndSetIfChanged(ref _hex, value)) return;

            RaisePropertyChanged(nameof(Fnv24));
            RaisePropertyChanged(nameof(Fnv32));
            RaisePropertyChanged(nameof(Fnv56));
            RaisePropertyChanged(nameof(Fnv64));
        }
    }

    public bool Dec
    {
        get => _dec;
        set
        {
            if (!RaiseAndSetIfChanged(ref _dec, value)) return;

            RaisePropertyChanged(nameof(Fnv24));
            RaisePropertyChanged(nameof(Fnv32));
            RaisePropertyChanged(nameof(Fnv56));
            RaisePropertyChanged(nameof(Fnv64));
        }
    }

    public string Fnv24
    {
        get
        {
            uint num = FNV.Hash24(Text, HighBit);

            if (Hex) return "0x" + num.ToString("x8").ToUpper();
            return Dec ? num.ToString() : string.Empty;
        }
    }

    public string Fnv32
    {
        get
        {
            uint num = FNV.Hash32(Text, HighBit);

            if (Hex) return "0x" + num.ToString("x8").ToUpper();
            return Dec ? num.ToString() : string.Empty;
        }
    }

    public string Fnv56
    {
        get
        {
            ulong num = FNV.Hash56(Text, HighBit);

            if (Hex) return "0x" + num.ToString("x16").ToUpper();
            return Dec ? num.ToString() : string.Empty;
        }
    }

    public string Fnv64
    {
        get
        {
            ulong num = FNV.Hash64(Text, HighBit);

            if (Hex) return "0x" + num.ToString("x16").ToUpper();
            return Dec ? num.ToString() : string.Empty;
        }
    }

    public bool HighBit
    {
        get => _highBit;
        set
        {
            if (!RaiseAndSetIfChanged(ref _highBit, value)) return;

            RaisePropertyChanged(nameof(Fnv24));
            RaisePropertyChanged(nameof(Fnv32));
            RaisePropertyChanged(nameof(Fnv56));
            RaisePropertyChanged(nameof(Fnv64));
        }
    }

    public void Close(HashGenerator hashGenerator)
    {
        WindowsManager.OtherWindows.Remove(hashGenerator);
    }
}