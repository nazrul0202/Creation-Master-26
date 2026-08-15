using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using FifaLibrary;

namespace CreationMaster;

public class PatchedObject
{
	public enum EUsedObject
	{
		Undefined,
		UseCms,
		UseNew,
		UseFitting
	}

	public static bool s_RefereeKitNotLoaded;

	private static int s_PlayerCount;

	public static bool s_TeamCrossReferenceRequired;

	public static bool s_PlayerCrossReferenceRequired;

	public static bool s_CountryCrossReferenceRequired;

	public static bool s_ShoesCrossReferenceRequired;

	public static bool s_BallCrossReferenceRequired;

	public static bool s_AdboardCrossReferenceRequired;

	private static Language s_Language = null;

	private static DataSet s_FifaDataSet = null;

	private static Table s_LangTable = null;

	private static Table s_NationsTable = null;

	private static Table s_TeamsTable = null;

	private static Table s_TeamplayerlinksTable = null;

	private static Table s_TeamNationLinksTable = null;

	private static Table s_TeamstadiumlinksTable = null;

	private static Table s_TeamformationteamstylelinkTable = null;

	private static Table s_StadiumassignmentsTable = null;

	private static Table s_ManagerTable = null;

	private static Table s_TeamkitsTable = null;

	private static Table s_RowteamnationlinksTable = null;

	private static Table s_TeamnationlinksTable = null;

	private static Table s_FormationsTable = null;

	private static Table s_LeaguesTable = null;

	private static Table s_BoardOutcomesTable = null;

	private static Table s_LeagueteamlinksTable = null;

	private static Table s_PlayernamesTable = null;

	private static Table s_DcPlayernamesTable = null;

	private static Table s_PlayersTable = null;

	private static Table s_PlayersLoanTable = null;

	private static Table s_PreviousTeamTable = null;

	private static Table s_RefereeTable = null;

	private static Table s_StadiumsTable = null;

	private static PlayerNames s_PlayerNames = null;

	private static DataSet s_Fifa12DataSet = null;

	private string m_Type;

	private string m_Name;

	private int m_Id;

	private int m_ImportId;

	private object m_ReplacedObject;

	private object m_NewObject;

	private object m_CmsObject;

	private bool m_Imported;

	private EUsedObject m_UsedObject;

	private bool m_IsCmsNew;

	private static int s_LastLoadedTeamId = -1;

	private static int[] c_FormationSwitchTable = new int[21]
	{
		801, 806, 808, 808, 808, 809, 803, 805, 802, 803,
		805, 804, 801, 801, 805, 806, 807, 806, 807, 801,
		801
	};

	public string Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public string Name
	{
		get
		{
			return m_Name;
		}
		set
		{
			m_Name = value;
		}
	}

	public int Id
	{
		get
		{
			return m_Id;
		}
		set
		{
			m_Id = value;
		}
	}

	public int ImportId => m_ImportId;

	public object ReplacedObject
	{
		get
		{
			return m_ReplacedObject;
		}
		set
		{
			m_ReplacedObject = value;
			if (IsUsedFittingObject() && m_ReplacedObject != null)
			{
				m_ImportId = ((IdObject)m_ReplacedObject).Id;
			}
		}
	}

	public object NewObject
	{
		get
		{
			return m_NewObject;
		}
		set
		{
			m_NewObject = value;
		}
	}

	public object CmsObject
	{
		get
		{
			return m_CmsObject;
		}
		set
		{
			m_CmsObject = value;
		}
	}

	public bool Imported
	{
		get
		{
			return m_Imported;
		}
		set
		{
			m_Imported = value;
		}
	}

	public EUsedObject UsedObject
	{
		get
		{
			return m_UsedObject;
		}
		set
		{
			m_UsedObject = value;
			switch (m_UsedObject)
			{
			case EUsedObject.UseNew:
				m_ImportId = ((IdObject)m_NewObject).Id;
				break;
			case EUsedObject.UseFitting:
				m_ImportId = ((IdObject)m_ReplacedObject).Id;
				break;
			case EUsedObject.UseCms:
				m_ImportId = ((IdObject)m_CmsObject).Id;
				break;
			}
		}
	}

	public bool IsCmsNew
	{
		get
		{
			return m_IsCmsNew;
		}
		set
		{
			m_IsCmsNew = value;
		}
	}

	public static void SetLanguageDataSet(DataSet langDataSet)
	{
		if (!(langDataSet.DataSetName == "LANG14") && !(langDataSet.DataSetName == "LANG15") && !(langDataSet.DataSetName == "LANG16"))
		{
			return;
		}
		DataTable dataTable = langDataSet.Tables["LanguageStrings"];
		s_LangTable = null;
		if (dataTable != null)
		{
			s_LangTable = new Table(FifaEnvironment.LangDb.Table[TI.lang].TableDescriptor);
			s_LangTable.ConvertFromDataTable(dataTable);
			if (s_LangTable != null)
			{
				s_Language = new Language(s_LangTable);
			}
		}
	}

	private static DataRow ConvertDefaultDataRowFromPreviousFifa(DataRow previousFifaDataRow, DataRow fifaDataRow)
	{
		for (int i = 0; i < previousFifaDataRow.ItemArray.Length; i++)
		{
			string columnName = previousFifaDataRow.Table.Columns[i].ColumnName;
			if (fifaDataRow.Table.Columns.Contains(columnName))
			{
				fifaDataRow[columnName] = previousFifaDataRow[i];
			}
		}
		return fifaDataRow;
	}

	public static void ConvertDataTableFromPreviousFifa(DataTable previousFifaDataTable)
	{
		foreach (DataRow row in previousFifaDataTable.Rows)
		{
			ConvertDataRowFromPreviousFifa(row);
		}
	}

	public static void ConvertDataRowFromPreviousFifa(DataRow previousFifaDataRow)
	{
		_ = previousFifaDataRow.Table.TableName == "players";
	}

	public static void ConvertPlayersFromPreviousFifa(DataRow playersPreviousDataRow)
	{
		DataRow dataRow = s_Fifa12DataSet.Tables["players"].NewRow();
		ConvertDefaultDataRowFromPreviousFifa(playersPreviousDataRow, dataRow);
		Record record = new Record(FifaEnvironment.FifaDb.Table[TI.players].TableDescriptor);
		record.ConvertFromDataRow(dataRow);
		record.IntField[FI.players_playerid] = (int)playersPreviousDataRow["playerid"];
	}

	public static bool SetFifaDataSet(DataSet fifaDataSet)
	{
		if (fifaDataSet.DataSetName == "FIFA16")
		{
			s_FifaDataSet = fifaDataSet;
			s_NationsTable = ConvertTable("nations", TI.nations);
			s_TeamsTable = ConvertTable("teams", TI.teams);
			s_TeamplayerlinksTable = ConvertTable("teamplayerlinks", TI.teamplayerlinks);
			s_TeamstadiumlinksTable = ConvertTable("teamstadiumlinks", TI.teamstadiumlinks);
			s_TeamformationteamstylelinkTable = ConvertTable("teamformationteamstylelinks", TI.teamformationteamstylelinks);
			s_StadiumassignmentsTable = ConvertTable("stadiumassignments", TI.stadiumassignments);
			s_ManagerTable = ConvertTable("manager", TI.manager);
			s_TeamkitsTable = ConvertTable("teamkits", TI.teamkits);
			s_RowteamnationlinksTable = ConvertTable("rowteamnationlinks", TI.rowteamnationlinks);
			s_TeamnationlinksTable = ConvertTable("teamnationlinks", TI.teamnationlinks);
			s_FormationsTable = ConvertTable("formations", TI.formations);
			s_LeaguesTable = ConvertTable("leagues", TI.leagues);
			s_BoardOutcomesTable = ConvertTable("career_boardoutcomes", TI.career_boardoutcomes);
			s_LeagueteamlinksTable = ConvertTable("leagueteamlinks", TI.leagueteamlinks);
			s_PlayersTable = ConvertTable("players", TI.players);
			s_PlayersLoanTable = ConvertTable("playerloans", TI.playerloans);
			s_PreviousTeamTable = ConvertTable("previousteam", TI.previousteam);
			s_RefereeTable = ConvertTable("referee", TI.referee);
			s_StadiumsTable = ConvertTable("stadiums", TI.stadiums);
			s_PlayernamesTable = ConvertTable("playernames", TI.playernames);
			s_DcPlayernamesTable = ConvertTable("dcplayernames", TI.dcplayernames);
			if (s_DcPlayernamesTable == null)
			{
				s_PlayerNames = new PlayerNames(s_PlayernamesTable);
			}
			else if (s_PlayernamesTable != null && s_PlayernamesTable.Records.Length != 0)
			{
				s_PlayerNames = new PlayerNames(s_PlayernamesTable, s_DcPlayernamesTable);
			}
			return true;
		}
		if (fifaDataSet.DataSetName == "FIFA15" || fifaDataSet.DataSetName == "FIFA14")
		{
			s_FifaDataSet = fifaDataSet;
			s_NationsTable = ConvertTable("nations", TI.nations);
			s_TeamsTable = ConvertTable("teams", TI.teams);
			s_TeamplayerlinksTable = ConvertTableFrom15To16("teamplayerlinks", TI.teamplayerlinks);
			s_TeamstadiumlinksTable = ConvertTable("teamstadiumlinks", TI.teamstadiumlinks);
			s_TeamformationteamstylelinkTable = ConvertTable("teamformationteamstylelinks", TI.teamformationteamstylelinks);
			s_StadiumassignmentsTable = ConvertTable("stadiumassignments", TI.stadiumassignments);
			s_ManagerTable = ConvertTable("manager", TI.manager);
			s_TeamkitsTable = ConvertTable("teamkits", TI.teamkits);
			s_RowteamnationlinksTable = ConvertTable("rowteamnationlinks", TI.rowteamnationlinks);
			s_TeamnationlinksTable = ConvertTable("teamnationlinks", TI.teamnationlinks);
			s_FormationsTable = ConvertTableFrom15To16("formations", TI.formations);
			s_LeaguesTable = ConvertTable("leagues", TI.leagues);
			s_BoardOutcomesTable = ConvertTable("career_boardoutcomes", TI.career_boardoutcomes);
			s_LeagueteamlinksTable = ConvertTableFrom15To16("leagueteamlinks", TI.leagueteamlinks);
			s_PlayersTable = ConvertTableFrom15To16("players", TI.players);
			s_RefereeTable = ConvertTableFrom15To16("referee", TI.referee);
			s_StadiumsTable = ConvertTableFrom15To16("stadiums", TI.stadiums);
			s_PlayernamesTable = ConvertTable("playernames", TI.playernames);
			if (s_PlayernamesTable != null && s_PlayernamesTable.Records.Length != 0)
			{
				s_PlayerNames = new PlayerNames(s_PlayernamesTable);
			}
			return true;
		}
		FifaEnvironment.UserMessages.ShowMessage(1032);
		return false;
	}

