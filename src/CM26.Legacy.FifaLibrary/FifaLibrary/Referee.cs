using System;
using System.Drawing;

namespace FifaLibrary;

public class Referee : IdObject
{
	private static int c_MaxNLeagues = 8;

	private Bitmap m_EyesTextureBitmap;

	private Bitmap m_FaceTextureBitmap;

	private Bitmap m_HairColorTextureBitmap;

	private Bitmap m_HairAlfaTextureBitmap;

	private Rx3File m_HeadModelFile;

	private Rx3File m_HairModelFile;

	private string m_firstname;

	private string m_surname;

	private DateTime m_birthdate;

	private bool m_gender;

	public int m_refereeheadid_unused;

	public bool m_isinternationalreferee;

	private int m_nationalitycode;

	private Country m_Country;

	private int[] m_leagueids = new int[c_MaxNLeagues];

	private League[] m_Leagues = new League[c_MaxNLeagues];

	private int m_height;

	private int m_weight;

	private int m_eyecolorcode;

	private int m_eyebrowcode;

	private int m_stylecode;

	private int m_cardstrictness;

	private int m_foulstrictness;

	private int m_homecitycode;

	private int m_sockheightcode;

	private int m_haireffecttypecode;

	private int m_hairlinecode;

	private int m_hairpartcode;

	private int m_hairstateid;

	private int m_hairvariationid;

	private int m_sweatid;

	private int m_wrinkleid;

	private int m_proxyhaircolorid;

	private int m_proxyheadclass;

	private int m_isreal;

	private static Random m_Randomizer = new Random();

	private int m_bodytypecode;

	private int m_hairtypecode;

	private int m_headtypecode;

	private int m_headclasscode;

	private int m_haircolorcode;

	private int m_facialhairtypecode;

	private int m_facialhaircolorcode;

	private int m_sideburnscode;

	private int m_skintypecode;

	private int m_skintonecode;

	private int m_jerseysleevelengthcode;

	private int m_shoedesigncode;

	private int m_shoecolorcode1;

	private int m_shoecolorcode2;

	private int m_shoetypecode;

	private int[] c_HairToFacial = new int[10] { 1, 0, 1, 0, 1, 3, 2, 4, 3, 3 };

	private int[] c_RefereHair = new int[83]
	{
		0, 1, 2, 8, 13, 14, 16, 19, 21, 22,
		23, 24, 25, 26, 28, 29, 30, 31, 32, 35,
		36, 37, 38, 40, 41, 42, 43, 45, 46, 47,
		54, 57, 58, 59, 62, 64, 65, 66, 67, 69,
		70, 72, 73, 74, 75, 77, 78, 82, 83, 85,
		86, 87, 88, 89, 90, 92, 93, 95, 98, 100,
		101, 102, 103, 104, 105, 106, 107, 108, 111, 112,
		113, 114, 115, 116, 117, 119, 122, 136, 141, 144,
		145, 149, 150
	};

	public Bitmap EyesTextureBitmap => m_EyesTextureBitmap;

	public Bitmap FaceTextureBitmap => m_FaceTextureBitmap;

	public Bitmap HairColorTextureBitmap => m_HairColorTextureBitmap;

	public Bitmap HairAlfaTextureBitmap => m_HairAlfaTextureBitmap;

	public string firstname
	{
		get
		{
			return m_firstname;
		}
		set
		{
			m_firstname = value;
		}
	}

	public string surname
	{
		get
		{
			return m_surname;
		}
		set
		{
			m_surname = value;
		}
	}

	public DateTime birthdate
	{
		get
		{
			return m_birthdate;
		}
		set
		{
			m_birthdate = value;
		}
	}

	public bool Female
	{
		get
		{
			return m_gender;
		}
		set
		{
			m_gender = value;
		}
	}

	public bool Male
	{
		get
		{
			return !m_gender;
		}
		set
		{
			m_gender = !value;
		}
	}

	public int nationalitycode
	{
		get
		{
			return m_nationalitycode;
		}
		set
		{
			m_nationalitycode = value;
		}
	}

	public Country Country
	{
		get
		{
			return m_Country;
		}
		set
		{
			m_Country = value;
			if (m_Country != null)
			{
				m_nationalitycode = m_Country.Id;
			}
		}
	}

	public int[] leagueids
	{
		get
		{
			return m_leagueids;
		}
		set
		{
			m_leagueids = value;
		}
	}

	public League[] Leagues
	{
		get
		{
			return m_Leagues;
		}
		set
		{
			m_Leagues = value;
		}
	}

