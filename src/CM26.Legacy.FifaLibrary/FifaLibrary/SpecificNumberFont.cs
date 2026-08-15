using System.Drawing;

namespace FifaLibrary;

public class SpecificNumberFont
{
	private int m_TeamId;

	private EJerseyShorts m_JerseyShorts;

	private EKitType m_KitType;

	public SpecificNumberFont(int teamId, EJerseyShorts jerseyShorts, EKitType kitType)
	{
		m_TeamId = teamId;
		m_JerseyShorts = jerseyShorts;
		m_KitType = kitType;
	}

	public static string SpecificNumberFontFileName(int teamId, EJerseyShorts jerseyShorts, EKitType kitType)
	{
		string[] obj = new string[7]
		{
			"data/sceneassets/kitnumbers/specifickitnumbers_",
			teamId.ToString(),
			"_",
			null,
			null,
			null,
			null
		};
		int num = (int)jerseyShorts;
		obj[3] = num.ToString();
		obj[4] = "_0_";
		num = (int)kitType;
		obj[5] = num.ToString();
		obj[6] = ".rx3";
		return string.Concat(obj);
	}

	public string SpecificNumberFontFileName()
	{
		return SpecificNumberFontFileName(m_TeamId, m_JerseyShorts, m_KitType);
	}

	public static string SpecificNumberFontTemplateName()
	{
		return "data/sceneassets/kitnumbers/specifickitnumbers_#_%_0_@.rx3";
	}

	public bool SetNumberFont(Bitmap[] bitmaps)
	{
		return FifaEnvironment.ImportBmpsIntoZdata(ids: new int[3]
		{
			m_TeamId,
			(int)m_JerseyShorts,
			(int)m_KitType
		}, templateRx3Name: SpecificNumberFontTemplateName(), bitmaps: bitmaps, compressionMode: ECompressionMode.None, signatures: null);
	}
}
