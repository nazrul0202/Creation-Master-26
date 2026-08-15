using System.IO;

namespace FifaLibrary;

public class StandingTableEntry
{
	public int index1;

	public int isUsed;

	public int compId;

	public int teamId;

	public int n;

	public int homeWin;

	public int homeDraw;

	public int homeLost;

	public int homeGoalsFor;

	public int homeGoalsAgainst;

	public int awayWin;

	public int awayDraw;

	public int awayLost;

	public int awayGoalsFor;

	public int awayGoalsAgainst;

	public int points;

	public StandingTableEntry()
	{
		Clean();
	}

	public static StandingTableEntry Read(BinaryReader br)
	{
		StandingTableEntry standingTableEntry = new StandingTableEntry();
		standingTableEntry.index1 = br.ReadInt16();
		standingTableEntry.isUsed = br.ReadByte();
		standingTableEntry.compId = br.ReadInt16();
		standingTableEntry.teamId = br.ReadInt32();
		standingTableEntry.n = br.ReadByte();
		standingTableEntry.homeWin = br.ReadByte();
		standingTableEntry.homeDraw = br.ReadByte();
		standingTableEntry.homeLost = br.ReadByte();
		standingTableEntry.homeGoalsFor = br.ReadByte();
		standingTableEntry.homeGoalsAgainst = br.ReadByte();
		standingTableEntry.awayWin = br.ReadByte();
		standingTableEntry.awayDraw = br.ReadByte();
		standingTableEntry.awayLost = br.ReadByte();
		standingTableEntry.awayGoalsFor = br.ReadByte();
		standingTableEntry.awayGoalsAgainst = br.ReadByte();
		standingTableEntry.points = br.ReadByte();
		if (standingTableEntry.isUsed == 1)
		{
			return standingTableEntry;
		}
		return null;
	}

	public static int TotalLength()
	{
		return 21;
	}

	public bool Write(BinaryWriter bw)
	{
		bw.Write((short)index1);
		bw.Write((byte)isUsed);
		bw.Write((short)compId);
		bw.Write(teamId);
		bw.Write((byte)n);
		bw.Write((byte)homeWin);
		bw.Write((byte)homeDraw);
		bw.Write((byte)homeLost);
		bw.Write((byte)homeGoalsFor);
		bw.Write((byte)homeGoalsAgainst);
		bw.Write((byte)awayWin);
		bw.Write((byte)awayDraw);
		bw.Write((byte)awayLost);
		bw.Write((byte)awayGoalsFor);
		bw.Write((byte)awayGoalsAgainst);
		bw.Write((byte)points);
		return false;
	}

	public void Clean()
	{
		isUsed = 0;
		compId = -1;
		teamId = -1;
		n = 0;
		homeWin = 0;
		homeDraw = 0;
		homeLost = 0;
		homeGoalsFor = 0;
		homeGoalsAgainst = 0;
		awayWin = 0;
		awayDraw = 0;
		awayLost = 0;
		awayGoalsFor = 0;
		awayGoalsAgainst = 0;
		points = 0;
	}
}
