using System;
using System.Drawing;
using FifaLibrary;

namespace CreationMaster;

public class RFS_SkillsGenerator
{
	public enum RFS_EWorkRate
	{
		Medium,
		Low,
		High
	}

	public enum RFS_EWeakFoot
	{
		VeryPoor = 1,
		Poor,
		Average,
		Good,
		Ambidexter
	}

	public enum RFS_EGkKickStyle
	{
		GkKick0,
		GkKick1,
		GkKick2,
		GkKick3
	}

	public enum RFS_EGkSaveStyle
	{
		Traditional,
		Acrobatic
	}

	public enum RFS_ESkillMoves
	{
		Stars1,
		Stars2,
		Stars3,
		Stars4,
		Stars5
	}

	private static ulong c_Inflexible = 1uL;

	private static ulong c_Longthrows = 2uL;

	private static ulong c_PowerfulFreeKicks = 4uL;

	private static ulong c_Diver = 8uL;

	private static ulong c_InjuryProne = 16uL;

	private static ulong c_InjuryFree = 32uL;

	private static ulong c_AvoidsWeakFoot = 64uL;

	private static ulong c_Divesintotackles = 128uL;

	private static ulong c_BeatDefensiveLine = 256uL;

	private static ulong c_Selfish = 512uL;

	private static ulong c_Leadership = 1024uL;

	private static ulong c_ArguesWithOfficials = 2048uL;

	private static ulong c_Earlycrosser = 4096uL;

	private static ulong c_FinesseShot = 8192uL;

	private static ulong c_Flair = 16384uL;

	private static ulong c_LongPasser = 32768uL;

	private static ulong c_LongShotTaker = 65536uL;

	private static ulong c_Technicaldribbler = 131072uL;

	private static ulong c_Playmaker = 262144uL;

	private static ulong c_Pushesupforcorners = 524288uL;

	private static ulong c_Puncher = 1048576uL;

	private static ulong c_GkLongThrower = 2097152uL;

	private static ulong c_PowerHeader = 4194304uL;

	private static ulong c_GkOneOnOne = 8388608uL;

	private static ulong c_GiantThrow = 16777216uL;

	private static ulong c_OutsideFootShot = 33554432uL;

	private static ulong c_CrowdFavorite = 67108864uL;

	private static ulong c_SwervePasser = 134217728uL;

	private static ulong c_SecondWind = 268435456uL;

	private static ulong c_AcrobaticClearance = 536870912uL;

	private static ulong c_FancyFeet = 4294967296uL;

	private static ulong c_FancyPasses = 8589934592uL;

	private static ulong c_FancyFlicks = 17179869184uL;

	private static ulong c_StutterPenalty = 34359738368uL;

	private static ulong c_ChipperPenalty = 68719476736uL;

	private static ulong c_BycicleKick = 137438953472uL;

	private static ulong c_DivingHeader = 274877906944uL;

	private static ulong c_DrivenPass = 549755813888uL;

	private static ulong c_GkFlatKick = 1099511627776uL;

	private static ulong c_HighClubIdentification = 2199023255552uL;

	private static ulong c_TeamPlayer = 4398046511104uL;

	public static Color[] c_StandardColors = new Color[14]
	{
		Color.White,
		Color.Black,
		Color.Blue,
		Color.Red,
		Color.Yellow,
		Color.DarkGreen,
		Color.Orange,
		Color.Violet,
		Color.DarkRed,
		Color.Pink,
		Color.Brown,
		Color.LightSkyBlue,
		Color.Navy,
		Color.Gray
	};

	public static int[,] c_PlayerGrowth = new int[3, 13]
	{
		{
			13, 12, 11, 11, 11, 11, 9, 7, 5, 4,
			3, 2, 1
		},
		{
			20, 17, 15, 13, 11, 9, 7, 5, 2, 1,
			0, 0, 0
		},
		{
			10, 9, 9, 7, 5, 10, 12, 12, 12, 11,
			1, 1, 1
		}
	};

	public static int[] c_KeeperProfile = new int[32]
	{
		455, 322, 470, 479, 250, 144, 153, 154, 135, 160,
		768, 723, 699, 749, 788, 149, 232, 671, 291, 144,
		134, 147, 717, 297, 294, 142, 463, 347, 138, 640,
		402, 148
	};

