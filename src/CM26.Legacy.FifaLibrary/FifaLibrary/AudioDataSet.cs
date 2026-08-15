using System.Data;
using System.IO;

namespace FifaLibrary;

public class AudioDataSet : DataSet
{
	private string m_XmlFileName;

	public string XmlFileName => m_XmlFileName;

	public AudioDataSet(string xmlFileName)
	{
		m_XmlFileName = xmlFileName;
		Load();
	}

	private bool Load()
	{
		if (!File.Exists(m_XmlFileName))
		{
			return false;
		}
		FileStream fileStream = new FileStream(m_XmlFileName, FileMode.Open, FileAccess.Read);
		ReadXml(fileStream);
		fileStream.Close();
		return true;
	}
}
