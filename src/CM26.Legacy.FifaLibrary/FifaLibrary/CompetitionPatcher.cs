using System;
using System.IO;

namespace FifaLibrary;

public class CompetitionPatcher
{
	private FileStream m_fs;

	private BinaryReader m_br;

	private BinaryWriter m_bw;

	private long m_InitialPosition;

	private long m_UserTeamsPosition;

	private long m_AdvancementPosition;

	private long m_CompidsPosition;

	private long m_CompobjPosition;

	private long m_InitteamsjPosition;

	private long m_GamesPosition;

	private long m_SchedulePosition;

	private long m_TasksPosition;

	private long m_SettingsPosition;

	private long m_CompidsDatesPosition;

	private long m_StandingTablePosition;

	private long m_PlayerStatsPosition;

	private long m_WeatherPosition;

	private long m_CardsPosition;

	private long m_TeamsPosition;

	private long m_FatiguePosition;

	private Table m_Career_Users;

	private Table m_Career_Calendar;

	private Table m_Career_ManagerPref;

	private CareerFile m_FileToPatch;

	private int m_DummyUserTeamId;

	private int m_DummyMyCompetitionId;

	private int m_DummyMyStageId;

	private int m_DummyMyGroupId;

	private int m_DummyMaxCompetitionId;

	private int[] m_DummyTeamIds;

	private int[] m_DummyTeamKeys;

	private int[] m_DummyStandingKeys;

	private int m_DummyTodayDate;

	private int m_nTeams;

	private int m_DummyCountryObjid;

	private string m_DummyCompetitionNameString;

	private string m_DummyCountryNameString;

	private int m_UserTeamId;

	private int m_OpponentTeamId;

	private int[] m_TeamIds;

	private int m_CompetitionAsset;

	private int m_FirstLegHomeGoals;

	private int m_FirstLegAwayGoals;

	private int m_GameTime;

	private int m_GameDate;

	private int m_StadiumId;

	private string m_StadiumName;

	private int[][] m_CompetitionStats;

	private int[][] m_PlayerStats;

	private int m_Weather;

	private int m_SunsetTime;

	private int m_NightTime;

	private Referee m_Referee;

	private int m_PendingCards;

	private int m_Fatigue;

	private int m_Injuries;

	public CareerFile FileToPatch
	{
		get
		{
			return m_FileToPatch;
		}
		set
		{
			m_FileToPatch = value;
		}
	}

	public int DummyUserTeamId
	{
		get
		{
			return m_DummyUserTeamId;
		}
		set
		{
			m_DummyUserTeamId = value;
		}
	}

	public int UserTeamId
	{
		get
		{
			return m_UserTeamId;
		}
		set
		{
			m_UserTeamId = value;
		}
	}

	public int OpponentTeamId
	{
		get
		{
			return m_OpponentTeamId;
		}
		set
		{
			m_OpponentTeamId = value;
		}
	}

	public int[] TeamIds
	{
		get
		{
			return m_TeamIds;
		}
		set
		{
			m_TeamIds = value;
		}
	}

	public int CompetitionAsset
	{
		get
		{
			return m_CompetitionAsset;
		}
		set
		{
			m_CompetitionAsset = value;
		}
	}

	public int FirstLegHomeGoals
	{
		get
		{
			return m_FirstLegHomeGoals;
		}
		set
		{
			m_FirstLegHomeGoals = value;
		}
	}

	public int FirstLegAwayGoals
	{
		get
		{
			return m_FirstLegAwayGoals;
		}
		set
		{
			m_FirstLegAwayGoals = value;
		}
	}

	public int StadiumId
	{
		get
		{
			return m_StadiumId;
		}
		set
		{
			m_StadiumId = value;
		}
	}

	public string StadiumName
	{
		get
		{
			return m_StadiumName;
		}
		set
		{
			m_StadiumName = value;
		}
	}

	public int[][] CompetitionStats
	{
		get
		{
			return m_CompetitionStats;
		}
		set
		{
			m_CompetitionStats = value;
		}
	}

	public int[][] PlayerStats
	{
		get
		{
			return m_PlayerStats;
		}
		set
		{
			m_PlayerStats = value;
		}
	}

	public int Weather
	{
		get
		{
			return m_Weather;
		}
		set
		{
			m_Weather = value;
		}
	}

	public int SunsetTime
	{
		get
		{
			return m_SunsetTime;
		}
		set
		{
			m_SunsetTime = value;
		}
	}

	public int NightTime
	{
		get
		{
			return m_NightTime;
		}
		set
		{
			m_NightTime = value;
		}
	}

	public Referee Referee
	{
		get
		{
			return m_Referee;
		}
		set
		{
			m_Referee = value;
		}
	}

	public int PendingCards
	{
		get
		{
			return m_PendingCards;
		}
		set
		{
			m_PendingCards = value;
		}
	}

	public bool Initialize(string fileNameToPatch)
	{
		CareerFile fileToPatch = new CareerFile(fileNameToPatch, FifaEnvironment.FifaXmlFileName);
		return Initialize(fileToPatch);
	}

