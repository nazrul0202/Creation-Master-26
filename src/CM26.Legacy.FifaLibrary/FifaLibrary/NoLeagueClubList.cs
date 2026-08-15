namespace FifaLibrary;

public class NoLeagueClubList : IdArrayList
{
	public NoLeagueClubList()
		: base(typeof(NoLeagueClub))
	{
		Clear();
		Add(new NoLeagueClub());
	}
}
