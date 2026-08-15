using System;
using System.IO;

namespace FifaLibrary;

public static class ContainersPatcher
{
	public static string s_FileName;

	public static string s_FolderName;

	private static void SetFileName()
	{
		if (s_FileName == null)
		{
			s_FolderName = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\FIFA 16\\0\\FIFA16";
			s_FileName = s_FolderName + "\\CONTAINERS";
		}
	}

	private static void FormatAndWrite(string type, bool isAuto, DateTime date, BinaryWriter bw)
	{
		int value = type.Length + 14 + ((!isAuto) ? 1 : 2);
		bw.Write(value);
		bw.Write(0);
		string text = type;
		text += date.Year.ToString("D4");
		text += date.Month.ToString("D2");
		text += date.Day.ToString("D2");
		text += date.Hour.ToString("D2");
		text += date.Minute.ToString("D2");
		text += date.Second.ToString("D2");
		if (isAuto)
		{
			text += "A";
		}
		FifaUtil.WriteNullTerminatedString(bw, text);
	}

	public static bool RegenerateFifa16()
	{
		SetFileName();
		if (!File.Exists(s_FileName))
		{
			return false;
		}
		FileStream fileStream = new FileStream(s_FileName, FileMode.Create, FileAccess.Write);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		string[] directories = Directory.GetDirectories(s_FolderName);
		foreach (string obj in directories)
		{
			string path = obj + "\\DATA";
			string path2 = obj + "\\INDEX";
			if (File.Exists(path) && File.Exists(path2))
			{
				File.GetCreationTime(path);
				DateTime creationTime = File.GetCreationTime(path2);
				FileStream fileStream2 = new FileStream(path, FileMode.Open, FileAccess.Read);
				BinaryReader binaryReader = new BinaryReader(fileStream2);
				fileStream2.Position = 16L;
				bool isAuto = FifaUtil.ReadNullTerminatedString(binaryReader).Contains(" - Auto");
				fileStream2.Position = 116L;
				string text = FifaUtil.ReadNullTerminatedString(binaryReader);
				binaryReader.Close();
				fileStream2.Close();
				switch (text)
				{
				case "SaveType_Tourna":
					FormatAndWrite("Tournament", isAuto, creationTime, binaryWriter);
					break;
				case "SaveType_Settin":
					FormatAndWrite("Settings", isAuto: false, creationTime, binaryWriter);
					break;
				case "SaveType_Career":
					FormatAndWrite("Career", isAuto, creationTime, binaryWriter);
					break;
				case "SaveType_Matchd":
					FormatAndWrite("MatchDay", isAuto: false, creationTime, binaryWriter);
					break;
				}
			}
		}
		binaryWriter.Close();
		fileStream.Close();
		return true;
	}

	public static bool RegenerateRFSTournaments()
	{
		SetFileName();
		if (!File.Exists(s_FileName))
		{
			return false;
		}
		FileStream fileStream = new FileStream(s_FileName, FileMode.Create, FileAccess.Write);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		string[] directories = Directory.GetDirectories(s_FolderName);
		for (int i = 0; i < directories.Length; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(directories[i]);
			string obj = directories[i];
			string path = obj + "\\DATA";
			string path2 = obj + "\\INDEX";
			if (!File.Exists(path) || !File.Exists(path2))
			{
				continue;
			}
			File.GetCreationTime(path);
			DateTime date = File.GetCreationTime(path2);
			FileStream fileStream2 = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(fileStream2);
			fileStream2.Position = 16L;
			bool isAuto = FifaUtil.ReadNullTerminatedString(binaryReader).Contains(" - Auto");
			fileStream2.Position = 116L;
			string text = FifaUtil.ReadNullTerminatedString(binaryReader);
			binaryReader.Close();
			fileStream2.Close();
			switch (text)
			{
			case "SaveType_Tourna":
				switch (fileNameWithoutExtension)
				{
				case "968d5edd":
					date = new DateTime(2021, 6, 8, 23, 20, 45);
					break;
				case "892246ec":
					date = new DateTime(2021, 6, 9, 0, 10, 45);
					break;
				case "45dff1f4":
					date = new DateTime(2021, 6, 13, 23, 47, 34);
					break;
				case "d7e554f0":
					date = new DateTime(2021, 6, 13, 23, 45, 54);
					break;
				case "d4e5503a":
					date = new DateTime(2021, 6, 13, 23, 45, 1);
					break;
				case "56005eaf":
					date = new DateTime(2021, 6, 13, 23, 48, 34);
					break;
				}
				FormatAndWrite("Tournament", isAuto, date, binaryWriter);
				break;
			case "SaveType_Settin":
				FormatAndWrite("Settings", isAuto: false, date, binaryWriter);
				break;
			case "SaveType_Career":
				FormatAndWrite("Career", isAuto, date, binaryWriter);
				break;
			case "SaveType_Matchd":
				FormatAndWrite("MatchDay", isAuto: false, date, binaryWriter);
				break;
			}
		}
		binaryWriter.Close();
		fileStream.Close();
		return true;
	}

