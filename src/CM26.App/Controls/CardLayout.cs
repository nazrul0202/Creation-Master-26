using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CM26.App.Theming;

namespace CM26.App.Controls;

/// <summary>
/// Shared card/workbench layout helpers used by every section to achieve a
/// consistent white-card look. All methods are static and return the created
/// control so the caller can dock, position, or add children.
/// </summary>
public static class CardLayout
{
    // Palette — matches the PlayersSection light card style. These are switched
    // by ApplyTheme() so the card design follows the app-wide light/dark mode.
    public static Color CardBackground = Color.FromArgb(235, 237, 234);
    public static Color CardWhite = Color.White;
    public static Color CardText = Color.FromArgb(37, 37, 34);
    public static Color CardMuted = Color.FromArgb(94, 108, 57);
    public static Color CardSubtle = Color.FromArgb(106, 110, 101);
    public static Color CardFieldLabel = Color.FromArgb(55, 55, 51);
    public static Color CardFieldBg = Color.FromArgb(246, 248, 244);

    /// <summary>
    /// Switches the card palette to match the current app theme. Sections are
    /// rebuilt on a theme toggle, so this only needs to run once before any
    /// card is created (the Theme palette hook keeps it in sync automatically).
    /// </summary>
    public static void ApplyTheme()
    {
        if (Theme.IsDark)
        {
            CardBackground = Color.FromArgb(30, 31, 35);
            CardWhite = Color.FromArgb(43, 45, 51);
            CardText = Color.FromArgb(224, 224, 224);
            CardMuted = Color.FromArgb(158, 163, 148);
            CardSubtle = Color.FromArgb(170, 172, 178);
            CardFieldLabel = Color.FromArgb(205, 206, 210);
            CardFieldBg = Color.FromArgb(51, 53, 60);
        }
        else
        {
            CardBackground = Color.FromArgb(235, 237, 234);
            CardWhite = Color.White;
            CardText = Color.FromArgb(37, 37, 34);
            CardMuted = Color.FromArgb(94, 108, 57);
            CardSubtle = Color.FromArgb(106, 110, 101);
            CardFieldLabel = Color.FromArgb(55, 55, 51);
            CardFieldBg = Color.FromArgb(246, 248, 244);
        }
    }

    // FC26 franchise accent colours
    public static readonly Color Fc26Green = Color.FromArgb(116, 185, 34);
    public static readonly Color Fc26Red = Color.FromArgb(210, 54, 62);
    public static readonly Color Fc26Blue = Color.FromArgb(57, 160, 197);
    public static readonly Color Fc26Yellow = Color.FromArgb(232, 175, 33);
    public static readonly Color Fc26Purple = Color.FromArgb(190, 95, 219);
    public static readonly Color Fc26Orange = Color.FromArgb(224, 100, 79);
    public static readonly Color Fc26Dark = Color.FromArgb(20, 42, 63);

    /// <summary>Creates the outermost card container that fills a scrollable canvas.</summary>
    public static Panel CreateCard(int width, int height)
    {
        var card = new Panel
        {
            Location = new Point(12, 12),
            Size = new Size(width, height),
            BackColor = CardBackground,
        };
        return card;
    }

