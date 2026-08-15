using System.Collections.Generic;
using System.Text;

namespace FifaLibrary;

public class NameDictionary : Dictionary<int, string>
{
	public NameDictionary(DbFile fifaDbFile)
	{
		Table commentaryNamesTable = fifaDbFile.Table[TI.commentarynames];
		Load(commentaryNamesTable);
		Table playernamesTable = fifaDbFile.Table[TI.playernames];
		FillFromPlayernames(playernamesTable);
	}

	public NameDictionary(Table commentaryNamesTable, Table playernamesTable)
	{
		Load(commentaryNamesTable);
		FillFromPlayernames(playernamesTable);
	}

	public void Load(Table commentaryNamesTable)
	{
		Clear();
		for (int i = 0; i < commentaryNamesTable.NRecords; i++)
		{
			Record record = commentaryNamesTable.Records[i];
			int key = record.IntField[FI.commentarynames_commentaryid];
			if (!ContainsKey(key))
			{
				string text = string.Empty;
				if (record.TableDescriptor.NCompressedStringFields > 0)
				{
					text = record.CompressedString[FI.commentarynames_commentarystring];
				}
				else if (record.TableDescriptor.NStringFields > 0)
				{
					text = record.StringField[FI.commentarynames_commentarystring];
				}
				string text2 = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(text));
				if (text2.Length > 0)
				{
					_ = text2.ToUpper()[0];
				}
				Add(key, text);
			}
		}
	}

	public void FillFromPlayernames(Table playernamesTable)
	{
		for (int i = 0; i < playernamesTable.NRecords; i++)
		{
			Record record = playernamesTable.Records[i];
			int num = record.IntField[FI.playernames_commentaryid];
			if (num != 900000 && !ContainsKey(num))
			{
				string text = record.CompressedString[FI.playernames_name];
				string text2 = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(text));
				if (text2.Length > 0)
				{
					_ = text2.ToUpper()[0];
				}
				Add(num, text);
			}
		}
	}

	public void Save(DbFile fifaDbFile)
	{
		Table commentaryNamesTable = fifaDbFile.Table[TI.commentarynames];
		Save(commentaryNamesTable);
	}

	public void Save(Table commentaryNamesTable)
	{
		commentaryNamesTable.ResizeRecords(base.Count);
		commentaryNamesTable.NValidRecords = base.Count;
		int num = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, string> current = enumerator.Current;
			Record obj = commentaryNamesTable.Records[num];
			num++;
			string value = current.Value;
			char c = 'Z';
			string text = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(value));
			if (text.Length > 0)
			{
				c = text.ToUpper()[0];
			}
			obj.IntField[FI.commentarynames_commentarystartingletter] = c - 65 + 1;
			obj.IntField[FI.commentarynames_commentaryid] = current.Key;
			obj.IntField[FI.commentarynames_commentarypreview] = 1;
			obj.CompressedString[FI.commentarynames_commentarystring] = value;
		}
	}

	public int TryGetKey(string value)
	{
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<int, string> current = enumerator.Current;
				if (current.Value.ToLower() == value.ToLower())
				{
					return current.Key;
				}
			}
		}
		return -1;
	}

	public int GetNewKey()
	{
		for (int i = 910000; i < 965535; i++)
		{
			if (!ContainsKey(i))
			{
				return i;
			}
		}
		return 999999;
	}
}
