using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace FifaLibrary;

public class Audio
{
	private string m_StandardLanguage;

	private string m_XmlFileName;

	private string m_SbrFileName;

	private string m_SbsFileName;

	private DataSet m_AudioXmlDataSet;

	private uint m_SimpleSurnameKey;

	private uint m_PlayerNamesKey;

	private SbrFile m_SbrFile;

	private SbsFile m_SbsFile;

	private static CommentaryDictionary m_CommentaryDictionary = new CommentaryDictionary();

	public string StandardLanguage
	{
		get
		{
			return m_StandardLanguage;
		}
		set
		{
			m_StandardLanguage = value;
			if (m_StandardLanguage == null)
			{
				m_XmlFileName = null;
				m_SbrFileName = null;
				m_SbsFileName = null;
				return;
			}
			m_XmlFileName = "audiodata/speechdata/" + m_StandardLanguage + "/" + m_StandardLanguage + ".xml";
			m_SbrFileName = "audiodata/speechdata/" + m_StandardLanguage + "/" + m_StandardLanguage + "_bank.sbr";
			m_SbsFileName = "audiodata/speechdata/" + m_StandardLanguage + "/" + m_StandardLanguage + "_bank.sbs";
		}
	}

	public string XmlFileName
	{
		get
		{
			return m_XmlFileName;
		}
		set
		{
			m_XmlFileName = value;
		}
	}

	public string SbrFileName
	{
		get
		{
			return m_SbrFileName;
		}
		set
		{
			m_SbrFileName = value;
		}
	}

	public string SbsFileName
	{
		get
		{
			return m_SbsFileName;
		}
		set
		{
			m_SbsFileName = value;
		}
	}

	public DataSet DescriptorDataSet
	{
		get
		{
			return m_AudioXmlDataSet;
		}
		set
		{
			m_AudioXmlDataSet = value;
		}
	}

	public SbrFile SbrFile => m_SbrFile;

	public SbsFile SbsFile => m_SbsFile;

	public static CommentaryDictionary CommentaryDictionary => m_CommentaryDictionary;

	public Audio()
	{
		if (!m_CommentaryDictionary.IsInitialized())
		{
			m_CommentaryDictionary.Initialize();
		}
	}

	public bool CheckAndExtract()
	{
		bool flag = IsAudioExtracted();
		if (!flag && IsAudioPresent())
		{
			flag = ExtractAudio();
		}
		return flag;
	}

	public bool IsAudioExtracted()
	{
		if (!File.Exists(FifaEnvironment.GameDir + SbrFileName))
		{
			return false;
		}
		if (!File.Exists(FifaEnvironment.GameDir + SbsFileName))
		{
			return false;
		}
		FifaEnvironment.FifaFat.HideFile(SbrFileName);
		FifaEnvironment.FifaFat.HideFile(SbsFileName);
		return true;
	}

	public bool IsAudioPresent()
	{
		return FifaEnvironment.FifaFat.IsArchivedFilePresent(m_SbrFileName);
	}

	public bool ExtractAudio()
	{
		if (!IsAudioPresent())
		{
			return false;
		}
		if (!FifaEnvironment.FifaFat.ExtractFile(SbrFileName))
		{
			return false;
		}
		if (!FifaEnvironment.FifaFat.ExtractFile(SbsFileName))
		{
			return false;
		}
		return true;
	}

	public bool IsAudioPatched()
	{
		if (m_SbrFile.IsPatched())
		{
			return m_SbsFile.IsPatched();
		}
		return false;
	}

	public bool Patch()
	{
		int uncompressedSize = m_SbsFile.BaseFile.UncompressedSize;
		bool flag = m_SbrFile.Patch(uncompressedSize);
		if (!flag)
		{
			return false;
		}
		return flag;
	}

	public bool OpenForReading()
	{
		return OpenForReading(FifaEnvironment.Players, FifaEnvironment.NameDictionary);
	}

	public bool OpenForReading(PlayerList players, NameDictionary nameDictionary)
	{
		if (players == null)
		{
			players = FifaEnvironment.Players;
		}
		if (nameDictionary == null)
		{
			nameDictionary = FifaEnvironment.NameDictionary;
		}
		LoadXml();
		LoadSbs();
		LoadSbr(m_SbsFile);
		m_SbsFile.BaseFile.ReleaseReader(m_SbsFile.BinaryReader);
		if (m_SbrFile.PlayerNamesGroup != null)
		{
			m_SbrFile.PlayerNamesGroup.NameSoundList.LinkPlayers(players);
		}
		if (m_SbrFile.SimpleSurnamesGroup != null)
		{
			m_SbrFile.SimpleSurnamesGroup.NameSoundList.LinkNameDictionary(nameDictionary);
		}
		return true;
	}