	public int height
	{
		get
		{
			return m_height;
		}
		set
		{
			m_height = value;
		}
	}

	public int weight
	{
		get
		{
			return m_weight;
		}
		set
		{
			m_weight = value;
		}
	}

	public int eyecolorcode
	{
		get
		{
			return m_eyecolorcode;
		}
		set
		{
			m_eyecolorcode = value;
			m_EyesTextureBitmap = null;
		}
	}

	public int eyebrowcode
	{
		get
		{
			return m_eyebrowcode;
		}
		set
		{
			m_eyebrowcode = value;
			m_FaceTextureBitmap = null;
		}
	}

	public int stylecode
	{
		get
		{
			return m_stylecode;
		}
		set
		{
			m_stylecode = value;
		}
	}

	public int cardstrictness
	{
		get
		{
			return m_cardstrictness;
		}
		set
		{
			m_cardstrictness = value;
		}
	}

	public int foulstrictness
	{
		get
		{
			return m_foulstrictness;
		}
		set
		{
			m_foulstrictness = value;
		}
	}

	public int bodytypecode
	{
		get
		{
			return m_bodytypecode;
		}
		set
		{
			m_bodytypecode = value;
		}
	}

	public int hairtypecode
	{
		get
		{
			return m_hairtypecode;
		}
		set
		{
			m_hairtypecode = value;
			m_HairModelFile = null;
		}
	}

	public int headtypecode
	{
		get
		{
			return m_headtypecode;
		}
		set
		{
			m_headtypecode = value;
			m_HeadModelFile = null;
		}
	}

	public int haircolorcode
	{
		get
		{
			return m_haircolorcode;
		}
		set
		{
			m_haircolorcode = value;
			m_HairColorTextureBitmap = null;
			m_HairAlfaTextureBitmap = null;
		}
	}

	public int facialhairtypecode
	{
		get
		{
			return m_facialhairtypecode;
		}
		set
		{
			m_facialhairtypecode = value;
			m_FaceTextureBitmap = null;
		}
	}

	public int facialhaircolorcode
	{
		get
		{
			return m_facialhaircolorcode;
		}
		set
		{
			m_facialhaircolorcode = value;
			m_FaceTextureBitmap = null;
		}
	}

	public int sideburnscode
	{
		get
		{
			return m_sideburnscode;
		}
		set
		{
			m_sideburnscode = value;
			m_FaceTextureBitmap = null;
		}
	}

	public int skintypecode
	{
		get
		{
			return m_skintypecode;
		}
		set
		{
			m_skintypecode = value;
			m_FaceTextureBitmap = null;
		}
	}

	public int skintonecode
	{
		get
		{
			return m_skintonecode;
		}
		set
		{
			m_skintonecode = value;
			m_FaceTextureBitmap = null;
		}
	}

	public int jerseysleevelengthcode
	{
		get
		{
			return m_jerseysleevelengthcode;
		}
		set
		{
			m_jerseysleevelengthcode = value;
		}
	}

	public int shoedesigncode
	{
		get
		{
			return m_shoedesigncode;
		}
		set
		{
			m_shoedesigncode = value;
		}
	}

	public int shoecolorcode1
	{
		get
		{
			return m_shoecolorcode1;
		}
		set
		{
			m_shoecolorcode1 = value;
		}
	}

	public int shoecolorcode2
	{
		get
		{
			return m_shoecolorcode2;
		}
		set
		{
			m_shoecolorcode2 = value;
		}
	}

	public int shoetypecode
	{
		get
		{
			return m_shoetypecode;
		}
		set
		{
			m_shoetypecode = value;
		}
	}

	public void SetLeague(int id)
	{
		for (int i = 0; i < m_Leagues.Length; i++)
		{
			if (m_leagueids[i] == id)
			{
				return;
			}
		}
		for (int j = 0; j < m_Leagues.Length; j++)
		{
			if (m_leagueids[j] == 0)
			{
				m_leagueids[j] = id;
				break;
			}
		}
	}

	public int CntLeagues()
	{
		int num = 0;
		for (int i = 0; i < m_Leagues.Length; i++)
		{
			if (m_leagueids[i] != 0)
			{
				num++;
			}
		}
		return num;
	}

	public int GetMainLeague()
	{
		for (int i = 0; i < m_Leagues.Length; i++)
		{
			if (m_leagueids[i] != 0)
			{
				return m_leagueids[i];
			}
		}
		return 1;
	}

	public bool IsInLeague(League league)
	{
		for (int i = 0; i < m_Leagues.Length; i++)
		{
			if (Leagues[i] == league)
			{
				return true;
			}
		}
		return false;
	}

