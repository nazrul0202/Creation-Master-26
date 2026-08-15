namespace FifaLibrary;

public static class MainDbPatcher
{
	public static bool PatchStadiumName(int stadiumId, string stadiumName)
	{
		foreach (Stadium stadium in FifaEnvironment.Stadiums)
		{
			if (stadium.Id == stadiumId)
			{
				stadium.LocalName = stadiumName;
				return true;
			}
		}
		return false;
	}

	public static bool PatchStadiumCountry(int stadiumId, int policeType, int languageIndex, int mowingPattern, bool hasShortTop, bool hasDeepNet, bool hasHexagonal, int netTension)
	{
		foreach (Stadium stadium in FifaEnvironment.Stadiums)
		{
			if (stadium.Id == stadiumId)
			{
				stadium.policetypecode = policeType;
				stadium.StadiumLanguage = languageIndex;
				stadium.stadiumgoalnettype = (hasShortTop ? 1 : 0);
				stadium.stadiumgoalnetpattern = (hasHexagonal ? 1 : 0);
				if (netTension < 0)
				{
					netTension = 0;
				}
				else if (netTension > 2)
				{
					netTension = 2;
				}
				stadium.stadiumgoalnettension = netTension;
				stadium.IsDeepNet = hasDeepNet;
				if (mowingPattern < 0)
				{
					mowingPattern = 0;
				}
				else if (mowingPattern > 15)
				{
					mowingPattern = 15;
				}
				stadium.MowingPatternId = mowingPattern;
				stadium.NetColor = 0;
				return true;
			}
		}
		return false;
	}
}
