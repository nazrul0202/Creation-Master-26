namespace CM26.MeshKit;

public sealed class MeshSetLod
{
	public MeshType Type { get; }
	public List<MeshSetSection> Sections { get; } = new();
	public MeshLayoutFlags Flags { get; }
	public uint IndexBufferSize { get; }
	public uint VertexBufferSize { get; }
	public Guid ChunkId { get; }
	public string FullName { get; }
	public string Name { get; }
	public string ShortName { get; }
	public List<uint> BoneIndexArray { get; } = new();
	public byte[]? InlineData { get; set; }
	public List<List<byte>> CategorySubsetIndices { get; } = new();

	// RenderFormat_R32_UINT ordinal in the generated FC26 enum registry.
	private const int RenderFormatR32Uint = 46;
	private readonly int indexBufferFormat;

	public int IndexUnitSize => indexBufferFormat == RenderFormatR32Uint ? 32 : 16;

	/// <summary>Byte offset in the LOD data where the index stream begins.</summary>
	public uint IndexOffset => VertexBufferSize;

	public MeshSetLod(DataVersion dataVersion, MeshReader reader, long additionalData, ref int sectionIndex)
	{
		ArgumentNullException.ThrowIfNull(reader, nameof(reader));
		Type = (MeshType)reader.ReadUInt32LittleEndian();
		reader.ReadUInt32LittleEndian(); // maxInstances
		var sectionCount = reader.ReadUInt32LittleEndian();
		var sectionOffset = reader.ReadInt64LittleEndian();
		var currentPosition = reader.Position;
		reader.Position = sectionOffset + additionalData;
		for (var i = 0; i < sectionCount; i++)
			Sections.Add(new MeshSetSection(dataVersion, reader, additionalData, sectionIndex++));
		reader.Position = currentPosition;

		for (var j = 0; j < 5; j++)
		{
			var count = reader.ReadInt32LittleEndian();
			var subsetCategoryOffset = reader.ReadInt64LittleEndian();
			CategorySubsetIndices.Add(new List<byte>());
			currentPosition = reader.Position;
			reader.Position = subsetCategoryOffset + additionalData;
			for (var k = 0; k < count; k++)
				CategorySubsetIndices[j].Add(reader.ReadByte());
			reader.Position = currentPosition;
		}

		Flags = (MeshLayoutFlags)reader.ReadUInt32LittleEndian();
		indexBufferFormat = reader.ReadInt32LittleEndian();
		IndexBufferSize = reader.ReadUInt32LittleEndian();
		VertexBufferSize = reader.ReadUInt32LittleEndian();
		reader.ReadBytes(20); // FC26 ext
		ChunkId = reader.ReadGuid();
		reader.ReadUInt32LittleEndian(); // inlineDataOffset
		reader.ReadUInt32LittleEndian(); // unknown2
		var stringOffset1 = reader.ReadInt64LittleEndian();
		var stringOffset2 = reader.ReadInt64LittleEndian();
		var stringOffset3 = reader.ReadInt64LittleEndian();
		reader.ReadUInt32LittleEndian(); // nameHash
		reader.ReadInt64LittleEndian();

		reader.Pad(16);
		currentPosition = reader.Position;
		if (Type == MeshType.Skinned)
		{
			var bonePartCount = reader.ReadUInt32LittleEndian();
			var boneIndexArrayOffset = reader.ReadInt64LittleEndian();
			reader.Position = boneIndexArrayOffset + additionalData;
			for (var l = 0; l < bonePartCount; l++) BoneIndexArray.Add(reader.ReadUInt32LittleEndian());
		}
		else if (Type == MeshType.Composite)
		{
			// subsetPartIndices masks are not needed by the exporter.
			reader.ReadInt64LittleEndian();
		}

		// LOD names are stored at absolute relocation offsets.
		var currentPos2 = reader.Position;
		reader.Position = stringOffset1 + additionalData;
		var shaderDebugName = reader.ReadNullTerminatedString();
		reader.Position = stringOffset2 + additionalData;
		var nameValue = reader.ReadNullTerminatedString();
		reader.Position = stringOffset3 + additionalData;
		var shortName = reader.ReadNullTerminatedString();
		reader.Position = currentPos2;

		FullName = shaderDebugName;
		Name = nameValue;
		ShortName = shortName;
		// Re-center on the next LOD boundary.
		reader.Position = currentPosition;
	}

	public bool IsSectionRenderable(MeshSetSection section)
	{
		if (section.PrimitiveCount == 0) return false;
		if (!IsSectionInCategory(section, MeshSubsetCategory.Opaque) &&
			!IsSectionInCategory(section, MeshSubsetCategory.Transparent))
			return IsSectionInCategory(section, MeshSubsetCategory.TransparentDecal);
		return true;
	}

	public void ReadInlineData(MeshReader reader)
	{
		if (ChunkId == Guid.Empty)
		{
			InlineData = reader.ReadBytes((int)(VertexBufferSize + IndexBufferSize));
			reader.Pad(16);
		}
	}

	private bool IsSectionInCategory(MeshSetSection section, MeshSubsetCategory category)
	{
		var sectionIndex = Sections.IndexOf(section);
		if (sectionIndex < 0 || (int)category >= CategorySubsetIndices.Count) return false;
		return CategorySubsetIndices[(int)category].Contains((byte)sectionIndex);
	}
}