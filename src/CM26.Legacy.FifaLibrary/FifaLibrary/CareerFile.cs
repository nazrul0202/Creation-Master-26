using System.Data;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary;

public class CareerFile
{
	private ToolStripProgressBar m_ProgressBar;

	private string m_InGameName;

	protected string m_FileName;

	protected string m_XmlFileName;

	protected DataSet m_DescriptorDataSet;

	protected FifaPlatform m_Platform;

	private char[] m_HeaderSignature;

	private byte[] m_CryptArea;

	private uint m_CrcEaHeader;

	private long m_CrcEaHeaderPosition;

	private long m_CrcPosition;

	private DbFile[] m_Database = new DbFile[4];

	private int m_NDatabases;

	private static uint[] s_CrcTable = new uint[256]
	{
		0u, 79764919u, 159529838u, 222504665u, 319059676u, 398814059u, 445009330u, 507990021u, 638119352u, 583659535u,
		797628118u, 726387553u, 890018660u, 835552979u, 1015980042u, 944750013u, 1276238704u, 1221641927u, 1167319070u, 1095957929u,
		1595256236u, 1540665371u, 1452775106u, 1381403509u, 1780037320u, 1859660671u, 1671105958u, 1733955601u, 2031960084u, 2111593891u,
		1889500026u, 1952343757u, 2552477408u, 2632100695u, 2443283854u, 2506133561u, 2334638140u, 2414271883u, 2191915858u, 2254759653u,
		3190512472u, 3135915759u, 3081330742u, 3009969537u, 2905550212u, 2850959411u, 2762807018u, 2691435357u, 3560074640u, 3505614887u,
		3719321342u, 3648080713u, 3342211916u, 3287746299u, 3467911202u, 3396681109u, 4063920168u, 4143685023u, 4223187782u, 4286162673u,
		3779000052u, 3858754371u, 3904687514u, 3967668269u, 881225847u, 809987520u, 1023691545u, 969234094u, 662832811u, 591600412u,
		771767749u, 717299826u, 311336399u, 374308984u, 453813921u, 533576470u, 25881363u, 88864420u, 134795389u, 214552010u,
		2023205639u, 2086057648u, 1897238633u, 1976864222u, 1804852699u, 1867694188u, 1645340341u, 1724971778u, 1587496639u, 1516133128u,
		1461550545u, 1406951526u, 1302016099u, 1230646740u, 1142491917u, 1087903418u, 2896545431u, 2825181984u, 2770861561u, 2716262478u,
		3215044683u, 3143675388u, 3055782693u, 3001194130u, 2326604591u, 2389456536u, 2200899649u, 2280525302u, 2578013683u, 2640855108u,
		2418763421u, 2498394922u, 3769900519u, 3832873040u, 3912640137u, 3992402750u, 4088425275u, 4151408268u, 4197601365u, 4277358050u,
		3334271071u, 3263032808u, 3476998961u, 3422541446u, 3585640067u, 3514407732u, 3694837229u, 3640369242u, 1762451694u, 1842216281u,
		1619975040u, 1682949687u, 2047383090u, 2127137669u, 1938468188u, 2001449195u, 1325665622u, 1271206113u, 1183200824u, 1111960463u,
		1543535498u, 1489069629u, 1434599652u, 1363369299u, 622672798u, 568075817u, 748617968u, 677256519u, 907627842u, 853037301u,
		1067152940u, 995781531u, 51762726u, 131386257u, 177728840u, 240578815u, 269590778u, 349224269u, 429104020u, 491947555u,
		4046411278u, 4126034873u, 4172115296u, 4234965207u, 3794477266u, 3874110821u, 3953728444u, 4016571915u, 3609705398u, 3555108353u,
		3735388376u, 3664026991u, 3290680682u, 3236090077u, 3449943556u, 3378572211u, 3174993278u, 3120533705u, 3032266256u, 2961025959u,
		2923101090u, 2868635157u, 2813903052u, 2742672763u, 2604032198u, 2683796849u, 2461293480u, 2524268063u, 2284983834u, 2364738477u,
		2175806836u, 2238787779u, 1569362073u, 1498123566u, 1409854455u, 1355396672u, 1317987909u, 1246755826u, 1192025387u, 1137557660u,
		2072149281u, 2135122070u, 1912620623u, 1992383480u, 1753615357u, 1816598090u, 1627664531u, 1707420964u, 295390185u, 358241886u,
		404320391u, 483945776u, 43990325u, 106832002u, 186451547u, 266083308u, 932423249u, 861060070u, 1041341759u, 986742920u,
		613929101u, 542559546u, 756411363u, 701822548u, 3316196985u, 3244833742u, 3425377559u, 3370778784u, 3601682597u, 3530312978u,
		3744426955u, 3689838204u, 3819031489u, 3881883254u, 3928223919u, 4007849240u, 4037393693u, 4100235434u, 4180117107u, 4259748804u,
		2310601993u, 2373574846u, 2151335527u, 2231098320u, 2596047829u, 2659030626u, 2470359227u, 2550115596u, 2947551409u, 2876312838u,
		2788305887u, 2733848168u, 3165939309u, 3094707162u, 3040238851u, 2985771188u
	};

