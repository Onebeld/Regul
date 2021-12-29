using Onebeld.Extensions;

namespace Regul.S3PE.Structures
{
	public class STBLResource : ViewModelBase
	{
		private ulong _key;
		private string _value;

		public ulong Key
		{
			get => _key;
			set => RaiseAndSetIfChanged(ref _key, value);
		}

		public string Value
		{
			get => _value;
			set => RaiseAndSetIfChanged(ref _value, value);
		}
	}
}
