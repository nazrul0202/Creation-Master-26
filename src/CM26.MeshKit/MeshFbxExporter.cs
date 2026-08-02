using System.Globalization;
using System.Numerics;
using System.Text;

namespace CM26.MeshKit;

/// <summary>
/// Self-contained ASCII FBX 7.4 exporter. Reproduces the geometry extraction
/// of FET's MeshToFbxExporter without the Autodesk FBX SDK: control points,
/// normals, UV channels and triangle indices are re-emitted as
/// Blender/Assimp-compatible ASCII FBX nodes.
/// </summary>
public sealed class MeshFbxExporter
{
	private readonly MeshSet mesh;
	private readonly Dictionary<Guid, byte[]> chunkData = new();
	private readonly List<string> connections = new();
	private readonly List<long> boneModelIds = new();
	private long skeletonRootId;
	private int boneCount;

	public MeshFbxExporter(MeshSet mesh) =>
		this.mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));

	public void AddChunkData(Guid id, byte[] data) => chunkData[id] = data ?? throw new ArgumentNullException(nameof(id));

	public string Export(string meshName, string? textureFileName = null)
	{
		var sb = new StringBuilder(1 << 20);
		WriteHeader(sb, meshName);
		sb.AppendLine();
		WriteGlobalSettings(sb);
		sb.AppendLine();

		sb.AppendLine("Objects:  {");
		var nextObjectId = 1L;
		if (mesh.Type is MeshType.Skinned or MeshType.Composite && mesh.PartCount > 0)
			CreateSkeletonNodes(sb, ref nextObjectId);
		for (var lodIndex = 0; lodIndex < mesh.Lods.Count; lodIndex++)
		{
			var lod = mesh.Lods[lodIndex];
			var data = lod.ChunkId != Guid.Empty && chunkData.TryGetValue(lod.ChunkId, out var chunk)
				? chunk
				: lod.InlineData;
			if (data == null || data.Length == 0) continue;
			ExportLod(sb, meshName, lod, data, textureFileName, ref nextObjectId);
		}
		sb.AppendLine("}");
		sb.AppendLine();

		sb.AppendLine("Connections:  {");
		foreach (var connection in connections)
			sb.AppendLine(connection);
		sb.AppendLine("}");
		return sb.ToString();
	}

	private void WriteHeader(StringBuilder sb, string meshName)
	{
		sb.AppendLine("; FBX 7.4.0 project file");
		sb.AppendLine("FBXHeaderExtension:  {");
		sb.AppendLine("\tFBXHeaderVersion: 1003");
		sb.AppendLine("\tFBXVersion: 7400");
		sb.AppendLine("\tCreator: \"Creation Master 26 MeshKit\"");
		sb.AppendLine("\tCreationTimeStamp:  {");
		sb.AppendLine("\t\tVersion: 1000");
		sb.AppendLine("\t\tYear: " + DateTime.UtcNow.Year);
		sb.AppendLine("\t\tMonth: " + DateTime.UtcNow.Month);
		sb.AppendLine("\t\tDay: " + DateTime.UtcNow.Day);
		sb.AppendLine("\t\tHour: " + DateTime.UtcNow.Hour);
		sb.AppendLine("\t\tMinute: " + DateTime.UtcNow.Minute);
		sb.AppendLine("\t\tSecond: " + DateTime.UtcNow.Second);
		sb.AppendLine("\t\tMillisecond: " + DateTime.UtcNow.Millisecond);
		sb.AppendLine("\t}");
		sb.AppendLine("}");
	}

	private static void WriteGlobalSettings(StringBuilder sb)
	{
		sb.AppendLine("GlobalSettings:  {");
		sb.AppendLine("\tVersion: 1000");
		sb.AppendLine("\tProperties70:  {");
		sb.AppendLine("\t\tP: \"UpAxis\", \"int\", \"Integer\", \"\",1");
		sb.AppendLine("\t\tP: \"UpAxisSign\", \"int\", \"Integer\", \"\",1");
		sb.AppendLine("\t\tP: \"FrontAxis\", \"int\", \"Integer\", \"\",2");
		sb.AppendLine("\t\tP: \"FrontAxisSign\", \"int\", \"Integer\", \"\",1");
		sb.AppendLine("\t\tP: \"CoordAxis\", \"int\", \"Integer\", \"\",0");
		sb.AppendLine("\t\tP: \"CoordAxisSign\", \"int\", \"Integer\", \"\",1");
		sb.AppendLine("\t\tP: \"UnitScaleFactor\", \"double\", \"Number\", \"\",100");
		sb.AppendLine("\t\tP: \"OriginalUnitScaleFactor\", \"double\", \"Number\", \"\",100");
		sb.AppendLine("\t}");
		sb.AppendLine("}");
	}

	private void ExportLod(StringBuilder sb, string meshName, MeshSetLod lod, byte[] data, string? textureFileName, ref long nextObjectId)
	{
		foreach (var section in lod.Sections)
		{
			if (string.IsNullOrEmpty(section.Name) || !lod.IsSectionRenderable(section) ||
				section.GeometryDeclDesc.Length == 0)
				continue;

			var geometry = new SectionGeometry(section, lod, data);
			if (geometry.Positions.Count == 0 || geometry.Triangles.Count == 0) continue;

			var actorName = Sanitize($"{meshName}_{lod.ShortName}_{section.Name}");
			if (string.IsNullOrEmpty(actorName)) actorName = "mesh_" + nextObjectId;
			var geomId = nextObjectId++;
			var modelId = nextObjectId++;

			// Geometry
			sb.Append("\tGeometry: ").Append(geomId).Append(", \"Geometry::").Append(actorName).Append("\", \"Mesh\" {").AppendLine();
			sb.AppendLine("\t\tVertices: *" + (geometry.Positions.Count * 3) + " {");
			sb.AppendLine("\t\t\ta: " + JoinFloats(geometry.Positions));
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t\tPolygonVertexIndex: *" + (geometry.Triangles.Count * 3) + " {");
			sb.AppendLine("\t\t\ta: " + JoinPolygon(geometry));
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t\tGeometryVersion: 124");
			if (geometry.NormalCount > 0)
			{
				sb.AppendLine("\t\tLayerElementNormal: 0 {");
				sb.AppendLine("\t\t\tVersion: 101");
				sb.AppendLine("\t\t\tName: \"\"");
				sb.AppendLine("\t\t\tMappingInformationType: \"ByPolygonVertex\"");
				sb.AppendLine("\t\t\tReferenceInformationType: \"Direct\"");
				sb.AppendLine("\t\t\tNormals: *" + (geometry.Triangles.Count * 3) + " {");
				sb.AppendLine("\t\t\t\ta: " + JoinFloats(geometry.AggregatedNormals));
				sb.AppendLine("\t\t\t}");
				sb.AppendLine("\t\t}");
			}
			if (geometry.UvCount > 0)
			{
				sb.AppendLine("\t\tLayerElementUV: 0 {");
				sb.AppendLine("\t\t\tVersion: 101");
				sb.AppendLine("\t\t\tName: \"UVChannel_1\"");
				sb.AppendLine("\t\t\tMappingInformationType: \"ByPolygonVertex\"");
				sb.AppendLine("\t\t\tReferenceInformationType: \"Direct\"");
				sb.AppendLine("\t\t\tUV: *" + (geometry.Triangles.Count * 2) + " {");
				sb.AppendLine("\t\t\t\ta: " + JoinUvs(geometry.AggregatedUvs));
				sb.AppendLine("\t\t\t}");
				sb.AppendLine("\t\t}");
			}
			if (!string.IsNullOrEmpty(textureFileName))
			{
				sb.AppendLine("\t\tLayerElementMaterial: 0 {");
				sb.AppendLine("\t\t\tVersion: 101");
				sb.AppendLine("\t\t\tName: \"\"");
				sb.AppendLine("\t\t\tMappingInformationType: \"ByPolygon\"");
				sb.AppendLine("\t\t\tReferenceInformationType: \"IndexToDirect\"");
				sb.AppendLine("\t\t\tMaterials: *1 {");
				sb.AppendLine("\t\t\t\ta: 0");
				sb.AppendLine("\t\t\t}");
				sb.AppendLine("\t\t}");
			}
			sb.AppendLine("\t\tLayer: 0 {");
			sb.AppendLine("\t\t\tVersion: 100");
			if (geometry.NormalCount > 0)
			{
				sb.AppendLine("\t\t\tLayerElement:  {");
				sb.AppendLine("\t\t\t\tType: \"LayerElementNormal\"");
				sb.AppendLine("\t\t\t\tTypedIndex: 0");
				sb.AppendLine("\t\t\t}");
			}
			if (geometry.UvCount > 0)
			{
				sb.AppendLine("\t\t\tLayerElement:  {");
				sb.AppendLine("\t\t\t\tType: \"LayerElementUV\"");
				sb.AppendLine("\t\t\t\tTypedIndex: 0");
				sb.AppendLine("\t\t\t}");
			}
			if (!string.IsNullOrEmpty(textureFileName))
			{
				sb.AppendLine("\t\t\tLayerElement:  {");
				sb.AppendLine("\t\t\t\tType: \"LayerElementMaterial\"");
				sb.AppendLine("\t\t\t\tTypedIndex: 0");
				sb.AppendLine("\t\t\t}");
			}
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");

			// Model
			sb.Append("Model: ").Append(modelId).Append(", \"Model::").Append(actorName).Append("\", \"Mesh\" {").AppendLine();
			sb.AppendLine("\t\tVersion: 232");
			sb.AppendLine("\t\tProperties70:  {");
			sb.AppendLine("\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",1,1,1");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t\tShading: T");
			sb.AppendLine("\t\tCulling: \"CullingOff\"");
			sb.AppendLine("\t}");

			connections.Add("C: \"OO\"," + geomId + "," + modelId);
			connections.Add("C: \"OO\"," + modelId + ",0");

			if (boneCount > 0 && boneModelIds.Count > 0 && lod.Type is MeshType.Skinned or MeshType.Composite)
				ExportSkin(sb, geometry, section, geomId, lod.Type, ref nextObjectId);

			if (!string.IsNullOrEmpty(textureFileName))
			{
				var materialId = nextObjectId++;
				var textureId = nextObjectId++;
				sb.Append("Material: ").Append(materialId).Append(", \"Material::").Append(actorName).Append("\", \"\" {").AppendLine();
				sb.AppendLine("\t\tVersion: 102");
				sb.AppendLine("\t\tShadingModel: \"phong\"");
				sb.AppendLine("\t\tShadingProperties:  {");
				sb.AppendLine("\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",0.5,0.5,0.5");
				sb.AppendLine("\t\t}");
				sb.AppendLine("\t}");
				sb.AppendLine("\tTexture: " + textureId + ", \"Texture::" + actorName + "\", \"\" {");
				sb.AppendLine("\t\tType: \"TextureVideoClip\"");
				sb.AppendLine("\t\tVersion: 202");
				sb.AppendLine("\t\tTextureName: \"Texture::" + actorName + "\"");
				sb.AppendLine("\t\tFileName: \"" + textureFileName + "\"");
				sb.AppendLine("\t\tRelativeFilename: \"" + textureFileName + "\"");
				sb.AppendLine("\t\tModelUVTranslation: 0,0");
				sb.AppendLine("\t\tModelUVScaling: 1,1");
				sb.AppendLine("\t\tTexture_Alpha_Source: \"None\"");
				sb.AppendLine("\t\tCropping: 0,0,0,0");
				sb.AppendLine("\t}");
				connections.Add("C: \"OO\"," + materialId + "," + modelId);
				connections.Add("C: \"OP\"," + textureId + "," + materialId + ",\"DiffuseColor\",\"\"");
			}
		}
	}

	/// <summary>Creates an ASCII skeleton: a LimbNode root plus one child per
	/// skeleton bone. Names mirror FET's Composite export so existing tools and
	/// UI expectations that reference PART_&lt;n&gt; keep working.</summary>
	private void CreateSkeletonNodes(StringBuilder sb, ref long nextObjectId)
	{
		boneCount = mesh.PartCount;
		skeletonRootId = nextObjectId++;
		sb.Append("Model: ").Append(skeletonRootId).Append(", \"ROOT\", \"LimbNode\" {").AppendLine();
		sb.AppendLine("\t\tVersion: 232");
		sb.AppendLine("\t\tProperties70:  {");
		sb.AppendLine("\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",0,0,0");
		sb.AppendLine("\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\",0,0,0");
		sb.AppendLine("\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",1,1,1");
		sb.AppendLine("\t\t}");
		sb.Append("\t\tShading: F").AppendLine();
		sb.Append("\t\tCulling: \"CullingOff\"").AppendLine();
		sb.AppendLine("\t}");
		connections.Add("C: \"OO\"," + skeletonRootId + ",0");

		for (var boneIndex = 0; boneIndex < boneCount; boneIndex++)
		{
			var boneModelId = nextObjectId++;
			boneModelIds.Add(boneModelId);
			var boneName = "PART_" + boneIndex.ToString(CultureInfo.InvariantCulture);
			sb.Append("Model: ").Append(boneModelId).Append(", \"").Append(boneName).Append("\", \"LimbNode\" {").AppendLine();
			sb.AppendLine("\t\tVersion: 232");
			sb.AppendLine("\t\tProperties70:  {");
			sb.AppendLine("\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\",0,0,0");
			sb.AppendLine("\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",1,1,1");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");
			connections.Add("C: \"OO\"," + boneModelId + "," + skeletonRootId);
		}
	}

	/// <summary>Mirrors FET's FBXCreateSkin for the composite skeleton: every
	/// vertex that names a bone is added as a control-point index on the cluster
	/// linked to that bone. No EBX skeleton is required; skin is self-contained.</summary>
	private void ExportSkin(StringBuilder sb, SectionGeometry geometry, MeshSetSection section,
		long geomId, MeshType lodType, ref long nextObjectId)
	{
		var influences = geometry.BoneInfluences;
		if (influences == null) return;

		var skeletonId = nextObjectId++;
		sb.AppendLine("\tDeformer: " + skeletonId + ", \"Skin\", \"Skin\" {");
		sb.AppendLine("\t\tVersion: 101");
		sb.AppendLine("\t\tType: \"Skin\"");
		sb.AppendLine("\t}");
		connections.Add("C: \"OO\"," + skeletonId + "," + geomId);

		var boneWeights = new SortedDictionary<int, Dictionary<int, float>>();
		for (var vertexIndex = 0; vertexIndex < influences.Count; vertexIndex++)
		{
			var perVertex = influences[vertexIndex];
			if (perVertex == null) continue;
			for (var s = 0; s < perVertex.Length; s++)
			{
				var influence = perVertex[s];
				if (influence.Weight <= 0f || influence.LocalIndex >= section.BoneList.Length) continue;
				var subIndex = section.BoneList[influence.LocalIndex];
				if ((subIndex & 0x8000) != 0)
					subIndex = (ushort)(subIndex - 32768 + boneCount);
				if (subIndex >= boneModelIds.Count) continue;
				if (lodType == MeshType.Composite)
					influence = influence with { Weight = 1f };
				if (!boneWeights.TryGetValue(subIndex, out var perVertexWeights))
					boneWeights[subIndex] = perVertexWeights = new Dictionary<int, float>();
				perVertexWeights[vertexIndex] = influence.Weight;
			}
		}

		foreach (var (boneSubIndex, set) in boneWeights)
		{
			var clusterId = nextObjectId++;
			var boneModelId = boneModelIds[boneSubIndex];
			var (indices, weights) = (new List<int>(set.Count), new List<float>(set.Count));
			foreach (var (vertexIndex, weight) in set)
			{
				indices.Add(vertexIndex);
				weights.Add(weight);
			}

			sb.AppendLine("\tDeformer: " + clusterId + ", \"Cluster\", \"Cluster\" {");
			sb.AppendLine("\t\tVersion: 100");
			sb.AppendLine("\t\tMode: \"LetalOne\"");
			sb.AppendLine("\t\tIndexes: *" + indices.Count + " {");
			sb.AppendLine("\t\t\ta: " + JoinIntegers(indices));
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t\tWeights: *" + weights.Count + " {");
			sb.AppendLine("\t\t\ta: " + JoinWeights(weights));
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t\tTransform: *16 {");
			sb.AppendLine("\t\t\ta: 1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t\tTransformLink: *16 {");
			sb.AppendLine("\t\t\ta: 1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1");
			sb.AppendLine("\t\t}");
			sb.AppendLine("\t}");
			connections.Add("C: \"OO\"," + clusterId + "," + skeletonId);
			connections.Add("C: \"OO\"," + boneModelId + "," + clusterId);
		}
	}

	private static string JoinIntegers(IReadOnlyCollection<int> values)
	{
		var sb = new StringBuilder();
		foreach (var value in values)
		{
			if (sb.Length > 0) sb.Append(',');
			sb.Append(value.ToString(CultureInfo.InvariantCulture));
		}
		return sb.ToString();
	}

	private static string JoinWeights(IReadOnlyCollection<float> values)
	{
		var sb = new StringBuilder();
		foreach (var value in values)
		{
			if (sb.Length > 0) sb.Append(',');
			sb.Append(value.ToString("G9", CultureInfo.InvariantCulture));
		}
		return sb.ToString();
	}

	private static string Sanitize(string name)
	{
		var buffer = name
			.Replace('\\', '_').Replace('/', '_').Replace(':', '_')
			.Replace(';', '_').Replace('{', '(').Replace('}', ')')
			.Trim();
		return string.IsNullOrEmpty(buffer) ? "mesh" : buffer;
	}

	private static string JoinFloats(IEnumerable<Vector3> values)
	{
		var sb = new StringBuilder();
		foreach (var value in values)
		{
			if (sb.Length > 0) sb.Append(',');
			sb.Append(value.X.ToString("G9", CultureInfo.InvariantCulture)).Append(',')
			  .Append(value.Y.ToString("G9", CultureInfo.InvariantCulture)).Append(',')
			  .Append(value.Z.ToString("G9", CultureInfo.InvariantCulture));
		}
		return sb.ToString();
	}

	private static string JoinPolygon(SectionGeometry geometry)
	{
		var sb = new StringBuilder();
		for (var i = 0; i < geometry.Triangles.Count; i++)
		{
			var tri = geometry.Triangles[i];
			if (sb.Length > 0) sb.Append(',');
			sb.Append(tri[0].ToString(CultureInfo.InvariantCulture)).Append(',')
			  .Append(tri[1].ToString(CultureInfo.InvariantCulture)).Append(',')
			  .Append((-(tri[2] + 1)).ToString(CultureInfo.InvariantCulture));
		}
		return sb.ToString();
	}

	private static string JoinUvs(IEnumerable<Vector2> values)
	{
		var sb = new StringBuilder();
		foreach (var value in values)
		{
			if (sb.Length > 0) sb.Append(',');
			sb.Append(value.X.ToString("G9", CultureInfo.InvariantCulture)).Append(',')
			  .Append(value.Y.ToString("G9", CultureInfo.InvariantCulture));
		}
		return sb.ToString();
	}
}