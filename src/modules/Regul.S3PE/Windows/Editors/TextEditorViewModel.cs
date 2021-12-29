using AvaloniaEdit.Document;
using Onebeld.Extensions;
using Regul.Base;

namespace Regul.S3PE.Windows.Editors
{
    public class TextEditorViewModel : ViewModelBase
    {
        private TextDocument _textDocument;

        public TextDocument TextDocument
        {
            get => _textDocument;
            set => RaiseAndSetIfChanged(ref _textDocument, value);
        }

        private void OK()
        {
            TextEditor foundTextEditor = WindowsManager.FindModalWindow<TextEditor>();
            WindowsManager.OtherModalWindows.Remove(foundTextEditor);

            foundTextEditor?.Close(TextDocument.Text);
        }

        private void Close()
        {
            TextEditor foundTextEditor = WindowsManager.FindModalWindow<TextEditor>();
            WindowsManager.OtherModalWindows.Remove(foundTextEditor);

            foundTextEditor?.Close(null);
        }
    }
}