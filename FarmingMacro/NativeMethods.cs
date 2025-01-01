using System.Runtime.InteropServices;
using System.Text;

namespace FarmingMacro
{
    internal sealed partial class NativeMethods
    {
        //window系
        [LibraryImport("user32.dll")]
        private static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        private static partial int GetWindowTextLengthW(IntPtr hWnd);

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]

        private static partial int GetWindowTextW(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

        [LibraryImport("user32.dll")]
        private static partial IntPtr WindowFromPoint(POINT point);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        //Console系
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FreeConsole();

        [LibraryImport("kernel32.dll")]
        private static partial IntPtr GetConsoleWindow();

        [LibraryImport("user32.dll")]
        private static partial IntPtr GetSystemMenu(IntPtr hWnd, UInt32 bRevert);

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial UInt32 RemoveMenu(IntPtr hMenu, UInt32 nPosition, UInt32 wFlags);
        private const UInt32 SC_CLOSE = 0x0000F060;
        private const UInt32 SC_MAXIMIZE = 0xF030;
        private const UInt32 MF_BYCOMMAND = 0x0;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr GetStdHandle(int nStdHandle);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        const int STD_INPUT_HANDLE = -10;
        const uint ENABLE_QUICK_EDIT = 0x0040;

        [LibraryImport("kernel32.dll")]
        private static partial IntPtr GetLastError();

        //キーボード、マウス操作
        [LibraryImport("user32.dll", SetLastError = true)]
        internal static partial void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [LibraryImport("user32.dll")]
        internal static partial void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        private const int SWP_SHOWWINDOW = 0x0040;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
            public int width
            {
                get
                {
                    return Right - Left;
                }
            }
            public int height
            {
                get
                {
                    return Bottom - Top;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            internal POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            internal int x, y;
        }
        /// <summary>
        /// 整形済みのコンソールウィンドウをアタッチして最前面に固定する
        /// </summary>
        public static void AttachConsole()
        {
            AllocConsole();

            Console.Title = "自動作物回収機";
            Console.CursorVisible = false;
#pragma warning disable CA2000
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
#pragma warning restore CA2000
            Console.CancelKeyPress += Console_CancelKeyPress;
            Console.OutputEncoding = Encoding.UTF8;

            IntPtr consoleHandle = GetConsoleWindow();
            IntPtr closeMenuHandle = GetSystemMenu(consoleHandle, 0);

            if (RemoveMenu(closeMenuHandle, SC_CLOSE, MF_BYCOMMAND) == 0)
            {
                Console.WriteLine($"RemoveMenu関数が失敗しました。Error : {Marshal.GetLastWin32Error()}");
            }
            if (RemoveMenu(closeMenuHandle, SC_MAXIMIZE, MF_BYCOMMAND) == 0)
            {
                Console.WriteLine($"RemoveMenu関数が失敗しました。Error : {Marshal.GetLastWin32Error()}");
            }

            uint consoleMode = 0;
            IntPtr inputHandle = GetStdHandle(STD_INPUT_HANDLE);
            if (!GetConsoleMode(inputHandle, out consoleMode))
            {
                Console.WriteLine(GetLastError());
            }
            consoleMode &= ~ENABLE_QUICK_EDIT;
            SetConsoleMode(inputHandle, consoleMode);

            SetWindowPos(GetConsoleWindow(), -1, -7, 310, 250, 200, SWP_SHOWWINDOW);
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// ウィンドウのハンドルからタイトルを取得する
        /// </summary>
        /// <param name="hWnd">対象のハンドル</param>
        /// <returns></returns>
        public static string GetWindowTitleFromhWnd(IntPtr hWnd)
        {
            int titleLen = GetWindowTextLengthW(hWnd);
            int retCode = 0;
            titleLen = GetWindowTextLengthW(hWnd) + 1;
            char[] chars = new char[titleLen];
            retCode = GetWindowTextW(hWnd, chars, titleLen);
            return new string(chars).Replace("\0", "", StringComparison.Ordinal);
        }

        /// <summary>
        /// マウスカーソルの下にあるアクティブウィンドウのタイトルを取得する
        /// </summary>
        /// <returns></returns>

        public static string GetCursorWindowTitle()
        {
            POINT cursorPos = new POINT(Cursor.Position.X, Cursor.Position.Y);
            IntPtr hWnd = WindowFromPoint(cursorPos);
            return GetWindowTitleFromhWnd(hWnd);
        }

        public static Bitmap GetHiddenWindow()
        {
            IntPtr handle = GetForegroundWindow();

            //ウィンドウサイズ取得
            RECT rect;
            GetWindowRect(handle, out rect);

            //ウィンドウをキャプチャする
            Bitmap img = new Bitmap(rect.width, rect.height);

            Graphics memg = Graphics.FromImage(img);
            IntPtr dc = memg.GetHdc();
            PrintWindow(handle, dc, 0);

            memg.ReleaseHdc(dc);
            memg.Dispose();
            return img;
        }
    }
}
