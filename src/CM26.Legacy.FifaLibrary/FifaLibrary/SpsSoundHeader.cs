using System.IO;

namespace FifaLibrary;

public class SpsSoundHeader
{
	private ushort m_Sig;

	private ushort m_Size;

	private uint m_Version;

	private EACodecId m_CodecId;

	private uint m_ChannelConfig;

	private uint m_SampleRate;

	private uint m_Type;

	private uint m_LoopFlag;

	private uint m_nSamples;

	private byte[] m_Fixed;

	public ushort Size => m_Size;

	public EACodecId CodecId
	{
		get
		{
			return m_CodecId;
		}
		set
		{
			m_CodecId = value;
		}
	}

	public uint nSamples
	{
		get
		{
			return m_nSamples;
		}
		set
		{
			m_nSamples = value;
		}
	}

	public SpsSoundHeader()
	{
		m_Sig = 18432;
		m_Size = 12;
		m_Version = 1u;
		m_CodecId = EACodecId.CodecNone;
		m_ChannelConfig = 0u;
		m_SampleRate = 0u;
		m_Type = 1u;
		m_LoopFlag = 0u;
		m_nSamples = 0u;
	}

	public bool Load(BinaryReader br)
	{
		m_Sig = FifaUtil.SwapEndian(br.ReadUInt16());
		if (m_Sig != 18432)
		{
			return false;
		}
		m_Size = FifaUtil.SwapEndian(br.ReadUInt16());
		if (m_Size != 12 && m_Size != 20)
		{
			return false;
		}
		byte b = br.ReadByte();
		m_Version = (uint)((b >> 4) & 0xF);
		m_CodecId = (EACodecId)(b & 0xF);
		b = br.ReadByte();
		m_ChannelConfig = (uint)((b >> 2) & 0x3C);
		uint num = (uint)((b & 3) << 16);
		m_SampleRate = FifaUtil.SwapEndian(br.ReadUInt16());
		m_SampleRate += num;
		uint num2 = FifaUtil.SwapEndian(br.ReadUInt32());
		m_nSamples = num2 & 0x1FFFFFFF;
		m_Type = (num2 >> 30) & 3;
		m_LoopFlag = (num2 >> 29) & 1;
		if (m_Size == 20)
		{
			m_Fixed = br.ReadBytes(8);
		}
		return true;
	}

	public static bool CheckSoundHeader(BinaryReader br, ref int logicalOffset)
	{
		if (br.BaseStream.Length < br.BaseStream.Position + 12)
		{
			return false;
		}
		ushort num = FifaUtil.SwapEndian(br.ReadUInt16());
		ushort num2 = FifaUtil.SwapEndian(br.ReadUInt16());
		br.BaseStream.Position -= 4L;
		if (num != 18432 || (num2 != 12 && num2 != 20))
		{
			return false;
		}
		br.BaseStream.Position += num2;
		logicalOffset += num2;
		return true;
	}

	public bool Save(BinaryWriter bw)
	{
		bw.Write(FifaUtil.SwapEndian(m_Sig));
		bw.Write(FifaUtil.SwapEndian(m_Size));
		int codecId = (int)m_CodecId;
		uint x = ((uint)m_Version << 28) | ((uint)codecId << 24) |
			((uint)m_ChannelConfig << 18) | (uint)m_SampleRate;
		bw.Write(FifaUtil.SwapEndian(x));
		x = ((uint)m_Type << 30) | ((uint)m_LoopFlag << 29) | (uint)m_nSamples;
		bw.Write(FifaUtil.SwapEndian(x));
		if (m_Size == 20)
		{
			bw.Write(m_Fixed);
		}
		return true;
	}

	public void ForceSize12()
	{
		m_Size = 12;
	}
}
