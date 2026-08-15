namespace FifaLibrary;

public class DummyKitList : IdArrayList
{
	public DummyKitList()
		: base(typeof(DummyKit))
	{
		Clear();
		Add(new DummyKit());
	}
}
