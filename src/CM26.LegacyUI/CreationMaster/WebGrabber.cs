using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using FifaLibrary;
using HtmlAgilityPack;

namespace CreationMaster;

public class WebGrabber
{
	public enum EWebSiteDomain
	{
		None,
		Transfermrkt,
		Sofifa,
		Soccerway,
		Sortitoutsi
	}

	private HtmlAgilityPack.HtmlDocument m_CurrentHtmlDocument;

	private DataTable m_WebTable = new DataTable("PlayerWebData");

	private string m_Url;

	private List<Bitmap> m_WebPictures = new List<Bitmap>();

	private Bitmap m_Picture;

	private bool m_CanScrapTeam;

	private bool m_CanScrapPlayer;

	private bool m_CanScrapMultiplePlayers;

	private bool m_CanScrapManager;

	private bool m_CanScrapLeagueManagers;

	private bool m_CanScrapClubValues;

	private bool m_CanScrapLeague;

	private EWebSiteDomain m_WebSiteDomain;

	private Bitmap m_PlayerPicture;

	private int m_NewPersonId;

	private int m_NewTeamId;

	private bool m_IsDecemberContract;

	public DataTable WebTable => m_WebTable;

	public string Url => m_Url;

	public List<Bitmap> WebPictures => m_WebPictures;

	public bool CanExtractWebTeam => m_CanScrapTeam;

	public bool CanScrapPlayer => m_CanScrapPlayer;

	public bool CanScrapMultiplePlayers => m_CanScrapMultiplePlayers;

	public bool CanScrapManager => m_CanScrapManager;

	public bool CanScrapLeagueManagers => m_CanScrapLeagueManagers;

	public bool CanScrapClubValues => m_CanScrapClubValues;

	public bool CanScrapLeague => m_CanScrapLeague;

	public EWebSiteDomain WebSiteDomain => m_WebSiteDomain;

	public Bitmap Picture => m_PlayerPicture;

	public WebGrabber()
	{
		m_WebTable.Columns.Add("website");
		m_WebTable.Columns.Add("type");
		m_WebTable.Columns.Add("id");
		m_WebTable.Columns.Add("name");
		m_WebTable.Columns.Add("firstname");
		m_WebTable.Columns.Add("lastname");
		m_WebTable.Columns.Add("commonname");
		m_WebTable.Columns.Add("country");
		m_WebTable.Columns.Add("birthdate");
		m_WebTable.Columns.Add("age");
		m_WebTable.Columns.Add("role");
		m_WebTable.Columns.Add("height");
		m_WebTable.Columns.Add("weight");
		m_WebTable.Columns.Add("foot");
		m_WebTable.Columns.Add("team");
		m_WebTable.Columns.Add("number");
		m_WebTable.Columns.Add("since");
		m_WebTable.Columns.Add("contract");
		m_WebTable.Columns.Add("loantype");
		m_WebTable.Columns.Add("previousteam");
		m_WebTable.Columns.Add("loanedfrom");
		m_WebTable.Columns.Add("loanenddate");
		m_WebTable.Columns.Add("marketvalue");
		m_WebTable.Columns.Add("stadium");
		m_WebTable.Columns.Add("seats");
		m_WebTable.Columns.Add("totalmarketvalue");
		m_WebTable.Columns.Add("formation");
		m_WebTable.Columns.Add("weakfoot");
		m_WebTable.Columns.Add("skillmoves");
		m_WebTable.Columns.Add("crossing");
		m_WebTable.Columns.Add("finishing");
		m_WebTable.Columns.Add("heading");
		m_WebTable.Columns.Add("shortpassing");
		m_WebTable.Columns.Add("volleys");
		m_WebTable.Columns.Add("dribbling");
		m_WebTable.Columns.Add("curve");
		m_WebTable.Columns.Add("fkaccuracy");
		m_WebTable.Columns.Add("longpassing");
		m_WebTable.Columns.Add("ballcontrol");
		m_WebTable.Columns.Add("acceleration");
		m_WebTable.Columns.Add("sprintspeed");
		m_WebTable.Columns.Add("agility");
		m_WebTable.Columns.Add("reactions");
		m_WebTable.Columns.Add("balance");
		m_WebTable.Columns.Add("shotpower");
		m_WebTable.Columns.Add("jumping");
		m_WebTable.Columns.Add("stamina");
		m_WebTable.Columns.Add("strength");
		m_WebTable.Columns.Add("longshots");
		m_WebTable.Columns.Add("aggression");
		m_WebTable.Columns.Add("interceptions");
		m_WebTable.Columns.Add("positioning");
		m_WebTable.Columns.Add("vision");
		m_WebTable.Columns.Add("penalties");
		m_WebTable.Columns.Add("standingtackle");
		m_WebTable.Columns.Add("slidingtackle");
		m_WebTable.Columns.Add("marking");
		m_WebTable.Columns.Add("gkdiving");
		m_WebTable.Columns.Add("gkhandling");
		m_WebTable.Columns.Add("gkkicking");
		m_WebTable.Columns.Add("gkpositioning");
		m_WebTable.Columns.Add("gkreflexes");
		m_WebTable.Columns.Add("potential");
		m_WebTable.Columns.Add("overall");
	}

	public bool Sync(string webDocumentTitle, string source)
	{
		webDocumentTitle = webDocumentTitle.ToLower();
		source = source.ToLower();
		m_Url = source;
		m_CanScrapTeam = false;
		m_CanScrapPlayer = false;
		m_CanScrapMultiplePlayers = false;
		m_CanScrapManager = false;
		m_CanScrapClubValues = false;
		m_CanScrapLeague = false;
		if (webDocumentTitle.EndsWith("transfermarkt"))
		{
			m_WebSiteDomain = EWebSiteDomain.Transfermrkt;
			m_CanScrapTeam = webDocumentTitle.Contains("detailed squad") && webDocumentTitle.Contains("detailed view");
			m_CanScrapPlayer = webDocumentTitle.Contains("player profile");
			m_CanScrapManager = webDocumentTitle.Contains("manager profile");
			m_CanScrapClubValues = webDocumentTitle.Contains("club market value |");
			m_CanScrapLeagueManagers = webDocumentTitle.Contains("current and past coaches compared") || webDocumentTitle.Contains("available coaches");
			m_CanScrapLeague = source.Contains("/wettbewerb/");
		}
		if (webDocumentTitle.Contains("sofifa"))
		{
			m_WebSiteDomain = EWebSiteDomain.Sofifa;
			if (source.Contains("team/"))
			{
				m_CanScrapTeam = true;
			}
			if (source.Contains("player/"))
			{
				m_CanScrapPlayer = true;
			}
			if (source.Contains("players"))
			{
				m_CanScrapMultiplePlayers = true;
			}
		}
		if (webDocumentTitle.Contains("soccerway"))
		{
			m_WebSiteDomain = EWebSiteDomain.Soccerway;
			if (source.Contains("coaches"))
			{
				m_CanScrapManager = true;
			}
			if (source.Contains("profile"))
			{
				m_CanScrapPlayer = true;
			}
		}
		if (webDocumentTitle.Contains("football manager"))
		{
			m_WebSiteDomain = EWebSiteDomain.Sortitoutsi;
			if (source.Contains("person"))
			{
				m_CanScrapPlayer = true;
			}
		}
		return m_CanScrapTeam | m_CanScrapPlayer | m_CanScrapManager | m_CanScrapClubValues | m_CanScrapLeagueManagers | m_CanScrapMultiplePlayers | m_CanScrapLeague;
	}

