using System.IO;

namespace FifaLibrary;

public class SbrEntry
{
	private long m_BeginOfFilePosition;

	private int m_TesdFileOffset;

	private uint m_SampleGroupKey;

	private int m_UnknownTesd4_000000E0;

	private int m_DsetId;

	private int m_UnknownTesd16;

	private int m_PointerToAddressOfDataStream;

	private int m_AddressOfDataStream;

	private int m_PointerToAddressOfSoundOffsetTable;

	private int m_UnknownTesd32;

	private int m_UnknownTesd36;

	private int m_UnknownTesd40;

	private int m_UnknownTesd44;

	private int m_UnknownTesd48;

	private int m_UnknownTesd52;

	private int m_nSounds;

	private int m_UnknownTesd60;

	private short m_NumberOfSections;

	private short m_UnknownTesd62_0001;

	private int m_UnknownTesd64_00900048;

	private int m_UnknownTesd68_000000B0;

	private bool m_IsRudPresent;

	private byte m_UnknownRud4;

	private byte m_UnknownRud5;

	private byte m_UnknownRud6;

	private byte m_UnknownRud7;

	private int m_UnknownRud8;

	private int m_UnknownRud12;

	private int m_UnknownRud16;

	private int m_UnknownRud20;

	private byte m_UnknownFfo4_03;

	private byte m_UnknownFfo5_02;

	private byte m_SoundOffsetMultiplier;

	private int m_FirstSoundAddress;

	private int m_UnknownFfo12;

	private int m_AddressOfSoundOffsetTable;

	private int m_PointerToAddressOfSoundSortingTable;

	private int m_UnknownSbs4;

	private int m_UnknownSbs8;

	private int m_UnknownSbs12;

	private int m_UnknownSbs16;

	private int m_UnknownSbs20;

	private byte m_UnknownDis4_02;

	private byte m_UnknownDis5_02;

	private int m_SizeOfSoundOffset;

	private short m_SizeOfSoundSortingEntry;

	private int m_UnknownDis8;

	private int m_UnknownDis12;

	private int m_AddressOfSoundSortingTable;

	private int m_PointerToAddressOfEmphasisTable;

	private int m_AddressOfEmphasisTable;

	private int m_PointerToAddressOfIdsTable;

	private int m_UnknownDis32;

	private int m_UnknownDis36;

	private int m_UnknownDis40;

	private int m_UnknownDis44;

	private int m_SizeOfEmphasisEntry;

	private int m_UnknownDis52_02000000;

	private int m_ColumnKey1;

	private int m_nIds;

	private short m_SizeOfIdsEntry;

	private int m_FirtsId;

	private int m_LastId;

	private int m_AddressOfIdsTable;

	private int m_PointerToNextGroup;

	private int m_ColumnKey2;

	private int m_UnknownDis84_00000002;

	private int m_UnknownDis88_00000001;

	private int m_UnknownDis92_00000002;

	private int m_UnknownDis96;

	private int m_UnknownDis100;

	private int m_TotSamples;

	private NameSoundList m_NameSoundList = new NameSoundList();

	public int TesdFileOffset
	{
		get
		{
			return m_TesdFileOffset;
		}
		set
		{
			m_TesdFileOffset = value;
		}
	}

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

	public byte SoundOffsetMultiplier
	{
		get
		{
			return m_SoundOffsetMultiplier;
		}
		set
		{
			m_SoundOffsetMultiplier = value;
		}
	}

	public int FirstSoundAddress
	{
		get
		{
			return m_FirstSoundAddress;
		}
		set
		{
			m_FirstSoundAddress = value;
		}
	}

	public int SizeOfSoundOffset
	{
		get
		{
			return m_SizeOfSoundOffset;
		}
		set
		{
			m_SizeOfSoundOffset = value;
		}
	}

	public short SizeOfSoundSortingEntry
	{
		get
		{
			return m_SizeOfSoundSortingEntry;
		}
		set
		{
			m_SizeOfSoundSortingEntry = value;
		}
	}