	public ToolStripProgressBar ProgressBar
	{
		set
		{
			m_ProgressBar = value;
			for (int i = 0; i < m_NDatabases; i++)
			{
				m_Database[i].ProgressBar = m_ProgressBar;
			}
		}
	}

	public string InGameName
	{
		get
		{
			return m_InGameName;
		}
		set
		{
			m_InGameName = value;
		}
	}

	public string FileName => m_FileName;

	public string XmlFileName => m_XmlFileName;

	public DataSet DescriptorDataSet
	{
		get
		{
			return m_DescriptorDataSet;
		}
		set
		{
			m_DescriptorDataSet = value;
		}
	}

	public FifaPlatform Platform
	{
		get
		{
			return m_Platform;
		}
		set
		{
			m_Platform = value;
		}
	}

	public long CrcPosition => m_CrcPosition;

	public DbFile[] Databases => m_Database;

	public int NDatabases => m_NDatabases;

	public CareerFile(string careerFileName, string xmlFileName, ToolStripProgressBar progressBar)
	{
		m_ProgressBar = progressBar;
		m_FileName = careerFileName;
		m_XmlFileName = xmlFileName;
		m_DescriptorDataSet = null;
		Load();
	}

	public CareerFile(string careerFileName, string xmlFileName)
	{
		m_ProgressBar = null;
		m_FileName = careerFileName;
		m_XmlFileName = xmlFileName;
		m_DescriptorDataSet = null;
		Load();
	}

	public bool Load()
	{
		if (!LoadXml())
		{
			return false;
		}
		return LoadEA(m_FileName);
	}

	public bool LoadXml(string xmlFileName)
	{
		if (!File.Exists(xmlFileName))
		{
			return false;
		}
		m_XmlFileName = xmlFileName;
		m_DescriptorDataSet = new DataSet("XML_Descriptor");
		m_DescriptorDataSet.ReadXml(m_XmlFileName);
		return true;
	}

	public bool LoadXml()
	{
		if (m_XmlFileName == null)
		{
			return false;
		}
		return LoadXml(m_XmlFileName);
	}

	public bool LoadEA(string fileName)
	{
		bool num = FileName.Contains("16");
		FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
		DbReader dbReader = new DbReader(fileStream, FifaPlatform.PC);
		dbReader.BaseStream.Position = 18L;
		if (num)
		{
			dbReader.BaseStream.Position = 16L;
		}
		m_InGameName = FifaUtil.ReadNullTerminatedString(dbReader);
		if (SearchSignature(dbReader, "SaveType"))
		{
			FifaUtil.ReadNullTerminatedString(dbReader);
			m_CrcPosition = dbReader.BaseStream.Position;
		}
		else
		{
			m_CrcPosition = -1L;
		}
		while (dbReader.BaseStream.Position < dbReader.BaseStream.Length)
		{
			m_Database[m_NDatabases] = new DbFile();
			m_Database[m_NDatabases].ProgressBar = m_ProgressBar;
			m_Database[m_NDatabases].DescriptorDataSet = m_DescriptorDataSet;
			long position = dbReader.BaseStream.Position;
			if (m_Database[m_NDatabases].LoadDb(dbReader, skipData: true))
			{
				position = dbReader.BaseStream.Position;
				m_NDatabases++;
				if (m_NDatabases == m_Database.Length)
				{
					break;
				}
				continue;
			}
			dbReader.BaseStream.Position = position;
			break;
		}
		if (fileName.Contains("FIFA 16"))
		{
			AnalyzeFileFifa16(dbReader);
		}
		else
		{
			AnalyzeFileEAFC(dbReader);
		}
		dbReader.Close();
		fileStream.Close();
		return true;
	}