	public string ExtractTeamNameFromWebTitle(string webDocumentTitle)
	{
		int num = webDocumentTitle.LastIndexOf('-');
		string result = string.Empty;
		if (num >= 3)
		{
			result = webDocumentTitle.Substring(0, num - 1);
		}
		return result;
	}

	public string ExtractPlayerNameFromWebTitle(string webDocumentTitle)
	{
		int num = webDocumentTitle.LastIndexOf('-');
		string result = string.Empty;
		if (num >= 3)
		{
			result = webDocumentTitle.Substring(0, num - 1);
		}
		return result;
	}

	public bool ExtractRosterInfoFromWeb(System.Windows.Forms.HtmlDocument webPage)
	{
		EWebSiteDomain webSiteDomain = m_WebSiteDomain;
		bool result = false;
		switch (webSiteDomain)
		{
		case EWebSiteDomain.Soccerway:
			result = ExtractRosterInfoFromSoccerway(webPage);
			break;
		case EWebSiteDomain.Transfermrkt:
			result = ExtractRosterInfoFromTransfermrkt(webPage);
			break;
		}
		return result;
	}

	private bool ExtractRosterInfoFromSoccerway(System.Windows.Forms.HtmlDocument webPage)
	{
		return false;
	}

	private void SplitPlayerName(string nameSurname, DataRow webDataRow)
	{
		int num = nameSurname.IndexOf(' ');
		string text = null;
		string text2 = null;
		if (num < 0 || webDataRow["country"].ToString().Contains("Korea"))
		{
			webDataRow["commonname"] = nameSurname;
			return;
		}
		int num2 = nameSurname.LastIndexOf(' ');
		if (num2 == num)
		{
			text = nameSurname.Substring(0, num);
			webDataRow["firstname"] = text;
			text2 = nameSurname.Substring(num + 1);
			webDataRow["lastname"] = text2;
		}
		else if (nameSurname.Substring(num, num2 - num + 1).ToLower() == " da " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " das " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " la " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " le " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " de " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " del " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " di " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " ten " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " van der " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " van de " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " van " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " st. " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " el " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " al " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " de la " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " mac " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " mc " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " von " || nameSurname.Substring(num, num2 - num + 1).ToLower() == " ben ")
		{
			text = nameSurname.Substring(0, num);
			webDataRow["firstname"] = text;
			text2 = nameSurname.Substring(num + 1);
			webDataRow["lastname"] = text2;
		}
		else
		{
			text = nameSurname.Substring(0, num2);
			webDataRow["firstname"] = text;
			text2 = nameSurname.Substring(num2 + 1);
			webDataRow["lastname"] = text2;
		}
	}

	public bool ExtractRosterInfoFromTransfermrkt(System.Windows.Forms.HtmlDocument webPage)
	{
		HtmlElementCollection elementsByTagName = webPage.GetElementsByTagName("span");
		m_WebTable.Rows.Clear();
		DataRow dataRow = m_WebTable.NewRow();
		string text = ExtractTeamNameFromWebTitle(webPage.Title);
		string empty = string.Empty;
		if (m_PlayerPicture != null)
		{
			m_PlayerPicture.Dispose();
			m_PlayerPicture = null;
		}
		dataRow["website"] = "transfermrkt";
		dataRow["name"] = text;
		dataRow["type"] = "Team";
		Team team = FifaEnvironment.Teams.MatchByname(text);
		if (team != null)
		{
			dataRow["id"] = team.Id.ToString();
		}
		else
		{
			int newId = FifaEnvironment.Teams.GetNewId();
			if (newId != -1)
			{
				dataRow["id"] = newId.ToString();
			}
		}
		int num = FifaEnvironment.Players.GetNewId();
		for (int i = 0; i < elementsByTagName.Count; i++)
		{
			HtmlElement htmlElement = elementsByTagName[i];
			if (htmlElement.OuterText == null)
			{
				continue;
			}
			if (htmlElement.OuterText.Contains("Stadium"))
			{
				if (elementsByTagName[i + 1].Children.Count > 0)
				{
					if (elementsByTagName[i + 1].Children.Count > 0)
					{
						dataRow["stadium"] = elementsByTagName[i + 1].Children[0].OuterText;
					}
					if (elementsByTagName[i + 1].Children.Count > 1)
					{
						dataRow["seats"] = elementsByTagName[i + 1].Children[1].OuterText;
					}
				}
			}
			else if ((htmlElement.OuterText.Contains("Mil") || htmlElement.OuterText.Contains("Bil") || htmlElement.OuterText.Contains("mil") || htmlElement.OuterText.Contains("bil")) && htmlElement.Parent.OuterText.ToLower().Contains("total market value"))
			{
				empty = htmlElement.Parent.OuterText;
				int length = empty.IndexOf("Total");
				empty = empty.Substring(0, length);
				dataRow["totalmarketvalue"] = empty;
			}
		}
		m_WebTable.Rows.Add(dataRow);
		Bitmap bitmap = SearchImageContaining(webPage, "wappen/head");
		if (bitmap != null)
		{
			int width = bitmap.Width * 256 / bitmap.Height;
			m_PlayerPicture = GraphicUtil.ResizeBitmap(bitmap, width, 256, InterpolationMode.HighQualityBicubic);
			m_PlayerPicture = GraphicUtil.CanvasSizeBitmap(m_PlayerPicture, 256, 256);
		}
		elementsByTagName = webPage.GetElementsByTagName("table");
		foreach (HtmlElement item in elementsByTagName)
		{
			if (!item.OuterText.ToLower().StartsWith("\r\n\r\n#\r\n\r\n\r\nplayer\r"))
			{
				continue;
			}
			if (item.Children.Count < 2)
			{
				return false;
			}
			HtmlElement htmlElement3 = item.Children[1];
			int count = htmlElement3.Children.Count;
			_ = new string[count, 12];
			for (int j = 0; j < count; j++)
			{
				HtmlElement htmlElement4 = htmlElement3.Children[j];
				string outerText = htmlElement4.Children[0].OuterText;
				string empty2 = string.Empty;
				HtmlElementCollection elementsByTagName2 = htmlElement4.Children[1].GetElementsByTagName("A");
				int count2 = elementsByTagName2.Count;
				empty2 = elementsByTagName2[count2 - 2].OuterText;
				string value = string.Empty;
				string value2 = string.Empty;
				int num2;
				string innerHtml;
				if (elementsByTagName2[0].InnerHtml.Contains("loan from "))
				{
					innerHtml = elementsByTagName2[0].InnerHtml;
					num2 = innerHtml.IndexOf("loan from ");
					innerHtml = innerHtml.Substring(num2 + 10);
					num2 = innerHtml.IndexOf(" until ");
					if (num2 >= 0)
					{
						innerHtml = innerHtml.Substring(0, num2);
						value = innerHtml;
						innerHtml = elementsByTagName2[0].InnerHtml;
						num2 = innerHtml.IndexOf(" until ");
						innerHtml = innerHtml.Substring(num2 + 7);
						num2 = innerHtml.IndexOf('"');
						innerHtml = innerHtml.Substring(0, num2);
						value2 = innerHtml;
					}
				}
				else
				{
					elementsByTagName2[0].InnerHtml.Contains("Joined in: ");
				}
				elementsByTagName2 = htmlElement4.Children[1].GetElementsByTagName("TD");
				int count3 = elementsByTagName2.Count;
				string outerText2 = elementsByTagName2[count3 - 1].OuterText;
				string empty3 = string.Empty;
				innerHtml = htmlElement4.Children[2].OuterText;
				num2 = innerHtml.IndexOf('(');
				empty3 = innerHtml.Substring(0, num2 - 1);
				string value3 = innerHtml.Substring(num2 + 1, 2);
				innerHtml = htmlElement4.Children[3].InnerHtml;
				string value4 = string.Empty;
				int num3 = innerHtml.IndexOf('"');
				if (num3 >= 0)
				{
					innerHtml = innerHtml.Substring(num3 + 1);
					int num4 = innerHtml.IndexOf('"');
					if (num4 >= 1)
					{
						value4 = innerHtml.Substring(0, num4);
					}
				}
				string outerText3 = htmlElement4.Children[4].OuterText;
				if (outerText3 != null)
				{
					outerText3 = outerText3.Replace(".", string.Empty);
					outerText3 = outerText3.Replace(",", string.Empty);
					outerText3 = outerText3.Replace(" ", string.Empty);
					outerText3 = outerText3.Replace("m", string.Empty);
				}
				else
				{
					outerText3 = "175";
				}
				string outerText4 = htmlElement4.Children[5].OuterText;
				string outerText5 = htmlElement4.Children[6].OuterText;
				string value5 = string.Empty;
				innerHtml = htmlElement4.Children[7].InnerHtml;
				if (innerHtml != null)
				{
					num3 = innerHtml.IndexOf("alt=");
					if (num3 >= 0)
					{
						innerHtml = innerHtml.Substring(num3 + 5);
						int num5 = innerHtml.IndexOf('"');
						if (num5 >= 1)
						{
							value5 = innerHtml.Substring(0, num5);
						}
					}
				}
				string outerText6 = htmlElement4.Children[8].OuterText;
				string outerText7 = htmlElement4.Children[9].OuterText;
				dataRow = m_WebTable.NewRow();
				dataRow["name"] = empty2;
				dataRow["type"] = "Player";
				dataRow["birthdate"] = empty3;
				dataRow["age"] = value3;
				DateTime birthdate = FifaUtil.ConvertToDate(empty3);
				dataRow["country"] = value4;
				dataRow["role"] = outerText2;
				dataRow["height"] = outerText3;
				dataRow["foot"] = outerText4;
				dataRow["number"] = outerText;
				dataRow["team"] = text;
				dataRow["since"] = outerText5;
				dataRow["contract"] = outerText6;
				dataRow["previousteam"] = value5;
				dataRow["loanedfrom"] = value;
				dataRow["loanenddate"] = value2;
				dataRow["marketvalue"] = outerText7;
				SplitPlayerName(empty2, dataRow);
				string commonName = dataRow["commonname"].ToString();
				string firstName = dataRow["firstname"].ToString();
				string lastName = dataRow["lastname"].ToString();
				Player player = FifaEnvironment.Players.MatchPlayerByNameBirthday(ref firstName, ref lastName, ref commonName, birthdate);
				dataRow["commonname"] = commonName;
				dataRow["firstname"] = firstName;
				dataRow["lastname"] = lastName;
				if (player != null)
				{
					dataRow["id"] = player.Id.ToString();
				}
				else
				{
					dataRow["id"] = num.ToString();
					num = FifaEnvironment.Players.GetNextId(num + 1);
				}
				m_WebTable.Rows.Add(dataRow);
			}
			return true;
		}
		return false;
	}

