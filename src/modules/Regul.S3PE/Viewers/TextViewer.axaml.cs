using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit.Highlighting;
using Regul.S3PE.Interfaces;
using Regul.S3PI.Interfaces;

namespace Regul.S3PE.Viewers
{
	public class TextViewer : UserControl, IViewer
	{
		public AvaloniaEdit.TextEditor AvaloniaEdit { get; set; }
		public Button Button { get; set; }

		public TextViewer()
		{
			AvaloniaXamlLoader.Load(this);

			AvaloniaEdit = this.FindControl<AvaloniaEdit.TextEditor>("PART_AvaloniaEdit");
			Button = this.FindControl<Button>("PART_Edit");

			Button.Click += (s, e) =>
			{
				EditResource?.Invoke();
			};
		}
		public TextViewer(IResource resource, Action action, string tag) : this()
		{
			EditResource = action;
			Resource = resource;

			switch (tag)
			{
				case "CNFG":
				case "LAYO":
				case "VOCE":
				case "MIXR":
				case "ITUN":
				case "DMTR":
				case "_INI":
				case "SKIL":
				case "PTRN":
				case "BUFF":
				case "RMLS":
				case "TRIG":
				case "SIMO":
				case "S3SA":
				case "TXTC":
				case "_XML":
				case "COMP":
				case "HTML":
					AvaloniaEdit.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML");
					AvaloniaEdit.Document.Text = new StreamReader(Resource.Stream).ReadToEnd();
					break;
				case "_CSS":
					AvaloniaEdit.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("CSS");
					AvaloniaEdit.Document.Text = new StreamReader(Resource.Stream).ReadToEnd();
					break;
				default:
					AvaloniaEdit.Document.Text = new StreamReader(Resource.Stream).ReadToEnd();
					break;
			}

			if (action == null)
				Button.IsVisible = false;
		}

		public TextViewer(string s) : this()
		{
			AvaloniaEdit.Document.Text = s;

			Button.IsVisible = false;
		}

		public IResource Resource { get; set; }
		public Action EditResource { get; set; }
	}
}