	private static void ConvertTablesFrom14to15()
	{
		if (s_TeamsTable != null)
		{
			for (int i = 0; i < s_TeamsTable.Records.Length; i++)
			{
				Record record = s_TeamsTable.Records[i];
				record.IntField[FI.teams_rightfreekicktakerid] = record.IntField[FI.teams_freekicktakerid];
				record.IntField[FI.teams_leftfreekicktakerid] = record.IntField[FI.teams_freekicktakerid];
			}
		}
		if (s_FormationsTable != null)
		{
			for (int j = 0; j < s_FormationsTable.Records.Length; j++)
			{
				Record obj = s_FormationsTable.Records[j];
				int roleid = obj.IntField[FI.formations_position0];
				obj.IntField[FI.formations_playerinstruction0_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position1];
				obj.IntField[FI.formations_playerinstruction1_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position2];
				obj.IntField[FI.formations_playerinstruction2_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position3];
				obj.IntField[FI.formations_playerinstruction3_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position4];
				obj.IntField[FI.formations_playerinstruction4_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position5];
				obj.IntField[FI.formations_playerinstruction5_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position6];
				obj.IntField[FI.formations_playerinstruction6_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position7];
				obj.IntField[FI.formations_playerinstruction7_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position8];
				obj.IntField[FI.formations_playerinstruction8_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position9];
				obj.IntField[FI.formations_playerinstruction9_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position10];
				obj.IntField[FI.formations_playerinstruction10_1] = PlayingRole.GetDefaultInstruction(roleid);
			}
		}
	}

	private static void ConvertTablesFrom15to16()
	{
		if (s_TeamsTable != null)
		{
			for (int i = 0; i < s_TeamsTable.Records.Length; i++)
			{
			}
		}
		if (s_FormationsTable != null)
		{
			for (int j = 0; j < s_FormationsTable.Records.Length; j++)
			{
				Record obj = s_FormationsTable.Records[j];
				int roleid = obj.IntField[FI.formations_position0];
				obj.IntField[FI.formations_playerinstruction0_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position1];
				obj.IntField[FI.formations_playerinstruction1_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position2];
				obj.IntField[FI.formations_playerinstruction2_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position3];
				obj.IntField[FI.formations_playerinstruction3_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position4];
				obj.IntField[FI.formations_playerinstruction4_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position5];
				obj.IntField[FI.formations_playerinstruction5_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position6];
				obj.IntField[FI.formations_playerinstruction6_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position7];
				obj.IntField[FI.formations_playerinstruction7_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position8];
				obj.IntField[FI.formations_playerinstruction8_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position9];
				obj.IntField[FI.formations_playerinstruction9_1] = PlayingRole.GetDefaultInstruction(roleid);
				roleid = obj.IntField[FI.formations_position10];
				obj.IntField[FI.formations_playerinstruction10_1] = PlayingRole.GetDefaultInstruction(roleid);
			}
		}
	}

	private static void ConvertTablesFrom15to14()
	{
	}

	private static Table ConvertTable(string tableName, int tableIndex)
	{
		if (s_FifaDataSet == null)
		{
			return null;
		}
		s_FifaDataSet.Locale = new CultureInfo(CultureInfo.InvariantCulture.LCID);
		DataTable dataTable = s_FifaDataSet.Tables[tableName];
		if (dataTable == null)
		{
			return null;
		}
		Table table = new Table(FifaEnvironment.FifaDb.Table[tableIndex].TableDescriptor);
		table.ConvertFromDataTable(dataTable);
		return table;
	}

	private static Table ConvertTableFrom15To16(string tableName, int tableIndex)
	{
		if (s_FifaDataSet == null)
		{
			return null;
		}
		s_FifaDataSet.Locale = new CultureInfo(CultureInfo.InvariantCulture.LCID);
		DataTable dataTable = s_FifaDataSet.Tables[tableName];
		if (dataTable == null)
		{
			return null;
		}
		Table table = new Table(FifaEnvironment.FifaDb.Table[tableIndex].TableDescriptor);
		table.ConvertFromDataTableFrom15To16(dataTable);
		return table;
	}

	private static DataTable ConvertDataTableFromPreviousFifa(string tableName, int tableIndex)
	{
		return ConvertDataTableFromPreviousFifa(tableName, tableName, tableIndex);
	}

	private static DataTable ConvertDataTableFromPreviousFifa(string oldTableName, string newTableName, int newTableIndex)
	{
		if (s_FifaDataSet == null)
		{
			return null;
		}
		if (!s_Fifa12DataSet.Tables.Contains(newTableName))
		{
			return null;
		}
		DataTable dataTable = s_FifaDataSet.Tables[oldTableName];
		s_Fifa12DataSet.Tables[newTableName].Clear();
		foreach (DataRow row in dataTable.Rows)
		{
			DataRow dataRow = s_Fifa12DataSet.Tables[newTableName].NewRow();
			ConvertDefaultDataRowFromPreviousFifa(row, dataRow);
			s_Fifa12DataSet.Tables[newTableName].Rows.Add(dataRow);
		}
		return s_Fifa12DataSet.Tables[newTableName];
	}

	public static void Initialize()
	{
		s_PlayerCount = 0;
	}

	public bool IsUsedUndefinedObject()
	{
		return m_UsedObject == EUsedObject.Undefined;
	}

	public bool IsUsedNewObject()
	{
		return m_UsedObject == EUsedObject.UseNew;
	}

	public bool IsUsedCmsObject()
	{
		return m_UsedObject == EUsedObject.UseCms;
	}

	public bool IsUsedFittingObject()
	{
		return m_UsedObject == EUsedObject.UseFitting;
	}

	public bool IsObjectUsedNew()
	{
		if (m_UsedObject != EUsedObject.UseNew)
		{
			if (m_UsedObject == EUsedObject.UseCms)
			{
				return IsCmsNew;
			}
			return false;
		}
		return true;
	}

	public Player GetPlayerToImport()
	{
		return (Player)GetUsedObject();
	}

	public PatchedObject(string type, string name, int id)
	{
		m_Type = type;
		m_Name = name;
		m_Id = id;
		m_Imported = false;
	}

	public void UseReplacedObject()
	{
		UsedObject = EUsedObject.UseFitting;
	}

	public void UsePatchId()
	{
		UsedObject = EUsedObject.UseCms;
	}

	public void UseNewObject()
	{
		if (m_NewObject != null)
		{
			UsedObject = EUsedObject.UseNew;
		}
		else if (AssignAutoNewObject())
		{
			UsedObject = EUsedObject.UseNew;
		}
	}

	private void RemoveObject(object toBeRemovedObject)
	{
		if (m_Type == "Player")
		{
			FifaEnvironment.Players.RemoveId((Player)toBeRemovedObject);
		}
		else if (m_Type == "Team")
		{
			FifaEnvironment.Teams.RemoveId((Team)toBeRemovedObject);
		}
		else if (m_Type == "League")
		{
			FifaEnvironment.Leagues.RemoveId((League)toBeRemovedObject);
		}
		else if (m_Type == "Country")
		{
			FifaEnvironment.Countries.RemoveId((Country)toBeRemovedObject);
		}
		else if (m_Type == "Stadium")
		{
			FifaEnvironment.Stadiums.RemoveId((Stadium)toBeRemovedObject);
		}
		else if (m_Type == "Referee")
		{
			FifaEnvironment.Referees.RemoveId((Referee)toBeRemovedObject);
		}
		else if (m_Type == "Formation")
		{
			FifaEnvironment.Formations.RemoveId((Formation)toBeRemovedObject);
		}
		else if (!(m_Type == "Sponsor"))
		{
			if (m_Type == "Ball")
			{
				FifaEnvironment.Balls.RemoveId((Ball)toBeRemovedObject);
			}
			else if (m_Type == "Adboard")
			{
				FifaEnvironment.Adboards.RemoveId((Adboard)toBeRemovedObject);
			}
			else if (m_Type == "NumberFont")
			{
				FifaEnvironment.NumberFonts.RemoveId((NumberFont)toBeRemovedObject);
			}
			else if (m_Type == "NameFont")
			{
				FifaEnvironment.NameFonts.RemoveId((NameFont)toBeRemovedObject);
			}
			else if (m_Type == "Shoes")
			{
				FifaEnvironment.Shoes.RemoveId((Shoes)toBeRemovedObject);
			}
			else if (m_Type == "Net")
			{
				FifaEnvironment.Nets.RemoveId((Net)toBeRemovedObject);
			}
			else if (m_Type == "Grass")
			{
				FifaEnvironment.GkGloves.RemoveId((GkGloves)toBeRemovedObject);
			}
			else if (m_Type == "MowingPatterns")
			{
				FifaEnvironment.MowingPatterns.RemoveId((MowingPattern)toBeRemovedObject);
			}
			else if (m_Type == "Kit")
			{
				FifaEnvironment.Kits.RemoveId((Kit)toBeRemovedObject);
			}
		}
	}

	public void RemoveNewObject()
	{
		if (m_NewObject != null)
		{
			RemoveObject(m_NewObject);
		}
		if (IsCmsNew && m_CmsObject != null)
		{
			RemoveObject(m_CmsObject);
		}
	}

	public void RemoveNewObjectIfUnused()
	{
		object usedObject = GetUsedObject();
		if (usedObject != m_NewObject && m_NewObject != null)
		{
			RemoveObject(m_NewObject);
		}
		if (usedObject != m_CmsObject && m_CmsObject != null && IsCmsNew)
		{
			RemoveObject(m_CmsObject);
		}
	}

	public void RemoveNewObjectIfNotImported()
	{
		object usedObject = GetUsedObject();
		if (m_Imported)
		{
			if (usedObject != m_NewObject && m_NewObject != null)
			{
				RemoveObject(m_NewObject);
			}
			if (usedObject != m_CmsObject && m_CmsObject != null && IsCmsNew)
			{
				RemoveObject(m_CmsObject);
			}
		}
		else
		{
			if (m_NewObject != null)
			{
				RemoveObject(m_NewObject);
			}
			if (m_CmsObject != null && IsCmsNew)
			{
				RemoveObject(m_CmsObject);
			}
		}
	}

	public Color GetColor()
	{
		if (IsObjectUsedNew())
		{
			if (m_NewObject != null || m_CmsObject != null)
			{
				return Color.Green;
			}
			return Color.Gray;
		}
		return Color.Red;
	}

	private IdObject CreateNewObject()
	{
		return CreateNewObject(-1);
	}

