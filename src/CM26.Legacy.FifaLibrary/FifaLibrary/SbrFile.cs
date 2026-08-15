using System.IO;

namespace FifaLibrary;

public class SbrFile
{
	private FifaFile m_BaseFile;

	private long m_BeginOfFilePosition;

	private long m_OldTesdKeyToBreak = -1L;

	private int m_Length;

	private string m_SbrFileName;

	protected char[] m_Signature;

	private int m_EndOfPointers;

	private int m_StartOfPointers;

	private int m_nPointers;

	private int m_BankKey;

	private int[] m_PointersToTSD;

	private int[] m_PointersToNext;

	private int m_TESDChainTerminatorAddress = -1;

	private SbrEntry m_PlayerNamesGroup;

	private SbrEntry m_SimpleSurnamesGroup;

	private uint m_PlayerNamesGroupKey;

	private uint m_SimpleSurnamesGroupKey;

	public FifaFile BaseFile => m_BaseFile;

	public int Length => m_Length;

	public string SbrFileName => m_SbrFileName;

	public SbrEntry PlayerNamesGroup
	{
		get
		{
			return m_PlayerNamesGroup;
		}
		set
		{
			m_PlayerNamesGroup = value;
		}
	}

	public SbrEntry SimpleSurnamesGroup
	{
		get
		{
			return m_SimpleSurnamesGroup;
		}
		set
		{
			m_SimpleSurnamesGroup = value;
		}
	}

	public uint PlayerNamesGroupKey
	{
		get
		{
			return m_PlayerNamesGroupKey;
		}
		set
		{
			m_PlayerNamesGroupKey = value;
		}
	}

	public uint SimpleSurnamesGroupKey
	{
		get
		{
			return m_SimpleSurnamesGroupKey;
		}
		set
		{
			m_SimpleSurnamesGroupKey = value;
		}
	}

	public SbrFile(string sbrFileName)
	{
		m_SbrFileName = sbrFileName;
		m_BaseFile = FifaEnvironment.GetFileFromZdata(sbrFileName);
	}

	public bool Load(SbsFile sbsFile)
	{
		if (m_BaseFile == null)
		{
			return false;
		}
		BinaryReader reader = m_BaseFile.GetReader();
		m_BeginOfFilePosition = reader.BaseStream.Position;
		m_Signature = reader.ReadChars(4);
		if (m_Signature[0] != 'S' || m_Signature[1] != 'B' || m_Signature[2] != 'l' || m_Signature[3] != 'e')
		{
			return false;
		}
		m_Length = reader.ReadInt32();
		reader.ReadInt16();
		m_nPointers = reader.ReadInt16();
		m_BankKey = reader.ReadInt32();
		reader.BaseStream.Position += 16L;
		m_EndOfPointers = reader.ReadInt32();
		m_StartOfPointers = reader.ReadInt32();
		m_PointersToTSD = new int[m_nPointers];
		m_PointersToNext = new int[m_nPointers];
		reader.BaseStream.Position = m_BeginOfFilePosition + m_StartOfPointers;
		for (int i = 0; i < m_nPointers; i++)
		{
			m_PointersToTSD[i] = reader.ReadInt32();
			m_PointersToNext[i] = reader.ReadInt32();
		}
		for (int j = 0; j < m_nPointers; j++)
		{
			reader.BaseStream.Position = m_BeginOfFilePosition + m_PointersToTSD[j];
			uint groupKey = SbrEntry.GetGroupKey(reader);
			if (groupKey == m_SimpleSurnamesGroupKey)
			{
				m_SimpleSurnamesGroup = new SbrEntry();
				m_SimpleSurnamesGroup.Load(reader, m_BeginOfFilePosition, sbsFile);
			}
			if (groupKey == m_PlayerNamesGroupKey)
			{
				m_PlayerNamesGroup = new SbrEntry();
				m_PlayerNamesGroup.Load(reader, m_BeginOfFilePosition, sbsFile);
			}
		}
		reader.BaseStream.Position = m_BeginOfFilePosition + m_PointersToTSD[m_nPointers - 1];
		int num = (int)(reader.BaseStream.Length - reader.BaseStream.Position) / 4;
		for (int k = 0; k < num; k++)
		{
			if (reader.ReadInt32() == -1)
			{
				m_TESDChainTerminatorAddress = (int)reader.BaseStream.Position - 4;
				break;
			}
		}
		m_BaseFile.ReleaseReader(reader);
		return true;
	}

	public bool IsPatched()
	{
		int num = m_PointersToTSD[m_nPointers - 1];
		if (m_SimpleSurnamesGroup == null)
		{
			return false;
		}
		return m_SimpleSurnamesGroup.TesdFileOffset == num;
	}

	public bool Patch(int firstSoundAddress)
	{
		if (m_SimpleSurnamesGroup == null)
		{
			return false;
		}
		int i;
		for (i = 0; i < m_nPointers && m_PointersToTSD[i] != m_SimpleSurnamesGroup.TesdFileOffset; i++)
		{
		}
		_ = m_PointersToTSD[i + 1];
		_ = m_SimpleSurnamesGroup.TesdFileOffset;
		m_OldTesdKeyToBreak = m_SimpleSurnamesGroup.TesdFileOffset;
		for (int j = i; j < m_nPointers - 1; j++)
		{
			m_PointersToTSD[j] = m_PointersToTSD[j + 1];
		}
		m_PointersToTSD[m_nPointers - 1] = m_Length;
		m_SimpleSurnamesGroup.TesdFileOffset = m_Length;
		m_SimpleSurnamesGroup.PresetSimpleSurnames();
		m_SimpleSurnamesGroup.FirstSoundAddress = firstSoundAddress;
		return true;
	}

	public bool Save(BinaryWriter sbrBW, BinaryWriter sbsBW)
	{
		m_SimpleSurnamesGroup.Save(sbrBW, sbsBW);
		if (m_OldTesdKeyToBreak >= 0)
		{
			sbrBW.BaseStream.Position = m_OldTesdKeyToBreak + 12;
			sbrBW.Write(0);
			sbrBW.BaseStream.Position = m_TESDChainTerminatorAddress;
			int value = m_PointersToTSD[m_nPointers - 1] + 16;
			sbrBW.Write(value);
			sbrBW.BaseStream.Position = m_StartOfPointers;
			for (int i = 0; i < m_nPointers; i++)
			{
				sbrBW.Write(m_PointersToTSD[i]);
				sbrBW.Write(m_PointersToNext[i]);
			}
		}
		int value2 = (int)sbrBW.BaseStream.Length;
		sbrBW.Seek(4, SeekOrigin.Begin);
		sbrBW.Write(value2);
		return true;
	}
}
