namespace CM26.MeshKit;

public sealed class MeshSetSection
{
	public string Name { get; }
	public int MaterialId { get; }
	public uint PrimitiveCount { get; }
	public uint StartIndex { get; }
	public uint VertexOffset { get; }
	public uint VertexCount { get; }
	public GeometryDeclarationDesc[] GeometryDeclDesc { get; }
	public ushort[] BoneList { get; private set; } = Array.Empty<ushort>();
	public PrimitiveType PrimitiveType { get; }
	public byte BonesPerVertex { get; }
	public int DeclCount { get; } = 1;

	public MeshSetSection(DataVersion dataVersion, MeshReader reader, long additionalData, int index)
	{
		ArgumentNullException.ThrowIfNull(reader, nameof(reader));
		if (dataVersion >= DataVersion.FC25) DeclCount = 2;

		reader.ReadInt64LittleEndian(); // offset1
		var stringOffset = reader.ReadInt64LittleEndian();
		var boneIndicesOffset = reader.ReadInt64LittleEndian();
		var boneCount = reader.ReadUInt16LittleEndian();
		reader.ReadUInt16LittleEndian(); // unknown1 FC26
		MaterialId = reader.ReadUInt16LittleEndian();
		reader.ReadByte(); // vertexStride
		PrimitiveType = (PrimitiveType)reader.ReadByte();
		PrimitiveCount = reader.ReadUInt32LittleEndian();
		StartIndex = reader.ReadUInt32LittleEndian();
		VertexOffset = reader.ReadUInt32LittleEndian();
		VertexCount = reader.ReadUInt32LittleEndian();
		reader.ReadUInt32LittleEndian(); // unknown2
		reader.ReadBytes(24); // unknown3 FC26

		for (var i = 0; i < 6; i++) reader.ReadSingleLittleEndian(); // texCoordRatios

		GeometryDeclDesc = new GeometryDeclarationDesc[DeclCount];
		for (var i = 0; i < DeclCount; i++)
		{
			GeometryDeclDesc[i] = new GeometryDeclarationDesc
			{
				Elements = new GeometryDeclarationDesc.Element[16],
				Streams = new GeometryDeclarationDesc.Stream[16]
			};
			for (var j = 0; j < 16; j++)
			{
				GeometryDeclDesc[i].Elements[j] = new GeometryDeclarationDesc.Element
				{
					Usage = (VertexElementUsage)reader.ReadByte(),
					Format = (VertexElementFormat)reader.ReadByte(),
					Offset = reader.ReadByte(),
					StreamIndex = reader.ReadByte()
				};
			}
			for (var k = 0; k < 16; k++)
			{
				GeometryDeclDesc[i].Streams[k] = new GeometryDeclarationDesc.Stream
				{
					VertexStride = reader.ReadByte(),
					Classification = (VertexElementClassification)reader.ReadByte()
				};
			}
			GeometryDeclDesc[i].ElementCount = reader.ReadByte();
			GeometryDeclDesc[i].StreamCount = reader.ReadByte();
			reader.Pad(4);
		}

		if (dataVersion >= DataVersion.Madden22)
		{
			if (dataVersion >= DataVersion.FC26)
			{
				reader.ReadBytes(18); // unknown4
				BonesPerVertex = reader.ReadByte();
				reader.ReadBytes(17); // unknown5
			}
			reader.ReadAxisAlignedBox(); // boundingBox
		}

		reader.Pad(16);
		var position3 = reader.Position;
		reader.Position = boneIndicesOffset + additionalData;
		BoneList = new ushort[boneCount];
		for (var m = 0; m < boneCount; m++) BoneList[m] = reader.ReadUInt16LittleEndian();
		reader.Position = stringOffset + additionalData;
		Name = reader.ReadNullTerminatedString();
		reader.Position = position3;
	}
}