	public Referee(Record r)
		: base(r.IntField[FI.referee_refereeid])
	{
		Load(r);
	}

	public Referee(int refereeid)
		: base(refereeid)
	{
		int newRefereeHeadId = FifaEnvironment.Year == 26 || FifaEnvironment.Referees == null
			? -1000 - refereeid
			: FifaEnvironment.Referees.GetNewRefereeHeadId();
		m_refereeheadid_unused = ((newRefereeHeadId > -1000) ? (-1000) : newRefereeHeadId);
		m_firstname = "";
		m_surname = "Referee " + base.Id;
		m_birthdate = new DateTime(1970, 6, 15);
		m_nationalitycode = 0;
		for (int i = 0; i < m_Leagues.Length; i++)
		{
			m_leagueids[i] = 0;
			m_Leagues[i] = null;
		}
		m_height = 180;
		m_weight = 75;
		m_bodytypecode = 1;
		m_shoedesigncode = 0;
		m_shoecolorcode1 = 30;
		m_shoecolorcode2 = 31;
		m_shoetypecode = 72;
		m_jerseysleevelengthcode = 0;
		m_eyecolorcode = 1;
		m_eyebrowcode = 0;
		m_facialhairtypecode = 0;
		m_facialhaircolorcode = 0;
		m_hairtypecode = 0;
		m_haircolorcode = 0;
		m_headtypecode = 1;
		m_headclasscode = 1;
		m_sideburnscode = 0;
		m_skintypecode = 0;
		m_skintonecode = 2;
		m_gender = false;
		m_stylecode = 1;
		m_cardstrictness = 1;
		m_foulstrictness = 1;
		m_homecitycode = 5;
		m_sockheightcode = 0;
		m_haireffecttypecode = 0;
		m_hairlinecode = 0;
		m_hairpartcode = 0;
		m_hairstateid = 0;
		m_hairvariationid = 0;
		m_sweatid = 0;
		m_wrinkleid = 0;
		m_proxyhaircolorid = 1;
		m_proxyheadclass = 1;
		m_isreal = 0;
	}

	public void Load(Record r)
	{
		m_firstname = r.StringField[FI.referee_firstname];
		m_surname = r.StringField[FI.referee_surname];
		m_nationalitycode = r.GetAndCheckIntField(FI.referee_nationalitycode);
		int andCheckIntField = r.GetAndCheckIntField(FI.referee_leagueid);
		SetLeague(andCheckIntField);
		DateTime dateTime = FifaUtil.ConvertToDate(r.GetAndCheckIntField(FI.referee_birthdate));
		if (dateTime.Year < 1900)
		{
			dateTime = new DateTime(1980, 1, 1);
		}
		m_birthdate = dateTime;
		m_height = r.GetAndCheckIntField(FI.referee_height);
		m_weight = r.GetAndCheckIntField(FI.referee_weight);
		m_bodytypecode = r.GetAndCheckIntField(FI.referee_bodytypecode) - 1;
		m_shoedesigncode = r.GetAndCheckIntField(FI.referee_shoedesigncode);
		m_shoecolorcode1 = r.GetAndCheckIntField(FI.referee_shoecolorcode1);
		m_shoecolorcode2 = r.GetAndCheckIntField(FI.referee_shoecolorcode2);
		m_shoetypecode = r.GetAndCheckIntField(FI.referee_shoetypecode);
		m_jerseysleevelengthcode = r.GetAndCheckIntField(FI.referee_jerseysleevelengthcode);
		m_eyecolorcode = r.GetAndCheckIntField(FI.referee_eyecolorcode);
		m_eyebrowcode = r.GetAndCheckIntField(FI.referee_eyebrowcode);
		m_facialhairtypecode = r.GetAndCheckIntField(FI.referee_facialhairtypecode);
		m_facialhaircolorcode = r.GetAndCheckIntField(FI.referee_facialhaircolorcode);
		m_hairtypecode = r.GetAndCheckIntField(FI.referee_hairtypecode);
		m_haircolorcode = r.GetAndCheckIntField(FI.referee_haircolorcode);
		m_headtypecode = r.GetAndCheckIntField(FI.referee_headtypecode);
		m_headclasscode = r.GetAndCheckIntField(FI.referee_headclasscode);
		m_sideburnscode = r.GetAndCheckIntField(FI.referee_sideburnscode);
		m_skintypecode = r.GetAndCheckIntField(FI.referee_skintypecode);
		m_skintonecode = r.GetAndCheckIntField(FI.referee_skintonecode);
		m_stylecode = r.GetAndCheckIntField(FI.referee_stylecode);
		m_cardstrictness = r.GetAndCheckIntField(FI.referee_cardstrictness);
		m_foulstrictness = r.GetAndCheckIntField(FI.referee_foulstrictness);
		m_homecitycode = r.GetAndCheckIntField(FI.referee_homecitycode);
		m_sockheightcode = r.GetAndCheckIntField(FI.referee_sockheightcode);
		m_haireffecttypecode = r.GetAndCheckIntField(FI.referee_haireffecttypecode);
		m_hairlinecode = r.GetAndCheckIntField(FI.referee_hairlinecode);
		m_hairpartcode = r.GetAndCheckIntField(FI.referee_hairpartcode);
		m_hairstateid = r.GetAndCheckIntField(FI.referee_hairstateid);
		m_hairvariationid = r.GetAndCheckIntField(FI.referee_hairvariationid);
		m_sweatid = r.GetAndCheckIntField(FI.referee_sweatid);
		m_wrinkleid = r.GetAndCheckIntField(FI.referee_wrinkleid);
		m_proxyhaircolorid = r.GetAndCheckIntField(FI.referee_proxyhaircolorid);
		m_proxyheadclass = r.GetAndCheckIntField(FI.referee_proxyheadclass);
		m_gender = ((r.GetAndCheckIntField(FI.referee_gender) != 0) ? true : false);
		if (FI.referee_isreal >= 0)
		{
			m_isreal = r.GetAndCheckIntField(FI.referee_isreal);
		}
	}

