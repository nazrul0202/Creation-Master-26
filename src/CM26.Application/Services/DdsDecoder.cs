using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CM26.Application.Services;

/// <summary>
/// Minimal, self-contained DDS (DirectDraw Surface) decoder supporting the BC formats actually
/// present in the local FC26 asset set. Only BC3/DXT5 (and BC1/DXT1 for safety) are decoded;
/// anything else is reported as unsupported rather than mis-decoded. Pure managed code — no
/// native DirectXTex dependency. Read-only; never writes to the source file.
/// </summary>
internal static class DdsDecoder
{
    private const uint DdsMagic = 0x20534444; // "DDS "
    private const uint DdpfFourCc = 0x4;
    private const uint DdpfRgb = 0x40;
    private const uint DdpfAlphaPixels = 0x1;

    private static uint FourCc(string s) =>
        (uint)(s[0] | (s[1] << 8) | (s[2] << 16) | (s[3] << 24));

    public struct DdsInfo
    {
        public int Width;
        public int Height;
        public int MipLevels;
        public string Format;      // "DXT1", "DXT5", "UNCOMPRESSED", "UNSUPPORTED"
        public bool HasAlpha;
        public bool IsSupported;
        public int DataOffset;     // offset of first mip level
    }

    /// <summary>Parse the 128-byte DDS header. Returns false if not a valid DDS.</summary>
    public static bool TryReadHeader(byte[] bytes, out DdsInfo info)
    {
        info = default;
        if (bytes.Length < 128) return false;
        if (BitConverter.ToUInt32(bytes, 0) != DdsMagic) return false;
        // header size must be 124
        if (BitConverter.ToUInt32(bytes, 4) != 124) return false;

        info.Height = BitConverter.ToInt32(bytes, 12);
        info.Width = BitConverter.ToInt32(bytes, 16);
        info.MipLevels = Math.Max(1, BitConverter.ToInt32(bytes, 28));
        uint pfFlags = BitConverter.ToUInt32(bytes, 80);
        uint fourCc = BitConverter.ToUInt32(bytes, 84);
        info.DataOffset = 128;
        info.HasAlpha = (pfFlags & DdpfAlphaPixels) != 0;

        if ((pfFlags & DdpfFourCc) != 0)
        {
            if (fourCc == FourCc("DXT5")) { info.Format = "DXT5"; info.HasAlpha = true; info.IsSupported = true; }
            else if (fourCc == FourCc("DXT1")) { info.Format = "DXT1"; info.IsSupported = true; }
            else if (fourCc == FourCc("DX10")) { info.Format = "DX10"; info.IsSupported = false; }
            else { info.Format = "FOURCC:" + FourCcToString(fourCc); info.IsSupported = false; }
        }
        else if ((pfFlags & DdpfRgb) != 0)
        {
            uint rgbBitCount = BitConverter.ToUInt32(bytes, 88);
            // Support 32bpp A8R8G8B8 / X8R8G8B8 only (rare in this set).
            if (rgbBitCount == 32) { info.Format = "A8R8G8B8"; info.IsSupported = true; info.HasAlpha = (pfFlags & DdpfAlphaPixels) != 0; }
            else { info.Format = $"RGB{rgbBitCount}"; info.IsSupported = false; }
        }
        else
        {
            info.Format = "UNKNOWN";
            info.IsSupported = false;
        }
        return info.Width > 0 && info.Height > 0;
    }

    private static string FourCcToString(uint v) =>
        new(new[] { (char)(v & 0xFF), (char)((v >> 8) & 0xFF), (char)((v >> 16) & 0xFF), (char)((v >> 24) & 0xFF) });

    /// <summary>Decode mip level 0 to a 32bpp ARGB <see cref="Bitmap"/>. Caller owns the bitmap.</summary>
    public static Bitmap? DecodeToBitmap(byte[] bytes, in DdsInfo info, CancellationToken ct)
    {
        if (!info.IsSupported) return null;
        int w = info.Width, h = info.Height;
        if (w <= 0 || h <= 0 || w > 8192 || h > 8192) return null;
        var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        var rect = new Rectangle(0, 0, w, h);
        var data = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        bool unlocked = false;
        try
        {
            int stride = data.Stride;
            if (stride <= 0) { bmp.UnlockBits(data); unlocked = true; bmp.Dispose(); return null; }
            byte[] dest = new byte[stride * h];
            switch (info.Format)
            {
                case "DXT5": DecodeBc3(bytes, info.DataOffset, w, h, dest, stride, ct); break;
                case "DXT1": DecodeBc1(bytes, info.DataOffset, w, h, dest, stride, ct); break;
                case "A8R8G8B8": CopyBgra32(bytes, info.DataOffset, w, h, dest, stride); break;
                default: bmp.UnlockBits(data); unlocked = true; bmp.Dispose(); return null;
            }
            Marshal.Copy(dest, 0, data.Scan0, dest.Length);
        }
        catch (System.AccessViolationException)
        {
            if (!unlocked) { try { bmp.UnlockBits(data); } catch { } }
            bmp.Dispose();
            return null;
        }
        finally
        {
            if (!unlocked) bmp.UnlockBits(data);
        }
        return bmp;
    }

    private static void CopyBgra32(byte[] src, int srcOffset, int w, int h, byte[] dest, int stride)
    {
        // DDS A8R8G8B8 is stored B,G,R,A little-endian — same byte order as 32bppArgb memory.
        for (int y = 0; y < h; y++)
        {
            int s = srcOffset + y * w * 4;
            int d = y * stride;
            Array.Copy(src, s, dest, d, w * 4);
        }
    }

