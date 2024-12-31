using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MoveType = FarmingMacro.MoveTypeChecker.MoveType;

namespace FarmingMacro
{
    internal class MacroBase
    {

        // Constants for mouse event flags.
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        private const int KEYEVENTF_KEYDOWN = 0x0000; // Key down event
        private const int KEYEVENTF_KEYUP = 0x0002; // Key up event


        private static Stopwatch rap = new Stopwatch();
        private static Stopwatch all = new Stopwatch();
        private static MoveType befMove;
        private static MoveType move;

        private static bool isRunning; // Flag to indicate if the macro is running
        public MacroBase()
        {
            isRunning = false;
            rap.Reset();
            all.Reset();
        }
        public async static void FullAutomaticalyCropCollecter(string cropType)
        {
            if (isRunning)
            {
                isRunning = false;
                return;
            }

            isRunning = true;
            await Task.Run(() =>
            {
                Console.Clear();
                all.Start();
                rap.Start();
                move = MoveTypeChecker.GetMoveTypeFromPixels(cropType);
                while (isRunning)
                {
                    switch (move)
                    {
                        case MoveType.warp:
                        case MoveType.left:
                        case MoveType.right:
                        case MoveType.forward:
                        case MoveType.back:
                            BaseMove(move, cropType);
                            break;
                        case MoveType.nomatch:
                            move = befMove switch
                            {
                                MoveType.right => MoveType.right,   //再起動のため
                                MoveType.left => MoveType.left,
                                _ => MoveTypeChecker.GetMoveTypeFromPixels(cropType)
                            };
                            WaitRandom(500, 501);
                            break;
                    }
                }
            }).ConfigureAwait(true);
        }

        private static void BaseMove(MoveType _move, string cropType)
        {
            befMove = _move;
            move = MoveType.nomatch;
            rap.Restart();
            NativeMethods.keybd_event((byte)_move, 0, KEYEVENTF_KEYDOWN, 0);
            NativeMethods.mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            while (rap.Elapsed < new TimeSpan(0, 0, 2, 30) && isRunning)
            {
                WaitRandom(500, 501);
                move = MoveTypeChecker.GetMoveTypeFromPixels(cropType);
                if (move != MoveType.nomatch)
                    break;
            }
            NativeMethods.keybd_event((byte)_move, 0, KEYEVENTF_KEYUP, 0);
            NativeMethods.mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void StopMacro()
        {
            isRunning = false;
            rap.Stop();
            all.Stop();
        }

        private static void WaitRandom(int minimum, int limit = -1)
        {
            if (limit == -1)
                limit = minimum + 1;
            CheckReboot();
            LogHistory();
            string foreTitle = NativeMethods.GetCursorWindowTitle();
            if (foreTitle != "Minecraft 1.8.9")
            {
                isRunning = false;
            }
            Random random = new Random();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int lim = random.Next(minimum, limit);
            while (stopwatch.Elapsed < new TimeSpan(0, 0, 0, 0, lim) && isRunning) { }
        }

        public static void CheckReboot()
        {
            string source = $@"C:\Users\{Environment.UserName}\AppData\Roaming\.minecraft\versions\1.8.9-forge1.8.9-11.15.1.2318-1.8.9\logs\latest.log";
            string dest = $@"{Environment.CurrentDirectory}\log\log.txt";
            File.Copy(source, dest, true);

            string fileTxt = File.ReadAllText(dest);
            DateTime date = DateTime.Now;

            string rebootPattern = $@"\[{date.Hour.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}:{date.Minute.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}:[0-9]+\].*This server will restart soon:";
            string disconnectPattern = $@"\[{date.Hour.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}:{date.Minute.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}:{date.Second / 10}[0-9]\].*Detected leaving skyblock!";

            if (Regex.IsMatch(fileTxt, rebootPattern) ||
                Regex.IsMatch(fileTxt, disconnectPattern))
            {
                StopMacro();
            }
        }
        private static void LogHistory()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"NOW : {DateTime.Now}");
            string befStr = befMove switch
            {
                MoveType.nomatch => "nomatch",
                MoveType.right => "right  ",
                MoveType.left => "left   ",
                MoveType.warp => "warp   ",
                MoveType.forward => "forward",
                MoveType.back => "back   ",
                _ => "NOTYPE"
            };
            string moveStr = move switch
            {
                MoveType.nomatch => "nomatch",
                MoveType.right => "right  ",
                MoveType.left => "left   ",
                MoveType.warp => "warp   ",
                MoveType.forward => "forward",
                MoveType.back => "back   ",
                _ => "NOTYPE"
            };
            Console.WriteLine($"BMV : {befStr}");
            Console.WriteLine($"MOV : {move}");
            Console.WriteLine($"ALL : {all.Elapsed}");
            Console.WriteLine($"RAP : {rap.Elapsed}");
        }
    }
}