	public bool ExtractPlayerInfoFromTransfermrkt(System.Windows.Forms.HtmlDocument webPage)
	{
		HtmlElementCollection elementsByTagName = webPage.GetElementsByTagName("table");
		string text = null;
		string text2 = null;
		string text3 = null;
		DateTime birthdate = default(DateTime);
		bool flag = false;
		string text4 = ExtractPlayerNameFromWebTitle(webPage.Title);
		m_WebTable.Rows.Clear();
		DataRow dataRow = m_WebTable.NewRow();
		dataRow["name"] = text4;
		dataRow["type"] = "Player";
		dataRow["website"] = "transfermrkt";
		for (int i = 0; i < elementsByTagName.Count; i++)
		{
			HtmlElement htmlElement = elementsByTagName[i];
			if (htmlElement.Children[0].OuterText.Contains("Season"))
			{
				HtmlElementCollection elementsByTagName2 = htmlElement.Children[1].GetElementsByTagName("tr");
				for (int j = 1; j < elementsByTagName2.Count; j += 2)
				{
					HtmlElementCollection elementsByTagName3 = elementsByTagName2[j].GetElementsByTagName("td");
					if (elementsByTagName3.Count >= 11)
					{
						string outerText = elementsByTagName3[10].OuterText;
						if (outerText != "-")
						{
							dataRow["marketvalue"] = outerText;
							break;
						}
					}
				}
			}
			if (!htmlElement.Children[0].OuterText.Contains("Date of Birth:") && !htmlElement.Children[0].OuterText.Contains("Date of birth:"))
			{
				continue;
			}
			flag = true;
			HtmlElement htmlElement2 = htmlElement.Children[0];
			for (int k = 0; k < htmlElement2.Children.Count; k++)
			{
				string outerText2 = htmlElement2.Children[k].Children[0].OuterText;
				string outerText3 = htmlElement2.Children[k].Children[1].OuterText;
				outerText3 = outerText3.Replace("\t", "");
				outerText3 = outerText3.Trim();
				switch (outerText2)
				{
				case "Age:":
					dataRow["age"] = outerText3;
					break;
				case "Weight ":
					dataRow["weight"] = outerText3;
					break;
				case "Joined:":
					dataRow["since"] = outerText3;
					break;
				case "Contract until:":
				case "Contract expires:":
				case "Contract there until:":
					dataRow["contract"] = outerText3;
					break;
				case "Nationality:":
				case "Citizenship:":
				{
					int num = outerText3.IndexOf('\r');
					if (num >= 0)
					{
						outerText3 = outerText3.Substring(0, num);
					}
					dataRow["country"] = outerText3;
					break;
				}
				case "Last name ":
					dataRow["lastname"] = outerText3;
					text2 = outerText3;
					break;
				case "Height:":
					outerText3 = outerText3.Replace(".", string.Empty);
					outerText3 = outerText3.Replace(",", string.Empty);
					outerText3 = outerText3.Replace(" ", string.Empty);
					outerText3 = outerText3.Replace("m", string.Empty);
					dataRow["height"] = outerText3;
					break;
				case "Foot:":
					dataRow["foot"] = outerText3;
					break;
				case "In the team since:":
					dataRow["since"] = outerText3;
					break;
				case "First name ":
					dataRow["firstname"] = outerText3;
					text = outerText3;
					break;
				case "Date of birth:":
				case "Date of Birth:":
					dataRow["birthdate"] = outerText3;
					birthdate = FifaUtil.ConvertToDate(outerText3);
					break;
				case "Position:":
					dataRow["role"] = outerText3;
					break;
				case "on loan from:":
					dataRow["loanedfrom"] = outerText3;
					dataRow["loanenddate"] = dataRow["contract"];
					break;
				}
			}
		}
		if (flag)
		{
			SplitPlayerName(text4, dataRow);
			text3 = dataRow["commonname"].ToString();
			text = dataRow["firstname"].ToString();
			text2 = dataRow["lastname"].ToString();
			Player player = FifaEnvironment.Players.MatchPlayerByNameBirthday(ref text, ref text2, ref text3, birthdate);
			dataRow["commonname"] = text3;
			dataRow["firstname"] = text;
			dataRow["lastname"] = text2;
			if (player != null)
			{
				dataRow["id"] = player.Id.ToString();
			}
			else
			{
				dataRow["id"] = FifaEnvironment.Players.GetNewId().ToString();
			}
			m_WebTable.Rows.Add(dataRow);
			Bitmap bitmap = null;
			if (bitmap != null)
			{
				m_PlayerPicture = bitmap;
			}
			else
			{
				m_PlayerPicture = null;
			}
			return true;
		}
		return false;
	}