	public void FillFromLeagueRefereeLinks(Record r)
	{
		int andCheckIntField = r.GetAndCheckIntField(FI.leaguerefereelinks_leagueid);
		SetLeague(andCheckIntField);
	}

	public override string ToString()
	{
		return m_surname + " " + m_firstname;
	}

	public string DatabaseString()
	{
		return ToString();
	}

	public void LinkCountry(CountryList countryList)
	{
		if (countryList == null)
		{
			return;
		}
		m_Country = (Country)countryList.SearchId(m_nationalitycode);
		if (m_Country == null)
		{
			if (countryList.Count > 0)
			{
				m_Country = (Country)countryList[0];
				m_nationalitycode = m_Country.Id;
			}
			else
			{
				m_nationalitycode = 0;
			}
		}
	}

	public void LinkLeague(LeagueList leagueList)
	{
		if (leagueList == null)
		{
			return;
		}
		for (int i = 0; i < m_Leagues.Length; i++)
		{
			if (m_leagueids[i] != 0)
			{
				m_Leagues[i] = (League)leagueList.SearchId(m_leagueids[i]);
				if (m_Leagues[i] == null)
				{
					m_leagueids[i] = 0;
					m_Leagues[i] = null;
				}
			}
		}
	}

	public string GenericHairModelFileName()
	{
		return "data/sceneassets/hair/hair_" + m_hairtypecode + "_1_0.rx3";
	}

	public string GenericFaceTextureFileName()
	{
		return "data/sceneassets/faces/face_0_1_0_" + m_eyebrowcode + "_" + m_sideburnscode + "_" + m_facialhaircolorcode + "_" + m_facialhairtypecode + "_" + m_skintypecode + "_" + m_skintonecode + "_textures.rx3";
	}

	public string GenericHeadModelFileName()
	{
		return "data/sceneassets/heads/head_" + m_headtypecode + "_1.rx3";
	}

	public override IdObject Clone(int refereeid)
	{
		Referee referee = (Referee)base.Clone(refereeid);
		if (referee == null)
		{
			return null;
		}
		referee.m_refereeheadid_unused = m_refereeheadid_unused;
		referee.m_firstname = "";
		referee.m_surname = "Referee " + referee.Id;
		return referee;
	}

	public string GenericBearTextureFileName()
	{
		return GenericBearTextureFileName(m_skintonecode, m_facialhairtypecode, m_facialhaircolorcode);
	}

	public string RfsGenericBearTextureFileName()
	{
		return RfsGenericBearTextureFileName(m_facialhairtypecode, m_facialhaircolorcode);
	}

	private static string GenericBearTextureFileName(int skintone, int facialhairtype, int facialhaircolor)
	{
		return "data/sceneassets/genheadtex/beard_" + skintone + "_" + facialhairtype + "_" + facialhaircolor + ".rx3";
	}

	private static string RfsGenericBearTextureFileName(int facialhairtype, int facialhaircolor)
	{
		return "data/sceneassets/genheadtex/beard_" + facialhairtype + "_" + facialhaircolor + ".rx3";
	}

