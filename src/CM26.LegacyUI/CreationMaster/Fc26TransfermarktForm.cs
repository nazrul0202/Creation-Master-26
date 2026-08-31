using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FifaLibrary;
using HtmlAgilityPack;

namespace CreationMaster;

/// <summary>Classic CM26 Transfermarkt preview/import workflow. No database value is
/// changed until Apply is pressed; the normal CM26 save transaction remains authoritative.</summary>
public sealed class Fc26TransfermarktForm : Form
{
	private static readonly HttpClient Client = CreateClient();
	private readonly Player _player;
	private readonly TextBox _url = new TextBox { Dock = DockStyle.Fill };
	private readonly TextBox _firstName = new TextBox { Dock = DockStyle.Fill };
	private readonly TextBox _lastName = new TextBox { Dock = DockStyle.Fill };
	private readonly DateTimePicker _birthDate = new DateTimePicker { Format = DateTimePickerFormat.Short, Dock = DockStyle.Fill };
	private readonly NumericUpDown _height = new NumericUpDown { Minimum = 140, Maximum = 220, Dock = DockStyle.Fill };
	private readonly ComboBox _country = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
	private readonly ComboBox _position = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
	private readonly ComboBox _team = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
	private readonly NumericUpDown _overall = new NumericUpDown { Minimum = 10, Maximum = 99, Dock = DockStyle.Fill };
	private readonly NumericUpDown _potential = new NumericUpDown { Minimum = 10, Maximum = 99, Dock = DockStyle.Fill };
	private readonly CheckBox _generateAttributes = new CheckBox { Text = "Generate position-based attributes at selected OVR", AutoSize = true };
	private readonly CheckBox _applyAppearance = new CheckBox { Text = "Apply nationality-aware generic appearance suggestion", AutoSize = true };
	private readonly Label _audit = new Label { AutoSize = true, ForeColor = Color.DimGray };
	private readonly Label _status = new Label { AutoSize = true, ForeColor = Color.FromArgb(20, 88, 45) };
	private string _sourceUrl = string.Empty;
	private int _suggestedHead;
	private int _suggestedSkin;

	public Fc26TransfermarktForm(Player player)
	{
		_player = player ?? throw new ArgumentNullException(nameof(player));
		Text = "CM26 — Transfermarkt Player Import";
		StartPosition = FormStartPosition.CenterParent;
		MinimumSize = new Size(720, 560);
		Size = new Size(820, 620);
		Font = SystemFonts.MessageBoxFont;

		var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 4 };
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

		var source = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
		source.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		source.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		source.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		source.Controls.Add(new Label { Text = "Profile URL / player name", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
		source.Controls.Add(_url, 1, 0);
		var load = new Button { Text = "Load preview", AutoSize = true };
		load.Click += async (_, _) => await LoadPreviewAsync(load);
		source.Controls.Add(load, 2, 0);
		root.Controls.Add(source, 0, 0);

		var fields = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 8), ColumnCount = 2, AutoScroll = true };
		fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
		fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		AddField(fields, "First name", _firstName);
		AddField(fields, "Last name", _lastName);
		AddField(fields, "Date of birth", _birthDate);
		AddField(fields, "Height (cm)", _height);
		AddField(fields, "Nationality", _country);
		AddField(fields, "Preferred position", _position);
		AddField(fields, "Target team", _team);
		AddField(fields, "Overall", _overall);
		AddField(fields, "Potential", _potential);
		AddField(fields, "Attribute generator", _generateAttributes);
		AddField(fields, "Appearance suggestion", _applyAppearance);
		AddField(fields, "Source audit", _audit);
		root.Controls.Add(fields, 0, 1);
		root.Controls.Add(_status, 0, 2);

