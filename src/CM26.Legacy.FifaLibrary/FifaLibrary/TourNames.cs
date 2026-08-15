using System.Data;
using System.Drawing;

namespace FifaLibrary;

public class TourNames
{
	private int m_ballid = -1;

	private int m_confedid;

	private string m_TourName;

	private string m_Continent;

	private string m_TourType;

	private string m_languagestring;

	private string m_ShortName;

	private string m_LongName;

	private Color m_compcolour1;

	private Color m_compcolour2;

	private int m_competitioncolor1g;

	private int m_competitioncolor1r;

	private int m_competitioncolor1b;

	private int m_competitioncolor2g;

	private int m_competitioncolor2r;

	private int m_competitioncolor2b;

	private int m_isteampitchflagenabledstage;

	private int m_isstanchionflamethrowerenabledstage;

	private int m_goaljingle;

	private int m_isuniqueleagueflagenabled;

	private int m_isstadiumdressingenabledstage;

	private int m_isgoallinetechcompenabledstage;

	private int m_abbapenalties;

	private int m_competitionimportance;

	private int m_iscompetitionpodiumenabled;

	private int m_adboardplacement;

	private int m_iscompetitionscarfenabled;

	private int m_isbannerenabled;

	private int m_pitchtarps;

	private int m_competitionid;

	private int m_introconfetti;

	private int m_colorregion;

	private int m_isuniqueadboardscompenabled;

	private int m_isballplinthenabledstage;

	private int m_iscompetitionpoleflagenabled;

	private int m_isteampitchflagenabled;

	private int m_isflamethrowercannonsenabled;

	private int m_iscenterpitchflagenabledstage;

	private int m_iscompetitioncrowdcardsenabled;

	private int m_isarchwayenabled;

	private int m_isgoallinetechcompenabled;

	private int m_replay360degree;

	private int m_isuniquetrophypedestalenabled;

	private int m_isinjuryboardenabled;

	private int m_isintroconfettienabledstage;

	private int m_inflatables;

	private int m_isstadiumdressingunique;

	private int m_isvanishingsprayhomeenabled;

	private int m_isflamethrowercannonsenabledstage;

	private int m_isuniquehandshakeboardenabled;

	private int m_isvanishingsprayenabled;

	private int m_stanchionflamethrower;

	private int m_authenticpodiumskin;

	private int m_languageregion;

	private int m_ispitchtarpsenabledstage;

	private int m_adboardplacementstage;

	private int m_isstadiumdressinguniquestage;

	private int m_iscenterpitchflagenabled;

	private int m_isgoallinetechhomeleagueenabled;

	private int m_stadiumcrowdmap;

	private int m_isballplinthenabled;

	private int m_onpitchgraphics;

	private int m_crowdskintonecode;

	private int m_isarchwayenabledstage;

	private int m_isgoalnetadsenabledstage;

	private int m_isstadiumdressingenabled;

	private int m_goalnetads;

	public int confedid
	{
		get
		{
			return m_confedid;
		}
		set
		{
			m_confedid = value;
		}
	}

	public string TourName
	{
		get
		{
			return m_TourName;
		}
		set
		{
			m_TourName = value;
		}
	}

	public string Continent
	{
		get
		{
			return m_Continent;
		}
		set
		{
			m_Continent = value;
		}
	}

	public string TourType
	{
		get
		{
			return m_TourType;
		}
		set
		{
			m_TourType = value;
		}
	}

	public string languagestring
	{
		get
		{
			return m_languagestring;
		}
		set
		{
			m_languagestring = value;
		}
	}

	public string ShortName
	{
		get
		{
			return m_ShortName;
		}
		set
		{
			m_ShortName = value;
		}
	}

	public string LongName
	{
		get
		{
			return m_LongName;
		}
		set
		{
			m_LongName = value;
		}
	}

	public Color compcolour1
	{
		get
		{
			return m_compcolour1;
		}
		set
		{
			m_compcolour1 = value;
		}
	}

