using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Regul.Base;
using System.IO;
using Onebeld.Extensions;

namespace Regul.S3PE.Viewers
{
	public class ImageViewerViewModel : ViewModelBase
	{
		private Bitmap _bitmapImage;

		public S3PEViewModel ParentViewModel;

		private float _scale = 100;

		public Bitmap BitmapImage
		{
			get => _bitmapImage;
			set => RaiseAndSetIfChanged(ref _bitmapImage, value);
		}

		public float Scale
		{
			get => _scale;
			set
			{
				if (!RaiseAndSetIfChanged(ref _scale, value) || _bitmapImage == null) return;

				RaisePropertyChanged(nameof(WidthScale));
				RaisePropertyChanged(nameof(HeightScale));
			}
		}

		public double WidthScale
		{
			get
			{
				if (_bitmapImage == null) return 0;

				return _bitmapImage.Size.Width * (Scale * 0.01);
			}
		}

		public double HeightScale
		{
			get
			{
				if (_bitmapImage == null) return 0;

				return _bitmapImage.Size.Height * (Scale * 0.01);
			}
		}

		public string ImageWidth
		{
			get
			{
				if (_bitmapImage == null) return "0";

				return _bitmapImage.Size.Width.ToString();
			}
		}

		public string ImageHeight
		{
			get
			{
				if (_bitmapImage == null) return "0";

				return _bitmapImage.Size.Height.ToString();
			}
		}

		public ImageViewerViewModel() { }

		public void LoadImage(Stream stream)
		{
			if (stream.Length != 0)
				BitmapImage = new Bitmap(stream);

			RaisePropertyChanged(nameof(ImageWidth));
			RaisePropertyChanged(nameof(ImageHeight));

			RaisePropertyChanged(nameof(WidthScale));
			RaisePropertyChanged(nameof(HeightScale));
		}

		public async void ExportImage()
		{
			SaveFileDialog dialog = new SaveFileDialog()
			{
				Filters = new System.Collections.Generic.List<FileDialogFilter>
				{
					new FileDialogFilter
					{
						Name = App.GetResource<string>("ImageFiles"),
						Extensions = {"png"}
					}
				}
			};

			string result = await dialog.ShowAsync(WindowsManager.MainWindow);

			if (!string.IsNullOrEmpty(result))
			{
				BitmapImage.Save(result);
			}
		}

		public void ImportImage() => ParentViewModel.ImportImag();
	}
}
