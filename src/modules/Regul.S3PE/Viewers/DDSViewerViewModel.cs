using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Pfim;
using Regul.Base;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using Onebeld.Extensions;

namespace Regul.S3PE.Viewers
{
	public class DDSViewerViewModel : ViewModelBase
	{
		private IImage _image;
		private Stream _dds;
		private Bitmap _bitmapImage;

		private bool _alphaChannel;
		private float _scale = 100;

		public bool AlphaChannel
		{
			get => _alphaChannel;
			set
			{
				if (!RaiseAndSetIfChanged(ref _alphaChannel, value) || _image == null) return;

				UpdateViewer();
			}
		}

		public float Scale
		{
			get => _scale;
			set
			{
				if (!RaiseAndSetIfChanged(ref _scale, value) || _image == null) return;

				RaisePropertyChanged(nameof(WidthScale));
				RaisePropertyChanged(nameof(HeightScale));
			}
		}

		public double WidthScale
		{
			get
			{
				if (_image == null) return 0;

				return _image.Width * (Scale * 0.01);
			}
		}

		public double HeightScale
		{
			get
			{
				if (_image == null) return 0;

				return _image.Height * (Scale * 0.01);
			}
		}

		public string ImageWidth
		{
			get
			{
				if (_image == null) return "0";

				return _image.Width.ToString();
			}
		}

		public string ImageHeight
		{
			get
			{
				if (_image == null) return "0";

				return _image.Height.ToString();
			}
		}

		public Bitmap BitmapImage
		{
			get => _bitmapImage;
			set => RaiseAndSetIfChanged(ref _bitmapImage, value);
		}

		public void UpdateViewer()
		{
			if (_image == null) return;

			byte[] data;
			switch (_image.Format)
			{
				case ImageFormat.R5g6b5:
					data = GetData<Bgr565>(new PngEncoder { ColorType = AlphaChannel ? PngColorType.RgbWithAlpha : PngColorType.Rgb });
					break;
				case ImageFormat.R5g5b5a1:
					data = GetData<Bgra5551>(new PngEncoder { ColorType = AlphaChannel ? PngColorType.RgbWithAlpha : PngColorType.Rgb });
					break;
				case ImageFormat.Rgb24:
					data = GetData<Rgb24>(new PngEncoder { ColorType = AlphaChannel ? PngColorType.RgbWithAlpha : PngColorType.Rgb });
					break;
				case ImageFormat.Rgba32:
					data = GetData<Bgra32>(new PngEncoder { ColorType = AlphaChannel ? PngColorType.RgbWithAlpha : PngColorType.Rgb });
					break;

				default:
					throw new NotImplementedException($"Unsupported pixel format ({_image.Format})");
			}

			using (MemoryStream ms = new MemoryStream(data))
			{
				RaisePropertyChanged(nameof(WidthScale));
				RaisePropertyChanged(nameof(HeightScale));

				BitmapImage = new Bitmap(ms);
			}
		}

		public void LoadDDS(Stream stream)
		{
			_dds = stream;
			_image = Dds.Create(stream, new Pfim.PfimConfig());

			RaisePropertyChanged(nameof(ImageWidth));
			RaisePropertyChanged(nameof(ImageHeight));
			UpdateViewer();
		}

		private byte[] GetData<T>(SixLabors.ImageSharp.Formats.IImageEncoder encoder) where T : unmanaged, IPixel<T>
		{
			using (MemoryStream stream = new MemoryStream())
			{
				SixLabors.ImageSharp.Image<T> im = SixLabors.ImageSharp.Image.LoadPixelData<T>(_image.Data, _image.Width, _image.Height);

				im.Save(stream, encoder);

				return stream.ToArray();
			}
		}

		private async void ExportToDDS()
		{
			SaveFileDialog dialog = new SaveFileDialog()
			{
				Filters = new System.Collections.Generic.List<FileDialogFilter>
				{
					new FileDialogFilter
					{
						Name = "DDS-" + App.GetResource<string>("Files"),
						Extensions = {"dds"}
					}
				}
			};

			string result = await dialog.ShowAsync(WindowsManager.MainWindow);

			if (!string.IsNullOrEmpty(result))
			{
				BitmapImage.Save(result);
			}
		}

		private async void ExportToImagePNG()
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
	}
}
