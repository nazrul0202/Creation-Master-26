using System.Collections.Generic;
using System.Data;

namespace FifaLibrary;

public class PreSeasonTour
{
	public List<TourNames> tourlist;

	public List<Tours> tourtypelist;

	public PreSeasonTour(DataTable dtnames, DataTable dttour, List<Record> _listr)
	{
		tourlist = new List<TourNames>();
		foreach (Record item in _listr)
		{
			int andCheckIntField = item.GetAndCheckIntField(FI.competition_competitionid);
			int num = 0;
			foreach (DataRow row in dtnames.Rows)
			{
				if (row["Asset ID"].ToString() == andCheckIntField.ToString())
				{
					TourNames tourNames = new TourNames(item)
					{
						languagestring = row["Tour Name"].ToString()
					};
					tourNames.LongName = FifaEnvironment.Language.GetString(tourNames.languagestring);
					tourNames.ShortName = FifaEnvironment.Language.GetString(tourNames.languagestring + "_abbr15");
					tourNames.Continent = row["Continent"].ToString();
					tourNames.TourType = row["Tour Type"].ToString();
					tourNames.TourName = "Pre-Season Tour " + num;
					tourlist.Add(tourNames);
				}
				num++;
			}
		}
		tourtypelist = new List<Tours>();
		int num2 = 0;
		foreach (DataRow row2 in dttour.Rows)
		{
			Tours tours = new Tours(row2);
			tours.name = "Pre-Season Comp " + num2;
			tours.setconfed();
			tourtypelist.Add(tours);
			num2++;
		}
	}

	public void deletetour(Tours t)
	{
		tourtypelist.Remove(t);
	}

	public Tours newtour()
	{
		int count = tourtypelist.Count;
		Tours tours = new Tours();
		tours.name = "Pre-Season Comp " + count;
		tours.setconfed();
		tourtypelist.Add(tours);
		return tours;
	}

	public bool SaveToCompetition(Table t)
	{
		int count = tourlist.Count;
		int nRecords = t.NRecords;
		t.ResizeRecords(count + nRecords);
		int num = nRecords;
		foreach (TourNames item in tourlist)
		{
			item.SaveCompetition(t.Records[num]);
			num++;
		}
		t.SortByKeys();
		return true;
	}

	public DataTable savetourlist(DataTable basetable)
	{
		DataTable dataTable = new DataTable();
		foreach (DataColumn column in basetable.Columns)
		{
			dataTable.Columns.Add(column.ColumnName);
		}
		foreach (Tours item in tourtypelist)
		{
			item.newrow(dataTable);
		}
		return dataTable;
	}

	public DataTable savetourlistnames(DataTable basetable)
	{
		DataTable dataTable = new DataTable();
		foreach (DataColumn column in basetable.Columns)
		{
			dataTable.Columns.Add(column.ColumnName);
		}
		dataTable.Rows.Add("", "", "", "");
		foreach (TourNames item in tourlist)
		{
			item.newrow(dataTable);
		}
		return dataTable;
	}
}