	private Bitmap SearchImageContaining(System.Windows.Forms.HtmlDocument webPage, string caption1)
	{
		return null;
	}

	public bool ExtractPlayerInfoFromSoccerway(System.Windows.Forms.HtmlDocument webPage)
	{
		HtmlElementCollection elementsByTagName = webPage.GetElementsByTagName("dl");
		if (elementsByTagName.Count == 1)
		{
			HtmlElement htmlElement = elementsByTagName[0];
			m_WebTable.Rows.Clear();
			DataRow dataRow = m_WebTable.NewRow();
			string firstName = null;
			string lastName = null;
			string text = null;
			DateTime birthdate = default(DateTime);
			for (int i = 0; i < htmlElement.Children.Count; i += 2)
			{
				string outerText = htmlElement.Children[i].OuterText;
				string outerText2 = htmlElement.Children[i + 1].OuterText;
				switch (outerText)
				{
				case "Age ":
					outerText2.Trim();
					dataRow["age"] = outerText2;
					break;
				case "Weight ":
					outerText2.Replace("kg", "").Trim();
					dataRow["weight"] = outerText2;
					break;
				case "Height ":
				{
					string text4 = outerText2.Replace("cm", "");
					text4 = text4.Trim();
					dataRow["height"] = text4;
					break;
				}
				case "Country of birth ":
				case "Nationality ":
					dataRow["country"] = outerText2.Trim();
					break;
				case "Last name ":
					lastName = (string)(dataRow["lastname"] = outerText2.Trim());
					break;
				case "First name ":
					firstName = (string)(dataRow["firstname"] = outerText2.Trim());
					break;
				case "Date of birth ":
					dataRow["birthdate"] = outerText2.Trim();
					birthdate = FifaUtil.ConvertToDate(outerText2);
					break;
				case "Position ":
					dataRow["role"] = outerText2.Trim();
					break;
				case "Foot ":
					dataRow["foot"] = outerText2.Trim();
					break;
				}
			}
			dataRow["name"] = firstName + " " + lastName;
			dataRow["type"] = "Player";
			dataRow["website"] = "soccerway";
			text = dataRow["commonname"].ToString();
			Player player = FifaEnvironment.Players.MatchPlayerByNameBirthday(ref firstName, ref lastName, ref text, birthdate);
			dataRow["commonname"] = text;
			dataRow["firstname"] = firstName;
			dataRow["lastname"] = lastName;
			if (player != null)
			{
				dataRow["id"] = player.Id.ToString();
			}
			else
			{
				dataRow["id"] = FifaEnvironment.Players.GetNewId().ToString();
			}
			m_WebTable.Rows.Add(dataRow);
			m_PlayerPicture = SearchImageContaining(webPage, "150x150");
			m_PlayerPicture = GraphicUtil.MakeAutoTransparent(m_PlayerPicture);
			m_PlayerPicture = GraphicUtil.ResizeBitmap(m_PlayerPicture, 128, 128, InterpolationMode.HighQualityBicubic);
			return true;
		}
		return false;
	}

	public bool ExtractPlayerInfoFromWeb(System.Windows.Forms.HtmlDocument webPage)
	{
		EWebSiteDomain webSiteDomain = m_WebSiteDomain;
		if (webSiteDomain != EWebSiteDomain.Transfermrkt)
		{
			return webSiteDomain == EWebSiteDomain.Soccerway && ExtractPlayerInfoFromSoccerway(webPage);
		}
		return ExtractPlayerInfoFromTransfermrkt(webPage);
	}

	public bool ExtractInfoFromWeb(HtmlAgilityPack.HtmlDocument doc)
	{
		m_CurrentHtmlDocument = doc;
		m_WebPictures.Clear();
		m_WebTable.Rows.Clear();
		if (m_Picture != null)
		{
			m_Picture.Dispose();
			m_Picture = null;
		}
		m_NewPersonId = -1;
		m_NewTeamId = -1;
		bool result = false;
		switch (m_WebSiteDomain)
		{
		case EWebSiteDomain.Transfermrkt:
			result = ExtractInfoFromTransfermrkt();
			break;
		case EWebSiteDomain.Sofifa:
			result = ExtractInfoFromSofifa();
			break;
		case EWebSiteDomain.Sortitoutsi:
			result = ExtractInfoFromSortitusi();
			break;
		case EWebSiteDomain.Soccerway:
			result = ExtractInfoFromSoccerway();
			break;
		}
		return result;
	}

	public Image DownloadImage(Uri url)
	{
		try
		{
			return Image.FromStream(new MemoryStream(new WebClient().DownloadData(url)));
		}
		catch
		{
			return null;
		}
	}

	private bool ExtractInfoFromTransfermrkt()
	{
		if (m_CanScrapTeam)
		{
			return ExtractTeamFromTransfermrkt();
		}
		if (m_CanScrapPlayer)
		{
			return ExtractPlayerFromTransfermrkt();
		}
		if (m_CanScrapManager)
		{
			return ExtractManagerFromTransfermrkt();
		}
		if (m_CanScrapClubValues)
		{
			return ExtractClubValueFromTransfermrkt();
		}
		if (m_CanScrapLeagueManagers)
		{
			return ExtractManagersFromTransfermrkt();
		}
		if (m_CanScrapLeague)
		{
			return ExtractLeagueFromTransfermrkt();
		}
		return false;
	}

	private bool ExtractLeagueFromTransfermrkt()
	{
		m_WebTable.NewRow();
		HtmlNodeCollection htmlNodeCollection = m_CurrentHtmlDocument.DocumentNode.SelectNodes("//td[@class='hauptlink no-border-links']/a");
		HtmlWeb htmlWeb = new HtmlWeb();
		HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
		if (htmlNodeCollection == null)
		{
			return false;
		}
		foreach (HtmlNode item in (IEnumerable<HtmlNode>)htmlNodeCollection)
		{
			string attributeValue = item.GetAttributeValue("href", string.Empty);
			if (!attributeValue.Contains("saison_id"))
			{
				continue;
			}
			attributeValue += "/plus/1";
			attributeValue = attributeValue.Replace("startseite", "kader");
			string text = "https://www.transfermarkt.com" + attributeValue;
			if (text != string.Empty)
			{
				try
				{
					htmlDocument = htmlWeb.Load(text);
					m_CurrentHtmlDocument = htmlDocument;
					ExtractTeamFromTransfermrkt();
				}
				catch
				{
				}
			}
		}
		return true;
	}

