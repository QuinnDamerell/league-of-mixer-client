using LeagueOfMixerClient.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfMixerClient.LeagueCore
{
    public class LeagueGrabber
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClientRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        private static extern IntPtr ClientToScreen(IntPtr hWnd, ref NativePoint rect);      

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int Left;
            public int Top;
        }

        private string m_processName;

        public LeagueGrabber(string processName)
        {
            m_processName = processName;
        }

        public Bitmap Capture()
        {
            // First, look for the league window.
            IntPtr leagueWnd = IntPtr.Zero;
            foreach (Process pList in Process.GetProcesses())
            {
                if (pList.ProcessName.Equals(m_processName))
                {
                    leagueWnd = pList.MainWindowHandle;
                }
            }
            if (leagueWnd == IntPtr.Zero)
            {
                Logger.Info("Failed to find League Window.");
                return null;
            }

            Rectangle bounds;
            try
            {
                // Get the window bound
                var rect = new Rect();
                if(GetClientRect(leagueWnd, ref rect) == IntPtr.Zero)
                {
                    throw new Exception("GetWindowRect call failed");
                }
                var point = new NativePoint()
                {
                    Top = rect.Top,
                    Left = rect.Left
                };
                if (ClientToScreen(leagueWnd, ref point) == IntPtr.Zero)
                {
                    throw new Exception("ClientToScreen call failed");
                }
                bounds = new Rectangle(point.Left, point.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
            catch(Exception e)
            {
                Logger.Error("Failed to get League window bounds", e);
                return null;
            }

            Bitmap result;
            try
            {
                // Create a bitmap
                result = new Bitmap(bounds.Width, bounds.Height);
                using (var g = Graphics.FromImage(result))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to copy bitmap for league.", e);
                return null;
            }

            return result;
        }

        public System.Drawing.Point CursorPosition
        {
            get;
            protected set;
        }
    }


}
