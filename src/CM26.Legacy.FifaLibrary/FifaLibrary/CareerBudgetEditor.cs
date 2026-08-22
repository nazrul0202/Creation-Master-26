using System;
using System.IO;

namespace FifaLibrary;

/// <summary>
/// Reads and updates the manager's real FC26 Career budget. This data lives in
/// the Career save, not in the static teams table used by the squads editor.
/// </summary>
public sealed class CareerBudgetEditor
{
	private readonly CareerFile m_CareerFile;
	private readonly Record m_ManagerPreference;

	public string FileName => m_CareerFile.FileName;

	public string InGameName => m_CareerFile.InGameName;

	public int ClubTeamId { get; }

	public int TransferBudget => m_ManagerPreference.GetIntField("transferbudget");

	public int StartOfSeasonTransferBudget => m_ManagerPreference.GetIntField("startofseasontransferbudget");

	private CareerBudgetEditor(CareerFile careerFile, Record managerPreference, int clubTeamId)
	{
		m_CareerFile = careerFile;
		m_ManagerPreference = managerPreference;
		ClubTeamId = clubTeamId;
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
		return new CareerBudgetEditor(careerFile, managerPreference, user.GetIntField("clubteamid"));
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
		}
		catch
		{
			// Keep this editor instance consistent with the untouched/backup save when
			// the EA container writer rejects the operation.
			m_ManagerPreference.SetField("transferbudget", originalTransferBudget);
			m_ManagerPreference.SetField("startofseasontransferbudget", originalStartOfSeasonBudget);
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
