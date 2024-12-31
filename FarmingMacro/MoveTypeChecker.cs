using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FarmingMacro
{
    internal sealed class MoveTypeChecker
    {
        //それぞれの数字は仮想コード
        internal enum MoveType
        {
            nomatch,
            right = 0x44,
            left = 0x41,
            warp = 0x30,
            forward = 0x57,
            back = 0x53
        }
        private static byte[] GetCoordByteArray(int threashold = 44)
        {
            Bitmap coordBit = GetSplitedHiddenWindow();

            int pos = 0;
            byte red = 229;
            byte green = 191;
            byte blue = 82;
            int diff = 0;
            BitmapData data = coordBit.LockBits(new Rectangle(0, 0, coordBit.Width, coordBit.Height), ImageLockMode.ReadWrite, coordBit.PixelFormat);
            byte[] buf = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);
            for (int h = 0; h < data.Height; h++)
            {
                for (int w = 0; w < data.Width; w++)
                {
                    pos = h * data.Stride + w * 4;//Format32bppArgbだと*4
                    diff = (Math.Abs(blue - buf[pos]) + Math.Abs(green - buf[pos + 1]) + Math.Abs(red - buf[pos + 2])) / 3;
                    if (diff > threashold)
                    {
                        buf[pos] = 255;
                        buf[pos + 1] = 255;
                        buf[pos + 2] = 255;
                    }
                    else
                    {
                        buf[pos] = 0;
                        buf[pos + 1] = 0;
                        buf[pos + 2] = 0;
                    }
                }
            }
            coordBit.UnlockBits(data);
            coordBit.Dispose();
            return buf;
        }
        private static Bitmap To32bppArg(string filePath)
        {
            Bitmap bmp = new Bitmap(filePath);
            if (bmp.PixelFormat == PixelFormat.Format32bppArgb) return bmp;

            var bmp2 = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
            bmp.Dispose();

            using (var g = Graphics.FromImage(bmp2))
            {
                g.PageUnit = GraphicsUnit.Pixel;
                g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);//DrawImageUnscaledだと表示したときに少し変になることがある
            };

            return bmp2;
        }
        private static int GetImageDiff(Bitmap bitA, byte[] bufB)
        {
            int diff = 0;
            int pos = 0;
            int result = 0;
            BitmapData dataA = bitA.LockBits(new Rectangle(0, 0, bitA.Width, bitA.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            byte[] bufA = new byte[dataA.Stride * dataA.Height];
            Marshal.Copy(dataA.Scan0, bufA, 0, bufA.Length);
            for (int y = 0; y < dataA.Height; y++)
            {
                for (int x = 0; x < dataA.Width; x++)
                {
                    pos = y * dataA.Stride + x * 4;
                    diff = (Math.Abs(bufA[pos] - bufB[pos]) + Math.Abs(bufA[pos + 2] - bufB[pos + 2]) + Math.Abs(bufA[pos + 2] - bufB[pos + 2])) / 3;
                    if (diff != 0)
                    {
                        result++;
                    }
                }
            }
            bitA.UnlockBits(dataA);
            bitA.Dispose();
            return result;
        }

        public static MoveType GetMoveTypeFromPixels(string cropType)
        {
            int diff = 100000000;
            int fileNum = -1;
            if (!Directory.Exists($@"{Environment.CurrentDirectory}\images\farm\{cropType}"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("正しい作物が選択されていません");
                Console.ForegroundColor = ConsoleColor.White;
                return MoveType.nomatch;
            }
            foreach (string filePath in Directory.GetFiles($@"{Environment.CurrentDirectory}\images\farm\{cropType}"))
            {
                int temp = 0;
                using (Bitmap fileBit = To32bppArg(filePath))
                {
                    temp = GetImageDiff(fileBit, GetCoordByteArray());
                }
                if (diff > temp)
                {
                    diff = temp;
                    if (!int.TryParse(Path.GetFileNameWithoutExtension(filePath), out fileNum))
                    {
                        throw new FormatException();
                    }
                    if (temp < 2)
                    {
                        switch (cropType)
                        {
                            case "mushroom":
                                return fileNum switch
                                {
                                    0 => MoveType.right,
                                    2 => MoveType.right,
                                    4 => MoveType.right,
                                    6 => MoveType.warp,
                                    1 => MoveType.left,
                                    3 => MoveType.left,
                                    5 => MoveType.left,
                                    _ => MoveType.nomatch
                                };
                            case "pumpkin":
                            case "melon":
                                return fileNum switch
                                {
                                    0 => MoveType.left,
                                    1 => MoveType.right,
                                    2 => MoveType.left,
                                    3 => MoveType.forward,
                                    4 => MoveType.right,
                                    5 => MoveType.left,
                                    6 => MoveType.right,
                                    7 => MoveType.warp,
                                    _ => MoveType.nomatch
                                };
                            case "wart":
                            case "carrot":
                                return fileNum switch
                                {
                                    0 => MoveType.right,
                                    2 => MoveType.right,
                                    4 => MoveType.right,
                                    1 => MoveType.left,
                                    3 => MoveType.left,
                                    5 => MoveType.warp,
                                    _ => MoveType.nomatch
                                };
                            case "wheat":
                            case "potato":
                                return fileNum switch
                                {
                                    0 => MoveType.left,
                                    2 => MoveType.left,
                                    4 => MoveType.left,
                                    1 => MoveType.right,
                                    3 => MoveType.right,
                                    5 => MoveType.warp,
                                    _ => MoveType.nomatch
                                };
                            case "cocoa":
                                return fileNum switch
                                {
                                    0 => MoveType.forward,
                                    1 => MoveType.left,
                                    2 => MoveType.back,
                                    3 => MoveType.left,
                                    4 => MoveType.forward,
                                    5 => MoveType.left,
                                    6 => MoveType.back,
                                    7 => MoveType.left,
                                    8 => MoveType.forward,
                                    9 => MoveType.left,
                                    10 => MoveType.back,
                                    11 => MoveType.left,
                                    12 => MoveType.forward,
                                    13 => MoveType.left,
                                    14 => MoveType.back,
                                    15 => MoveType.left,
                                    16 => MoveType.forward,
                                    17 => MoveType.left,
                                    18 => MoveType.back,
                                    19 => MoveType.left,
                                    20 => MoveType.forward,
                                    21 => MoveType.left,
                                    22 => MoveType.back,
                                    23 => MoveType.left,
                                    24 => MoveType.forward,
                                    25 => MoveType.left,
                                    26 => MoveType.back,
                                    _ => MoveType.nomatch
                                };
                            default:
                                throw new ArgumentNullException(nameof(cropType), "正しいcropTypeが選択されていません");

                        }
                    }
                }
            }
            return MoveType.nomatch;
        }

        private static Bitmap GetSplitedHiddenWindow()
        {
            int xCorrector = 8 - 2;//黒枠線分 - 補正
            int yCorrector = 23 + 8;//タイトルバー+黒枠線分
            int rowCorrector = 42;//debugInfoの行補正

            //ウィンドウをキャプチャする
            Bitmap img = NativeMethods.GetHiddenWindow();

            //ここを調整 うまくいかぬTT
            Rectangle splintRect = new Rectangle(xCorrector, yCorrector + rowCorrector, 100, 13);
            Bitmap splintImg = img.Clone(splintRect, img.PixelFormat);
            img.Dispose();
            return splintImg;
        }
    }
}