	public bool Initialize(CareerFile fileToPatch)
	{
		if (fileToPatch == null)
		{
			return false;
		}
		m_FileToPatch = fileToPatch;
		int tableIndex = FileToPatch.Databases[0].GetTableIndex("career_users");
		if (tableIndex < 0)
		{
			return false;
		}
		m_Career_Users = FileToPatch.Databases[0].Table[tableIndex];
		if (m_Career_Users == null)
		{
			return false;
		}
		if (m_Career_Users.Records.Length < 1)
		{
			return false;
		}
		int fieldIndex = m_Career_Users.TableDescriptor.GetFieldIndex("clubteamid");
		m_DummyUserTeamId = m_Career_Users.Records[0].IntField[fieldIndex];
		fieldIndex = m_Career_Users.TableDescriptor.GetFieldIndex("primarycompobjid");
		m_DummyMyCompetitionId = m_Career_Users.Records[0].IntField[fieldIndex];
		tableIndex = FileToPatch.Databases[0].GetTableIndex("career_calendar");
		if (tableIndex < 0)
		{
			return false;
		}
		m_Career_Calendar = FileToPatch.Databases[0].Table[tableIndex];
		if (m_Career_Calendar == null)
		{
			return false;
		}
		if (m_Career_Calendar.Records.Length < 1)
		{
			return false;
		}
		fieldIndex = m_Career_Calendar.TableDescriptor.GetFieldIndex("currdate");
		m_DummyTodayDate = m_Career_Calendar.Records[0].IntField[fieldIndex];
		m_fs = new FileStream(m_FileToPatch.FileName, FileMode.Open, FileAccess.ReadWrite);
		m_br = new BinaryReader(m_fs);
		m_bw = new BinaryWriter(m_fs);
		m_fs.Position = m_FileToPatch.Databases[2].SignaturePosition;
		m_InitialPosition = SearchSignature("em001");
		if (m_InitialPosition < 0)
		{
			return false;
		}
		m_UserTeamsPosition = m_InitialPosition + 786;
		m_AdvancementPosition = m_UserTeamsPosition + 512;
		m_CompidsPosition = m_AdvancementPosition + 36000;
		m_CompobjPosition = m_CompidsPosition + 1000;
		m_InitteamsjPosition = m_CompobjPosition + 94000;
		m_GamesPosition = m_InitteamsjPosition + 5000;
		m_SchedulePosition = m_GamesPosition + 84000;
		m_TasksPosition = m_SchedulePosition + 84000;
		m_SettingsPosition = m_TasksPosition + 16000;
		m_CompidsDatesPosition = m_SettingsPosition + 40000;
		m_StandingTablePosition = m_CompidsDatesPosition + 1400;
		m_PlayerStatsPosition = m_StandingTablePosition + 111300;
		m_WeatherPosition = m_PlayerStatsPosition + 336000;
		m_CardsPosition = m_WeatherPosition + 6270 + 6;
		m_TeamsPosition = m_CardsPosition + 128032 + 652;
		m_FatiguePosition = m_TeamsPosition + 4 + 1024 + 6;
		m_br.BaseStream.Position = m_CompidsPosition;
		for (int i = 0; i < 100; i++)
		{
			m_br.ReadInt16();
			m_br.ReadByte();
			short num = m_br.ReadInt16();
			m_br.ReadInt32();
			m_br.ReadByte();
			if (num == m_DummyMyCompetitionId)
			{
				m_br.BaseStream.Position += 3L;
				m_DummyMaxCompetitionId = m_br.ReadInt16();
				break;
			}
		}
		m_br.BaseStream.Position = m_TeamsPosition;
		m_nTeams = m_br.ReadInt32();
		m_DummyTeamIds = new int[m_nTeams];
		m_DummyTeamKeys = new int[m_nTeams];
		for (int j = 0; j < m_nTeams; j++)
		{
			int num2 = m_br.ReadInt32();
			int num3 = m_br.ReadInt32();
			m_DummyTeamKeys[j] = num2;
			m_DummyTeamIds[j] = num3;
		}
		m_br.BaseStream.Position = m_CompobjPosition;
		m_DummyCountryObjid = 0;
		for (int k = 0; k < 2000; k++)
		{
			m_br.ReadInt16();
			m_br.ReadByte();
			int num4 = m_br.ReadInt16();
			byte num5 = m_br.ReadByte();
			FifaUtil.ReadNullPaddedString(m_br, 6);
			string text = FifaUtil.ReadNullPaddedString(m_br, 33);
			m_br.ReadInt16();
			if (num5 == 2)
			{
				m_DummyCountryObjid = num4;
				m_DummyCountryNameString = text;
			}
			if (num4 == m_DummyMyCompetitionId)
			{
				m_DummyCompetitionNameString = text;
				break;
			}
		}
		m_DummyStandingKeys = new int[m_nTeams];
		m_br.BaseStream.Position = m_StandingTablePosition;
		for (int l = 0; l < 5300; l++)
		{
			int num6 = m_br.ReadInt16();
			m_br.ReadByte();
			int num7 = m_br.ReadInt16();
			int num8 = m_br.ReadInt32();
			m_br.ReadByte();
			m_br.BaseStream.Position += 11L;
			if (num7 >= m_DummyMyCompetitionId && num7 < m_DummyMaxCompetitionId && num8 != -1)
			{
				if (num8 == m_DummyUserTeamId)
				{
					m_DummyMyStageId = num7 - 1;
					m_DummyMyGroupId = num7;
				}
				for (int m = 0; m < m_nTeams; m++)
				{
					if (num8 == m_DummyTeamIds[m])
					{
						m_DummyStandingKeys[m] = num6;
						break;
					}
				}
			}
			if (num7 >= m_DummyMaxCompetitionId)
			{
				break;
			}
		}
		return true;
	}

	public bool Save()
	{
		m_br.Close();
		m_bw.Close();
		m_fs.Close();
		m_FileToPatch.SaveEa(m_FileToPatch.FileName);
		return true;
	}

	public bool Close()
	{
		m_br.Close();
		m_bw.Close();
		m_fs.Close();
		return true;
	}

