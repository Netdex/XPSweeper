using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XPSweeper
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public byte[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }
        public Graphics Graphics { get; private set; }
        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new byte[width * height * 4];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            Graphics = Graphics.FromImage(Bitmap);
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            Graphics.Dispose();
            BitsHandle.Free();
        }

        public void TakeScreenshot(Memory.RECT rect)
        {
            Graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(rect.Width, rect.Height));
        }
    }
}