    // Blittable RGBA pixel (avoids Span<Color>, which is not allowed for managed Color).
    private struct Rgba { public byte R, G, B, A; }

    // ---- BC1 / DXT1 ----
    private static void DecodeBc1(byte[] src, int srcOffset, int w, int h, byte[] dest, int stride, CancellationToken ct)
    {
        int blocksX = (w + 3) / 4, blocksY = (h + 3) / 4;
        Span<Rgba> palette = stackalloc Rgba[4];
        for (int by = 0; by < blocksY; by++)
        {
            ct.ThrowIfCancellationRequested();
            for (int bx = 0; bx < blocksX; bx++)
            {
                int blockOffset = srcOffset + (by * blocksX + bx) * 8;
                if (blockOffset + 8 > src.Length) return;
                ushort c0 = BitConverter.ToUInt16(src, blockOffset);
                ushort c1 = BitConverter.ToUInt16(src, blockOffset + 2);
                uint bits = BitConverter.ToUInt32(src, blockOffset + 4);
                palette[0] = From565(c0);
                palette[1] = From565(c1);
                bool opaque = c0 > c1;
                if (opaque)
                {
                    palette[2] = Lerp(palette[0], palette[1], 2, 1, 3);
                    palette[3] = Lerp(palette[0], palette[1], 1, 2, 3);
                }
                else
                {
                    palette[2] = Lerp(palette[0], palette[1], 1, 1, 2);
                    palette[3] = new Rgba { R = 0, G = 0, B = 0, A = 0 };
                }
                for (int py = 0; py < 4; py++)
                {
                    int y = by * 4 + py; if (y >= h) break;
                    for (int px = 0; px < 4; px++)
                    {
                        int x = bx * 4 + px; if (x >= w) break;
                        int idx = (int)((bits >> (2 * (4 * py + px))) & 0x3);
                        WritePixel(dest, stride, x, y, palette[idx]);
                    }
                }
            }
        }
    }

    // ---- BC3 / DXT5 ----
    private static void DecodeBc3(byte[] src, int srcOffset, int w, int h, byte[] dest, int stride, CancellationToken ct)
    {
        int blocksX = (w + 3) / 4, blocksY = (h + 3) / 4;
        Span<Rgba> palette = stackalloc Rgba[4];
        Span<byte> alpha = stackalloc byte[8];
        for (int by = 0; by < blocksY; by++)
        {
            ct.ThrowIfCancellationRequested();
            for (int bx = 0; bx < blocksX; bx++)
            {
                int blockOffset = srcOffset + (by * blocksX + bx) * 16;
                if (blockOffset + 16 > src.Length) return;

                // alpha block (8 bytes)
                byte a0 = src[blockOffset], a1 = src[blockOffset + 1];
                alpha[0] = a0; alpha[1] = a1;
                if (a0 > a1)
                {
                    for (int i = 1; i <= 6; i++) alpha[i + 1] = (byte)(((7 - i) * a0 + i * a1) / 7);
                }
                else
                {
                    for (int i = 1; i <= 4; i++) alpha[i + 1] = (byte)(((5 - i) * a0 + i * a1) / 5);
                    alpha[6] = 0; alpha[7] = 255;
                }
                ulong alphaBits = (ulong)BitConverter.ToUInt64(src, blockOffset) >> 16; // 48-bit indices

                // colour block (next 8 bytes)
                ushort c0 = BitConverter.ToUInt16(src, blockOffset + 8);
                ushort c1 = BitConverter.ToUInt16(src, blockOffset + 10);
                uint bits = BitConverter.ToUInt32(src, blockOffset + 12);
                palette[0] = From565(c0);
                palette[1] = From565(c1);
                palette[2] = Lerp(palette[0], palette[1], 2, 1, 3);
                palette[3] = Lerp(palette[0], palette[1], 1, 2, 3);

                for (int py = 0; py < 4; py++)
                {
                    int y = by * 4 + py; if (y >= h) break;
                    for (int px = 0; px < 4; px++)
                    {
                        int x = bx * 4 + px; if (x >= w) break;
                        int pi = 4 * py + px;
                        int cidx = (int)((bits >> (2 * pi)) & 0x3);
                        int aidx = (int)((alphaBits >> (3 * pi)) & 0x7);
                        var c = palette[cidx];
                        c.A = alpha[aidx];
                        WritePixel(dest, stride, x, y, c);
                    }
                }
            }
        }
    }

    private static Rgba From565(ushort v) => new()
    {
        R = (byte)(((v >> 11) & 0x1F) * 255 / 31),
        G = (byte)(((v >> 5) & 0x3F) * 255 / 63),
        B = (byte)((v & 0x1F) * 255 / 31),
        A = 255,
    };

    private static Rgba Lerp(Rgba c0, Rgba c1, int w0, int w1, int div) => new()
    {
        R = (byte)((c0.R * w0 + c1.R * w1) / div),
        G = (byte)((c0.G * w0 + c1.G * w1) / div),
        B = (byte)((c0.B * w0 + c1.B * w1) / div),
        A = 255,
    };

    private static void WritePixel(byte[] dest, int stride, int x, int y, in Rgba c)
    {
        int o = y * stride + x * 4;
        dest[o + 0] = c.B;
        dest[o + 1] = c.G;
        dest[o + 2] = c.R;
        dest[o + 3] = c.A;
    }
}
