using System;
using System.Collections.Generic;

namespace FifaLibrary;

public class Language : Dictionary<int, string>
{
	public enum ETournamentStringType
	{
		Full,
		Abbr15
	}

	public enum ELeagueStringType
	{
		Full,
		Abbr15
	}

	public enum ECountryStringType
	{
		Full,
		Abbr3,
		Abbr15
	}

	public enum ETeamStringType
	{
		Full = 0,
		Abbr3 = 1,
		Abbr7 = 4,
		Abbr10 = 2,
		Abbr15 = 3
	}

	private Dictionary<int, string> m_Conventional;

	private Table m_LangTable;

	/// <summary>Creates an empty language catalogue for the FC26 bridge.</summary>
	public Language()
	{
		m_Conventional = new Dictionary<int, string>();
	}

	public Language(Table langTable)
	{
		m_LangTable = langTable;
		m_Conventional = new Dictionary<int, string>();
		Load(m_LangTable);
	}

	public void Load(Table langTable)
	{
		Clear();
		m_Conventional.Clear();
		for (int i = 0; i < langTable.NRecords; i++)
		{
			Record record = langTable.Records[i];
			int key = record.IntField[FI.language_hashid];
			if (!ContainsKey(key))
			{
				string value = record.CompressedString[FI.language_sourcetext];
				Add(key, value);
				value = record.CompressedString[FI.language_stringid];
				m_Conventional.Add(key, value);
			}
		}
	}

