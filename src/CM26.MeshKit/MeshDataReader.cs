using System.Buffers.Binary;

namespace CM26.MeshKit;

/// <summary>
/// Position-aware reader over a LOD's vertex/index buffer(s). Mirrors the
/// offset arithmetic FETCH uses so that vertex and bone data resolve against
/// the same byte ranges as the original FBX tool.
/// </summary>
public sealed class MeshDataReader
{
	private readonly byte[] data;
	private int position;

	public long Position
	{
		get => position;
		set => position = checked((int)value);
	}

	public MeshDataReader(byte[] data) => this.data = data ?? throw new ArgumentNullException(nameof(data));

	private ReadOnlySpan<byte> Span(int count)
	{
		if (count < 0 || position > data.Length || count > data.Length - position)
			throw new EndOfStreamException($"MeshDataReader exceeds its buffer at +0x{position:X}.");
		var result = data.AsSpan(position, count);
		position += count;
		return result;
	}

	public byte ReadByte() => Span(1)[0];
	public ushort ReadUInt16LittleEndian() => BinaryPrimitives.ReadUInt16LittleEndian(Span(2));
	public int ReadInt16LittleEndian() => (short)BinaryPrimitives.ReadUInt16LittleEndian(Span(2));
	public uint ReadUInt32LittleEndian() => BinaryPrimitives.ReadUInt32LittleEndian(Span(4));
	public int ReadInt32LittleEndian() => BinaryPrimitives.ReadInt32LittleEndian(Span(4));
	public float ReadSingleLittleEndian() => BinaryPrimitives.ReadSingleLittleEndian(Span(4));
}