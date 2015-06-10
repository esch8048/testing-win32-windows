using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Timers;
using System.Globalization;
using System.IO;

namespace WpfApplication2
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("user32.dll")]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        // Pinvoke declaration for ShowWindow
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        private IntPtr currentActiveWindow;
        private IntPtr thisWindow;

        public MainWindow()
        {
            InitializeComponent();
            currentActiveWindow = GetForegroundWindow();
            //get the process associated with this application
            //Process thisProcess = Process.GetCurrentProcess();

            //get the main window (this window) associated with the process
            //IntPtr handle = thisProcess.MainWindowHandle;
            //SetForegroundWindow(handle);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            thisWindow = new WindowInteropHelper(Window.GetWindow(this)).Handle;
            SetWindowPos(thisWindow, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            //start timer.
            Timer timer = new Timer(500);
            timer.Elapsed += updateActiveWindow;
            timer.Enabled = true;
        }

        private void updateActiveWindow(Object source, ElapsedEventArgs e)
        {
            IntPtr curr = GetForegroundWindow();
            if(curr != thisWindow && curr != IntPtr.Zero)
            {
                currentActiveWindow = curr;
            }
            Debug.WriteLine("Current Window: " + curr + " This window: " + thisWindow);
        }

        private void notepad_Click(object sender, RoutedEventArgs e)
        {
            Process[] p = Process.GetProcessesByName("notepad++");
            if (p.Length != 0)
            {
                //open first instance
                ShowWindow(p[0].MainWindowHandle, SW_SHOW);
                ShowWindow(p[0].MainWindowHandle, SW_SHOW);
                SetActiveWindow(p[0].MainWindowHandle);
            }
            else
            {
                Process.Start("C://Program Files (x86)//Notepad++//notepad++.exe");
            }
        }

        private void screenshot_Click(object sender, RoutedEventArgs e)
        {
            CaptureScreen();
        }

        public void CaptureScreen()
        {
            System.Drawing.Image imageCapture = CaptureWindow(currentActiveWindow);
            String path = "C:\\Users\\esch8_000\\Desktop\\WPFScreenshots\\";
            path += DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.CurrentUICulture.DateTimeFormat) + "\\" + DateTime.Now.ToString("HH", CultureInfo.CurrentUICulture.DateTimeFormat);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += "\\screenshot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff", CultureInfo.CurrentUICulture.DateTimeFormat) + ".png";
            imageCapture.Save(path, ImageFormat.Png);
        }

        public static System.Drawing.Image CaptureWindow(IntPtr handle)
        {

            IntPtr hdcSrc = GetWindowDC(handle);

            Rect windowRect = new Rect();
            GetWindowRect(handle, ref windowRect);

            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);

            IntPtr hOld = SelectObject(hdcDest, hBitmap);
            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, 13369376);
            SelectObject(hdcDest, hOld);
            DeleteDC(hdcDest);
            ReleaseDC(handle, hdcSrc);

            System.Drawing.Image image = System.Drawing.Image.FromHbitmap(hBitmap);
            DeleteObject(hBitmap);

            return image;
        }
    }
}
