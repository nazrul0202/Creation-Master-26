using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CM26.App.Controls;

namespace CM26.App.Sections;

/// <summary>
/// Local CM26 Scraper integration. Its verified squad output is previewed here
/// and then routed to Teams, where players and roster links are created together.
/// </summary>
public sealed class TransfersSection : SectionBase
{
    private readonly TextBox _url = new();
    private readonly Button _fetch = new();
    private readonly Button _export = new();
    private readonly Button _scraper = new();
    private readonly Button _refreshOutput = new();
    private readonly Button _chooseOutput = new();
    private readonly Button _importToTeam = new();
    private readonly ComboBox _destinationTeam = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _team = new();
    private readonly Label _status = new();
    private readonly ListView _squad = new();
    private readonly List<TransfermarktPlayer> _players = [];
    private string _scraperWorkbookPath = string.Empty;

    public override string SectionKey => "transfers";
    public override string SectionTitle => "Data Sync";
    protected override string TableName => "";
    protected override bool SinglePane => true;
    protected override bool ShowRecordCommandStrip => false;

    public TransfersSection(AppServices services) : base(services)
    {
        var page = new TabPage("Team & Squad Scraper")
        {
            BackColor = SystemColors.Control,
            Padding = new Padding(8)
        };
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = SystemColors.Control
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var destination = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        destination.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        destination.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        destination.Controls.Add(new Label { Text = "Destination team", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        _destinationTeam.Dock = DockStyle.Fill;
        destination.Controls.Add(_destinationTeam, 1, 0);

        var scraperActions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        scraperActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        scraperActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        scraperActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        scraperActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _scraper.Text = "Open CM26 Scraper";
        _scraper.Dock = DockStyle.Fill;
        _scraper.Click += (_, _) => OpenLocalScraper();
        _refreshOutput.Text = "Refresh output";
        _refreshOutput.Dock = DockStyle.Fill;
        _refreshOutput.Click += (_, _) => DetectScraperOutput(showMissingMessage: true);
        _chooseOutput.Text = "Choose output...";
        _chooseOutput.Dock = DockStyle.Fill;
        _chooseOutput.Click += (_, _) => ChooseScraperOutput();
        _importToTeam.Text = "Import to selected team";
        _importToTeam.Dock = DockStyle.Fill;
        _importToTeam.Enabled = false;
        _importToTeam.Click += (_, _) => ImportDetectedSquad();
        scraperActions.Controls.Add(_scraper, 0, 0);
        scraperActions.Controls.Add(_refreshOutput, 1, 0);
        scraperActions.Controls.Add(_chooseOutput, 2, 0);
        scraperActions.Controls.Add(_importToTeam, 3, 0);

        var address = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
        address.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        address.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        address.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        address.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        _url.Dock = DockStyle.Fill;
        _url.PlaceholderText = "Paste a Transfermarkt club squad URL";
        _fetch.Text = "Load Squad";
        _fetch.Dock = DockStyle.Fill;
        _fetch.Click += async (_, _) => await FetchAsync();
        _export.Text = "Export CSV";
        _export.Dock = DockStyle.Fill;
        _export.Enabled = false;
        _export.Click += (_, _) => ExportCsv();
        address.Controls.Add(_url, 0, 0);
        address.Controls.Add(_fetch, 1, 0);
        address.Controls.Add(_export, 2, 0);
        address.Controls.Add(new Label { Text = "URL preview only", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter }, 3, 0);

        _team.Dock = DockStyle.Fill;
        _team.Font = new Font("Segoe UI Semibold", 15f);
        _team.Text = "CM26 Scraper import";
        _team.TextAlign = ContentAlignment.MiddleLeft;

        _squad.Dock = DockStyle.Fill;
        _squad.View = View.Details;
        _squad.FullRowSelect = true;
        _squad.GridLines = true;
        _squad.HideSelection = false;
        _squad.Columns.Add("#", 48);
        _squad.Columns.Add("Player", 240);
        _squad.Columns.Add("Position", 175);
        _squad.Columns.Add("Date of birth", 125);
        _squad.Columns.Add("Nationality", 170);
        _squad.Columns.Add("Market value", 120);
        _squad.Columns.Add("Transfermarkt ID", 125);

        _status.Dock = DockStyle.Fill;
        _status.Text = "Open the local scraper, scrape a squad, then Refresh output. CM26 will import that output directly into the selected team database records.";
        _status.TextAlign = ContentAlignment.MiddleLeft;
        _status.AutoEllipsis = true;

        root.Controls.Add(destination, 0, 0);
        root.Controls.Add(scraperActions, 0, 1);
        root.Controls.Add(address, 0, 2);
        root.Controls.Add(_team, 0, 3);
        root.Controls.Add(_squad, 0, 4);
        root.Controls.Add(_status, 0, 5);
        page.Controls.Add(root);
        Tabs.TabPages.Add(page);
    }

    protected override IReadOnlyList<CM26.Application.Models.RecordListItem> GetRecords() =>
        Array.Empty<CM26.Application.Models.RecordListItem>();

    protected override void ShowRecord(int recordIndex) { }

    public override void ActivateSection()
    {
        base.ActivateSection();
        PopulateDestinationTeams();
        DetectScraperOutput(showMissingMessage: false);
    }

    private void PopulateDestinationTeams()
    {
        var previous = (_destinationTeam.SelectedItem as TeamChoice)?.TeamId ?? 0;
        _destinationTeam.Items.Clear();
        if (!Services.Session.IsLoaded) return;
        var teams = Services.RequireData().GetTeams()
            .Select(item => new TeamChoice(Parse(Services.Session.GetCell("teams", item.RecordIndex, "teamid")), item.Title))
            .Where(item => item.TeamId > 0)
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _destinationTeam.Items.AddRange(teams);
        var selected = teams.ToList().FindIndex(item => item.TeamId == previous);
        if (selected >= 0) _destinationTeam.SelectedIndex = selected;
        else if (_destinationTeam.Items.Count > 0) _destinationTeam.SelectedIndex = 0;
    }

    private void DetectScraperOutput(bool showMissingMessage)
    {
        var exe = ExternalToolLocator.FindFile(Path.Combine("CM26 SCRAPER", "CM26 Scraper.exe"), Path.Combine("CM26 Scraper", "CM26 Scraper.exe"));
        var root = exe == null ? null : Path.GetDirectoryName(exe);
        var candidates = root == null ? [] : new[]
        {
            Path.Combine(root, "Scraped teams"),
            Path.Combine(root, "Batch Results", "Teams"),
        };
        var workbooks = candidates.Where(Directory.Exists)
            .SelectMany(folder => Directory.EnumerateFiles(folder, "*.xlsx", SearchOption.TopDirectoryOnly))
            .ToArray();
        var squadOutputs = workbooks.Where(file => Path.GetFileName(file)
            .StartsWith("squad_", StringComparison.OrdinalIgnoreCase)).ToArray();
        _scraperWorkbookPath = (squadOutputs.Length > 0 ? squadOutputs : workbooks)
            .OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_scraperWorkbookPath))
        {
            _importToTeam.Enabled = false;
            if (showMissingMessage) MessageBox.Show(this, "No scraper squad output was found. Run the scraper first, then click Refresh output.", "Data Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            LoadScraperPreview(_scraperWorkbookPath);
            _importToTeam.Enabled = _destinationTeam.Items.Count > 0;
        }
        catch (Exception ex)
        {
            _scraperWorkbookPath = string.Empty;
            _importToTeam.Enabled = false;
            if (showMissingMessage) MessageBox.Show(this, ex.Message, "Data Sync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ChooseScraperOutput()
    {
        using var dialog = new OpenFileDialog { Filter = "CM26 Scraper workbook (*.xlsx)|*.xlsx", CheckFileExists = true, Title = "Select CM26 Scraper output" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _scraperWorkbookPath = dialog.FileName;
        try { LoadScraperPreview(_scraperWorkbookPath); _importToTeam.Enabled = _destinationTeam.Items.Count > 0; }
        catch (Exception ex) { _scraperWorkbookPath = string.Empty; _importToTeam.Enabled = false; MessageBox.Show(this, ex.Message, "Data Sync", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void LoadScraperPreview(string path)
    {
        var workbook = new CompdataWorkbookService();
        workbook.Open(path);
        var data = workbook.SheetNames.Select(workbook.ReadSheet).FirstOrDefault(table => table.Columns.Contains("firstname") && table.Columns.Contains("lastname"))
            ?? throw new InvalidDataException("The scraper output has no player sheet.");
        _squad.Items.Clear();
        foreach (System.Data.DataRow row in data.Rows)
        {
            var first = Cell(row, "firstname"); var last = Cell(row, "lastname");
            if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(last)) continue;
            _squad.Items.Add(new ListViewItem([Cell(row, "jerseynumber"), $"{first} {last}".Trim(), Cell(row, "position"), Cell(row, "birthdate"), Cell(row, "nationality"), string.Empty, Cell(row, "tmprofile")]));
        }
        _team.Text = $"Scraper output: {Path.GetFileName(path)}";
        _status.Text = $"{_squad.Items.Count} player(s) ready. Select the destination team and click Import to selected team.";
    }

    private void ImportDetectedSquad()
    {
        if (_destinationTeam.SelectedItem is not TeamChoice team || string.IsNullOrWhiteSpace(_scraperWorkbookPath)) return;
        Services.RequestScraperSquadImport(team.TeamId, _scraperWorkbookPath);
    }

    private async Task FetchAsync()
    {
        if (!TryValidateUrl(_url.Text.Trim(), out var uri))
        {
            MessageBox.Show(this, "Use a valid HTTPS Transfermarkt club/squad URL.",
                "Transfermarkt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _fetch.Enabled = false;
        _export.Enabled = false;
        _status.Text = "Loading Transfermarkt team and squad information...";
        _squad.Items.Clear();
        _players.Clear();
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(35) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "Chrome/126.0 Safari/537.36 CreationMaster26/1.0");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-GB,en;q=0.9");
            var html = await client.GetStringAsync(uri);
            var result = ParseHtml(html);
            _team.Text = string.IsNullOrWhiteSpace(result.TeamName)
                ? "Transfermarkt squad"
                : result.TeamName;
            _players.AddRange(result.Players);
            foreach (var player in _players)
            {
                _squad.Items.Add(new ListViewItem([
                    player.Number, player.Name, player.Position, player.BirthDate,
                    player.Nationality, player.MarketValue, player.Id
                ]));
            }
            _export.Enabled = _players.Count > 0;
            _status.Text = _players.Count == 0
                ? "The page loaded, but no squad rows were recognised. Open the club's detailed squad page and try again."
                : $"Loaded {_players.Count} squad players from Transfermarkt. Database records have not been changed.";
        }
        catch (Exception ex)
        {
            _status.Text = "Transfermarkt request failed.";
            MessageBox.Show(this, ex.Message, "Transfermarkt",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _fetch.Enabled = true;
        }
    }

    private void ExportCsv()
    {
        if (_players.Count == 0) return;
        using var dialog = new SaveFileDialog
        {
            Title = "Export Transfermarkt squad",
            Filter = "CSV file (*.csv)|*.csv",
            FileName = SafeFileName(_team.Text) + "-squad.csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var lines = new List<string>
        {
            "Number,Player,Position,Date of birth,Nationality,Market value,Transfermarkt ID"
        };
        lines.AddRange(_players.Select(p => string.Join(",", new[]
        {
            p.Number, p.Name, p.Position, p.BirthDate, p.Nationality, p.MarketValue, p.Id
        }.Select(Csv))));
        File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        _status.Text = $"Exported {_players.Count} players to {dialog.FileName}";
    }

    private void OpenLocalScraper()
    {
        var executable = ExternalToolLocator.FindFile(
            Path.Combine("CM26 SCRAPER", "CM26 Scraper.exe"),
            Path.Combine("CM26 Scraper", "CM26 Scraper.exe"));
        if (executable == null)
        {
            MessageBox.Show(this,
                "CM26 Scraper was not found. Keep it in D:\\FC26 FILE TOOL\\CM26 SCRAPER (or Tools\\CM26 Scraper), run a squad scrape, then click Refresh output.",
                "Data Sync", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo(executable)
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(executable)!,
            });
            _status.Text = $"Opened full scraper: {executable}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Data Sync", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static bool TryValidateUrl(string text, out Uri uri)
    {
        uri = null!;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var parsed) ||
            parsed.Scheme != Uri.UriSchemeHttps)
            return false;
        var host = parsed.Host.ToLowerInvariant();
        if (!(host == "transfermarkt.com" || host.StartsWith("www.transfermarkt.") ||
              host.StartsWith("www.transfermarkt.co.") || host.StartsWith("transfermarkt.")))
            return false;
        uri = parsed;
        return true;
    }

    private static TransfermarktResult ParseHtml(string html)
    {
        var title = Match(html, @"<h1[^>]*>(?<v>.*?)</h1>");
        if (string.IsNullOrWhiteSpace(title))
            title = Match(html, @"<title[^>]*>(?<v>.*?)</title>").Split('|')[0].Trim();

        var squadHtml = html;
        var tableStart = html.IndexOf("<table class=\"items\"", StringComparison.OrdinalIgnoreCase);
        if (tableStart >= 0)
        {
            var bodyEnd = html.IndexOf("</tbody>", tableStart, StringComparison.OrdinalIgnoreCase);
            if (bodyEnd > tableStart)
                squadHtml = html.Substring(tableStart, bodyEnd + 8 - tableStart);
        }

        var anchorPattern = new Regex(
            @"<a[^>]+href=""(?<href>[^""]*/profil/spieler/(?<id>\d+)[^""]*)""[^>]*>(?<name>.*?)</a>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var matches = anchorPattern.Matches(squadHtml);
        var players = new List<TransfermarktPlayer>();
        var seen = new HashSet<string>();
        foreach (Match match in matches)
        {
            var id = match.Groups["id"].Value;
            var name = Clean(match.Groups["name"].Value);
            if (name.Length < 2 || !seen.Add(id)) continue;

            var from = Math.Max(0, match.Index - 700);
            var length = Math.Min(squadHtml.Length - from, 2800);
            var block = squadHtml.Substring(from, length);
            var number = Match(block, @"class\s*=\s*[""']?rn_nummer[""']?[^>]*>\s*(?<v>[^<]*)");
            var position = Match(block, @"</tr>\s*<tr>\s*<td[^>]*>(?<v>.*?)</td>");
            var birthDate = Match(block, @"(?<v>\d{2}[./]\d{2}[./]\d{4})");
            var marketValue = Match(block, @"(?<v>(?:\u20AC|&euro;)\s*[\d,.]+\s*(?:m|k)?)");
            var nationality = Nationality(block, name);
            players.Add(new TransfermarktPlayer(
                Clean(number), name, Clean(position), Clean(birthDate),
                nationality, Clean(marketValue), id));
        }
        return new TransfermarktResult(Clean(title), players);
    }

    internal static (string TeamName, int PlayerCount) ParseSummaryForTest(string html)
    {
        var parsed = ParseHtml(html);
        return (parsed.TeamName, parsed.Players.Count);
    }

    private static string Nationality(string block, string playerName)
    {
        foreach (Match match in Regex.Matches(block,
                     @"<img[^>]+(?:title|alt)=""(?<v>[^""]+)""",
                     RegexOptions.IgnoreCase | RegexOptions.Singleline))
        {
            var value = Clean(match.Groups["v"].Value);
            if (value.Length > 1 && !value.Equals(playerName, StringComparison.OrdinalIgnoreCase) &&
                !value.Contains("portrait", StringComparison.OrdinalIgnoreCase))
                return value;
        }
        return "";
    }

    private static string Match(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return match.Success ? Clean(match.Groups["v"].Value) : "";
    }

    private static string Clean(string text) =>
        WebUtility.HtmlDecode(Regex.Replace(text ?? "", "<[^>]+>", " "))
            .Replace('\u00a0', ' ').Trim();

    private static string Csv(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
    private static string Cell(System.Data.DataRow row, string column) => row.Table.Columns.Contains(column)
        ? Convert.ToString(row[column])?.Trim() ?? string.Empty : string.Empty;
    private static string SafeFileName(string value) =>
        string.Concat((string.IsNullOrWhiteSpace(value) ? "transfermarkt" : value)
            .Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

    private sealed record TransfermarktResult(string TeamName, IReadOnlyList<TransfermarktPlayer> Players);
    private sealed record TeamChoice(int TeamId, string Name)
    {
        public override string ToString() => $"{Name} [{TeamId}]";
    }
    private sealed record TransfermarktPlayer(
        string Number, string Name, string Position, string BirthDate,
        string Nationality, string MarketValue, string Id);
}
