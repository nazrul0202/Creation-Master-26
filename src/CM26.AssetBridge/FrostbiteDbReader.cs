using System.Buffers.Binary;
using System.Text;

namespace CM26.AssetBridge;

/// <summary>
/// Independent reader for Frostbite's compact DbObject stream used by
/// layout.toc. The implementation is read-only, bounded, and owns no code or
/// runtime dependency from another modding tool.
/// </summary>
internal sealed class FrostbiteDbReader
{
    private const int HeaderV0 = 0x00D1CE00;
    private const int HeaderV1 = 0x00D1CE01;
    private const int HeaderV3 = 0x00D1CE03;
    private const int MaxDepth = 128;
    private const int MaxCollectionBytes = 128 * 1024 * 1024;
    private const int MaxStringBytes = 16 * 1024 * 1024;

    public Dictionary<string, object?> ReadObject(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
        var header = ReadInt32BigEndian(reader);
        if (header is HeaderV0 or HeaderV1)
        {
            if (stream.Length < 556) throw new InvalidDataException("Truncated Frostbite DbObject header.");
            stream.Position = 556;
        }
        else if (header != HeaderV3)
        {
            // Headerless streams begin with the first entry prefix.
            stream.Position = 0;
        }

        var (_, value) = ReadEntry(reader, 0);
        return value as Dictionary<string, object?>
            ?? throw new InvalidDataException("Frostbite layout root is not an object.");
    }

    private static (string Name, object? Value) ReadEntry(BinaryReader reader, int depth)
    {
        if (depth > MaxDepth) throw new InvalidDataException("Frostbite DbObject nesting limit exceeded.");
        var prefix = reader.ReadByte();
        var type = prefix & 0x1F;
        if (type == 0) return (string.Empty, null);
        var name = (prefix & 0x80) == 0 ? ReadNullTerminatedString(reader) : string.Empty;

        return type switch
        {
            1 => (name, ReadArray(reader, depth + 1)),
            2 => (name, ReadObjectBody(reader, depth + 1)),
            6 => (name, reader.ReadByte() == 1),
            7 => (name, ReadSizedString(reader)),
            8 => (name, reader.ReadInt32()),
            9 => (name, reader.ReadInt64()),
            11 => (name, reader.ReadSingle()),
            12 => (name, reader.ReadDouble()),
            15 => (name, new Guid(ReadExact(reader, 16))),
            16 => (name, Convert.ToHexString(ReadExact(reader, 20))),
            19 => (name, ReadBlob(reader)),
            _ => throw new NotSupportedException($"Unsupported Frostbite DbObject type {type} ({name})."),
        };
    }

    private static List<object?> ReadArray(BinaryReader reader, int depth)
    {
        var length = Read7BitEncodedLong(reader);
        ValidateCollectionLength(reader, length);
        var end = reader.BaseStream.Position + length;
        var list = new List<object?>();
        while (reader.BaseStream.Position < end)
        {
            var (_, value) = ReadEntry(reader, depth);
            if (value == null) break;
            list.Add(value);
        }
        if (reader.BaseStream.Position > end)
            throw new InvalidDataException("Frostbite DbObject array exceeded its declared length.");
        reader.BaseStream.Position = end;
        return list;
    }

    private static Dictionary<string, object?> ReadObjectBody(BinaryReader reader, int depth)
    {
        var length = Read7BitEncodedLong(reader);
        ValidateCollectionLength(reader, length);
        var end = reader.BaseStream.Position + length;
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        while (reader.BaseStream.Position < end)
        {
            var (name, value) = ReadEntry(reader, depth);
            if (value == null) break;
            if (string.IsNullOrEmpty(name))
                throw new InvalidDataException("Named Frostbite object entry has no name.");
            result[name] = value;
        }
        if (reader.BaseStream.Position > end)
            throw new InvalidDataException("Frostbite DbObject exceeded its declared length.");
        reader.BaseStream.Position = end;
        return result;
    }

    private static string ReadSizedString(BinaryReader reader)
    {
        var length = Read7BitEncodedLong(reader);
        if (length is < 0 or > MaxStringBytes) throw new InvalidDataException("Invalid Frostbite string length.");
        var bytes = ReadExact(reader, checked((int)length));
        var contentLength = bytes.Length;
        while (contentLength > 0 && bytes[contentLength - 1] == 0) contentLength--;
        return Encoding.UTF8.GetString(bytes, 0, contentLength);
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        var bytes = new List<byte>(64);
        while (true)
        {
            var value = reader.ReadByte();
            if (value == 0) return Encoding.UTF8.GetString(bytes.ToArray());
            if (bytes.Count >= MaxStringBytes) throw new InvalidDataException("Unterminated Frostbite string.");
            bytes.Add(value);
        }
    }

    private static byte[] ReadBlob(BinaryReader reader)
    {
        var length = Read7BitEncodedLong(reader);
        if (length is < 0 or > MaxCollectionBytes) throw new InvalidDataException("Invalid Frostbite blob length.");
        return ReadExact(reader, checked((int)length));
    }

    private static long Read7BitEncodedLong(BinaryReader reader)
    {
        ulong result = 0;
        for (var shift = 0; shift < 64; shift += 7)
        {
            var value = reader.ReadByte();
            result |= (ulong)(value & 0x7F) << shift;
            if ((value & 0x80) == 0)
                return checked((long)result);
        }
        throw new InvalidDataException("Invalid Frostbite 7-bit integer.");
    }

    private static int ReadInt32BigEndian(BinaryReader reader)
    {
        Span<byte> bytes = stackalloc byte[4];
        var read = reader.BaseStream.Read(bytes);
        if (read != bytes.Length) throw new EndOfStreamException();
        return BinaryPrimitives.ReadInt32BigEndian(bytes);
    }

    private static byte[] ReadExact(BinaryReader reader, int length)
    {
        var data = reader.ReadBytes(length);
        if (data.Length != length) throw new EndOfStreamException();
        return data;
    }

    private static void ValidateCollectionLength(BinaryReader reader, long length)
    {
        if (length is < 0 or > MaxCollectionBytes ||
            length > reader.BaseStream.Length - reader.BaseStream.Position)
            throw new InvalidDataException("Invalid Frostbite collection length.");
    }
}