	public string GenericBrowTextureFileName()
	{
		return GenericBrowTextureFileName(m_skintonecode, m_eyebrowcode, m_facialhaircolorcode);
	}

	public string RfsGenericBrowTextureFileName()
	{
		return RfsGenericBrowTextureFileName(m_eyebrowcode, m_facialhaircolorcode);
	}

	private static string GenericBrowTextureFileName(int skintone, int eyebrow, int facialhaircolor)
	{
		return "data/sceneassets/genheadtex/brow_" + skintone + "_" + eyebrow + "_" + facialhaircolor + ".rx3";
	}

	private static string RfsGenericBrowTextureFileName(int eyebrow, int facialhaircolor)
	{
		return "data/sceneassets/genheadtex/brow_" + eyebrow + "_" + facialhaircolor + ".rx3";
	}

	public void CleanFaceTexture()
	{
		if (m_FaceTextureBitmap != null)
		{
			m_FaceTextureBitmap.Dispose();
		}
		m_FaceTextureBitmap = null;
	}

	public Bitmap GetFaceTexture()
	{
		if (m_FaceTextureBitmap != null)
		{
			return m_FaceTextureBitmap;
		}
		m_FaceTextureBitmap = BuildGenericFaceTexture();
		return m_FaceTextureBitmap;
	}

	private Bitmap BuildGenericFaceTexture()
	{
		Bitmap bitmap = FifaEnvironment.GetBmpFromRx3(Player.GenericSkinTextureFileName(m_skintonecode, m_skintypecode));
		if (bitmap == null)
		{
			return null;
		}
		Rectangle destRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		Bitmap bmpFromRx;
		if (m_facialhairtypecode != 0)
		{
			bmpFromRx = FifaEnvironment.GetBmpFromRx3(RfsGenericBearTextureFileName());
			if (bmpFromRx == null)
			{
				bmpFromRx = FifaEnvironment.GetBmpFromRx3(GenericBearTextureFileName());
			}
			if (bmpFromRx != null)
			{
				bitmap = GraphicUtil.Overlap(bitmap, bmpFromRx, destRectangle);
			}
		}
		bmpFromRx = FifaEnvironment.GetBmpFromRx3(RfsGenericBrowTextureFileName());
		if (bmpFromRx == null)
		{
			bmpFromRx = FifaEnvironment.GetBmpFromRx3(GenericBrowTextureFileName());
		}
		if (bmpFromRx != null)
		{
			bitmap = GraphicUtil.Overlap(bitmap, bmpFromRx, destRectangle);
		}
		return bitmap;
	}

	public string GenericEyesTextureFileName()
	{
		return "data/sceneassets/heads/eyes_" + m_eyecolorcode + "_1_textures.rx3";
	}

	public Bitmap GetEyesTexture()
	{
		if (m_EyesTextureBitmap != null)
		{
			return m_EyesTextureBitmap;
		}
		string text = null;
		text = GenericEyesTextureFileName();
		m_EyesTextureBitmap = FifaEnvironment.GetBmpFromRx3(text, 0);
		return m_EyesTextureBitmap;
	}

	public void CleanEyesTexture()
	{
		if (m_EyesTextureBitmap != null)
		{
			m_EyesTextureBitmap.Dispose();
		}
		m_EyesTextureBitmap = null;
	}

	public string GenericHairColorTextureFileName()
	{
		return "data/sceneassets/hair/haircolour_" + m_hairtypecode + "_0_" + m_haircolorcode + "_0_1_textures.rx3";
	}

	public void CleanHairColorTexture()
	{
		if (m_HairColorTextureBitmap != null)
		{
			m_HairColorTextureBitmap.Dispose();
		}
		m_HairColorTextureBitmap = null;
	}

	public Bitmap GetHairColorTexture()
	{
		if (m_HairColorTextureBitmap != null)
		{
			return m_HairColorTextureBitmap;
		}
		GetHairTextures();
		return m_HairColorTextureBitmap;
	}

	public Bitmap GetGenericHairColorTexture()
	{
		Bitmap[] bmpsFromRx = FifaEnvironment.GetBmpsFromRx3(GenericHairTexturesFileName());
		if (bmpsFromRx != null)
		{
			if (m_haircolorcode >= Player.s_GenericColors.Length)
			{
				m_haircolorcode = 0;
			}
			return GraphicUtil.MultiplyColorToBitmap(bmpsFromRx[1], Player.s_GenericColors[m_haircolorcode], Player.s_GenericColorsDivisor, preserveAlfa: false);
		}
		return null;
	}

