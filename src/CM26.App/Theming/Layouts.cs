using System.Drawing.Drawing2D;
using CM26.App.Controls;

namespace CM26.App.Theming;

/// <summary>Responsive layout primitives shared by editor sections.</summary>
public static class Layouts
{
    public static GroupBox DockedGroup(string title, Color accent, DockStyle dock, int? height = null)
    {
        var group = new GroupBox
        {
            Text = title,
            Dock = dock,
            BackColor = CardLayout.CardWhite,
            ForeColor = accent,
            Font = Theme.BodyBold,
            Padding = new Padding(10, 24, 10, 10),
            Margin = new Padding(6),
        };
        if (height.HasValue) group.Height = height.Value;
        return group;
    }

    public static TextBox FieldRow(Control parent, string label, string fieldName, int labelWidth)
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 30,
            AutoSize = false,
            ColumnCount = 2,
            Margin = new Padding(0, 2, 0, 2),
            BackColor = parent.BackColor,
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, labelWidth));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            ForeColor = CardLayout.CardFieldLabel,
            Font = Theme.Body,
        }, 0, 0);
        var editor = new TextBox { Name = fieldName, Tag = fieldName, Dock = DockStyle.Fill, Font = Theme.Body };
        Theme.ApplyTextBox(editor);
        row.Controls.Add(editor, 1, 0);
        parent.Controls.Add(row);
        row.BringToFront();
        return editor;
    }
}

/// <summary>A resize-safe football pitch used by formation and team workspaces.</summary>
public sealed class RatableBoard : Panel
{
    public bool DrawBasePitch { get; set; } = true;

    public RatableBoard()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.FromArgb(43, 132, 82);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (DrawBasePitch) DrawPitch(e.Graphics, ClientRectangle, BackColor);
    }

    public static Rectangle DrawPitch(Graphics graphics, Rectangle bounds, Color background)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(background);
        var field = new Rectangle(9, 9, Math.Max(1, bounds.Width - 19), Math.Max(1, bounds.Height - 19));
        if (bounds.Width < 100 || bounds.Height < 100) return field;
        using (var stripe = new SolidBrush(Color.FromArgb(18, 255, 255, 255)))
        {
            var stripeHeight = Math.Max(1, field.Height / 8);
            for (var i = 0; i < 8; i += 2)
                graphics.FillRectangle(stripe, field.Left, field.Top + (i * stripeHeight), field.Width, stripeHeight);
        }
        using var pen = new Pen(Color.FromArgb(225, Color.White), 2);
        graphics.DrawRectangle(pen, field);
        var centre = new Point(field.Left + field.Width / 2, field.Top + field.Height / 2);
        var circle = Math.Max(30, Math.Min(90, Math.Min(field.Width, field.Height) / 5));
        graphics.DrawEllipse(pen, centre.X - circle / 2, centre.Y - circle / 2, circle, circle);
        graphics.FillEllipse(Brushes.White, centre.X - 3, centre.Y - 3, 6, 6);
        graphics.DrawLine(pen, field.Left, centre.Y, field.Right, centre.Y);
        var penaltyWidth = Math.Max(70, Math.Min(230, field.Width / 3));
        var penaltyHeight = Math.Max(30, Math.Min(64, field.Height / 6));
        var sixWidth = Math.Max(40, Math.Min(105, field.Width / 7));
        var sixHeight = Math.Max(16, Math.Min(25, field.Height / 14));
        graphics.DrawRectangle(pen, centre.X - penaltyWidth / 2, field.Top, penaltyWidth, penaltyHeight);
        graphics.DrawRectangle(pen, centre.X - sixWidth / 2, field.Top, sixWidth, sixHeight);
        graphics.DrawRectangle(pen, centre.X - penaltyWidth / 2, field.Bottom - penaltyHeight, penaltyWidth, penaltyHeight);
        graphics.DrawRectangle(pen, centre.X - sixWidth / 2, field.Bottom - sixHeight, sixWidth, sixHeight);
        return field;
    }
}
