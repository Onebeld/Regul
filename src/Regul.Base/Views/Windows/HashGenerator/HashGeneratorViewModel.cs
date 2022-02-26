#region

using System.Security.Cryptography;
using Onebeld.Extensions;

#endregion

namespace Regul.Base.Views.Windows
{
    public class HashGeneratorViewModel : ViewModelBase
    {
        private bool _dec;
        private bool _hex = true;
        private bool _highBit;

        private ulong _number;

        private string _text;

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

                RaisePropertyChanged(nameof(FNV24));
                RaisePropertyChanged(nameof(FNV32));
                RaisePropertyChanged(nameof(FNV56));
                RaisePropertyChanged(nameof(FNV64));
            }
        }

        public bool Hex
        {
            get => _hex;
            set
            {
                if (!RaiseAndSetIfChanged(ref _hex, value)) return;

                RaisePropertyChanged(nameof(FNV24));
                RaisePropertyChanged(nameof(FNV32));
                RaisePropertyChanged(nameof(FNV56));
                RaisePropertyChanged(nameof(FNV64));
            }
        }

        public bool Dec
        {
            get => _dec;
            set
            {
                if (!RaiseAndSetIfChanged(ref _dec, value)) return;

                RaisePropertyChanged(nameof(FNV24));
                RaisePropertyChanged(nameof(FNV32));
                RaisePropertyChanged(nameof(FNV56));
                RaisePropertyChanged(nameof(FNV64));
            }
        }

        public string FNV24
        {
            get
            {
                uint num = FNV.Hash24(Text, HighBit);

                if (Hex) return "0x" + num.ToString("x8").ToUpper();
                return Dec ? num.ToString() : string.Empty;
            }
        }

        public string FNV32
        {
            get
            {
                uint num = FNV.Hash32(Text, HighBit);

                if (Hex) return "0x" + num.ToString("x8").ToUpper();
                return Dec ? num.ToString() : string.Empty;
            }
        }

        public string FNV56
        {
            get
            {
                ulong num = FNV.Hash56(Text, HighBit);

                if (Hex) return "0x" + num.ToString("x16").ToUpper();
                return Dec ? num.ToString() : string.Empty;
            }
        }

        public string FNV64
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

                RaisePropertyChanged(nameof(FNV24));
                RaisePropertyChanged(nameof(FNV32));
                RaisePropertyChanged(nameof(FNV56));
                RaisePropertyChanged(nameof(FNV64));
            }
        }

        public void Close(HashGenerator hashGenerator)
        {
            WindowsManager.OtherWindows.Remove(hashGenerator);
        }
    }
}