	public static int[] c_SideBackProfile = new int[32]
	{
		774, 746, 733, 711, 723, 747, 649, 700, 468, 538,
		127, 127, 128, 129, 124, 638, 727, 718, 669, 596,
		740, 617, 735, 715, 690, 772, 787, 802, 760, 694,
		600, 482
	};

	public static int[] c_BackProfile = new int[32]
	{
		577, 767, 545, 542, 619, 466, 441, 498, 342, 421,
		135, 122, 132, 131, 126, 752, 755, 740, 611, 463,
		769, 386, 701, 651, 635, 755, 604, 655, 782, 802,
		467, 366
	};

	public static int[] c_DefMidfielderProfile = new int[32]
	{
		668, 786, 688, 666, 743, 632, 606, 689, 527, 585,
		188, 184, 192, 187, 207, 650, 760, 713, 739, 620,
		707, 592, 740, 762, 700, 717, 669, 797, 762, 745,
		701, 547
	};

	public static int[] c_SideMidfielderProfile = new int[32]
	{
		811, 577, 799, 763, 780, 737, 719, 793, 691, 686,
		113, 106, 105, 111, 102, 537, 408, 654, 666, 717,
		358, 727, 724, 741, 756, 352, 798, 718, 392, 606,
		715, 660
	};

	public static int[] c_MidfielderProfile = new int[32]
	{
		683, 735, 718, 727, 779, 720, 702, 733, 612, 670,
		197, 109, 107, 109, 107, 582, 684, 654, 767, 719,
		597, 696, 744, 792, 750, 630, 662, 761, 670, 678,
		764, 631
	};

	public static int[] c_AdvMidfielderProfile = new int[32]
	{
		744, 598, 780, 780, 789, 712, 732, 778, 675, 715,
		160, 160, 154, 165, 151, 530, 445, 595, 704, 720,
		385, 724, 732, 767, 730, 427, 716, 699, 442, 579,
		752, 675
	};

	public static int[] c_ForwardProfile = new int[32]
	{
		783, 540, 783, 730, 810, 700, 730, 800, 698, 713,
		100, 188, 103, 130, 195, 513, 298, 503, 703, 723,
		220, 755, 715, 763, 698, 250, 748, 675, 250, 490,
		745, 675
	};

	public static int[] c_WingProfile = new int[32]
	{
		827, 493, 816, 752, 781, 706, 720, 788, 691, 656,
		196, 105, 118, 195, 112, 538, 321, 600, 658, 717,
		286, 714, 730, 707, 750, 289, 812, 665, 328, 610,
		716, 683
	};

	public static int[] c_StrikerProfile = new int[32]
	{
		727, 618, 699, 638, 743, 591, 643, 726, 772, 583,
		127, 120, 126, 138, 124, 738, 324, 711, 556, 707,
		255, 773, 741, 686, 784, 250, 739, 703, 296, 754,
		656, 735
	};

	public static double[] c_HeightCorrectionFactor = new double[32]
	{
		1.01, 0.0, -1.25, -1.64, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.89, -1.0, 0.0, 1.09,
		0.0, 0.0
	};

	public static double c_BalanceHeightFactor = -1.64;

	public static double c_StrengthHeightFactor = 1.09;

	public static double c_AgilityHeightFactor = -1.25;

	public static double c_StaminaHeightFactor = -1.0;

	public static double c_SpeedHeightFactor = -0.89;

	public static double c_AccelerationHeightFactor = -1.01;

	public static void RandomizeOverall(Player player, int age)
	{
		int num = 0;
		FifaUtil.RandomizeGaussianDouble(70.0, 6.0, out var rand, 70.0, 6.0, out var _);
		rand += (double)num;
		double num2 = 75.0;
		int num3 = ((age < 30) ? age : 30);
		for (int i = 0; i < num3 - 17; i++)
		{
			num2 += 0.25 * (double)c_PlayerGrowth[0, i];
			_ = 100.0;
		}
		player.potential = (byte)Math.Round(rand);
		player.overallrating = (int)((double)player.potential * num2 / 100.0);
		if (player.overallrating > 75 && age > 22)
		{
			if (age <= 24)
			{
				int overallrating = (player.potential - 75) / 4 + 75;
				player.overallrating = overallrating;
			}
			else
			{
				int potential = (player.potential - 75) / 4 + 75;
				player.potential = potential;
				player.overallrating = (int)((double)player.potential * num2 / 100.0);
			}
		}
	}

