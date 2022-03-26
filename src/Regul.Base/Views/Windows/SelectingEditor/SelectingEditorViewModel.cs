﻿using System.Linq;
using Avalonia.Collections;
using Onebeld.Extensions;
using Regul.ModuleSystem;

namespace Regul.Base.Views.Windows
{
    public class SelectingEditorViewModel : ViewModelBase
    {
        private AvaloniaList<Editor> _editors;
        private AvaloniaList<Editor> _foundEditors = new AvaloniaList<Editor>();

        private string _searchText;
        private Editor _selectedItem;

        public void Initialize()
        {
            FindEditor();
            SelectedItem = Editors.First();
        }

        public void CloseWithSave()
        {
            WindowsManager.FindModalWindow<SelectingEditor>().Close(SelectedItem);
        }

        public void Close()
        {
            WindowsManager.FindModalWindow<SelectingEditor>().Close(null);
        }

        public void FindEditor()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                FoundEditors = Editors;
                return;
            }

            FoundEditors = new AvaloniaList<Editor>();

            //foreach (Editor editor in Editors)
            for (int i = 0; i < Editors.Count; i++)
            {
                Editor editor = Editors[i];
                if (editor.Name.ToLower().Contains(SearchText.ToLower()))
                    FoundEditors.Add(editor);
            }
        }

        #region Propeties

        public Editor SelectedItem
        {
            get => _selectedItem;
            set => RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public AvaloniaList<Editor> Editors
        {
            get => _editors;
            set => RaiseAndSetIfChanged(ref _editors, value);
        }

        public AvaloniaList<Editor> FoundEditors
        {
            get => _foundEditors;
            set => RaiseAndSetIfChanged(ref _foundEditors, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                RaiseAndSetIfChanged(ref _searchText, value);
                FindEditor();
            }
        }

        #endregion
    }
}