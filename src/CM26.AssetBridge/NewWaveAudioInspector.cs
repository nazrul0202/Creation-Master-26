using System.Buffers.Binary;

namespace CM26.AssetBridge;

/// <summary>
/// Bounded reader for the Harmony/NewWave sample-bank header used by FC26.
/// The layout follows FET's NewWaveAssetHarmonySampleBankParser, but this
/// implementation deliberately reads only the bank and dataset descriptors
/// needed by CM26's public audio browser.
/// </summary>
internal static class NewWaveAudioInspector
{
    private const uint LittleEndianMagic = 1701593683; // "SBle"
    private const uint BigEndianMagic = 1700938323;    // "SBbe"
    private const uint DataSetMagic = 1146307924;      // "TESD" in LE storage
    private const int HeaderSize = 72;
    private const int MaximumDataSets = 4096;
    private static readonly Dictionary<uint, string> KnownDataSets = new()
    {
        [Djb2("Chunks")] = "Chunks",
        [Djb2("Segments")] = "Segments",
        [Djb2("Variations")] = "Variations",
        [Djb2("Selection")] = "Selection",
        [Djb2("SelectionParameters")] = "Selection Parameters",
        [Djb2("Persistence")] = "Persistence"
    };

    public static AudioBankResult Inspect(string name, byte[] bytes)
    {
        if (bytes.Length < 64) throw new InvalidDataException("NewWave bank is too short.");
        var magic = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
        var little = magic switch
        {
            LittleEndianMagic => true,
            BigEndianMagic => false,
            _ => throw new InvalidDataException(
                $"Asset is not a Harmony/NewWave sample bank (magic 0x{magic:X8}).")
        };
        uint U32(int offset) => little
            ? BinaryPrimitives.ReadUInt32LittleEndian(Slice(bytes, offset, 4))
            : BinaryPrimitives.ReadUInt32BigEndian(Slice(bytes, offset, 4));
        ushort U16(int offset) => little
            ? BinaryPrimitives.ReadUInt16LittleEndian(Slice(bytes, offset, 2))
            : BinaryPrimitives.ReadUInt16BigEndian(Slice(bytes, offset, 2));
        int I32(int offset) => unchecked((int)U32(offset));

        var alignmentPower = bytes[8];
        if (alignmentPower > 20) throw new InvalidDataException("Invalid NewWave alignment.");
        var version = bytes[9];
        if (version != 0) throw new InvalidDataException($"Unsupported NewWave bank version {version}.");
        var count = U16(10);
        if (count > MaximumDataSets) throw new InvalidDataException("NewWave dataset count is unsafe.");
        var bankKey = U32(12);
        var projectKey = U32(16);
        var dataSetTable = checked((int)U32(24));
        Ensure(bytes, dataSetTable, checked(count * 8), "dataset reference table");

        var result = new List<AudioDataSetResult>(count);
        for (var i = 0; i < count; i++)
        {
            var offset = checked((int)U32(dataSetTable + i * 8));
            Ensure(bytes, offset, HeaderSize, "dataset header");
            if (U32(offset) != DataSetMagic)
                throw new InvalidDataException($"Dataset {i} has an invalid header.");
            var id = U32(offset + 8);
            var sampleGroup = U32(offset + 12);
            var rows = I32(offset + 56);
            var fields = U16(offset + 60);
            var indexes = U16(offset + 62);
            if (rows < 0 || rows > 10_000_000)
                throw new InvalidDataException($"Dataset {i} row count is unsafe.");
            result.Add(new AudioDataSetResult(
                id, KnownDataSets.GetValueOrDefault(id, $"Dataset 0x{id:X8}"),
                sampleGroup, rows, fields, indexes));
        }
        return new AudioBankResult(
            name, little ? "Little Endian" : "Big Endian",
            1 << alignmentPower, version, bankKey, projectKey, result);
    }

    private static ReadOnlySpan<byte> Slice(byte[] bytes, int offset, int length)
    {
        Ensure(bytes, offset, length, "NewWave field");
        return bytes.AsSpan(offset, length);
    }

    private static void Ensure(byte[] bytes, int offset, int length, string label)
    {
        if (offset < 0 || length < 0 || offset > bytes.Length || length > bytes.Length - offset)
            throw new InvalidDataException($"{label} lies outside the bank.");
    }

    private static uint Djb2(string value)
    {
        uint hash = 5381;
        foreach (var character in value) hash = (hash * 33) ^ (byte)character;
        return hash;
    }
}
