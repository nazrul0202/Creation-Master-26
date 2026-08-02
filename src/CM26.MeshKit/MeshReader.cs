using System.Buffers.Binary;
using System.Text;

namespace CM26.MeshKit;

/// <summary>
/// Binary reader over the decoded Mesh RES payload. Only the little-endian
/// primitives required by the MeshSet layout are exposed.
/// </summary>
public sealed class MeshReader : IDisposable
{
	public Stream BaseStream { get; }

	public long Position
	{
		get => BaseStream.Position;
		set => BaseStream.Position = value;
	}

	public long Length => BaseStream.Length;

	public MeshReader(Stream stream) => BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));

	public void Pad(int alignment)
	{
		var remainder = Position % alignment;
		if (remainder != 0) Position += alignment - remainder;
	}

	public byte ReadByte()
	{
		var value = BaseStream.ReadByte();
		if (value == -1) throw new EndOfStreamException();
		return (byte)value;
	}

	public byte[] ReadBytes(int count)
	{
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		var buffer = new byte[count];
		BaseStream.ReadExactly(buffer);
		return buffer;
	}

	public short ReadInt16LittleEndian() => BinaryPrimitives.ReadInt16LittleEndian(ReadExactly(2));
	public ushort ReadUInt16LittleEndian() => BinaryPrimitives.ReadUInt16LittleEndian(ReadExactly(2));
	public int ReadInt32LittleEndian() => BinaryPrimitives.ReadInt32LittleEndian(ReadExactly(4));
	public uint ReadUInt32LittleEndian() => BinaryPrimitives.ReadUInt32LittleEndian(ReadExactly(4));
	public long ReadInt64LittleEndian() => BinaryPrimitives.ReadInt64LittleEndian(ReadExactly(8));
	public ulong ReadUInt64LittleEndian() => BinaryPrimitives.ReadUInt64LittleEndian(ReadExactly(8));
	public float ReadSingleLittleEndian() => BinaryPrimitives.ReadSingleLittleEndian(ReadExactly(4));

	public Guid ReadGuid()
	{
		var span = ReadExactly(16);
		return new Guid(span);
	}

	public Vec3 ReadVec3()
	{
		return new Vec3
		{
			x = ReadSingleLittleEndian(),
			y = ReadSingleLittleEndian(),
			z = ReadSingleLittleEndian(),
			pad = ReadSingleLittleEndian()
		};
	}

	public AxisAlignedBox ReadAxisAlignedBox()
	{
		return new AxisAlignedBox { min = ReadVec3(), max = ReadVec3() };
	}

	public LinearTransform ReadLinearTransform()
	{
		return new LinearTransform
		{
			right = ReadVec3(),
			up = ReadVec3(),
			forward = ReadVec3(),
			trans = ReadVec3()
		};
	}

	public string ReadNullTerminatedString()
	{
		var builder = new StringBuilder();
		while (true)
		{
			var chr = BaseStream.ReadByte();
			if (chr == -1 || chr == 0) break;
			builder.Append((char)chr);
		}
		return builder.ToString();
	}

	private byte[] ReadExactly(int count)
	{
		var buffer = new byte[count];
		BaseStream.ReadExactly(buffer);
		return buffer;
	}

	public void Dispose() => BaseStream.Dispose();
}