	public string GenericHairTexturesFileName()
	{
		return "data/sceneassets/hair/hair_" + m_hairtypecode + "_1_textures.rx3";
	}

	public Bitmap[] GetHairTextures()
	{
		if (m_HairAlfaTextureBitmap != null && m_HairColorTextureBitmap != null)
		{
			return new Bitmap[2] { m_HairAlfaTextureBitmap, m_HairColorTextureBitmap };
		}
		Bitmap[] bmpsFromRx = FifaEnvironment.GetBmpsFromRx3(GenericHairTexturesFileName());
		m_HairAlfaTextureBitmap = bmpsFromRx[0];
		if (m_haircolorcode >= Player.s_GenericColors.Length)
		{
			m_haircolorcode = 0;
		}
		m_HairColorTextureBitmap = GraphicUtil.MultiplyColorToBitmap(bmpsFromRx[1], Player.s_GenericColors[m_haircolorcode], Player.s_GenericColorsDivisor, preserveAlfa: false);
		return bmpsFromRx;
	}

	public void CleanHairAlfaTexture()
	{
		if (m_HairAlfaTextureBitmap != null)
		{
			m_HairAlfaTextureBitmap.Dispose();
		}
		m_HairAlfaTextureBitmap = null;
	}

	public Bitmap GetHairAlfaTexture()
	{
		if (m_HairAlfaTextureBitmap != null)
		{
			return m_HairAlfaTextureBitmap;
		}
		GetHairTextures();
		return m_HairAlfaTextureBitmap;
	}

	public void CleanHairTextures()
	{
		CleanHairColorTexture();
		CleanHairAlfaTexture();
	}

	public string GenericHairAlfaTextureFileName()
	{
		return "data/sceneassets/hair/hair_" + m_hairtypecode + "_0_1_textures.rx3";
	}

	public Rx3File GetHeadModel()
	{
		if (m_HeadModelFile != null)
		{
			return m_HeadModelFile;
		}
		Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float32;
		m_HeadModelFile = FifaEnvironment.GetRx3FromZdata(GenericHeadModelFileName());
		return m_HeadModelFile;
	}

	public void CleanHeadModel()
	{
		m_HeadModelFile = null;
	}

	public void CleanHead()
	{
		CleanFaceTexture();
		CleanEyesTexture();
		CleanHeadModel();
	}

	public Rx3File GetHairModel()
	{
		if (m_HairModelFile != null)
		{
			return m_HairModelFile;
		}
		Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float16;
		m_HairModelFile = FifaEnvironment.GetRx3FromZdata(GenericHairModelFileName());
		return m_HairModelFile;
	}

	public void CleanHairModel()
	{
		m_HairModelFile = null;
	}

	public void CleanHair()
	{
		CleanHairModel();
		CleanHairTextures();
	}

	public void CleanAllHead()
	{
		CleanHair();
		CleanHead();
	}

	public static string PhotoBigFileName(int refereeId)
	{
		return "data/ui/artassets/referee/ref_" + refereeId + ".big";
	}

	public string PhotoBigFileName()
	{
		return PhotoBigFileName(base.Id);
	}

	public string PhotoTemplateFileName()
	{
		return "data/ui/artassets/referee/ref_#.big";
	}

	public string PhotoDdsFileName()
	{
		return "2";
	}

	public Bitmap GetPhoto()
	{
		return FifaEnvironment.GetArtasset(PhotoBigFileName());
	}

	public bool SetPhoto(Bitmap bitmap)
	{
		return FifaEnvironment.SetArtasset(PhotoTemplateFileName(), PhotoDdsFileName(), base.Id, bitmap);
	}

	public bool DeletePhoto()
	{
		return FifaEnvironment.DeleteFromZdata(PhotoBigFileName());
	}

