using System;
using System.Drawing;

namespace FifaLibrary;

public class Role : IdObject
{
	private int m_Xmin;

	private int m_Xmax;

	private int m_Ymin;

	private int m_Ymax;

	public ERole RoleId
	{
		get
		{
			return (ERole)base.Id;
		}
		set
		{
			base.Id = (int)value;
		}
	}

	public int Xmin
	{
		get
		{
			return m_Xmin;
		}
		set
		{
			m_Xmin = value;
		}
	}

	public int Xmax
	{
		get
		{
			return m_Xmax;
		}
		set
		{
			m_Xmax = value;
		}
	}

	public int Ymin
	{
		get
		{
			return m_Ymin;
		}
		set
		{
			m_Ymin = value;
		}
	}

	public int Ymax
	{
		get
		{
			return m_Ymax;
		}
		set
		{
			m_Ymax = value;
		}
	}

	public Role(ERole eRole)
		: base((int)eRole)
	{
	}

	public Role(int roleId)
		: base(roleId)
	{
	}

	public override string ToString()
	{
		string text = string.Empty;
		if (FifaEnvironment.Language != null)
		{
			text = FifaEnvironment.Language.GetRoleShortString(base.Id) + " - ";
		}
		return RoleId switch
		{
			ERole.Goalkeeper => text + "Goalkeeper", 
			ERole.Sweeper => text + "Sweeper", 
			ERole.Right_Wing_Back => text + "Right Wing Back", 
			ERole.Right_Back => text + "Right Back", 
			ERole.Right_Central_Back => text + "Right Central Back", 
			ERole.Central_Back => text + "Central Back", 
			ERole.Left_Central_Back => text + "Left Central Back", 
			ERole.Left_Back => text + "Left Back", 
			ERole.Left_Wing_Back => text + "Left Wing Back", 
			ERole.Right_Defensive_Midfielder => text + "Right Defensive Midfielder", 
			ERole.Central_Defensive_Midfielder => text + "Central Defensive Midfielder", 
			ERole.Left_Defensive_Midfielder => text + "Left Defensive Midfielder", 
			ERole.Right_Midfielder => text + "Right Midfielder", 
			ERole.Right_Central_Midfielder => text + "Right Central Midfielder", 
			ERole.Central_Midfielder => text + "Central Midfielder", 
			ERole.Left_Central_Midfielder => text + "Left Central Midfielder", 
			ERole.Left_Midfielder => text + "Left Midfielder", 
			ERole.Right_Advanced_Midfielder => text + "Right Advanced Midfielder", 
			ERole.Central_Advanced_Midfielder => text + "Central Advanced Midfielder", 
			ERole.Left_Advanced_Midfielder => text + "Left Advanced Midfielder", 
			ERole.Right_Forward => text + "Right Forward", 
			ERole.Central_Forward => text + "Central Forward", 
			ERole.Left_Forward => text + "Left Forward", 
			ERole.Right_Wing => text + "Right Wing", 
			ERole.Right_Striker => text + "Right Striker", 
			ERole.Central_Striker => text + "Central Striker", 
			ERole.Left_Striker => text + "Left Striker", 
			ERole.Left_Wing => text + "Left Wing", 
			ERole.Substitute => text + "Substitute", 
			ERole.Tribune => text + "Tribune", 
			_ => string.Empty, 
		};
	}

	public string ToShortString()
	{
		if (FifaEnvironment.Language != null)
		{
			return FifaEnvironment.Language.GetRoleShortString(base.Id);
		}
		return string.Empty;
	}

	public string ToLongString()
	{
		if (FifaEnvironment.Language != null)
		{
			return FifaEnvironment.Language.GetRoleLongString(base.Id);
		}
		return string.Empty;
	}

	public void SetShortString(string shortName)
	{
		if (FifaEnvironment.Language != null)
		{
			FifaEnvironment.Language.SetRoleShortString(base.Id, shortName);
		}
	}

	public Role(Record r)
		: base(r.IntField[r.TableDescriptor.GetFieldIndex("positionid")])
	{
		Load(r);
	}

	public void Load(Record r)
	{
		float val = r.FloatField[FI.fieldpositionboundingboxes_pointx0];
		float val2 = r.FloatField[FI.fieldpositionboundingboxes_pointx1];
		float val3 = r.FloatField[FI.fieldpositionboundingboxes_pointx2];
		float val4 = r.FloatField[FI.fieldpositionboundingboxes_pointx3];
		float val5 = r.FloatField[FI.fieldpositionboundingboxes_pointy0];
		float val6 = r.FloatField[FI.fieldpositionboundingboxes_pointy1];
		float val7 = r.FloatField[FI.fieldpositionboundingboxes_pointy2];
		float val8 = r.FloatField[FI.fieldpositionboundingboxes_pointy3];
		m_Xmin = Convert.ToInt32(Math.Min(Math.Min(val, val2), Math.Min(val3, val4)) * 100f);
		m_Xmax = Convert.ToInt32(Math.Max(Math.Max(val, val2), Math.Max(val3, val4)) * 100f);
		m_Ymin = Convert.ToInt32(Math.Min(Math.Min(val5, val6), Math.Min(val7, val8)) * 100f);
		m_Ymax = Convert.ToInt32(Math.Max(Math.Max(val5, val6), Math.Max(val7, val8)) * 100f);
	}

	public void Save(Record r)
	{
		r.IntField[FI.fieldpositionboundingboxes_positionid] = base.Id;
		r.FloatField[FI.fieldpositionboundingboxes_pointx0] = (float)m_Xmin / 100f;
		r.FloatField[FI.fieldpositionboundingboxes_pointy0] = (float)m_Ymin / 100f;
		r.FloatField[FI.fieldpositionboundingboxes_pointx1] = (float)m_Xmin / 100f;
		r.FloatField[FI.fieldpositionboundingboxes_pointy1] = (float)m_Ymax / 100f;
		r.FloatField[FI.fieldpositionboundingboxes_pointx2] = (float)m_Xmax / 100f;
		r.FloatField[FI.fieldpositionboundingboxes_pointy2] = (float)m_Ymax / 100f;
		r.FloatField[FI.fieldpositionboundingboxes_pointx3] = (float)m_Xmax / 100f;
		r.FloatField[FI.fieldpositionboundingboxes_pointy3] = (float)m_Ymin / 100f;
	}

	public Point GetCenter()
	{
		return new Point((m_Xmax + m_Xmin) / 2, (m_Ymax + m_Ymin) / 2);
	}

	public static ERole ConvertToERole(string s)
	{
		for (int i = 0; i < 29; i++)
		{
			ERole result = (ERole)i;
			if (result.ToString() == s)
			{
				return result;
			}
		}
		return ERole.Tribune;
	}
}
