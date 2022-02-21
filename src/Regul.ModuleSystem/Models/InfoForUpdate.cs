﻿using Onebeld.Extensions;
using System;

namespace Regul.ModuleSystem.Models
{
	public class InfoForUpdate : ViewModelBase
	{
		private Version _newVersion;
		private Version _minRegulVersion;
		private string _linkForDownload;
		private bool _itIsPossibleToUpdate = true;
		private bool _readyForUpdate;

		public Version NewVersion
		{
			get => _newVersion;
			set => RaiseAndSetIfChanged(ref _newVersion, value);
		}
		public Version MinRegulVersion
		{
			get => _minRegulVersion;
			set => RaiseAndSetIfChanged(ref _minRegulVersion, value);
		}
		public string LinkForDownload
		{
			get => _linkForDownload;
			set => RaiseAndSetIfChanged(ref _linkForDownload, value);
		}

		public bool ItIsPossibleToUpdate
		{
			get => _itIsPossibleToUpdate;
			set => RaiseAndSetIfChanged(ref _itIsPossibleToUpdate, value);
		}

		public bool ReadyForUpdate
		{
			get => _readyForUpdate;
			set => RaiseAndSetIfChanged(ref _readyForUpdate, value);
		}
	}
}