	public void SaveReferee(Record r)
	{
		r.IntField[FI.referee_refereeid] = base.Id;
		r.IntField[FI.referee_birthdate] = FifaUtil.ConvertFromDate(m_birthdate);
		r.StringField[FI.referee_firstname] = m_firstname;
		r.StringField[FI.referee_surname] = m_surname;
		r.StringField[FI.referee_firstname] = m_firstname;
		r.StringField[FI.referee_surname] = m_surname;
		r.IntField[FI.referee_nationalitycode] = m_nationalitycode;
		r.IntField[FI.referee_leagueid] = GetMainLeague();
		r.IntField[FI.referee_height] = m_height;
		r.IntField[FI.referee_weight] = m_weight;
		r.IntField[FI.referee_bodytypecode] = m_bodytypecode + 1;
		r.IntField[FI.referee_shoedesigncode] = m_shoedesigncode;
		r.IntField[FI.referee_shoecolorcode1] = m_shoecolorcode1;
		r.IntField[FI.referee_shoecolorcode2] = m_shoecolorcode2;
		r.IntField[FI.referee_shoetypecode] = m_shoetypecode;
		r.IntField[FI.referee_jerseysleevelengthcode] = m_jerseysleevelengthcode;
		r.IntField[FI.referee_eyecolorcode] = m_eyecolorcode;
		r.IntField[FI.referee_eyebrowcode] = m_eyebrowcode;
		r.IntField[FI.referee_facialhairtypecode] = m_facialhairtypecode;
		r.IntField[FI.referee_facialhaircolorcode] = m_facialhaircolorcode;
		r.IntField[FI.referee_hairtypecode] = m_hairtypecode;
		r.IntField[FI.referee_haircolorcode] = m_haircolorcode;
		r.IntField[FI.referee_headtypecode] = m_headtypecode;
		r.IntField[FI.referee_headclasscode] = m_headclasscode;
		r.IntField[FI.referee_sideburnscode] = 0;
		r.IntField[FI.referee_skintypecode] = m_skintypecode;
		r.IntField[FI.referee_skintonecode] = m_skintonecode;
		r.IntField[FI.referee_stylecode] = m_stylecode;
		r.IntField[FI.referee_cardstrictness] = m_cardstrictness;
		r.IntField[FI.referee_foulstrictness] = m_foulstrictness;
		r.IntField[FI.referee_homecitycode] = m_homecitycode;
		r.IntField[FI.referee_sockheightcode] = m_sockheightcode;
		r.IntField[FI.referee_haireffecttypecode] = m_haireffecttypecode;
		r.IntField[FI.referee_hairlinecode] = m_hairlinecode;
		r.IntField[FI.referee_hairpartcode] = m_hairpartcode;
		r.IntField[FI.referee_hairstateid] = m_hairstateid;
		r.IntField[FI.referee_hairvariationid] = m_hairvariationid;
		r.IntField[FI.referee_sweatid] = m_sweatid;
		r.IntField[FI.referee_wrinkleid] = m_wrinkleid;
		r.IntField[FI.referee_proxyhaircolorid] = m_proxyhaircolorid;
		r.IntField[FI.referee_proxyheadclass] = m_proxyheadclass;
		r.IntField[FI.referee_gender] = (m_gender ? 1 : 0);
		if (FI.referee_isreal >= 0)
		{
			r.IntField[FI.referee_isreal] = m_isreal;
		}
	}

	public void SaveLeagueRefereeLinks(Record r, int index)
	{
		r.IntField[FI.leaguerefereelinks_refereeid] = base.Id;
		r.IntField[FI.leaguerefereelinks_leagueid] = m_leagueids[index];
	}

	public void BuildHairPartsTextures()
	{
		if (m_HairColorTextureBitmap != null && m_HairAlfaTextureBitmap != null && m_HairColorTextureBitmap != null && m_HairAlfaTextureBitmap != null)
		{
			GraphicUtil.GetAlfaFromChannel((Bitmap)m_HairColorTextureBitmap.Clone(), m_HairAlfaTextureBitmap, 2);
			GraphicUtil.GetAlfaFromChannel((Bitmap)m_HairColorTextureBitmap.Clone(), m_HairAlfaTextureBitmap, 1);
		}
	}