	public int SizeOfEmphasisEntry
	{
		get
		{
			return m_SizeOfEmphasisEntry;
		}
		set
		{
			m_SizeOfEmphasisEntry = value;
		}
	}

	public short SizeOfIdsEntry
	{
		get
		{
			return m_SizeOfIdsEntry;
		}
		set
		{
			m_SizeOfIdsEntry = value;
		}
	}

	public NameSoundList NameSoundList => m_NameSoundList;

	public static uint GetGroupKey(BinaryReader br)
	{
		long position = br.BaseStream.Position;
		br.BaseStream.Position += 12L;
		uint result = br.ReadUInt32();
		br.BaseStream.Position = position;
		return result;
	}

	public bool Load(BinaryReader br, long beginOfFilePosition, SbsFile sbsFile)
	{
		m_BeginOfFilePosition = beginOfFilePosition;
		int num = br.ReadInt32();
		if (num != 1146307924)
		{
			return false;
		}
		m_TesdFileOffset = (int)(br.BaseStream.Position - 4 - m_BeginOfFilePosition);
		m_UnknownTesd4_000000E0 = br.ReadInt32();
		m_DsetId = br.ReadInt32();
		m_SampleGroupKey = br.ReadUInt32();
		m_UnknownTesd16 = br.ReadInt32();
		m_PointerToAddressOfDataStream = br.ReadInt32();
		m_AddressOfDataStream = br.ReadInt32();
		m_PointerToAddressOfSoundOffsetTable = br.ReadInt32();
		m_UnknownTesd32 = br.ReadInt32();
		m_UnknownTesd36 = br.ReadInt32();
		m_UnknownTesd40 = br.ReadInt32();
		m_UnknownTesd44 = br.ReadInt32();
		m_UnknownTesd48 = br.ReadInt32();
		m_UnknownTesd52 = br.ReadInt32();
		m_nSounds = br.ReadInt32();
		m_NumberOfSections = br.ReadInt16();
		m_UnknownTesd62_0001 = br.ReadInt16();
		m_UnknownTesd64_00900048 = br.ReadInt32();
		m_UnknownTesd68_000000B0 = br.ReadInt32();
		num = br.ReadInt32();
		if (num == 776230226)
		{
			m_UnknownRud4 = br.ReadByte();
			m_UnknownRud5 = br.ReadByte();
			m_UnknownRud6 = br.ReadByte();
			m_UnknownRud7 = br.ReadByte();
			m_UnknownRud8 = br.ReadInt32();
			m_UnknownRud12 = br.ReadInt32();
			m_UnknownRud16 = br.ReadInt32();
			m_UnknownRud20 = br.ReadInt32();
			m_IsRudPresent = true;
			num = br.ReadInt32();
		}
		else
		{
			m_IsRudPresent = false;
		}
		if (num != 776947270)
		{
			return false;
		}
		m_UnknownFfo4_03 = br.ReadByte();
		m_UnknownFfo5_02 = br.ReadByte();
		m_SoundOffsetMultiplier = br.ReadByte();
		m_SizeOfSoundOffset = br.ReadByte();
		m_FirstSoundAddress = br.ReadInt32();
		m_UnknownFfo12 = br.ReadInt32();
		m_AddressOfSoundOffsetTable = br.ReadInt32();
		m_PointerToAddressOfSoundSortingTable = br.ReadInt32();
		num = br.ReadInt32();
		if (num != 777208403)
		{
			return false;
		}
		m_UnknownSbs4 = br.ReadInt32();
		m_UnknownSbs8 = br.ReadInt32();
		m_UnknownSbs12 = br.ReadInt32();
		m_UnknownSbs16 = br.ReadInt32();
		m_UnknownSbs20 = br.ReadInt32();
		if (m_UnknownSbs4 == 7 && m_UnknownSbs8 == 6 && m_UnknownSbs12 == 0 && m_UnknownSbs16 == 0)
		{
			_ = m_UnknownSbs20;
		}
		num = br.ReadInt32();
		if (num != 777210180)
		{
			return false;
		}
		m_UnknownDis4_02 = br.ReadByte();
		m_UnknownDis5_02 = br.ReadByte();
		m_SizeOfSoundSortingEntry = FifaUtil.SwapEndian(br.ReadInt16());
		m_UnknownDis8 = br.ReadInt32();
		m_UnknownDis12 = br.ReadInt32();
		m_AddressOfSoundSortingTable = br.ReadInt32();
		m_PointerToAddressOfEmphasisTable = br.ReadInt32();
		m_AddressOfEmphasisTable = br.ReadInt32();
		m_PointerToAddressOfIdsTable = br.ReadInt32();
		m_UnknownDis32 = br.ReadInt32();
		m_UnknownDis36 = br.ReadInt32();
		m_UnknownDis40 = br.ReadInt32();
		m_UnknownDis44 = br.ReadInt32();
		m_SizeOfEmphasisEntry = br.ReadInt32();
		m_UnknownDis52_02000000 = br.ReadInt32();
		m_ColumnKey1 = br.ReadInt32();
		m_nIds = br.ReadInt16();
		m_SizeOfIdsEntry = FifaUtil.SwapEndian(br.ReadInt16());
		m_FirtsId = br.ReadInt32();
		m_LastId = br.ReadInt32();
		m_AddressOfIdsTable = br.ReadInt32();
		m_PointerToNextGroup = br.ReadInt32();
		m_ColumnKey2 = br.ReadInt32();
		m_UnknownDis84_00000002 = br.ReadInt32();
		m_UnknownDis88_00000001 = br.ReadInt32();
		m_UnknownDis92_00000002 = br.ReadInt32();
		m_UnknownDis96 = br.ReadInt32();
		m_UnknownDis100 = br.ReadInt32();
		bool num2 = LoadIdsTable(br);
		bool flag = LoadEmphasisTable(br);
		bool num3 = num2 && flag;
		flag = LoadSortingTable(br);
		bool num4 = num3 && flag;
		flag = LoadSoundOffsetTable(br, sbsFile);
		return num4 && flag;
	}

