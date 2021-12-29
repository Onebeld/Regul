using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace PleasantUI.Controls.Custom
{
	public class PleasantDialogWindow : PleasantModalWindow, IStyleable
    {
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<PleasantDialogWindow, string>(nameof(Title), "ModalWindow");

        public static readonly StyledProperty<Control> TitleContentProperty =
            AvaloniaProperty.Register<PleasantDialogWindow, Control>(nameof(TitleContent));

        public static readonly StyledProperty<IBitmap> IconProperty =
            AvaloniaProperty.Register<PleasantDialogWindow, IBitmap>(nameof(Icon));

        public static readonly StyledProperty<bool> ShowTitleBarProperty =
            AvaloniaProperty.Register<PleasantDialogWindow, bool>(nameof(ShowTitleBar), true);

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Control TitleContent
        {
            get => GetValue(TitleContentProperty);
            set => SetValue(TitleContentProperty, value);
        }

        public IBitmap Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool ShowTitleBar
        {
            get => GetValue(ShowTitleBarProperty);
            set => SetValue(ShowTitleBarProperty, value);
        }

        static PleasantDialogWindow()
        {
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            
            PseudoClasses.Set(":open", true);

            Focus();
            PointerPressed += (s,_e) => { Focus(); }; 
        }

		protected override async Task OnAnimation()
		{
            await Task.Delay(200);
		}

		Type IStyleable.StyleKey => typeof(PleasantDialogWindow);
    }
}