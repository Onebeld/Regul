#region

using System.Runtime.Serialization;
using Onebeld.Extensions;

#endregion

namespace PleasantUI.Media
{
    [DataContract]
    public class Theme : ViewModelBase
    {
        private uint _accentColor;

        private uint _backgroundColor;
        private uint _backgroundControlColor;
        private uint _backgroundLevel1Color;
        private uint _borderColor;
        private uint _closeButtonColor;
        private uint _errorColor;
        private uint _foregroundColor;
        private uint _foregroundOpacityColor;
        private uint _hoverBackgroundColor;

        private uint _hoverScrollBoxColor;

        private uint _mbDefaultButtonBackgroundColor;
        private uint _mbDefaultButtonForegroundColor;
        private uint _mbDefaultButtonHoverBackgroundColor;
        private uint _mbErrorColor;

        private uint _mbQuestionColor;
        private uint _mbSuccessColor;
        private uint _mbWarningColor;
        private string _name = string.Empty;
        private uint _pressedForegroundColor;
        private uint _scrollBoxColor;
        private uint _selectionColor;

        private uint _titleBarColor;
        private uint _titleBarFullscreenColor;
        private uint _windowBorderColor;

        [DataMember]
        public string Name
        {
            get => _name;
            set => RaiseAndSetIfChanged(ref _name, value);
        }

        [DataMember]
        public uint BackgroundColor
        {
            get => _backgroundColor;
            set => RaiseAndSetIfChanged(ref _backgroundColor, value);
        }

        [DataMember]
        public uint BackgroundControlColor
        {
            get => _backgroundControlColor;
            set => RaiseAndSetIfChanged(ref _backgroundControlColor, value);
        }

        [DataMember]
        public uint BackgroundLevel1Color
        {
            get => _backgroundLevel1Color;
            set => RaiseAndSetIfChanged(ref _backgroundLevel1Color, value);
        }

        [DataMember]
        public uint HoverBackgroundColor
        {
            get => _hoverBackgroundColor;
            set => RaiseAndSetIfChanged(ref _hoverBackgroundColor, value);
        }

        [DataMember]
        public uint WindowBorderColor
        {
            get => _windowBorderColor;
            set => RaiseAndSetIfChanged(ref _windowBorderColor, value);
        }

        [DataMember]
        public uint SelectionColor
        {
            get => _selectionColor;
            set => RaiseAndSetIfChanged(ref _selectionColor, value);
        }

        [DataMember]
        public uint BorderColor
        {
            get => _borderColor;
            set => RaiseAndSetIfChanged(ref _borderColor, value);
        }

        [DataMember]
        public uint ForegroundColor
        {
            get => _foregroundColor;
            set => RaiseAndSetIfChanged(ref _foregroundColor, value);
        }

        [DataMember]
        public uint ForegroundOpacityColor
        {
            get => _foregroundOpacityColor;
            set => RaiseAndSetIfChanged(ref _foregroundOpacityColor, value);
        }

        [DataMember]
        public uint PressedForegroundColor
        {
            get => _pressedForegroundColor;
            set => RaiseAndSetIfChanged(ref _pressedForegroundColor, value);
        }

        [DataMember]
        public uint AccentColor
        {
            get => _accentColor;
            set => RaiseAndSetIfChanged(ref _accentColor, value);
        }

        [DataMember]
        public uint ErrorColor
        {
            get => _errorColor;
            set => RaiseAndSetIfChanged(ref _errorColor, value);
        }

        [DataMember]
        public uint TitleBarColor
        {
            get => _titleBarColor;
            set => RaiseAndSetIfChanged(ref _titleBarColor, value);
        }

        [DataMember]
        public uint TitleBarFullscreenColor
        {
            get => _titleBarFullscreenColor;
            set => RaiseAndSetIfChanged(ref _titleBarFullscreenColor, value);
        }

        [DataMember]
        public uint CloseButtonColor
        {
            get => _closeButtonColor;
            set => RaiseAndSetIfChanged(ref _closeButtonColor, value);
        }

        [DataMember]
        public uint HoverScrollBoxColor
        {
            get => _hoverScrollBoxColor;
            set => RaiseAndSetIfChanged(ref _hoverScrollBoxColor, value);
        }

        [DataMember]
        public uint ScrollBoxColor
        {
            get => _scrollBoxColor;
            set => RaiseAndSetIfChanged(ref _scrollBoxColor, value);
        }

        [DataMember]
        public uint MBDefaultButtonBackgroundColor
        {
            get => _mbDefaultButtonBackgroundColor;
            set => RaiseAndSetIfChanged(ref _mbDefaultButtonBackgroundColor, value);
        }

        [DataMember]
        public uint MBDefaultButtonForegroundColor
        {
            get => _mbDefaultButtonForegroundColor;
            set => RaiseAndSetIfChanged(ref _mbDefaultButtonForegroundColor, value);
        }

        [DataMember]
        public uint MBDefaultButtonHoverBackgroundColor
        {
            get => _mbDefaultButtonHoverBackgroundColor;
            set => RaiseAndSetIfChanged(ref _mbDefaultButtonHoverBackgroundColor, value);
        }

        [DataMember]
        public uint MBQuestionColor
        {
            get => _mbQuestionColor;
            set => RaiseAndSetIfChanged(ref _mbQuestionColor, value);
        }

        [DataMember]
        public uint MBSuccessColor
        {
            get => _mbSuccessColor;
            set => RaiseAndSetIfChanged(ref _mbSuccessColor, value);
        }

        [DataMember]
        public uint MBWarningColor
        {
            get => _mbWarningColor;
            set => RaiseAndSetIfChanged(ref _mbWarningColor, value);
        }

        [DataMember]
        public uint MBErrorColor
        {
            get => _mbErrorColor;
            set => RaiseAndSetIfChanged(ref _mbErrorColor, value);
        }
    }
}