	public void Save(BinaryWriter bw, BinaryWriter sbsBinaryWriter)
	{
		bw.BaseStream.Position = m_TesdFileOffset;
		m_AddressOfIdsTable = m_TesdFileOffset + 224;
		bw.BaseStream.Position = m_AddressOfIdsTable;
		SaveIdsTable(bw);
		m_AddressOfEmphasisTable = (int)bw.BaseStream.Position;
		SaveEmphasisTable(bw);
		m_AddressOfSoundSortingTable = (int)bw.BaseStream.Position;
		SaveSortingTable(bw);
		m_AddressOfSoundOffsetTable = (int)bw.BaseStream.Position;
		SaveSoundOffsetTable(bw, sbsBinaryWriter);
		int num = (int)bw.BaseStream.Position;
		int num2 = num % 16;
		if (num2 != 0)
		{
			num += 16 - num2;
		}
		bw.BaseStream.SetLength(num);
		bw.BaseStream.Position = m_TesdFileOffset;
		SaveSections(bw);
	}

	private bool LoadIdsTable(BinaryReader br)
	{
		if (m_AddressOfIdsTable == 0)
		{
			return false;
		}
		br.BaseStream.Position = m_BeginOfFilePosition + m_AddressOfIdsTable;
		for (int i = 0; i < m_nIds; i++)
		{
			int num = ((m_SizeOfIdsEntry != 2) ? ((m_SizeOfIdsEntry == 4) ? br.ReadInt32() : 0) : br.ReadUInt16());
			num += m_FirtsId;
			NameSound value = new NameSound(num);
			m_NameSoundList.Add(value);
		}
		return true;
	}

