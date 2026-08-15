using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace FifaLibrary;

public class CommentaryDictionary : Dictionary<string, int>
{
	private static DataSet m_CommentaryDataSet;

	public void Initialize()
	{
		string path = FifaEnvironment.LaunchDir + "\\CommentaryNames.xml";
		if (!File.Exists(path) || m_CommentaryDataSet != null)
		{
			return;
		}
		m_CommentaryDataSet = new DataSet();
		FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
		m_CommentaryDataSet.ReadXml(fileStream);
		fileStream.Close();
		foreach (DataRow row in m_CommentaryDataSet.Tables[0].Rows)
		{
			string key = (string)row["Name"];
			int value = Convert.ToInt32(row["Id"]);
			if (!ContainsKey(key))
			{
				Add(key, value);
			}
		}
	}

	public int SearchName(string name)
	{
		if (TryGetValue(name, out var value))
		{
			return value;
		}
		return -1;
	}

	public bool IsInitialized()
	{
		return m_CommentaryDataSet != null;
	}
}
