using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace PleasantUI.Controls;

public partial class ContentDialog : ModalWindow, IStyleable
{
    static ContentDialog() { }

    Type IStyleable.StyleKey => typeof(ContentDialog);

    protected override async Task OnAnimation() => await Task.Delay(200);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Focus();
    }
}