	private IdObject CreateNewObject(int id)
	{
		IdObject idObject = null;
		if (m_Type == "Player")
		{
			idObject = ((id == -1) ? FifaEnvironment.Players.CreateNewId() : FifaEnvironment.Players.CreateNewId(id));
		}
		else if (m_Type == "Team")
		{
			idObject = ((id == -1) ? FifaEnvironment.Teams.CreateNewId() : FifaEnvironment.Teams.CreateNewId(id));
		}
		else if (m_Type == "League")
		{
			idObject = ((id == -1) ? FifaEnvironment.Leagues.CreateNewId() : FifaEnvironment.Leagues.CreateNewId(id));
		}
		else if (m_Type == "Country")
		{
			idObject = ((id == -1) ? FifaEnvironment.Countries.CreateNewId() : FifaEnvironment.Countries.CreateNewId(id));
		}
		else if (m_Type == "Stadium")
		{
			idObject = ((id == -1) ? FifaEnvironment.Stadiums.CreateNewId() : FifaEnvironment.Stadiums.CreateNewId(id));
		}
		else if (m_Type == "Referee")
		{
			idObject = ((id == -1) ? FifaEnvironment.Referees.CreateNewId() : FifaEnvironment.Referees.CreateNewId(id));
		}
		else if (m_Type == "Formation")
		{
			idObject = FifaEnvironment.Formations.CreateNewId();
		}
		else if (m_Type == "Ball")
		{
			idObject = ((id == -1) ? FifaEnvironment.Balls.CreateNewId() : FifaEnvironment.Balls.CreateNewId(id));
		}
		else if (m_Type == "Adboard")
		{
			idObject = ((id == -1) ? FifaEnvironment.Adboards.CreateNewId() : FifaEnvironment.Adboards.CreateNewId(id));
		}
		else if (m_Type == "NumberFont")
		{
			idObject = ((id == -1) ? FifaEnvironment.NumberFonts.CreateNewId() : FifaEnvironment.NumberFonts.CreateNewId(id));
		}
		else if (m_Type == "NameFont")
		{
			idObject = ((id == -1) ? FifaEnvironment.NameFonts.CreateNewId() : FifaEnvironment.NameFonts.CreateNewId(id));
		}
		else if (m_Type == "Shoes")
		{
			idObject = ((id == -1) ? FifaEnvironment.Shoes.CreateNewId() : FifaEnvironment.Shoes.CreateNewId(id));
		}
		else if (m_Type == "Net")
		{
			idObject = ((id == -1) ? FifaEnvironment.Nets.CreateNewId() : FifaEnvironment.Nets.CreateNewId(id));
		}
		else if (m_Type == "GkGloves")
		{
			idObject = ((id == -1) ? FifaEnvironment.GkGloves.CreateNewId() : FifaEnvironment.GkGloves.CreateNewId(id));
		}
		else if (m_Type == "MowingPattern")
		{
			idObject = ((id == -1) ? FifaEnvironment.MowingPatterns.CreateNewId() : FifaEnvironment.MowingPatterns.CreateNewId(id));
		}
		else if (m_Type == "Kit")
		{
			idObject = FifaEnvironment.Kits.CreateNewId();
			int num = m_Id / 10;
			((Kit)idObject).teamid = num;
			((Kit)idObject).kittype = m_Id - 10 * num;
		}
		if (idObject == null)
		{
			FifaEnvironment.UserMessages.ShowMessage(5043);
		}
		return idObject;
	}

	private bool AssignAutoNewObject()
	{
		if (IsCmsNew)
		{
			m_NewObject = m_CmsObject;
		}
		else
		{
			m_NewObject = CreateNewObject();
		}
		return m_NewObject != null;
	}

	private bool AssignFittingObject()
	{
		if (m_Type == "Player")
		{
			m_ReplacedObject = FifaEnvironment.Players.FitPlayer(m_Name, m_Id);
			if (m_ReplacedObject != null)
			{
				UsedObject = EUsedObject.UseFitting;
			}
			if (m_ReplacedObject == null && FifaEnvironment.Players.Count > 0)
			{
				m_ReplacedObject = FifaEnvironment.Players[0];
			}
		}
		else if (m_Type == "Team")
		{
			m_ReplacedObject = FifaEnvironment.Teams.FitTeam(m_Name, m_Id);
			s_LastLoadedTeamId = m_Id;
			if (m_ReplacedObject != null)
			{
				UsedObject = EUsedObject.UseFitting;
			}
			if (m_ReplacedObject == null && FifaEnvironment.Teams.Count > 0)
			{
				m_ReplacedObject = FifaEnvironment.Teams[0];
			}
		}
		else if (m_Type == "League")
		{
			m_ReplacedObject = FifaEnvironment.Leagues.FitLeague(m_Name, m_Id);
			if (m_ReplacedObject != null)
			{
				UsedObject = EUsedObject.UseFitting;
			}
			if (m_ReplacedObject == null && FifaEnvironment.Leagues.Count > 0)
			{
				m_ReplacedObject = FifaEnvironment.Leagues[0];
			}
		}
		else if (m_Type == "Country")
		{
			m_ReplacedObject = FifaEnvironment.Countries.FitCountry(m_Name, m_Id);
			if (m_ReplacedObject != null)
			{
				UsedObject = EUsedObject.UseFitting;
			}
			if (m_ReplacedObject == null && FifaEnvironment.Countries.Count > 0)
			{
				m_ReplacedObject = FifaEnvironment.Countries[0];
			}
		}
		else if (m_Type == "Stadium")
		{
			m_ReplacedObject = FifaEnvironment.Stadiums.FitStadium(m_Name, m_Id);
			if (m_ReplacedObject != null)
			{
				UsedObject = EUsedObject.UseFitting;
			}
			if (m_ReplacedObject == null && FifaEnvironment.Stadiums.Count > 0)
			{
				m_ReplacedObject = FifaEnvironment.Stadiums[0];
			}
		}
		else if (m_Type == "Referee")
		{
			m_ReplacedObject = FifaEnvironment.Referees.FitReferee(m_Name, m_Id);
			if (m_ReplacedObject != null)
			{
				UsedObject = EUsedObject.UseFitting;
			}
			if (m_ReplacedObject == null && FifaEnvironment.Referees.Count > 0)
			{
				m_ReplacedObject = FifaEnvironment.Referees[0];
			}
		}
		else if (m_Type == "Formation")
		{
			m_ReplacedObject = FifaEnvironment.Formations.FitFormationByTeamId(s_LastLoadedTeamId);
			if (m_ReplacedObject != null)
			{
				UsedObject = EUsedObject.UseFitting;
			}
			if (m_ReplacedObject == null && FifaEnvironment.Formations.Count > 0)
			{
				m_ReplacedObject = FifaEnvironment.Formations[0];
			}
		}
		else if (!(m_Type == "Sponsor"))
		{
			if (m_Type == "Kit")
			{
				m_ReplacedObject = FifaEnvironment.Kits.FitKit(m_Name, m_Id);
				if (m_ReplacedObject != null)
				{
					UsedObject = EUsedObject.UseFitting;
				}
				if (m_ReplacedObject == null && FifaEnvironment.Kits.Count > 0)
				{
					m_ReplacedObject = FifaEnvironment.Kits[0];
				}
			}
			else if (m_Type == "Ball")
			{
				m_ReplacedObject = FifaEnvironment.Balls.SearchId(m_Id);
				if (m_ReplacedObject != null)
				{
					UsedObject = EUsedObject.UseFitting;
				}
				if (m_ReplacedObject == null && FifaEnvironment.Balls.Count > 0)
				{
					m_ReplacedObject = FifaEnvironment.Balls[0];
				}
			}
			else if (m_Type == "Adboard")
			{
				m_ReplacedObject = FifaEnvironment.Adboards.SearchId(m_Id);
				if (m_ReplacedObject != null)
				{
					UsedObject = EUsedObject.UseFitting;
				}
				if (m_ReplacedObject == null && FifaEnvironment.Adboards.Count > 0)
				{
					m_ReplacedObject = FifaEnvironment.Adboards[0];
				}
			}
			else if (m_Type == "NumberFont")
			{
				m_ReplacedObject = FifaEnvironment.NumberFonts.SearchId(m_Id);
				if (m_ReplacedObject != null)
				{
					UsedObject = EUsedObject.UseFitting;
				}
				if (m_ReplacedObject == null && FifaEnvironment.NumberFonts.Count > 0)
				{
					m_ReplacedObject = FifaEnvironment.NumberFonts[0];
				}
			}
			else if (m_Type == "NameFont")
			{
				m_ReplacedObject = FifaEnvironment.NameFonts.SearchId(m_Id);
				if (m_ReplacedObject != null)
				{
					UsedObject = EUsedObject.UseFitting;
				}
				if (m_ReplacedObject == null && FifaEnvironment.NameFonts.Count > 0)
				{
					m_ReplacedObject = FifaEnvironment.NameFonts[0];
				}
			}
			else if (m_Type == "Shoes")
			{
				m_ReplacedObject = FifaEnvironment.Shoes.SearchId(m_Id);
				if (m_ReplacedObject != null)
				{
					UsedObject = EUsedObject.UseFitting;
				}
				if (m_ReplacedObject == null && FifaEnvironment.Shoes.Count > 0)
				{
					m_ReplacedObject = FifaEnvironment.Shoes[0];
				}
			}
			else if (m_Type == "Net")
			{
				m_ReplacedObject = FifaEnvironment.Nets.SearchId(m_Id);
				if (m_ReplacedObject != null)
				{
					UsedObject = EUsedObject.UseFitting;
				}
				if (m_ReplacedObject == null && FifaEnvironment.Nets.Count > 0)
				{
					m_ReplacedObject = FifaEnvironment.Nets[0];
				}
			}
			else if (m_Type == "GkGloves")
			{
				m_ReplacedObject = FifaEnvironment.GkGloves.SearchId(m_Id);
				if (m_ReplacedObject != null)
				{
					UsedObject = EUsedObject.UseFitting;
				}
				if (m_ReplacedObject == null && FifaEnvironment.GkGloves.Count > 0)
				{
					m_ReplacedObject = FifaEnvironment.GkGloves[0];
				}
			}
			else if (m_Type == "MowingPattern")
			{
				m_ReplacedObject = FifaEnvironment.MowingPatterns.SearchId(m_Id);
				if (m_ReplacedObject != null)
				{
					UsedObject = EUsedObject.UseFitting;
				}
				if (m_ReplacedObject == null && FifaEnvironment.MowingPatterns.Count > 0)
				{
					m_ReplacedObject = FifaEnvironment.MowingPatterns[0];
				}
			}
		}
		return m_ReplacedObject != null;
	}

