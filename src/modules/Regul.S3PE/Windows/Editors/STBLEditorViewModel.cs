using Avalonia.Collections;
using AvaloniaEdit.Document;
using Regul.Base;
using Regul.S3PE.Structures;
using System.Collections.Generic;
using Onebeld.Extensions;

namespace Regul.S3PE.Windows.Editors
{
	public class STBLEditorViewModel : ViewModelBase
	{
		private TextDocument _textDocument;
		
		private STBLResource _selectedResource;
		private AvaloniaList<STBLResource> _resources = new AvaloniaList<STBLResource>();

		public STBLResource SelectedResource
		{
			get => _selectedResource;
			set
			{
				RaiseAndSetIfChanged(ref _selectedResource, value);
				if (value != null)
				{
					TextDocument.Text = value.Value ?? string.Empty;
				}
				else TextDocument.Text = string.Empty;
			}
		}

		public AvaloniaList<STBLResource> Resources
		{
			get => _resources;
			set => RaiseAndSetIfChanged(ref _resources, value);
		}

		public TextDocument TextDocument
		{
			get => _textDocument;
			set => RaiseAndSetIfChanged(ref _textDocument, value);
		}

		public void Initialize(IDictionary<ulong, string> dictionary)
		{
			TextDocument = new TextDocument();
			TextDocument.Changed += TextDocument_Changed;

			foreach (KeyValuePair<ulong, string> pair in dictionary)
			{
				Resources.Add(new STBLResource {Key = pair.Key, Value = pair.Value});
			}
		}

		private void TextDocument_Changed(object sender, DocumentChangeEventArgs e)
		{
			if (SelectedResource != null)
			{
				SelectedResource.Value = TextDocument.Text;
			}
		}

		private void AddResource()
		{
			STBLResource resource = new STBLResource();
			Resources.Add(resource);
			SelectedResource = resource;
		}

		private void RemoveResource()
		{
			Resources.Remove(SelectedResource);
		}

		private void OK()
		{
			IDictionary<ulong, string> dictionary = new Dictionary<ulong, string>();

			foreach (STBLResource resource in Resources)
				dictionary.Add(resource.Key, resource.Value);

			STBLEditor foundTextEditor = WindowsManager.FindModalWindow<STBLEditor>();
			WindowsManager.OtherModalWindows.Remove(foundTextEditor);

			foundTextEditor?.Close(dictionary);
		}

		private void Close()
		{
			STBLEditor foundTextEditor = WindowsManager.FindModalWindow<STBLEditor>();
			WindowsManager.OtherModalWindows.Remove(foundTextEditor);

			foundTextEditor?.Close(null);
		}
	}
}
