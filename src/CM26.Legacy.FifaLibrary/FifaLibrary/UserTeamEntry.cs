using System.IO;

namespace FifaLibrary;

public class UserTeamEntry
{
	public int index1;

	public int always1;

	public int index2;

	public int teamId;

	public bool Read(BinaryReader br)
	{
		index1 = br.ReadByte();
		always1 = FifaUtil.SwapEndian(br.ReadInt16());
		index2 = br.ReadByte();
		teamId = br.ReadInt32();
		return teamId != -1;
	}

	public bool Write(BinaryWriter bw)
	{
		bw.Write((byte)index1);
		bw.Write((short)always1);
		bw.Write((byte)index2);
		bw.Write(teamId);
		return teamId != -1;
	}

	public void Clean()
	{
		teamId = -1;
	}
}
