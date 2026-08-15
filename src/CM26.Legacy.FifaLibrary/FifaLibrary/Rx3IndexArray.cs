using System.IO;

namespace FifaLibrary;

public class Rx3IndexArray
{
	public enum ETriangleListType
	{
		InvertOdd,
		InvertEven
	}

	private static ETriangleListType s_TriangleListType;

	private int m_Size;

	private int m_nIndex;

	private int m_SizeOfIndex;

	private int m_Unknown;

	private int m_nFaces;

	private ushort[] m_IndexStream;

	private bool m_IsTriangleList;

	private bool m_SwapEndian;

	public static ETriangleListType TriangleListType
	{
		get
		{
			return s_TriangleListType;
		}
		set
		{
			s_TriangleListType = value;
		}
	}

	public int NIndex => m_nIndex;

	public int nFaces => m_nFaces;

	public ushort[] IndexStream => m_IndexStream;

	public bool IsTriangleList => m_IsTriangleList;

	public Rx3IndexArray(BinaryReader r, bool swapEndian)
	{
		m_SwapEndian = swapEndian;
		Load(r);
	}

	public bool Load(BinaryReader r)
	{
		if (m_SwapEndian)
		{
			m_Size = FifaUtil.SwapEndian(r.ReadInt32());
			m_nIndex = FifaUtil.SwapEndian(r.ReadInt32());
			m_SizeOfIndex = r.ReadInt32();
			m_Unknown = r.ReadInt32();
			m_IndexStream = new ushort[m_nIndex];
			for (int i = 0; i < m_nIndex; i++)
			{
				m_IndexStream[i] = FifaUtil.SwapEndian(r.ReadUInt16());
			}
		}
		else
		{
			m_Size = r.ReadInt32();
			m_nIndex = r.ReadInt32();
			m_SizeOfIndex = r.ReadInt32();
			m_Unknown = r.ReadInt32();
			m_IndexStream = new ushort[m_nIndex];
			for (int j = 0; j < m_nIndex; j++)
			{
				m_IndexStream[j] = r.ReadUInt16();
			}
		}
		m_IsTriangleList = false;
		m_nFaces = m_nIndex / 3;
		if (m_nFaces * 3 != m_nIndex)
		{
			m_IsTriangleList = true;
		}
		else
		{
			for (int k = 0; k < nFaces; k++)
			{
				ushort num = m_IndexStream[k * 3];
				ushort num2 = m_IndexStream[k * 3 + 1];
				ushort num3 = m_IndexStream[k * 3 + 2];
				if (num == num2 || num2 == num3 || num == num3)
				{
					m_IsTriangleList = true;
					break;
				}
			}
		}
		if (m_IsTriangleList)
		{
			m_nFaces = 0;
			for (int l = 0; l < m_nIndex - 2; l++)
			{
				ushort num4 = m_IndexStream[l];
				ushort num5 = m_IndexStream[l + 1];
				ushort num6 = m_IndexStream[l + 2];
				if (num4 != num5 && num5 != num6 && num4 != num6)
				{
					m_nFaces++;
				}
			}
		}
		return true;
	}

	public bool Save(BinaryWriter w)
	{
		if (m_SwapEndian)
		{
			w.Write(FifaUtil.SwapEndian(m_Size));
			w.Write(FifaUtil.SwapEndian(m_nIndex));
			w.Write(m_SizeOfIndex);
			w.Write(m_Unknown);
			for (int i = 0; i < m_nIndex; i++)
			{
				w.Write(FifaUtil.SwapEndian(m_IndexStream[i]));
			}
		}
		else
		{
			w.Write(m_Size);
			w.Write(m_nIndex);
			w.Write(m_SizeOfIndex);
			w.Write(m_Unknown);
			for (int j = 0; j < m_nIndex; j++)
			{
				w.Write(m_IndexStream[j]);
			}
		}
		return true;
	}

	public bool SetIndexStream(ushort[] indexStream)
	{
		if (m_IndexStream.Length != indexStream.Length)
		{
			return false;
		}
		for (int i = 0; i < m_IndexStream.Length; i++)
		{
			m_IndexStream[i] = indexStream[i];
		}
		return true;
	}
}