	private bool LoadEmphasisTable(BinaryReader br)
	{
		if (m_AddressOfEmphasisTable == 0)
		{
			int num = 0;
			for (int i = 0; i < m_NameSoundList.Count; i++)
			{
				NameSound obj = (NameSound)m_NameSoundList[i];
				obj.HighSoundIndex = num++;
				obj.LowSoundIndex = num++;
			}
			return false;
		}
		br.BaseStream.Position = m_BeginOfFilePosition + m_AddressOfEmphasisTable;
		for (int j = 0; j < m_nIds; j++)
		{
			int num2 = 0;
			int num3 = 0;
			if (m_SizeOfEmphasisEntry == 1)
			{
				num2 = br.ReadByte();
				num3 = br.ReadByte();
			}
			else if (m_SizeOfEmphasisEntry == 2)
			{
				num2 = br.ReadUInt16();
				num3 = br.ReadUInt16();
			}
			if (num2 >= m_nSounds)
			{
				num2 = m_nSounds - 1;
			}
			if (num3 >= m_nSounds)
			{
				num3 = m_nSounds - 1;
			}
			NameSound nameSound = (NameSound)m_NameSoundList[j];
			nameSound.HighSoundIndex = num2;
			nameSound.LowSoundIndex = num3;
			if (j > 0 && num2 == ((NameSound)m_NameSoundList[j - 1]).LowSoundIndex)
			{
				((NameSound)m_NameSoundList[j - 1]).LowSoundIndex = -1;
			}
			if (num2 == num3)
			{
				nameSound.HighSoundIndex = -1;
			}
		}
		return true;
	}

	private bool LoadSortingTable(BinaryReader br)
	{
		if (m_AddressOfSoundSortingTable == 0)
		{
			return false;
		}
		br.BaseStream.Position = m_BeginOfFilePosition + m_AddressOfSoundSortingTable;
		for (int i = 0; i < m_nSounds; i++)
		{
			if (m_SizeOfSoundSortingEntry == 2)
			{
				br.ReadUInt16();
			}
			else if (m_SizeOfSoundSortingEntry == 4)
			{
				br.ReadInt32();
			}
		}
		return true;
	}

	private bool LoadSoundOffsetTable(BinaryReader br, SbsFile sbsFile)
	{
		if (m_AddressOfSoundOffsetTable == 0)
		{
			return false;
		}
		br.BaseStream.Position = m_BeginOfFilePosition + m_AddressOfSoundOffsetTable;
		m_TotSamples = 0;
		for (int i = 0; i < m_nSounds; i++)
		{
			int num = 0;
			if (m_SizeOfSoundOffset == 2)
			{
				num = br.ReadUInt16() * 32;
			}
			else if (m_SizeOfSoundOffset == 4)
			{
				num = br.ReadInt32();
			}
			SpsSound spsSound = sbsFile.ReadSound(num + m_FirstSoundAddress);
			m_TotSamples += spsSound.nSamples;
			foreach (NameSound nameSound in m_NameSoundList)
			{
				if (nameSound.HighSoundIndex == i)
				{
					nameSound.HighSoundOffset = num;
					nameSound.HighSound = spsSound;
				}
				if (nameSound.LowSoundIndex == i)
				{
					nameSound.LowSoundOffset = num;
					nameSound.LowSound = spsSound;
				}
			}
		}
		return true;
	}

	private void SaveIdsTable(BinaryWriter bw)
	{
		m_NameSoundList.SortId();
		m_nIds = m_NameSoundList.Count;
		NameSound nameSound = (NameSound)m_NameSoundList[0];
		m_FirtsId = nameSound.Id;
		nameSound = (NameSound)m_NameSoundList[m_nIds - 1];
		m_LastId = nameSound.Id;
		foreach (NameSound nameSound2 in m_NameSoundList)
		{
			_ = nameSound2.Id;
			_ = 910000;
			if (m_SizeOfIdsEntry == 2)
			{
				short value = (short)(nameSound2.Id - m_FirtsId);
				bw.Write(value);
			}
			else if (m_SizeOfIdsEntry == 4)
			{
				int value2 = nameSound2.Id - m_FirtsId;
				bw.Write(value2);
			}
		}
	}

