using System.Drawing;

namespace FifaLibrary;

public class NumberFont : IdObject
{
	private static int s_MaxColors = 20;

	private int m_Style;

	private int m_Color;

	public NumberFont(int fontId)
		: base(fontId)
	{
		ComputeStyleAndColor(fontId);
	}

	private void ComputeStyleAndColor(int fontId)
	{
		m_Style = fontId / s_MaxColors;
		m_Color = fontId - m_Style * s_MaxColors;
	}

	public override string ToString()
	{
		FifaUtil.PadBlanks(base.Id.ToString(), 3);
		string text = "Font n. " + m_Style + " ";
		return m_Color switch
		{
			0 => text + "Transparent", 
			1 => text + "White", 
			2 => text + "Black", 
			3 => text + "Blue", 
			4 => text + "Red", 
			5 => text + "Yellow", 
			6 => text + "Green", 
			7 => text + "Orange", 
			8 => text + "Violet", 
			9 => text + "Brown", 
			10 => text + "Pink", 
			11 => text + "Dark Red", 
			12 => text + "Cyano", 
			13 => text + "Dark Blue", 
			14 => text + "Gray", 
			15 => text + "Pale Green", 
			16 => text + "Dark Gold", 
			17 => text + "Gold", 
			18 => text + "Light Red", 
			19 => text + "Dark Green", 
			_ => text + m_Color, 
		};
	}

	public static void Clone(int oldStyle, int oldColor, int newStyle, int newColor)
	{
		FifaEnvironment.CloneIntoZdata(NumberFontFileName(oldStyle, oldColor), NumberFontFileName(newStyle, newColor));
	}

	public static string NumberFontFileName(int styleId, int colorId)
	{
		return "data/sceneassets/kitnumbers/kitnumbers_" + styleId + "_" + colorId + ".rx3";
	}

	public string NumberFontFileName()
	{
		return NumberFontFileName(m_Style, m_Color);
	}

	public static string NumberFontTemplateName()
	{
		return "data/sceneassets/kitnumbers/kitnumbers_#_%.rx3";
	}

	public static Rx3Signatures NumberFontSignature(int id, int colorId)
	{
		string[] array = new string[10];
		for (int i = 0; i < 10; i++)
		{
			array[i] = "numbers_" + id + "_" + colorId + "_" + i + ".";
		}
		return new Rx3Signatures(439280, 26, array);
	}

	public static Rx3File GetNumberFontRx3(int style, int color)
	{
		return FifaEnvironment.GetRx3FromZdata(NumberFontFileName(style, color));
	}

	public static Bitmap[] GetNumberFont(int style, int color)
	{
		return FifaEnvironment.GetBmpsFromRx3(NumberFontFileName(style, color));
	}

	public static Bitmap[] GetSpecificNumberFont(int teamid, EJerseyShorts jerseyShort, EKitType kitType)
	{
		return FifaEnvironment.GetBmpsFromRx3(SpecificNumberFont.SpecificNumberFontFileName(teamid, jerseyShort, kitType));
	}

	public static bool SetNumberFont(int style, int color, Bitmap[] bitmaps)
	{
		return FifaEnvironment.ImportBmpsIntoZdata(ids: new int[2] { style, color }, templateRx3Name: NumberFontTemplateName(), bitmaps: bitmaps, compressionMode: ECompressionMode.Chunkzip, signatures: NumberFontSignature(style, color));
	}

	public static bool SetNumberFont(int style, int color, string rx3FileName)
	{
		return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, NumberFontFileName(style, color), delete: false, ECompressionMode.Chunkzip);
	}

	public static bool Delete(int style, int color)
	{
		return FifaEnvironment.DeleteFromZdata(NumberFontFileName(style, color));
	}

	public static bool Import(int style, int color, string rx3FileName)
	{
		string archivedName = NumberFontFileName(style, color);
		return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, archivedName, delete: false, ECompressionMode.Chunkzip, NumberFontSignature(style, color));
	}

	public static bool Export(int style, int color, string exportDir)
	{
		return FifaEnvironment.ExportFileFromZdata(NumberFontFileName(style, color), exportDir);
	}
}
