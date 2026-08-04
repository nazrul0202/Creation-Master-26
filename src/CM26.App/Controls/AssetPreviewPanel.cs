using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CM26.App.Theming;
using CM26.Application.Services;

namespace CM26.App.Controls;

/// <summary>The three honest display states for an asset preview.</summary>
public enum AssetPreviewState
{
    /// <summary>No asset configured / none exists for this record.</summary>
    Unavailable,
    /// <summary>A real file is being decoded on a background thread.</summary>
    Loading,
    /// <summary>A real decoded image is displayed.</summary>
    Loaded,
    /// <summary>The file exists but could not be decoded (corrupt / unsupported).</summary>
    Unsupported,
}

/// <summary>
/// Shared, reusable asset-preview control used by every section. Shows a real decoded image
/// when one exists; otherwise a clearly labelled honest state. Decode happens off the UI thread
/// with cancellation, so selecting through a list stays responsive. The source file is never
/// locked. No third-party native object is exposed — only a managed <see cref="Image"/>.
/// </summary>
public sealed class AssetPreviewPanel : UserControl
{
    private readonly ITexturePreviewService _textures;
    private readonly PictureBox _picture;
    private readonly Label _stateLabel;
    private readonly Label _captionLabel;
    private Image? _current;                 // owned; disposed on replace
    private CancellationTokenSource? _cts;
    private long _requestSerial;

    public AssetPreviewPanel(ITexturePreviewService textures)
    {
        _textures = textures;
        BackColor = Theme.Raised;
        Size = new Size(180, 200);

        _picture = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
        };
        _stateLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Muted,
            Font = Theme.Muted9,
            BackColor = Color.Transparent,
            AutoEllipsis = true,
        };
        _captionLabel = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 20,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.Muted,
            Font = Theme.Muted9,
            BackColor = Theme.Panel,
            AutoEllipsis = true,
            Visible = false,
        };

        Controls.Add(_picture);
        Controls.Add(_stateLabel);
        Controls.Add(_captionLabel);

        // subtle border
        Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.Border);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        };
        Disposed += (_, _) => CancelAndDispose();
    }

    public AssetPreviewState State { get; private set; } = AssetPreviewState.Unavailable;

    /// <summary>Show a clear, honest "no asset" state. Used when no real file exists.</summary>
    public void ShowUnavailable(string reason = "No local asset")
    {
        CancelPending();
        SetImage(null);
        State = AssetPreviewState.Unavailable;
        _picture.Visible = false;
        _stateLabel.Visible = true;
        _stateLabel.Text = reason;
        _stateLabel.ForeColor = Theme.Muted;
        _captionLabel.Visible = false;
    }

    /// <summary>Load and preview a real file. Decode is async + cancellable.</summary>
    public void ShowAsset(string filePath, string? caption = null)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            ShowUnavailable("No local asset");
            return;
        }

        CancelPending();
        State = AssetPreviewState.Loading;
        _picture.Visible = false;
        _stateLabel.Visible = true;
        _stateLabel.Text = "Loading…";
        _stateLabel.ForeColor = Theme.Muted;

        var cts = new CancellationTokenSource();
        _cts = cts;
        long serial = Interlocked.Increment(ref _requestSerial);
        int maxW = Math.Max(64, Width - 4);
        int maxH = Math.Max(64, Height - (_captionLabel.Visible ? 24 : 4));

        Task.Run(() =>
        {
            Image? img = null;
            string? error = null;
            try
            {
                img = _textures.CreatePreview(filePath, maxW, maxH, cts.Token);
                if (img == null) error = "Unsupported or corrupt image";
            }
            catch (OperationCanceledException) { return; }
            catch (OutOfMemoryException) { error = "Image file may be corrupt"; }
            catch { error = "Could not read image"; }

            if (cts.IsCancellationRequested) { img?.Dispose(); return; }
            TextureMetadata meta = new();
            try { meta = _textures.ReadMetadata(filePath); }
            catch { /* metadata is optional */ }
            BeginInvoke(() =>
            {
                if (serial != _requestSerial || IsDisposed) { img?.Dispose(); return; }
                if (img != null)
                {
                    SetImage(img);
                    State = AssetPreviewState.Loaded;
                    _picture.Visible = true;
                    _stateLabel.Visible = false;
                    if (!string.IsNullOrEmpty(caption))
                    {
                        _captionLabel.Text = caption;
                        _captionLabel.Visible = true;
                    }
                    else if (meta.IsReadable)
                    {
                        _captionLabel.Text = $"{meta.Width}×{meta.Height} {meta.Format}";
                        _captionLabel.Visible = true;
                    }
                }
                else
                {
                    SetImage(null);
                    State = AssetPreviewState.Unsupported;
                    _picture.Visible = false;
                    _stateLabel.Visible = true;
                    _stateLabel.Text = error ?? "Unsupported or corrupt image";
                    _stateLabel.ForeColor = Theme.Warning;
                    _captionLabel.Visible = false;
                }
            });
        }, cts.Token);
    }

    private void SetImage(Image? img)
    {
        var old = _current;
        _current = img;
        _picture.Image = img;
        old?.Dispose();
    }

    private void CancelPending()
    {
        Interlocked.Increment(ref _requestSerial);
        try { _cts?.Cancel(); } catch { }
        _cts?.Dispose();
        _cts = null;
    }

    private void CancelAndDispose()
    {
        CancelPending();
        SetImage(null);
    }
}