	private void SaveEmphasisTable(BinaryWriter bw)
	{
		ushort num = 0;
		foreach (NameSound nameSound in m_NameSoundList)
		{
			if (nameSound.HighSound == null)
			{
				nameSound.HighSound = new SpsSound();
				nameSound.HighSound.LoadSilence();
			}
			if (nameSound.LowSound == null)
			{
				nameSound.LowSound = new SpsSound();
				nameSound.LowSound.LoadSilence();
			}
			if (nameSound.HighSound == nameSound.LowSound)
			{
				nameSound.HighSoundIndex = num;
				bw.Write(num);
				nameSound.LowSoundIndex = num;
				bw.Write(num);
				num++;
			}
			else
			{
				nameSound.HighSoundIndex = num;
				bw.Write(num);
				num = (ushort)(nameSound.LowSoundIndex = (ushort)(num + 1));
				bw.Write(num);
				num++;
			}
		}
		bw.Write(num);
		m_nSounds = num;
	}

	private void SaveSortingTable(BinaryWriter bw)
	{
		for (int i = 0; i < m_nSounds; i++)
		{
			if (m_SizeOfSoundSortingEntry == 2)
			{
				bw.Write((short)i);
			}
			else if (m_SizeOfSoundSortingEntry == 4)
			{
				bw.Write(i);
			}
		}
	}

	private void SaveSoundOffsetTable(BinaryWriter bw, BinaryWriter sbsWriter)
	{
		sbsWriter.BaseStream.Position = m_FirstSoundAddress;
		int num = 0;
		foreach (NameSound nameSound in m_NameSoundList)
		{
			int num2 = (int)sbsWriter.BaseStream.Position - m_FirstSoundAddress;
			nameSound.HighSound.Save(sbsWriter);
			if (m_SizeOfSoundOffset == 2)
			{
				nameSound.HighSoundOffset = num2;
				num2 /= 32;
				bw.Write((short)num2);
			}
			else
			{
				nameSound.HighSoundOffset = num2;
				bw.Write(num2);
			}
			num++;
			if (nameSound.HighSound != nameSound.LowSound)
			{
				num2 = (int)sbsWriter.BaseStream.Position - m_FirstSoundAddress;
				nameSound.LowSound.Save(sbsWriter);
				if (m_SizeOfSoundOffset == 2)
				{
					nameSound.LowSoundOffset = num2;
					num2 /= 32;
					bw.Write((short)num2);
				}
				else
				{
					nameSound.LowSoundOffset = num2;
					bw.Write(num2);
				}
				num++;
			}
		}
		int num3 = (int)sbsWriter.BaseStream.Position;
		sbsWriter.BaseStream.SetLength(num3);
	}

