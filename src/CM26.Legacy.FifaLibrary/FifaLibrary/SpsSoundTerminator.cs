using System.IO;

namespace FifaLibrary;

public class SpsSoundTerminator
{
	private ushort m_TerminatorSignature;

	private ushort m_Size;

	private int m_nPadding;

	public ushort Size
	{
		get
		{
			return m_Size;
		}
		set
		{
			m_Size = value;
		}
	}

	public int nPadding
	{
		get
		{
			return m_nPadding;
		}
		set
		{
			m_nPadding = value;
		}
	}

	public SpsSoundTerminator()
	{
		m_TerminatorSignature = 17664;
		m_Size = 4;
	}

	public static bool CheckSoundTerminator(BinaryReader br, ref int logicalOffset)
	{
		ushort num = FifaUtil.SwapEndian(br.ReadUInt16());
		ushort num2 = FifaUtil.SwapEndian(br.ReadUInt16());
		br.BaseStream.Position -= 4L;
		if (num != 17664)
		{
			return false;
		}
		int num3 = (logicalOffset + num2) % 16;
		if (num3 != 0)
		{
			num3 = 16 - num3;
		}
		if (br.BaseStream.Position + num2 + num3 > br.BaseStream.Length)
		{
			return false;
		}
		logicalOffset = logicalOffset + num2 + num3;
		br.BaseStream.Position += num2 + num3;
		return true;
	}

	public bool Load(BinaryReader br)
	{
		long position = br.BaseStream.Position;
		m_TerminatorSignature = FifaUtil.SwapEndian(br.ReadUInt16());
		if (m_TerminatorSignature != 17664)
		{
			br.BaseStream.Position = position;
			return false;
		}
		m_Size = FifaUtil.SwapEndian(br.ReadUInt16());
		m_nPadding = (int)(br.BaseStream.Position % 32);
		if (m_nPadding != 0)
		{
			m_nPadding = 32 - m_nPadding;
		}
		br.ReadBytes(m_nPadding);
		return true;
	}

	public bool Save(BinaryWriter bw)
	{
		bw.Write(FifaUtil.SwapEndian(m_TerminatorSignature));
		bw.Write(FifaUtil.SwapEndian(m_Size));
		m_nPadding = (int)(bw.BaseStream.Position % 32);
		if (m_nPadding != 0)
		{
			m_nPadding = 32 - m_nPadding;
		}
		for (int i = 0; i < m_nPadding; i++)
		{
			bw.Write((byte)0);
		}
		return true;
	}
}