	private static string GetDigit(string input)
	{
		return new string(input.Where((char c) => char.IsDigit(c)).ToArray());
	}

	private bool ExtractTeamFromTransfermrkt()
	{
		DataRow dataRow = m_WebTable.NewRow();
		dataRow["website"] = "transfermrkt";
		dataRow["type"] = "Team";
		HtmlNode documentNode = m_CurrentHtmlDocument.DocumentNode;
		HtmlNode htmlNode = documentNode.SelectSingleNode("//main/header/div/h1");
		string srcString = ((htmlNode != null) ? htmlNode.InnerText : string.Empty);
		srcString = (string)(dataRow["name"] = (dataRow["team"] = CleanString(srcString)));
		htmlNode = documentNode.SelectSingleNode("//span[@class='data-header__content']/a/img");
		string empty = string.Empty;
		if (htmlNode != null)
		{
			empty = htmlNode.GetAttributeValue("title", string.Empty);
			dataRow["country"] = empty;
		}
		Team team = FifaEnvironment.Teams.MatchByname(srcString);
		if (team != null)
		{
			dataRow["id"] = team.Id.ToString();
		}
		else
		{
			if (m_NewTeamId == -1)
			{
				m_NewTeamId = FifaEnvironment.Teams.GetNewId();
				if (m_NewTeamId < 0)
				{
					return false;
				}
			}
			else
			{
				m_NewTeamId = FifaEnvironment.Teams.GetNextId(m_NewTeamId + 1);
			}
			int newTeamId = m_NewTeamId;
			if (newTeamId != -1)
			{
				dataRow["id"] = newTeamId.ToString();
			}
		}
		htmlNode = documentNode.SelectSingleNode("//div[@class='data-header__details']/ul[2]/li[2]/span/a");
		dataRow["stadium"] = ((htmlNode != null) ? htmlNode.InnerText : string.Empty);
		if (htmlNode != null)
		{
			htmlNode = htmlNode.SelectSingleNode("following::span");
			dataRow["seats"] = ((htmlNode != null) ? htmlNode.InnerText : string.Empty);
		}
		htmlNode = documentNode.SelectSingleNode("//a[@class = 'data-header__market-value-wrapper']");
		if (htmlNode != null)
		{
			string value = htmlNode.ChildNodes[1].InnerText + htmlNode.ChildNodes[2].InnerText;
			dataRow["totalmarketvalue"] = value;
		}
		htmlNode = documentNode.SelectSingleNode("//div[@class = 'data-header__profile-container']/img");
		if (htmlNode != null)
		{
			Uri url = new Uri(htmlNode.GetAttributeValue("src", string.Empty));
			Bitmap bitmap = (Bitmap)DownloadImage(url);
			if (bitmap != null)
			{
				int width = bitmap.Width * 256 / bitmap.Height;
				m_Picture = GraphicUtil.ResizeBitmap(bitmap, width, 256, InterpolationMode.HighQualityBicubic);
				m_Picture = GraphicUtil.CanvasSizeBitmap(m_Picture, 256, 256);
			}
		}
		m_WebTable.Rows.Add(dataRow);
		m_WebPictures.Add(m_Picture);
		htmlNode = documentNode.SelectSingleNode("//div[@id = 'yw1']/table/tbody");
		if (htmlNode == null)
		{
			return false;
		}
		HtmlNodeCollection htmlNodeCollection = htmlNode.SelectNodes("tr");
		if (m_NewPersonId == -1)
		{
			m_NewPersonId = FifaEnvironment.Players.GetNewId();
			if (m_NewPersonId < 0)
			{
				return false;
			}
		}
		char[] trimChars = new char[2] { '\n', ' ' };
		foreach (HtmlNode item in (IEnumerable<HtmlNode>)htmlNodeCollection)
		{
			HtmlNode htmlNode2 = item.SelectSingleNode("td[1]/div");
			string value2 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			htmlNode2 = item.SelectSingleNode("td[2]/table/tr[1]/td/img");
			string attributeValue = htmlNode2.GetAttributeValue("title", string.Empty);
			string attributeValue2 = htmlNode2.GetAttributeValue("data-src", string.Empty);
			m_Picture = null;
			if (!attributeValue2.Contains("default"))
			{
				Uri url2 = new Uri(attributeValue2);
				Bitmap bitmap2 = (Bitmap)DownloadImage(url2);
				if (bitmap2 != null)
				{
					int width2 = bitmap2.Width * 128 / bitmap2.Height;
					m_Picture = GraphicUtil.ResizeBitmap(bitmap2, width2, 128, InterpolationMode.HighQualityBicubic);
					m_Picture = GraphicUtil.CanvasSizeBitmap(m_Picture, 128, 128);
				}
			}
			m_WebPictures.Add(m_Picture);
			htmlNode2 = item.SelectSingleNode("td[2]/table/tr[2]/td");
			string text2 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			text2 = text2.TrimStart(trimChars).TrimEnd(trimChars);
			htmlNode2 = item.SelectSingleNode("td[2]/span/a");
			string text3 = ((htmlNode2 != null) ? htmlNode2.GetAttributeValue("title", string.Empty) : string.Empty);
			htmlNode2 = item.SelectSingleNode("td[2]/span/a/img");
			string text4 = ((htmlNode2 != null) ? htmlNode2.GetAttributeValue("alt", string.Empty) : string.Empty);
			string text5 = string.Empty;
			int num;
			if (text3.Contains("loan from"))
			{
				num = text3.IndexOf(" until ");
				text5 = ((num >= 0) ? text3.Substring(num + 7) : string.Empty);
			}
			htmlNode2 = item.SelectSingleNode("td[3]");
			string text6 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			num = text6.IndexOf('(');
			string text7 = ((num >= 0) ? text6.Substring(0, num) : string.Empty);
			string value3 = ((num >= 0) ? text6.Substring(num + 1, 2) : string.Empty);
			int num2 = 4;
			htmlNode2 = item.SelectSingleNode("td[" + num2 + "]/img");
			string value4 = ((htmlNode2 != null) ? htmlNode2.GetAttributeValue("title", string.Empty) : string.Empty);
			num2++;
			htmlNode2 = item.SelectSingleNode("td[" + num2 + "]");
			string text8 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			if (text8 == "")
			{
				num2++;
				htmlNode2 = item.SelectSingleNode("td[" + num2 + "]");
				text8 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			}
			text8 = GetDigit(text8);
			num2++;
			htmlNode2 = item.SelectSingleNode("td[" + num2 + "]");
			string value5 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			num2++;
			htmlNode2 = item.SelectSingleNode("td[" + num2 + "]");
			string value6 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			num2++;
			htmlNode2 = item.SelectSingleNode("td[" + num2 + "]/a/img");
			string value7 = ((htmlNode2 != null) ? htmlNode2.GetAttributeValue("alt", string.Empty) : string.Empty);
			num2++;
			htmlNode2 = item.SelectSingleNode("td[" + num2 + "]");
			string value8 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			num2++;
			htmlNode2 = item.SelectSingleNode("td[" + num2 + "]/a");
			string value9 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			num2++;
			dataRow = m_WebTable.NewRow();
			dataRow["website"] = "transfermrkt";
			dataRow["name"] = attributeValue;
			dataRow["type"] = "Player";
			dataRow["birthdate"] = text7;
			dataRow["age"] = value3;
			DateTime birthdate = FifaUtil.ConvertToDate(text7);
			dataRow["country"] = value4;
			dataRow["role"] = text2;
			dataRow["height"] = text8;
			dataRow["foot"] = value5;
			dataRow["number"] = value2;
			dataRow["team"] = srcString;
			dataRow["since"] = value6;
			dataRow["contract"] = value8;
			dataRow["previousteam"] = value7;
			dataRow["loanenddate"] = text5;
			dataRow["loanedfrom"] = ((text5 != string.Empty) ? text4 : string.Empty);
			dataRow["marketvalue"] = value9;
			SplitPlayerName(attributeValue, dataRow);
			string commonName = dataRow["commonname"].ToString();
			string firstName = dataRow["firstname"].ToString();
			string lastName = dataRow["lastname"].ToString();
			Player player = FifaEnvironment.Players.MatchPlayerByNameBirthday(ref firstName, ref lastName, ref commonName, birthdate);
			dataRow["commonname"] = commonName;
			dataRow["firstname"] = firstName;
			dataRow["lastname"] = lastName;
			if (player != null)
			{
				dataRow["id"] = player.Id.ToString();
			}
			else
			{
				dataRow["id"] = m_NewPersonId.ToString();
				m_NewPersonId = FifaEnvironment.Players.GetNextId(m_NewPersonId + 1);
			}
			m_WebTable.Rows.Add(dataRow);
		}
		return true;
	}

