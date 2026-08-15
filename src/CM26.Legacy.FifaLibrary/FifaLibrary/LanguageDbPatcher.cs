namespace FifaLibrary;

public class LanguageDbPatcher
{
	public static void PatchStadiumName(int stadiumId, string stadiumName)
	{
		FifaEnvironment.Language.SetStadiumName(stadiumId, stadiumName);
	}

	public static void PatchCompetitionName()
	{
	}
}
