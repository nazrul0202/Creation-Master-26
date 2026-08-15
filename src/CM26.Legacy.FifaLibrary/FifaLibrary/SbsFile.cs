using System.IO;

namespace FifaLibrary;

public class SbsFile
{
	private FifaFile m_BaseFile;

	private long m_BeginOfFilePosition;

	private BinaryReader m_BinaryReader;

	private bool m_Start20;

	private bool m_Start40;

	private bool m_Start50;

	private BinaryWriter m_BinaryWriter;

	private FileStream m_FileStream;

	private string m_SbsFileName;

	private long m_FileLength = -1L;

	private SpsSound[] m_SpsSounds;

	private int m_nSounds;

	protected char[] m_Signature;

	public FifaFile BaseFile => m_BaseFile;

	public long BeginOfFilePosition => m_BeginOfFilePosition;

	public BinaryReader BinaryReader => m_BinaryReader;

	public BinaryWriter BinaryWriter => m_BinaryWriter;

	public string SbsFileName => m_SbsFileName;

	public long FileLength => m_FileLength;

	public SpsSound[] Sounds => m_SpsSounds;

	public int nSounds
	{
		get
		{
			return m_nSounds;
		}
		set
		{
			m_nSounds = value;
		}
	}

	public SbsFile(string sbsFileName)
	{
		m_SbsFileName = sbsFileName;
		m_BaseFile = FifaEnvironment.GetFileFromZdata(sbsFileName);
	}

	public bool Load()
	{
		if (m_BaseFile != null)
		{
			m_BinaryReader = m_BaseFile.GetReader();
			m_BeginOfFilePosition = m_BinaryReader.BaseStream.Position;
		}
		else
		{
			if (!File.Exists(m_SbsFileName))
			{
				return false;
			}
			m_FileStream = new FileStream(m_SbsFileName, FileMode.Open, FileAccess.ReadWrite);
			if (m_FileStream == null)
			{
				return false;
			}
			m_BinaryReader = new BinaryReader(m_FileStream);
			if (m_BinaryReader == null)
			{
				return false;
			}
			m_BinaryWriter = new BinaryWriter(m_FileStream);
			if (m_BinaryWriter == null)
			{
				return false;
			}
			m_BeginOfFilePosition = 0L;
			m_FileLength = m_FileStream.Length;
		}
		m_Signature = m_BinaryReader.ReadChars(4);
		if (m_Signature[0] != 'd' || m_Signature[1] != 'a' || m_Signature[2] != 't' || m_Signature[3] != 'a')
		{
			return false;
		}
		m_BinaryReader.BaseStream.Position += 28L;
		int num = 0;
		int logicalOffset = 32;
		m_Start20 = SpsSound.IsSoundData(m_BinaryReader, ref logicalOffset);
		m_Start40 = false;
		m_Start50 = false;
		if (!m_Start20)
		{
			logicalOffset = 64;
			m_BinaryReader.BaseStream.Position = 64L;
			m_Start40 = SpsSound.IsSoundData(m_BinaryReader, ref logicalOffset);
		}
		if (!m_Start20 && !m_Start40)
		{
			logicalOffset = 80;
			m_BinaryReader.BaseStream.Position = 80L;
			m_Start50 = SpsSound.IsSoundData(m_BinaryReader, ref logicalOffset);
		}
		if (m_Start20)
		{
			m_BinaryReader.BaseStream.Position = m_BeginOfFilePosition + 32;
		}
		else if (m_Start40)
		{
			m_BinaryReader.BaseStream.Position = m_BeginOfFilePosition + 64;
		}
		else
		{
			if (!m_Start50)
			{
				return false;
			}
			m_BinaryReader.BaseStream.Position = m_BeginOfFilePosition + 80;
		}
		while (SpsSound.IsSoundData(m_BinaryReader, ref logicalOffset))
		{
			num++;
			SkipZeroes();
			if (m_Start40)
			{
				m_BinaryReader.BaseStream.Position += 32L;
			}
			else if (m_Start50)
			{
				m_BinaryReader.BaseStream.Position += 48L;
			}
		}
		m_nSounds = num;
		if (m_nSounds == 0)
		{
			Close();
		}
		return num > 0;
	}

