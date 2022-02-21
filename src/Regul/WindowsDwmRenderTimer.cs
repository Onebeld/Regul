using Avalonia.Rendering;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Regul
{
	public class WindowsDwmRenderTimer : IRenderTimer
	{
		public event Action<TimeSpan> Tick;

		public WindowsDwmRenderTimer()
		{
			Thread renderTick = new Thread(() =>
			{
				Stopwatch sw = new Stopwatch();
				sw.Start();
				while (true)
				{
					DwmFlush();
					Tick?.Invoke(sw.Elapsed);
				}
			})
			{
				IsBackground = true,
				Priority = ThreadPriority.Highest
			};

			renderTick.Start();
		}

		[DllImport("Dwmapi.dll")]
		private static extern int DwmFlush();
	}
}
