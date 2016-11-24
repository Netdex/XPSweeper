using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XPSweeper.Strategy;

namespace XPSweeper
{
    public partial class XPSweeper : Form
    {
        public XPSweeper()
        {
            InitializeComponent();
        }




        private void XPSweeper_Load(object sender, EventArgs e)
        {
            DoInit();
            lblDelay.Text = $"{timer.Interval} ms";
            trackBarDelay.Value = timer.Interval;
        }
        private const string ProcessName = "WinMine__XP";
        private static readonly Point MagicPoint = new Point(9, 3);
        private static Rectangle FocusRectangle;
        private static readonly Font DebugFont = new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular);
        private static readonly int[,] Colors =
        {
            { 0xC0, 0xC0, 0xC0 }, { 0xFF, 0x00, 0x00 }, { 0x00, 0x80, 0x00 }, { 0x00, 0x00, 0xFF },
            { 0x80, 0x00, 0x00 }, { 0x00, 0x00, 0x80 }, { 0x80, 0x80, 0x00 }, { 0x00, 0x00, 0x00 },
            { 0x96, 0x96, 0x96 }
        };
        private static readonly Brush[] VizBrushes =
        {
            Brushes.DarkGray, Brushes.Blue, Brushes.Green, Brushes.Red, Brushes.DarkBlue,
            Brushes.DarkRed, Brushes.DarkCyan, Brushes.Black, Brushes.Gray
        };

        private Process _Process;
        public DirectBitmap Screen;
        public DirectBitmap GfxBuf;
        private int BWidth, BHeight;
        /*
         * >0 : Adjacent mine count
         *  0 : Empty
         * -1 : Unknown
         * -2 : Known mine
         * -3 : Known safe
         */
        private int[,] mfArr;
        private const int PixelSize = 16;
        private Memory.RECT WindowLocation;

        private void DoInit()
        {
            _Process = Process.GetProcessesByName(ProcessName).FirstOrDefault();
            Memory.SetForegroundWindow(_Process.MainWindowHandle);
            Memory.RECT rect;
            Memory.GetWindowRect(_Process.MainWindowHandle, out rect);
            Screen?.Dispose();
            Screen = new DirectBitmap(rect.Width, rect.Height);
            FocusRectangle = new Rectangle(
                   3 + 12, 112,
                   Screen.Width - 3 - 3 - 12 - 8,
                   Screen.Height - 112 - 8 - 3);
            GfxBuf?.Dispose();
            GfxBuf = new DirectBitmap(Screen.Width, Screen.Height);
            DoCapture();
            BWidth = FocusRectangle.Width / PixelSize;
            BHeight = FocusRectangle.Height / PixelSize;
            mfArr = new int[BWidth, BHeight];
            for (int y = 0; y < BHeight; y++)
                for (int x = 0; x < BWidth; x++)
                    mfArr[x, y] = -1;
        }

        private void DoCapture()
        {
            Memory.SetForegroundWindow(_Process.MainWindowHandle);
            Memory.GetWindowRect(_Process.MainWindowHandle, out WindowLocation);
            Screen.TakeScreenshot(WindowLocation);
            Array.Copy(Screen.Bits, 0, GfxBuf.Bits, 0, Screen.Bits.Length);
            GfxBuf.Graphics.DrawRectangle(Pens.Black, FocusRectangle);
        }

        private void DoProcess()
        {
            for (int y = 0; y < BHeight; y++)
            {
                for (int x = 0; x < BWidth; x++)
                {
                    if (mfArr[x, y] == -1 || mfArr[x, y] == -3)
                    {
                        int dx = FocusRectangle.X + x * PixelSize;
                        int dy = FocusRectangle.Y + y * PixelSize;
                        int ax = dx + MagicPoint.X;
                        int ay = dy + MagicPoint.Y;

                        int id = -9999;
                        for (int i = 0; i < Colors.GetLength(0); i++)
                        {
                            if (
                                Screen.Bits[(ay * Screen.Width + ax) * 4 + 0] == Colors[i, 0]
                                && Screen.Bits[(ay * Screen.Width + ax) * 4 + 1] == Colors[i, 1]
                                && Screen.Bits[(ay * Screen.Width + ax) * 4 + 2] == Colors[i, 2])
                            {
                                id = i;
                                break;
                            }
                        }
                        if (id == 0 && Screen.Bits[(dy * Screen.Width + dx) * 4 + 0] == 0xFF)
                            id = -1;

                        mfArr[x, y] = id;
                    }
                }
            }
        }

        private void DoUpdate()
        {
            if (!MFNonAmbiguous.IdentifyAdjMines(mfArr))
            {

                if (!MFDeductive.DeduceMineLocation(mfArr))
                {
                    GfxBuf.Graphics.DrawString("AMBIGUOUS", DebugFont, Brushes.Red, 10, 50);
                    btnStop_Click(null, null);
                }
                else
                    GfxBuf.Graphics.DrawString("DEDUCTIVE", DebugFont, Brushes.Yellow, 10, 50);
            }
            else
                GfxBuf.Graphics.DrawString("NON-AMBIGUOUS", DebugFont, Brushes.Green, 10, 50);
            MFNonAmbiguous.ClearKnown(mfArr);
        }

        private void DoVisualization()
        {
            for (int y = 0; y < BHeight; y++)
            {
                for (int x = 0; x < BWidth; x++)
                {
                    int dx = FocusRectangle.X + x * PixelSize;
                    int dy = FocusRectangle.Y + y * PixelSize;

                    GfxBuf.Graphics.FillRectangle(
                        mfArr[x, y] == -2 ? Brushes.DarkRed :
                        mfArr[x, y] == -3 ? Brushes.Green :
                        Brushes.White,
                        dx, dy, PixelSize, PixelSize);
                    GfxBuf.Graphics.DrawRectangle(Pens.Black, dx, dy, PixelSize, PixelSize);
                    string s;
                    switch (mfArr[x, y])
                    {
                        case -3:
                            s = "~";
                            break;
                        case -2:
                            s = "!";
                            break;
                        case -1:
                            s = "X";
                            break;
                        case 0:
                            s = ".";
                            break;
                        default:
                            s = mfArr[x, y] + "";
                            break;
                    }
                    GfxBuf.Graphics.DrawString(s,
                        DebugFont, mfArr[x, y] >= 0 ? VizBrushes[mfArr[x, y]] : Brushes.DarkGray,
                        dx + 3, dy + 1);
                }
            }
            pictureBox.Image = GfxBuf.Bitmap;
        }

        private void DoAction()
        {
            int sx = WindowLocation.Left + FocusRectangle.X;
            int sy = WindowLocation.Top + FocusRectangle.Y;

            for (int y = 0; y < BHeight; y++)
            {
                for (int x = 0; x < BWidth; x++)
                {
                    if (mfArr[x, y] == -3)
                    {
                        MouseOperations.SetCursorPosition(sx + x * PixelSize, sy + y * PixelSize);
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftDown);
                        MouseOperations.MouseEvent(MouseOperations.MouseEventFlags.LeftUp);
                    }
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            DoInit();
            DoVisualization();
        }

        private void trackBarDelay_Scroll(object sender, EventArgs e)
        {
            timer.Interval = trackBarDelay.Value;
            lblDelay.Text = $"{timer.Interval}ms";
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            timer.Start();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer.Stop();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DoCapture();
            DoProcess();
            DoUpdate();
            DoVisualization();
            DoAction();
        }


    }
}