	public bool CreateHead3D(string xFileName)
	{
		BuildHairPartsTextures();
		Rx3File headModel = GetHeadModel();
		if (m_FaceTextureBitmap == null || headModel == null)
		{
			return false;
		}
		new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], m_FaceTextureBitmap);
		if (m_EyesTextureBitmap == null || headModel == null)
		{
			return false;
		}
		new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], m_EyesTextureBitmap);
		headModel = GetHairModel();
		if (m_FaceTextureBitmap == null || headModel == null)
		{
			return false;
		}
		new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], m_HairColorTextureBitmap);
		return true;
	}

	public void RandomizeCaucasianAppearance()
	{
		if (m_Randomizer.Next(1, 11) <= 7)
		{
			m_headtypecode = GenericHead.c_CaucasicModels[m_Randomizer.Next(0, GenericHead.c_CaucasicModels.Length)];
		}
		else
		{
			m_headtypecode = GenericHead.c_LatinModels[m_Randomizer.Next(0, GenericHead.c_LatinModels.Length)];
		}
		m_hairtypecode = c_RefereHair[m_Randomizer.Next(0, c_RefereHair.Length)];
		m_haircolorcode = m_Randomizer.Next(0, 8);
		m_sideburnscode = 0;
		m_skintonecode = m_Randomizer.Next(1, 5);
		m_skintypecode = 2;
		m_eyebrowcode = m_Randomizer.Next(0, 3);
		m_eyecolorcode = m_Randomizer.Next(1, 8);
		m_facialhairtypecode = 0;
		if (m_facialhairtypecode == 2 || m_facialhairtypecode > 7)
		{
			m_facialhairtypecode = 0;
		}
		if (m_facialhairtypecode == 2)
		{
			m_facialhairtypecode = 0;
		}
		m_facialhaircolorcode = c_HairToFacial[m_haircolorcode];
	}

	public void RandomizeAsiaticAppearance()
	{
		if (m_Randomizer.Next(1, 11) <= 9)
		{
			m_headtypecode = GenericHead.c_AsiaticModels[m_Randomizer.Next(0, GenericHead.c_AsiaticModels.Length)];
		}
		else
		{
			m_headtypecode = GenericHead.c_CaucasicModels[m_Randomizer.Next(0, GenericHead.c_CaucasicModels.Length)];
		}
		m_hairtypecode = c_RefereHair[m_Randomizer.Next(0, c_RefereHair.Length)];
		m_haircolorcode = m_Randomizer.Next(1, 6);
		if (m_haircolorcode == 2 || m_haircolorcode == 4)
		{
			m_haircolorcode = 1;
		}
		if (m_haircolorcode == 3)
		{
			m_haircolorcode = 1;
		}
		m_sideburnscode = 0;
		m_skintonecode = m_Randomizer.Next(2, 7);
		if (m_skintonecode == 3)
		{
			m_skintonecode = 4;
		}
		m_skintypecode = 2;
		m_eyecolorcode = m_Randomizer.Next(3, 8);
		m_eyebrowcode = m_Randomizer.Next(0, 3);
		m_facialhairtypecode = 0;
		if (m_facialhairtypecode == 2 || m_facialhairtypecode > 7)
		{
			m_facialhairtypecode = 0;
		}
		m_facialhaircolorcode = c_HairToFacial[m_haircolorcode];
	}

	public void RandomizeAfricanAppearance()
	{
		if (m_Randomizer.Next(1, 11) <= 7)
		{
			m_headtypecode = GenericHead.c_AfricanModels[m_Randomizer.Next(0, GenericHead.c_AfricanModels.Length)];
		}
		else
		{
			m_headtypecode = GenericHead.c_LatinModels[m_Randomizer.Next(0, GenericHead.c_LatinModels.Length)];
		}
		m_hairtypecode = c_RefereHair[m_Randomizer.Next(0, c_RefereHair.Length)];
		m_haircolorcode = 1;
		m_sideburnscode = 0;
		m_skintonecode = m_Randomizer.Next(6, 11);
		if (m_skintonecode == 7)
		{
			m_skintonecode = 8;
		}
		m_skintypecode = 2;
		m_eyecolorcode = m_Randomizer.Next(3, 5);
		m_eyebrowcode = m_Randomizer.Next(0, 3);
		m_facialhairtypecode = 0;
		if (m_facialhairtypecode == 2 || m_facialhairtypecode > 7)
		{
			m_facialhairtypecode = 0;
		}
		if (m_facialhairtypecode == 2)
		{
			m_facialhairtypecode = 0;
		}
		m_facialhaircolorcode = c_HairToFacial[m_haircolorcode];
	}

	public void RandomizeLatinAppearance()
	{
		if (m_Randomizer.Next(1, 11) <= 7)
		{
			m_headtypecode = GenericHead.c_LatinModels[m_Randomizer.Next(0, GenericHead.c_LatinModels.Length)];
		}
		else
		{
			m_headtypecode = GenericHead.c_CaucasicModels[m_Randomizer.Next(0, GenericHead.c_CaucasicModels.Length)];
		}
		m_hairtypecode = c_RefereHair[m_Randomizer.Next(0, c_RefereHair.Length)];
		m_haircolorcode = 1;
		m_sideburnscode = 0;
		m_skintonecode = m_Randomizer.Next(4, 7);
		m_skintypecode = 2;
		m_eyecolorcode = m_Randomizer.Next(3, 8);
		m_facialhairtypecode = 0;
		if (m_facialhairtypecode == 2 || m_facialhairtypecode > 7)
		{
			m_facialhairtypecode = 0;
		}
		if (m_facialhairtypecode == 2)
		{
			m_facialhairtypecode = 0;
		}
		m_facialhaircolorcode = c_HairToFacial[m_haircolorcode];
	}
}