	public bool OpenForEditing()
	{
		return OpenForEditing(FifaEnvironment.Players, FifaEnvironment.NameDictionary);
	}

	public bool OpenForEditing(PlayerList players, NameDictionary nameDictionary)
	{
		if (players == null)
		{
			players = FifaEnvironment.Players;
		}
		if (nameDictionary == null)
		{
			nameDictionary = FifaEnvironment.NameDictionary;
		}
		if (!CheckAndExtract())
		{
			return false;
		}
		OpenForReading(players, nameDictionary);
		if (!IsAudioPatched())
		{
			Patch();
		}
		return true;
	}

	private bool LoadXml()
	{
		FifaFile fileFromZdata = FifaEnvironment.GetFileFromZdata(m_XmlFileName);
		if (fileFromZdata == null)
		{
			return false;
		}
		StreamReader streamReader = fileFromZdata.GetStreamReader();
		m_AudioXmlDataSet = new DataSet();
		m_AudioXmlDataSet.ReadXml(streamReader);
		DataTable dataTable = m_AudioXmlDataSet.Tables["SampleGroup"];
		string text = "pSIMPLE_SURNAME";
		string text2 = "pPLAYER_NAMES";
		string text3 = null;
		string text4 = null;
		for (int i = 0; i < dataTable.Rows.Count; i++)
		{
			DataRow dataRow = dataTable.Rows[i];
			if (dataRow["Name"].ToString() == text)
			{
				text3 = dataRow["SampleGroupKey"].ToString();
			}
			if (dataRow["Name"].ToString() == text2)
			{
				text4 = dataRow["SampleGroupKey"].ToString();
			}
		}
		if (text3 != null)
		{
			m_SimpleSurnameKey = Convert.ToUInt32(text3, 16);
		}
		if (text4 != null)
		{
			m_PlayerNamesKey = Convert.ToUInt32(text4, 16);
		}
		if (text3 != null)
		{
			return text4 != null;
		}
		return false;
	}

	private bool LoadSbr(SbsFile sbsFile)
	{
		m_SbrFile = new SbrFile(m_SbrFileName);
		m_SbrFile.SimpleSurnamesGroupKey = m_SimpleSurnameKey;
		m_SbrFile.PlayerNamesGroupKey = m_PlayerNamesKey;
		return m_SbrFile.Load(sbsFile);
	}

	public bool LoadSbs()
	{
		m_SbsFile = new SbsFile(m_SbsFileName);
		return m_SbsFile.Load();
	}

	public bool Save()
	{
		string text = FifaEnvironment.GameDir + m_SbsFileName;
		File.Copy(text, text + ".bak", overwrite: true);
		string text2 = FifaEnvironment.GameDir + m_SbrFileName;
		File.Copy(text2, text2 + ".bak", overwrite: true);
		string physicalName = m_SbrFile.BaseFile.PhysicalName;
		string physicalName2 = m_SbsFile.BaseFile.PhysicalName;
		FileStream fileStream = new FileStream(physicalName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		FileStream fileStream2 = new FileStream(physicalName2, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		BinaryWriter binaryWriter2 = new BinaryWriter(fileStream2);
		bool result = m_SbrFile.Save(binaryWriter, binaryWriter2);
		fileStream.Close();
		binaryWriter.Close();
		fileStream2.Close();
		binaryWriter2.Close();
		return result;
	}

	private bool SaveSbs()
	{
		return true;
	}

	public void UseStandardId()
	{
		foreach (KeyValuePair<int, string> item in FifaEnvironment.NameDictionary)
		{
			string value = item.Value;
			int key = item.Key;
			int num = CommentaryDictionary.SearchName(value);
			if (num != key)
			{
				FifaEnvironment.NameDictionary.Remove(key);
				FifaEnvironment.NameDictionary.Add(num, value);
				((NameSound)m_SbrFile.SimpleSurnamesGroup.NameSoundList.SearchId(key)).Id = num;
			}
		}
	}
}