	private bool SearchSignature(BinaryReader br, string signature)
	{
		int num = 0;
		int length = signature.Length;
		char[] array = signature.ToCharArray();
		long position = br.BaseStream.Position;
		do
		{
			if (br.ReadByte() == array[num])
			{
				num++;
				if (num == length)
				{
					br.BaseStream.Position -= length;
					return true;
				}
			}
			else
			{
				num = 0;
			}
		}
		while (br.BaseStream.Position < br.BaseStream.Length);
		br.BaseStream.Position = position;
		return false;
	}

	private bool SetUserTeam(BinaryReader r, BinaryWriter w, int teamId)
	{
		long position = w.BaseStream.Position;
		w.BaseStream.Position = position + 790;
		r.ReadInt32();
		w.BaseStream.Position -= 4L;
		w.Write(teamId);
		r.BaseStream.Position = position + 37298;
		int num = -1;
		for (int i = 0; i < 100; i++)
		{
			r.ReadInt16();
			r.ReadByte();
			num = r.ReadInt16();
			r.ReadInt32();
			r.ReadByte();
		}
		if (num < 0)
		{
			return false;
		}
		r.BaseStream.Position = position + 362698;
		return false;
	}

	private void AnalyzeFileFifa16(BinaryReader r)
	{
		StreamWriter streamWriter = new StreamWriter(FifaEnvironment.ExportFolder + "Tournament.txt");
		streamWriter.WriteLine("------------------ User Teams ---------------------------");
		r.BaseStream.Position += 786L;
		for (int i = 0; i < 64; i++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			int num = r.ReadByte();
			int num2 = FifaUtil.SwapEndian(r.ReadInt16());
			int num3 = r.ReadByte();
			int num4 = r.ReadInt32();
			streamWriter.Write(num + "\t" + num2 + "\t" + num3 + "\t" + num4);
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ Advancement ---------------------------");
		for (int j = 0; j < 4000; j++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + ",");
			streamWriter.Write((int)r.ReadByte() + ",");
			streamWriter.Write((int)r.ReadInt16() + ",");
			int value = r.ReadByte();
			streamWriter.Write(value);
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ compids  ---------------------------");
		for (int k = 0; k < 100; k++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			int value2 = r.ReadByte();
			streamWriter.Write(value2);
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ compobj  ---------------------------");
		for (int l = 0; l < 2000; l++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			string text = FifaUtil.ReadNullPaddedString(r, 6);
			streamWriter.Write(text + "\t");
			text = FifaUtil.ReadNullPaddedString(r, 33);
			streamWriter.Write(text + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ initteams  ---------------------------");
		for (int m = 0; m < 500; m++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ Partite Giocate  ---------------------------");
		streamWriter.WriteLine("\tKey\tUsed\tTour\tLeg\tDate\t\tTime\tTHkey\tHGoal\tHRig\tTAkey\tAGoal\tARig");
		for (int n = 0; n < 4000; n++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ schedule  ---------------------------");
		streamWriter.WriteLine("\t\t\tId\tDay\tLeg\tMin\tMax\tTime");
		for (int num5 = 0; num5 < 7000; num5++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ tasks  ---------------------------");
		for (int num6 = 0; num6 < 800; num6++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ settings ---------------------------");
		streamWriter.WriteLine("\tKey\tUsed\tId\tRule\tPar1");
		for (int num7 = 0; num7 < 4000; num7++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write(r.ReadUInt32() + "\t");
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ compids date ---------------------------");
		streamWriter.WriteLine("\tKey\tUsed\tTourn\tUsed\tEndDate\t?\tStartYear");
		for (int num8 = 0; num8 < 100; num8++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ Tabellone ---------------------------");
		streamWriter.WriteLine("\tKey\tUsed\tId\tTeam\tn\tHV HP HS GF GS AV AP AS GF GS Pt");
		for (int num9 = 0; num9 < 5300; num9++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			int num10 = r.ReadByte();
			string text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			num10 = r.ReadByte();
			text2 = ((num10 < 10) ? "  " : " ");
			streamWriter.Write(num10 + text2);
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ Statistiche Giocatori ---------------------------");
		streamWriter.WriteLine("\tKey\tUsed\tTeam\tPlayer\tTourn\tMin     Sum     Pl Go ?? As MVPYC RC PA ?? RI ?? S5 S4 S3 S2 S1 G1 G2 G3 Date1    Date2    Date3  ");
		for (int num11 = 0; num11 < 7000; num11++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			int num12 = r.ReadByte();
			string text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadByte();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadInt32();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadInt32();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			num12 = r.ReadInt32();
			text3 = ((num12 < 10) ? "  " : " ");
			streamWriter.Write(num12 + text3);
			streamWriter.Write("\r\n");
		}
		streamWriter.WriteLine("------------------ Weather ---------------------------");
		streamWriter.WriteLine("\tKey\tUsed\tId\tMo Su Ha Cl Ra Sh Sn Fl Ov Fo");
		for (int num13 = 0; num13 < 330; num13++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadByte() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			int num14 = r.ReadByte();
			string text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			num14 = r.ReadByte();
			text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			num14 = r.ReadByte();
			text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			num14 = r.ReadByte();
			text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			num14 = r.ReadByte();
			text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			num14 = r.ReadByte();
			text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			num14 = r.ReadByte();
			text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			num14 = r.ReadByte();
			text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			num14 = r.ReadByte();
			text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			num14 = r.ReadByte();
			text4 = ((num14 < 10) ? "  " : " ");
			streamWriter.Write(num14 + text4);
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write((int)r.ReadInt16() + "\t");
			streamWriter.Write("\r\n");
		}
		string value3 = FifaUtil.ReadNullTerminatedString(r);
		streamWriter.WriteLine(value3);
		streamWriter.WriteLine("------------------ Cartellini Pendenti ---------------------------");
		streamWriter.WriteLine("\tKey\tTourn\tPlayer\tTeam\tUsed\tYel\t*\t*\tRed");
		for (int num15 = 0; num15 < 4001; num15++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write("(" + num15 + ")\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write("\r\n");
		}
		SearchSignature(r, "tmm001");
		streamWriter.WriteLine("tm0001\r\n");
		r.BaseStream.Position += 652L;
		int num16 = r.ReadInt32();
		streamWriter.WriteLine(num16);
		streamWriter.WriteLine("------------------ Teams Info ---------------------------");
		for (int num17 = 0; num17 < num16; num17++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write("\r\n");
		}
		SearchSignature(r, "fm001");
		streamWriter.WriteLine("fm001\r\n");
		streamWriter.WriteLine("------------------ Ultima Partita Fatica e Infortuni ---------------------------");
		streamWriter.WriteLine("\tKey\tPlayer\tTeam\tUsed\tStamina\t*\t*\t*\t*\tDate\tCanPlay");
		r.BaseStream.Position += 6L;
		for (int num18 = 0; num18 < 2000; num18++)
		{
			streamWriter.Write(r.BaseStream.Position.ToString("x6") + "\t");
			streamWriter.Write("(" + num18 + ")\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write(r.ReadInt32() + "\t");
			streamWriter.Write("\r\n");
		}
		streamWriter.Close();
	}

	private void AnalyzeFileEAFC(BinaryReader r)
	{
	}

	public bool SaveEa(string fileName, string templateName)
	{
		string fileName2 = Path.GetFileName(fileName);
		if (fileName2.StartsWith("Squad"))
		{
			return SaveEa(fileName);
		}
		if (fileName2.StartsWith("Career"))
		{
			return SaveEa(fileName);
		}
		return false;
	}

	public bool SaveEa(string fileName)
	{
		bool num = fileName.Contains("FIFA16") || fileName.Contains("RFS");
		FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
		DbWriter dbWriter = new DbWriter(fileStream, m_Platform);
		DbReader dbReader = new DbReader(fileStream, m_Platform);
		int num2 = (num ? 16 : 18);
		dbReader.BaseStream.Position = num2;
		int num3 = 0;
		while (dbReader.ReadByte() != 0)
		{
			num3++;
		}
		while (dbReader.ReadByte() == 0)
		{
			num3++;
		}
		dbWriter.BaseStream.Position = num2;
		if (m_InGameName.Length > num3)
		{
			m_InGameName = m_InGameName.Substring(0, num3);
		}
		FifaUtil.WriteNullPaddedString(dbWriter, m_InGameName, num3);
		if (m_CrcPosition != -1)
		{
			dbWriter.BaseStream.Position = m_CrcPosition;
			for (int i = 0; i < 8; i++)
			{
				dbWriter.Write((byte)0);
			}
		}
		for (int j = 0; j < m_NDatabases; j++)
		{
			dbWriter.BaseStream.Position = m_Database[j].SignaturePosition;
			m_Database[j].SaveDb(dbWriter);
		}
		for (int k = 0; k < m_NDatabases; k++)
		{
			m_Database[k].ComputeAllCrc(dbReader, dbWriter);
		}
		dbReader.Close();
		dbWriter.Close();
		fileStream.Close();
		return true;
	}

	public bool ExportDB(int dbIndex, string fileName)
	{
		FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
		DbWriter dbWriter = new DbWriter(fileStream, m_Platform);
		DbReader dbReader = new DbReader(fileStream, m_Platform);
		if (dbIndex >= m_NDatabases)
		{
			return false;
		}
		long signaturePosition = m_Database[dbIndex].SignaturePosition;
		m_Database[dbIndex].SignaturePosition = 0L;
		m_Database[dbIndex].SaveDb(dbWriter);
		m_Database[dbIndex].ComputeAllCrc(dbReader, dbWriter);
		m_Database[dbIndex].SignaturePosition = signaturePosition;
		dbReader.Close();
		dbWriter.Close();
		fileStream.Close();
		return true;
	}

	private uint ComputeCrc(BinaryReader r, int offset, int size)
	{
		uint num = uint.MaxValue;
		r.BaseStream.Position = offset;
		for (int i = 0; i < size; i++)
		{
			byte b = r.ReadByte();
			num = s_CrcTable[((num >> 24) ^ b) & 0xFF] ^ (num << 8);
		}
		return num ^ 0xFFFFFFFFu;
	}

	private uint ComputeChecksum24(BinaryReader r)
	{
		r.BaseStream.Position = 0L;
		uint num = r.ReadByte();
		num <<= 8;
		num |= r.ReadByte();
		num <<= 8;
		num |= r.ReadByte();
		num <<= 8;
		num |= r.ReadByte();
		num ^= 0xFFFFFFFFu;
		for (int num2 = 20; num2 > 0; num2--)
		{
			byte b = r.ReadByte();
			uint num3 = s_CrcTable[num >> 24];
			num <<= 8;
			num |= b;
			num ^= num3;
		}
		return num ^ 0xFFFFFFFFu;
	}

	public void ConvertFromDataSet(DataSet[] dataSet)
	{
		if (dataSet.Length == m_NDatabases)
		{
			for (int i = 0; i < m_NDatabases; i++)
			{
				m_Database[i].ConvertFromDataSet(dataSet[i]);
			}
		}
	}

	public DataSet[] ConvertToDataSet()
	{
		DataSet[] array = new DataSet[m_NDatabases];
		for (int i = 0; i < m_NDatabases; i++)
		{
			array[i] = m_Database[i].ConvertToDataSet();
		}
		return array;
	}
}
