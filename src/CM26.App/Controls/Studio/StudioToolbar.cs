using System.Drawing;
using System.Windows.Forms;

namespace CM26.App.Controls.Studio;

/// <summary>
/// Section toolbar with breadcrumb/title, search, prev/next, new and count.
/// </summary>
public sealed class StudioToolbar : Panel
{
    private readonly Label _breadcrumb;
    private readonly TextBox _search;
    private readonly Button _searchBtn;
    private readonly Button _prevBtn;
    private readonly Button _nextBtn;
    private readonly Button _newBtn;
    private readonly Button _filterBtn;
    private readonly Button _openBtn;
    private readonly Button _saveBtn;
    private readonly Button _undoBtn;
    private readonly Button _redoBtn;
    private readonly Button _validateBtn;
    private readonly ProgressBar _progress;
    private readonly Label _count;

    public event EventHandler? OpenClicked;
    public event EventHandler? SaveClicked;
    public event EventHandler? UndoClicked;
    public event EventHandler? RedoClicked;
    public event EventHandler? ValidateClicked;
    public event EventHandler? SearchClicked;
    public event EventHandler? PreviousClicked;
    public event EventHandler? NextClicked;
    public event EventHandler? NewClicked;
    public event EventHandler? FilterClicked;
    public event EventHandler? SearchTextChanged;
    public event KeyEventHandler? SearchKeyDown;

    public Button OpenButton => _openBtn;
    public Button SaveButton => _saveBtn;
    public Button UndoButton => _undoBtn;
    public Button RedoButton => _redoBtn;
    public Button ValidateButton => _validateBtn;
    public ProgressBar Progress => _progress;
    public TextBox SearchBox => _search;

    public StudioToolbar()
    {
        DoubleBuffered = true;
        Dock = DockStyle.Top;
        Height = 52;
        BackColor = StudioColors.Surface;
        Padding = new Padding(StudioSpacing.Medium, StudioSpacing.Small, StudioSpacing.Medium, StudioSpacing.Small);

        _breadcrumb = new Label
        {
            AutoSize = true,
            ForeColor = StudioColors.PrimaryText,
            Font = StudioFonts.SectionTitle,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Left,
        };

        _count = new Label
        {
            AutoSize = true,
            ForeColor = StudioColors.MutedText,
            Font = StudioFonts.Metadata,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Left,
            Padding = new Padding(StudioSpacing.Medium, 0, 0, 0),
        };

        _search = new TextBox
        {
            Width = 220,
            Height = 28,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = StudioColors.InputBackground,
            ForeColor = StudioColors.PrimaryText,
            Font = StudioFonts.RowPrimary,
            PlaceholderText = "Search records…",
        };
        _search.TextChanged += (_, _) => SearchTextChanged?.Invoke(this, EventArgs.Empty);
        _search.KeyDown += (s, e) => SearchKeyDown?.Invoke(this, e);

        _searchBtn = MakeToolButton("Go", "Search records");
        _searchBtn.Click += (_, _) => SearchClicked?.Invoke(this, EventArgs.Empty);

        _prevBtn = MakeToolButton("◀", "Previous record");
        _prevBtn.Click += (_, _) => PreviousClicked?.Invoke(this, EventArgs.Empty);

        _nextBtn = MakeToolButton("▶", "Next record");
        _nextBtn.Click += (_, _) => NextClicked?.Invoke(this, EventArgs.Empty);

        _newBtn = MakeToolButton("+ New", "Create new record", primary: true);
        _newBtn.Click += (_, _) => NewClicked?.Invoke(this, EventArgs.Empty);

        _filterBtn = MakeToolButton("Filter", "Show filters");
        _filterBtn.Click += (_, _) => FilterClicked?.Invoke(this, EventArgs.Empty);

        _openBtn = MakeToolButton("Open Game", "Detect the game and load its database and assets automatically (Ctrl+O)");
        _openBtn.Click += (_, _) => OpenClicked?.Invoke(this, EventArgs.Empty);

        _saveBtn = MakeToolButton("Save", "Apply staged changes directly to FC26 for offline use (Ctrl+S).", primary: true);
        _saveBtn.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);

        _undoBtn = MakeToolButton("Undo", "Undo last change (Ctrl+Z)");
        _undoBtn.Click += (_, _) => UndoClicked?.Invoke(this, EventArgs.Empty);

        _redoBtn = MakeToolButton("Redo", "Redo the last undone change (Ctrl+Y)");
        _redoBtn.Click += (_, _) => RedoClicked?.Invoke(this, EventArgs.Empty);

        _validateBtn = MakeToolButton("Validate", "Validate staged changes");
        _validateBtn.Click += (_, _) => ValidateClicked?.Invoke(this, EventArgs.Empty);

        _progress = new ProgressBar
        {
            Visible = false,
            Width = 170,
            Height = 18,
            Style = ProgressBarStyle.Marquee,
            Margin = new Padding(StudioSpacing.Medium, 5, 0, 0),
        };

        var rightFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            BackColor = Color.Transparent,
        };
        rightFlow.Controls.Add(_filterBtn);
        rightFlow.Controls.Add(_newBtn);
        rightFlow.Controls.Add(_nextBtn);
        rightFlow.Controls.Add(_prevBtn);
        rightFlow.Controls.Add(_searchBtn);
        rightFlow.Controls.Add(_search);

        var leftFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            BackColor = Color.Transparent,
        };
        leftFlow.Controls.Add(_openBtn);
        leftFlow.Controls.Add(_saveBtn);
        leftFlow.Controls.Add(_undoBtn);
        leftFlow.Controls.Add(_redoBtn);
        leftFlow.Controls.Add(_validateBtn);
        leftFlow.Controls.Add(_progress);

        var centerFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(StudioSpacing.Medium, 0, 0, 0),
        };
        centerFlow.Controls.Add(_breadcrumb);
        centerFlow.Controls.Add(_count);

        Controls.Add(rightFlow);
        Controls.Add(centerFlow);
        Controls.Add(leftFlow);
    }

    public string Title
    {
        get => _breadcrumb.Text;
        set => _breadcrumb.Text = value;
    }

    public string RecordCountText
    {
        get => _count.Text;
        set => _count.Text = value;
    }

    public string SearchText
    {
        get => _search.Text;
        set => _search.Text = value;
    }

    public bool CanCreate
    {
        get => _newBtn.Visible;
        set => _newBtn.Visible = value;
    }

    public bool ShowFilter
    {
        get => _filterBtn.Visible;
        set => _filterBtn.Visible = value;
    }

    public void FocusSearch() => _search.Focus();

    private Button MakeToolButton(string text, string tooltip, bool primary = false)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = false,
            Height = 28,
            Width = TextRenderer.MeasureText(text, StudioFonts.Button).Width + 22,
            Margin = new Padding(StudioSpacing.Tiny, 0, StudioSpacing.Tiny, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? StudioColors.Green : StudioColors.RaisedSurface,
            ForeColor = primary ? StudioColors.PrimaryText : StudioColors.PrimaryText,
            Font = StudioFonts.Button,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
        };
        button.FlatAppearance.BorderColor = primary ? StudioColors.Green : StudioColors.CardBorder;
        button.FlatAppearance.MouseOverBackColor = primary ? StudioColors.GreenHover : StudioColors.CardBorder;
        button.FlatAppearance.MouseDownBackColor = primary ? StudioColors.GreenHover : StudioColors.CardBorder;
        new ToolTip().SetToolTip(button, tooltip);
        return button;
    }
}
