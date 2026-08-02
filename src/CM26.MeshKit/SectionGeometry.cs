using System.Numerics;

namespace CM26.MeshKit;

/// <summary>
/// Extracts positions, normals and UVs from a section's vertex buffer and its
/// triangle index list, mirroring FET's FBXExportSubObject byte arithmetic.
/// </summary>
public sealed class SectionGeometry
{
	public List<Vector3> Positions { get; } = new();
	public List<int[]> Triangles { get; } = new();
	public int NormalCount { get; }
	public int UvCount { get; }
	public IReadOnlyList<Vector3> AggregatedNormals => aggregatedNormals;
	public IReadOnlyList<Vector2> AggregatedUvs => aggregatedUvs;

	/// <summary>Per-control-point bone influences (local bone index into the
	/// section BoneList and its bias) read straight from the vertex stream.</summary>
	public IReadOnlyList<BoneInfluence[]>? BoneInfluences { get; }

	public readonly record struct BoneInfluence(ushort LocalIndex, float Weight);

	private readonly List<Vector3> aggregatedNormals = new();
	private readonly List<Vector2> aggregatedUvs = new();

	public SectionGeometry(MeshSetSection section, MeshSetLod lod, byte[] data)
	{
		ArgumentNullException.ThrowIfNull(section, nameof(section));
		var vertexCount = (int)section.VertexCount;
		var reader = new MeshDataReader(data);
		var decl = section.GeometryDeclDesc.Length > 0 ? section.GeometryDeclDesc[0] : default;

		// Per-vertex attribute buffers (indexed by control point).
		var positions = new List<Vector3>(vertexCount);
		var normals = new Vector3[vertexCount];
		var uvs = new Vector2?[vertexCount];
		var bones = new BoneInfluence[vertexCount][];
		var hasNormal = false;
		var hasUv = false;
		var hasBones = false;

		// FETCH resolves each element against the accumulated stream offset.
		//
		// section.VertexOffset is relative to the start of the LOD vertex data,
		// i.e. the section-relative position inside 'data' (which contains the
		// vertex stream concatenated with the index stream). Index data begins
		// at lod.VertexBufferSize.
		var totalStride = 0;
		for (var s = 0; s < decl.StreamCount && s < decl.Streams.Length; s++)
		{
			var stream = decl.Streams[s];
			if (stream.VertexStride == 0) continue;

			for (var v = 0; v < vertexCount; v++)
			{
				var baseOffset = checked((long)section.VertexOffset + totalStride * vertexCount + v * (long)stream.VertexStride);
				var currentStride = 0;

				for (var e = 0; e < decl.Elements.Length; e++)
				{
					var element = decl.Elements[e];
					if (currentStride >= totalStride + stream.VertexStride) break;
					if (currentStride >= totalStride)
					{
						var elementPosition = baseOffset + (currentStride - totalStride);
						reader.Position = elementPosition;
						switch (element.Usage)
						{
							case VertexElementUsage.Pos:
								positions.Add(ReadFloat3(reader, element.Format));
								break;
							case VertexElementUsage.Normal:
								normals[v] = ReadFloat3(reader, element.Format);
								hasNormal = true;
								break;
							case VertexElementUsage.TexCoord0:
								uvs[v] = ReadFloat2(reader, element.Format);
								hasUv = true;
								break;
							case VertexElementUsage.BoneIndices:
							case VertexElementUsage.BoneIndices2:
								if (bones[v] == null) bones[v] = new BoneInfluence[8];
								ReadBoneIndices(reader, element.Format, bones[v]!, element.Usage == VertexElementUsage.BoneIndices ? 0 : 4);
								hasBones = true;
								break;
							case VertexElementUsage.BoneWeights:
							case VertexElementUsage.BoneWeights2:
								if (bones[v] == null) bones[v] = new BoneInfluence[8];
								ReadBoneWeights(reader, element.Format, bones[v]!, element.Usage == VertexElementUsage.BoneWeights ? 0 : 4);
								hasBones = true;
								break;
						}
					}
					currentStride += element.Size;
				}
			}
			totalStride += stream.VertexStride;
		}

		if (positions.Count == vertexCount)
			Positions.AddRange(positions);

		// Triangle list from the index stream (which lives past the vertex bytes).
		var indexSize = lod.IndexUnitSize / 8;
		var indexStart = checked((long)lod.IndexOffset);
		reader.Position = indexStart + (long)section.StartIndex * indexSize;
		var triCount = (int)section.PrimitiveCount;
		for (var i = 0; i < triCount; i++)
		{
			var a = (int)(indexSize == 2 ? reader.ReadUInt16LittleEndian() : reader.ReadUInt32LittleEndian());
			var b = (int)(indexSize == 2 ? reader.ReadUInt16LittleEndian() : reader.ReadUInt32LittleEndian());
			var c = (int)(indexSize == 2 ? reader.ReadUInt16LittleEndian() : reader.ReadUInt32LittleEndian());
			Triangles.Add(new[] { a, b, c });
		}

		if (hasNormal)
		{
			NormalCount = vertexCount;
			foreach (var tri in Triangles)
			{
				foreach (var idx in tri)
				{
					aggregatedNormals.Add(idx >= 0 && idx < normals.Length ? normals[idx] : Vector3.UnitZ);
				}
			}
		}
		if (hasUv)
		{
			UvCount = vertexCount;
			foreach (var tri in Triangles)
			{
				foreach (var idx in tri)
				{
					aggregatedUvs.Add(idx >= 0 && idx < uvs.Length && uvs[idx] is { } uv ? uv : Vector2.Zero);
				}
			}
		}
		if (hasBones) BoneInfluences = bones;
	}