    /// <summary>
    /// Creates a white rounded-rectangle header panel with a colored left accent
    /// bar. The caller adds children (face, name, meta, metrics) inside it.
    /// </summary>
    public static Panel CreateHeader(int width, int height, Color accent)
    {
        var header = new Panel
        {
            Location = new Point(16, 16),
            Size = new Size(width, height),
            BackColor = CardWhite,
        };
        ApplyRounded(header, 14);
        // Left accent bar
        header.Controls.Add(new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(6, height),
            BackColor = accent,
        });
        return header;
    }

    /// <summary>
    /// Creates a white rounded-rectangle group card with a colored top accent bar
    /// and a title label. Used for attribute groups (ATTACKING, SKILL, etc.) and
    /// info sections (TEAM INFO, CONTRACT, etc.).
    /// </summary>
    public static Panel CreateGroup(Control parent, string title, Color accent, int x, int y, int width = 418, int height = 160)
    {
        var group = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = CardWhite,
        };
        ApplyRounded(group, 12);
        // Top accent bar
        group.Controls.Add(new Panel
        {
            Location = Point.Empty,
            Size = new Size(width, 4),
            BackColor = accent,
            Dock = DockStyle.Top,
        });
        // Keep the title entirely above the first content row.  Several legacy
        // editors deliberately start their first field at Y=20; the previous
        // 12..32 title band painted over those fields after a real record was
        // loaded.  This compact 6..20 band is the shared content contract.
        group.Controls.Add(new Label
        {
            Text = title,
            Location = new Point(14, 6),
            Size = new Size(width - 28, 14),
            Font = Theme.Muted9,
            ForeColor = accent,
        });
        parent.Controls.Add(group);
        return group;
    }

    /// <summary>
    /// Creates a small info tile (like OVR / POT / GRO) with a large number
    /// and a subtitle label.
    /// </summary>
    public static (Panel Tile, Label Value) CreateTile(Control parent, string title, int x, int y, Color accent, int width = 90, int height = 112)
    {
        var tile = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = accent,
        };
        ApplyRounded(tile, 14);
        var value = new Label
        {
            Location = new Point(5, 10),
            Size = new Size(width - 10, 54),
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
        };
        tile.Controls.Add(value);
        tile.Controls.Add(new Label
        {
            Text = title,
            Location = new Point(4, height - 39),
            Size = new Size(width - 8, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = Theme.BodyBold,
            ForeColor = Color.White,
        });
        parent.Controls.Add(tile);
        return (tile, value);
    }

    /// <summary>
    /// Creates a fact block (label + value) inside a parent panel. Used for
    /// "PLAYER INFO", "TEAM INFO", etc.
    /// </summary>
    public static Label CreateFact(Control parent, string title, int x, int y, int width = 196, string suffix = "")
    {
        var block = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(width, 58),
            BackColor = CardFieldBg,
        };
        ApplyRounded(block, 8);
        block.Controls.Add(new Label
        {
            Text = title.ToUpperInvariant(),
            Location = new Point(10, 6),
            Size = new Size(width - 20, 16),
            Font = new Font(Theme.Body, FontStyle.Bold),
            ForeColor = CardSubtle,
        });
        var value = new Label
        {
            Location = new Point(10, 25),
            Size = new Size(width - 20, 26),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = CardText,
            AutoEllipsis = true,
            Tag = suffix,
        };
        block.Controls.Add(value);
        parent.Controls.Add(block);
        return value;
    }

    /// <summary>
    /// Adds an editable attribute row (label + textbox) inside a group panel.
    /// Returns the TextBox for data binding.
    /// </summary>
    public static TextBox AddFieldRow(Control group, string label, string field, int y, bool readOnly = false)
    {
        group.Controls.Add(new Label
        {
            Text = label,
            Location = new Point(14, y),
            Size = new Size(305, 18),
            Font = Theme.Body,
            ForeColor = CardFieldLabel,
        });
        var editor = new TextBox
        {
            Location = new Point(336, y - 1),
            Size = new Size(64, 20),
            BorderStyle = BorderStyle.None,
            TextAlign = HorizontalAlignment.Right,
            Font = Theme.BodyBold,
            ForeColor = CardText,
            BackColor = CardWhite,
            ReadOnly = readOnly,
            Tag = field,
        };
        group.Controls.Add(editor);
        return editor;
    }

    /// <summary>
    /// Creates a read-only attribute row (label + value label) inside a group panel.
    /// </summary>
    public static Label AddReadOnlyRow(Control group, string label, int y, int valueWidth = 64)
    {
        group.Controls.Add(new Label
        {
            Text = label,
            Location = new Point(14, y),
            Size = new Size(305, 18),
            Font = Theme.Body,
            ForeColor = CardFieldLabel,
        });
        var value = new Label
        {
            Location = new Point(336, y),
            Size = new Size(valueWidth, 18),
            Font = Theme.BodyBold,
            ForeColor = CardText,
            TextAlign = ContentAlignment.MiddleRight,
        };
        group.Controls.Add(value);
        return value;
    }

    /// <summary>
    /// Applies rounded corners to a control via a GraphicsPath region.
    /// </summary>
    public static void ApplyRounded(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0) return;
        var d = Math.Min(radius * 2, Math.Min(control.Width, control.Height));
        var rect = new Rectangle(0, 0, control.Width, control.Height);
        using var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        control.Region?.Dispose();
        control.Region = new Region(path);
    }

    /// <summary>
    /// Lightens a color by blending toward white.
    /// </summary>
    public static Color Lighten(Color color, int amount)
    {
        amount = Math.Clamp(amount, 0, 255);
        return Color.FromArgb(
            color.R + (255 - color.R) * amount / 255,
            color.G + (255 - color.G) * amount / 255,
            color.B + (255 - color.B) * amount / 255);
    }

    /// <summary>
    /// FC26-style header: white rounded panel with left green accent bar,
    /// logo/face picture, title, and meta line. Returns the header panel
    /// so callers can add metric tiles or other elements inside it.
    /// </summary>
    public static Panel CreateFc26Header(int width, int height)
    {
        var header = new Panel
        {
            Location = new Point(16, 16),
            Size = new Size(width, height),
            BackColor = CardWhite,
        };
        ApplyRounded(header, 14);
        header.Controls.Add(new Panel
        {
            Location = Point.Empty,
            Size = new Size(6, height),
            BackColor = Fc26Green,
        });
        return header;
    }

    /// <summary>
    /// Adds an FC26-style metric tile (e.g. PAC, SHO, PAS) to the header.
    /// </summary>
    public static (Panel Tile, Label Value) AddFc26Metric(Control parent, string code, int x, Color accent)
    {
        var metric = new Panel
        {
            Location = new Point(x, 96),
            Size = new Size(106, 32),
            BackColor = accent,
        };
        ApplyRounded(metric, 8);
        var value = new Label
        {
            Location = new Point(11, 4),
            Size = new Size(40, 23),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleLeft,
        };
        metric.Controls.Add(value);
        metric.Controls.Add(new Label
        {
            Text = code,
            Location = new Point(47, 5),
            Size = new Size(52, 20),
            Font = Theme.BodyBold,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleRight,
        });
        parent.Controls.Add(metric);
        return (metric, value);
    }
}