	public void Save(Table langTable)
	{
		langTable.ResizeRecords(base.Count);
		langTable.NValidRecords = base.Count;
		int num = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, string> current = enumerator.Current;
			Record record = langTable.Records[num];
			num++;
			record.IntField[FI.language_hashid] = current.Key;
			if (!m_Conventional.TryGetValue(current.Key, out var value))
			{
				value = string.Empty;
			}
			if (current.Value != null)
			{
				record.CompressedString[FI.language_sourcetext] = current.Value;
			}
			else
			{
				record.CompressedString[FI.language_sourcetext] = string.Empty;
			}
			record.CompressedString[FI.language_stringid] = value;
		}
	}

	public uint GetTournamentHash(int assetId, ETournamentStringType stringType)
	{
		return FifaUtil.ComputeLanguageHash(GetTournamentConventionalString(assetId, stringType));
	}

	public string GetTournamentConventionalString(int assetId, ETournamentStringType stringType)
	{
		string text;
		switch (stringType)
		{
		case ETournamentStringType.Full:
			text = "TrophyName_";
			break;
		case ETournamentStringType.Abbr15:
			text = "TrophyName_Abbr15_";
			break;
		default:
			return null;
		}
		return text + assetId;
	}

	public string GetTournamentString(int assetId, ETournamentStringType stringType)
	{
		string tournamentConventionalString = GetTournamentConventionalString(assetId, stringType);
		if (tournamentConventionalString == null)
		{
			return string.Empty;
		}
		return GetString(tournamentConventionalString);
	}

	public void SetTournamentString(int assetId, ETournamentStringType stringType, string name)
	{
		string tournamentConventionalString = GetTournamentConventionalString(assetId, stringType);
		if (tournamentConventionalString != null)
		{
			SetString(tournamentConventionalString, name);
		}
	}

	public void RemoveTournamentString(int assetId, ETournamentStringType stringType)
	{
		string tournamentConventionalString = GetTournamentConventionalString(assetId, stringType);
		if (tournamentConventionalString != null)
		{
			RemoveString(tournamentConventionalString);
		}
	}

	public uint GetFormationtHash(int formationFullNameId)
	{
		return FifaUtil.ComputeLanguageHash(GetFormationConventionalString(formationFullNameId));
	}

	public string GetFormationConventionalString(int formationFullNameId)
	{
		return "Formation_FullName_" + formationFullNameId;
	}

	public int GetFreeFormationFullNameId()
	{
		for (int i = 0; i < 31; i++)
		{
			if (GetFormationString(i) == null)
			{
				return i;
			}
		}
		return -1;
	}

	public int SearchFormationFullName(string fullName)
	{
		for (int i = 0; i < 31; i++)
		{
			if (GetFormationString(i) == fullName)
			{
				return i;
			}
		}
		return -1;
	}

	public string GetFormationString(int formationFullNameId)
	{
		if (formationFullNameId < 0)
		{
			return null;
		}
		string formationConventionalString = GetFormationConventionalString(formationFullNameId);
		if (formationConventionalString == null)
		{
			return string.Empty;
		}
		return GetString(formationConventionalString);
	}

	public void SetFormationString(int formationFullNameId, string name)
	{
		string formationConventionalString = GetFormationConventionalString(formationFullNameId);
		if (formationConventionalString != null)
		{
			SetString(formationConventionalString, name);
		}
	}

	public void RemoveFormationString(int assetId)
	{
		string formationConventionalString = GetFormationConventionalString(assetId);
		if (formationConventionalString != null)
		{
			RemoveString(formationConventionalString);
		}
	}

	public uint GetLeagueHash(int assetId, ELeagueStringType stringType)
	{
		return FifaUtil.ComputeLanguageHash(GetLeagueConventionalString(assetId, stringType));
	}

	private string GetLeagueConventionalString(int assetId, ELeagueStringType stringType)
	{
		string text;
		switch (stringType)
		{
		case ELeagueStringType.Full:
			text = "LeagueName_";
			break;
		case ELeagueStringType.Abbr15:
			text = "LeagueName_Abbr15_";
			break;
		default:
			return null;
		}
		return text + assetId;
	}

	public string GetLeagueString(int assetId, ELeagueStringType stringType)
	{
		string leagueConventionalString = GetLeagueConventionalString(assetId, stringType);
		if (leagueConventionalString == null)
		{
			return string.Empty;
		}
		return GetString(leagueConventionalString);
	}

	public void SetLeagueString(int assetId, ELeagueStringType stringType, string name)
	{
		string leagueConventionalString = GetLeagueConventionalString(assetId, stringType);
		if (leagueConventionalString != null)
		{
			SetString(leagueConventionalString, name);
		}
	}

	public void RemoveLeagueString(int assetId, ELeagueStringType stringType)
	{
		string leagueConventionalString = GetLeagueConventionalString(assetId, stringType);
		if (leagueConventionalString != null)
		{
			RemoveString(leagueConventionalString);
		}
	}

	public uint GetStadiumHash(int id)
	{
		return FifaUtil.ComputeLanguageHash(GetStadiumConventionalString(id));
	}

	private string GetStadiumConventionalString(int stadiumId)
	{
		return "StadiumName_" + stadiumId;
	}

	public string GetStadiumName(int stadiumId)
	{
		string stadiumConventionalString = GetStadiumConventionalString(stadiumId);
		return GetString(stadiumConventionalString);
	}

	public void SetStadiumName(int stadiumId, string stadiumName)
	{
		string stadiumConventionalString = GetStadiumConventionalString(stadiumId);
		SetString(stadiumConventionalString, stadiumName);
	}

	public void RemoveStadiumName(int stadiumId)
	{
		string stadiumConventionalString = GetStadiumConventionalString(stadiumId);
		RemoveString(stadiumConventionalString);
	}

	public uint GetBallHash(int id)
	{
		return FifaUtil.ComputeLanguageHash(GetBallConventionalString(id, isGeneric: true));
	}

	private string GetBallConventionalString(int ballId, bool isGeneric)
	{
		if (isGeneric)
		{
			return "ballname_" + ballId;
		}
		return "BallName_" + ballId;
	}

	public string GetBallName(int ballId, out bool isGeneric)
	{
		string ballConventionalString = GetBallConventionalString(ballId, isGeneric: true);
		int key = (int)FifaUtil.ComputeLanguageHash(ballConventionalString);
		string result = GetString(key);
		string conventionalString = GetConventionalString(key);
		isGeneric = conventionalString == ballConventionalString;
		return result;
	}

	public void SetBallName(int ballId, string ballName, bool isGeneric)
	{
		string ballConventionalString = GetBallConventionalString(ballId, isGeneric);
		SetString(ballConventionalString, ballName);
	}

	public void RemoveBallName(int ballId)
	{
		string ballConventionalString = GetBallConventionalString(ballId, isGeneric: true);
		RemoveString(ballConventionalString);
	}

	public uint GetShoesHash(int id)
	{
		return FifaUtil.ComputeLanguageHash(GetShoesConventionalString(id));
	}

	private string GetShoesConventionalString(int ShoesId)
	{
		return "CreatePlayerBoot_" + ShoesId;
	}

	public string GetShoesName(int ShoesId)
	{
		int key = (int)FifaUtil.ComputeLanguageHash(GetShoesConventionalString(ShoesId));
		return GetString(key);
	}

	public void SetShoesName(int ShoesId, string ShoesName)
	{
		string shoesConventionalString = GetShoesConventionalString(ShoesId);
		SetString(shoesConventionalString, ShoesName);
	}

	public void RemoveShoesName(int ShoesId)
	{
		string shoesConventionalString = GetShoesConventionalString(ShoesId);
		RemoveString(shoesConventionalString);
	}

	public uint GetCountryHash(int countryId, ECountryStringType stringType)
	{
		return FifaUtil.ComputeLanguageHash(GetCountryConventionalString(countryId, stringType));
	}

	public string GetCountryConventionalString(int countryId, ECountryStringType stringType)
	{
		return stringType switch
		{
			ECountryStringType.Full => "NationName_" + countryId, 
			ECountryStringType.Abbr15 => "NationName_" + countryId + "_abbr_15", 
			ECountryStringType.Abbr3 => "nationname_abbr3_" + countryId, 
			_ => null, 
		};
	}

	public string GetCountryString(int countryId, ECountryStringType stringType)
	{
		string countryConventionalString = GetCountryConventionalString(countryId, stringType);
		return GetString(countryConventionalString);
	}

	public void SetCountryString(int countryId, ECountryStringType stringType, string countryName)
	{
		string countryConventionalString = GetCountryConventionalString(countryId, stringType);
		SetString(countryConventionalString, countryName);
	}

	public void RemoveCountryStrings(int countryId)
	{
		string countryConventionalString = GetCountryConventionalString(countryId, ECountryStringType.Abbr15);
		RemoveString(countryConventionalString);
		countryConventionalString = GetCountryConventionalString(countryId, ECountryStringType.Abbr3);
		RemoveString(countryConventionalString);
		countryConventionalString = GetCountryConventionalString(countryId, ECountryStringType.Full);
		RemoveString(countryConventionalString);
	}

	public void RemoveCountryString(int countryId, ECountryStringType stringType)
	{
		string countryConventionalString = GetCountryConventionalString(countryId, stringType);
		RemoveString(countryConventionalString);
	}

	private string GetRoleLongConventionalString(int roleId)
	{
		return "SoccerFormationPosFull_" + roleId;
	}

	private string GetRoleShortConventionalString(int roleId)
	{
		string text = "SoccerFormationPos_Abbr4_";
		return roleId switch
		{
			0 => text + "GK", 
			1 => text + "SW", 
			2 => text + "RWB", 
			3 => text + "RB", 
			4 => text + "RCB", 
			5 => text + "CB", 
			6 => text + "LCB", 
			7 => text + "LB", 
			8 => text + "LWB", 
			9 => text + "RDM", 
			10 => text + "CDM", 
			11 => text + "LDM", 
			12 => text + "RM", 
			13 => text + "RCM", 
			14 => text + "CM", 
			15 => text + "LCM", 
			16 => text + "LM", 
			17 => text + "RAM", 
			18 => text + "CAM", 
			19 => text + "LAM", 
			20 => text + "RF", 
			21 => text + "CF", 
			22 => text + "LF", 
			23 => text + "RW", 
			24 => text + "RS", 
			25 => text + "ST", 
			26 => text + "LS", 
			27 => text + "LW", 
			_ => null, 
		};
	}

	public string GetRoleShortString(int roleId)
	{
		string fallback = roleId switch
		{
			0 => "GK", 1 => "SW", 2 => "RWB", 3 => "RB", 4 => "RCB", 5 => "CB",
			6 => "LCB", 7 => "LB", 8 => "LWB", 9 => "RDM", 10 => "CDM", 11 => "LDM",
			12 => "RM", 13 => "RCM", 14 => "CM", 15 => "LCM", 16 => "LM", 17 => "RAM",
			18 => "CAM", 19 => "LAM", 20 => "RF", 21 => "CF", 22 => "LF", 23 => "RW",
			24 => "RS", 25 => "ST", 26 => "LS", 27 => "LW", 28 => "SUB", 29 => "RES",
			_ => string.Empty
		};
		// FC26 snapshots do not carry FIFA 16 localization keys. Returning the
		// fixed CM16 abbreviations also prevents an internal SoccerFormationPos
		// key being displayed and truncated as "Soc..." in the roster grid.
		if (FifaEnvironment.Year == 26) return fallback;
		string conventional = GetRoleShortConventionalString(roleId);
		string localized = conventional == null ? null : GetString(conventional);
		if (!string.IsNullOrEmpty(localized) &&
			!string.Equals(localized, conventional, StringComparison.OrdinalIgnoreCase) &&
			!localized.StartsWith("SoccerFormationPos_", StringComparison.OrdinalIgnoreCase))
		{
			return localized;
		}
		// An FC26 bridge has no FIFA 16 language database.  Never expose the
		// internal Frostbite/legacy localization key in CM16 list columns.
		return fallback;
	}

	public void SetRoleShortString(int roleId, string roleShortName)
	{
		string roleShortConventionalString = GetRoleShortConventionalString(roleId);
		SetString(roleShortConventionalString, roleShortName);
	}

	public string GetRoleLongString(int roleId)
	{
		string fallback = roleId switch
		{
			0 => "Goalkeeper", 1 => "Sweeper", 2 => "Right Wing Back", 3 => "Right Back",
			4 => "Right Central Back", 5 => "Central Back", 6 => "Left Central Back", 7 => "Left Back",
			8 => "Left Wing Back", 9 => "Right Defensive Midfielder", 10 => "Central Defensive Midfielder",
			11 => "Left Defensive Midfielder", 12 => "Right Midfielder", 13 => "Right Central Midfielder",
			14 => "Central Midfielder", 15 => "Left Central Midfielder", 16 => "Left Midfielder",
			17 => "Right Attacking Midfielder", 18 => "Central Attacking Midfielder",
			19 => "Left Attacking Midfielder", 20 => "Right Forward", 21 => "Centre Forward",
			22 => "Left Forward", 23 => "Right Wing", 24 => "Right Striker", 25 => "Striker",
			26 => "Left Striker", 27 => "Left Wing", 28 => "Substitute", 29 => "Reserve", _ => string.Empty
		};
		if (FifaEnvironment.Year == 26) return fallback;
		string roleLongConventionalString = GetRoleLongConventionalString(roleId);
		string localized = GetString(roleLongConventionalString);
		if (!string.IsNullOrEmpty(localized) &&
			!string.Equals(localized, roleLongConventionalString, StringComparison.OrdinalIgnoreCase) &&
			!localized.StartsWith("SoccerFormationPosFull_", StringComparison.OrdinalIgnoreCase))
		{
			return localized;
		}
		return fallback;
	}

	public void SetRoleLongString(int roleId, string roleLongName)
	{
		string roleLongConventionalString = GetRoleLongConventionalString(roleId);
		SetString(roleLongConventionalString, roleLongName);
	}

	public uint GetSponsorDescriptionHash(int id)
	{
		return FifaUtil.ComputeLanguageHash(GetSponsorDescrConventionalString(id));
	}

	public uint GetSponsorNameHash(int id)
	{
		return FifaUtil.ComputeLanguageHash(GetSponsorNameConventionalString(id));
	}

	private string GetSponsorNameConventionalString(int sponsorId)
	{
		return "mm_Sponsor" + sponsorId;
	}

	private string GetSponsorDescrConventionalString(int sponsorId)
	{
		return "mm_SponsorBio" + sponsorId;
	}

	public string GetSponsorName(int sponsorId)
	{
		string sponsorNameConventionalString = GetSponsorNameConventionalString(sponsorId);
		return GetString(sponsorNameConventionalString);
	}

	public string GetSponsorDescription(int sponsorId)
	{
		string sponsorDescrConventionalString = GetSponsorDescrConventionalString(sponsorId);
		return GetString(sponsorDescrConventionalString);
	}

	public void SetSponsorName(int sponsorId, string sponsorName)
	{
		string sponsorNameConventionalString = GetSponsorNameConventionalString(sponsorId);
		SetString(sponsorNameConventionalString, sponsorName);
	}

	public void SetSponsorDescription(int sponsorId, string sponsorDesc)
	{
		string sponsorDescrConventionalString = GetSponsorDescrConventionalString(sponsorId);
		SetString(sponsorDescrConventionalString, sponsorDesc);
	}

	public void RemoveSponsorName(int sponsorId)
	{
		string sponsorNameConventionalString = GetSponsorNameConventionalString(sponsorId);
		RemoveString(sponsorNameConventionalString);
	}

	public void RemoveSponsorDescription(int sponsorId)
	{
		string sponsorDescrConventionalString = GetSponsorDescrConventionalString(sponsorId);
		RemoveString(sponsorDescrConventionalString);
	}

	public uint GetTeamHash(int teamId, ETeamStringType stringType)
	{
		return FifaUtil.ComputeLanguageHash(GetTeamConventionalString(teamId, stringType));
	}

	public string GetTeamConventionalString(int teamId, ETeamStringType stringType)
	{
		string text;
		switch (stringType)
		{
		case ETeamStringType.Full:
			text = "TeamName_";
			break;
		case ETeamStringType.Abbr10:
			text = "TeamName_Abbr10_";
			break;
		case ETeamStringType.Abbr15:
			text = "TeamName_Abbr15_";
			break;
		case ETeamStringType.Abbr3:
			text = "TeamName_Abbr3_";
			break;
		case ETeamStringType.Abbr7:
			text = "TeamName_Abbr7_";
			break;
		default:
			return null;
		}
		return text + teamId;
	}

	public string GetTeamString(int teamId, ETeamStringType stringType)
	{
		string teamConventionalString = GetTeamConventionalString(teamId, stringType);
		return GetString(teamConventionalString);
	}

	public void SetTeamString(int teamId, ETeamStringType stringType, string teamName)
	{
		string teamConventionalString = GetTeamConventionalString(teamId, stringType);
		SetString(teamConventionalString, teamName);
	}

	public void RemoveTeamStrings(int teamId)
	{
		string teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr10);
		RemoveString(teamConventionalString);
		teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr15);
		RemoveString(teamConventionalString);
		teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr3);
		RemoveString(teamConventionalString);
		teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr7);
		RemoveString(teamConventionalString);
		teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Full);
		RemoveString(teamConventionalString);
	}

	public void RemoveTeamString(int teamId, ETeamStringType stringType)
	{
		string teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr10);
		RemoveString(teamConventionalString);
	}

	public string GetString(int key)
	{
		if (TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public string GetConventionalString(int key)
	{
		if (m_Conventional.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public string GetString(string stringConventional)
	{
		int key = (int)FifaUtil.ComputeLanguageHash(stringConventional);
		if (TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public void SetString(string stringConventional, string stringValue)
	{
		if (stringValue != null && !(stringValue == string.Empty) && stringConventional != null && !(stringConventional == string.Empty))
		{
			int key = (int)FifaUtil.ComputeLanguageHash(stringConventional);
			SetString(key, stringConventional, stringValue);
		}
	}

	public void SetString(int key, string stringConventional, string stringValue)
	{
		if (ContainsKey(key))
		{
			Remove(key);
		}
		Add(key, stringValue);
		if (m_Conventional.ContainsKey(key))
		{
			m_Conventional.Remove(key);
		}
		m_Conventional.Add(key, stringConventional);
	}

	public void RemoveString(int key)
	{
		if (ContainsKey(key))
		{
			Remove(key);
		}
		if (m_Conventional.ContainsKey(key))
		{
			m_Conventional.Remove(key);
		}
	}

	public void RemoveString(string stringConventional)
	{
		int key = (int)FifaUtil.ComputeLanguageHash(stringConventional);
		RemoveString(key);
	}
}
