using System;
using System.IO;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    public static extern bool BitBlt(IntPtr hDestDC, int xDest, int yDest, int width, int height, IntPtr hSrcDC, int xSrc, int ySrc, uint dwRop);

    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [DllImport("gdi32.dll")]
    public static extern int DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern int DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern int GetDIBits(IntPtr hDC, IntPtr hBitmap, uint uStartScan, uint cScanLines, byte[] lpvBits, ref BITMAPINFO lpbmi, uint uUsage);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    static void Main()
    {
        int ScreenWidth = GetSystemMetrics(0); // SM_CXSCREEN
        int ScreenHeight = GetSystemMetrics(1); // SM_CYSCREEN

        IntPtr hDC = GetDC(GetDesktopWindow());
        IntPtr hMemDC = CreateCompatibleDC(hDC);

        IntPtr hBitmap = CreateCompatibleBitmap(hDC, ScreenWidth, ScreenHeight);
        SelectObject(hMemDC, hBitmap);

        // Forgot this one first D:, its here we take the actual screenshot
        BitBlt(hMemDC, 0, 0, ScreenWidth, ScreenHeight, hDC, 0, 0, 0x00CC0020);

        BITMAPINFO bmi = new BITMAPINFO();
        bmi.biSize = (uint)Marshal.SizeOf(bmi);
        bmi.biWidth = ScreenWidth;
        bmi.biHeight = -ScreenHeight; // Use negative value to indicate upside-down bitmap
        bmi.biPlanes = 1;
        bmi.biBitCount = 32;

        byte[] buffer = new byte[ScreenWidth * ScreenHeight * 4]; // Assuming 32-bit color depth

        GetDIBits(hMemDC, hBitmap, 0, (uint)ScreenHeight, buffer, ref bmi, 0);

        DeleteObject(hBitmap);
        DeleteDC(hMemDC);
        ReleaseDC(GetDesktopWindow(), hDC);

        // Save the captured screenshot as a BMP file
        SaveAsBmp(buffer, ScreenWidth, ScreenHeight, "screenshot.bmp");
        Console.ReadKey(true);
    }


    // Something like this if you instead wish to transfer the data
    static byte[] CreateBmpFromBuffer(byte[] imageData, int width, int height)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            // Write BMP header
            writer.Write('B');
            writer.Write('M');
            writer.Write(54 + imageData.Length);
            writer.Write(0);
            writer.Write(54);

            // Write DIB header
            writer.Write(40);
            writer.Write(width);
            writer.Write(height);
            writer.Write((ushort)1);
            writer.Write((ushort)32);
            writer.Write(0);
            writer.Write(imageData.Length);
            writer.Write(2835); // Horizontal resolution (pixels per meter)
            writer.Write(2835); // Vertical resolution (pixels per meter)
            writer.Write(0);
            writer.Write(0);

            // Write pixel data (from the buffer)
            for (int y = height - 1; y >= 0; y--) // BMP stores pixels bottom to top
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4; // 4 bytes per pixel (32-bit)
                    writer.Write(imageData[index + 2]); // Blue channel
                    writer.Write(imageData[index + 1]); // Green channel
                    writer.Write(imageData[index + 0]); // Red channel
                    writer.Write(imageData[index + 3]); // Alpha channel
                }
            }

            return ms.ToArray();
        }
    }

    static void SaveAsBmp(byte[] imageData, int width, int height, string fileName)
    {
        using (FileStream fs = new FileStream(fileName, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            // Write BMP header
            writer.Write('B');
            writer.Write('M');
            writer.Write(54 + imageData.Length);
            writer.Write(0);
            writer.Write(54);

            // Write DIB header
            writer.Write(40);
            writer.Write(width);
            writer.Write(height);
            writer.Write((ushort)1);
            writer.Write((ushort)32);
            writer.Write(0);
            writer.Write(imageData.Length);
            writer.Write(2835); // Horizontal resolution (pixels per meter)
            writer.Write(2835); // Vertical resolution (pixels per meter)
            writer.Write(0);
            writer.Write(0);

            // Write pixel data (from the buffer)
            for (int y = height - 1; y >= 0; y--) // BMP stores pixels bottom to top
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width + x) * 4; // 4 bytes per pixel (32-bit)
                    writer.Write(imageData[index + 2]); // Blue channel
                    writer.Write(imageData[index + 1]); // Green channel
                    writer.Write(imageData[index + 0]); // Red channel
                    writer.Write(imageData[index + 3]); // Alpha channel
                }
            }
        }
    }
}
