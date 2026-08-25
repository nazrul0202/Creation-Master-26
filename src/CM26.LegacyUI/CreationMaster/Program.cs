using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace CreationMaster;

internal static class Program
{
	[STAThread]
	private static void Main(string[] args)
	{
		Fc26HostBridge.Configure(args);
		FifaLibrary.FifaEnvironment.Fc26AssetExporter = Fc26HostBridge.ExportAsset;
		if (args.Length >= 2 && string.Equals(args[0], "--cm26-data-integrity-test", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				var afghanistan = FifaLibrary.FifaEnvironment.Countries.SearchId(149) as FifaLibrary.Country
					?? throw new InvalidDataException("FC26 nation 149 (Afghanistan) is missing.");
				if (afghanistan.Confederation != 4)
					throw new InvalidDataException("Afghanistan must map to legacy Asia index 4, not " + afghanistan.Confederation + ".");

				var manchesterUnited = FifaLibrary.FifaEnvironment.Teams.SearchId(11) as FifaLibrary.Team
					?? throw new InvalidDataException("FC26 team 11 (Manchester United) is missing.");
				if (manchesterUnited.Roster.Count < 11)
					throw new InvalidDataException("Manchester United has fewer than eleven loaded players.");
				var starterRoles = new System.Collections.Generic.HashSet<int>();
				for (var slot = 0; slot < 11; slot++)
				{
					var player = manchesterUnited.Roster[slot] as FifaLibrary.TeamPlayer;
					if (player == null || player.position < 0 || player.position >= 28)
						throw new InvalidDataException("Manchester United starter slot " + slot + " has no valid pitch role.");
					starterRoles.Add(player.position);
				}
				if (starterRoles.Count != 11)
					throw new InvalidDataException("Manchester United starters collapse into " + starterRoles.Count + " pitch roles.");
				Environment.ExitCode = 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Environment.ExitCode = 1;
			}
			return;
		}
		if (args.Length >= 3 && string.Equals(args[0], "--cm26-team-transfer-plan-test", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				FifaLibrary.Team source = null;
				FifaLibrary.Team destination = null;
				FifaLibrary.TeamPlayer playerLink = null;
				foreach (FifaLibrary.Team candidate in FifaLibrary.FifaEnvironment.Teams)
				{
					if (candidate.NationalTeam) continue;
					if (source == null && candidate.Roster.Count > 0)
					{
						source = candidate;
						playerLink = (FifaLibrary.TeamPlayer)candidate.Roster[0];
					}
					else if (source != null && candidate != source) { destination = candidate; break; }
				}
				if (source == null || destination == null || playerLink == null)
					throw new InvalidDataException("FC26 transfer test could not find two club teams.");
				// The UI move uses this same linked TeamPlayer object. Setting its
				// destination is sufficient here to verify snapshot persistence without
				// disturbing formation/set-piece assignments during the diagnostic.
				playerLink.Team = destination;
				Fc26SnapshotLoader.WriteChanges(args[2]);
				var plan = File.ReadAllText(args[2]);
				var expected = "\"FieldName\":\"teamid\",\"Value\":\"" + destination.Id + "\"";
				if (plan.IndexOf(expected, StringComparison.Ordinal) < 0)
					throw new InvalidDataException("FC26 player transfer was not saved as a teamplayerlinks.teamid change.");
				Environment.ExitCode = 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Environment.ExitCode = 1;
			}
			return;
		}
		if (args.Length >= 3 && string.Equals(args[0], "--cm26-tactics-plan-test", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				var team = FifaLibrary.FifaEnvironment.Teams.SearchId(111235) as FifaLibrary.Team
					?? throw new InvalidDataException("FC26 tactics test team 111235 is missing.");
				team.buildupplay = team.buildupplay == 3 ? 2 : 3;
				team.defensivedepth = team.defensivedepth == 90 ? 65 : 90;
				team.SetFc26KnownTraitMask(1, (team.GetFc26TraitMask(1) ^ 1) & 1023);
				Fc26SnapshotLoader.WriteChanges(args[2]);
				var plan = File.ReadAllText(args[2]);
				foreach (var required in new[]
				{
					"\"TableName\":\"teams\"", "\"FieldName\":\"buildupplay\"",
					"\"FieldName\":\"defensivedepth\"", "\"FieldName\":\"trait1vequal\"",
					"\"TableName\":\"default_mentalities\"", "\"TableName\":\"defaultteamdata\""
				})
					if (plan.IndexOf(required, StringComparison.Ordinal) < 0)
						throw new InvalidDataException("FC26 tactics plan is missing " + required + ".");
				Environment.ExitCode = 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Environment.ExitCode = 1;
			}
			return;
		}
		if (args.Length >= 3 && string.Equals(args[0], "--cm26-plan", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				var count = Fc26SnapshotLoader.WriteChanges(args[2]);
				Environment.ExitCode = count == 0 ? 0 : 2;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Environment.ExitCode = 1;
			}
			return;
		}
		if (args.Length >= 3 && string.Equals(args[0], "--cm26-detail-plan-test", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				var rivals = Fc26SnapshotLoader.DetailTable("rivals")
					?? throw new InvalidDataException("Rival records are missing from the snapshot.");
				if (rivals.Rows.Count == 0) throw new InvalidDataException("No rival record is available for the test.");
				var original = int.TryParse(rivals.Value(0, "rivaltype"), out var value) ? value : 0;
				var replacement = original == 1 ? 2 : 1;
				Fc26SnapshotLoader.StageDetailValue("rivals", 0, "rivaltype", replacement.ToString());
				Fc26SnapshotLoader.WriteChanges(args[2]);
				var plan = File.ReadAllText(args[2]);
				if (plan.IndexOf("\"TableName\":\"rivals\"", StringComparison.Ordinal) < 0 ||
					plan.IndexOf("\"FieldName\":\"rivaltype\"", StringComparison.Ordinal) < 0)
					throw new InvalidDataException("The structured detail edit was not written to the save plan.");
				Environment.ExitCode = 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Environment.ExitCode = 1;
			}
			return;
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
		if (args.Length >= 1 && string.Equals(args[0], "--cm26-ui-integration-test", StringComparison.OrdinalIgnoreCase))
		{
			var uiLog = Path.Combine(Path.GetTempPath(), "cm26-ui-integration.log");
			var errorLog = Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log");
			File.WriteAllText(uiLog, "starting" + Environment.NewLine);
			if (File.Exists(errorLog)) File.Delete(errorLog);
			try
			{
				var main = new MainForm();
				File.AppendAllText(uiLog, "classic shell constructed" + Environment.NewLine);
				if (FindControl(main, "fc26DirectToolsStrip") != null)
					throw new InvalidDataException("The obsolete global Direct Tools bar is still present.");
				foreach (var forbidden in new[]
				{
					"cm26InlineSectionTabs", "cm26Inline_PlayerIDNames", "cm26Inline_FaceMiniface",
					"cm26Inline_BatchPlayerMatrix", "cm26Inline_AdvancedTeamData",
					"cm26Inline_AdvancedLeagueData", "cm26Inline_AdvancedAudioRecords"
				})
				{
					if (FindControl(main, forbidden) != null)
						throw new InvalidDataException("Raw/embedded section remains visible: " + forbidden);
				}
				if (ContainsMenuText(main.MainMenuStrip?.Items, "Advanced Database Workspace") ||
					ContainsMenuText(main.MainMenuStrip?.Items, "Internal Utilities"))
					throw new InvalidDataException("A raw database/internal utility entry is still exposed in the public menu.");
				if (!ContainsControlText(main.m_TeamForm, "Club Relations") ||
					!ContainsControlText(main.m_PlayerForm, "Career Details") ||
					!ContainsControlText(main.m_CountryForm, "Association Details") ||
					!ContainsControlText(main.m_LeagueForm, "League Details"))
					throw new InvalidDataException("A mapped, friendly CM26 details surface is missing.");
				var miniface = FindControl(main.m_PlayerForm, "viewer2DPhoto");
				if (miniface == null || miniface.Width > 128 || miniface.Height > 153)
					throw new InvalidDataException("Player miniface exceeds the classic CM26 layout boundary.");
				File.AppendAllText(uiLog, "passed" + Environment.NewLine);
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				File.WriteAllText(errorLog, ex.ToString());
				Environment.Exit(1);
			}
			return;
		}
		if (args.Length >= 3 && string.Equals(args[0], "--cm26-smoke", StringComparison.OrdinalIgnoreCase))
		{
			var errorLog = Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log");
			if (File.Exists(errorLog)) File.Delete(errorLog);
			Application.ThreadException += (_, e) =>
			{
				File.WriteAllText(errorLog, e.Exception.ToString());
				Environment.Exit(1);
			};
			AppDomain.CurrentDomain.UnhandledException += (_, e) =>
			{
				File.WriteAllText(errorLog, (e.ExceptionObject as Exception)?.ToString() ?? "Unknown smoke-test failure.");
				Environment.Exit(1);
			};
			try
			{
				var smokeLog = Path.Combine(Path.GetTempPath(), "cm26-legacy-smoke.log");
				if (File.Exists(smokeLog)) File.Delete(smokeLog);
				var total = Stopwatch.StartNew();
				using var main = new MainForm();
				main.Show();
				Application.DoEvents();
				main.LoadFc26Snapshot(args[1], showCountry: false);
				foreach (var section in args[2].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					var name = section.Trim();
					var timer = Stopwatch.StartNew();
					main.ClickFc26SectionForSmoke(name);
					Application.DoEvents();
					main.AssertFc26SectionVisible(name);
					main.AuditFc26RecordsForSmoke(name);
					File.AppendAllText(smokeLog, name + "=" + timer.ElapsedMilliseconds + "ms" + Environment.NewLine);
				}
				File.AppendAllText(smokeLog, "total=" + total.ElapsedMilliseconds + "ms" + Environment.NewLine);
				main.Dispose();
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				File.WriteAllText(errorLog, ex.ToString());
				Environment.Exit(1);
			}
		}
		var diagnostic = args.Length >= 1 && string.Equals(args[0], "--cm26-snapshot", StringComparison.OrdinalIgnoreCase);
		Application.ThreadException += (_, e) => HandleUnhandled(e.Exception, diagnostic);
		AppDomain.CurrentDomain.UnhandledException += (_, e) => HandleUnhandled(e.ExceptionObject as Exception, diagnostic);
		try
		{
			var main = new MainForm();
			if (args.Length >= 2 && string.Equals(args[0], "--cm26-snapshot", StringComparison.OrdinalIgnoreCase))
			{
				main.LoadFc26Snapshot(args[1], showCountry: args.Length < 3);
				if (args.Length >= 3) main.ShowFc26Section(args[2]);
			}
			Application.Run(main);
		}
		catch (Exception ex)
		{
			var log = Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log");
			File.WriteAllText(log, ex.ToString());
			if (!(args.Length >= 1 && string.Equals(args[0], "--cm26-snapshot", StringComparison.OrdinalIgnoreCase)))
				MessageBox.Show(ex.Message + "\r\n\r\n" + log, "Creation Master 26", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private static void HandleUnhandled(Exception exception, bool diagnostic)
	{
		var log = Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log");
		File.WriteAllText(log, (exception ?? new Exception("Unknown legacy UI error.")).ToString());
		if (diagnostic) Environment.Exit(1);
		MessageBox.Show((exception?.Message ?? "Unknown error") + "\r\n\r\n" + log,
			"Creation Master 26", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}

	private static Control FindControl(Control root, string name)
	{
		if (root == null) return null;
		if (string.Equals(root.Name, name, StringComparison.Ordinal)) return root;
		foreach (Control child in root.Controls)
		{
			var match = FindControl(child, name);
			if (match != null) return match;
		}
		return null;
	}

	private static bool ContainsMenuText(ToolStripItemCollection items, string text)
	{
		if (items == null) return false;
		foreach (ToolStripItem item in items)
		{
			if (item.Text?.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) return true;
			if (item is ToolStripDropDownItem dropDown && ContainsMenuText(dropDown.DropDownItems, text)) return true;
		}
		return false;
	}

	private static bool ContainsControlText(Control root, string text)
	{
		if (root == null) return false;
		if (string.Equals(root.Text, text, StringComparison.OrdinalIgnoreCase)) return true;
		foreach (Control child in root.Controls)
			if (ContainsControlText(child, text)) return true;
		return false;
	}
}
