using System.Buffers.Binary;

namespace CM26.MeshKit;

internal static class Djb2Hash
{
	private const uint Seed = 5381u;
	private const uint Prime = 33u;

	public static uint HashString32(string data)
	{
		var hash = Seed;
		foreach (var c in data) hash = (hash * Prime) ^ (byte)c;
		return hash;
	}
}

public sealed class MeshSet
{
	private readonly ushort[] lodFadeDistanceFactors = new ushort[12];
	private readonly ushort[] subsetStartIndices = new ushort[12];
	private readonly ushort totalBoneCount;
	private long additionalData;

	// ResMeta is the 16-byte FC26 resource meta header carried by the bundle
	// manifest. Callers pass it as a raw byte[16] (e.g. hex-decoded ResMeta).
	public uint ResMetaLayoutSize { get; }
	public uint RelocationSize { get; }
	public uint VertexIndexSize { get; }

	public AxisAlignedBox BoundingBox { get; }
	public List<MeshSetLod> Lods { get; } = new();
	public MeshType Type { get; }
	public MeshSetLayoutFlags Flags { get; }
	public string FullName { get; }
	public string Name { get; }
	public int PartCount => totalBoneCount;
	public LinearTransform[]? PartTransforms { get; }
	public ushort[]? BoneIndices { get; }
	public AxisAlignedBox[]? BoneBoundingBoxes { get; }

	public MeshSet(DataVersion dataVersion, MemoryStream stream, byte[] resMeta)
	{
		ArgumentNullException.ThrowIfNull(stream, nameof(stream));
		ArgumentNullException.ThrowIfNull(resMeta, nameof(resMeta));

		var isFc26 = dataVersion >= DataVersion.FC26;
		ResMetaLayoutSize = ReadMetaU32(resMeta, 0);
		RelocationSize = ReadMetaU32(resMeta, 4);
		VertexIndexSize = ReadMetaU32(resMeta, 8);

		using var reader = new MeshReader(stream);
		if (isFc26)
		{
			additionalData = 16L;
			reader.ReadUInt32LittleEndian(); // stream duplicates the layout header meshSetSize
			reader.ReadUInt32LittleEndian(); // meshSize
			reader.ReadUInt32LittleEndian(); // subsetSize
			reader.ReadUInt32LittleEndian(); // geometrySize
		}
		else
		{
			// historical data omitted, only exposes the FC26 path
		}

		BoundingBox = reader.ReadAxisAlignedBox();
		var lodOffsets = new long[6];
		for (var i = 0; i < 6; i++) lodOffsets[i] = reader.ReadInt64LittleEndian();
		reader.ReadInt64LittleEndian();
		var fullNameOffset = reader.ReadInt64LittleEndian();
		var nameOffset = reader.ReadInt64LittleEndian();
		reader.ReadUInt32LittleEndian(); // nameHash
		Type = (MeshType)reader.ReadByte();
		reader.ReadBytes(11); // unknown1 FC24+
		for (var j = 0; j < 12; j++) lodFadeDistanceFactors[j] = reader.ReadUInt16LittleEndian();
		Flags = (MeshSetLayoutFlags)reader.ReadUInt64LittleEndian();
		reader.ReadByte(); // shaderDrawOrder
		reader.ReadByte(); // shaderDrawOrderUserSlot
		reader.ReadInt16LittleEndian(); // shaderDrawOrderSubOrder
		var lodsCount = reader.ReadUInt16LittleEndian();
		reader.ReadUInt16LittleEndian(); // total subset count

		for (var k = 0; k < 6; k++) subsetStartIndices[k] = reader.ReadUInt16LittleEndian();

		if (Type == MeshType.Skinned)
		{
			totalBoneCount = reader.ReadUInt16LittleEndian();
			var cullBoxCount = reader.ReadUInt16LittleEndian();
			if (cullBoxCount != 0)
			{
				var boneIndicesOffset = reader.ReadInt64LittleEndian();
				var boneBoundingBoxOffset = reader.ReadInt64LittleEndian();
				var saved = reader.Position;
				if (boneIndicesOffset != 0)
				{
					reader.Position = boneIndicesOffset + additionalData;
					BoneIndices = new ushort[cullBoxCount];
					for (var l = 0; l < cullBoxCount; l++) BoneIndices[l] = reader.ReadUInt16LittleEndian();
				}
				if (boneBoundingBoxOffset != 0)
				{
					reader.Position = boneBoundingBoxOffset + additionalData;
					BoneBoundingBoxes = new AxisAlignedBox[cullBoxCount];
					for (var m = 0; m < cullBoxCount; m++) BoneBoundingBoxes[m] = reader.ReadAxisAlignedBox();
				}
				reader.Position = saved;
			}
		}
		else if (Type == MeshType.Composite)
		{
			totalBoneCount = reader.ReadUInt16LittleEndian();
			reader.ReadUInt16LittleEndian();
			var partTransformsOffset = reader.ReadInt64LittleEndian();
			reader.Pad(4);
			var partBoundingBoxOffset = reader.ReadInt64LittleEndian();
			var positionBeforeReadingPartData = reader.Position;
			if (partTransformsOffset != 0)
			{
				reader.Position = partTransformsOffset + additionalData;
				PartTransforms = new LinearTransform[totalBoneCount];
				for (var n = 0; n < totalBoneCount; n++) PartTransforms[n] = reader.ReadLinearTransform();
			}
			reader.Position = positionBeforeReadingPartData;
		}
		reader.Pad(16);

		var sectionIndex = 0;
		if (lodsCount > lodOffsets.Length)
			throw new InvalidDataException($"Mesh has {lodsCount} LODs but only {lodOffsets.Length} offsets are supported.");
		for (var num2 = 0; num2 < lodsCount; num2++)
		{
			reader.Position = lodOffsets[num2] + additionalData;
			Lods.Add(new MeshSetLod(dataVersion, reader, additionalData, ref sectionIndex));
		}
		reader.Pad(16);
		reader.Position = fullNameOffset + additionalData;
		FullName = reader.ReadNullTerminatedString();
		reader.Position = nameOffset + additionalData;
		Name = reader.ReadNullTerminatedString();

		if (ResMetaLayoutSize != 0 && VertexIndexSize != 0)
		{
			reader.Position = ResMetaLayoutSize + RelocationSize;
			reader.Pad(16);
			foreach (var lod in Lods) lod.ReadInlineData(reader);
		}
	}

	private static uint ReadMetaU32(byte[] meta, int offset)
	{
		if (meta == null || offset < 0 || offset + 4 > meta.Length)
			throw new InvalidDataException("Mesh resMeta is too short to read a UInt32.");
		return BinaryPrimitives.ReadUInt32LittleEndian(meta.AsSpan(offset, 4));
	}
}