using System.Text.Json.Serialization;

namespace CM26.AssetBridge;

public sealed record BridgeRequest(
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("gameRoot")] string? GameRoot = null,
    [property: JsonPropertyName("query")] string? Query = null,
    [property: JsonPropertyName("assetType")] string? AssetType = null,
    [property: JsonPropertyName("outputPath")] string? OutputPath = null,
    [property: JsonPropertyName("maxResults")] int MaxResults = 50);

public sealed record BridgeResponse(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("command")] string Command,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("inventory")] FrostbiteInventory? Inventory = null,
    [property: JsonPropertyName("assets")] IReadOnlyList<BridgeAssetResult>? Assets = null,
    [property: JsonPropertyName("outputPath")] string? OutputPath = null,
    [property: JsonPropertyName("audioBank")] AudioBankResult? AudioBank = null);

public sealed record AudioBankResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("endian")] string Endian,
    [property: JsonPropertyName("alignment")] int Alignment,
    [property: JsonPropertyName("version")] byte Version,
    [property: JsonPropertyName("bankKey")] uint BankKey,
    [property: JsonPropertyName("projectKey")] uint ProjectKey,
    [property: JsonPropertyName("dataSets")] IReadOnlyList<AudioDataSetResult> DataSets);

public sealed record AudioDataSetResult(
    [property: JsonPropertyName("id")] uint Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sampleGroupId")] uint SampleGroupId,
    [property: JsonPropertyName("rowCount")] int RowCount,
    [property: JsonPropertyName("fieldCount")] ushort FieldCount,
    [property: JsonPropertyName("indexCount")] ushort IndexCount);

public sealed record BridgeAssetResult(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sha1")] string Sha1,
    [property: JsonPropertyName("originalSize")] uint OriginalSize,
    [property: JsonPropertyName("compressedSize")] uint CompressedSize,
    [property: JsonPropertyName("resType")] uint ResType,
    [property: JsonPropertyName("resRid")] ulong ResRid);

public sealed record FrostbiteCodecMethodResult(
    [property: JsonPropertyName("method")] byte Method,
    [property: JsonPropertyName("methodData")] byte MethodData,
    [property: JsonPropertyName("blockCount")] long BlockCount,
    [property: JsonPropertyName("packedBytes")] long PackedBytes,
    [property: JsonPropertyName("unpackedBytes")] long UnpackedBytes);

public sealed record FrostbiteCodecAuditResult(
    [property: JsonPropertyName("indexedAssets")] int IndexedAssets,
    [property: JsonPropertyName("uniquePayloads")] int UniquePayloads,
    [property: JsonPropertyName("blockCount")] long BlockCount,
    [property: JsonPropertyName("unavailablePayloads")] int UnavailablePayloads,
    [property: JsonPropertyName("unavailableCasFiles")] IReadOnlyList<string> UnavailableCasFiles,
    [property: JsonPropertyName("errorCount")] int ErrorCount,
    [property: JsonPropertyName("methods")] IReadOnlyList<FrostbiteCodecMethodResult> Methods,
    [property: JsonPropertyName("errors")] IReadOnlyList<string> Errors);

public sealed record FrostbiteAssetCapabilityAuditResult(
    [property: JsonPropertyName("textureCount")] int TextureCount,
    [property: JsonPropertyName("textureFormats")] IReadOnlyDictionary<int, int> TextureFormats,
    [property: JsonPropertyName("unsupportedTextureFormats")] IReadOnlyDictionary<int, int> UnsupportedTextureFormats,
    [property: JsonPropertyName("meshCount")] int MeshCount,
    [property: JsonPropertyName("meshSectionCount")] int MeshSectionCount,
    [property: JsonPropertyName("unsupportedVertexFormats")] IReadOnlyDictionary<string, int> UnsupportedVertexFormats,
    [property: JsonPropertyName("unavailableCount")] int UnavailableCount,
    [property: JsonPropertyName("errorCount")] int ErrorCount,
    [property: JsonPropertyName("errors")] IReadOnlyList<string> Errors);

public sealed record FrostbiteTextureSampleResult(
    [property: JsonPropertyName("renderFormat")] int RenderFormat,
    [property: JsonPropertyName("assetName")] string AssetName,
    [property: JsonPropertyName("ddsPath")] string DdsPath);

public sealed record FrostbiteFile(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("length")] long Length,
    [property: JsonPropertyName("lastWriteUtcTicks")] long LastWriteUtcTicks);

public sealed record FrostbiteTocIndex(
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("flags")] uint Flags,
    [property: JsonPropertyName("bundleCount")] int BundleCount,
    [property: JsonPropertyName("chunkCount")] int ChunkCount,
    [property: JsonPropertyName("bundleNames")] IReadOnlyList<string> BundleNames);

public sealed record FrostbiteInventory(
    [property: JsonPropertyName("indexFormatVersion")] int IndexFormatVersion,
    [property: JsonPropertyName("gameRoot")] string GameRoot,
    [property: JsonPropertyName("layoutMagic")] string LayoutMagic,
    [property: JsonPropertyName("baseVersion")] int BaseVersion,
    [property: JsonPropertyName("headVersion")] int HeadVersion,
    [property: JsonPropertyName("superBundleCount")] int SuperBundleCount,
    [property: JsonPropertyName("catalogCount")] int CatalogCount,
    [property: JsonPropertyName("fingerprint")] string Fingerprint,
    [property: JsonPropertyName("containerCount")] int ContainerCount,
    [property: JsonPropertyName("tocCount")] int TocCount,
    [property: JsonPropertyName("parsedTocCount")] int ParsedTocCount,
    [property: JsonPropertyName("indexedBundleCount")] int IndexedBundleCount,
    [property: JsonPropertyName("indexedChunkCount")] int IndexedChunkCount,
    [property: JsonPropertyName("ebxAssetCount")] int EbxAssetCount,
    [property: JsonPropertyName("resAssetCount")] int ResAssetCount,
    [property: JsonPropertyName("bundleChunkAssetCount")] int BundleChunkAssetCount,
    [property: JsonPropertyName("uniqueAssetCount")] int UniqueAssetCount,
    [property: JsonPropertyName("assetIndexErrors")] IReadOnlyList<string> AssetIndexErrors,
    [property: JsonPropertyName("assetNameSamples")] IReadOnlyList<string> AssetNameSamples,
    [property: JsonPropertyName("tocErrors")] IReadOnlyList<string> TocErrors,
    [property: JsonPropertyName("tocIndexes")] IReadOnlyList<FrostbiteTocIndex> TocIndexes,
    [property: JsonPropertyName("files")] IReadOnlyList<FrostbiteFile> Files);
