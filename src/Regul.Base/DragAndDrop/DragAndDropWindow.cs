using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using PleasantUI.Controls.Custom;
using System;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Regul.Base.DragAndDrop
{
	public enum TypeDrop
	{
		All,
		OnlyFile,
		OnlyModule
	}

	public enum TypeData
	{
		File,
		Module
	}

	public class DragAndDropWindow : PleasantModalWindow, IStyleable
	{
		private TypeDrop _typeDrop;

		private Border _moduleDrop;
		private Border _fileDrop;

		static DragAndDropWindow() { }

		public DragAndDropWindow(TypeDrop typeDrop)
		{
			_typeDrop = typeDrop;
		}

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			_moduleDrop = e.NameScope.Get<Border>("PART_ModuleDrop");
			_fileDrop = e.NameScope.Get<Border>("Part_FileDrop");
			
			_moduleDrop.AddHandler(DragDrop.DragOverEvent, DragOver);
			_moduleDrop.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
			_fileDrop.AddHandler(DragDrop.DragOverEvent, DragOver);
			_fileDrop.AddHandler(DragDrop.DragLeaveEvent, DragLeave);

			switch (_typeDrop)
			{
				case TypeDrop.OnlyFile:
					_moduleDrop.IsVisible = false;
					
					_fileDrop.AddHandler(DragDrop.DropEvent, FileDrop);
					break;
				case TypeDrop.OnlyModule:
					_fileDrop.IsVisible = false;
					
					_moduleDrop.AddHandler(DragDrop.DropEvent, ModuleDrop);
					break;

				default:
					_fileDrop.AddHandler(DragDrop.DropEvent, FileDrop);
					_moduleDrop.AddHandler(DragDrop.DropEvent, ModuleDrop);
					break;
			}
		}

		private void ModuleDrop(object sender, DragEventArgs e)
		{
			DragAndDropWindow window = WindowsManager.FindModalWindow<DragAndDropWindow>();
			WindowsManager.OtherModalWindows.Remove(window);
			window?.Close(TypeData.Module);
		}

		private void FileDrop(object sender, DragEventArgs e)
		{
			DragAndDropWindow window = WindowsManager.FindModalWindow<DragAndDropWindow>();
			WindowsManager.OtherModalWindows.Remove(window);
			window?.Close(TypeData.File);
		}

		private void DragOver(object s, DragEventArgs e)
		{
			Border border = (Border)s;

			border.Background = App.GetResource<SolidColorBrush>("HoverBackgroundBrush");
		}

		private void DragLeave(object s, RoutedEventArgs e)
		{
			Border border = (Border)s;
			
			border.Background = App.GetResource<SolidColorBrush>("BackgroundControlBrush");
		}

		Type IStyleable.StyleKey => typeof(DragAndDropWindow);
	}
}
