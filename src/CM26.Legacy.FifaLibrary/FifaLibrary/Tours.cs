using System;
using System.Collections.Generic;
using System.Data;

namespace FifaLibrary;

public class Tours
{
	public static Dictionary<int, string> Confederations = new Dictionary<int, string>
	{
		{ 1, "Europe" },
		{ 3, "South America" },
		{ 4, "Asia" },
		{ 6, "North America" }
	};

	public static Dictionary<int, string> tourtypes = new Dictionary<int, string>
	{
		{ 0, "World Tour" },
		{ 1, "Continental Tour" },
		{ 2, "National Tour" }
	};

	private string m_name;

	private int m_Confedid;

	private string m_Confed;

	private int m_Type;

	private int m_NationID;

	private string m_Nation;

	private decimal m_StarRating;

	private int m_MinimumBudget;

	private int m_European;

	private int m_NorthAmerican;

	private int m_SouthAmerican;

	private int m_ROW;

	private int m_Local;

	private int m_Continental;

	private int m_QualifyPrize;

	private int m_GroupPrize;

	private int m_FinalPrize;

	public string name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	public int Confedid
	{
		get
		{
			return m_Confedid;
		}
		set
		{
			m_Confedid = value;
		}
	}

	public string Confed
	{
		get
		{
			return m_Confed;
		}
		set
		{
			m_Confed = value;
		}
	}

	public int Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public int NationID
	{
		get
		{
			return m_NationID;
		}
		set
		{
			m_NationID = value;
		}
	}

	public string Nation
	{
		get
		{
			return m_Nation;
		}
		set
		{
			m_Nation = value;
		}
	}

	public decimal StarRating
	{
		get
		{
			return m_StarRating;
		}
		set
		{
			m_StarRating = value;
		}
	}

	public int MinimumBudget
	{
		get
		{
			return m_MinimumBudget;
		}
		set
		{
			m_MinimumBudget = value;
		}
	}

	public int European
	{
		get
		{
			return m_European;
		}
		set
		{
			m_European = value;
		}
	}

	public int NorthAmerican
	{
		get
		{
			return m_NorthAmerican;
		}
		set
		{
			m_NorthAmerican = value;
		}
	}

	public int SouthAmerican
	{
		get
		{
			return m_SouthAmerican;
		}
		set
		{
			m_SouthAmerican = value;
		}
	}

	public int ROW
	{
		get
		{
			return m_ROW;
		}
		set
		{
			m_ROW = value;
		}
	}

	public int Local
	{
		get
		{
			return m_Local;
		}
		set
		{
			m_Local = value;
		}
	}

	public int Continental
	{
		get
		{
			return m_Continental;
		}
		set
		{
			m_Continental = value;
		}
	}

	public int QualifyPrize
	{
		get
		{
			return m_QualifyPrize;
		}
		set
		{
			m_QualifyPrize = value;
		}
	}

	public int GroupPrize
	{
		get
		{
			return m_GroupPrize;
		}
		set
		{
			m_GroupPrize = value;
		}
	}

	public int FinalPrize
	{
		get
		{
			return m_FinalPrize;
		}
		set
		{
			m_FinalPrize = value;
		}
	}

	public string GetConfed()
	{
		if (Confederations.ContainsKey(m_Confedid))
		{
			return Confederations[m_Confedid];
		}
		return "";
	}

	public string Gettourtype()
	{
		if (tourtypes.ContainsKey(m_Type))
		{
			return tourtypes[m_Type];
		}
		return "";
	}

	public override string ToString()
	{
		if (m_name != null)
		{
			return m_name;
		}
		return string.Empty;
	}

	public Tours(DataRow dr)
	{
		m_Type = Convert.ToInt32(dr["Type"]);
		m_NationID = ((!(dr["Nation ID"].ToString() == "*")) ? Convert.ToInt32(dr["Nation ID"]) : 0);
		m_Nation = dr["Nation"].ToString();
		m_StarRating = Convert.ToDecimal(dr["Star Rating"]);
		m_MinimumBudget = Convert.ToInt32(dr["Minimum Budget"]);
		m_European = Convert.ToInt32(dr["European"]);
		m_NorthAmerican = Convert.ToInt32(dr["North American"]);
		m_SouthAmerican = Convert.ToInt32(dr["South American"]);
		m_ROW = Convert.ToInt32(dr["ROW"]);
		m_Local = Convert.ToInt32(dr["Local"]);
		m_Continental = Convert.ToInt32(dr["Continental"]);
		m_QualifyPrize = Convert.ToInt32(dr["Qualify Prize"]);
		m_GroupPrize = Convert.ToInt32(dr["Group Prize"]);
		m_FinalPrize = Convert.ToInt32(dr["Final Prize"]);
	}

	public Tours()
	{
		m_Type = 0;
		m_NationID = 0;
		m_Nation = "Any";
		m_StarRating = default(decimal);
		m_MinimumBudget = 0;
		m_European = 0;
		m_NorthAmerican = 0;
		m_SouthAmerican = 0;
		m_ROW = 0;
		m_Local = 0;
		m_Continental = 0;
		m_QualifyPrize = 0;
		m_GroupPrize = 0;
		m_FinalPrize = 0;
	}

	public void setconfed()
	{
		if (m_NationID != 0)
		{
			m_Confedid = FifaEnvironment.Countries.SearchCountry(m_NationID).Confederation;
			m_Confed = GetConfed();
		}
	}

	public void newrow(DataTable t)
	{
		DataRow dataRow = t.NewRow();
		dataRow[0] = m_Type;
		dataRow[1] = m_NationID;
		dataRow[2] = m_Nation;
		dataRow[3] = m_StarRating;
		dataRow[4] = m_MinimumBudget;
		dataRow[5] = m_European;
		dataRow[6] = m_NorthAmerican;
		dataRow[7] = m_SouthAmerican;
		dataRow[8] = m_ROW;
		dataRow[9] = m_Local;
		dataRow[10] = m_Continental;
		dataRow[11] = m_QualifyPrize;
		dataRow[12] = m_GroupPrize;
		dataRow[13] = m_FinalPrize;
		t.Rows.Add(dataRow);
	}
}