	public void SetInGameName(string inGameName)
	{
		m_FileToPatch.InGameName = inGameName;
	}

	private long SearchSignature(string signature)
	{
		int num = 0;
		int length = signature.Length;
		char[] array = signature.ToCharArray();
		long position = m_br.BaseStream.Position;
		do
		{
			if (m_br.ReadByte() == array[num])
			{
				num++;
				if (num == length)
				{
					m_br.BaseStream.Position -= length;
					return m_br.BaseStream.Position;
				}
			}
			else
			{
				num = 0;
			}
		}
		while (m_br.BaseStream.Position < m_br.BaseStream.Length);
		m_br.BaseStream.Position = position;
		return -1L;
	}

	public bool SetStandings(int nTeams, int[][] standingsArray)
	{
		m_br.BaseStream.Position = m_StandingTablePosition;
		StandingTableEntry standingTableEntry = null;
		standingTableEntry = SearchStandingsGroup();
		if (standingTableEntry != null)
		{
			PatchStandingsGroup(standingTableEntry, nTeams, standingsArray);
			CleanUnusedStandings(standingTableEntry.compId);
			return true;
		}
		return false;
	}

	public bool SetStandingsCup(int nTeams, int[][] standingsArray)
	{
		m_br.BaseStream.Position = m_StandingTablePosition;
		StandingTableEntry standingTableEntry = null;
		bool flag = true;
		do
		{
			standingTableEntry = SearchStandingsGroup();
			if (standingTableEntry != null)
			{
				if (flag)
				{
					PatchStandingsTeam(standingTableEntry, nTeams, standingsArray);
					flag = false;
				}
				else if (standingTableEntry.teamId != -1)
				{
					PatchStandingsGroup(standingTableEntry, nTeams, standingsArray);
				}
				else
				{
					m_br.BaseStream.Position += StandingTableEntry.TotalLength();
				}
			}
		}
		while (standingTableEntry != null);
		return true;
	}

	private StandingTableEntry SearchStandingsGroup()
	{
		for (int i = 0; i < 5300; i++)
		{
			StandingTableEntry standingTableEntry = StandingTableEntry.Read(m_br);
			if (standingTableEntry == null)
			{
				return null;
			}
			if (standingTableEntry.compId == m_DummyMyGroupId)
			{
				m_br.BaseStream.Position -= StandingTableEntry.TotalLength();
				_ = standingTableEntry.index1;
				return standingTableEntry;
			}
		}
		return null;
	}

