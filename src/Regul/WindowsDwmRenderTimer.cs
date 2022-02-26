#region

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Rendering;

#endregion

namespace Regul
{
    public class WindowsDwmRenderTimer : IRenderTimer
    {
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

        public event Action<TimeSpan> Tick;

        [DllImport("Dwmapi.dll")]
        private static extern int DwmFlush();
    }
}