	public Color compcolour2
	{
		get
		{
			return m_compcolour2;
		}
		set
		{
			m_compcolour2 = value;
		}
	}

	public int competitioncolor1g
	{
		get
		{
			return m_competitioncolor1g;
		}
		set
		{
			m_competitioncolor1g = value;
		}
	}

	public int competitioncolor1r
	{
		get
		{
			return m_competitioncolor1r;
		}
		set
		{
			m_competitioncolor1r = value;
		}
	}

	public int competitioncolor1b
	{
		get
		{
			return m_competitioncolor1b;
		}
		set
		{
			m_competitioncolor1b = value;
		}
	}

	public int competitioncolor2g
	{
		get
		{
			return m_competitioncolor2g;
		}
		set
		{
			m_competitioncolor2g = value;
		}
	}

	public int competitioncolor2r
	{
		get
		{
			return m_competitioncolor2r;
		}
		set
		{
			m_competitioncolor2r = value;
		}
	}

	public int competitioncolor2b
	{
		get
		{
			return m_competitioncolor2b;
		}
		set
		{
			m_competitioncolor2b = value;
		}
	}

	public int ballid
	{
		get
		{
			return m_ballid;
		}
		set
		{
			m_ballid = value;
		}
	}

	public int isteampitchflagenabledstage
	{
		get
		{
			return m_isteampitchflagenabledstage;
		}
		set
		{
			m_isteampitchflagenabledstage = value;
		}
	}

	public int isstanchionflamethrowerenabledstage
	{
		get
		{
			return m_isstanchionflamethrowerenabledstage;
		}
		set
		{
			m_isstanchionflamethrowerenabledstage = value;
		}
	}

	public int goaljingle
	{
		get
		{
			return m_goaljingle;
		}
		set
		{
			m_goaljingle = value;
		}
	}

	public int isuniqueleagueflagenabled
	{
		get
		{
			return m_isuniqueleagueflagenabled;
		}
		set
		{
			m_isuniqueleagueflagenabled = value;
		}
	}

	public int isstadiumdressingenabledstage
	{
		get
		{
			return m_isstadiumdressingenabledstage;
		}
		set
		{
			m_isstadiumdressingenabledstage = value;
		}
	}

	public int isgoallinetechcompenabledstage
	{
		get
		{
			return m_isgoallinetechcompenabledstage;
		}
		set
		{
			m_isgoallinetechcompenabledstage = value;
		}
	}

	public int abbapenalties
	{
		get
		{
			return m_abbapenalties;
		}
		set
		{
			m_abbapenalties = value;
		}
	}

	public int competitionimportance
	{
		get
		{
			return m_competitionimportance;
		}
		set
		{
			m_competitionimportance = value;
		}
	}

	public int iscompetitionpodiumenabled
	{
		get
		{
			return m_iscompetitionpodiumenabled;
		}
		set
		{
			m_iscompetitionpodiumenabled = value;
		}
	}

	public int adboardplacement
	{
		get
		{
			return m_adboardplacement;
		}
		set
		{
			m_adboardplacement = value;
		}
	}

	public int iscompetitionscarfenabled
	{
		get
		{
			return m_iscompetitionscarfenabled;
		}
		set
		{
			m_iscompetitionscarfenabled = value;
		}
	}

	public int isbannerenabled
	{
		get
		{
			return m_isbannerenabled;
		}
		set
		{
			m_isbannerenabled = value;
		}
	}

	public int pitchtarps
	{
		get
		{
			return m_pitchtarps;
		}
		set
		{
			m_pitchtarps = value;
		}
	}

	public int competitionid
	{
		get
		{
			return m_competitionid;
		}
		set
		{
			m_competitionid = value;
		}
	}

	public int introconfetti
	{
		get
		{
			return m_introconfetti;
		}
		set
		{
			m_introconfetti = value;
		}
	}

