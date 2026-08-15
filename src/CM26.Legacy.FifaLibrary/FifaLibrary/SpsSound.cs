using System.Collections.Generic;
using System.IO;

namespace FifaLibrary;

public class SpsSound
{
	private static byte[] c_SilenceSound = new byte[128]
	{
		72, 0, 0, 12, 25, 0, 125, 0, 64, 0,
		15, 0, 68, 0, 0, 104, 0, 0, 15, 0,
		15, 14, 157, 102, 0, 0, 18, 118, 192, 0,
		1, 39, 108, 0, 0, 15, 15, 14, 157, 102,
		0, 0, 18, 118, 192, 0, 1, 39, 108, 0,
		0, 15, 15, 14, 157, 102, 0, 0, 18, 118,
		192, 0, 1, 39, 108, 0, 0, 15, 15, 14,
		157, 102, 0, 0, 18, 118, 192, 0, 1, 39,
		108, 0, 0, 15, 15, 14, 157, 102, 0, 0,
		18, 118, 192, 0, 1, 39, 108, 0, 0, 15,
		15, 14, 157, 102, 0, 1, 242, 118, 192, 0,
		1, 39, 108, 0, 0, 15, 69, 0, 0, 4,
		0, 0, 0, 0, 0, 0, 0, 0
	};

	private SpsSoundHeader m_Header;

	private SpsSoundTerminator m_Terminator;

	private List<SpsSoundData> m_DataSegments;

	private long m_Position;

	private long m_Size;

	private long m_Room;

	private int m_nSamples;

	private int m_nSegments;

	private bool m_IsValid;

	public SpsSoundHeader Header => m_Header;

	public SpsSoundTerminator Terminator => m_Terminator;

	public List<SpsSoundData> DataSegments => m_DataSegments;

	public List<SpsSoundData> Segments => m_DataSegments;

	public long Position
	{
		get
		{
			return m_Position;
		}
		set
		{
			m_Position = value;
		}
	}

	public long Size
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

	public long Room
	{
		get
		{
			return m_Room;
		}
		set
		{
			m_Room = value;
		}
	}

	public int nSamples => m_nSamples;

	public SpsSound()
	{
		m_DataSegments = new List<SpsSoundData>();
	}

	public SpsSound(int nSegments)
	{
		m_DataSegments = new List<SpsSoundData>();
	}

	public SpsSound(string fileName)
	{
		bool flag = false;
		if (File.Exists(fileName))
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			if (fileStream != null)
			{
				BinaryReader br = new BinaryReader(fileStream);
				m_Position = 0L;
				m_DataSegments = new List<SpsSoundData>();
				Load(br);
				flag = true;
			}
		}
		if (!flag)
		{
			m_DataSegments = new List<SpsSoundData>();
		}
	}

	public SpsSound(BinaryReader br)
	{
		m_Position = br.BaseStream.Position;
		m_DataSegments = new List<SpsSoundData>();
		Load(br);
	}

	public void LoadSilence()
	{
		BinaryReader br = new BinaryReader(new MemoryStream(c_SilenceSound));
		Load(br);
	}

	public bool SkipZeroes(BinaryReader br)
	{
		if (br == null)
		{
			return false;
		}
		int num;
		do
		{
			num = br.PeekChar();
			if (num < 0)
			{
				return false;
			}
			if (num == 0)
			{
				br.ReadByte();
			}
		}
		while (num == 0);
		return true;
	}

	public bool Load(BinaryReader br)
	{
		long position = br.BaseStream.Position;
		m_nSamples = 0;
		m_Header = new SpsSoundHeader();
		if (!m_Header.Load(br))
		{
			return false;
		}
		int logicalOffset = 0;
		bool flag;
		do
		{
			long position2 = br.BaseStream.Position;
			flag = SpsSoundData.CheckSoundSegment(br, ref logicalOffset);
			br.BaseStream.Position = position2;
			if (flag)
			{
				SpsSoundData spsSoundData = new SpsSoundData();
				spsSoundData.Load(br);
				m_DataSegments.Add(spsSoundData);
			}
		}
		while (flag);
		if (m_DataSegments.Count == 0)
		{
			return false;
		}
		m_Terminator = new SpsSoundTerminator();
		m_IsValid = m_Terminator.Load(br);
		long position3 = br.BaseStream.Position;
		m_Size = position3 - position;
		long num = m_Size / 16;
		if (m_Size % 16 != 0)
		{
			m_Size = (num + 1) * 16;
		}
		SkipZeroes(br);
		long position4 = br.BaseStream.Position;
		m_Room = position4 - position;
		_ = m_Room;
		_ = m_Size;
		return m_IsValid;
	}

	public bool Save(BinaryWriter bw)
	{
		if (m_Terminator == null)
		{
			return false;
		}
		m_Position = bw.BaseStream.Position;
		m_Header.Save(bw);
		for (int i = 0; i < m_DataSegments.Count; i++)
		{
			m_DataSegments[i].Save(bw);
		}
		m_Terminator.Save(bw);
		return true;
	}

	public bool ExportAsFile(string fullFileName)
	{
		FileStream fileStream = new FileStream(fullFileName, FileMode.Create, FileAccess.ReadWrite);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		long position = m_Position;
		bool result = Save(binaryWriter);
		m_Position = position;
		binaryWriter.Close();
		fileStream.Close();
		return result;
	}

	public bool ImportFromFile(string fullFileName)
	{
		FileStream fileStream = new FileStream(fullFileName, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		long position = m_Position;
		bool result = Load(binaryReader);
		m_Position = position;
		binaryReader.Close();
		fileStream.Close();
		return result;
	}

	public static int CheckSoundData(BinaryReader br, int logicalOffset)
	{
		int result = -1;
		int num = logicalOffset % 32;
		long position = br.BaseStream.Position;
		if (num != 0)
		{
			num = 32 - num;
			if (br.BaseStream.Length < br.BaseStream.Position + num + 12)
			{
				return result;
			}
			br.BaseStream.Position += num;
		}
		int num2 = br.ReadInt32();
		br.BaseStream.Position += position;
		if (num2 == 201326664)
		{
			result = logicalOffset + num;
		}
		return result;
	}

	public static bool IsSoundData(BinaryReader br, ref int logicalOffset)
	{
		if (!SpsSoundHeader.CheckSoundHeader(br, ref logicalOffset))
		{
			return false;
		}
		while (!SpsSoundTerminator.CheckSoundTerminator(br, ref logicalOffset))
		{
			if (!SpsSoundData.CheckSoundSegment(br, ref logicalOffset))
			{
				return false;
			}
		}
		return true;
	}

	public void AddHeaderAndTerminator()
	{
		m_Header = new SpsSoundHeader();
		uint num = 0u;
		for (int i = 0; i < m_nSegments; i++)
		{
			num += (uint)m_DataSegments[i].nSamples;
		}
		m_Header.nSamples = num;
		m_Terminator = new SpsSoundTerminator();
	}
}