	public bool SkipZeroes()
	{
		if (m_BinaryReader == null)
		{
			return false;
		}
		int num;
		do
		{
			num = m_BinaryReader.PeekChar();
			if (num < 0)
			{
				return false;
			}
			if (num == 0)
			{
				m_BinaryReader.ReadByte();
			}
		}
		while (num == 0);
		return true;
	}

	public SpsSound ReadSound()
	{
		if (m_BinaryReader == null)
		{
			return null;
		}
		return new SpsSound(m_BinaryReader);
	}

	public SpsSound ReadSound(int offset)
	{
		if (m_BinaryReader == null)
		{
			return null;
		}
		m_BinaryReader.BaseStream.Position = offset + m_BeginOfFilePosition;
		return new SpsSound(m_BinaryReader);
	}

	public int ReadAllSounds()
	{
		m_SpsSounds = new SpsSound[m_nSounds];
		if (m_Start20)
		{
			m_BinaryReader.BaseStream.Position = m_BeginOfFilePosition + 32;
		}
		else if (m_Start40)
		{
			m_BinaryReader.BaseStream.Position = m_BeginOfFilePosition + 64;
		}
		else if (m_Start50)
		{
			m_BinaryReader.BaseStream.Position = m_BeginOfFilePosition + 80;
		}
		for (int i = 0; i < m_nSounds; i++)
		{
			m_SpsSounds[i] = ReadSound();
			if (m_Start40)
			{
				m_BinaryReader.BaseStream.Position += 32L;
			}
			else if (m_Start50)
			{
				m_BinaryReader.BaseStream.Position += 48L;
			}
		}
		return m_nSounds;
	}

	public void WriteSound(SpsSound sound)
	{
		if (m_BinaryWriter != null)
		{
			sound.Save(m_BinaryWriter);
		}
	}

	public void WriteSound(SpsSound sound, int offset)
	{
		if (m_BinaryWriter != null)
		{
			m_BinaryWriter.BaseStream.Position = offset + m_BeginOfFilePosition;
			sound.Save(m_BinaryWriter);
		}
	}

	public void Close()
	{
		if (m_BinaryReader != null)
		{
			m_BinaryReader.Close();
		}
		if (m_BinaryWriter != null)
		{
			m_BinaryWriter.Close();
		}
		if (m_FileStream != null)
		{
			m_FileStream.Close();
		}
		m_BinaryReader = null;
		m_BinaryWriter = null;
		m_FileStream = null;
	}

	public bool IsPatched()
	{
		return true;
	}

	public bool Patch()
	{
		return true;
	}

	public bool ReplaceBlindSound(long position, long size, string spsFileName)
	{
		if (!File.Exists(spsFileName))
		{
			return false;
		}
		long length = new FileInfo(spsFileName).Length;
		if (length > size)
		{
			return false;
		}
		FileStream fileStream = new FileStream(spsFileName, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		m_BinaryWriter.BaseStream.Position = position;
		for (long num = 0L; num < length; num++)
		{
			m_BinaryWriter.Write(binaryReader.ReadByte());
		}
		binaryReader.Close();
		fileStream.Close();
		for (long num2 = length; num2 < size; num2++)
		{
			m_BinaryWriter.Write((byte)0);
		}
		int num3 = SearchIndexSoundByPosition(position);
		if (num3 >= 0)
		{
			SpsSound spsSound = new SpsSound(spsFileName);
			m_SpsSounds[num3] = spsSound;
			spsSound.Position = position;
			spsSound.Size = size;
		}
		return true;
	}

	private SpsSound SearchSoundByPosition(long position)
	{
		for (int i = 0; i < m_nSounds; i++)
		{
			if (m_SpsSounds[i].Position == position)
			{
				return m_SpsSounds[i];
			}
		}
		return null;
	}

	private int SearchIndexSoundByPosition(long position)
	{
		for (int i = 0; i < m_nSounds; i++)
		{
			if (m_SpsSounds[i].Position == position)
			{
				return i;
			}
		}
		return -1;
	}
}
