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

        private Grid _titleBar;
        private PleasantBorder _window;

        /*
        public bool DragWindow
		{
            get => GetValue(DragWindowProperty);
            set => SetValue(DragWindowProperty, value);
		}
		*/

        /*
        private Point InitialPointerLocation
		{
            get => GetValue(InitialPointerLocationProperty);
            set => SetValue(InitialPointerLocationProperty, value);
		}
		*/

        /*
        public Point InitialDragShift
		{
            get => GetValue(InitialDragShiftProperty);
            set => SetValue(InitialDragShiftProperty, value);
		}
		*/

        static PleasantDialogWindow()
        {
        }

        //private static readonly StyledProperty<bool> DragWindowProperty =
        //    AvaloniaProperty.Register<PleasantDialogWindow, bool>(nameof(DragWindow), true);

        //private static readonly AttachedProperty<Point> InitialPointerLocationProperty =
        //    AvaloniaProperty.RegisterAttached<PleasantDialogWindow, IControl, Point>(nameof(InitialPointerLocation));

        //private static readonly AttachedProperty<Point> InitialDragShiftProperty =
        //    AvaloniaProperty.RegisterAttached<PleasantDialogWindow, IControl, Point>(nameof(InitialDragShift));

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

        Type IStyleable.StyleKey => typeof(PleasantDialogWindow);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            PseudoClasses.Set(":open", true);

            _titleBar = e.NameScope.Find<Grid>("PART_TitleBar");
            _window = e.NameScope.Find<PleasantBorder>("PART_ModalWindow");

            e.NameScope.Find<Button>("PART_CloseButton").Click += (s, _e) => Close();

            Focus();
            PointerPressed += (s, _e) => { Focus(); };

            // TODO: Work on window dragging

            /*
            _window.RenderTransform = new TranslateTransform();

			_titleBar.PointerPressed += _titleBar_PointerPressed;
            _titleBar.PointerReleased += Control_PointerReleased;
            */
        }

        protected override async Task OnAnimation()
        {
            await Task.Delay(200);
        }

        /*
        private void _titleBar_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            IControl control = _window;

            e.Pointer.Capture(control);

            Point currentPointerPositionInWindow = GetCurrentPointerPositionInWindow(control, e);

            InitialPointerLocation = currentPointerPositionInWindow;

            Point startControlPosition = GetShift(control);

            InitialDragShift = startControlPosition;

			_titleBar.PointerMoved += Control_PointerMoved;
        }
        

        
		private void Control_PointerReleased(object sender, PointerReleasedEventArgs e)
		{
			IControl control = _window;

            e.Pointer.Capture(null);

            ShiftControl(control, e);

            _titleBar.PointerMoved -= Control_PointerMoved;
		}

		private void Control_PointerMoved(object sender, PointerEventArgs e)
		{
			IControl control = _window;

            ShiftControl(control, e);
		}
		

		private Point GetCurrentPointerPositionInWindow(IControl control, PointerEventArgs e)
        {
            return e.GetPosition(GetVisualLayerManager(control));
        }

        private static VisualLayerManager GetVisualLayerManager(IControl control)
        {
            return control.GetVisualAncestors().OfType<VisualLayerManager>().FirstOrDefault();
        }

        
        private void SetInitialPointerLocation(IControl obj, Point value)
        {
            obj.SetValue(InitialPointerLocationProperty, value);
        }
        

        private static Point GetShift(IControl control)
        {
            TranslateTransform translateTransform = (TranslateTransform)control.RenderTransform;
            return new Point(translateTransform.X, translateTransform.Y);
        }

        private void SetShift(IControl control, Point shift)
        {
            if ((control.RenderTransform is TranslateTransform) == false)
			{
                control.RenderTransform = new TranslateTransform(shift.X, shift.Y);
            }

            TranslateTransform translateTransform = (TranslateTransform)control.RenderTransform;

            translateTransform.X = shift.X;
            translateTransform.Y = shift.Y;
        }

        
        private void ShiftControl(IControl control, PointerEventArgs e)
        {
            // get the current pointer location
            Point currentPointerPosition = GetCurrentPointerPositionInWindow(control, e);

            // get the pointer location when Drag operation was started
            Point startPointerPosition = InitialPointerLocation;

            // diff is how far the pointer shifted
            Point diff = currentPointerPosition - startPointerPosition;

            // get the original shift when the drag operation started
            Point startControlPosition = InitialDragShift;

            // get the resulting shift as the sum of 
            // pointer shift during the drag and the original shift
            Point shift = diff + startControlPosition;

            // set the shift on the control
            SetShift(control, shift);
        }
        */
    }
}