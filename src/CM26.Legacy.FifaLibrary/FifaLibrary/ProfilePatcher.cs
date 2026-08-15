using System;
using System.IO;

namespace FifaLibrary;

public static class ProfilePatcher
{
	private static int[] c_RfsCpuDefaultSlides = new int[16]
	{
		75, 50, 15, 25, 55, 40, 80, 70, 15, 40,
		90, 50, 35, 50, 50, 25
	};

	private static int[] c_RfsUserDefaultSlides = new int[17]
	{
		75, 50, 15, 25, 45, 25, 80, 70, 15, 40,
		90, 50, 35, 50, 50, 50, 25
	};

	private static int[] c_TrainingAudio = new int[6] { 0, 0, 10, 10, 1, 10 };

	private static int[] s_UserSlides = new int[17];

	private static int[] s_CpuSlides = new int[16];

	private static int[] s_AudioValues = new int[6];

	private static int[] s_UserBackupSlides = new int[16];

	private static int[] s_CpuBackupSlides = new int[16];

	private static int[] s_AudioBackupValues = new int[6];

	public static string s_FileName;

	private static void SetDefaultValues(bool useRfsValues)
	{
		for (int i = 0; i < 16; i++)
		{
			s_CpuSlides[i] = 50;
		}
		for (int j = 0; j < 17; j++)
		{
			s_UserSlides[j] = 50;
		}
		for (int k = 0; k < 6; k++)
		{
			s_AudioValues[k] = 10;
		}
	}

	public static void Open(string fileName)
	{
		s_FileName = fileName;
		FileStream fileStream = new FileStream(s_FileName, FileMode.Open, FileAccess.Read);
		if (fileStream == null)
		{
			return;
		}
		BinaryReader binaryReader = new BinaryReader(fileStream);
		if (binaryReader != null)
		{
			binaryReader.BaseStream.Position = 2480L;
			for (int i = 0; i < 6; i++)
			{
				s_AudioValues[i] = binaryReader.ReadInt32();
			}
			binaryReader.BaseStream.Position = 2636L;
			for (int j = 0; j < 16; j++)
			{
				s_CpuSlides[j] = binaryReader.ReadInt32();
			}
			for (int k = 0; k < 16; k++)
			{
				s_UserSlides[k] = binaryReader.ReadInt32();
			}
			binaryReader.Close();
			fileStream.Close();
		}
	}

	public static void SetTrainingAudio()
	{
		for (int i = 0; i < 6; i++)
		{
			s_AudioValues[i] = c_TrainingAudio[i];
		}
	}

	public static void SetRfsSlides()
	{
		for (int i = 0; i < 16; i++)
		{
			s_CpuSlides[i] = c_RfsCpuDefaultSlides[i];
		}
		for (int j = 0; j < 17; j++)
		{
			s_UserSlides[j] = c_RfsUserDefaultSlides[j];
		}
	}

	public static void ApplyDynamicSlides(int[] deltaSlides)
	{
		for (int i = 0; i < 16; i++)
		{
			s_CpuSlides[i] += deltaSlides[i];
			if (s_CpuSlides[i] < 5)
			{
				s_CpuSlides[i] = 5;
			}
			if (s_CpuSlides[i] > 95)
			{
				s_CpuSlides[i] = 95;
			}
			if (i >= 11 && i <= 13)
			{
				s_UserSlides[i] -= deltaSlides[i];
				if (s_UserSlides[i] < 5)
				{
					s_UserSlides[i] = 5;
				}
				if (s_UserSlides[i] > 95)
				{
					s_UserSlides[i] = 95;
				}
			}
		}
	}

	public static void SetEaSlides()
	{
		for (int i = 0; i < 16; i++)
		{
			s_CpuSlides[i] = 50;
		}
		for (int j = 0; j < 17; j++)
		{
			s_UserSlides[j] = 50;
		}
	}

	public static void SetDefaultAudio()
	{
		for (int i = 0; i < 6; i++)
		{
			s_AudioValues[i] = 10;
		}
	}

	public static void SetValues(int[] userValues, int[] cpuValues, int[] audioValues)
	{
		FileStream fileStream = new FileStream(s_FileName, FileMode.Open, FileAccess.Write);
		if (fileStream == null)
		{
			return;
		}
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		if (binaryWriter == null)
		{
			return;
		}
		if (audioValues != null)
		{
			binaryWriter.BaseStream.Position = 2480L;
			for (int i = 0; i < 6; i++)
			{
				binaryWriter.Write(audioValues[i]);
			}
		}
		binaryWriter.BaseStream.Position = 1196L;
		if (cpuValues != null)
		{
			for (int j = 0; j < 16; j++)
			{
				binaryWriter.Write(cpuValues[j]);
			}
		}
		if (userValues != null)
		{
			for (int k = 0; k < 16; k++)
			{
				binaryWriter.Write(userValues[k]);
			}
		}
		binaryWriter.Close();
		fileStream.Close();
	}

	public static bool Save()
	{
		FileStream fileStream = new FileStream(s_FileName, FileMode.Open, FileAccess.Write);
		if (fileStream == null)
		{
			return false;
		}
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		if (binaryWriter == null)
		{
			return false;
		}
		DateTime lastWriteTime = File.GetLastWriteTime(s_FileName);
		int num = 132;
		binaryWriter.BaseStream.Position = num;
		for (int i = 0; i < 8; i++)
		{
			binaryWriter.Write((byte)0);
		}
		binaryWriter.BaseStream.Position = 2480L;
		for (int j = 0; j < 6; j++)
		{
			binaryWriter.Write(s_AudioValues[j]);
		}
		binaryWriter.BaseStream.Position = 2636L;
		for (int k = 0; k < 16; k++)
		{
			binaryWriter.Write(s_CpuSlides[k]);
		}
		for (int l = 0; l < 17; l++)
		{
			binaryWriter.Write(s_UserSlides[l]);
		}
		binaryWriter.Close();
		fileStream.Close();
		File.SetLastWriteTime(s_FileName, lastWriteTime);
		return true;
	}

	public static void RestoreValues()
	{
		for (int i = 0; i < 6; i++)
		{
			s_AudioValues[i] = s_AudioBackupValues[i];
		}
		for (int j = 0; j < 16; j++)
		{
			s_CpuSlides[j] = s_CpuBackupSlides[j];
			s_UserSlides[j] = s_UserBackupSlides[j];
		}
	}

	public static void BackupValues()
	{
		for (int i = 0; i < 6; i++)
		{
			s_AudioBackupValues[i] = s_AudioValues[i];
		}
		for (int j = 0; j < 16; j++)
		{
			s_CpuBackupSlides[j] = s_CpuSlides[j];
			s_UserBackupSlides[j] = s_UserSlides[j];
		}
	}
}