	/// <summary>Reads the bone indices into slot (0..4) exactly as FET's
	/// FBXCreateSkin: they are stored reverse-order per 4-element group so the
	/// first byte maps to weight slot 3 within the group.</summary>
	private static void ReadBoneIndices(MeshDataReader reader, VertexElementFormat format, BoneInfluence[] target, int slotBase)
	{
		switch (format)
		{
			case VertexElementFormat.Byte4:
			case VertexElementFormat.Byte4N:
			case VertexElementFormat.UByte4:
			case VertexElementFormat.UByte4N:
				target[slotBase + 3] = target[slotBase + 3] with { LocalIndex = reader.ReadByte() };
				target[slotBase + 2] = target[slotBase + 2] with { LocalIndex = reader.ReadByte() };
				target[slotBase + 1] = target[slotBase + 1] with { LocalIndex = reader.ReadByte() };
				target[slotBase + 0] = target[slotBase + 0] with { LocalIndex = reader.ReadByte() };
				break;
			case VertexElementFormat.UShort2:
			case VertexElementFormat.UShort2N:
				target[slotBase + 1] = target[slotBase + 1] with { LocalIndex = reader.ReadUInt16LittleEndian() };
				target[slotBase + 0] = target[slotBase + 0] with { LocalIndex = reader.ReadUInt16LittleEndian() };
				break;
			case VertexElementFormat.UShort4:
			case VertexElementFormat.UShort4N:
			default:
				target[slotBase + 3] = target[slotBase + 3] with { LocalIndex = reader.ReadUInt16LittleEndian() };
				target[slotBase + 2] = target[slotBase + 2] with { LocalIndex = reader.ReadUInt16LittleEndian() };
				target[slotBase + 1] = target[slotBase + 1] with { LocalIndex = reader.ReadUInt16LittleEndian() };
				target[slotBase + 0] = target[slotBase + 0] with { LocalIndex = reader.ReadUInt16LittleEndian() };
				break;
		}
	}