	private bool ExtractPlayerFromTransfermrkt()
	{
		DataRow dataRow = m_WebTable.NewRow();
		string value = null;
		string value2 = null;
		string text = string.Empty;
		string value3 = null;
		string value4 = null;
		string value5 = null;
		string value6 = null;
		string text2 = null;
		dataRow["website"] = "transfermrkt";
		dataRow["type"] = "Player";
		HtmlNode documentNode = m_CurrentHtmlDocument.DocumentNode;
		HtmlNode htmlNode = documentNode.SelectSingleNode("//h1[@class='data-header__headline-wrapper']");
		if (htmlNode == null)
		{
			return false;
		}
		char[] trimChars = new char[3] { '\n', ' ', '#' };
		char[] trimChars2 = new char[13]
		{
			'\n', ' ', '#', '1', '2', '3', '4', '5', '6', '7',
			'8', '9', '0'
		};
		string innerText = htmlNode.InnerText;
		innerText = innerText.TrimStart(trimChars2).TrimEnd(trimChars2);
		HtmlNode htmlNode2 = htmlNode.SelectSingleNode("span");
		string text3 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
		text3 = text3.TrimStart(trimChars).TrimEnd(trimChars);
		htmlNode2 = htmlNode.SelectSingleNode("strong");
		string text4 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
		text4 = text4.TrimStart(trimChars).TrimEnd(trimChars);
		htmlNode = documentNode.SelectSingleNode("//img[@class = 'data-header__profile-image']");
		if (htmlNode != null)
		{
			Uri url = new Uri(htmlNode.GetAttributeValue("src", string.Empty));
			Bitmap bitmap = (Bitmap)DownloadImage(url);
			if (bitmap != null)
			{
				m_Picture = bitmap;
			}
			m_WebPictures.Add(m_Picture);
		}
		htmlNode2 = documentNode.SelectSingleNode("//span[@class='data-header__club']/a");
		string value7 = ((htmlNode2 != null) ? htmlNode2.GetAttributeValue("title", string.Empty) : string.Empty);
		htmlNode2 = documentNode.SelectSingleNode("//a[@class='data-header__market-value-wrapper']");
		string text5 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
		int num = text5.IndexOf(' ');
		if (num > 0)
		{
			text5 = text5.Substring(0, num);
		}
		htmlNode2 = documentNode.SelectSingleNode("//span[@itemprop='birthDate']");
		text2 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
		text2 = text2.TrimStart(trimChars).TrimEnd(trimChars);
		int num2 = text2.IndexOf('(');
		if (num2 > 0)
		{
			text2 = text2.Substring(0, num2);
		}
		htmlNode2 = documentNode.SelectSingleNode("//span[@itemprop='nationality']/img");
		string value8 = ((htmlNode2 != null) ? htmlNode2.GetAttributeValue("title", string.Empty) : string.Empty);
		htmlNode2 = documentNode.SelectSingleNode("//span[@itemprop='height']");
		string input = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
		input = GetDigit(input);
		htmlNode = documentNode.SelectSingleNode("//div[@class='info-table info-table--right-space ']");
		if (htmlNode == null)
		{
			return false;
		}
		htmlNode2 = htmlNode.SelectSingleNode("span[1]");
		int num3 = 1;
		while (htmlNode2 != null)
		{
			htmlNode2 = htmlNode.SelectSingleNode("span[" + num3 + "]");
			HtmlNode htmlNode3 = htmlNode.SelectSingleNode("span[" + (num3 + 1) + "]");
			string text6 = ((htmlNode2 != null) ? htmlNode2.InnerText : string.Empty);
			text6 = text6.TrimStart(trimChars).TrimEnd(trimChars);
			string text7 = ((htmlNode3 != null) ? htmlNode3.InnerText : string.Empty);
			text7 = text7.TrimStart(trimChars).TrimEnd(trimChars);
			if (text6.Contains("Date of birth:"))
			{
				text2 = text7;
			}
			else if (text6.Contains("Foot:"))
			{
				value = text7;
			}
			if (text6.Contains("Age:"))
			{
				value5 = text7;
			}
			else if (text6.Contains("Joined:"))
			{
				value2 = text7;
			}
			else if (text6.Contains("Contract option:"))
			{
				if (!text7.ToLower().Contains("obligation") && !text7.ToLower().Contains("option"))
				{
				}
			}
			else if (text6.Contains("Contract there expires:"))
			{
				text = text7;
			}
			else if (text6.Contains("On loan from:"))
			{
				value6 = text7;
				value3 = text;
			}
			else if (text6.Contains("Contract expires:"))
			{
				text = text7;
			}
			else if (text6.Contains("Current club:"))
			{
				value7 = text7;
			}
			else if (text6.Contains("Position:"))
			{
				value4 = text7;
			}
			num3 += 2;
		}
		dataRow = m_WebTable.NewRow();
		dataRow["website"] = "transfermrkt";
		dataRow["name"] = innerText;
		dataRow["type"] = "Player";
		dataRow["birthdate"] = text2;
		dataRow["age"] = value5;
		DateTime birthdate = FifaUtil.ConvertToDate(text2);
		dataRow["country"] = value8;
		dataRow["role"] = value4;
		dataRow["height"] = input;
		dataRow["foot"] = value;
		dataRow["number"] = text3;
		dataRow["team"] = value7;
		dataRow["since"] = value2;
		dataRow["contract"] = text;
		dataRow["loanenddate"] = value3;
		dataRow["loanedfrom"] = value6;
		dataRow["marketvalue"] = text5;
		SplitPlayerName(innerText, dataRow);
		string commonName = dataRow["commonname"].ToString();
		string firstName = dataRow["firstname"].ToString();
		text4 = dataRow["lastname"].ToString();
		Player player = FifaEnvironment.Players.MatchPlayerByNameBirthday(ref firstName, ref text4, ref commonName, birthdate);
		dataRow["commonname"] = commonName;
		dataRow["firstname"] = firstName;
		dataRow["lastname"] = text4;
		if (player != null)
		{
			dataRow["id"] = player.Id.ToString();
		}
		else
		{
			dataRow["id"] = m_NewPersonId.ToString();
			m_NewPersonId = FifaEnvironment.Players.GetNextId(m_NewPersonId + 1);
		}
		m_WebTable.Rows.Add(dataRow);
		return true;
	}

