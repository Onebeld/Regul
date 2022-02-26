#region

using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using PleasantUI.Controls.Custom;

#endregion

namespace Regul.Base.DragAndDrop
{
    public enum TypeDrop
    {
        File,
        Module
    }

    public class DragAndDropWindow : PleasantModalWindow, IStyleable
    {
        private Border _fileDrop;

        private Border _moduleDrop;
        private readonly TypeDrop _typeDrop;

        public DragAndDropWindow(TypeDrop typeDrop)
        {
            _typeDrop = typeDrop;
        }

        Type IStyleable.StyleKey => typeof(DragAndDropWindow);

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _moduleDrop = e.NameScope.Get<Border>("PART_ModuleDrop");
            _fileDrop = e.NameScope.Get<Border>("Part_FileDrop");

            switch (_typeDrop)
            {
                case TypeDrop.File:
                    _moduleDrop.IsVisible = false;
                    break;
                case TypeDrop.Module:
                    _fileDrop.IsVisible = false;
                    break;
            }
        }
    }
}