		var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft };
		var apply = new Button { Text = "Preview & Apply", AutoSize = true, BackColor = Color.FromArgb(28, 112, 57), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
		apply.FlatAppearance.BorderSize = 0;
		apply.Click += (_, _) => ApplyChanges();
		var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
		buttons.Controls.Add(apply);
		buttons.Controls.Add(cancel);
		root.Controls.Add(buttons, 0, 3);
		Controls.Add(root);
		AcceptButton = apply;
		CancelButton = cancel;
		LoadChoices();
		LoadCurrentPlayer();
	}

	private static void AddField(TableLayoutPanel panel, string label, Control control)
	{
		var row = panel.RowCount++;
		panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		panel.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) }, 0, row);
		control.Margin = new Padding(3, 4, 3, 4);
		panel.Controls.Add(control, 1, row);
	}

	private void LoadChoices()
	{
		foreach (Country item in FifaEnvironment.Countries) _country.Items.Add(new Choice(item.Id, item.ToString()));
		var positions = new[]
		{
			("GK", ERole.Goalkeeper), ("RWB", ERole.Right_Wing_Back), ("RB", ERole.Right_Back),
			("CB", ERole.Central_Back), ("LB", ERole.Left_Back), ("LWB", ERole.Left_Wing_Back),
			("CDM", ERole.Central_Defensive_Midfielder), ("RM", ERole.Right_Midfielder),
			("CM", ERole.Central_Midfielder), ("LM", ERole.Left_Midfielder),
			("CAM", ERole.Central_Advanced_Midfielder), ("RW", ERole.Right_Wing),
			("CF", ERole.Central_Forward), ("ST", ERole.Central_Striker), ("LW", ERole.Left_Wing)
		};
		foreach (var item in positions) _position.Items.Add(new Choice((int)item.Item2, item.Item1));
		_team.Items.Add(new Choice(-1, "Keep current team"));
		foreach (Team item in FifaEnvironment.Teams)
			if (item.IsClub()) _team.Items.Add(new Choice(item.Id, item.ToString()));
	}

	private void LoadCurrentPlayer()
	{
		_firstName.Text = _player.firstname ?? string.Empty;
		_lastName.Text = _player.lastname ?? string.Empty;
		_birthDate.Value = ClampDate(_player.birthdate);
		_height.Value = Math.Max(_height.Minimum, Math.Min(_height.Maximum, _player.height));
		SelectId(_country, _player.nationality);
		SelectId(_position, _player.preferredposition1);
		_team.SelectedIndex = 0;
		_overall.Value = Math.Max(10, Math.Min(99, _player.overallrating));
		_potential.Value = Math.Max(10, Math.Min(99, _player.potential));
		_audit.Text = "Current CM26 record — no web data loaded";
	}

	private async System.Threading.Tasks.Task LoadPreviewAsync(Control loadButton)
	{
		Uri uri;
		if (!TryTransfermarktUri(_url.Text, out uri) && string.IsNullOrWhiteSpace(_url.Text))
		{
			MessageBox.Show(this, "Enter a player name or valid HTTPS Transfermarkt profile URL.", "Transfermarkt", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			return;
		}
		try
		{
			loadButton.Enabled = false;
			if (uri == null)
			{
				_status.Text = "Searching Transfermarkt by player name...";
				uri = await FindPlayerUriAsync(_url.Text.Trim());
			}
			_status.Text = "Loading profile preview...";
			var html = await Client.GetStringAsync(uri);
			var profile = ParseProfile(html);
			if (string.IsNullOrWhiteSpace(profile.Name)) throw new InvalidOperationException("Transfermarkt profile details were not found on this page.");
			var names = SplitName(profile.Name);
			_firstName.Text = names.Item1;
			_lastName.Text = names.Item2;
			if (profile.BirthDate.HasValue) _birthDate.Value = ClampDate(profile.BirthDate.Value);
			if (profile.Height > 0) _height.Value = Math.Max(_height.Minimum, Math.Min(_height.Maximum, profile.Height));
			SelectText(_country, profile.Nationality);
			SelectPosition(profile.Position);
			ApplyGeneratedSuggestions(profile);
			_sourceUrl = uri.AbsoluteUri;
			_audit.Text = "Loaded " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " — " + _sourceUrl;
			_status.Text = "Preview loaded. Review every value before Apply.";
		}
		catch (Exception ex)
		{
			_status.Text = "Transfermarkt request failed.";
			Fc26FriendlyError.Show(this, "Transfermarkt preview", ex, "No player data was applied. Check the URL or source page, then retry.");
		}
		finally { loadButton.Enabled = true; }
	}

	private void ApplyChanges()
	{
		var target = _team.SelectedItem as Choice;
		var summary = _firstName.Text.Trim() + " " + _lastName.Text.Trim() + "\r\n" +
			"DOB: " + _birthDate.Value.ToShortDateString() + " | Height: " + _height.Value + " cm\r\n" +
			"OVR/POT: " + _overall.Value + "/" + _potential.Value + "\r\n" +
			"Team: " + (target?.Name ?? "Keep current team") + "\r\n\r\n" +
			"Apply these values to the staged CM26 record?";
		if (MessageBox.Show(this, summary, "Transfermarkt — Preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

		_player.firstname = _firstName.Text.Trim();
		_player.lastname = _lastName.Text.Trim();
		_player.birthdate = _birthDate.Value.Date;
		_player.height = (int)_height.Value;
		if (_country.SelectedItem is Choice nation) _player.nationality = nation.Id;
		if (_position.SelectedItem is Choice position) _player.preferredposition1 = position.Id;
		if (_generateAttributes.Checked) _player.RandomizeSkillsExactly((int)_overall.Value);
		else _player.overallrating = (int)_overall.Value;
		_player.potential = Math.Max(_player.overallrating, (int)_potential.Value);
		if (_applyAppearance.Checked)
		{
			_player.headclasscode = 1;
			_player.headtypecode = _suggestedHead;
			_player.skintonecode = _suggestedSkin;
		}

		if (target != null && target.Id >= 0)
		{
			var destination = (Team)FifaEnvironment.Teams.SearchId(target.Id);
			var currentClub = _player.GetClub();
			if (destination != null && currentClub != destination)
			{
				currentClub?.RemoveTeamPlayer(_player);
				destination.AddTeamPlayer(_player);
			}
		}
		AppendAuditLog();

		DialogResult = DialogResult.OK;
		Close();
	}

	private static Profile ParseProfile(string html)
	{
		var doc = new HtmlAgilityPack.HtmlDocument();
		doc.LoadHtml(html ?? string.Empty);
		string name = Meta(doc, "og:title");
		if (name.Contains(" - ")) name = name.Split(new[] { " - " }, StringSplitOptions.None)[0];
		if (string.IsNullOrWhiteSpace(name)) name = NodeText(doc.DocumentNode.SelectSingleNode("//h1"));
		var plain = WebUtility.HtmlDecode(doc.DocumentNode.InnerText ?? string.Empty).Replace('\u00a0', ' ');
		var dobText = FindFact(plain, "Date of birth/Age", "Date of birth");
		DateTime? dob = null;
		var dobMatch = Regex.Match(dobText, @"\d{1,2}/\d{1,2}/\d{4}|\d{1,2}\.\d{1,2}\.\d{4}");
		if (dobMatch.Success && DateTime.TryParse(dobMatch.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)) dob = parsed;
		var heightText = FindFact(plain, "Height");
		var height = 0;
		var metres = Regex.Match(heightText, @"(?<m>[12])[,.](?<cm>\d{2})\s*m");
		if (metres.Success) height = int.Parse(metres.Groups["m"].Value) * 100 + int.Parse(metres.Groups["cm"].Value);
		var marketValue = ParseMarketValue(FindFact(plain, "Market value", "Current market value"));
		if (marketValue <= 0) marketValue = ExtractMarketValue(doc);
		return new Profile(name.Trim(), dob, height,
			FindFact(plain, "Citizenship", "Nationality"), FindFact(plain, "Position"), marketValue);
	}

	private static float ExtractMarketValue(HtmlAgilityPack.HtmlDocument document)
	{
		var nodes = document.DocumentNode.SelectNodes("//*[contains(text(),'€') or contains(text(),'£')]");
		if (nodes == null) return 0;
		foreach (var node in nodes)
		{
			var text = WebUtility.HtmlDecode(node.InnerText ?? string.Empty);
			if (!Regex.IsMatch(text, @"\d[\d,.]*\s*(m|k|bn)\b", RegexOptions.IgnoreCase)) continue;
			var value = ParseMarketValue(text);
			if (value > 0) return value;
		}
		return 0;
	}

	private void ApplyGeneratedSuggestions(Profile profile)
	{
		var age = DateTime.Today.Year - (profile.BirthDate ?? _birthDate.Value).Year;
		if ((profile.BirthDate ?? _birthDate.Value).Date > DateTime.Today.AddYears(-age)) age--;
		if (_position.SelectedItem is Choice role && profile.MarketValueMillions > 0)
		{
			var suggestedOverall = _player.EstimateSkills(profile.MarketValueMillions, Math.Max(15, age), (ERole)role.Id);
			_overall.Value = Math.Max(_overall.Minimum, Math.Min(_overall.Maximum, suggestedOverall));
			var growth = age <= 18 ? 9 : age <= 20 ? 7 : age <= 22 ? 6 : age <= 24 ? 4 : age <= 27 ? 3 : age <= 29 ? 1 : 0;
			_potential.Value = Math.Max(_potential.Minimum, Math.Min(_potential.Maximum, Math.Min(96, suggestedOverall + growth)));
		}
		var nation = _country.SelectedItem as Choice;
		var country = nation == null ? null : (Country)FifaEnvironment.Countries.SearchId(nation.Id);
		var confederation = country == null ? Country.EConfederation.None : (Country.EConfederation)country.m_confederation;
		if (confederation == Country.EConfederation.Africa) { _suggestedHead = 1000; _suggestedSkin = 8; }
		else if (confederation == Country.EConfederation.South_America) { _suggestedHead = 1500; _suggestedSkin = 6; }
		else if (confederation == Country.EConfederation.Asia) { _suggestedHead = 500; _suggestedSkin = 4; }
		else { _suggestedHead = 0; _suggestedSkin = 3; }
		_applyAppearance.Text = "Apply generic head " + _suggestedHead + " / skin " + _suggestedSkin + " (nationality suggestion)";
		if (profile.MarketValueMillions > 0)
			_status.Text = "Generated FC26 suggestions from €" + profile.MarketValueMillions.ToString("0.##") + "m, age and position. Review before Apply.";
	}

	private static float ParseMarketValue(string value)
	{
		var text = WebUtility.HtmlDecode(value ?? string.Empty).Replace(',', '.').ToLowerInvariant();
		var match = Regex.Match(text, @"(?<v>\d+(?:\.\d+)?)\s*(?<u>m|k|bn)?");
		if (!match.Success || !float.TryParse(match.Groups["v"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var amount)) return 0;
		var unit = match.Groups["u"].Value;
		return unit == "k" ? amount / 1000f : unit == "bn" ? amount * 1000f : amount;
	}

	private static async System.Threading.Tasks.Task<Uri> FindPlayerUriAsync(string query)
	{
		var search = new Uri("https://www.transfermarkt.com/schnellsuche/ergebnis/schnellsuche?query=" + Uri.EscapeDataString(query));
		var html = await Client.GetStringAsync(search);
		var match = Regex.Match(html, "href=[\\\"'](?<v>[^\\\"']*/profil/spieler/\\d+[^\\\"']*)[\\\"']", RegexOptions.IgnoreCase);
		if (!match.Success) throw new InvalidOperationException("No Transfermarkt player profile matched that name.");
		var href = WebUtility.HtmlDecode(match.Groups["v"].Value);
		if (Uri.TryCreate(search, href, out var result) && TryTransfermarktUri(result.AbsoluteUri, out var allowed)) return allowed;
		throw new InvalidOperationException("The Transfermarkt search result did not contain a safe player profile URL.");
	}

	private void AppendAuditLog()
	{
		try
		{
			var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Creation Master 26");
			Directory.CreateDirectory(folder);
			var line = string.Join("\t", DateTime.Now.ToString("s"), _player.Id.ToString(),
				(_player.firstname + " " + _player.lastname).Trim(), _sourceUrl, "staged") + Environment.NewLine;
			File.AppendAllText(Path.Combine(folder, "transfermarkt-audit.tsv"), line);
		}
		catch { }
	}

	private static string FindFact(string text, params string[] labels)
	{
		foreach (var label in labels)
		{
			var match = Regex.Match(text, Regex.Escape(label) + @"\s*:\s*(?<v>[^\r\n]+)", RegexOptions.IgnoreCase);
			if (match.Success) return Regex.Replace(match.Groups["v"].Value, @"\s+", " ").Trim();
		}
		return string.Empty;
	}

	private static string Meta(HtmlAgilityPack.HtmlDocument doc, string property) =>
		doc.DocumentNode.SelectSingleNode("//meta[@property='" + property + "']")?.GetAttributeValue("content", "") ?? string.Empty;
	private static string NodeText(HtmlNode node) => WebUtility.HtmlDecode(node?.InnerText ?? string.Empty).Trim();
	private static Tuple<string, string> SplitName(string name)
	{
		var parts = Regex.Split((name ?? string.Empty).Trim(), @"\s+");
		return parts.Length <= 1 ? Tuple.Create(string.Empty, parts.FirstOrDefault() ?? string.Empty) :
			Tuple.Create(string.Join(" ", parts.Take(parts.Length - 1)), parts[parts.Length - 1]);
	}

	private void SelectPosition(string value)
	{
		var text = (value ?? string.Empty).ToUpperInvariant();
		var code = text.Contains("GOALKEEPER") ? "GK" : text.Contains("CENTRE-BACK") || text.Contains("CENTER-BACK") ? "CB" :
			text.Contains("LEFT-BACK") ? "LB" : text.Contains("RIGHT-BACK") ? "RB" : text.Contains("DEFENSIVE") ? "CDM" :
			text.Contains("ATTACKING MIDFIELD") ? "CAM" : text.Contains("CENTRAL MIDFIELD") ? "CM" :
			text.Contains("LEFT WINGER") ? "LW" : text.Contains("RIGHT WINGER") ? "RW" :
			text.Contains("CENTRE-FORWARD") || text.Contains("CENTER-FORWARD") ? "ST" : string.Empty;
		SelectText(_position, code);
	}

	private static void SelectId(ComboBox combo, int id)
	{
		for (var i = 0; i < combo.Items.Count; i++) if (((Choice)combo.Items[i]).Id == id) { combo.SelectedIndex = i; return; }
		if (combo.Items.Count > 0) combo.SelectedIndex = 0;
	}
	private static void SelectText(ComboBox combo, string text)
	{
		if (string.IsNullOrWhiteSpace(text)) return;
		for (var i = 0; i < combo.Items.Count; i++)
			if (((Choice)combo.Items[i]).Name.IndexOf(text.Trim(), StringComparison.OrdinalIgnoreCase) >= 0 ||
				text.IndexOf(((Choice)combo.Items[i]).Name, StringComparison.OrdinalIgnoreCase) >= 0) { combo.SelectedIndex = i; return; }
	}
	private static DateTime ClampDate(DateTime date) => date < new DateTime(1900, 1, 1) ? new DateTime(2000, 1, 1) : date > DateTime.Today ? DateTime.Today : date;
	private static bool TryTransfermarktUri(string value, out Uri uri)
	{
		uri = null;
		if (!Uri.TryCreate((value ?? string.Empty).Trim(), UriKind.Absolute, out var parsed) || parsed.Scheme != Uri.UriSchemeHttps) return false;
		var host = parsed.Host.ToLowerInvariant();
		if (!host.StartsWith("www.transfermarkt.", StringComparison.Ordinal) && !host.StartsWith("transfermarkt.", StringComparison.Ordinal)) return false;
		uri = parsed;
		return true;
	}
	private static HttpClient CreateClient()
	{
		var client = new HttpClient { Timeout = TimeSpan.FromSeconds(25) };
		client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 CM26/1.0");
		client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-GB,en;q=0.9");
		return client;
	}

	private sealed class Choice
	{
		public int Id { get; }
		public string Name { get; }
		public Choice(int id, string name) { Id = id; Name = name ?? string.Empty; }
		public override string ToString() => Name + (Id >= 0 ? " [" + Id + "]" : string.Empty);
	}
	private sealed class Profile
	{
		public string Name { get; }
		public DateTime? BirthDate { get; }
		public int Height { get; }
		public string Nationality { get; }
		public string Position { get; }
		public float MarketValueMillions { get; }
		public Profile(string name, DateTime? birthDate, int height, string nationality, string position, float marketValueMillions)
		{ Name = name; BirthDate = birthDate; Height = height; Nationality = nationality; Position = position; MarketValueMillions = marketValueMillions; }
	}
}
