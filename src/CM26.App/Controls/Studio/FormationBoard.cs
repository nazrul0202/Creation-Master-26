using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Modern dark football pitch with 11 draggable player markers.
/// </summary>
public sealed class FormationBoard : Control
{
    private readonly List<FormationSlot> _slots = new();
    private FormationSlot? _draggedSlot;
    private FormationSlot? _selectedSlot;

    public event EventHandler<FormationSlotEventArgs>? SlotClicked;
    public event EventHandler<FormationDropEventArgs>? PlayerDropped;

    public FormationBoard()
    {
        DoubleBuffered = true;
        BackColor = StudioColors.PitchGreen;
        ForeColor = StudioColors.PitchLine;
        AllowDrop = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    public void ClearSlots() => _slots.Clear();

    public void AddSlot(FormationSlot slot)
    {
        _slots.Add(slot);
        Invalidate();
    }

    public void SetSlots(IEnumerable<FormationSlot> slots)
    {
        _slots.Clear();
        _slots.AddRange(slots);
        Invalidate();
    }

    public FormationSlot? SelectedSlot
    {
        get => _selectedSlot;
        set
        {
            _selectedSlot = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        DrawPitch(g);

        foreach (var slot in _slots)
            DrawSlot(g, slot);

        base.OnPaint(e);
    }

    private void DrawPitch(Graphics g)
    {
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var bg = new LinearGradientBrush(rect, StudioColors.PitchGreen, Color.FromArgb(0x0A, 0x2E, 0x1E), LinearGradientMode.Vertical);
        g.FillRectangle(bg, rect);

        using var line = new Pen(Color.FromArgb(120, StudioColors.PitchLine), 2f);
        g.DrawRectangle(line, rect);

        // Half-way line
        g.DrawLine(line, rect.Width / 2, 0, rect.Width / 2, rect.Height);
        // Center circle
        var centerX = rect.Width / 2;
        var centerY = rect.Height / 2;
        var radius = Math.Min(rect.Width, rect.Height) / 10;
        g.DrawEllipse(line, centerX - radius, centerY - radius, radius * 2, radius * 2);
        // Penalty boxes
        var boxW = rect.Width * 0.18;
        var boxH = rect.Height * 0.35;
        g.DrawRectangle(line, 0, (int)(centerY - boxH / 2), (int)boxW, (int)boxH);
        g.DrawRectangle(line, rect.Width - (int)boxW, (int)(centerY - boxH / 2), (int)boxW, (int)boxH);
    }

    private void DrawSlot(Graphics g, FormationSlot slot)
    {
        var x = (int)(slot.RelativeX * Width);
        var y = (int)(slot.RelativeY * Height);
        var size = Math.Min(64, Math.Max(48, Width / 16));
        var rect = new Rectangle(x - size / 2, y - size / 2, size, size);

        var isSelected = ReferenceEquals(slot, _selectedSlot);
        var markerColor = isSelected ? StudioColors.CyanAccent : StudioColors.Surface;

        using var path = RoundedRect(rect, size / 4);
        using var bg = new SolidBrush(markerColor);
        g.FillPath(bg, path);

        if (isSelected)
        {
            using var glow = new Pen(StudioColors.CyanAccent, 2f);
            g.DrawPath(glow, path);
        }
        else
        {
            using var border = new Pen(StudioColors.CardBorder, 1f);
            g.DrawPath(border, path);
        }

        if (slot.Miniface != null)
        {
            var imgRect = new Rectangle(rect.X + 4, rect.Y + 4, size - 8, size - 8);
            g.DrawImage(slot.Miniface, imgRect);
        }
        else
        {
            TextRenderer.DrawText(g, slot.Position, StudioFonts.Chip, rect, StudioColors.CyanAccent,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        var labelY = rect.Bottom + 2;
        var labelRect = new Rectangle(rect.X - 20, labelY, rect.Width + 40, 16);
        TextRenderer.DrawText(g, slot.PlayerName, StudioFonts.Metadata, labelRect, StudioColors.PrimaryText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var badgeRect = new Rectangle(rect.X + size - 18, rect.Y - 6, 22, 16);
        using var badgePath = RoundedRect(badgeRect, 4);
        using var badgeBg = new SolidBrush(StudioColors.RatingSoftColor(slot.Overall));
        g.FillPath(badgeBg, badgePath);
        using var badgeBorder = new Pen(StudioColors.RatingColor(slot.Overall), 1f);
        g.DrawPath(badgeBorder, badgePath);
        TextRenderer.DrawText(g, slot.Overall.ToString(), StudioFonts.Metadata, badgeRect, StudioColors.RatingColor(slot.Overall),
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        var slot = HitTest(e.Location);
        if (slot != null)
        {
            _selectedSlot = slot;
            _draggedSlot = slot;
            SlotClicked?.Invoke(this, new FormationSlotEventArgs(slot));
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_draggedSlot != null && e.Button == MouseButtons.Left)
        {
            var dx = e.X - _draggedSlot.RelativeX * Width;
            var dy = e.Y - _draggedSlot.RelativeY * Height;
            if (Math.Abs(dx) > 5 || Math.Abs(dy) > 5)
                DoDragDrop(_draggedSlot, DragDropEffects.Move);
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _draggedSlot = null;
        base.OnMouseUp(e);
    }

    protected override void OnDragEnter(DragEventArgs drgevent)
    {
        drgevent.Effect = DragDropEffects.Move;
        Invalidate();
        base.OnDragEnter(drgevent);
    }

    protected override void OnDragDrop(DragEventArgs drgevent)
    {
        if (drgevent.Data == null) return;
        var client = PointToClient(new Point(drgevent.X, drgevent.Y));
        var slot = HitTest(client);
        if (drgevent.Data.GetDataPresent(typeof(int)))
        {
            var playerId = (int)drgevent.Data.GetData(typeof(int))!;
            PlayerDropped?.Invoke(this, new FormationDropEventArgs(playerId, slot, client));
        }
        else if (drgevent.Data.GetDataPresent(typeof(FormationSlot)) && slot != null)
        {
            var source = (FormationSlot)drgevent.Data.GetData(typeof(FormationSlot))!;
            PlayerDropped?.Invoke(this, new FormationDropEventArgs(source.PlayerId, slot, client));
        }
        Invalidate();
        base.OnDragDrop(drgevent);
    }

    private FormationSlot? HitTest(Point point)
    {
        FormationSlot? best = null;
        foreach (var slot in _slots)
        {
            var x = slot.RelativeX * Width;
            var y = slot.RelativeY * Height;
            var size = Math.Min(64, Math.Max(48, Width / 16));
            var dx = point.X - x;
            var dy = point.Y - y;
            if (dx * dx + dy * dy <= (size / 2f) * (size / 2f))
                best = slot;
        }
        return best;
    }

    private static GraphicsPath RoundedRect(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
        if (d <= 0) d = 1;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public sealed class FormationSlot
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int Overall { get; set; }
    public int Potential { get; set; }
    public Image? Miniface { get; set; }
    public float RelativeX { get; set; }
    public float RelativeY { get; set; }
    public int SlotIndex { get; set; }
}

public sealed class FormationSlotEventArgs : EventArgs
{
    public FormationSlot Slot { get; }
    public FormationSlotEventArgs(FormationSlot slot) => Slot = slot;
}

public sealed class FormationDropEventArgs : EventArgs
{
    public int PlayerId { get; }
    public FormationSlot? TargetSlot { get; }
    public Point DropLocation { get; }
    public FormationDropEventArgs(int playerId, FormationSlot? targetSlot, Point dropLocation)
    {
        PlayerId = playerId;
        TargetSlot = targetSlot;
        DropLocation = dropLocation;
    }
}
