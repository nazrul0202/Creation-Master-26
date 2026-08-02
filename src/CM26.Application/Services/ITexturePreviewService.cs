using System.Drawing;

namespace CM26.Application.Services;

/// <summary>Read-only metadata about a texture file. Never throws on a missing/corrupt file.</summary>
public sealed record TextureMetadata
{
    public bool IsReadable { get; init; }
    public string? Error { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int MipLevels { get; init; }
    public string Format { get; init; } = string.Empty;   // e.g. "DXT5/BC3", "PNG", "JPEG"
    public bool HasAlpha { get; init; }
    public long FileSizeBytes { get; init; }
    public string FilePath { get; init; } = string.Empty;
}

/// <summary>
/// Read-only texture preview service. Opens a texture file, reports metadata, and produces a
/// scaled preview <see cref="Image"/>. Implementations must:
/// preserve alpha, respect aspect ratio, expose mipmap metadata, dispose native/managed
/// resources, never lock the source file permanently, and tolerate corrupt/unsupported files
/// (reporting them via <see cref="TextureMetadata.IsReadable"/> instead of throwing).
/// No third-party native object is exposed to callers — only managed <see cref="Image"/>.
/// </summary>
public interface ITexturePreviewService
{
    /// <summary>True if the file extension/exists and the service can attempt a decode.</summary>
    bool CanOpen(string filePath);

    /// <summary>Read header/metadata without decoding the full image. Never throws.</summary>
    TextureMetadata ReadMetadata(string filePath);

    /// <summary>
    /// Create a scaled preview fitting within (maximumWidth, maximumHeight), preserving aspect
    /// ratio and alpha. Returns null when the file genuinely cannot be decoded (caller shows an
    /// honest labelled "unavailable" state). Throws nothing for I/O or format errors.
    /// </summary>
    Image? CreatePreview(string filePath, int maximumWidth, int maximumHeight, CancellationToken cancellationToken = default);
}