	public int colorregion
	{
		get
		{
			return m_colorregion;
		}
		set
		{
			m_colorregion = value;
		}
	}

	public int isuniqueadboardscompenabled
	{
		get
		{
			return m_isuniqueadboardscompenabled;
		}
		set
		{
			m_isuniqueadboardscompenabled = value;
		}
	}

	public int isballplinthenabledstage
	{
		get
		{
			return m_isballplinthenabledstage;
		}
		set
		{
			m_isballplinthenabledstage = value;
		}
	}

	public int iscompetitionpoleflagenabled
	{
		get
		{
			return m_iscompetitionpoleflagenabled;
		}
		set
		{
			m_iscompetitionpoleflagenabled = value;
		}
	}

	public int isteampitchflagenabled
	{
		get
		{
			return m_isteampitchflagenabled;
		}
		set
		{
			m_isteampitchflagenabled = value;
		}
	}

	public int isflamethrowercannonsenabled
	{
		get
		{
			return m_isflamethrowercannonsenabled;
		}
		set
		{
			m_isflamethrowercannonsenabled = value;
		}
	}

	public int iscenterpitchflagenabledstage
	{
		get
		{
			return m_iscenterpitchflagenabledstage;
		}
		set
		{
			m_iscenterpitchflagenabledstage = value;
		}
	}

	public int iscompetitioncrowdcardsenabled
	{
		get
		{
			return m_iscompetitioncrowdcardsenabled;
		}
		set
		{
			m_iscompetitioncrowdcardsenabled = value;
		}
	}

	public int isarchwayenabled
	{
		get
		{
			return m_isarchwayenabled;
		}
		set
		{
			m_isarchwayenabled = value;
		}
	}

	public int isgoallinetechcompenabled
	{
		get
		{
			return m_isgoallinetechcompenabled;
		}
		set
		{
			m_isgoallinetechcompenabled = value;
		}
	}

	public int replay360degree
	{
		get
		{
			return m_replay360degree;
		}
		set
		{
			m_replay360degree = value;
		}
	}

	public int isuniquetrophypedestalenabled
	{
		get
		{
			return m_isuniquetrophypedestalenabled;
		}
		set
		{
			m_isuniquetrophypedestalenabled = value;
		}
	}

	public int isinjuryboardenabled
	{
		get
		{
			return m_isinjuryboardenabled;
		}
		set
		{
			m_isinjuryboardenabled = value;
		}
	}

	public int isintroconfettienabledstage
	{
		get
		{
			return m_isintroconfettienabledstage;
		}
		set
		{
			m_isintroconfettienabledstage = value;
		}
	}

	public int inflatables
	{
		get
		{
			return m_inflatables;
		}
		set
		{
			m_inflatables = value;
		}
	}

	public int isstadiumdressingunique
	{
		get
		{
			return m_isstadiumdressingunique;
		}
		set
		{
			m_isstadiumdressingunique = value;
		}
	}

	public int isvanishingsprayhomeenabled
	{
		get
		{
			return m_isvanishingsprayhomeenabled;
		}
		set
		{
			m_isvanishingsprayhomeenabled = value;
		}
	}

	public int isflamethrowercannonsenabledstage
	{
		get
		{
			return m_isflamethrowercannonsenabledstage;
		}
		set
		{
			m_isflamethrowercannonsenabledstage = value;
		}
	}

	public int isuniquehandshakeboardenabled
	{
		get
		{
			return m_isuniquehandshakeboardenabled;
		}
		set
		{
			m_isuniquehandshakeboardenabled = value;
		}
	}

	public int isvanishingsprayenabled
	{
		get
		{
			return m_isvanishingsprayenabled;
		}
		set
		{
			m_isvanishingsprayenabled = value;
		}
	}

	public int stanchionflamethrower
	{
		get
		{
			return m_stanchionflamethrower;
		}
		set
		{
			m_stanchionflamethrower = value;
		}
	}