	private string CleanString(string srcString)
	{
		if (srcString == null)
		{
			return string.Empty;
		}
		if (srcString == string.Empty)
		{
			return srcString;
		}
		return srcString.Replace('\t', ' ').Replace('\n', ' ').Trim();
	}

	private bool ExtractManagerFromTransfermrkt()
	{
		return false;
	}

	private bool ExtractManagersFromTransfermrkt()
	{
		m_WebTable.NewRow();
		HtmlNodeCollection htmlNodeCollection = m_CurrentHtmlDocument.DocumentNode.SelectNodes("//td[@class='hauptlink']/a");
		HtmlWeb htmlWeb = new HtmlWeb();
		HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
		if (htmlNodeCollection == null)
		{
			return false;
		}
		foreach (HtmlNode item in (IEnumerable<HtmlNode>)htmlNodeCollection)
		{
			string attributeValue = item.GetAttributeValue("href", string.Empty);
			if (!attributeValue.Contains("trainer"))
			{
				continue;
			}
			string text = "https://www.transfermarkt.com" + attributeValue;
			if (text != string.Empty)
			{
				try
				{
					htmlDocument = htmlWeb.Load(text);
					m_CurrentHtmlDocument = htmlDocument;
					ExtractManagerFromTransfermrkt();
				}
				catch
				{
				}
			}
		}
		return true;
	}

	private bool ExtractClubValueFromTransfermrkt()
	{
		return false;
	}

	private bool ExtractInfoFromSofifa()
	{
		if (m_CanScrapPlayer)
		{
			return ExtractPlayerFromSofifa();
		}
		if (m_CanScrapTeam)
		{
			return ExtractTeamFromSofifa();
		}
		if (m_CanScrapMultiplePlayers)
		{
			return ExtractMultiplePlayersFromSofifa();
		}
		return false;
	}

	private bool ExtractTeamFromSofifa()
	{
		HtmlNode documentNode = m_CurrentHtmlDocument.DocumentNode;
		m_IsDecemberContract = false;
		HtmlNode htmlNode = documentNode.SelectSingleNode("//div[@class='info']/h1");
		if (htmlNode != null)
		{
			string innerText = htmlNode.InnerText;
			_ = FifaEnvironment.Teams.MatchByname(innerText)?.Country;
		}
		HtmlNodeCollection htmlNodeCollection = documentNode.SelectNodes("//figure");
		HtmlWeb htmlWeb = new HtmlWeb();
		HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
		foreach (HtmlNode item in (IEnumerable<HtmlNode>)htmlNodeCollection)
		{
			HtmlNode htmlNode2 = item.SelectSingleNode("../../td[2]/a");
			string text = "https://sofifa.com" + htmlNode2.GetAttributeValue("href", string.Empty);
			if (text != string.Empty)
			{
				try
				{
					m_Url = text;
					htmlDocument = htmlWeb.Load(text);
					m_CurrentHtmlDocument = htmlDocument;
					ExtractPlayerFromSofifa();
				}
				catch
				{
				}
			}
		}
		return true;
	}

	private bool ExtractMultiplePlayersFromSofifa()
	{
		HtmlNodeCollection htmlNodeCollection = m_CurrentHtmlDocument.DocumentNode.SelectNodes("//td[@class='col-name']/a[@role='tooltip']");
		HtmlWeb htmlWeb = new HtmlWeb();
		HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
		if (htmlNodeCollection == null)
		{
			return false;
		}
		foreach (HtmlNode item in (IEnumerable<HtmlNode>)htmlNodeCollection)
		{
			string text = "https://sofifa.com" + item.GetAttributeValue("href", string.Empty);
			if (text != string.Empty)
			{
				try
				{
					htmlDocument = htmlWeb.Load(text);
					m_CurrentHtmlDocument = htmlDocument;
					ExtractPlayerFromSofifa();
				}
				catch
				{
				}
			}
		}
		return true;
	}