	private static void ReadBoneWeights(MeshDataReader reader, VertexElementFormat format, BoneInfluence[] target, int slotBase)
	{
		switch (format)
		{
			case VertexElementFormat.Byte4:
			case VertexElementFormat.Byte4N:
			case VertexElementFormat.UByte4:
			case VertexElementFormat.UByte4N:
			case VertexElementFormat.UByteN:
				target[slotBase + 3] = target[slotBase + 3] with { Weight = reader.ReadByte() / 255f };
				target[slotBase + 2] = target[slotBase + 2] with { Weight = reader.ReadByte() / 255f };
				target[slotBase + 1] = target[slotBase + 1] with { Weight = reader.ReadByte() / 255f };
				target[slotBase + 0] = target[slotBase + 0] with { Weight = reader.ReadByte() / 255f };
				break;
			case VertexElementFormat.Float4:
				target[slotBase + 3] = target[slotBase + 3] with { Weight = reader.ReadSingleLittleEndian() };
				target[slotBase + 2] = target[slotBase + 2] with { Weight = reader.ReadSingleLittleEndian() };
				target[slotBase + 1] = target[slotBase + 1] with { Weight = reader.ReadSingleLittleEndian() };
				target[slotBase + 0] = target[slotBase + 0] with { Weight = reader.ReadSingleLittleEndian() };
				break;
			case VertexElementFormat.Float3:
				target[slotBase + 2] = target[slotBase + 2] with { Weight = reader.ReadSingleLittleEndian() };
				target[slotBase + 1] = target[slotBase + 1] with { Weight = reader.ReadSingleLittleEndian() };
				target[slotBase + 0] = target[slotBase + 0] with { Weight = reader.ReadSingleLittleEndian() };
				break;
			case VertexElementFormat.Float:
				target[slotBase + 0] = target[slotBase + 0] with { Weight = reader.ReadSingleLittleEndian() };
				break;
		}
	}

	private static Vector3 ReadFloat3(MeshDataReader reader, VertexElementFormat format)
	{
		switch (format)
		{
			case VertexElementFormat.Float3:
				return new Vector3(
					reader.ReadSingleLittleEndian(),
					reader.ReadSingleLittleEndian(),
					reader.ReadSingleLittleEndian());
			case VertexElementFormat.Float4:
			{
				var value = new Vector3(
					reader.ReadSingleLittleEndian(),
					reader.ReadSingleLittleEndian(),
					reader.ReadSingleLittleEndian());
				reader.ReadSingleLittleEndian(); // W component
				return value;
			}
			case VertexElementFormat.Half3:
				return new Vector3(
					HalfUtils.Unpack(reader.ReadUInt16LittleEndian()),
					HalfUtils.Unpack(reader.ReadUInt16LittleEndian()),
					HalfUtils.Unpack(reader.ReadUInt16LittleEndian()));
			case VertexElementFormat.Half4:
			{
				var value = new Vector3(
					HalfUtils.Unpack(reader.ReadUInt16LittleEndian()),
					HalfUtils.Unpack(reader.ReadUInt16LittleEndian()),
					HalfUtils.Unpack(reader.ReadUInt16LittleEndian()));
				reader.ReadUInt16LittleEndian(); // W component
				return value;
			}
			default:
				throw new NotSupportedException($"Position/normal format {format} is not supported yet.");
		}
	}

	private static Vector2 ReadFloat2(MeshDataReader reader, VertexElementFormat format)
	{
		switch (format)
		{
			case VertexElementFormat.Float:
			case VertexElementFormat.Float2:
				return new Vector2(
					reader.ReadSingleLittleEndian(),
					reader.ReadSingleLittleEndian());
			case VertexElementFormat.Half:
			case VertexElementFormat.Half2:
				return new Vector2(
					HalfUtils.Unpack(reader.ReadUInt16LittleEndian()),
					HalfUtils.Unpack(reader.ReadUInt16LittleEndian()));
			default:
				throw new NotSupportedException($"UV format {format} is not supported yet.");
		}
	}
}