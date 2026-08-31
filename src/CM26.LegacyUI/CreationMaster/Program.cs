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
		if (args.Length >= 3 && string.Equals(args[0], "--cm26-create-plan-test", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				var created = new (string Section, FifaLibrary.IdObject Item)[]
				{
					("nation", FifaLibrary.FifaEnvironment.Countries.CreateNewId()),
					("league", FifaLibrary.FifaEnvironment.Leagues.CreateNewId()),
					("team", FifaLibrary.FifaEnvironment.Teams.CreateNewId()),
					("player", FifaLibrary.FifaEnvironment.Players.CreateNewId())
				};
				foreach (var value in created)
				{
					if (value.Item == null) throw new InvalidDataException("No free ID is available for " + value.Section + ".");
					Fc26SnapshotLoader.StageNewEntity(value.Section, value.Item);
				}
				Fc26SnapshotLoader.AssignTeamToLeague((FifaLibrary.Team)created[2].Item,
					(FifaLibrary.League)created[1].Item);
				Fc26SnapshotLoader.WriteChanges(args[2]);
				var plan = File.ReadAllText(args[2]);
				foreach (var required in new[]
				{
					"\"TableName\":\"nations\"", "\"FieldName\":\"nationid\"",
					"\"TableName\":\"leagues\"", "\"FieldName\":\"leagueid\"",
					"\"TableName\":\"teams\"", "\"FieldName\":\"teamid\"",
					"\"TableName\":\"players\"", "\"FieldName\":\"playerid\"",
					"\"TableName\":\"leagueteamlinks\"", "\"FieldName\":\"leagueid\"",
					"\"Kind\":\"duplicate\""
				})
					if (plan.IndexOf(required, StringComparison.Ordinal) < 0)
						throw new InvalidDataException("Direct creation plan is missing " + required + ".");
				Environment.ExitCode = 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Environment.ExitCode = 1;
			}
			return;
		}
		if (args.Length >= 2 && string.Equals(args[0], "--cm26-id-availability", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				Console.WriteLine(Fc26SnapshotLoader.DescribeIdAvailability());
				Environment.ExitCode = 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Console.Error.WriteLine(ex.Message);
				Environment.ExitCode = 1;
			}
			return;
		}
		if (args.Length >= 3 && string.Equals(args[0], "--cm26-player-name-plan-test", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				FifaLibrary.Player source = null;
				FifaLibrary.Player target = null;
				foreach (FifaLibrary.Player candidate in FifaLibrary.FifaEnvironment.Players)
				{
					if (candidate.lastnameid <= 0 || string.IsNullOrWhiteSpace(candidate.lastname)) continue;
					if (source == null) { source = candidate; continue; }
					if (candidate != source) { target = candidate; break; }
				}
				if (source == null || target == null)
					throw new InvalidDataException("The FC26 snapshot has no two players suitable for the shared-name test.");

				// Reproduce the real UI failure: two Player objects point at one
				// shared nameid, but one has a different edited surname.  The writer
				// must allocate a fresh row rather than changing the shared source.
				var sharedId = source.lastnameid;
				var editedName = (source.lastname.Trim() + " CM26 Test").Trim();
				target.lastnameid = sharedId;
				target.lastname = editedName;
				Fc26SnapshotLoader.WriteChanges(args[2]);
				var plan = File.ReadAllText(args[2]);
				if (plan.IndexOf("\"TableName\":\"playernames\"", StringComparison.Ordinal) < 0 ||
					plan.IndexOf("\"FieldName\":\"lastnameid\"", StringComparison.Ordinal) < 0 ||
					plan.IndexOf(editedName, StringComparison.Ordinal) < 0 ||
					plan.IndexOf("\"Kind\":\"duplicate\"", StringComparison.Ordinal) < 0)
					throw new InvalidDataException("Shared player-name edit did not allocate a new playernames row.");
				Console.WriteLine("PLAYER NAME PLAN TEST: shared ID " + sharedId + " detached to a fresh playernames row.");
				Environment.ExitCode = 0;
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Console.Error.WriteLine(ex.Message);
				Environment.ExitCode = 1;
			}
			return;
		}
		if (args.Length >= 2 && string.Equals(args[0], "--cm26-team-roster-clone-test", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				Fc26SnapshotLoader.Load(args[1]);
				FifaLibrary.Team source = null;
				foreach (FifaLibrary.Team candidate in FifaLibrary.FifaEnvironment.Teams)
				{
					if (!candidate.NationalTeam && candidate.Roster.Count > 0) { source = candidate; break; }
				}
				if (source == null) throw new InvalidDataException("No FC26 club roster is available for clone testing.");
				var link = (FifaLibrary.TeamPlayer)source.Roster[0];
				var cloneId = FifaLibrary.FifaEnvironment.Players.GetNewId();
				var clone = (FifaLibrary.Player)FifaLibrary.FifaEnvironment.Players.CloneId(link.Player, cloneId);
				clone.headclasscode = 1;
				clone.firstname = string.Empty;
				clone.lastname = "Player_" + clone.Id;
				clone.commonname = string.Empty;
				clone.playerjerseyname = string.Empty;
				clone.commentaryid = 900000;
				clone.RandomizeAppearanceSameRace();
				var clonedLink = new FifaLibrary.TeamPlayer(clone) { position = link.position, jerseynumber = link.jerseynumber };
				if (clone.commentaryid != 900000 || clonedLink.Player != clone)
					throw new InvalidDataException("FC26 starter-roster clone did not complete correctly.");
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
		if (args.Length >= 1 && string.Equals(args[0], "--cm26-workspace-size-test", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				using (var main = new MainForm())
				{
					main.Size = new System.Drawing.Size(1600, 900);
					main.Show();
					Application.DoEvents();
					var workspace = FindControl(main, "panelMain") as Panel
						?? throw new InvalidDataException("The classic main workspace panel is missing.");
					var showEditor = typeof(MainForm).GetMethod("ShowFormOnPanel",
						System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
						?? throw new InvalidDataException("The classic editor host method is missing.");
					using (var sizeProbe = new Form { TopLevel = false, FormBorderStyle = FormBorderStyle.None })
					{
						showEditor.Invoke(main, new object[] { sizeProbe, workspace });
						Application.DoEvents();
						if (sizeProbe.Bounds != workspace.ClientRectangle)
							throw new InvalidDataException("A hosted editor does not fill the classic workspace: " +
								sizeProbe.Bounds + " vs " + workspace.ClientRectangle + ".");
					}
				}
				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log"), ex.ToString());
				Environment.Exit(1);
			}
			return;
		}
		if (args.Length >= 1 && string.Equals(args[0], "--cm26-ui-integration-test", StringComparison.OrdinalIgnoreCase))
		{
			var uiLog = Path.Combine(Path.GetTempPath(), "cm26-ui-integration.log");
			var errorLog = Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log");
			File.WriteAllText(uiLog, "starting" + Environment.NewLine);
			if (File.Exists(errorLog)) File.Delete(errorLog);
			try
			{
				// Create Team can clone a starter roster while FC26 intentionally
				// leaves the legacy PlayerNamesList unloaded. This must round-trip
				// locally rather than throwing a NullReferenceException.
				var commentaryProbe = new FifaLibrary.Player(999999) { lastname = "CM26_Test" };
				commentaryProbe.commentaryid = 900000;
				if (commentaryProbe.commentaryid != 900000)
					throw new InvalidDataException("FC26 player commentary fallback failed.");
				var main = new MainForm();
				File.AppendAllText(uiLog, "classic shell constructed" + Environment.NewLine);
				var previousYear = FifaLibrary.FifaEnvironment.Year;
				FifaLibrary.FifaEnvironment.Year = 26;
				try
				{
					if (!main.m_TeamForm.UsesGuidedFc26TeamCreator)
						throw new InvalidDataException("The Team picker New button does not route to the guided FC26 creator.");
				}
				finally { FifaLibrary.FifaEnvironment.Year = previousYear; }
				using (var legacyMessage = new FifaLibrary.UserMessage())
				{
					var messageType = typeof(FifaLibrary.UserMessage);
					var suppressCheck = messageType.GetField("checkSuppressMessage",
						System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
						?.GetValue(legacyMessage) as CheckBox;
					var suppress = messageType.GetMethod("SuppressCurrentMessageIfRequested",
						System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
					if (suppressCheck == null || suppress == null)
						throw new InvalidDataException("Legacy message safety guard is unavailable.");
					suppressCheck.Checked = true;
					suppress.Invoke(legacyMessage, null);
				}
				foreach (var command in new[] { "Create New League...", "Create New Team..." })
					if (!ContainsMenuText(main.MainMenuStrip?.Items, command))
						throw new InvalidDataException("The direct record command is missing: " + command);
				if (!ContainsMenuText(main.MainMenuStrip?.Items, "Public Readiness Centre..."))
					throw new InvalidDataException("The public direct-edit readiness entry point is missing.");
				if (ContainsMenuText(main.MainMenuStrip?.Items, "Create New Player...") ||
					ContainsMenuText(main.MainMenuStrip?.Items, "Create New Nation..."))
					throw new InvalidDataException("The public Create menu must contain only League and Team.");
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
					ContainsMenuText(main.MainMenuStrip?.Items, "Internal Utilities") ||
					ContainsMenuText(main.MainMenuStrip?.Items, "Visual Asset Manager") ||
					ContainsMenuText(main.MainMenuStrip?.Items, "Compdata Editor") ||
					ContainsMenuText(main.MainMenuStrip?.Items, "Batch Player Editor") ||
					ContainsMenuText(main.MainMenuStrip?.Items, "Miniface & Face Tools") ||
					ContainsMenuText(main.MainMenuStrip?.Items, "Roster, National Team") ||
					ContainsMenuText(main.MainMenuStrip?.Items, "Career Save Module"))
					throw new InvalidDataException("A raw or separate specialist editor is still exposed in the public menu.");
				using (var launcher = new Fc26ProjectLauncherForm(
					() => { }, () => { }, () => { }, () => { },
					() => { }, () => { }, () => { }, () => { }))
				{
					foreach (var requiredLauncherAction in new[]
					{
						"Open FC26 Game", "Open extracted database", "Open CM26 project/session",
						"Database & localisation", "Squads & roster",
						"Manager / Player Career", "Tournament / competitions"
					})
					{
						if (!ContainsControlText(launcher, requiredLauncherAction))
							throw new InvalidDataException("The comprehensive project launcher is missing: " + requiredLauncherAction);
					}
				}
				using (var readiness = new Fc26PublicReadinessForm(main))
				{
					foreach (var required in new[]
					{
						"DIRECT EDIT PIPELINE", "Team Complete & Squad Doctor", "League & Compdata Pro",
						"Kit & Asset Centre", "Safe ID Migration", "Save Direct to FC26"
					})
						if (!ContainsControlText(readiness, required))
							throw new InvalidDataException("Public Readiness Centre is missing: " + required);
					if (ContainsControlText(readiness, "Open in FIFA Editing Tool"))
						throw new InvalidDataException("Public workflow must not instruct users to import through FIFA Editing Tool.");
				}
				if (ContainsControlText(main.m_TeamForm, "Club Relations") ||
					ContainsControlText(main.m_TeamForm, "Rev. Mod. Extensions") ||
					ContainsControlText(main.m_PlayerForm, "Career Details"))
					throw new InvalidDataException("A removed legacy/duplicate tab is still exposed.");
				if (!ContainsControlText(main.m_CountryForm, "Association Details") ||
					!ContainsControlText(main.m_LeagueForm, "League Details"))
					throw new InvalidDataException("A mapped, friendly CM26 details surface is missing.");
				if (ContainsControlText(main.m_LeagueForm, "Create Team Here") ||
					ContainsControlText(main.m_LeagueForm, "Make In-Game Ready"))
					throw new InvalidDataException("League still exposes obsolete multi-step creation buttons.");
				if (!ContainsControlText(main.m_TrophyForm, "Load FC26 Compdata") ||
					!ContainsControlText(main.m_TrophyForm, "Stage Compdata to Save"))
					throw new InvalidDataException("League-to-Compdata save integration is missing.");
				if (!ContainsControlText(main.m_PlayerForm, "Tactical Roles") ||
						!ContainsControlText(main.m_PlayerForm, "Composure") ||
						!ContainsControlText(main.m_PlayerForm, "Def. Awareness"))
					throw new InvalidDataException("The mapped FC26 Player Info/Skills controls are missing.");
				if (CountNamedControls(main.m_PlayerForm, "comboFc26Playstyle") != 34 ||
						!ContainsControlText(main.m_PlayerForm, "None / PlayStyle / PlayStyle+"))
					throw new InvalidDataException("The FC26 three-state PlayStyle editor is incomplete.");
				var tacticalRolesPage = FindControl(main.m_PlayerForm, "pageFc26TacticalRoles") as TabPage;
				if (tacticalRolesPage == null || tacticalRolesPage.Parent == null)
					throw new InvalidDataException("Tactical Roles must have a dedicated Player tab.");
				if (ContainsControlText(main.m_ManagerForm, "Manager Records") ||
					ContainsControlText(main.m_RefereeForm, "Competition Kits"))
					throw new InvalidDataException("A disconnected details popup is still exposed.");
				var miniface = FindControl(main.m_PlayerForm, "viewer2DPhoto");
				if (miniface == null || miniface.Width > 104 || miniface.Height > 129)
					throw new InvalidDataException("Player miniface exceeds the classic CM26 layout boundary.");
				var missingMappedSurfaces = new System.Collections.Generic.List<string>();
				if (ContainsControlText(main.m_TeamForm, "Career Money (Dollars)"))
					missingMappedSurfaces.Add("obsolete Career Money panel remains");
				foreach (var check in new[]
				{
					new { Form = (Form)main.m_TeamForm, Text = "Transfer Budget" },
					new { Form = (Form)main.m_TeamForm, Text = "Matchday Presentation" },
					new { Form = (Form)main.m_KitForm, Text = "Texture: checking" },
					new { Form = (Form)main.m_TrophyForm, Text = "Assign Teams" },
					new { Form = (Form)main.m_TrophyForm, Text = "Generate Schedule" },
					new { Form = (Form)main.m_TrophyForm, Text = "Career Ready Check" }
				})
					if (!ContainsControlText(check.Form, check.Text)) missingMappedSurfaces.Add(check.Text);
				if (missingMappedSurfaces.Count > 0)
					throw new InvalidDataException("Mapped FC26 surface check failed: " + string.Join(", ", missingMappedSurfaces));
				decimal decoBudget = TeamForm.CalculateDecoTransferBudget(162100, 6);
				if (Math.Abs(decoBudget - 17289023.40328413m) > 0.01m)
					throw new InvalidDataException("Deco Transfer Budget mapping has drifted: " +
						decoBudget.ToString("G29", System.Globalization.CultureInfo.InvariantCulture));
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
				Fc26FriendlyError.Show(null, "Startup", ex, "Close the dialog, reopen Creation Master 26 and use the diagnostic ID if the problem repeats.");
		}
	}

	private static void HandleUnhandled(Exception exception, bool diagnostic)
	{
		var log = Path.Combine(Path.GetTempPath(), "cm26-legacy-error.log");
		File.WriteAllText(log, (exception ?? new Exception("Unknown legacy UI error.")).ToString());
		if (diagnostic) Environment.Exit(1);
		Fc26FriendlyError.Show(null, "Unexpected error", exception ?? new Exception("Unknown error"),
			"The current action was stopped. If a Save was in progress, check Public Readiness > Open Recovery Folder before starting FC26.");
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

	private static int CountNamedControls(Control root, string prefix)
	{
		if (root == null) return 0;
		int count = root.Name?.StartsWith(prefix, StringComparison.Ordinal) == true ? 1 : 0;
		foreach (Control child in root.Controls) count += CountNamedControls(child, prefix);
		return count;
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
		if (root.Text?.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) return true;
		if (root is ToolStrip toolStrip && ContainsMenuText(toolStrip.Items, text)) return true;
		foreach (Control child in root.Controls)
			if (ContainsControlText(child, text)) return true;
		return false;
	}
}
