using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ThreadingTask = System.Threading.Tasks.Task;
using System.Windows.Forms;
using FifaLibrary;

namespace CreationMaster;

/// <summary>Batch miniface and exact-path native face management for the classic shell.
/// Every write is staged through the x64 host and committed only by File &gt; Save.</summary>
internal sealed class Fc26FaceToolsForm : Form
{
	private readonly TextBox _search = new TextBox { Width = 190 };
	private readonly ComboBox _team = new ComboBox { Width = 210, DropDownStyle = ComboBoxStyle.DropDownList };
	private readonly DataGridView _grid = new DataGridView();
	private readonly PictureBox _preview = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(36, 36, 36) };
	private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 25, Padding = new Padding(6, 4, 0, 0) };
	private readonly Button _scanButton = new Button { Text = "Scan selected", AutoSize = true };
	private readonly Button _cancelScan = new Button { Text = "Cancel scan", AutoSize = true, Enabled = false };
	private List<Row> _rows = new List<Row>();
	private CancellationTokenSource _scanCancellation;

	internal Fc26FaceToolsForm()
	{
		Text = "FC26 Miniface & Face Tools";
		StartPosition = FormStartPosition.CenterParent;
		Size = new Size(1120, 720);
		MinimumSize = new Size(900, 580);
		AutoScaleMode = AutoScaleMode.Dpi;
		Icon = Form.ActiveForm?.Icon;
		_team.Items.Add(new TeamChoice(null));
		if (FifaEnvironment.Teams != null)
			foreach (Team team in FifaEnvironment.Teams.Cast<Team>().OrderBy(item => item.TeamNameFull)) _team.Items.Add(new TeamChoice(team));
		_team.SelectedIndex = 0;
		var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8), WrapContents = true };
			top.Controls.AddRange(new Control[]
		{
			new Label { Text = "Player / ID", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, _search,
			new Label { Text = "Team", AutoSize = true, Padding = new Padding(8, 6, 0, 0) }, _team,
			Button("Refresh", (_, _) => RefreshRows()), _scanButton, _cancelScan,
			Button("Batch import minifaces", BatchImport), Button("Export visible", ExportVisible),
			Button("Missing report", MissingReport), Button("Rename linked assets", RenameAssets),
			Button("Assign specific face", AssignSpecificFace), Button("Generic appearance...", GenericAppearance),
			Button("Auto-align miniface", AlignMiniface), Button("Face similarity helper", FaceSimilarity),
			Button("Import native cranium/face", ImportNativeFace), Button("Export native cranium/face", ExportNativeFace)
		});
		_scanButton.Click += ScanSelected;
		_cancelScan.Click += (_, _) => _scanCancellation?.Cancel();
		FormClosed += (_, _) => _scanCancellation?.Cancel();
		_search.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { RefreshRows(); e.SuppressKeyPress = true; } };
		_team.SelectedIndexChanged += (_, _) => RefreshRows();
		_grid.Dock = DockStyle.Fill;
		_grid.ReadOnly = true;
		_grid.AllowUserToAddRows = false;
		_grid.AllowUserToDeleteRows = false;
		_grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		_grid.MultiSelect = true;
		_grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
		_grid.SelectionChanged += (_, _) => PreviewSelected();
		var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 790 };
		split.Panel1.Controls.Add(_grid);
		split.Panel2.Controls.Add(_preview);
		Controls.Add(split);
		Controls.Add(top);
		Controls.Add(_status);
		RefreshRows();
	}

	private static Button Button(string text, EventHandler click)
	{
		var button = new Button { Text = text, AutoSize = true };
		button.Click += click;
		return button;
	}

	private void RefreshRows()
	{
		var query = _search.Text.Trim();
		var team = (_team.SelectedItem as TeamChoice)?.Team;
		var players = FifaEnvironment.Players == null ? Enumerable.Empty<Player>() : FifaEnvironment.Players.Cast<Player>();
		_rows = players
			.Where(player => team == null || player.GetClub() == team)
			.Where(player => query.Length == 0 || player.Id.ToString().Contains(query) || player.ToString().IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
			.Take(2000).Select(player => new Row(player)).ToList();
		_grid.DataSource = null;
		_grid.DataSource = _rows;
		_status.Text = _rows.Count.ToString("N0") + " player(s). Scan checks exact FC26 paths without modifying the game.";
	}

	private IEnumerable<Row> SelectedRows()
	{
		var selected = _grid.SelectedRows.Cast<DataGridViewRow>().Select(row => row.DataBoundItem as Row).Where(row => row != null).ToArray();
		return selected.Length == 0 ? _rows.Take(500) : selected;
	}

	private async void ScanSelected(object sender, EventArgs e)
	{
		if (_scanCancellation != null) return;
		var targets = SelectedRows().ToArray();
		_scanCancellation = new CancellationTokenSource();
		_scanButton.Enabled = false;
		_cancelScan.Enabled = true;
		_status.Text = "Scanning " + targets.Length + " player(s) in the background...";
		var progress = new Progress<int>(value => _status.Text = "Scanning FC26 face assets... " + value + "/" + targets.Length);
		try
		{
			var token = _scanCancellation.Token;
			await ThreadingTask.Run(() =>
			{
				for (var index = 0; index < targets.Length; index++)
				{
					token.ThrowIfCancellationRequested();
					targets[index].Scan();
					((IProgress<int>)progress).Report(index + 1);
				}
			}, token);
			_grid.Refresh();
			_status.Text = "Scanned " + targets.Length + " player(s); installed/missing state refreshed.";
		}
		catch (OperationCanceledException) { _status.Text = "Face scan cancelled safely."; }
		catch (Exception ex)
		{
			_status.Text = "Face operation stopped safely.";
			Fc26FriendlyError.Show(this, "Face tools", ex, "No face assignment was accepted. Review the source image and selected player, then retry.");
		}
		finally
		{
			_scanCancellation.Dispose();
			_scanCancellation = null;
			_scanButton.Enabled = true;
			_cancelScan.Enabled = false;
		}
	}

	private void BatchImport(object sender, EventArgs e)
	{
		using (var dialog = new FolderBrowserDialog { Description = "Choose a folder containing p<playerid>.png/.jpg/.bmp/.dds minifaces" })
		{
			if (dialog.ShowDialog(this) != DialogResult.OK) return;
			Run("Staging batch minifaces...", () =>
			{
				var skipped = 0;
				var imports = new List<MinifaceImport>();
				foreach (var file in Directory.GetFiles(dialog.SelectedPath).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
				{
					var extension = Path.GetExtension(file);
					if (!new[] { ".png", ".jpg", ".jpeg", ".bmp", ".dds" }.Contains(extension, StringComparer.OrdinalIgnoreCase)) { skipped++; continue; }
					var stem = Path.GetFileNameWithoutExtension(file);
					if (!stem.StartsWith("p", StringComparison.OrdinalIgnoreCase) || !int.TryParse(stem.Substring(1), out var id) || FifaEnvironment.Players.SearchId(id) == null) { skipped++; continue; }
					imports.Add(new MinifaceImport(file, id, extension.Equals(".dds", StringComparison.OrdinalIgnoreCase)));
				}
				var duplicateIds = imports.GroupBy(item => item.PlayerId).Where(group => group.Count() > 1).Select(group => group.Key).ToArray();
				if (duplicateIds.Length > 0)
					throw new InvalidDataException("More than one miniface was supplied for player ID(s): " + string.Join(", ", duplicateIds) + ". Keep one p<id> file per player.");

				// Validate and decode every candidate before staging the first asset so
				// malformed input cannot leave a predictable half-imported batch.
				foreach (var item in imports)
				{
					if (item.IsDds)
					{
						if (!TryReadDdsDimensions(item.Path, out var width, out var height) || width != 180 || height != 180)
							throw new InvalidDataException(Path.GetFileName(item.Path) + " must be a readable 180×180 DDS miniface.");
					}
					else using (var image = Image.FromFile(item.Path))
					{
						if (image.Width <= 0 || image.Height <= 0) throw new InvalidDataException(Path.GetFileName(item.Path) + " is not a readable portrait image.");
					}
				}

				var count = 0;
				foreach (var item in imports)
				{
					if (item.IsDds)
						Fc26HostBridge.StageImage(Player.SpecificPhotoDdsFileName(item.PlayerId), item.Path, 180, 180);
					else
					{
						var temporary = Path.Combine(Path.GetTempPath(), "cm26_miniface_" + item.PlayerId + "_" + Guid.NewGuid().ToString("N") + ".png");
						try
						{
							using (var source = Image.FromFile(item.Path))
							using (var aligned = CreateAlignedMiniface(source))
								aligned.Save(temporary, System.Drawing.Imaging.ImageFormat.Png);
							Fc26HostBridge.StageImage(Player.SpecificPhotoDdsFileName(item.PlayerId), temporary, 180, 180);
						}
						finally { try { if (File.Exists(temporary)) File.Delete(temporary); } catch { } }
					}
					count++;
				}
				return count + " aligned 180×180 miniface(s) staged; " + skipped + " unrelated/unknown file(s) skipped. Use File > Save to commit.";
			});
		}
	}

	private void ExportVisible(object sender, EventArgs e)
	{
		using (var dialog = new FolderBrowserDialog { Description = "Choose output folder for installed minifaces" })
		{
			if (dialog.ShowDialog(this) != DialogResult.OK) return;
			Run("Exporting minifaces...", () =>
			{
				var count = 0;
				foreach (var row in SelectedRows())
				{
					var source = Fc26HostBridge.ExportAsset(Player.SpecificPhotoDdsFileName(row.Id));
					if (string.IsNullOrWhiteSpace(source) || !File.Exists(source)) continue;
					File.Copy(source, Path.Combine(dialog.SelectedPath, "p" + row.Id + Path.GetExtension(source)), true);
					count++;
				}
				return count + " installed miniface(s) exported.";
			});
		}
	}

	private void MissingReport(object sender, EventArgs e)
	{
		using (var dialog = new SaveFileDialog { Filter = "CSV report|*.csv", FileName = "CM26_missing_faces.csv" })
		{
			if (dialog.ShowDialog(this) != DialogResult.OK) return;
			Run("Building missing-face report...", () =>
			{
				var lines = new List<string> { "PlayerID,Player,Miniface,Head,FaceTexture,Hair" };
				foreach (var row in SelectedRows())
				{
					row.Scan();
					if (row.Miniface == "Installed" && row.Head == "Installed" && row.FaceTexture == "Installed" && row.Hair == "Installed") continue;
					lines.Add(row.Id + ",\"" + row.PlayerName.Replace("\"", "\"\"") + "\"," + row.Miniface + "," + row.Head + "," + row.FaceTexture + "," + row.Hair);
				}
				File.WriteAllLines(dialog.FileName, lines, Encoding.UTF8);
				return (lines.Count - 1) + " incomplete player asset record(s) written to the report.";
			});
		}
	}

	private void RenameAssets(object sender, EventArgs e)
	{
		var row = Current(); if (row == null) return;
		using (var dialog = new Form { Text = "Rename Player Assets", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, ClientSize = new Size(350, 125) })
		using (var id = new NumericUpDown { Minimum = 1, Maximum = 999999999, Value = row.Id, Location = new Point(145, 18), Width = 160 })
		using (var ok = new Button { Text = "Preview & Stage", DialogResult = DialogResult.OK, Location = new Point(185, 72), AutoSize = true })
		{
			dialog.Controls.AddRange(new Control[] { new Label { Text = "New Player ID", Location = new Point(20, 22), AutoSize = true }, id, ok, new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(275, 72), AutoSize = true } });
			if (dialog.ShowDialog(this) != DialogResult.OK) return;
			var newId = (int)id.Value;
			if (newId == row.Id) return;
			if (MessageBox.Show(this, "Stage every installed player-linked asset from ID " + row.Id + " to " + newId + "?\r\nThe database ID itself must be changed with the dependency-aware ID manager.", "Asset Rename Preview", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
			Run("Staging linked asset rename...", () => RenameFamilies(row.Id, newId) + " linked asset(s) staged for rename.");
		}
	}

	private static int RenameFamilies(int oldId, int newId)
	{
		var pairs = new[]
		{
			Tuple.Create(Player.SpecificPhotoDdsFileName(oldId), Player.SpecificPhotoDdsFileName(newId)),
			Tuple.Create(Player.SpecificHeadModelFileName(oldId), Player.SpecificHeadModelFileName(newId)),
			Tuple.Create(Player.SpecificFaceTextureFileName(oldId), Player.SpecificFaceTextureFileName(newId)),
			Tuple.Create(Player.SpecificHairModelFileName(oldId), Player.SpecificHairModelFileName(newId)),
			Tuple.Create(Player.SpecificHairLodModelFileName(oldId), Player.SpecificHairLodModelFileName(newId)),
			Tuple.Create(Player.SpecificHairTexturesFileName(oldId), Player.SpecificHairTexturesFileName(newId)),
			Tuple.Create("data/sceneassets/body/playerskin_" + oldId + "_textures.rx3", "data/sceneassets/body/playerskin_" + newId + "_textures.rx3"),
			Tuple.Create("data/sceneassets/tattoo/tattoo_" + oldId + "_0.rx3", "data/sceneassets/tattoo/tattoo_" + newId + "_0.rx3")
		};
		var count = 0;
		foreach (var pair in pairs)
		{
			var source = Fc26HostBridge.ExportAsset(pair.Item1);
			if (string.IsNullOrWhiteSpace(source) || !File.Exists(source)) continue;
			Fc26HostBridge.StageFile(pair.Item2, source);
			Fc26HostBridge.RemoveStagedAsset(pair.Item1);
			count++;
		}
		return count;
	}

	private void AssignSpecificFace(object sender, EventArgs e)
	{
		var row = Current(); if (row == null) return;
		row.Player.headclasscode = 0;
		_status.Text = "Specific face assignment staged for " + row.PlayerName + ". Import/check the matching native head and texture, then File > Save.";
	}

	private void GenericAppearance(object sender, EventArgs e)
	{
		var row = Current(); if (row == null) return;
		using (var dialog = new Form { Text = "Generic Face / Hair / Facial Hair", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, ClientSize = new Size(390, 235) })
		using (var head = Number(row.Player.headtypecode, 0, 999999, 170, 20))
		using (var hair = Number(row.Player.hairtypecode, 0, 999999, 170, 60))
		using (var beard = Number(row.Player.facialhairtypecode, 0, 999999, 170, 100))
		using (var skin = Number(row.Player.skintonecode, 1, 10, 170, 140))
		{
			dialog.Controls.AddRange(new Control[] { LabelAt("Generic head ID", 20), LabelAt("Hair ID", 60), LabelAt("Facial-hair ID", 100), LabelAt("Skin tone", 140), head, hair, beard, skin,
				new Button { Text = "Apply staged", DialogResult = DialogResult.OK, Location = new Point(205, 185), AutoSize = true }, new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(305, 185), AutoSize = true } });
			if (dialog.ShowDialog(this) != DialogResult.OK) return;
			row.Player.headclasscode = 1; row.Player.headtypecode = (int)head.Value; row.Player.hairtypecode = (int)hair.Value; row.Player.facialhairtypecode = (int)beard.Value; row.Player.skintonecode = (int)skin.Value;
			_status.Text = "Generic head/hair/facial-hair assignment staged for " + row.PlayerName + ".";
		}
	}

	private void ImportNativeFace(object sender, EventArgs e)
	{
		var row = Current(); if (row == null) return;
		using (var type = new Form { Text = "Import Native FC26 Face Asset", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, ClientSize = new Size(430, 150) })
		using (var choice = new ComboBox { Location = new Point(25, 25), Width = 375, DropDownStyle = ComboBoxStyle.DropDownList })
		{
			choice.Items.AddRange(new object[] { "Specific head model", "Specific face texture", "Specific hair model", "Specific hair LOD", "Specific hair texture", "Player skin", "Tattoo / cranium container" }); choice.SelectedIndex = 0;
			type.Controls.AddRange(new Control[] { choice, new Button { Text = "Choose native file...", DialogResult = DialogResult.OK, Location = new Point(230, 85), AutoSize = true }, new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(350, 85), AutoSize = true } });
			if (type.ShowDialog(this) != DialogResult.OK) return;
			var paths = new[] { Player.SpecificHeadModelFileName(row.Id), Player.SpecificFaceTextureFileName(row.Id), Player.SpecificHairModelFileName(row.Id), Player.SpecificHairLodModelFileName(row.Id), Player.SpecificHairTexturesFileName(row.Id), "data/sceneassets/body/playerskin_" + row.Id + "_textures.rx3", "data/sceneassets/tattoo/tattoo_" + row.Id + "_0.rx3" };
			using (var file = new OpenFileDialog { Filter = "FC26 native RX3/container|*.rx3;*.bin|All files|*.*", CheckFileExists = true })
			{
				if (file.ShowDialog(this) != DialogResult.OK) return;
				var target = paths[choice.SelectedIndex];
				if (!Path.GetExtension(file.FileName).Equals(Path.GetExtension(target), StringComparison.OrdinalIgnoreCase)) { MessageBox.Show(this, "Native replacement extension must match " + Path.GetExtension(target) + ".", Text); return; }
				Fc26HostBridge.StageFile(target, file.FileName);
				if (choice.SelectedIndex <= 1) row.Player.headclasscode = 0;
				_status.Text = "Native face/cranium asset staged at " + target + ". Use File > Save to validate and commit.";
			}
		}
	}

	private void ExportNativeFace(object sender, EventArgs e)
	{
		var row = Current(); if (row == null) return;
		var labels = new[] { "Specific head model", "Specific face texture", "Specific hair model", "Specific hair LOD", "Specific hair texture", "Player skin", "Tattoo / cranium container" };
		var paths = new[] { Player.SpecificHeadModelFileName(row.Id), Player.SpecificFaceTextureFileName(row.Id), Player.SpecificHairModelFileName(row.Id), Player.SpecificHairLodModelFileName(row.Id), Player.SpecificHairTexturesFileName(row.Id), "data/sceneassets/body/playerskin_" + row.Id + "_textures.rx3", "data/sceneassets/tattoo/tattoo_" + row.Id + "_0.rx3" };
		using (var dialog = new Form { Text = "Export Native FC26 Face Asset", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, ClientSize = new Size(430, 150) })
		using (var choice = new ComboBox { Location = new Point(25, 25), Width = 375, DropDownStyle = ComboBoxStyle.DropDownList })
		{
			choice.Items.AddRange(labels); choice.SelectedIndex = 0;
			dialog.Controls.AddRange(new Control[] { choice, new Button { Text = "Export...", DialogResult = DialogResult.OK, Location = new Point(250, 85), AutoSize = true }, new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(340, 85), AutoSize = true } });
			if (dialog.ShowDialog(this) != DialogResult.OK) return;
			var source = Fc26HostBridge.ExportAsset(paths[choice.SelectedIndex]);
			if (string.IsNullOrWhiteSpace(source) || !File.Exists(source)) { MessageBox.Show(this, "That native asset is not installed for the selected player."); return; }
			using var save = new SaveFileDialog { Filter = "Native FC26 asset|*" + Path.GetExtension(source) + "|All files|*.*", FileName = Path.GetFileName(source) };
			if (save.ShowDialog(this) != DialogResult.OK) return;
			File.Copy(source, save.FileName, true); _status.Text = "Native face/cranium asset exported: " + save.FileName;
		}
	}

	private void AlignMiniface(object sender, EventArgs e)
	{
		var row = Current(); if (row == null) return;
		using var open = new OpenFileDialog { Filter = "Portrait image|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*", CheckFileExists = true };
		if (open.ShowDialog(this) != DialogResult.OK) return;
		try
		{
			using var source = new Bitmap(open.FileName);
			using var aligned = CreateAlignedMiniface(source);
			var temp = Path.Combine(Path.GetTempPath(), "cm26_miniface_" + row.Id + "_" + Guid.NewGuid().ToString("N") + ".png");
			aligned.Save(temp, System.Drawing.Imaging.ImageFormat.Png);
			try { Fc26HostBridge.StageImage(Player.SpecificPhotoDdsFileName(row.Id), temp, 180, 180); }
			finally { try { File.Delete(temp); } catch { } }
			ReplacePreview(new Bitmap(aligned));
			_status.Text = "180×180 centred miniface staged for " + row.PlayerName + ". Preview and use File > Save to commit.";
		}
		catch (Exception ex) { Fc26FriendlyError.Show(this, "Align miniface", ex, "No miniface was staged."); }
	}

	private static Bitmap CreateAlignedMiniface(Image source)
	{
		if (source == null || source.Width <= 0 || source.Height <= 0) throw new InvalidDataException("A readable portrait image is required.");
		var side = Math.Min(source.Width, source.Height);
		var x = Math.Max(0, (source.Width - side) / 2);
		var y = Math.Max(0, Math.Min(source.Height - side, (source.Height - side) / 3));
		var aligned = new Bitmap(180, 180, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		using (var graphics = Graphics.FromImage(aligned))
		{
			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			graphics.DrawImage(source, new Rectangle(0, 0, 180, 180), new Rectangle(x, y, side, side), GraphicsUnit.Pixel);
		}
		return aligned;
	}

	private static bool TryReadDdsDimensions(string path, out int width, out int height)
	{
		width = 0; height = 0;
		try
		{
			using (var stream = File.OpenRead(path))
			using (var reader = new BinaryReader(stream))
			{
				if (stream.Length < 128 || reader.ReadUInt32() != 0x20534444 || reader.ReadUInt32() != 124) return false;
				reader.ReadUInt32();
				height = reader.ReadInt32(); width = reader.ReadInt32();
				return width > 0 && height > 0;
			}
		}
		catch { width = 0; height = 0; return false; }
	}

	internal static Bitmap CreateAlignedMinifaceForTest(Image source) => CreateAlignedMiniface(source);
	internal static bool TryReadDdsDimensionsForTest(string path, out int width, out int height) => TryReadDdsDimensions(path, out width, out height);

	private async void FaceSimilarity(object sender, EventArgs e)
	{
		using var open = new OpenFileDialog { Filter = "Portrait image|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*", CheckFileExists = true };
		if (open.ShowDialog(this) != DialogResult.OK) return;
		try
		{
			_status.Text = "Comparing portrait with installed minifaces in the background...";
			var targets = SelectedRows().Take(500).ToArray();
			var sourcePath = open.FileName;
			var matches = await ThreadingTask.Run(() =>
			{
				using var source = new Bitmap(sourcePath); var signature = ImageSignature(source);
				return targets.Select(target =>
				{
					var file = Fc26HostBridge.ExportAsset(Player.SpecificPhotoDdsFileName(target.Id));
					if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) return new { Row = target, Score = double.MaxValue };
					try { using var image = new Bitmap(file); return new { Row = target, Score = SignatureDistance(signature, ImageSignature(image)) }; }
					catch { return new { Row = target, Score = double.MaxValue }; }
				}).Where(result => result.Score < double.MaxValue).OrderBy(result => result.Score).Take(10).ToArray();
			});
			var report = matches.Length == 0 ? "No readable installed minifaces were found." : string.Join("\r\n", matches.Select((match, index) => (index + 1) + ". " + match.Row.PlayerName + " [" + match.Row.Id + "] — similarity " + Math.Max(0, 100 - match.Score).ToString("0.0") + "%"));
			MessageBox.Show(this, report + "\r\n\r\nThis is a visual colour/layout helper, not biometric identification.", "Face similarity helper");
			_status.Text = "Face similarity comparison complete.";
		}
		catch (Exception ex) { Fc26FriendlyError.Show(this, "Face similarity", ex, "No player assignment was changed."); }
	}

	private static double[] ImageSignature(Bitmap source)
	{
		using var sample = new Bitmap(8, 8);
		using (var graphics = Graphics.FromImage(sample)) graphics.DrawImage(source, new Rectangle(0, 0, 8, 8));
		var values = new List<double>(192);
		for (var y = 0; y < 8; y++) for (var x = 0; x < 8; x++) { var color = sample.GetPixel(x, y); values.Add(color.R / 255d); values.Add(color.G / 255d); values.Add(color.B / 255d); }
		return values.ToArray();
	}

	private static double SignatureDistance(double[] left, double[] right)
	{
		var sum = 0d; for (var index = 0; index < Math.Min(left.Length, right.Length); index++) { var delta = left[index] - right[index]; sum += delta * delta; }
		return Math.Min(100, Math.Sqrt(sum / Math.Max(1, left.Length)) * 100);
	}

	private void PreviewSelected()
	{
		var row = Current(); if (row == null) return;
		try
		{
			var file = Fc26HostBridge.ExportAsset(Player.SpecificPhotoDdsFileName(row.Id));
			if (string.IsNullOrWhiteSpace(file) || !File.Exists(file)) { ReplacePreview(null); return; }
			using (var source = Image.FromFile(file)) ReplacePreview(new Bitmap(source));
		}
		catch { ReplacePreview(null); }
	}

	private void ReplacePreview(Image image) { var old = _preview.Image; _preview.Image = image; old?.Dispose(); }
	private Row Current() => _grid.CurrentRow?.DataBoundItem as Row;
	private static NumericUpDown Number(int value, int min, int max, int x, int y) => new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, value)), Location = new Point(x, y), Width = 180 };
	private static Label LabelAt(string text, int y) => new Label { Text = text, Location = new Point(25, y + 4), AutoSize = true };
	private void Run(string message, Func<string> action)
	{
		try { Cursor = Cursors.WaitCursor; _status.Text = message; Application.DoEvents(); _status.Text = action(); }
		catch (Exception ex) { _status.Text = "Face operation stopped safely."; Fc26FriendlyError.Show(this, "Face tools", ex, "No unverified face or miniface change was accepted."); }
		finally { Cursor = Cursors.Default; }
	}

	private sealed class Row
	{
		internal Player Player { get; }
		internal Row(Player player) { Player = player; }
		public int Id => Player.Id;
		public string PlayerName => Player.ToString();
		public string Team => Player.GetClub()?.TeamNameFull ?? "Free agent";
		public string Miniface { get; private set; } = "Not scanned";
		public string Head { get; private set; } = "Not scanned";
		public string FaceTexture { get; private set; } = "Not scanned";
		public string Hair { get; private set; } = "Not scanned";
		internal void Scan()
		{
			Miniface = Exists(Player.SpecificPhotoDdsFileName(Id)); Head = Exists(Player.SpecificHeadModelFileName(Id));
			FaceTexture = Exists(Player.SpecificFaceTextureFileName(Id)); Hair = Exists(Player.SpecificHairModelFileName(Id));
		}
		private static string Exists(string path) { var file = Fc26HostBridge.ExportAsset(path); return !string.IsNullOrWhiteSpace(file) && File.Exists(file) ? "Installed" : "Missing"; }
	}
	private sealed class MinifaceImport
	{
		internal MinifaceImport(string path, int playerId, bool isDds) { Path = path; PlayerId = playerId; IsDds = isDds; }
		internal string Path { get; }
		internal int PlayerId { get; }
		internal bool IsDds { get; }
	}
	private sealed class TeamChoice
	{
		internal TeamChoice(Team team) { Team = team; }
		internal Team Team { get; }
		public override string ToString() => Team == null ? "All teams" : Team.TeamNameFull;
	}
}