	public int authenticpodiumskin
	{
		get
		{
			return m_authenticpodiumskin;
		}
		set
		{
			m_authenticpodiumskin = value;
		}
	}

	public int languageregion
	{
		get
		{
			return m_languageregion;
		}
		set
		{
			m_languageregion = value;
		}
	}

	public int ispitchtarpsenabledstage
	{
		get
		{
			return m_ispitchtarpsenabledstage;
		}
		set
		{
			m_ispitchtarpsenabledstage = value;
		}
	}

	public int adboardplacementstage
	{
		get
		{
			return m_adboardplacementstage;
		}
		set
		{
			m_adboardplacementstage = value;
		}
	}

	public int isstadiumdressinguniquestage
	{
		get
		{
			return m_isstadiumdressinguniquestage;
		}
		set
		{
			m_isstadiumdressinguniquestage = value;
		}
	}

	public int iscenterpitchflagenabled
	{
		get
		{
			return m_iscenterpitchflagenabled;
		}
		set
		{
			m_iscenterpitchflagenabled = value;
		}
	}

	public int isgoallinetechhomeleagueenabled
	{
		get
		{
			return m_isgoallinetechhomeleagueenabled;
		}
		set
		{
			m_isgoallinetechhomeleagueenabled = value;
		}
	}

	public int stadiumcrowdmap
	{
		get
		{
			return m_stadiumcrowdmap;
		}
		set
		{
			m_stadiumcrowdmap = value;
		}
	}

	public int isballplinthenabled
	{
		get
		{
			return m_isballplinthenabled;
		}
		set
		{
			m_isballplinthenabled = value;
		}
	}

	public int onpitchgraphics
	{
		get
		{
			return m_onpitchgraphics;
		}
		set
		{
			m_onpitchgraphics = value;
		}
	}

	public int crowdskintonecode
	{
		get
		{
			return m_crowdskintonecode;
		}
		set
		{
			m_crowdskintonecode = value;
		}
	}

	public int isarchwayenabledstage
	{
		get
		{
			return m_isarchwayenabledstage;
		}
		set
		{
			m_isarchwayenabledstage = value;
		}
	}

	public int isgoalnetadsenabledstage
	{
		get
		{
			return m_isgoalnetadsenabledstage;
		}
		set
		{
			m_isgoalnetadsenabledstage = value;
		}
	}

	public int isstadiumdressingenabled
	{
		get
		{
			return m_isstadiumdressingenabled;
		}
		set
		{
			m_isstadiumdressingenabled = value;
		}
	}

	public int goalnetads
	{
		get
		{
			return m_goalnetads;
		}
		set
		{
			m_goalnetads = value;
		}
	}

	public override string ToString()
	{
		if (m_TourName != null)
		{
			return m_TourName;
		}
		return string.Empty;
	}