	public static bool RegenerateFifa16(DateTime forceTournamentCreationDate)
	{
		SetFileName();
		if (!File.Exists(s_FileName))
		{
			return false;
		}
		FileStream fileStream = new FileStream(s_FileName, FileMode.Create, FileAccess.Write);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		string[] directories = Directory.GetDirectories(s_FolderName);
		foreach (string obj in directories)
		{
			string path = obj + "\\DATA";
			string path2 = obj + "\\INDEX";
			if (File.Exists(path) && File.Exists(path2))
			{
				File.GetCreationTime(path);
				DateTime creationTime = File.GetCreationTime(path2);
				FileStream fileStream2 = new FileStream(path, FileMode.Open, FileAccess.Read);
				BinaryReader binaryReader = new BinaryReader(fileStream2);
				fileStream2.Position = 16L;
				bool isAuto = FifaUtil.ReadNullTerminatedString(binaryReader).Contains(" - Auto");
				fileStream2.Position = 116L;
				string text = FifaUtil.ReadNullTerminatedString(binaryReader);
				binaryReader.Close();
				fileStream2.Close();
				switch (text)
				{
				case "SaveType_Tourna":
					FormatAndWrite("Tournament", isAuto, forceTournamentCreationDate, binaryWriter);
					break;
				case "SaveType_Settin":
					FormatAndWrite("Settings", isAuto: false, creationTime, binaryWriter);
					break;
				case "SaveType_Career":
					FormatAndWrite("Career", isAuto, creationTime, binaryWriter);
					break;
				case "SaveType_Matchd":
					FormatAndWrite("MatchDay", isAuto: false, creationTime, binaryWriter);
					break;
				}
			}
		}
		binaryWriter.Close();
		fileStream.Close();
		return true;
	}

	public static void DeleteAllTournamentFiles()
	{
		SetFileName();
		string[] directories = Directory.GetDirectories(s_FolderName);
		foreach (string text in directories)
		{
			string path = text + "\\DATA";
			string path2 = text + "\\INDEX";
			if (File.Exists(path) && File.Exists(path2))
			{
				FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
				BinaryReader binaryReader = new BinaryReader(fileStream);
				fileStream.Position = 116L;
				string text2 = FifaUtil.ReadNullTerminatedString(binaryReader);
				binaryReader.Close();
				fileStream.Close();
				if (text2 == "SaveType_Tourna")
				{
					Directory.Delete(text, recursive: true);
				}
			}
		}
	}

	public static void CopyTournamentFiles(string sourceFolder)
	{
		SetFileName();
		Path.GetFileNameWithoutExtension(sourceFolder);
		string path = sourceFolder + "\\INDEX";
		File.GetLastWriteTime(path);
		DateTime creationTime = File.GetCreationTime(path);
		string fileName = Path.GetFileName(sourceFolder);
		fileName = s_FolderName + "\\" + fileName;
		if (!Directory.Exists(fileName))
		{
			Directory.CreateDirectory(fileName);
		}
		string[] files = Directory.GetFiles(sourceFolder);
		foreach (string obj in files)
		{
			string fileName2 = Path.GetFileName(obj);
			string text = Path.Combine(fileName, fileName2);
			File.Copy(obj, text, overwrite: true);
			File.SetCreationTime(text, creationTime);
		}
		RegenerateRFSTournaments();
	}

	public static DateTime CheckLatestTournament(DateTime afterDate)
	{
		SetFileName();
		FileStream fileStream = new FileStream(s_FileName, FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader = new BinaryReader(fileStream);
		DateTime dateTime = afterDate.AddSeconds(10.0);
		while (fileStream.Position < fileStream.Length)
		{
			fileStream.Position += 8L;
			string text = FifaUtil.ReadNullTerminatedString(binaryReader);
			if (text.StartsWith("Tournament"))
			{
				string text2 = text.Substring(10);
				string value = text2.Substring(0, 4);
				string value2 = text2.Substring(4, 2);
				string value3 = text2.Substring(6, 2);
				string value4 = text2.Substring(8, 2);
				string value5 = text2.Substring(10, 2);
				string value6 = text2.Substring(12, 2);
				int year = Convert.ToInt32(value);
				int month = Convert.ToInt32(value2);
				int day = Convert.ToInt32(value3);
				int hour = Convert.ToInt32(value4);
				int minute = Convert.ToInt32(value5);
				int second = Convert.ToInt32(value6);
				DateTime dateTime2 = new DateTime(year, month, day, hour, minute, second);
				if (dateTime2 > dateTime)
				{
					dateTime = dateTime2;
				}
			}
		}
		binaryReader.Close();
		fileStream.Close();
		return dateTime;
	}

	public static string GetDataFileName(DateTime creationDate)
	{
		string[] directories = Directory.GetDirectories(s_FolderName);
		string text = null;
		for (int i = 0; i < directories.Length; i++)
		{
			DateTime creationTime = Directory.GetCreationTime(directories[i]);
			long num = 0L;
			num = Math.Abs(creationTime.Ticks - creationDate.Ticks);
			TimeSpan timeSpan = new TimeSpan(num);
			if (timeSpan.Minutes == 0 && timeSpan.Seconds <= 2)
			{
				text = directories[i] + "\\DATA";
				FileStream obj = new FileStream(text, FileMode.Open, FileAccess.Read)
				{
					Position = 125L
				};
				int num2 = obj.ReadByte();
				obj.Close();
				if (num2 == 84)
				{
					break;
				}
				text = null;
			}
		}
		return text;
	}

	public static string GetProfileFileName()
	{
		string[] directories = Directory.GetDirectories(s_FolderName);
		string text = null;
		for (int i = 0; i < directories.Length; i++)
		{
			Directory.GetCreationTime(directories[i]);
			text = directories[i] + "\\DATA";
			FileStream obj = new FileStream(text, FileMode.Open, FileAccess.Read)
			{
				Position = 125L
			};
			int num = obj.ReadByte();
			obj.Close();
			if (num == 83)
			{
				break;
			}
			text = null;
		}
		return text;
	}
}
