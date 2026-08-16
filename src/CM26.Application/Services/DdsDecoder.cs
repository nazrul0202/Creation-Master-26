using System.Buffers.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;

namespace CM26.Application.Services;

/// <summary>
/// DDS decoder for the complete set of DirectX texture formats currently present in FC26.
/// BC1-BC7 decoding is pure managed code; common uncompressed/HDR DXGI formats are converted
/// locally. Read-only; never writes to the source file.
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
        public uint DxgiFormat;
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
            else if (fourCc == FourCc("DX10"))
            {
                if (bytes.Length < 148) return false;
                info.DataOffset = 148;
                info.DxgiFormat = BitConverter.ToUInt32(bytes, 128);
                info.Format = DxgiName(info.DxgiFormat);
                info.IsSupported = IsSupportedDxgi(info.DxgiFormat);
                info.HasAlpha = info.DxgiFormat is 2 or 10 or 11 or 24 or 28 or 29 or 74 or 75 or 77 or 78 or 98 or 99;
            }
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

    private static bool IsSupportedDxgi(uint format) =>
        format is 2 or 10 or 11 or 24 or 28 or 29 or 35 or 56 or 61 or 67
            or 71 or 72 or 74 or 75 or 77 or 78 or 80 or 83 or 95 or 96 or 98 or 99;

    private static string DxgiName(uint format) => format switch
    {
        2 => "R32G32B32A32_FLOAT", 10 => "R16G16B16A16_FLOAT",
        11 => "R16G16B16A16_UNORM", 24 => "R10G10B10A2_UNORM",
        28 => "R8G8B8A8_UNORM", 29 => "R8G8B8A8_UNORM_SRGB",
        35 => "R16G16_UNORM", 56 => "R16_UNORM", 61 => "R8_UNORM",
        67 => "R9G9B9E5_SHAREDEXP", 71 => "BC1_UNORM", 72 => "BC1_UNORM_SRGB",
        74 => "BC2_UNORM", 75 => "BC2_UNORM_SRGB", 77 => "BC3_UNORM",
        78 => "BC3_UNORM_SRGB", 80 => "BC4_UNORM", 83 => "BC5_UNORM",
        95 => "BC6H_UF16", 96 => "BC6H_SF16", 98 => "BC7_UNORM",
        99 => "BC7_UNORM_SRGB", _ => $"DXGI_{format}",
    };

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
                default:
                    if (info.DxgiFormat != 0)
                        DecodeDxgi(bytes, info, dest, stride, ct);
                    else
                    {
                        bmp.UnlockBits(data); unlocked = true; bmp.Dispose(); return null;
                    }
                    break;
            }
            Marshal.Copy(dest, 0, data.Scan0, dest.Length);
        }

        finally
        {
            if (!unlocked) bmp.UnlockBits(data);
        }
        return bmp;
    }

    private static void DecodeDxgi(byte[] bytes, in DdsInfo info, byte[] dest, int stride, CancellationToken ct)
    {
        var payloadLength = bytes.Length - info.DataOffset;
        if (payloadLength <= 0) throw new InvalidDataException("DDS texture has no pixel payload.");
        var payload = new byte[payloadLength];
        Buffer.BlockCopy(bytes, info.DataOffset, payload, 0, payloadLength);
        var compression = info.DxgiFormat switch
        {
            71 or 72 => CompressionFormat.Bc1,
            74 or 75 => CompressionFormat.Bc2,
            77 or 78 => CompressionFormat.Bc3,
            80 => CompressionFormat.Bc4,
            83 => CompressionFormat.Bc5,
            95 => CompressionFormat.Bc6U,
            96 => CompressionFormat.Bc6S,
            98 or 99 => CompressionFormat.Bc7,
            _ => CompressionFormat.Unknown,
        };

        if (compression is CompressionFormat.Bc6U or CompressionFormat.Bc6S)
        {
            ct.ThrowIfCancellationRequested();
            var pixels = new BcDecoder().DecodeRawHdr(payload, info.Width, info.Height, compression);
            for (var i = 0; i < pixels.Length; i++)
            {
                if ((i & 0x3fff) == 0) ct.ThrowIfCancellationRequested();
                WritePixel(dest, stride, i % info.Width, i / info.Width, new Rgba
                {
                    R = FloatToDisplayByte(pixels[i].r),
                    G = FloatToDisplayByte(pixels[i].g),
                    B = FloatToDisplayByte(pixels[i].b), A = 255,
                });
            }
            return;
        }

        if (compression != CompressionFormat.Unknown)
        {
            ct.ThrowIfCancellationRequested();
            var pixels = new BcDecoder().DecodeRaw(payload, info.Width, info.Height, compression);
            for (var i = 0; i < pixels.Length; i++)
            {
                if ((i & 0x3fff) == 0) ct.ThrowIfCancellationRequested();
                var p = pixels[i];
                WritePixel(dest, stride, i % info.Width, i / info.Width,
                    new Rgba { R = p.r, G = p.g, B = p.b, A = p.a });
            }
            return;
        }

        DecodeUncompressedDxgi(payload, info.DxgiFormat, info.Width, info.Height, dest, stride, ct);
    }

    private static void DecodeUncompressedDxgi(
        byte[] src, uint format, int width, int height, byte[] dest, int stride, CancellationToken ct)
    {
        var bytesPerPixel = format switch
        {
            61 => 1, 56 => 2, 24 or 28 or 29 or 35 or 67 => 4,
            10 or 11 => 8, 2 => 16,
            _ => throw new NotSupportedException($"DXGI format {format} cannot be previewed."),
        };
        if ((long)width * height * bytesPerPixel > src.Length)
            throw new InvalidDataException("DDS uncompressed payload is truncated.");

        for (var y = 0; y < height; y++)
        {
            ct.ThrowIfCancellationRequested();
            for (var x = 0; x < width; x++)
            {
                var offset = (y * width + x) * bytesPerPixel;
                var color = format switch
                {
                    61 => Gray(src[offset]),
                    56 => Gray(ToByte(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset, 2)) / 65535f)),
                    28 or 29 => new Rgba { R = src[offset], G = src[offset + 1], B = src[offset + 2], A = src[offset + 3] },
                    24 => DecodeRgb10A2(BinaryPrimitives.ReadUInt32LittleEndian(src.AsSpan(offset, 4))),
                    35 => new Rgba
                    {
                        R = ToByte(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset, 2)) / 65535f),
                        G = ToByte(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset + 2, 2)) / 65535f), B = 0, A = 255,
                    },
                    67 => DecodeRgb9E5(BinaryPrimitives.ReadUInt32LittleEndian(src.AsSpan(offset, 4))),
                    10 => DecodeRgba16Float(src, offset),
                    11 => DecodeRgba16Unorm(src, offset),
                    2 => DecodeRgba32Float(src, offset),
                    _ => default,
                };
                WritePixel(dest, stride, x, y, color);
            }
        }
    }

    private static Rgba Gray(byte value) => new() { R = value, G = value, B = value, A = 255 };

    private static Rgba DecodeRgb10A2(uint value) => new()
    {
        R = ToByte((value & 0x3ff) / 1023f), G = ToByte(((value >> 10) & 0x3ff) / 1023f),
        B = ToByte(((value >> 20) & 0x3ff) / 1023f), A = ToByte(((value >> 30) & 0x3) / 3f),
    };

    private static Rgba DecodeRgb9E5(uint value)
    {
        var scale = MathF.Pow(2, ((value >> 27) & 0x1f) - 24);
        return new Rgba
        {
            R = FloatToDisplayByte((value & 0x1ff) * scale),
            G = FloatToDisplayByte(((value >> 9) & 0x1ff) * scale),
            B = FloatToDisplayByte(((value >> 18) & 0x1ff) * scale), A = 255,
        };
    }

    private static Rgba DecodeRgba16Float(byte[] src, int offset) => new()
    {
        R = FloatToDisplayByte((float)BitConverter.UInt16BitsToHalf(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset, 2)))),
        G = FloatToDisplayByte((float)BitConverter.UInt16BitsToHalf(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset + 2, 2)))),
        B = FloatToDisplayByte((float)BitConverter.UInt16BitsToHalf(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset + 4, 2)))),
        A = ToByte((float)BitConverter.UInt16BitsToHalf(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset + 6, 2)))),
    };

    private static Rgba DecodeRgba16Unorm(byte[] src, int offset) => new()
    {
        R = ToByte(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset, 2)) / 65535f),
        G = ToByte(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset + 2, 2)) / 65535f),
        B = ToByte(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset + 4, 2)) / 65535f),
        A = ToByte(BinaryPrimitives.ReadUInt16LittleEndian(src.AsSpan(offset + 6, 2)) / 65535f),
    };

    private static Rgba DecodeRgba32Float(byte[] src, int offset) => new()
    {
        R = FloatToDisplayByte(BitConverter.ToSingle(src, offset)),
        G = FloatToDisplayByte(BitConverter.ToSingle(src, offset + 4)),
        B = FloatToDisplayByte(BitConverter.ToSingle(src, offset + 8)),
        A = ToByte(BitConverter.ToSingle(src, offset + 12)),
    };

    private static byte FloatToDisplayByte(float value)
    {
        if (!float.IsFinite(value) || value <= 0) return 0;
        var mapped = value / (1f + value);
        return ToByte(MathF.Pow(mapped, 1f / 2.2f));
    }

    private static byte ToByte(float value) =>
        (byte)Math.Clamp((int)MathF.Round(value * 255f), 0, 255);

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