	public TourNames(Record r)
	{
		m_competitioncolor1g = r.GetAndCheckIntField(FI.competition_competitioncolor1g);
		m_competitioncolor2r = r.GetAndCheckIntField(FI.competition_competitioncolor2r);
		m_competitioncolor2g = r.GetAndCheckIntField(FI.competition_competitioncolor2g);
		m_competitioncolor1b = r.GetAndCheckIntField(FI.competition_competitioncolor1b);
		m_competitioncolor1r = r.GetAndCheckIntField(FI.competition_competitioncolor1r);
		m_competitioncolor2b = r.GetAndCheckIntField(FI.competition_competitioncolor2b);
		m_compcolour1 = Color.FromArgb(m_competitioncolor1r, m_competitioncolor1g, m_competitioncolor1b);
		m_compcolour2 = Color.FromArgb(m_competitioncolor2r, m_competitioncolor2g, m_competitioncolor2b);
		m_ballid = r.GetAndCheckIntField(FI.competition_ballid);
		m_isteampitchflagenabledstage = r.GetAndCheckIntField(FI.competition_isteampitchflagenabledstage);
		m_isstanchionflamethrowerenabledstage = r.GetAndCheckIntField(FI.competition_isstanchionflamethrowerenabledstage);
		m_goaljingle = r.GetAndCheckIntField(FI.competition_goaljingle);
		m_isuniqueleagueflagenabled = r.GetAndCheckIntField(FI.competition_isuniqueleagueflagenabled);
		m_isstadiumdressingenabledstage = r.GetAndCheckIntField(FI.competition_isstadiumdressingenabledstage);
		m_isgoallinetechcompenabledstage = r.GetAndCheckIntField(FI.competition_isgoallinetechcompenabledstage);
		m_abbapenalties = r.GetAndCheckIntField(FI.competition_abbapenalties);
		m_competitionimportance = r.GetAndCheckIntField(FI.competition_competitionimportance);
		m_iscompetitionpodiumenabled = r.GetAndCheckIntField(FI.competition_iscompetitionpodiumenabled);
		m_adboardplacement = r.GetAndCheckIntField(FI.competition_adboardplacement);
		m_iscompetitionscarfenabled = r.GetAndCheckIntField(FI.competition_iscompetitionscarfenabled);
		m_isbannerenabled = r.GetAndCheckIntField(FI.competition_isbannerenabled);
		m_pitchtarps = r.GetAndCheckIntField(FI.competition_pitchtarps);
		m_competitionid = r.GetAndCheckIntField(FI.competition_competitionid);
		m_introconfetti = r.GetAndCheckIntField(FI.competition_introconfetti);
		m_colorregion = r.GetAndCheckIntField(FI.competition_colorregion);
		m_isuniqueadboardscompenabled = r.GetAndCheckIntField(FI.competition_isuniqueadboardscompenabled);
		m_isballplinthenabledstage = r.GetAndCheckIntField(FI.competition_isballplinthenabledstage);
		m_iscompetitionpoleflagenabled = r.GetAndCheckIntField(FI.competition_iscompetitionpoleflagenabled);
		m_isteampitchflagenabled = r.GetAndCheckIntField(FI.competition_isteampitchflagenabled);
		m_isflamethrowercannonsenabled = r.GetAndCheckIntField(FI.competition_isflamethrowercannonsenabled);
		m_iscenterpitchflagenabledstage = r.GetAndCheckIntField(FI.competition_iscenterpitchflagenabledstage);
		m_iscompetitioncrowdcardsenabled = r.GetAndCheckIntField(FI.competition_iscompetitioncrowdcardsenabled);
		m_isarchwayenabled = r.GetAndCheckIntField(FI.competition_isarchwayenabled);
		m_isgoallinetechcompenabled = r.GetAndCheckIntField(FI.competition_isgoallinetechcompenabled);
		m_replay360degree = r.GetAndCheckIntField(FI.competition_replay360degree);
		m_isuniquetrophypedestalenabled = r.GetAndCheckIntField(FI.competition_isuniquetrophypedestalenabled);
		m_isinjuryboardenabled = r.GetAndCheckIntField(FI.competition_isinjuryboardenabled);
		m_isintroconfettienabledstage = r.GetAndCheckIntField(FI.competition_isintroconfettienabledstage);
		m_inflatables = r.GetAndCheckIntField(FI.competition_inflatables);
		m_isstadiumdressingunique = r.GetAndCheckIntField(FI.competition_isstadiumdressingunique);
		m_isvanishingsprayhomeenabled = r.GetAndCheckIntField(FI.competition_isvanishingsprayhomeenabled);
		m_isflamethrowercannonsenabledstage = r.GetAndCheckIntField(FI.competition_isflamethrowercannonsenabledstage);
		m_isuniquehandshakeboardenabled = r.GetAndCheckIntField(FI.competition_isuniquehandshakeboardenabled);
		m_isvanishingsprayenabled = r.GetAndCheckIntField(FI.competition_isvanishingsprayenabled);
		m_stanchionflamethrower = r.GetAndCheckIntField(FI.competition_stanchionflamethrower);
		m_authenticpodiumskin = r.GetAndCheckIntField(FI.competition_authenticpodiumskin);
		m_languageregion = r.GetAndCheckIntField(FI.competition_languageregion);
		m_ispitchtarpsenabledstage = r.GetAndCheckIntField(FI.competition_ispitchtarpsenabledstage);
		m_adboardplacementstage = r.GetAndCheckIntField(FI.competition_adboardplacementstage);
		m_isstadiumdressinguniquestage = r.GetAndCheckIntField(FI.competition_isstadiumdressinguniquestage);
		m_iscenterpitchflagenabled = r.GetAndCheckIntField(FI.competition_iscenterpitchflagenabled);
		m_isgoallinetechhomeleagueenabled = r.GetAndCheckIntField(FI.competition_isgoallinetechhomeleagueenabled);
		m_stadiumcrowdmap = r.GetAndCheckIntField(FI.competition_stadiumcrowdmap);
		m_isballplinthenabled = r.GetAndCheckIntField(FI.competition_isballplinthenabled);
		m_onpitchgraphics = r.GetAndCheckIntField(FI.competition_onpitchgraphics);
		m_crowdskintonecode = r.GetAndCheckIntField(FI.competition_crowdskintonecode);
		m_isarchwayenabledstage = r.GetAndCheckIntField(FI.competition_isarchwayenabledstage);
		m_isgoalnetadsenabledstage = r.GetAndCheckIntField(FI.competition_isgoalnetadsenabledstage);
		m_isstadiumdressingenabled = r.GetAndCheckIntField(FI.competition_isstadiumdressingenabled);
		m_goalnetads = r.GetAndCheckIntField(FI.competition_goalnetads);
	}

