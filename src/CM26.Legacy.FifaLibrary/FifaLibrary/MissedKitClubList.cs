namespace FifaLibrary;

public class MissedKitClubList : IdArrayList
{
	public MissedKitClubList()
		: base(typeof(MissedKitClub))
	{
		Clear();
		Add(new MissedKitClub());
	}
}
