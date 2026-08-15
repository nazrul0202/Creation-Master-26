using System.IO;

namespace FifaLibrary;

public class SpsSoundData
{
	private short m_Sig;

	private short m_Size;

	private int m_nSamples;

	private byte[] m_SoundData;

	public short Size
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

	public int nSamples
	{
		get
		{
			return m_nSamples;
		}
		set
		{
			m_nSamples = value;
		}
	}

	public byte[] SoundData
	{
		get
		{
			return m_SoundData;
		}
		set
		{
			m_SoundData = value;
		}
	}

	public SpsSoundData()
	{
		m_Sig = 17408;
		m_Size = 0;
		m_nSamples = 0;
	}

	public static bool CheckSoundSegment(BinaryReader br, ref int logicalOffset)
	{
		if (br.BaseStream.Length < br.BaseStream.Position + 4)
		{
			return false;
		}
		short num = FifaUtil.SwapEndian(br.ReadInt16());
		short num2 = FifaUtil.SwapEndian(br.ReadInt16());
		br.BaseStream.Position -= 4L;
		if (num != 17408)
		{
			return false;
		}
		if (br.BaseStream.Length < br.BaseStream.Position + num2)
		{
			return false;
		}
		br.BaseStream.Position += num2;
		logicalOffset += num2;
		return true;
	}

	public bool Load(BinaryReader br)
	{
		long position = br.BaseStream.Position;
		m_Sig = FifaUtil.SwapEndian(br.ReadInt16());
		if (m_Sig != 17408)
		{
			br.BaseStream.Position = position;
			return false;
		}
		m_Size = FifaUtil.SwapEndian(br.ReadInt16());
		m_nSamples = FifaUtil.SwapEndian(br.ReadInt32());
		int count = m_Size - 8;
		m_SoundData = br.ReadBytes(count);
		return true;
	}

	public bool Save(BinaryWriter bw)
	{
		bw.Write(FifaUtil.SwapEndian(m_Sig));
		bw.Write(FifaUtil.SwapEndian(m_Size));
		bw.Write(FifaUtil.SwapEndian(m_nSamples));
		_ = m_Size;
		bw.Write(m_SoundData);
		return true;
	}
}