	public void SaveCompetition(Record r)
	{
		r.IntField[FI.competition_competitioncolor1r] = m_compcolour1.R;
		r.IntField[FI.competition_competitioncolor1g] = m_compcolour1.G;
		r.IntField[FI.competition_competitioncolor1b] = m_compcolour1.B;
		r.IntField[FI.competition_competitioncolor2r] = m_compcolour2.R;
		r.IntField[FI.competition_competitioncolor2g] = m_compcolour2.G;
		r.IntField[FI.competition_competitioncolor2b] = m_compcolour2.B;
		r.IntField[FI.competition_ballid] = m_ballid;
		r.IntField[FI.competition_isteampitchflagenabledstage] = m_isteampitchflagenabledstage;
		r.IntField[FI.competition_isstanchionflamethrowerenabledstage] = m_isstanchionflamethrowerenabledstage;
		r.IntField[FI.competition_goaljingle] = m_goaljingle;
		r.IntField[FI.competition_isuniqueleagueflagenabled] = m_isuniqueleagueflagenabled;
		r.IntField[FI.competition_isstadiumdressingenabledstage] = m_isstadiumdressingenabledstage;
		r.IntField[FI.competition_isgoallinetechcompenabledstage] = m_isgoallinetechcompenabledstage;
		r.IntField[FI.competition_abbapenalties] = m_abbapenalties;
		r.IntField[FI.competition_competitionimportance] = m_competitionimportance;
		r.IntField[FI.competition_iscompetitionpodiumenabled] = m_iscompetitionpodiumenabled;
		r.IntField[FI.competition_adboardplacement] = m_adboardplacement;
		r.IntField[FI.competition_iscompetitionscarfenabled] = m_iscompetitionscarfenabled;
		r.IntField[FI.competition_isbannerenabled] = m_isbannerenabled;
		r.IntField[FI.competition_pitchtarps] = m_pitchtarps;
		r.IntField[FI.competition_competitionid] = m_competitionid;
		r.IntField[FI.competition_introconfetti] = m_introconfetti;
		r.IntField[FI.competition_colorregion] = m_colorregion;
		r.IntField[FI.competition_isuniqueadboardscompenabled] = m_isuniqueadboardscompenabled;
		r.IntField[FI.competition_isballplinthenabledstage] = m_isballplinthenabledstage;
		r.IntField[FI.competition_iscompetitionpoleflagenabled] = m_iscompetitionpoleflagenabled;
		r.IntField[FI.competition_isteampitchflagenabled] = m_isteampitchflagenabled;
		r.IntField[FI.competition_isflamethrowercannonsenabled] = m_isflamethrowercannonsenabled;
		r.IntField[FI.competition_iscenterpitchflagenabledstage] = m_iscenterpitchflagenabledstage;
		r.IntField[FI.competition_iscompetitioncrowdcardsenabled] = m_iscompetitioncrowdcardsenabled;
		r.IntField[FI.competition_isarchwayenabled] = m_isarchwayenabled;
		r.IntField[FI.competition_isgoallinetechcompenabled] = m_isgoallinetechcompenabled;
		r.IntField[FI.competition_replay360degree] = m_replay360degree;
		r.IntField[FI.competition_isuniquetrophypedestalenabled] = m_isuniquetrophypedestalenabled;
		r.IntField[FI.competition_isinjuryboardenabled] = m_isinjuryboardenabled;
		r.IntField[FI.competition_isintroconfettienabledstage] = m_isintroconfettienabledstage;
		r.IntField[FI.competition_inflatables] = m_inflatables;
		r.IntField[FI.competition_isstadiumdressingunique] = m_isstadiumdressingunique;
		r.IntField[FI.competition_isvanishingsprayhomeenabled] = m_isvanishingsprayhomeenabled;
		r.IntField[FI.competition_isflamethrowercannonsenabledstage] = m_isflamethrowercannonsenabledstage;
		r.IntField[FI.competition_isuniquehandshakeboardenabled] = m_isuniquehandshakeboardenabled;
		r.IntField[FI.competition_isvanishingsprayenabled] = m_isvanishingsprayenabled;
		r.IntField[FI.competition_stanchionflamethrower] = m_stanchionflamethrower;
		r.IntField[FI.competition_authenticpodiumskin] = m_authenticpodiumskin;
		r.IntField[FI.competition_languageregion] = m_languageregion;
		r.IntField[FI.competition_ispitchtarpsenabledstage] = m_ispitchtarpsenabledstage;
		r.IntField[FI.competition_adboardplacementstage] = m_adboardplacementstage;
		r.IntField[FI.competition_isstadiumdressinguniquestage] = m_isstadiumdressinguniquestage;
		r.IntField[FI.competition_iscenterpitchflagenabled] = m_iscenterpitchflagenabled;
		r.IntField[FI.competition_isgoallinetechhomeleagueenabled] = m_isgoallinetechhomeleagueenabled;
		r.IntField[FI.competition_stadiumcrowdmap] = m_stadiumcrowdmap;
		r.IntField[FI.competition_isballplinthenabled] = m_isballplinthenabled;
		r.IntField[FI.competition_onpitchgraphics] = m_onpitchgraphics;
		r.IntField[FI.competition_crowdskintonecode] = m_crowdskintonecode;
		r.IntField[FI.competition_isarchwayenabledstage] = m_isarchwayenabledstage;
		r.IntField[FI.competition_isgoalnetadsenabledstage] = m_isgoalnetadsenabledstage;
		r.IntField[FI.competition_isstadiumdressingenabled] = m_isstadiumdressingenabled;
		r.IntField[FI.competition_goalnetads] = m_goalnetads;
	}

	public void newrow(DataTable t)
	{
		DataRow dataRow = t.NewRow();
		dataRow[0] = m_competitionid.ToString();
		dataRow[1] = m_languagestring;
		dataRow[2] = m_Continent;
		dataRow[3] = m_TourType;
		t.Rows.Add(dataRow);
	}
}