	private void SaveSections(BinaryWriter bw)
	{
		int value = 1146307924;
		bw.Write(value);
		bw.Write(m_UnknownTesd4_000000E0);
		bw.Write(m_DsetId);
		bw.Write(m_SampleGroupKey);
		bw.Write(m_UnknownTesd16);
		m_PointerToAddressOfDataStream = (int)bw.BaseStream.Position + 4;
		bw.Write(m_PointerToAddressOfDataStream);
		bw.Write(m_AddressOfDataStream);
		m_PointerToAddressOfSoundOffsetTable = (int)bw.BaseStream.Position + 60;
		bw.Write(m_PointerToAddressOfSoundOffsetTable);
		bw.Write(m_UnknownTesd32);
		bw.Write(m_UnknownTesd36);
		bw.Write(m_UnknownTesd40);
		bw.Write(m_UnknownTesd44);
		bw.Write(m_UnknownTesd48);
		bw.Write(m_UnknownTesd52);
		bw.Write(m_nSounds);
		bw.Write(m_NumberOfSections);
		bw.Write(m_UnknownTesd62_0001);
		bw.Write(m_UnknownTesd64_00900048);
		bw.Write(m_UnknownTesd68_000000B0);
		value = 776947270;
		bw.Write(value);
		bw.Write(m_UnknownFfo4_03);
		bw.Write(m_UnknownFfo5_02);
		bw.Write(m_SoundOffsetMultiplier);
		bw.Write((byte)m_SizeOfSoundOffset);
		bw.Write(m_FirstSoundAddress);
		bw.Write(m_UnknownFfo12);
		bw.Write(m_AddressOfSoundOffsetTable);
		m_PointerToAddressOfSoundSortingTable = (int)bw.BaseStream.Position + 44;
		bw.Write(m_PointerToAddressOfSoundSortingTable);
		value = 777208403;
		bw.Write(value);
		bw.Write(m_UnknownSbs4);
		bw.Write(m_UnknownSbs8);
		bw.Write(m_UnknownSbs12);
		bw.Write(m_UnknownSbs16);
		bw.Write(m_UnknownSbs20);
		value = 777210180;
		bw.Write(value);
		bw.Write(m_UnknownDis4_02);
		bw.Write(m_UnknownDis5_02);
		bw.Write(FifaUtil.SwapEndian(m_SizeOfSoundSortingEntry));
		bw.Write(m_UnknownDis8);
		bw.Write(m_UnknownDis12);
		bw.Write(m_AddressOfSoundSortingTable);
		m_PointerToAddressOfEmphasisTable = (int)bw.BaseStream.Position + 4;
		bw.Write(m_PointerToAddressOfEmphasisTable);
		bw.Write(m_AddressOfEmphasisTable);
		m_PointerToAddressOfIdsTable = (int)bw.BaseStream.Position + 44;
		bw.Write(m_PointerToAddressOfIdsTable);
		bw.Write(m_UnknownDis32);
		bw.Write(m_UnknownDis36);
		bw.Write(m_UnknownDis40);
		bw.Write(m_UnknownDis44);
		bw.Write(m_SizeOfEmphasisEntry);
		bw.Write(m_UnknownDis52_02000000);
		bw.Write(m_ColumnKey1);
		bw.Write((short)m_nIds);
		bw.Write(FifaUtil.SwapEndian(m_SizeOfIdsEntry));
		bw.Write(m_FirtsId);
		bw.Write(m_LastId);
		bw.Write(m_AddressOfIdsTable);
		m_PointerToNextGroup = -1;
		bw.Write(m_PointerToNextGroup);
		bw.Write(m_ColumnKey2);
		bw.Write(m_UnknownDis84_00000002);
		bw.Write(m_UnknownDis88_00000001);
		bw.Write(m_UnknownDis92_00000002);
		bw.Write(m_UnknownDis96);
		bw.Write(m_UnknownDis100);
	}

	public void PresetSimpleSurnames()
	{
		m_UnknownTesd4_000000E0 = 224;
		m_UnknownTesd16 = 0;
		m_UnknownTesd32 = 0;
		m_UnknownTesd36 = 0;
		m_UnknownTesd40 = 0;
		m_UnknownTesd44 = 0;
		m_UnknownTesd48 = 0;
		m_UnknownTesd52 = 0;
		m_NumberOfSections = 3;
		m_UnknownTesd62_0001 = 1;
		m_UnknownTesd64_00900048 = 9437256;
		m_UnknownTesd68_000000B0 = 176;
		m_UnknownFfo4_03 = 3;
		m_UnknownFfo5_02 = 2;
		m_SoundOffsetMultiplier = 0;
		m_SizeOfSoundOffset = 4;
		m_UnknownFfo12 = 0;
		m_UnknownDis4_02 = 2;
		m_UnknownDis5_02 = 2;
		m_SizeOfSoundSortingEntry = 4;
		m_UnknownDis12 = 0;
		m_UnknownDis32 = 0;
		m_UnknownDis36 = 0;
		m_UnknownDis40 = 0;
		m_UnknownDis44 = 0;
		m_SizeOfEmphasisEntry = 2;
		m_UnknownDis52_02000000 = 33554432;
		m_SizeOfIdsEntry = 4;
		m_UnknownDis84_00000002 = 2;
		m_UnknownDis88_00000001 = 1;
		m_UnknownDis92_00000002 = 2;
		m_UnknownDis96 = 0;
		m_UnknownDis100 = 0;
	}
}