	public bool AssignCmsReplacedObject()
	{
		if (m_Type == "Player")
		{
			m_CmsObject = FifaEnvironment.Players.SearchId(Id);
		}
		else if (m_Type == "Team")
		{
			m_CmsObject = FifaEnvironment.Teams.SearchId(Id);
		}
		else if (m_Type == "League")
		{
			m_CmsObject = FifaEnvironment.Leagues.SearchId(Id);
		}
		else if (m_Type == "Country")
		{
			m_CmsObject = FifaEnvironment.Countries.SearchId(Id);
		}
		else if (m_Type == "Stadium")
		{
			m_CmsObject = FifaEnvironment.Stadiums.SearchId(Id);
		}
		else if (m_Type == "Referee")
		{
			m_CmsObject = FifaEnvironment.Referees.SearchId(Id);
		}
		else if (m_Type == "Formation")
		{
			if (((Formation)m_ReplacedObject).teamid >= 1)
			{
				m_CmsObject = m_ReplacedObject;
			}
			else
			{
				m_CmsObject = null;
			}
		}
		else if (m_Type == "Ball")
		{
			m_CmsObject = FifaEnvironment.Balls.SearchId(Id);
		}
		else if (m_Type == "Adboard")
		{
			m_CmsObject = FifaEnvironment.Adboards.SearchId(Id);
		}
		else if (m_Type == "NumberFont")
		{
			m_CmsObject = FifaEnvironment.NumberFonts.SearchId(Id);
		}
		else if (m_Type == "NameFont")
		{
			m_CmsObject = FifaEnvironment.NameFonts.SearchId(Id);
		}
		else if (m_Type == "Shoes")
		{
			m_CmsObject = FifaEnvironment.Shoes.SearchId(Id);
		}
		else if (m_Type == "Net")
		{
			m_CmsObject = FifaEnvironment.Nets.SearchId(Id);
		}
		else if (m_Type == "GkGloves")
		{
			m_CmsObject = FifaEnvironment.GkGloves.SearchId(Id);
		}
		else if (m_Type == "MowingPattern")
		{
			m_CmsObject = FifaEnvironment.MowingPatterns.SearchId(Id);
		}
		else if (m_Type == "Kit")
		{
			m_CmsObject = null;
		}
		IsCmsNew = m_CmsObject == null;
		m_UsedObject = EUsedObject.UseCms;
		return !IsCmsNew;
	}

	public bool AssignNewCmsObject()
	{
		if (IsCmsNew)
		{
			m_CmsObject = CreateNewObject(Id);
		}
		if (m_CmsObject != null)
		{
			UsedObject = EUsedObject.UseCms;
		}
		return m_CmsObject != null;
	}

	public void AssignReplacedObject()
	{
		AssignFittingObject();
		AssignCmsReplacedObject();
	}

	public void AssignNewObject()
	{
		if (IsUsedUndefinedObject() && AssignAutoNewObject())
		{
			UsedObject = EUsedObject.UseNew;
		}
	}

	public object GetUsedObject()
	{
		return UsedObject switch
		{
			EUsedObject.UseCms => m_CmsObject, 
			EUsedObject.UseFitting => m_ReplacedObject, 
			_ => m_NewObject, 
		};
	}

	public string GetObjectType()
	{
		return UsedObject switch
		{
			EUsedObject.UseCms => m_CmsObject.GetType().Name, 
			EUsedObject.UseFitting => m_ReplacedObject.GetType().Name, 
			EUsedObject.UseNew => m_NewObject.GetType().Name, 
			_ => null, 
		};
	}

