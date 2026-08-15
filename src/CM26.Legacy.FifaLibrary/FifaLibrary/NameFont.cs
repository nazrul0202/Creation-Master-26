using System.IO;

namespace FifaLibrary;

public class NameFont : IdObject
{
	public static void Clone(int oldStyle, int newStyle)
	{
		FifaEnvironment.CloneIntoZdata(NameFontFileName(oldStyle), NameFontFileName(newStyle));
	}

	public static string NameFontFileName(int id)
	{
		return "data/sceneassets/jerseyfonts/font_" + id + ".ttf";
	}

	public NameFont(int id)
		: base(id)
	{
	}

	public override string ToString()
	{
		return "Name Font n. " + base.Id;
	}

	public static string CanShowNameFont(int style)
	{
		string text = NameFontFileName(style);
		string text2 = FifaEnvironment.GameDir + text;
		if (File.Exists(text2))
		{
			return text2;
		}
		return null;
	}

	public static bool Delete(int style)
	{
		return FifaEnvironment.DeleteFromZdata(NameFontFileName(style));
	}

	public static bool Import(int style, string srcFileName)
	{
		string archivedName = NameFontFileName(style);
		return FifaEnvironment.ImportFileIntoZdataAs(srcFileName, archivedName, delete: false, ECompressionMode.Chunkzip2);
	}

	public static bool Export(int style, string exportDir)
	{
		return FifaEnvironment.ExportFileFromZdata(NameFontFileName(style), exportDir);
	}
}