	private bool ExtractPlayerFromSofifa()
	{
		DataRow dataRow = m_WebTable.NewRow();
		m_WebTable.Rows.Add(dataRow);
		HtmlNode documentNode = m_CurrentHtmlDocument.DocumentNode;
		HtmlNode htmlNode = documentNode.SelectSingleNode("//img[@data-type='player']");
		if (htmlNode == null)
		{
			return false;
		}
		m_Picture = null;
		Uri uri = new Uri(htmlNode.GetAttributeValue("data-src", string.Empty));
		if (uri.ToString().Contains("notfound"))
		{
			m_Picture = null;
		}
		else
		{
			m_Picture = ((uri != null) ? ((Bitmap)DownloadImage(uri)) : null);
		}
		m_WebPictures.Add(m_Picture);
		dataRow["website"] = "sofifa";
		dataRow["type"] = "Player";
		char[] separator = new char[1] { '/' };
		string[] array = m_Url.Split(separator);
		dataRow["id"] = array[4];
		htmlNode = documentNode.SelectSingleNode("//div[@class='profile clearfix']/h1");
		string text = htmlNode.InnerText;
		int num = text.IndexOf(';');
		if (num >= 0)
		{
			text = text.Substring(num + 1);
		}
		dataRow["name"] = text;
		SplitPlayerName(text, dataRow);
		dataRow["commonname"].ToString();
		dataRow["firstname"].ToString();
		dataRow["lastname"].ToString();
		htmlNode = documentNode.SelectSingleNode("//div[@class='profile clearfix']/p");
		string text2 = htmlNode.InnerText.Trim();
		HtmlNode htmlNode2 = htmlNode.SelectSingleNode("a");
		if (htmlNode2 != null)
		{
			dataRow["country"] = htmlNode2.GetAttributeValue("title", string.Empty);
		}
		htmlNode2 = htmlNode.SelectSingleNode("span");
		int num2;
		if (htmlNode2 != null)
		{
			string text3 = htmlNode2.InnerText.Trim();
			num2 = text3.IndexOf(' ');
			if (num2 < 0)
			{
				num2 = text3.Length;
			}
			dataRow["role"] = text2.Substring(0, num2);
		}
		num2 = text2.IndexOf('(');
		int num3 = text2.IndexOf(')');
		dataRow["birthdate"] = text2.Substring(num2 + 1, num3 - num2 - 1);
		num2 = text2.IndexOf("y.");
		dataRow["age"] = text2.Substring(num2 - 2, 2);
		num2 = text2.IndexOf("cm");
		dataRow["height"] = text2.Substring(num2 - 3, 3);
		num2 = text2.IndexOf("kg");
		dataRow["weight"] = text2.Substring(num2 - 3, 3).Trim();
		htmlNode = documentNode.SelectSingleNode("//label[text()='Preferred foot']");
		dataRow["foot"] = htmlNode.NextSibling.InnerText;
		htmlNode = documentNode.SelectSingleNode("//label[text()='Kit number']");
		if (htmlNode != null)
		{
			dataRow["number"] = htmlNode.NextSibling.InnerText;
		}
		else
		{
			dataRow["number"] = "-";
		}
		htmlNode = documentNode.SelectSingleNode("//label[text()='Joined']");
		if (htmlNode != null)
		{
			dataRow["since"] = htmlNode.NextSibling.InnerText;
		}
		htmlNode = documentNode.SelectSingleNode("//label[text()='Contract valid until']");
		if (htmlNode != null)
		{
			string innerText = htmlNode.NextSibling.InnerText;
			if (m_IsDecemberContract)
			{
				dataRow["contract"] = "Dec 31, " + innerText;
			}
			else
			{
				dataRow["contract"] = "Jun 30, " + innerText;
			}
		}
		else
		{
			dataRow["contract"] = string.Empty;
		}
		htmlNode = documentNode.SelectSingleNode("//div[text()='Value']");
		if (htmlNode != null)
		{
			dataRow["marketvalue"] = htmlNode.ParentNode.FirstChild.InnerText;
		}
		htmlNode = documentNode.SelectSingleNode("//div[text()='Potential']");
		dataRow["potential"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//div[text()='Overall rating']");
		dataRow["overall"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//label[text()='Weak foot']");
		if (htmlNode != null)
		{
			dataRow["weakfoot"] = htmlNode.ParentNode.FirstChild.InnerText;
		}
		htmlNode = documentNode.SelectSingleNode("//label[text()='Skill moves']");
		if (htmlNode != null)
		{
			dataRow["skillmoves"] = htmlNode.ParentNode.FirstChild.InnerText;
		}
		htmlNode = documentNode.SelectSingleNode("//span[text()='Crossing']");
		dataRow["crossing"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Finishing']");
		dataRow["finishing"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Heading accuracy']");
		dataRow["heading"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Short passing']");
		dataRow["shortpassing"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Volleys']");
		dataRow["volleys"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Dribbling']");
		dataRow["dribbling"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Curve']");
		dataRow["curve"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='FK Accuracy']");
		dataRow["fkaccuracy"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Long passing']");
		dataRow["longpassing"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Ball control']");
		dataRow["ballcontrol"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Acceleration']");
		dataRow["acceleration"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Sprint speed']");
		dataRow["sprintspeed"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Agility']");
		dataRow["agility"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Reactions']");
		dataRow["reactions"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Balance']");
		dataRow["balance"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Shot power']");
		dataRow["shotpower"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Jumping']");
		dataRow["jumping"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Stamina']");
		dataRow["stamina"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Strength']");
		dataRow["strength"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Long shots']");
		dataRow["longshots"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Aggression']");
		dataRow["aggression"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Interceptions']");
		dataRow["interceptions"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Att. Position']");
		if (htmlNode != null)
		{
			dataRow["positioning"] = htmlNode.ParentNode.FirstChild.InnerText;
		}
		else
		{
			htmlNode = documentNode.SelectSingleNode("//span[text()='Attack position']");
			if (htmlNode != null)
			{
				dataRow["positioning"] = htmlNode.ParentNode.FirstChild.InnerText;
			}
			else
			{
				dataRow["positioning"] = 50;
			}
		}
		htmlNode = documentNode.SelectSingleNode("//span[text()='Vision']");
		dataRow["vision"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Penalties']");
		dataRow["penalties"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Standing tackle']");
		dataRow["standingtackle"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Sliding tackle']");
		dataRow["slidingtackle"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='Defensive awareness']");
		if (htmlNode != null)
		{
			dataRow["marking"] = htmlNode.ParentNode.FirstChild.InnerText;
		}
		else
		{
			htmlNode = documentNode.SelectSingleNode("//span[text()='Marking']");
			if (htmlNode != null)
			{
				dataRow["marking"] = htmlNode.ParentNode.FirstChild.InnerText;
			}
			else
			{
				dataRow["marking"] = 50;
			}
		}
		htmlNode = documentNode.SelectSingleNode("//span[text()='GK Diving']");
		dataRow["gkdiving"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='GK Handling']");
		dataRow["gkhandling"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='GK Kicking']");
		dataRow["gkkicking"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='GK Positioning']");
		dataRow["gkpositioning"] = htmlNode.ParentNode.FirstChild.InnerText;
		htmlNode = documentNode.SelectSingleNode("//span[text()='GK Reflexes']");
		dataRow["gkreflexes"] = htmlNode.ParentNode.FirstChild.InnerText;
		return true;
	}

	private bool ExtractPersonFromSortitusi()
	{
		m_WebPictures.Clear();
		m_WebTable.Clear();
		bool flag = false;
		DataRow dataRow = m_WebTable.NewRow();
		m_WebTable.Rows.Add(dataRow);
		HtmlNode documentNode = m_CurrentHtmlDocument.DocumentNode;
		HtmlNodeCollection htmlNodeCollection = documentNode.SelectNodes("//div/img");
		if (htmlNodeCollection == null)
		{
			return false;
		}
		foreach (HtmlNode item in (IEnumerable<HtmlNode>)htmlNodeCollection)
		{
			string attributeValue = item.GetAttributeValue("src", string.Empty);
			if (attributeValue.Contains("/face/"))
			{
				Uri uri = new Uri(attributeValue);
				m_Picture = ((uri != null) ? ((Bitmap)DownloadImage(uri)) : null);
				m_WebPictures.Add(m_Picture);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		flag = false;
		dataRow["website"] = "sortitoutsi";
		string nameSurname = (string)(dataRow["name"] = documentNode.SelectSingleNode("//dt[text()='Name']").NextSibling.NextSibling.InnerText);
		SplitPlayerName(nameSurname, dataRow);
		string commonName = dataRow["commonname"].ToString();
		string firstName = dataRow["firstname"].ToString();
		string lastName = dataRow["lastname"].ToString();
		if (documentNode.SelectSingleNode("//dt[text()='Club Job']") == null)
		{
			m_NewPersonId = FifaEnvironment.Players.GetNewId();
			dataRow["type"] = "Player";
			Player player = FifaEnvironment.Players.MatchPlayerByNameBirthday(ref firstName, ref lastName, ref commonName, default(DateTime));
			dataRow["commonname"] = commonName;
			dataRow["firstname"] = firstName;
			dataRow["lastname"] = lastName;
			dataRow["id"] = player?.Id ?? m_NewPersonId;
			return true;
		}
		return false;
	}

	private bool ExtractManagerFromSortitusi()
	{
		return false;
	}

	private bool ExtractInfoFromSortitusi()
	{
		if (m_CanScrapPlayer)
		{
			return ExtractPersonFromSortitusi();
		}
		if (m_CanScrapManager)
		{
			return ExtractPersonFromSortitusi();
		}
		return false;
	}

	private bool ExtractInfoFromSoccerway()
	{
		return false;
	}
}