	public Player ImportWebPlayer(DataRow webData, Team importingTeam)
	{
		Player player = (Player)GetUsedObject();
		bool flag = IsObjectUsedNew();
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = flag || player.IsFreeAgent();
		bool flag5;
		if (importingTeam != null)
		{
			flag2 = !importingTeam.IsNationalTeam();
			flag3 = !player.IsFreeAgent() && !player.IsPlayingFor(importingTeam);
			flag5 = !player.IsFreeAgent() && player.IsPlayingFor(importingTeam);
		}
		else
		{
			flag5 = !player.IsFreeAgent();
		}
		if (webData["website"] != "sofifa" || flag)
		{
			player.commonname = webData["commonname"].ToString();
			if (player.commonname != string.Empty)
			{
				if (webData["firstname"].ToString() != string.Empty)
				{
					player.firstname = webData["firstname"].ToString();
				}
				if (webData["lastname"].ToString() != string.Empty)
				{
					player.lastname = webData["lastname"].ToString();
				}
				player.playerjerseyname = player.commonname;
			}
			else
			{
				player.firstname = webData["firstname"].ToString();
				player.lastname = webData["lastname"].ToString();
				player.playerjerseyname = player.lastname;
			}
		}
		string text = webData["birthdate"].ToString().Replace("\t", "");
		if (text != string.Empty)
		{
			try
			{
				DateTime birthdate = FifaUtil.ConvertToDate(text);
				player.birthdate = birthdate;
			}
			catch
			{
			}
		}
		if (player.birthdate.Year == 1)
		{
			return null;
		}
		if (webData["height"] != DBNull.Value)
		{
			try
			{
				player.height = Convert.ToInt32(webData["height"].ToString());
			}
			catch
			{
			}
		}
		if (webData["weight"].ToString() != string.Empty)
		{
			try
			{
				player.weight = Convert.ToInt32(webData["weight"].ToString());
			}
			catch
			{
			}
		}
		if (webData["country"] != DBNull.Value)
		{
			try
			{
				Country country = FifaEnvironment.Countries.SearchCountryByDatabaseName(webData["country"].ToString());
				if (country == null)
				{
					country = FifaEnvironment.Countries.SearchCountryByDatabaseName(webData["team"].ToString());
				}
				if (country != null)
				{
					player.Country = country;
				}
			}
			catch
			{
			}
		}
		if (webData["role"].ToString() != string.Empty)
		{
			try
			{
				switch (webData["role"].ToString().ToLower())
				{
				case "goalkeeper":
				case "keeper":
				case "gk":
					player.preferredposition1 = 0;
					break;
				case "defender":
				case "defence - centre-back":
				case "centre-back":
				case "defender - centre-back":
				case "cb":
					player.preferredposition1 = 5;
					break;
				case "left-back":
				case "left - back":
				case "defence - left-back":
				case "defender - left-back":
				case "lb":
				case "lwb":
					player.preferredposition1 = 7;
					break;
				case "defence - right-back":
				case "defender - right-back":
				case "right-back":
				case "rb":
				case "rwb":
					player.preferredposition1 = 3;
					break;
				case "midfielder - defensive midfield":
				case "midfield - defensive midfield":
				case "defensive midfield":
				case "cdm":
					player.preferredposition1 = 10;
					break;
				case "midfield":
				case "midfielder":
				case "midfield - central midfield":
				case "midfielder - central midfield":
				case "central midfield":
				case "cm":
				case "mittelfeld":
					player.preferredposition1 = 14;
					break;
				case "midfielder - right midfield":
				case "midfield - right midfield":
				case "right midfield":
				case "rm":
					player.preferredposition1 = 12;
					break;
				case "midfielder - left midfield":
				case "midfield - left midfield":
				case "left midfield":
				case "lm":
					player.preferredposition1 = 16;
					break;
				case "midfielder - attacking midfield":
				case "midfield - attacking midfield":
				case "attacking midfield":
				case "cam":
					player.preferredposition1 = 18;
					break;
				case "attack - left winger":
				case "striker - left wing":
				case "left wing":
				case "left winger":
				case "lw":
					player.preferredposition1 = 27;
					break;
				case "attack - right winger":
				case "forward - right winger":
				case "striker - right wing":
				case "right wing":
				case "right winger":
				case "rw":
					player.preferredposition1 = 23;
					break;
				case "striker":
				case "striker - centre-forward":
				case "centre-forward":
				case "attack - centre-forward":
				case "forward - centre-forward":
				case "forward":
				case "st":
					player.preferredposition1 = 25;
					break;
				case "forward - second striker":
				case "striker - secondary striker":
				case "attack - second striker":
				case "secondary striker":
				case "second striker":
				case "cf":
					player.preferredposition1 = 21;
					break;
				}
			}
			catch
			{
			}
		}
		if (webData["foot"].ToString() != string.Empty)
		{
			try
			{
				switch (webData["foot"].ToString().ToLower().Trim())
				{
				case "both":
					player.preferredfoot = 1;
					player.weakfootabilitytypecode = 5;
					break;
				case "left":
					player.preferredfoot = 1;
					player.weakfootabilitytypecode = 3;
					break;
				case "right":
				case "-":
					player.preferredfoot = 0;
					player.weakfootabilitytypecode = 3;
					break;
				}
			}
			catch
			{
			}
		}
		int num = 0;
		DateTime now = DateTime.Now;
		if (webData["age"] != DBNull.Value)
		{
			try
			{
				num = Convert.ToInt32(webData["age"].ToString());
			}
			catch
			{
			}
		}
		if (num == 0)
		{
			num = now.Year - player.birthdate.Year;
			if (now < player.birthdate.AddYears(num))
			{
				num--;
			}
		}
		if (webData["since"] != DBNull.Value)
		{
			int month = ((now.Month <= 6) ? 1 : 7);
			DateTime joindate = new DateTime(now.Year, month, 1);
			try
			{
				if (!webData["since"].ToString().Contains('-'))
				{
					joindate = FifaUtil.ConvertToDate(webData["since"].ToString());
				}
			}
			catch
			{
			}
			player.joindate = joindate;
		}
		bool flag6 = true;
		if (webData["contract"] != DBNull.Value && webData["contract"].ToString() != "-")
		{
			try
			{
				DateTime dateTime = FifaUtil.ConvertToDate(webData["contract"].ToString());
				if (dateTime.Year == 1)
				{
					int contractvaliduntil = Convert.ToInt32(webData["contract"].ToString());
					player.contractvaliduntil = contractvaliduntil;
				}
				if (dateTime.Year >= 2020)
				{
					player.contractvaliduntil = dateTime.Year;
				}
				flag6 = false;
			}
			catch
			{
			}
		}
		if (flag6)
		{
			_ = player.joindate;
			player.contractvaliduntil = player.joindate.Year + 2;
			if (num <= 21)
			{
				player.contractvaliduntil += 3;
			}
			else if (num <= 24)
			{
				player.contractvaliduntil += 2;
			}
			else if (num <= 27)
			{
				player.contractvaliduntil++;
			}
			if (player.contractvaliduntil < 2020)
			{
				player.contractvaliduntil = 2020;
			}
		}
		if (webData["previousteam"] != DBNull.Value)
		{
			try
			{
				Team team = FifaEnvironment.Teams.MatchByname(webData["previousteam"].ToString());
				if (team != null && team != importingTeam)
				{
					player.PreviousTeam = team;
				}
				else
				{
					player.PreviousTeam = null;
				}
			}
			catch
			{
			}
		}
		if (webData["loanedfrom"] != DBNull.Value && webData["loanedfrom"].ToString() != string.Empty)
		{
			try
			{
				Team team2 = FifaEnvironment.Teams.MatchByname(webData["loanedfrom"].ToString());
				if (team2 != null)
				{
					player.TeamLoanedFrom = team2;
					player.IsLoaned = true;
				}
			}
			catch
			{
			}
		}
		else if (player.IsLoaned)
		{
			player.TeamLoanedFrom = null;
			player.IsLoaned = false;
		}
		if (webData["loanenddate"] != DBNull.Value && webData["loanenddate"].ToString() != string.Empty)
		{
			try
			{
				DateTime loandateend = FifaUtil.ConvertToDate(webData["loanenddate"].ToString());
				player.loandateend = loandateend;
				if (player.contractvaliduntil <= player.loandateend.Year)
				{
					player.contractvaliduntil++;
				}
			}
			catch
			{
			}
		}
		if (webData["number"].ToString() != string.Empty)
		{
			try
			{
				int preferredNumber = Convert.ToInt32(webData["number"].ToString());
				player.preferredNumber = preferredNumber;
			}
			catch
			{
			}
		}
		if (importingTeam != null)
		{
			if (flag2)
			{
				if (flag4)
				{
					importingTeam.AddTeamPlayer(player, player.preferredNumber);
				}
				else if (flag5)
				{
					importingTeam.Roster.SearchTeamPlayer(player).jerseynumber = player.preferredNumber;
				}
				else if (flag3)
				{
					for (int i = 0; i < player.m_PlayingForTeams.Count; i++)
					{
						Team team3 = (Team)player.m_PlayingForTeams[i];
						if (!team3.IsNationalTeam())
						{
							team3.RemoveTeamPlayer(player);
							i--;
						}
					}
					importingTeam.AddTeamPlayer(player, player.preferredNumber);
				}
			}
			else if (!player.IsPlayingFor(importingTeam))
			{
				int freeNumber = importingTeam.Roster.GetFreeNumber();
				importingTeam.AddTeamPlayer(player, freeNumber);
			}
		}
		bool flag7 = false;
		float num2 = 0f;
		if (webData["marketvalue"] != DBNull.Value && !webData["marketvalue"].ToString().Contains('-'))
		{
			string text2 = webData["marketvalue"].ToString().ToLower();
			string text3 = " ";
			if (text2.Contains("€"))
			{
				text2 = text2.Replace("€", "");
			}
			if (text2.Contains("m"))
			{
				text2 = text2.Replace("m", "");
				text3 = "m";
			}
			if (text2.Contains("th"))
			{
				text2 = text2.Replace("th", "");
				text3 = "t";
			}
			if (text2.Contains("t"))
			{
				text2 = text2.Replace("t", "");
				text3 = "t";
			}
			if (text2.Contains("k"))
			{
				text2 = text2.Replace("k", "");
				text3 = "t";
			}
			if (text2.Contains(" "))
			{
				text2 = text2.Replace(" ", "");
			}
			string text4 = text2;
			try
			{
				num2 = Convert.ToSingle(text4, CultureInfo.InvariantCulture.NumberFormat);
			}
			catch
			{
				text4 = text4.Replace(',', '.');
				if (text4 != null && text4 != string.Empty)
				{
					num2 = Convert.ToSingle(text4, CultureInfo.InvariantCulture.NumberFormat);
				}
			}
			if (text3 == "t")
			{
				num2 /= 1000f;
				flag7 = true;
			}
			else if (text3 == "m")
			{
				flag7 = true;
			}
		}
		if (!flag7)
		{
			num2 = (float)Player.RandomizeNumber(50, 100) * 0.001f;
		}
		if (webData["website"].ToString() == "transfermrkt")
		{
			if (flag)
			{
				int num3 = player.EstimateSkills(num2, num, (ERole)player.preferredposition1);
				num3 = Player.RandomizeNumber(num3 - 2, num3 + 2 + 1);
				player.overallrating = num3;
				player.EstimatePotential(num);
				RFS_SkillsGenerator.RandomizeProfile(player, (ERole)player.preferredposition1);
				if (webData["weight"].ToString() == string.Empty)
				{
					player.RandomizeWeight();
				}
				player.potential = ((player.potential > player.overallrating) ? player.potential : player.overallrating);
			}
		}
		else if (webData["website"].ToString() == "sofifa")
		{
			if (webData["crossing"] != DBNull.Value)
			{
				player.crossing = Convert.ToInt32(webData["crossing"]);
			}
			if (webData["finishing"] != DBNull.Value)
			{
				player.finishing = Convert.ToInt32(webData["finishing"]);
			}
			if (webData["heading"] != DBNull.Value)
			{
				player.headingaccuracy = Convert.ToInt32(webData["heading"]);
			}
			if (webData["shortpassing"] != DBNull.Value)
			{
				player.shortpassing = Convert.ToInt32(webData["shortpassing"]);
			}
			if (webData["volleys"] != DBNull.Value)
			{
				player.volleys = Convert.ToInt32(webData["volleys"]);
			}
			if (webData["dribbling"] != DBNull.Value)
			{
				player.dribbling = Convert.ToInt32(webData["dribbling"]);
			}
			if (webData["curve"] != DBNull.Value)
			{
				player.curve = Convert.ToInt32(webData["curve"]);
			}
			if (webData["fkaccuracy"] != DBNull.Value)
			{
				player.freekickaccuracy = Convert.ToInt32(webData["fkaccuracy"]);
			}
			if (webData["longpassing"] != DBNull.Value)
			{
				player.longpassing = Convert.ToInt32(webData["longpassing"]);
			}
			if (webData["ballcontrol"] != DBNull.Value)
			{
				player.ballcontrol = Convert.ToInt32(webData["ballcontrol"]);
			}
			if (webData["acceleration"] != DBNull.Value)
			{
				player.acceleration = Convert.ToInt32(webData["acceleration"]);
			}
			if (webData["sprintspeed"] != DBNull.Value)
			{
				player.sprintspeed = Convert.ToInt32(webData["sprintspeed"]);
			}
			if (webData["agility"] != DBNull.Value)
			{
				player.agility = Convert.ToInt32(webData["agility"]);
			}
			if (webData["reactions"] != DBNull.Value)
			{
				player.reactions = Convert.ToInt32(webData["reactions"]);
			}
			if (webData["balance"] != DBNull.Value)
			{
				player.balance = Convert.ToInt32(webData["balance"]);
			}
			if (webData["shotpower"] != DBNull.Value)
			{
				player.shotpower = Convert.ToInt32(webData["shotpower"]);
			}
			if (webData["jumping"] != DBNull.Value)
			{
				player.jumping = Convert.ToInt32(webData["jumping"]);
			}
			if (webData["stamina"] != DBNull.Value)
			{
				player.stamina = Convert.ToInt32(webData["stamina"]);
			}
			if (webData["strength"] != DBNull.Value)
			{
				player.strength = Convert.ToInt32(webData["strength"]);
			}
			if (webData["longshots"] != DBNull.Value)
			{
				player.longshots = Convert.ToInt32(webData["longshots"]);
			}
			if (webData["aggression"] != DBNull.Value)
			{
				player.aggression = Convert.ToInt32(webData["aggression"]);
			}
			if (webData["interceptions"] != DBNull.Value)
			{
				player.interceptions = Convert.ToInt32(webData["interceptions"]);
			}
			if (webData["positioning"] != DBNull.Value)
			{
				player.positioning = Convert.ToInt32(webData["positioning"]);
			}
			if (webData["vision"] != DBNull.Value)
			{
				player.vision = Convert.ToInt32(webData["vision"]);
			}
			if (webData["penalties"] != DBNull.Value)
			{
				player.penalties = Convert.ToInt32(webData["penalties"]);
			}
			if (webData["standingtackle"] != DBNull.Value)
			{
				player.standingtackle = Convert.ToInt32(webData["standingtackle"]);
			}
			if (webData["slidingtackle"] != DBNull.Value)
			{
				player.slidingtackle = Convert.ToInt32(webData["slidingtackle"]);
			}
			if (webData["marking"] != DBNull.Value)
			{
				player.marking = Convert.ToInt32(webData["marking"]);
			}
			if (webData["gkdiving"] != DBNull.Value)
			{
				player.gkdiving = Convert.ToInt32(webData["gkdiving"]);
			}
			if (webData["gkhandling"] != DBNull.Value)
			{
				player.gkhandling = Convert.ToInt32(webData["gkhandling"]);
			}
			if (webData["gkkicking"] != DBNull.Value)
			{
				player.gkkicking = Convert.ToInt32(webData["gkkicking"]);
			}
			if (webData["gkpositioning"] != DBNull.Value)
			{
				player.gkpositioning = Convert.ToInt32(webData["gkpositioning"]);
			}
			if (webData["gkreflexes"] != DBNull.Value)
			{
				player.gkreflexes = Convert.ToInt32(webData["gkreflexes"]);
			}
			if (webData["potential"] != DBNull.Value)
			{
				player.potential = Convert.ToInt32(webData["potential"]);
			}
			if (webData["overall"] != DBNull.Value)
			{
				player.overallrating = Convert.ToInt32(webData["overall"]);
			}
			if (webData["weakfoot"] != DBNull.Value)
			{
				player.weakfootabilitytypecode = Convert.ToInt32(webData["weakfoot"].ToString());
			}
			if (webData["skillmoves"] != DBNull.Value)
			{
				player.skillmoves = Convert.ToInt32(webData["skillmoves"].ToString()) - 1;
			}
		}
		if (flag)
		{
			Player player2 = FifaEnvironment.Players.FindSimilarPlayer(player.Country, player.birthdate);
			if (player2 != null)
			{
				player.RandomizeAppearanceSimilarTo(player2);
			}
			player.jerseystylecode = 1;
		}
		m_Imported = true;
		return player;
	}

	private int EstimateSkills(int age, int marketValue, ERole role)
	{
		if (age > 19 && age > 22 && age > 25 && age > 27 && age > 30)
		{
			_ = 33;
		}
		return 60;
	}

	public Team ImportWebTeam(DataRow webData)
	{
		Team team = (Team)GetUsedObject();
		bool num = IsObjectUsedNew();
		team.DatabaseName = webData["name"].ToString();
		if (num)
		{
			if (webData["name"].ToString() != string.Empty)
			{
				team.TeamNameFull = team.DatabaseName;
				team.SetNameAutomatically(team.TeamNameFull, 15);
				team.SetNameAutomatically(team.TeamNameAbbr15, 10);
				team.SetNameAutomatically(team.TeamNameAbbr10, 7);
				team.SetNameAutomatically(team.TeamNameAbbr7, 3);
			}
			team.Formation = FifaEnvironment.Formations.CreateNewFormation();
			team.Formation.Team = team;
		}
		if (webData["stadium"].ToString() != string.Empty)
		{
			try
			{
				team.stadiumcustomname = webData["stadium"].ToString();
			}
			catch
			{
			}
		}
		if (webData["totalmarketvalue"].ToString() != string.Empty)
		{
			string text = webData["totalmarketvalue"].ToString();
			string text2 = text.Substring(text.Length - 1, 1);
			float num2 = 1f;
			string text3 = text.Substring(0, text.Length - 1);
			switch (text2)
			{
			case "M":
			case "m":
				num2 = 1000f;
				break;
			case "B":
			case "b":
				num2 = 1000000f;
				break;
			default:
				num2 = 1f;
				text3 = text3.Substring(0, text3.Length - 2);
				break;
			}
			text3 = text3.Replace(',', '.');
			float num3 = 0f;
			if (text3 != string.Empty)
			{
				num3 = Convert.ToSingle(text3, CultureInfo.InvariantCulture);
				num3 *= num2;
			}
			if (num3 != 0f)
			{
				team.transferbudget = Convert.ToInt32(num3 / 10f);
			}
		}
		m_Imported = true;
		return team;
	}

