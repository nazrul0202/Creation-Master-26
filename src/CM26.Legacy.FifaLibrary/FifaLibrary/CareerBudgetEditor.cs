using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FifaLibrary;

/// <summary>
/// Reads and updates the manager's real FC26 Career budget. This data lives in
/// the Career save, not in the static teams table used by the squads editor.
/// </summary>
public sealed class CareerBudgetEditor
{
	private readonly CareerFile m_CareerFile;
	private readonly Record m_ManagerPreference;
	private readonly string m_XmlFileName;

	public string FileName => m_CareerFile.FileName;

	public string InGameName => m_CareerFile.InGameName;

	public int ClubTeamId { get; }

	public int TransferBudget => m_ManagerPreference.GetIntField("transferbudget");

	public int StartOfSeasonTransferBudget => m_ManagerPreference.GetIntField("startofseasontransferbudget");

	private CareerBudgetEditor(CareerFile careerFile, Record managerPreference, int clubTeamId, string xmlFileName)
	{
		m_CareerFile = careerFile;
		m_ManagerPreference = managerPreference;
		ClubTeamId = clubTeamId;
		m_XmlFileName = xmlFileName;
	}

	/// <summary>
	/// Returns likely FC26 Career saves in newest-first order. EA Career saves do
	/// not consistently use a file extension, so detection is based on the
	/// official settings folder and the Career filename prefix. CM26 backup files
	/// are deliberately excluded.
	/// </summary>
	public static IReadOnlyList<string> FindCareerSaveCandidates()
	{
		var settingsFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		if (!string.IsNullOrWhiteSpace(documents))
		{
			settingsFolders.Add(Path.Combine(documents, "EA SPORTS FC 26", "settings"));
			settingsFolders.Add(Path.Combine(documents, "Electronic Arts", "EA SPORTS FC 26", "settings"));
		}

		var candidates = new List<FileInfo>();
		foreach (string folder in settingsFolders)
		{
			if (!Directory.Exists(folder)) continue;
			try
			{
				candidates.AddRange(new DirectoryInfo(folder).EnumerateFiles("Career*", SearchOption.TopDirectoryOnly)
					.Where(file => file.Length > 0
						&& file.Name.IndexOf(".cm26_", StringComparison.OrdinalIgnoreCase) < 0
						&& !file.Name.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)));
			}
			catch (UnauthorizedAccessException)
			{
				// A redirected/locked Documents folder must not prevent manual loading.
			}
		}

		return candidates
			.GroupBy(file => file.FullName, StringComparer.OrdinalIgnoreCase)
			.Select(group => group.First())
			.OrderByDescending(file => file.LastWriteTimeUtc)
			.Select(file => file.FullName)
			.ToArray();
	}

	public static CareerBudgetEditor Open(string fileName, string xmlFileName)
	{
		if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
		{
			throw new FileNotFoundException("Career save was not found.", fileName);
		}
		if (string.IsNullOrWhiteSpace(xmlFileName) || !File.Exists(xmlFileName))
		{
			throw new FileNotFoundException("FC26 database schema was not found.", xmlFileName);
		}

		CareerFile careerFile = new CareerFile(fileName, xmlFileName);
		if (careerFile.NDatabases < 1 || careerFile.Databases[0] == null)
		{
			throw new InvalidDataException("The selected file does not contain an FC26 Career database.");
		}

		DbFile database = careerFile.Databases[0];
		Table users = GetRequiredTable(database, "career_users");
		Table preferences = GetRequiredTable(database, "career_managerpref");
		EnsureField(users, "clubteamid");
		EnsureField(preferences, "transferbudget");
		EnsureField(preferences, "startofseasontransferbudget");

		Record user = FindUserRecord(users);
		Record managerPreference = FindClubPreferenceRecord(preferences);
		return new CareerBudgetEditor(careerFile, managerPreference, user.GetIntField("clubteamid"), xmlFileName);
	}

	public string Save(int transferBudget, int startOfSeasonTransferBudget)
	{
		if (transferBudget < 0 || startOfSeasonTransferBudget < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(transferBudget), "Transfer budgets cannot be negative.");
		}

		string backupFile = FileName + ".cm26_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".bak";
		File.Copy(FileName, backupFile, overwrite: false);

		int originalTransferBudget = TransferBudget;
		int originalStartOfSeasonBudget = StartOfSeasonTransferBudget;
		try
		{
			m_ManagerPreference.SetField("transferbudget", transferBudget);
			m_ManagerPreference.SetField("startofseasontransferbudget", startOfSeasonTransferBudget);
			if (!m_CareerFile.SaveEa(FileName))
			{
				throw new IOException("Creation Master could not write the Career save.");
			}

			// Do not report a successful Career write until the EA container can be
			// reopened and the two exact fields contain the requested values.  A
			// failed verification is rolled back from the backup immediately.
			CareerBudgetEditor verified = Open(FileName, m_XmlFileName);
			if (verified.ClubTeamId != ClubTeamId || verified.TransferBudget != transferBudget ||
				verified.StartOfSeasonTransferBudget != startOfSeasonTransferBudget)
			{
				throw new InvalidDataException("Career save verification failed: the reopened budget values do not match the requested values.");
			}
		}
		catch (Exception saveError)
		{
			// Restore the original container as well as this editor instance. The
			// backup remains available as an additional recovery point.
			m_ManagerPreference.SetField("transferbudget", originalTransferBudget);
			m_ManagerPreference.SetField("startofseasontransferbudget", originalStartOfSeasonBudget);
			try
			{
				File.Copy(backupFile, FileName, overwrite: true);
			}
			catch (Exception restoreError)
			{
				throw new IOException("Career save verification failed and CM26 could not restore the original automatically. Restore this backup manually: " + backupFile,
					new AggregateException(saveError, restoreError));
			}
			throw;
		}
		return backupFile;
	}

	private static Table GetRequiredTable(DbFile database, string tableName)
	{
		int tableIndex = database.GetTableIndex(tableName);
		if (tableIndex < 0)
		{
			throw new InvalidDataException("Required Career table is missing: " + tableName);
		}
		Table table = database.Table[tableIndex];
		if (table.Records == null || table.Records.Length == 0)
		{
			throw new InvalidDataException("Career table has no records: " + tableName);
		}
		return table;
	}

	private static void EnsureField(Table table, string fieldName)
	{
		if (table.TableDescriptor.GetFieldIndex(fieldName) < 0)
		{
			throw new InvalidDataException("Required Career field is missing: " + fieldName);
		}
	}

	private static Record FindUserRecord(Table users)
	{
		foreach (Record record in users.Records)
		{
			if (record != null && record.GetIntField("clubteamid") > 0)
			{
				return record;
			}
		}
		throw new InvalidDataException("The Career save does not contain an active club team.");
	}

	private static Record FindClubPreferenceRecord(Table preferences)
	{
		int preferenceIdIndex = preferences.TableDescriptor.GetFieldIndex("managerprefid");
		if (preferenceIdIndex >= 0)
		{
			foreach (Record record in preferences.Records)
			{
				if (record != null && record.IntField[preferenceIdIndex] == 0)
				{
					return record;
				}
			}
		}
		return preferences.Records[0];
	}
}
