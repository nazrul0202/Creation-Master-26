namespace FifaLibrary;

public class NameSound : IdObject
{
	private SpsSound m_LowSound;

	private SpsSound m_HighSound;

	private string m_Text;

	private int m_LowSoundIndex;

	private int m_HighSoundIndex;

	private int m_LowSoundOffset;

	private int m_HighSoundOffset;

	public SpsSound LowSound
	{
		get
		{
			return m_LowSound;
		}
		set
		{
			m_LowSound = value;
		}
	}

	public SpsSound HighSound
	{
		get
		{
			return m_HighSound;
		}
		set
		{
			m_HighSound = value;
		}
	}

	public string Text
	{
		get
		{
			return m_Text;
		}
		set
		{
			m_Text = value;
		}
	}

	public int LowSoundIndex
	{
		get
		{
			return m_LowSoundIndex;
		}
		set
		{
			m_LowSoundIndex = value;
		}
	}

	public int HighSoundIndex
	{
		get
		{
			return m_HighSoundIndex;
		}
		set
		{
			m_HighSoundIndex = value;
		}
	}

	public int LowSoundOffset
	{
		get
		{
			return m_LowSoundOffset;
		}
		set
		{
			m_LowSoundOffset = value;
		}
	}

	public int HighSoundOffset
	{
		get
		{
			return m_HighSoundOffset;
		}
		set
		{
			m_HighSoundOffset = value;
		}
	}

	public NameSound(int nameid)
		: base(nameid)
	{
		m_LowSoundIndex = -1;
		m_HighSoundIndex = -1;
	}

	public void LinkNameDictionary(NameDictionary nameDictionary)
	{
		if (nameDictionary != null && base.Id >= 900000)
		{
			nameDictionary.TryGetValue(base.Id, out var value);
			if (value == null)
			{
				m_Text = "NameId " + base.Id;
			}
			else
			{
				m_Text = value;
			}
		}
	}

	public void LinkPlayers(PlayerList playerList)
	{
		if (playerList != null && base.Id < 900000)
		{
			m_Text = "PlayerId " + base.Id;
			Player player = (Player)FifaEnvironment.Players.SearchId(base.Id);
			if (player != null)
			{
				m_Text = player.ToString();
			}
		}
	}

	public bool ExportHighSound(string fullFileName)
	{
		if (m_HighSound != null)
		{
			return m_HighSound.ExportAsFile(fullFileName);
		}
		return false;
	}

	public bool ExportLowSound(string fullFileName)
	{
		if (m_LowSound != null)
		{
			return m_LowSound.ExportAsFile(fullFileName);
		}
		return false;
	}
}