	public void Import()
	{
		if (m_Type == "Player")
		{
			ImportPlayer();
		}
		else if (m_Type == "Team")
		{
			ImportTeam();
		}
		else if (m_Type == "League")
		{
			ImportLeague();
		}
		else if (m_Type == "Country")
		{
			ImportCountry();
		}
		else if (m_Type == "Stadium")
		{
			ImportStadium();
		}
		else if (m_Type == "Referee")
		{
			ImportReferee();
		}
		else if (m_Type == "Formation")
		{
			ImportFormation();
		}
		else if (m_Type == "Ball")
		{
			ImportBall();
		}
		else if (m_Type == "Adboard")
		{
			ImportAdboard();
		}
		else if (m_Type == "NumberFont")
		{
			ImportNumberFont();
		}
		else if (m_Type == "NameFont")
		{
			ImportNameFont();
		}
		else if (m_Type == "Shoes")
		{
			ImportShoes();
		}
		else if (m_Type == "Net")
		{
			ImportNet();
		}
		else if (m_Type == "MowingPattern")
		{
			ImportMowingPattern();
		}
		else if (m_Type == "Kit")
		{
			ImportKit();
		}
		else if (m_Type == "GkGloves")
		{
			ImportGkGloves();
		}
	}

	private void ImportKit()
	{
		Kit kit = (Kit)GetUsedObject();
		if (kit == null)
		{
			return;
		}
		int num = m_Id / 10;
		int num2 = m_Id - 10 * num;
		if (MainForm.m_PatchLoaderForm.checkKits.Checked)
		{
			if (s_TeamkitsTable != null)
			{
				Record[] records = s_TeamkitsTable.Records;
				foreach (Record record in records)
				{
					if (record.IntField[FI.teamkits_teamtechid] == num && record.IntField[FI.teamkits_teamkittypetechid] == num2)
					{
						kit.Load(record);
						kit.Id = m_ImportId;
						if (s_TeamCrossReferenceRequired)
						{
							kit.teamid = MainForm.m_PatchLoaderForm.CrossReference("Team", num);
						}
						kit.LinkTeam(FifaEnvironment.Teams);
						if (kit.Team != null)
						{
							kit.Team.m_KitList.Add(kit);
							kit.Team.LinkKits(FifaEnvironment.Kits);
						}
						break;
					}
				}
			}
			string text = Kit.KitTextureFileName(num, kit.kittype, 0);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				kit.SetKitTextures(text);
			}
		}
		if (MainForm.m_PatchLoaderForm.checkMinikits.Checked)
		{
			string text2 = Kit.MiniKitDdsFileName(num, kit.kittype, 0);
			text2 = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text2;
			if (File.Exists(text2))
			{
				Bitmap bitmapFromDdsFile = FifaEnvironment.GetBitmapFromDdsFile(text2);
				kit.SetMiniKit(bitmapFromDdsFile);
			}
		}
		m_Imported = true;
	}

	private void ImportStadium()
	{
		Stadium stadium = (Stadium)GetUsedObject();
		if (stadium == null)
		{
			return;
		}
		if (MainForm.m_PatchLoaderForm.checkStadiumDatabase.Checked)
		{
			if (s_StadiumsTable != null)
			{
				Record[] records = s_StadiumsTable.Records;
				foreach (Record record in records)
				{
					if (record.IntField[FI.stadiums_stadiumid] == m_Id)
					{
						stadium.Load(record);
						stadium.LocalName = stadium.name;
						if (s_CountryCrossReferenceRequired)
						{
							stadium.countrycode = MainForm.m_PatchLoaderForm.CrossReference("Country", record.IntField[FI.stadiums_countrycode]);
						}
						stadium.LinkCountry(FifaEnvironment.Countries);
						if (s_TeamCrossReferenceRequired)
						{
							stadium.hometeamid = MainForm.m_PatchLoaderForm.CrossReference("Team", stadium.hometeamid);
						}
						stadium.LinkTeam(FifaEnvironment.Teams);
						break;
					}
				}
			}
			m_Imported = true;
		}
		if (MainForm.m_PatchLoaderForm.checkStadiumPreview.Checked)
		{
			for (int j = 0; j <= 4; j++)
			{
				if (j != 2)
				{
					string text = Stadium.PreviewBigFileName(m_Id, j);
					text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
					if (File.Exists(text))
					{
						Bitmap bitmapFromBigFile = FifaEnvironment.GetBitmapFromBigFile(text);
						stadium.SetPreview(j, bitmapFromBigFile);
					}
					text = Stadium.PreviewLargeBigFileName(m_Id, j);
					text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
					if (File.Exists(text))
					{
						Bitmap bitmapFromBigFile2 = FifaEnvironment.GetBitmapFromBigFile(text);
						stadium.SetPreviewLarge(j, bitmapFromBigFile2);
					}
				}
			}
		}
		if (!MainForm.m_PatchLoaderForm.checkStadiumModel.Checked)
		{
			return;
		}
		string text2 = Stadium.ModelFileName(m_Id);
		text2 = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text2;
		if (File.Exists(text2))
		{
			stadium.SetModel(text2);
		}
		text2 = Stadium.RadiosityFileName(m_Id);
		text2 = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text2;
		if (File.Exists(text2))
		{
			stadium.SetRadiosity(text2);
		}
		for (int k = 0; k <= 4; k++)
		{
			if (k != 2)
			{
				text2 = Stadium.TexturesFileName(m_Id, k);
				text2 = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text2;
				if (File.Exists(text2))
				{
					stadium.SetTextures(k, text2);
				}
				text2 = Stadium.CrowdFileName(m_Id, k);
				text2 = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text2;
				if (File.Exists(text2))
				{
					stadium.SetCrowd(k, text2);
				}
				string[] array = Stadium.GlaresLightFileNames(m_Id, k);
				for (int l = 0; l < array.Length; l++)
				{
					array[l] = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + array[l];
				}
				stadium.SetGlaresLight(array, k);
			}
		}
	}

	private void ImportPlayer()
	{
		Player player = (Player)GetUsedObject();
		if (player == null)
		{
			return;
		}
		if (s_PlayersTable != null)
		{
			if (s_PlayerCount >= s_PlayersTable.Records.Length)
			{
				s_PlayerCount = 0;
			}
			int num = s_PlayerCount;
			for (int i = 0; i < s_PlayersTable.Records.Length; i++)
			{
				Record record = s_PlayersTable.Records[num];
				if (record.IntField[FI.players_playerid] == m_Id)
				{
					s_PlayerCount = num + 1;
					player.Load(record);
					if (s_PlayerNames != null)
					{
						player.firstname = (s_PlayerNames.TryGetValue(player.firstnameid, out var name, isUsed: true) ? name : string.Empty);
						player.lastname = (s_PlayerNames.TryGetValue(player.lastnameid, out name, isUsed: true) ? name : string.Empty);
						player.commonname = (s_PlayerNames.TryGetValue(player.commonnameid, out name, isUsed: true) ? name : string.Empty);
						player.playerjerseyname = (s_PlayerNames.TryGetValue(player.playerjerseynameid, out name, isUsed: true) ? name : string.Empty);
					}
					else
					{
						player.firstname = string.Empty;
						player.lastname = "Player " + player.Id;
						player.commonname = string.Empty;
						player.playerjerseyname = string.Empty;
					}
					if (s_ShoesCrossReferenceRequired)
					{
						player.shoetypecode = MainForm.m_PatchLoaderForm.CrossReference("Shoes", player.shoetypecode);
					}
					if (s_CountryCrossReferenceRequired)
					{
						player.nationality = MainForm.m_PatchLoaderForm.CrossReference("Country", player.nationality);
					}
					player.LinkCountry(FifaEnvironment.Countries);
					player.IsLoaned = false;
					player.FillFromPlayerloans(s_PlayersLoanTable);
					player.FillFromPreviousTeam(s_PreviousTeamTable);
					player.LinkTeam(FifaEnvironment.Teams);
					break;
				}
				num++;
				if (num == s_PlayersTable.Records.Length)
				{
					num = 0;
				}
			}
		}
		if (MainForm.m_PatchLoaderForm.checkPlayerHead.Checked)
		{
			string text;
			if (FifaEnvironment.Year == 14)
			{
				text = Player.SpecificEyesTextureFileName(m_Id);
				text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
				if (File.Exists(text))
				{
					player.SetEyesTextures(text);
				}
			}
			text = Player.SpecificFaceTextureFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				player.SetFaceTextures(text);
				if (FifaEnvironment.Year == 15)
				{
					player.ConvertFaceTexturesFrom15To16();
				}
			}
			text = Player.SpecificHeadModelFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				player.SetHeadModel(text);
			}
			text = Player.SpecificHairTexturesFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				player.SetHairTextures(text);
			}
			text = Player.SpecificHairModelFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				player.SetHairModel(text);
			}
			text = Player.SpecificHairLodModelFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				player.SetHairLodModel(text);
			}
		}
		if (MainForm.m_PatchLoaderForm.checkPlayerMiniface.Checked)
		{
			string text = Player.SpecificPhotoDdsFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile = FifaEnvironment.GetBitmapFromDdsFile(text);
				player.SetPhoto(bitmapFromDdsFile);
			}
		}
		m_Imported = true;
	}

	private void ImportTeam()
	{
		Team team = (Team)GetUsedObject();
		if (team == null)
		{
			return;
		}
		if (MainForm.m_PatchLoaderForm.checkTeamDatabase.Checked)
		{
			if (s_TeamsTable != null)
			{
				Record[] records = s_TeamsTable.Records;
				foreach (Record record in records)
				{
					if (record.IntField[FI.teams_teamid] != m_Id)
					{
						continue;
					}
					team.Load(record);
					if (s_BallCrossReferenceRequired)
					{
						team.balltype = MainForm.m_PatchLoaderForm.CrossReference("Ball", team.balltype);
					}
					if (s_AdboardCrossReferenceRequired)
					{
						team.adboardid = MainForm.m_PatchLoaderForm.CrossReference("Adboard", team.adboardid);
					}
					if (s_TeamCrossReferenceRequired)
					{
						team.rivalteam = MainForm.m_PatchLoaderForm.CrossReference("Team", team.rivalteam);
					}
					team.LinkTeam(FifaEnvironment.Teams);
					if (s_PlayerCrossReferenceRequired)
					{
						team.captainid = MainForm.m_PatchLoaderForm.CrossReference("Player", team.captainid);
						team.penaltytakerid = MainForm.m_PatchLoaderForm.CrossReference("Player", team.penaltytakerid);
						team.freekicktakerid = MainForm.m_PatchLoaderForm.CrossReference("Player", team.freekicktakerid);
						team.longkicktakerid = MainForm.m_PatchLoaderForm.CrossReference("Player", team.longkicktakerid);
						team.leftcornerkicktakerid = MainForm.m_PatchLoaderForm.CrossReference("Player", team.leftcornerkicktakerid);
						team.rightcornerkicktakerid = MainForm.m_PatchLoaderForm.CrossReference("Player", team.rightcornerkicktakerid);
						if (FifaEnvironment.Year > 14)
						{
							team.leftfreekicktakerid = MainForm.m_PatchLoaderForm.CrossReference("Player", team.leftfreekicktakerid);
							team.rightcornerkicktakerid = MainForm.m_PatchLoaderForm.CrossReference("Player", team.rightcornerkicktakerid);
						}
					}
					break;
				}
			}
			if (s_TeamstadiumlinksTable != null)
			{
				Record[] records = s_TeamstadiumlinksTable.Records;
				foreach (Record record2 in records)
				{
					if (record2.IntField[FI.teamstadiumlinks_teamid] == m_Id)
					{
						int id = record2.IntField[FI.teamstadiumlinks_stadiumid];
						if (FifaEnvironment.Stadiums.SearchId(id) != null)
						{
							team.FillFromTeamStadiumLinks(record2);
							team.stadiumid = MainForm.m_PatchLoaderForm.CrossReference("Stadium", record2.IntField[FI.teamstadiumlinks_stadiumid]);
							team.LinkStadium(FifaEnvironment.Stadiums);
						}
						break;
					}
				}
			}
			if (s_StadiumassignmentsTable != null)
			{
				Record[] records = s_StadiumassignmentsTable.Records;
				foreach (Record record3 in records)
				{
					if (record3.IntField[FI.stadiumassignments_teamid] == m_Id)
					{
						team.FillFromStadiumAssignments(record3);
						break;
					}
				}
			}
			if (s_ManagerTable != null)
			{
				Record[] records = s_ManagerTable.Records;
				foreach (Record record4 in records)
				{
					if (record4.IntField[FI.manager_teamid] == m_Id)
					{
						team.FillFromManager(record4);
						break;
					}
				}
			}
			if (s_TeamformationteamstylelinkTable != null)
			{
				Record[] records = s_TeamformationteamstylelinkTable.Records;
				foreach (Record record5 in records)
				{
					if (record5.IntField[FI.teamformationteamstylelinks_teamid] == m_Id)
					{
						team.FillFromTeamFormationLinks(record5);
						team.formationid = MainForm.m_PatchLoaderForm.CrossReference("Formation", record5.IntField[FI.teamformationteamstylelinks_formationid]);
						team.LinkFormation(FifaEnvironment.Formations);
						break;
					}
				}
			}
			if (s_TeamplayerlinksTable != null)
			{
				team.Roster.ResetToEmpty();
				Record[] records = s_TeamplayerlinksTable.Records;
				foreach (Record record6 in records)
				{
					if (record6.IntField[FI.teamplayerlinks_teamid] != m_Id)
					{
						continue;
					}
					int id2 = record6.IntField[FI.teamplayerlinks_playerid];
					if (s_PlayerCrossReferenceRequired)
					{
						id2 = MainForm.m_PatchLoaderForm.CrossReference("Player", id2);
					}
					Player player = (Player)FifaEnvironment.Players.SearchId(id2);
					if (player != null)
					{
						player.PlayFor(team);
						TeamPlayer teamPlayer = new TeamPlayer(record6, player, team);
						if (teamPlayer != null)
						{
							team.Roster.Add(teamPlayer);
						}
						team.LinkPlayer(FifaEnvironment.Players);
					}
				}
				if (team.IsClub())
				{
					for (int j = 0; j < team.Roster.Count; j++)
					{
						Player player2 = ((TeamPlayer)team.Roster[j]).Player;
						bool flag = false;
						foreach (Team playingForTeam in player2.m_PlayingForTeams)
						{
							if (playingForTeam.IsClub() && playingForTeam != team)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
						for (int k = 0; k < player2.m_PlayingForTeams.Count; k++)
						{
							Team team3 = (Team)player2.m_PlayingForTeams[k];
							if (team3.IsClub() && team3 != team)
							{
								if (MainForm.m_PatchLoaderForm.radioTransferToNewTeam.Checked)
								{
									team3.RemoveTeamPlayer(player2);
									break;
								}
								if (MainForm.m_PatchLoaderForm.radioLeaveInExistingTeam.Checked)
								{
									team.RemoveTeamPlayer(player2);
									j--;
								}
							}
						}
					}
				}
			}
			if (s_TeamNationLinksTable != null)
			{
				Record[] records = s_TeamNationLinksTable.Records;
				foreach (Record record7 in records)
				{
					if (record7.IntField[FI.teamnationlinks_teamid] == m_Id)
					{
						team.FillFromTeamNationLinks(record7);
						if (s_TeamCrossReferenceRequired)
						{
							team.m_countryid_IfNationalTeam = MainForm.m_PatchLoaderForm.CrossReference("Team", team.m_countryid_IfNationalTeam);
						}
						team.LinkCountry(FifaEnvironment.Countries);
						break;
					}
				}
			}
			if (s_RowteamnationlinksTable != null)
			{
				Record[] records = s_RowteamnationlinksTable.Records;
				foreach (Record record8 in records)
				{
					if (record8.IntField[FI.rowteamnationlinks_teamid] == m_Id)
					{
						team.FillFromRowTeamNationLinks(record8);
						if (s_TeamCrossReferenceRequired)
						{
							team.m_countryid_IfRowTeam = MainForm.m_PatchLoaderForm.CrossReference("Team", team.m_countryid_IfRowTeam);
						}
						if (team.League == null)
						{
							team.LinkCountry(FifaEnvironment.Countries);
							League.GetDefaultLeague().AddTeam(team);
						}
						break;
					}
				}
			}
			if (team.Country == null)
			{
				_ = team.League;
			}
			if (s_Language != null)
			{
				team.TeamNameFull = s_Language.GetTeamString(m_Id, Language.ETeamStringType.Full);
				team.TeamNameAbbr15 = s_Language.GetTeamString(m_Id, Language.ETeamStringType.Abbr15);
				team.TeamNameAbbr10 = s_Language.GetTeamString(m_Id, Language.ETeamStringType.Abbr10);
				team.TeamNameAbbr7 = s_Language.GetTeamString(m_Id, Language.ETeamStringType.Abbr7);
				team.TeamNameAbbr3 = s_Language.GetTeamString(m_Id, Language.ETeamStringType.Abbr3);
			}
			team.SetNameAutomatically(team.TeamNameFull, 15);
			team.SetNameAutomatically(team.TeamNameAbbr15, 10);
			team.SetNameAutomatically(team.TeamNameAbbr10, 7);
			team.SetNameAutomatically(team.TeamNameAbbr7, 3);
		}
		if (MainForm.m_PatchLoaderForm.checkTeamLogo.Checked)
		{
			string text = Team.CrestDdsFileName(m_Id, MainForm.m_PatchLoaderForm.PatchYear);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile = FifaEnvironment.GetBitmapFromDdsFile(text);
				team.SetCrest(bitmapFromDdsFile);
				team.SetCrestDark(bitmapFromDdsFile);
			}
			text = Team.Crest50DdsFileName(m_Id, MainForm.m_PatchLoaderForm.PatchYear);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile2 = FifaEnvironment.GetBitmapFromDdsFile(text);
				team.SetCrest50(bitmapFromDdsFile2);
				team.SetCrest50Dark(bitmapFromDdsFile2);
			}
			text = Team.Crest32DdsFileName(m_Id, MainForm.m_PatchLoaderForm.PatchYear);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile3 = FifaEnvironment.GetBitmapFromDdsFile(text);
				team.SetCrest32(bitmapFromDdsFile3);
				team.SetCrest32Dark(bitmapFromDdsFile3);
			}
			text = Team.Crest16DdsFileName(m_Id, MainForm.m_PatchLoaderForm.PatchYear);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile4 = FifaEnvironment.GetBitmapFromDdsFile(text);
				team.SetCrest16(bitmapFromDdsFile4);
				team.SetCrest16Dark(bitmapFromDdsFile4);
			}
		}
		if (MainForm.m_PatchLoaderForm.checkTeamBanner.Checked)
		{
			string text = Team.BannerFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				team.SetBanner(text);
			}
		}
		if (MainForm.m_PatchLoaderForm.checkTeamFlags.Checked)
		{
			string text = Team.FlagFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				team.SetFlags(text);
			}
		}
		if (MainForm.m_PatchLoaderForm.checkTeamFlags.Checked)
		{
			string text = Team.ScarfFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				team.SetScarfs(text);
			}
			text = Team.RevModAdboardFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				team.SetRevModAdboard(text);
			}
			text = Team.RevModBallModelFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				team.SetRevModBallModel(text);
			}
			text = Team.RevModBallTextureFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				team.SetRevModBallTextures(text);
			}
			text = Team.RevModNetFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				team.SetRevModNet(text);
			}
			text = Team.RevModManagerModleFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				team.SetRevModManagerModel(text);
			}
			text = Team.RevModManagerTextureFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				team.SetRevModManagerTexture(text);
			}
		}
		m_Imported = true;
	}

	public int ConvertFormationTo08(int id07)
	{
		if (id07 <= 20)
		{
			int num = c_FormationSwitchTable[id07];
			if (FifaEnvironment.Formations.SearchId(num) != null)
			{
				return num;
			}
		}
		return FifaEnvironment.Formations.GetNewId();
	}

	private void ImportLeague()
	{
		League league = (League)GetUsedObject();
		if (league == null)
		{
			return;
		}
		if (MainForm.m_PatchLoaderForm.checkLeagueDatabase.Checked)
		{
			if (s_LeaguesTable != null)
			{
				Record[] records = s_LeaguesTable.Records;
				foreach (Record record in records)
				{
					if (record.IntField[FI.leagues_leagueid] != m_Id)
					{
						continue;
					}
					league.Load(record);
					if (s_CountryCrossReferenceRequired)
					{
						league.countryid = MainForm.m_PatchLoaderForm.CrossReference("Country", record.IntField[FI.leagues_countryid]);
					}
					league.LinkCountry(FifaEnvironment.Countries);
					if (s_Language != null)
					{
						league.ShortName = s_Language.GetLeagueString(m_Id, Language.ELeagueStringType.Abbr15);
						league.LongName = s_Language.GetLeagueString(m_Id, Language.ELeagueStringType.Full);
						if (league.LongName == null)
						{
							league.LongName = league.ShortName;
						}
						if (league.ShortName == null)
						{
							league.ShortName = league.LongName;
						}
					}
					if (league.LongName == null)
					{
						league.LongName = league.leaguename;
					}
					if (league.ShortName == null)
					{
						league.ShortName = league.leaguename;
					}
					break;
				}
			}
			if (s_BoardOutcomesTable != null)
			{
				Record[] records = s_BoardOutcomesTable.Records;
				foreach (Record record2 in records)
				{
					if (record2.IntField[FI.career_boardoutcomes_leagueid] == m_Id)
					{
						league.FillFromBoardOutcomes(record2);
					}
				}
			}
			if (s_LeagueteamlinksTable != null)
			{
				league.PlayingTeams.Clear();
				Record[] records = s_LeagueteamlinksTable.Records;
				foreach (Record record3 in records)
				{
					if (record3.IntField[FI.leagueteamlinks_leagueid] != m_Id)
					{
						continue;
					}
					int id = record3.IntField[FI.leagueteamlinks_teamid];
					if (s_TeamCrossReferenceRequired)
					{
						id = MainForm.m_PatchLoaderForm.CrossReference("Team", id);
					}
					Team team = (Team)FifaEnvironment.Teams.SearchId(id);
					if (team != null)
					{
						team.FillFromLeagueTeamLinks(record3);
						league.PlayingTeams.InsertId(team);
						if (team.League != null && team.League != league)
						{
							team.League.RemoveTeam(team);
						}
						team.League = league;
						if (team.Country == null)
						{
							team.Country = league.Country;
						}
						team.LinkLeague(FifaEnvironment.Leagues);
					}
				}
			}
		}
		if (MainForm.m_PatchLoaderForm.checkLeagueLogo.Checked)
		{
			string text = League.TinyLogoDdsFileName(m_Id, MainForm.m_PatchLoaderForm.PatchYear);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile = FifaEnvironment.GetBitmapFromDdsFile(text);
				league.SetTinyLogo(bitmapFromDdsFile);
				league.SetTinyLogoDark(bitmapFromDdsFile);
			}
			text = League.AnimLogoDdsFileName(m_Id, MainForm.m_PatchLoaderForm.PatchYear);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile2 = FifaEnvironment.GetBitmapFromDdsFile(text);
				league.SetAnimLogo(bitmapFromDdsFile2);
				league.SetAnimLogoDark(bitmapFromDdsFile2);
			}
			text = League.SmallLogoDdsFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile3 = FifaEnvironment.GetBitmapFromDdsFile(text);
				league.SetSmallLogo(bitmapFromDdsFile3);
				league.SetSmallLogoDark(bitmapFromDdsFile3);
			}
			text = League.Logo512x128DdsFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile4 = FifaEnvironment.GetBitmapFromDdsFile(text);
				league.SetLogo512x128(bitmapFromDdsFile4);
				league.SetLogo512x128Dark(bitmapFromDdsFile4);
			}
		}
		m_Imported = true;
	}

	private void ImportCountry()
	{
		Country country = (Country)GetUsedObject();
		if (country == null)
		{
			return;
		}
		if (MainForm.m_PatchLoaderForm.checkCountryDatabase.Checked)
		{
			if (s_NationsTable != null)
			{
				Record[] records = s_NationsTable.Records;
				foreach (Record record in records)
				{
					if (record.IntField[FI.nations_nationid] == m_Id)
					{
						country.Load(record);
						if (s_TeamCrossReferenceRequired)
						{
							country.NationalTeamId = MainForm.m_PatchLoaderForm.CrossReference("Team", country.NationalTeamId);
						}
						country.LinkTeam(FifaEnvironment.Teams);
						break;
					}
				}
				if (s_Language != null)
				{
					string countryString = s_Language.GetCountryString(m_Id, Language.ECountryStringType.Full);
					if (countryString != null)
					{
						country.LanguageName = countryString;
					}
				}
			}
			m_Imported = true;
		}
		if (MainForm.m_PatchLoaderForm.checkCountryFlag.Checked)
		{
			string text = Country.FlagBigFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromBigFile = FifaEnvironment.GetBitmapFromBigFile(text);
				country.SetFlag(bitmapFromBigFile);
			}
			text = Country.MiniFlagBigFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromBigFile2 = FifaEnvironment.GetBitmapFromBigFile(text);
				country.SetMiniFlag(bitmapFromBigFile2);
			}
			text = Country.CardFlagBigFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromBigFile3 = FifaEnvironment.GetBitmapFromBigFile(text);
				country.SetCardFlag(bitmapFromBigFile3);
			}
			text = Country.Flag512DdsFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile = FifaEnvironment.GetBitmapFromDdsFile(text);
				country.SetFlag512(bitmapFromDdsFile);
			}
		}
		if (MainForm.m_PatchLoaderForm.checkCountryMap.Checked)
		{
			string text = Country.ShapeFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Bitmap bitmapFromDdsFile2 = FifaEnvironment.GetBitmapFromDdsFile(text);
				country.SetShape(bitmapFromDdsFile2);
			}
		}
		m_Imported = true;
	}

	private void ImportReferee()
	{
		Referee referee = (Referee)GetUsedObject();
		if (referee == null)
		{
			return;
		}
		if (s_RefereeTable != null)
		{
			Record[] records = s_RefereeTable.Records;
			foreach (Record record in records)
			{
				if (record.IntField[FI.referee_refereeid] == m_Id)
				{
					referee.Load(record);
					referee.LinkLeague(FifaEnvironment.Leagues);
					if (s_CountryCrossReferenceRequired)
					{
						referee.nationalitycode = MainForm.m_PatchLoaderForm.CrossReference("Country", referee.nationalitycode);
					}
					referee.LinkCountry(FifaEnvironment.Countries);
					break;
				}
				if (FifaEnvironment.Year == 14 && MainForm.m_PatchLoaderForm.PatchYear == 14)
				{
					string text = Referee.PhotoBigFileName(m_Id);
					text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
					if (File.Exists(text))
					{
						Bitmap bitmapFromBigFile = FifaEnvironment.GetBitmapFromBigFile(text);
						referee.SetPhoto(bitmapFromBigFile);
					}
				}
			}
		}
		m_Imported = true;
	}

	private void ImportFormation()
	{
		Formation formation = (Formation)GetUsedObject();
		if (formation == null)
		{
			return;
		}
		if (s_FormationsTable != null)
		{
			Record[] records = s_FormationsTable.Records;
			foreach (Record record in records)
			{
				if (record.IntField[FI.formations_formationid] != m_Id)
				{
					continue;
				}
				formation.Load(record);
				if (formation.teamid != -1)
				{
					if (s_TeamCrossReferenceRequired)
					{
						formation.teamid = MainForm.m_PatchLoaderForm.CrossReference("Team", formation.teamid);
					}
					formation.LinkTeam(FifaEnvironment.Teams);
					if (formation.Team != null)
					{
						formation.Team.formationid = formation.Id;
						formation.Team.LinkFormation(FifaEnvironment.Formations);
					}
				}
				formation.LinkRoles(FifaEnvironment.Roles);
				break;
			}
		}
		m_Imported = true;
	}

	private void ImportBall()
	{
		Ball ball = (Ball)GetUsedObject();
		if (ball == null)
		{
			return;
		}
		string text = Ball.BallTextureFileName(m_Id);
		text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
		if (File.Exists(text))
		{
			ball.SetBallTextures(text);
		}
		text = Ball.BallModelFileName(m_Id);
		text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
		if (File.Exists(text))
		{
			ball.SetBallModel(text);
		}
		Bitmap bitmap = null;
		if (MainForm.m_PatchLoaderForm.PatchYear == 14)
		{
			text = Ball.BallPictureBigFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				bitmap = FifaEnvironment.GetBitmapFromBigFile(text);
			}
		}
		else
		{
			text = Ball.BallDdsFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				bitmap = FifaEnvironment.GetBitmapFromDdsFile(text);
			}
		}
		if (bitmap != null)
		{
			ball.SetBallPicture(bitmap);
		}
		if (s_Language != null)
		{
			ball.Name = s_Language.GetBallName(m_Id, out var _);
		}
		m_Imported = true;
	}

	private void ImportAdboard()
	{
		Adboard adboard = (Adboard)GetUsedObject();
		if (adboard != null)
		{
			string text = Adboard.AdboardFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Adboard.SetAdboard(adboard.Id, text);
			}
			m_Imported = true;
		}
	}

	private void ImportNumberFont()
	{
		if ((NumberFont)GetUsedObject() != null)
		{
			int num = m_Id / 20;
			int colorId = m_Id - 20 * num;
			string text = NumberFont.NumberFontFileName(num, colorId);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				num = m_ImportId / 20;
				colorId = m_ImportId - 20 * num;
				NumberFont.SetNumberFont(num, colorId, text);
			}
			m_Imported = true;
		}
	}

	private void ImportNameFont()
	{
		if ((NameFont)GetUsedObject() != null)
		{
			string text = NameFont.NameFontFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				NameFont.Import(m_ImportId, text);
			}
			m_Imported = true;
		}
	}

	private void ImportShoes()
	{
		if ((Shoes)GetUsedObject() != null)
		{
			string text = Shoes.ShoesTexturesFileName(m_Id, 0);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Shoes.SetShoesTextures(m_ImportId, 0, text);
			}
			text = Shoes.ShoesModelFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Shoes.SetShoesModel(m_ImportId, text);
			}
			m_Imported = true;
		}
	}

	private void ImportNet()
	{
		if ((Net)GetUsedObject() != null)
		{
			string text = Net.NetFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				Net.SetNet(m_ImportId, text);
			}
			m_Imported = true;
		}
	}

	private void ImportGkGloves()
	{
		if ((GkGloves)GetUsedObject() != null)
		{
			string text = GkGloves.GkGlovesTextureFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				GkGloves.SetGkGlovesTextures(m_ImportId, text);
			}
			m_Imported = true;
		}
	}

	private void ImportMowingPattern()
	{
		if ((MowingPattern)GetUsedObject() != null)
		{
			string text = MowingPattern.MowingPatternFileName(m_Id);
			text = MainForm.m_PatchLoaderForm.m_TempFolder + "\\" + text;
			if (File.Exists(text))
			{
				MowingPattern.SetMowingPattern(m_ImportId, text);
			}
			m_Imported = true;
		}
	}
}
