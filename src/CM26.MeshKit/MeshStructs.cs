namespace CM26.MeshKit;

public struct Vec3
{
	public float x;
	public float y;
	public float z;
	internal float pad;
}

public struct AxisAlignedBox
{
	public Vec3 min;
	public Vec3 max;
}

public struct LinearTransform
{
	public Vec3 right;
	public Vec3 up;
	public Vec3 forward;
	public Vec3 trans;
}

public struct GeometryDeclarationDesc
{
	public struct Element
	{
		public VertexElementUsage Usage;
		public VertexElementFormat Format;
		public byte Offset;
		public byte StreamIndex;

		public readonly int Size => Format switch
		{
			VertexElementFormat.None => 0,
			VertexElementFormat.Float => 4,
			VertexElementFormat.Float2 => 8,
			VertexElementFormat.Float3 => 12,
			VertexElementFormat.Float4 => 16,
			VertexElementFormat.Half => 2,
			VertexElementFormat.Half2 => 4,
			VertexElementFormat.Half3 => 6,
			VertexElementFormat.Half4 => 8,
			VertexElementFormat.UByteN => 1,
			VertexElementFormat.Byte4 => 4,
			VertexElementFormat.Byte4N => 4,
			VertexElementFormat.UByte4 => 4,
			VertexElementFormat.UByte4N => 4,
			VertexElementFormat.Short => 2,
			VertexElementFormat.Short2 => 4,
			VertexElementFormat.Short3 => 6,
			VertexElementFormat.Short4 => 8,
			VertexElementFormat.ShortN => 2,
			VertexElementFormat.Short2N => 4,
			VertexElementFormat.Short3N => 6,
			VertexElementFormat.Short4N => 8,
			VertexElementFormat.UShort2 => 4,
			VertexElementFormat.UShort4 => 8,
			VertexElementFormat.UShort2N => 4,
			VertexElementFormat.UShort4N => 8,
			VertexElementFormat.Int => 4,
			VertexElementFormat.Int2 => 8,
			VertexElementFormat.Int3 => 12,
			VertexElementFormat.Int4 => 16,
			VertexElementFormat.IntN => 4,
			VertexElementFormat.Int2N => 8,
			VertexElementFormat.Int4N => 16,
			VertexElementFormat.UInt => 4,
			VertexElementFormat.UInt2 => 8,
			VertexElementFormat.UInt3 => 12,
			VertexElementFormat.UInt4 => 16,
			VertexElementFormat.UIntN => 4,
			VertexElementFormat.UInt2N => 8,
			VertexElementFormat.UInt4N => 16,
			VertexElementFormat.Comp3_10_10_10 => 4,
			VertexElementFormat.Comp3N_10_10_10 => 4,
			VertexElementFormat.UComp3_10_10_10 => 4,
			VertexElementFormat.UComp3N_10_10_10 => 4,
			VertexElementFormat.Comp3_11_11_10 => 4,
			VertexElementFormat.Comp3N_11_11_10 => 4,
			VertexElementFormat.UComp3_11_11_10 => 4,
			VertexElementFormat.UComp3N_11_11_10 => 4,
			VertexElementFormat.Comp4_10_10_10_2 => 4,
			VertexElementFormat.Comp4N_10_10_10_2 => 4,
			VertexElementFormat.UComp4_10_10_10_2 => 4,
			VertexElementFormat.UComp4N_10_10_10_2 => 4,
			_ => 0
		};
	}

	public struct Stream
	{
		public byte VertexStride;
		public VertexElementClassification Classification;
	}

	public Element[] Elements;
	public Stream[] Streams;
	public byte ElementCount;
	public byte StreamCount;

	public const int MaxElements = 16;
	public const int MaxStreams = 16;
}