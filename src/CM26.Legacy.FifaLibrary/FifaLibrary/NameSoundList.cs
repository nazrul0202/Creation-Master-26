using System;
using System.Collections;

namespace FifaLibrary;

public class NameSoundList : IdArrayList
{
	public NameSoundList()
		: base(typeof(NameSound))
	{
	}

	public void Delete(NameSound nameSound)
	{
		RemoveId(nameSound);
	}

	public void LinkNameDictionary(NameDictionary nameDictionary)
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((NameSound)enumerator.Current).LinkNameDictionary(nameDictionary);
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	public void LinkPlayers(PlayerList playerList)
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((NameSound)enumerator.Current).LinkPlayers(playerList);
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}
}