	public static void RandomizeProfile(Player player, ERole preferredRole)
	{
		int overallrating = player.overallrating;
		double num = (double)player.overallrating / 75.0;
		int[] array = null;
		switch (preferredRole)
		{
		case ERole.Goalkeeper:
			array = c_KeeperProfile;
			break;
		case ERole.Right_Wing_Back:
		case ERole.Right_Back:
		case ERole.Left_Back:
		case ERole.Left_Wing_Back:
			array = c_SideBackProfile;
			break;
		case ERole.Central_Back:
			array = c_SideBackProfile;
			break;
		case ERole.Central_Defensive_Midfielder:
			array = c_DefMidfielderProfile;
			break;
		default:
			array = c_MidfielderProfile;
			break;
		case ERole.Central_Advanced_Midfielder:
			array = c_AdvMidfielderProfile;
			break;
		case ERole.Central_Forward:
			array = c_ForwardProfile;
			break;
		case ERole.Central_Striker:
			array = c_StrikerProfile;
			break;
		case ERole.Right_Midfielder:
		case ERole.Left_Midfielder:
			array = c_SideMidfielderProfile;
			break;
		case ERole.Right_Wing:
		case ERole.Left_Wing:
			array = c_WingProfile;
			break;
		}
		int num2 = (player.height - 180) * 10;
		if (num2 < -150)
		{
			num2 = -150;
		}
		if (num2 > 150)
		{
			num2 = 150;
		}
		int num3 = 0;
		int num4 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		int num5 = FifaUtil.RandomizeInteger(-50, 50);
		byte acceleration = (byte)((num4 + num5 + 5) / 10);
		player.acceleration = acceleration;
		int num6 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num6 + num5 + 5) / 10);
		player.aggression = acceleration;
		int num7 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num7 + num5 + 5) / 10);
		player.agility = acceleration;
		int num8 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num8 + num5 + 5) / 10);
		player.balance = acceleration;
		int num9 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num9 + num5 + 5) / 10);
		player.ballcontrol = acceleration;
		int num10 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num10 + num5 + 5) / 10);
		player.crossing = acceleration;
		int num11 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num11 + num5 + 5) / 10);
		player.curve = acceleration;
		int num12 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num12 + num5 + 5) / 10);
		player.dribbling = acceleration;
		int num13 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num13 + num5 + 5) / 10);
		player.finishing = acceleration;
		int num14 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num14 + num5 + 5) / 10);
		player.freekickaccuracy = acceleration;
		int num15 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num15 + num5 + 5) / 10);
		player.gkdiving = acceleration;
		int num16 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num16 + num5 + 5) / 10);
		player.gkhandling = acceleration;
		int num17 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num17 + num5 + 5) / 10);
		player.gkkicking = acceleration;
		int num18 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num18 + num5 + 5) / 10);
		player.gkpositioning = acceleration;
		int num19 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num19 + num5 + 5) / 10);
		player.gkreflexes = acceleration;
		int num20 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num20 + num5 + 5) / 10);
		player.headingaccuracy = acceleration;
		int num21 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num21 + num5 + 5) / 10);
		player.interceptions = acceleration;
		int num22 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num22 + num5 + 5) / 10);
		player.jumping = acceleration;
		int num23 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num23 + num5 + 5) / 10);
		player.longpassing = acceleration;
		int num24 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num24 + num5 + 5) / 10);
		player.longshots = acceleration;
		int num25 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num25 + num5 + 5) / 10);
		player.marking = acceleration;
		int num26 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num26 + num5 + 5) / 10);
		player.positioning = acceleration;
		int num27 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num27 + num5 + 5) / 10);
		player.reactions = acceleration;
		int num28 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num28 + num5 + 5) / 10);
		player.shortpassing = acceleration;
		int num29 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num29 + num5 + 5) / 10);
		player.shotpower = acceleration;
		int num30 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num30 + num5 + 5) / 10);
		player.slidingtackle = acceleration;
		int num31 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num31 + num5 + 5) / 10);
		player.sprintspeed = acceleration;
		int num32 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num32 + num5 + 5) / 10);
		player.stamina = acceleration;
		int num33 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num33 + num5 + 5) / 10);
		player.standingtackle = acceleration;
		int num34 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num34 + num5 + 5) / 10);
		player.strength = acceleration;
		int num35 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num35 + num5 + 5) / 10);
		player.vision = acceleration;
		int num36 = (int)((double)array[num3] * num + c_HeightCorrectionFactor[num3] * (double)num2);
		num3++;
		num5 = FifaUtil.RandomizeInteger(-50, 50);
		acceleration = (byte)((num36 + num5 + 5) / 10);
		player.volleys = acceleration;
		int num37 = (byte)player.overallrating;
		if (num37 != overallrating)
		{
			player.ChangeSkills(overallrating - num37);
		}
		num37 = (byte)player.overallrating;
		switch (preferredRole)
		{
		case ERole.Left_Back:
		case ERole.Left_Midfielder:
			player.preferredfoot = ((FifaUtil.Randomizer.Next(100) >= 20) ? 1 : 0);
			break;
		case ERole.Right_Back:
		case ERole.Right_Midfielder:
			player.preferredfoot = ((FifaUtil.Randomizer.Next(100) >= 80) ? 1 : 0);
			break;
		case ERole.Right_Wing:
		case ERole.Left_Wing:
			player.preferredfoot = ((FifaUtil.Randomizer.Next(100) >= 50) ? 1 : 0);
			break;
		default:
			player.preferredfoot = ((FifaUtil.Randomizer.Next(100) >= 88) ? 1 : 0);
			break;
		}
		num5 = FifaUtil.Randomizer.Next(100);
		if (num5 < 6)
		{
			player.weakfootabilitytypecode = 5;
		}
		else if (player.ballcontrol > 70)
		{
			player.weakfootabilitytypecode = ((num5 < 50) ? 3 : 4);
		}
		else if (player.ballcontrol > 60)
		{
			player.weakfootabilitytypecode = ((num5 < 30) ? 2 : ((num5 < 60) ? 3 : 4));
		}
		else
		{
			player.weakfootabilitytypecode = ((num5 < 30) ? 1 : ((num5 < 50) ? 2 : ((num5 < 80) ? 3 : 4)));
		}
		if (preferredRole == ERole.Goalkeeper)
		{
			player.gksavetype = ((FifaUtil.Randomizer.Next(100) >= 85) ? 1 : 0);
			player.gkkickstyle = FifaUtil.Randomizer.Next(4);
		}
		player.runningcode1 = (byte)FifaUtil.Randomizer.Next(128);
		player.runningcode1 = (byte)FifaUtil.Randomizer.Next(128);
		player.finishingcode1 = (byte)FifaUtil.Randomizer.Next(128);
		player.finishingcode2 = (byte)FifaUtil.Randomizer.Next(128);
		int num38 = player.overallrating - 5;
		player.defensiveworkrate = 0;
		if (player.aggression > num38)
		{
			player.defensiveworkrate = 2;
		}
		else if (player.dribbling > player.overallrating)
		{
			player.defensiveworkrate = 1;
		}
		player.attackingworkrate = 0;
		if (player.dribbling > num38)
		{
			player.attackingworkrate = 2;
		}
		else if (player.aggression > player.overallrating)
		{
			player.attackingworkrate = 1;
		}
		player.freekickaccuracy = (byte)((player.curve + player.longshots) / 2 + FifaUtil.RandomizeInteger(-5, 5));
		player.animfreekickstartposcode = FifaUtil.Randomizer.Next(10);
		player.animpenaltiesstartposcode = FifaUtil.Randomizer.Next(9);
		player.animpenaltiesmotionstylecode = FifaUtil.Randomizer.Next(7);
		player.animpenaltieskickstylecode = FifaUtil.Randomizer.Next(3);
		int num39 = (player.dribbling + player.ballcontrol) / 2;
		if (num39 >= 80)
		{
			player.skillmoves = ((FifaUtil.Randomizer.Next(100) > 80) ? 3 : 4);
		}
		else if (num39 >= 72)
		{
			player.skillmoves = ((FifaUtil.Randomizer.Next(100) > 80) ? 2 : 3);
		}
		else if (num39 >= 62)
		{
			player.skillmoves = ((FifaUtil.Randomizer.Next(100) > 70) ? 1 : 2);
		}
		else if (num39 >= 40)
		{
			player.skillmoves = ((FifaUtil.Randomizer.Next(100) <= 50) ? 1 : 0);
		}
		else
		{
			player.skillmoves = 0;
		}
	}

	public static void RandomizeTraits(Player player)
	{
		switch ((ERole)player.preferredposition1)
		{
		case ERole.Goalkeeper:
			RandomizeKeeperTraits(player);
			break;
		case ERole.Right_Back:
		case ERole.Left_Back:
			player.AvoidsWeakFoot = player.weakfootabilitytypecode == 1;
			player.Divesintotackles = player.aggression > player.overallrating + 3;
			player.Earlycrosser = player.crossing > player.overallrating + 3;
			player.AcrobaticClearance = player.balance > player.overallrating + 3;
			break;
		case ERole.Right_Midfielder:
		case ERole.Left_Midfielder:
			player.Earlycrosser = player.crossing > player.overallrating + 3;
			player.Flair = player.ballcontrol > player.overallrating + 5;
			player.SpeedDribbler = player.dribbling > player.overallrating + 5;
			player.FancyFeet = player.ballcontrol > player.overallrating + 5;
			player.FancyPasses = player.vision > player.overallrating + 5;
			player.FancyFlicks = player.vision > player.overallrating + 5;
			player.StutterPenalty = player.penalties > player.overallrating + 5;
			player.ChipperPenalty |= player.penalties > player.overallrating + 10;
			break;
		case ERole.Right_Wing:
		case ERole.Left_Wing:
			player.Diver = (player.acceleration + player.dribbling) / 2 >= player.overallrating + 3;
			player.BeatDefensiveLine = player.sprintspeed > player.overallrating + 5;
			player.Earlycrosser |= player.crossing > player.overallrating + 3;
			if (player.ballcontrol >= 75)
			{
				player.Flair = player.ballcontrol >= player.overallrating + 5;
				player.Flair |= player.ballcontrol + player.dribbling >= player.overallrating + 5;
				player.FancyFlicks = (player.balance + player.ballcontrol) / 2 >= player.overallrating + 5;
				player.FancyPasses = (player.vision + player.ballcontrol) / 2 >= player.overallrating + 5;
			}
			if (player.finishing >= 75)
			{
				player.FinesseShot = (player.curve + player.finishing) / 2 > player.overallrating + 3;
			}
			player.SpeedDribbler = (player.dribbling + player.sprintspeed) / 2 >= player.overallrating + 5;
			player.StutterPenalty = player.penalties > player.overallrating + 5;
			player.ChipperPenalty = player.penalties > player.overallrating + 10;
			break;
		case ERole.Central_Back:
			player.Divesintotackles = player.aggression > player.overallrating + 5;
			player.LongPasser = player.longpassing >= player.overallrating + 5;
			player.DrivenPass = (player.longpassing + player.vision) / 2 >= player.overallrating + 3;
			player.AcrobaticClearance |= player.balance >= player.overallrating + 3;
			player.PowerHeader = player.height >= 183 && player.jumping >= player.overallrating + 3;
			player.DivingHeader = player.height >= 180 && player.headingaccuracy >= player.overallrating + 3;
			break;
		case ERole.Central_Defensive_Midfielder:
			player.Divesintotackles = player.aggression > player.overallrating + 5;
			player.LongPasser = player.longpassing >= player.overallrating + 5;
			player.DrivenPass = (player.longpassing + player.vision) / 2 >= player.overallrating + 3;
			player.LongShotTaker = player.longshots >= player.overallrating + 3;
			break;
		case ERole.Central_Midfielder:
			player.Playmaker = player.shortpassing >= player.overallrating + 5;
			player.LongPasser = player.longpassing >= player.overallrating + 5;
			player.DrivenPass = (player.longpassing + player.vision) / 2 >= player.overallrating + 5;
			player.AvoidsWeakFoot = player.weakfootabilitytypecode == 1;
			player.SwervePasser = (player.shortpassing + player.curve) / 2 >= player.overallrating + 5;
			if (player.finishing >= 75)
			{
				player.FinesseShot = (player.curve + player.finishing) / 2 > player.overallrating + 3;
				player.OutsideFootShot = (player.finishing + player.ballcontrol) / 2 >= player.overallrating + 5;
			}
			if (player.ballcontrol >= 75)
			{
				player.Flair = player.ballcontrol >= player.overallrating + 5;
				player.FancyFeet = player.ballcontrol + player.dribbling >= player.overallrating + 5;
				player.FancyFlicks = (player.balance + player.ballcontrol) / 2 >= player.overallrating + 5;
				player.FancyPasses = (player.vision + player.ballcontrol) / 2 >= player.overallrating + 5;
			}
			player.SpeedDribbler = (player.dribbling + player.sprintspeed) / 2 >= player.overallrating + 5;
			player.LongShotTaker = player.longshots >= player.overallrating + 3;
			player.StutterPenalty = player.penalties > player.overallrating + 5;
			player.ChipperPenalty = player.penalties > player.overallrating + 10;
			break;
		case ERole.Central_Advanced_Midfielder:
		case ERole.Central_Forward:
			player.AvoidsWeakFoot = player.weakfootabilitytypecode == 1;
			player.SwervePasser = (player.shortpassing + player.curve) / 2 >= player.overallrating + 5;
			if (player.finishing >= 75)
			{
				player.FinesseShot = (player.curve + player.finishing) / 2 > player.overallrating + 3;
				player.OutsideFootShot = (player.finishing + player.ballcontrol) / 2 >= player.overallrating + 5;
			}
			if (player.ballcontrol >= 75)
			{
				player.Flair = player.ballcontrol >= player.overallrating + 5;
				player.FancyFeet = player.ballcontrol + player.dribbling >= player.overallrating + 5;
				player.FancyFlicks = (player.balance + player.ballcontrol) / 2 >= player.overallrating + 5;
				player.FancyPasses = (player.vision + player.ballcontrol) / 2 >= player.overallrating + 5;
			}
			player.SpeedDribbler = (player.dribbling + player.sprintspeed) / 2 >= player.overallrating + 5;
			player.LongShotTaker = player.longshots >= player.overallrating + 3;
			player.StutterPenalty = player.penalties > player.overallrating + 5;
			player.ChipperPenalty = player.penalties > player.overallrating + 10;
			break;
		case ERole.Central_Striker:
			player.Diver = (player.acceleration + player.dribbling) / 2 >= player.overallrating + 3;
			player.BeatDefensiveLine = player.sprintspeed > player.overallrating + 5;
			player.Selfish = (player.finishing + player.dribbling) / 2 >= player.overallrating + 5;
			if (player.finishing >= 75)
			{
				player.FinesseShot = (player.curve + player.finishing) / 2 > player.overallrating + 3;
				player.OutsideFootShot = (player.finishing + player.ballcontrol) / 2 >= player.overallrating + 3;
				player.BycicleKick = (player.finishing + player.balance) / 2 >= player.overallrating + 3;
			}
			if (player.ballcontrol >= 75)
			{
				player.Flair = player.ballcontrol >= player.overallrating + 5;
				player.FancyFeet = player.ballcontrol + player.dribbling >= player.overallrating + 5;
				player.FancyFlicks = (player.balance + player.ballcontrol) / 2 >= player.overallrating + 5;
				player.FancyPasses = (player.vision + player.ballcontrol) / 2 >= player.overallrating + 5;
			}
			player.LongShotTaker = player.longshots >= player.overallrating + 3;
			player.StutterPenalty = player.penalties > player.overallrating + 5;
			player.ChipperPenalty = player.penalties > player.overallrating + 10;
			player.SpeedDribbler = (player.dribbling + player.sprintspeed) / 2 >= player.overallrating + 5;
			player.PowerHeader = player.height >= 183 && player.jumping >= player.overallrating + 3;
			player.DivingHeader = player.height >= 180 && player.headingaccuracy >= player.overallrating + 3;
			break;
		}
		int num = FifaUtil.Randomizer.Next(100);
		if (num < 3)
		{
			player.GiantThrow = true;
		}
		else if (num < 6)
		{
			player.Longthrows = true;
		}
		player.SecondWind = player.stamina >= player.overallrating + 5;
	}

	private static void RandomizeKeeperTraits(Player player)
	{
		player.Pushesupforcorners = player.height >= 188;
		player.Puncher = player.height < 180;
		player.Puncher |= player.gkhandling < player.overallrating - 3;
		player.GkLongThrower = player.strength > player.overallrating + 3 && player.gkhandling > player.overallrating + 3;
		player.GkOneOnOne |= player.sprintspeed > player.overallrating - 3 && player.gkpositioning > player.overallrating + 3;
		player.GkFlatKick = player.gkkicking > player.overallrating - 3;
	}
}
