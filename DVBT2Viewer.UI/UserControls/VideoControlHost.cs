using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace DVBT2Viewer.UI.UserControls
{
    #region HwndHost descendant - video content renderer
    public class VideoControlHost : HwndHost
    {
        #region WindowPositionChangedCommand command
        public static readonly DependencyProperty WindowPositionChangedCommandProperty =
             DependencyProperty.Register("WindowPositionChangedCommand",
             typeof(ICommand),
             typeof(VideoControlHost));

        public ICommand WindowPositionChangedCommand
        {
            get { return (ICommand)GetValue(WindowPositionChangedCommandProperty); }
            set { SetValue(WindowPositionChangedCommandProperty, value); }
        }
        #endregion

        #region RepaintCommand command
        public static readonly DependencyProperty RepaintCommandProperty =
             DependencyProperty.Register("RepaintCommand",
             typeof(ICommand),
             typeof(VideoControlHost));

        public ICommand RepaintCommand
        {
            get { return (ICommand)GetValue(RepaintCommandProperty); }
            set { SetValue(RepaintCommandProperty, value); }
        }
        #endregion

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var hostHeight = (int)ActualHeight;
            var hostWidth = (int)ActualWidth;

            var hwndHost = CreateWindowEx(0, "static", "",
                WS_CHILD | WS_VISIBLE,
                0, 0,
                hostHeight, hostWidth,
                hwndParent.Handle,
                (IntPtr)HOST_ID,
                IntPtr.Zero,
                0);

            return new HandleRef(this, hwndHost);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }

        protected override void OnWindowPositionChanged(Rect rcBoundingBox)
        {
            if (WindowPositionChangedCommand != null)
            {
                WindowPositionChangedCommand.Execute(rcBoundingBox);
            }

            base.OnWindowPositionChanged(rcBoundingBox);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Process WM_PAINT windows message
            if (msg == WM_PAINT && RepaintCommand != null)
                RepaintCommand.Execute(null);

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle,
        string lpszClassName,
        string lpszWindowName,
        int style,
        int x, int y,
        int width, int height,
        IntPtr hwndParent,
        IntPtr hMenu,
        IntPtr hInst,
        [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Auto)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        internal const int WS_CHILD = 0x40000000;
        internal const int WS_VISIBLE = 0x10000000;
        internal const int HOST_ID = 0x00000002;
        internal const int WM_PAINT = 0x000F;
    }
    #endregion
}
