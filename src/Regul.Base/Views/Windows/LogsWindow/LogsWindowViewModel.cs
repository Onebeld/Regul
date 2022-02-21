using Avalonia.Controls;
using AvaloniaEdit.Document;
using Onebeld.Extensions;
using Onebeld.Logging;

namespace Regul.Base.Views.Windows
{
	public class LogsWindowViewModel : ViewModelBase
	{
		private TextDocument _textDocument = new TextDocument();

		public TextDocument TextDocument
		{
			get => _textDocument;
			set => RaiseAndSetIfChanged(ref _textDocument, value);
		}

		public LogsWindowViewModel()
		{
			//foreach (string item in Logger.Current.Logs)
			for (int i = 0; i < Logger.Current.Logs.Count; i++)
			{
				string item = Logger.Current.Logs[i];
				if (Logger.Current.Logs.IndexOf(item) == 0)
					TextDocument.Text += item;
				else
					TextDocument.Text += "\n" + item;
			}

			Logger.Current.Writed += Current_Writed;
		}

		private void Current_Writed(object sender, System.EventArgs e)
		{
			if (Logger.Current.Logs.IndexOf((string)sender) == 0)
				TextDocument.Text += (string)sender;
			else
				TextDocument.Text += "\n" + (string)sender;
		}

		private void ClearLogs()
		{
			Logger.Current.Logs.Clear();
			TextDocument.Text = string.Empty;
		}

		private async void SaveLogs()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog { Filters = new System.Collections.Generic.List<FileDialogFilter> 
			{
				new FileDialogFilter { Name = "Log-" + App.GetResource<string>("Files"), Extensions = { "log" } }
			}};

			string path = await saveFileDialog.ShowAsync(WindowsManager.FindWindow<LogsWindow>());

			if (!string.IsNullOrEmpty(path))
				Logger.Current.SaveLog(path);
		}
	}
}