	private void PatchStandingsGroup(StandingTableEntry firstEntry, int nTeams, int[][] standingsArray)
	{
		_ = firstEntry.index1;
		_ = firstEntry.compId;
		for (int i = 0; i < nTeams; i++)
		{
			m_br.ReadInt16();
			m_bw.Write((byte)1);
			m_br.ReadInt16();
			if (standingsArray[i] != null)
			{
				m_bw.Write(standingsArray[i][0]);
				m_bw.Write((byte)(standingsArray[i][1] - 1));
				m_bw.Write((byte)standingsArray[i][2]);
				m_bw.Write((byte)standingsArray[i][3]);
				m_bw.Write((byte)standingsArray[i][4]);
				m_bw.Write((byte)standingsArray[i][5]);
				m_bw.Write((byte)standingsArray[i][6]);
				m_bw.Write((byte)standingsArray[i][7]);
				m_bw.Write((byte)standingsArray[i][8]);
				m_bw.Write((byte)standingsArray[i][9]);
				m_bw.Write((byte)standingsArray[i][10]);
				m_bw.Write((byte)standingsArray[i][11]);
				m_bw.Write((byte)standingsArray[i][12]);
			}
			else
			{
				m_bw.Write(0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
				m_bw.Write((byte)0);
			}
		}
	}

	private void PatchStandingsTeam(StandingTableEntry firstEntry, int nTeams, int[][] standingsArray)
	{
		_ = firstEntry.index1;
		int compId = firstEntry.compId;
		for (int i = 0; i < nTeams; i++)
		{
			m_bw.BaseStream.Position += 5L;
			m_bw.Write(standingsArray[i][0]);
			m_bw.BaseStream.Position += 12L;
		}
		CleanUnusedStandings(compId);
	}

	private void CleanUnusedStandings(int compid)
	{
		for (int i = 0; i < 5300; i++)
		{
			StandingTableEntry standingTableEntry = StandingTableEntry.Read(m_br);
			m_bw.BaseStream.Position -= StandingTableEntry.TotalLength();
			if (standingTableEntry != null && standingTableEntry.compId == compid)
			{
				standingTableEntry.Clean();
				standingTableEntry.Write(m_bw);
				continue;
			}
			break;
		}
	}

	public void SetDatabaseReferee(RefereeList referees)
	{
		referees.SaveEA(m_FileToPatch.Databases[1]);
	}

	public void SetDatabasePlayers(PlayerList players)
	{
		m_FileToPatch.Databases[0].GetTable("career_playerlastmatchhistory").SetValidRecordAndClean(0);
		players.SaveEA(m_FileToPatch.Databases[1]);
	}

	public void SetDatabaseTeams(TeamList teams)
	{
		teams.SaveEA(m_FileToPatch.Databases[1]);
		foreach (Team team in teams)
		{
			FifaEnvironment.Language.SetTeamString(team.Id, Language.ETeamStringType.Abbr15, team.TeamNameAbbr15);
			FifaEnvironment.Language.SetTeamString(team.Id, Language.ETeamStringType.Abbr3, team.TeamNameAbbr3);
		}
	}

	public void SetDatabaseKits(KitList kits)
	{
		Table table = FifaEnvironment.FifaDb.Table[TI.teamkits];
		table.ResizeRecords(kits.Count);
		for (int i = 0; i < kits.Count; i++)
		{
			Kit kit = (Kit)kits[i];
			kit.SaveKit(table.Records[i], kit.Id);
		}
	}

	public bool SetPreferences(int difficultyLevel, int halfTimeDuration)
	{
		int tableIndex = m_FileToPatch.Databases[0].GetTableIndex("career_managerpref");
		if (tableIndex < 0)
		{
			return false;
		}
		Table obj = FileToPatch.Databases[0].Table[tableIndex];
		int fieldIndex = obj.TableDescriptor.GetFieldIndex("halflength");
		obj.Records[0].IntField[fieldIndex] = halfTimeDuration;
		fieldIndex = obj.TableDescriptor.GetFieldIndex("matchdifficulty");
		obj.Records[0].IntField[fieldIndex] = difficultyLevel;
		return true;
	}

	public bool SetTeams(int[] teamIds)
	{
		int fieldIndex = m_Career_Users.TableDescriptor.GetFieldIndex("clubteamid");
		m_Career_Users.Records[0].IntField[fieldIndex] = teamIds[0];
		for (int i = 0; i < m_Career_Users.Records[0].IntField.Length; i++)
		{
			m_Career_Users.Records[1].IntField[i] = m_Career_Users.Records[0].IntField[i];
		}
		m_Career_Users.Records[1].IntField[fieldIndex] = teamIds[1];
		fieldIndex = m_Career_Users.TableDescriptor.GetFieldIndex("userid");
		m_Career_Users.Records[1].IntField[fieldIndex] = 1;
		m_UserTeamId = teamIds[0];
		m_OpponentTeamId = teamIds[1];
		if (m_DummyTeamIds[0] != m_DummyUserTeamId)
		{
			if (m_DummyTeamIds[1] != m_DummyUserTeamId)
			{
				return false;
			}
			int num = teamIds[0];
			teamIds[0] = teamIds[1];
			teamIds[1] = num;
		}
		m_br.BaseStream.Position = m_UserTeamsPosition + 4;
		m_bw.Write(m_UserTeamId);
		m_bw.BaseStream.Position += 4L;
		m_bw.Write(m_OpponentTeamId);
		m_br.BaseStream.Position = m_TeamsPosition;
		m_bw.Write(teamIds.Length);
		m_br.ReadInt32();
		m_br.BaseStream.Position -= 4L;
		for (int j = 0; j < teamIds.Length; j++)
		{
			m_br.ReadInt32();
			m_bw.Write(teamIds[j]);
		}
		for (int k = teamIds.Length; k < m_nTeams; k++)
		{
			m_bw.Write(-1);
			m_bw.Write(-1);
		}
		m_br.BaseStream.Position = m_TasksPosition;
		for (int l = 0; l < 800; l++)
		{
			m_br.ReadInt16();
			int num2 = m_br.ReadByte();
			short num3 = m_br.ReadInt16();
			int num4 = m_br.ReadByte();
			int num5 = m_br.ReadInt16();
			int num6 = m_br.ReadInt32();
			m_br.ReadInt32();
			m_br.ReadInt32();
			if (num3 == m_DummyMyCompetitionId && num5 == m_DummyMyGroupId && num4 == 2)
			{
				if (num6 > teamIds.Length)
				{
					m_br.BaseStream.Position -= 18L;
					m_bw.Write((byte)0);
					m_bw.Write((short)0);
					m_bw.Write((byte)0);
					m_bw.Write((short)0);
					m_bw.Write(0);
					m_bw.Write(0);
					m_bw.Write(0);
				}
				if (num2 == 0)
				{
					break;
				}
			}
		}
		return false;
	}

	public bool SetStadium(int stadiumId)
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			m_br.ReadByte();
			short num = m_br.ReadInt16();
			int num2 = m_br.ReadByte();
			m_br.ReadUInt32();
			if (num == m_DummyMyStageId && num2 == 15)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(stadiumId);
				result = true;
				break;
			}
		}
		return result;
	}

	public bool CleanStadium()
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			m_br.ReadByte();
			short num = m_br.ReadInt16();
			int num2 = m_br.ReadByte();
			m_br.ReadUInt32();
			if (num == m_DummyMyStageId && num2 == 15)
			{
				m_br.BaseStream.Position -= 8L;
				m_bw.Write((byte)0);
				m_bw.Write((short)(-1));
				m_bw.Write((byte)0);
				m_bw.Write(0);
				result = true;
				break;
			}
		}
		return result;
	}

	public bool SetStageType(int stageTypeId)
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			int num = m_br.ReadByte();
			short num2 = m_br.ReadInt16();
			int num3 = m_br.ReadByte();
			m_br.ReadUInt32();
			if (num2 == m_DummyMyStageId && num3 == 25)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(stageTypeId);
				result = true;
				break;
			}
			if (num == 0)
			{
				break;
			}
		}
		return result;
	}

	public bool SetStageMode(int stageModeId)
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			int num = m_br.ReadByte();
			short num2 = m_br.ReadInt16();
			int num3 = m_br.ReadByte();
			m_br.ReadUInt32();
			if (num2 == m_DummyMyStageId && num3 == 14)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(stageModeId);
				result = true;
				break;
			}
			if (num == 0)
			{
				break;
			}
		}
		return result;
	}

	public bool SetTieRule2Legs(bool useAway, bool useET, bool usePens)
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			int num = m_br.ReadByte();
			short num2 = m_br.ReadInt16();
			int num3 = m_br.ReadByte();
			uint num4 = m_br.ReadUInt32();
			if (num2 == 0 && num3 == 19)
			{
				result = true;
				m_br.BaseStream.Position -= 4L;
				switch (num4)
				{
				case 4u:
					m_bw.Write(useAway ? 4 : 0);
					break;
				case 8u:
					m_bw.Write(useET ? 8 : 0);
					break;
				case 16u:
					m_bw.Write((useAway && useET) ? 16 : 0);
					break;
				case 32u:
					m_bw.Write(usePens ? 32 : 0);
					break;
				default:
					m_bw.Write(num4);
					break;
				}
			}
			if (num == 0)
			{
				break;
			}
		}
		return result;
	}

	public bool SetTieRule1Leg(bool useET, bool usePens)
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			m_br.ReadByte();
			short num = m_br.ReadInt16();
			int num2 = m_br.ReadByte();
			uint num3 = m_br.ReadUInt32();
			if (num == 0 && num2 == 17)
			{
				result = true;
				m_br.BaseStream.Position -= 4L;
				switch (num3)
				{
				case 8u:
					m_bw.Write(useET ? 8 : 0);
					break;
				case 32u:
					m_bw.Write(usePens ? 32 : 0);
					break;
				default:
					m_bw.Write(num3);
					break;
				}
			}
		}
		return result;
	}

	public bool SetStageInfo(int stageModeId, int stageTypeId, int nGames)
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			byte num = m_br.ReadByte();
			int num2 = m_br.ReadInt16();
			int num3 = m_br.ReadByte();
			m_br.ReadUInt32();
			if ((num2 == 0 || num2 == m_DummyMyStageId) && num3 == 3)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(5);
				result = true;
			}
			if (num2 == m_DummyMyStageId && num3 == 25)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(stageTypeId);
				result = true;
			}
			if (num2 == m_DummyMyStageId && num3 == 14)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(stageModeId);
				result = true;
			}
			if (num2 == m_DummyMyStageId && num3 == 64)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(nGames);
				result = true;
			}
			if (num == 0)
			{
				break;
			}
		}
		return result;
	}

	public bool SetCompetitionAsset(int assetId)
	{
		bool result = false;
		m_br.BaseStream.Position = m_CompobjPosition;
		for (int i = 0; i < 2000; i++)
		{
			m_br.ReadInt16();
			byte num = m_br.ReadByte();
			short num2 = m_br.ReadInt16();
			m_br.ReadByte();
			string text = FifaUtil.ReadNullPaddedString(m_br, 6);
			string text2 = FifaUtil.ReadNullPaddedString(m_br, 33);
			m_br.ReadInt16();
			if (num2 == m_DummyMyCompetitionId)
			{
				m_br.BaseStream.Position -= 41L;
				FifaUtil.WriteNullPaddedString(str: "C" + assetId, w: m_bw, length: 6);
				FifaUtil.WriteNullPaddedString(str: "TrophyName_Abbr15_" + assetId, w: m_bw, length: 33);
				m_br.BaseStream.Position += 2L;
				result = true;
			}
			if (num == 0)
			{
				break;
			}
		}
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int j = 0; j < 4000; j++)
		{
			m_br.ReadInt16();
			m_br.ReadByte();
			short num3 = m_br.ReadInt16();
			int num4 = m_br.ReadByte();
			m_br.ReadUInt32();
			if (num3 == m_DummyMyCompetitionId && num4 == 76)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(assetId);
				result = true;
				break;
			}
		}
		m_br.BaseStream.Position = m_TasksPosition;
		for (int k = 0; k < 800; k++)
		{
			m_br.ReadInt16();
			m_br.ReadByte();
			int num5 = m_br.ReadInt16();
			int num6 = m_br.ReadByte();
			m_br.ReadInt16();
			m_br.ReadInt32();
			m_br.ReadInt32();
			m_br.ReadInt32();
			if (num5 >= m_DummyMyCompetitionId && num5 < m_DummyMaxCompetitionId && (num6 == 8 || num6 == 10))
			{
				m_br.BaseStream.Position -= 12L;
				m_bw.Write(assetId);
				m_br.BaseStream.Position += 8L;
				result = true;
			}
		}
		return result;
	}

	public bool SetCompetitionType(int competitionType)
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			int num = m_br.ReadByte();
			short num2 = m_br.ReadInt16();
			int num3 = m_br.ReadByte();
			m_br.ReadUInt32();
			if (num2 == m_DummyMyCompetitionId && num3 == 81)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(competitionType);
				result = true;
				break;
			}
			if (num == 0)
			{
				break;
			}
		}
		return result;
	}

	public bool SetBenchPlayers(int nBenchPlayers)
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			int num = m_br.ReadByte();
			short num2 = m_br.ReadInt16();
			int num3 = m_br.ReadByte();
			m_br.ReadUInt32();
			if (num2 == 0 && num3 == 4)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(nBenchPlayers);
				result = true;
				break;
			}
			if (num == 0)
			{
				break;
			}
		}
		return result;
	}

	public bool SetMatchImportance(int importance)
	{
		bool result = false;
		m_br.BaseStream.Position = m_SettingsPosition;
		for (int i = 0; i < 4000; i++)
		{
			m_br.ReadInt16();
			byte num = m_br.ReadByte();
			m_br.ReadInt16();
			byte num2 = m_br.ReadByte();
			m_br.ReadUInt32();
			if (num2 == 21)
			{
				m_br.BaseStream.Position -= 4L;
				m_bw.Write(importance);
				result = true;
			}
			if (num == 0)
			{
				break;
			}
		}
		return result;
	}

	public bool SetDateAndTime(DateTime targetDate)
	{
		bool result = false;
		m_GameDate = FifaUtil.ConvertToFifaDate(targetDate);
		m_GameTime = FifaUtil.ConvertToFifaTime(targetDate);
		DateTime value = FifaUtil.ConvertFromFifaDate(m_DummyTodayDate);
		int days = targetDate.Subtract(value).Days;
		int fieldIndex = m_Career_Calendar.TableDescriptor.GetFieldIndex("currdate");
		m_Career_Calendar.Records[0].IntField[fieldIndex] = m_GameDate;
		fieldIndex = m_Career_Calendar.TableDescriptor.GetFieldIndex("setupdate");
		int fifaDate = m_Career_Calendar.Records[0].IntField[fieldIndex];
		fifaDate = FifaUtil.AddDays(fifaDate, days);
		m_Career_Calendar.Records[0].IntField[fieldIndex] = fifaDate;
		fieldIndex = m_Career_Calendar.TableDescriptor.GetFieldIndex("startdate");
		m_Career_Calendar.Records[0].IntField[fieldIndex] = fifaDate;
		fieldIndex = m_Career_Calendar.TableDescriptor.GetFieldIndex("enddate");
		int fifaDate2 = m_Career_Calendar.Records[0].IntField[fieldIndex];
		fifaDate2 = FifaUtil.AddDays(fifaDate2, days);
		m_Career_Calendar.Records[0].IntField[fieldIndex] = fifaDate2;
		m_br.BaseStream.Position = m_CompidsDatesPosition;
		for (int i = 0; i < 100; i++)
		{
			m_br.ReadInt16();
			m_br.ReadByte();
			m_br.ReadInt16();
			byte num = m_br.ReadByte();
			m_br.ReadByte();
			m_br.ReadInt32();
			m_br.ReadByte();
			int num2 = m_br.ReadInt16();
			if (num != 0)
			{
				m_br.BaseStream.Position -= 7L;
				m_bw.Write(fifaDate2);
				m_bw.BaseStream.Position++;
				num2 = fifaDate / 10000;
				m_bw.Write((short)num2);
				break;
			}
		}
		m_br.BaseStream.Position = m_GamesPosition;
		for (int j = 0; j < 4000; j++)
		{
			m_br.ReadInt16();
			m_br.ReadByte();
			short num3 = m_br.ReadInt16();
			m_br.ReadByte();
			int fifaDate3 = m_br.ReadInt32();
			int num4 = m_br.ReadInt16();
			m_br.ReadInt16();
			m_br.ReadByte();
			m_br.ReadByte();
			m_br.ReadInt16();
			m_br.ReadByte();
			m_br.ReadByte();
			m_br.ReadByte();
			if (num3 == m_DummyMyCompetitionId)
			{
				int value2 = FifaUtil.AddDays(fifaDate3, days);
				num4 = m_GameTime;
				m_bw.BaseStream.Position -= 15L;
				m_bw.Write(value2);
				m_bw.Write((short)num4);
				m_bw.BaseStream.Position += 9L;
				result = true;
			}
		}
		return result;
	}

	public bool SetWeather(int weatherType, int sunsetTime, int nightTime)
	{
		bool result = false;
		m_br.BaseStream.Position = m_WeatherPosition;
		for (int i = 0; i < 330; i++)
		{
			m_br.ReadInt16();
			int num = m_br.ReadByte();
			short num2 = m_br.ReadInt16();
			m_br.ReadByte();
			if (num2 == m_DummyCountryObjid)
			{
				for (int j = 0; j <= 8; j++)
				{
					byte value = (byte)((j == weatherType) ? 100 : 0);
					m_bw.Write(value);
				}
				m_bw.Write((short)sunsetTime);
				m_bw.Write((short)nightTime);
			}
			else
			{
				m_br.BaseStream.Position += 13L;
			}
			if (num == 0)
			{
				break;
			}
		}
		return result;
	}

	public void SetFileName(string fileName)
	{
		m_FileToPatch.InGameName = fileName;
		m_br.Close();
		m_bw.Close();
		m_fs.Close();
		m_FileToPatch.SaveEa(m_FileToPatch.FileName);
	}

	public void SetLeague(League targetLeague)
	{
		int id = targetLeague.Id;
		string shortName = targetLeague.ShortName;
		int level = targetLeague.level;
		int tableIndex = m_FileToPatch.Databases[1].GetTableIndex("leagues");
		if (tableIndex >= 0)
		{
			Table table = m_FileToPatch.Databases[1].Table[tableIndex];
			LeagueList leagueList = new LeagueList();
			leagueList.Load(table, 1, 3000);
			League league = (League)leagueList.SearchId(id);
			if (league == null)
			{
				league = new League(id);
				league.leaguename = shortName;
				league.level = level;
				leagueList.InsertId(league);
				leagueList.SaveLeaguesTable(table, resize: false);
			}
		}
	}

	public void SetPlayerStats(int[][] homeTeamStats, int[][] awayTeamStats)
	{
		m_br.BaseStream.Position = m_PlayerStatsPosition;
		int length = homeTeamStats.GetLength(0);
		int length2 = awayTeamStats.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			WritePlayerStats(homeTeamStats[i]);
		}
		for (int j = 0; j < length2; j++)
		{
			WritePlayerStats(awayTeamStats[j]);
		}
	}

	private void WritePlayerStats(int[] stats)
	{
		m_br.ReadInt16();
		m_bw.Write((byte)1);
		m_bw.Write(stats[0]);
		m_bw.Write(stats[1]);
		m_bw.Write((short)m_DummyMyCompetitionId);
		m_bw.Write((short)stats[3]);
		m_bw.Write((short)stats[4]);
		m_bw.Write((byte)stats[5]);
		m_bw.Write((byte)stats[6]);
		m_bw.Write((byte)stats[7]);
		m_bw.Write((byte)stats[8]);
		m_bw.Write((byte)stats[9]);
		m_bw.Write((byte)stats[10]);
		m_bw.Write((byte)stats[11]);
		m_bw.Write((byte)stats[12]);
		m_bw.Write((byte)stats[13]);
		m_bw.Write((byte)stats[14]);
		m_bw.Write((byte)stats[15]);
		m_bw.Write((byte)stats[16]);
		m_bw.Write((byte)stats[17]);
		m_bw.Write((byte)stats[18]);
		m_bw.Write((byte)stats[19]);
		m_bw.Write((byte)stats[20]);
		m_bw.Write((byte)stats[21]);
		m_bw.Write((byte)stats[22]);
		m_bw.Write((byte)stats[23]);
		m_bw.Write(stats[24]);
		m_bw.Write(stats[25]);
		m_bw.Write(stats[26]);
	}

	public void SetPlayerFatigue(int[][] homeTeamStats, int[][] awayTeamStats)
	{
		m_br.BaseStream.Position = m_FatiguePosition;
		int length = homeTeamStats.GetLength(0);
		int length2 = awayTeamStats.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			WritePlayerFatigue(homeTeamStats[i]);
		}
		for (int j = 0; j < length2; j++)
		{
			WritePlayerFatigue(awayTeamStats[j]);
		}
	}

	public void SetPendingCards(int[][] homeTeamStats, int[][] awayTeamStats)
	{
		m_br.BaseStream.Position = m_CardsPosition;
		int length = homeTeamStats.GetLength(0);
		int length2 = awayTeamStats.GetLength(0);
		for (int i = 0; i < length; i++)
		{
			WritePendingCards(homeTeamStats[i]);
		}
		for (int j = 0; j < length2; j++)
		{
			WritePendingCards(awayTeamStats[j]);
		}
	}

	private void WritePlayerFatigue(int[] stats)
	{
		m_bw.Write(stats[0]);
		m_bw.Write(stats[1]);
		m_bw.Write(stats[2]);
		m_bw.Write(stats[3]);
		m_bw.Write(stats[4]);
		m_bw.Write(stats[5]);
		m_bw.Write(stats[6]);
		m_bw.Write(stats[7]);
		m_bw.Write(stats[8]);
		m_bw.Write(stats[9]);
	}

	private void WritePendingCards(int[] stats)
	{
		if (stats[4] != 0 || stats[7] != 0)
		{
			stats[0] = m_DummyCountryObjid;
			m_bw.Write(stats[0]);
			m_bw.Write(stats[1]);
			m_bw.Write(stats[2]);
			m_bw.Write(stats[3]);
			m_bw.Write(stats[4]);
			m_bw.Write(stats[5]);
			m_bw.Write(stats[6]);
			m_bw.Write(stats[7]);
		}
	}

	public void SwitchHomeAway()
	{
		m_br.BaseStream.Position = m_GamesPosition;
	}

	public void SetGames(int[][] games, bool cleanUnused)
	{
		m_br.BaseStream.Position = m_GamesPosition;
		int num = 0;
		for (int i = 0; i < games.GetLength(0); i++)
		{
			if (games[i] != null)
			{
				WriteGame(games[i]);
				num++;
			}
		}
		if (!cleanUnused)
		{
			return;
		}
		for (int j = num; j < 552; j++)
		{
			m_br.ReadInt16();
			if (m_br.ReadByte() != 0)
			{
				m_br.BaseStream.Position--;
				m_bw.Write((byte)0);
				m_br.BaseStream.Position += 18L;
				continue;
			}
			break;
		}
	}

	private void WriteGame(int[] game)
	{
		if (game[3] < m_DummyStandingKeys.Length && game[6] < m_DummyStandingKeys.Length)
		{
			m_br.ReadInt16();
			m_bw.Write((byte)1);
			m_bw.Write((short)m_DummyMyCompetitionId);
			m_bw.Write((byte)game[0]);
			m_bw.Write(game[1]);
			m_bw.Write((short)game[2]);
			if (game[3] < m_DummyStandingKeys.Length)
			{
				m_bw.Write((short)m_DummyStandingKeys[game[3]]);
			}
			m_bw.Write((byte)game[4]);
			m_bw.Write((byte)game[5]);
			if (game[6] < m_DummyStandingKeys.Length)
			{
				m_bw.Write((short)m_DummyStandingKeys[game[6]]);
			}
			m_bw.Write((byte)game[7]);
			m_bw.Write((byte)game[8]);
			m_bw.Write((byte)game[9]);
		}
	}

	private void ReadGame()
	{
		m_br.ReadInt16();
		m_br.ReadByte();
		m_br.ReadInt16();
		m_br.ReadByte();
		m_br.ReadInt32();
		m_br.ReadInt16();
		m_br.ReadInt16();
		m_br.ReadByte();
		m_br.ReadByte();
		m_br.ReadInt16();
		m_br.ReadByte();
		m_br.ReadByte();
		m_br.ReadByte();
	}

	public bool ImportMatch(string dataFileName, ref int[,] teamData, ref int[,] playerData)
	{
		if (!File.Exists(dataFileName))
		{
			return false;
		}
		Table table = new CareerFile(dataFileName, FifaEnvironment.FifaXmlFileName).Databases[0].GetTable("career_playerlastmatchhistory");
		int fieldIndex = table.TableDescriptor.GetFieldIndex("teamid");
		int fieldIndex2 = table.TableDescriptor.GetFieldIndex("playerid");
		int num = 0;
		int num2 = teamData[0, 2];
		int num3 = teamData[1, 2];
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < table.NValidRecords; i++)
		{
			Record record = table.Records[i];
			int andCheckIntField = record.GetAndCheckIntField(fieldIndex);
			if (andCheckIntField == num2 || andCheckIntField == num3)
			{
				int andCheckIntField2 = record.GetAndCheckIntField(fieldIndex2);
				playerData[num, 0] = andCheckIntField;
				playerData[num, 1] = andCheckIntField2;
				if (andCheckIntField == num2)
				{
					playerData[num, 14] = ((num4 < 11) ? 1 : 0);
					num4++;
				}
				else
				{
					playerData[num, 14] = ((num5 < 11) ? 1 : 0);
					num5++;
				}
				num++;
			}
		}
		FileStream fileStream = new FileStream(dataFileName, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		fileStream.Position = m_GamesPosition;
		for (int j = 0; j < 4000; j++)
		{
			binaryReader.ReadInt16();
			binaryReader.ReadByte();
			binaryReader.ReadInt16();
			binaryReader.ReadByte();
			int num6 = binaryReader.ReadInt32();
			binaryReader.ReadInt16();
			int num7 = binaryReader.ReadInt16();
			int num8 = binaryReader.ReadByte();
			int num9 = binaryReader.ReadByte();
			int num10 = binaryReader.ReadInt16();
			int num11 = binaryReader.ReadByte();
			int num12 = binaryReader.ReadByte();
			binaryReader.ReadByte();
			if (num6 == m_GameDate)
			{
				if (num7 == m_DummyStandingKeys[0] && num10 == m_DummyStandingKeys[1])
				{
					teamData[0, 0] = num8;
					teamData[0, 1] = num9;
					teamData[1, 0] = num11;
					teamData[1, 1] = num12;
					break;
				}
				if (num7 == m_DummyStandingKeys[1] && num10 == m_DummyStandingKeys[0])
				{
					teamData[0, 0] = num8;
					teamData[0, 1] = num9;
					teamData[1, 0] = num11;
					teamData[1, 1] = num12;
					break;
				}
			}
		}
		fileStream.Position = m_PlayerStatsPosition;
		int num13 = 0;
		for (int k = 0; k < 7000; k++)
		{
			binaryReader.ReadInt16();
			if (binaryReader.ReadByte() == 0)
			{
				break;
			}
			int num14 = binaryReader.ReadInt32();
			int num15 = binaryReader.ReadInt32();
			binaryReader.ReadInt16();
			int num16 = binaryReader.ReadInt16();
			int num17 = binaryReader.ReadInt16();
			binaryReader.ReadByte();
			int num18 = binaryReader.ReadByte();
			int num19 = binaryReader.ReadByte();
			int num20 = binaryReader.ReadByte();
			int num21 = binaryReader.ReadByte();
			int num22 = binaryReader.ReadByte();
			int num23 = binaryReader.ReadByte();
			int num24 = binaryReader.ReadByte();
			int num25 = binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadByte();
			binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			if (num16 == 0)
			{
				continue;
			}
			if (table.NValidRecords != 0)
			{
				for (int l = 0; l < num; l++)
				{
					if (playerData[l, 1] == num15)
					{
						num13 = l;
						playerData[num13, 0] = num14;
						playerData[num13, 1] = num15;
						playerData[num13, 2] = num16;
						playerData[num13, 3] = num17;
						playerData[num13, 4] = num18;
						playerData[num13, 5] = num19;
						playerData[num13, 6] = num20;
						playerData[num13, 7] = num21;
						playerData[num13, 8] = num22;
						playerData[num13, 9] = num23;
						playerData[num13, 10] = num24;
						playerData[num13, 11] = num25;
						break;
					}
				}
			}
			else if (num14 == num2 || num14 == num3)
			{
				playerData[num13, 0] = num14;
				playerData[num13, 1] = num15;
				playerData[num13, 2] = num16;
				playerData[num13, 3] = num17;
				playerData[num13, 4] = num18;
				playerData[num13, 5] = num19;
				playerData[num13, 6] = num20;
				playerData[num13, 7] = num21;
				playerData[num13, 8] = num22;
				playerData[num13, 9] = num23;
				playerData[num13, 10] = num24;
				playerData[num13, 11] = num25;
				num13++;
			}
		}
		if (num13 > num)
		{
			num = num13;
		}
		fileStream.Position = m_FatiguePosition;
		int num26 = 0;
		for (int m = 0; m < 2000; m++)
		{
			int num27 = binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			int num28 = binaryReader.ReadInt32();
			int num29 = binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			int num30 = binaryReader.ReadInt32();
			int num31 = binaryReader.ReadInt32();
			if (num30 == 0)
			{
				break;
			}
			if (num28 == 0)
			{
				continue;
			}
			for (int n = 0; n < num; n++)
			{
				if (playerData[n, 1] == num27)
				{
					playerData[n, 12] = num29;
					playerData[n, 13] = ((num31 == 0) ? 1 : 0);
					if (num31 == 0)
					{
						num26++;
					}
					break;
				}
			}
		}
		if (num26 > 2)
		{
			for (int num32 = 0; num32 < num; num32++)
			{
				playerData[num32, 13] = 0;
			}
		}
		binaryReader.Close();
		fileStream.Close();
		return true;
	}
}
