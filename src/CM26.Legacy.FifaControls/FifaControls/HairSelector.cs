using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class HairSelector : Form
{
	public enum ESelectionType
	{
		Hair,
		Face
	}

	private int[] m_FaceLut = new int[189]
	{
		1, 2, 17, 6, 12, 24, 25, 0, 3, 5,
		7, 8, 10, 11, 13, 14, 15, 18, 20, 22,
		9, 16, 19, 23, 4, 21, 522, 506, 517, 514,
		504, 509, 500, 519, 532, 521, 523, 508, 503, 505,
		513, 510, 516, 502, 507, 518, 511, 524, 525, 527,
		528, 530, 531, 512, 520, 529, 501, 515, 1019, 526,
		1017, 1007, 1022, 1026, 1018, 1006, 1021, 1024, 1025, 1005,
		1000, 1011, 1001, 1002, 1010, 1015, 1003, 1016, 1009, 1027,
		1013, 1014, 1004, 1023, 1012, 1020, 1008, 1526, 1527, 1500,
		1525, 1513, 1508, 1503, 1523, 1528, 1519, 1502, 1505, 1512,
		1516, 1521, 1524, 1507, 1504, 1501, 1510, 1511, 1509, 1506,
		1514, 1515, 1517, 1518, 1522, 1520, 2016, 2015, 2007, 2013,
		2017, 2002, 2005, 2006, 2008, 2011, 2014, 2000, 2004, 2010,
		2001, 2003, 2009, 2012, 2019, 2020, 2021, 2022, 2023, 2024,
		2025, 2026, 2027, 2028, 2029, 2030, 2500, 2501, 2502, 2503,
		2504, 2505, 2506, 2507, 2508, 2509, 2510, 2511, 2512, 2513,
		2514, 2515, 2516, 2517, 2518, 3000, 3001, 3002, 3003, 3004,
		3005, 3500, 3501, 3502, 3503, 3504, 3505, 4000, 4001, 4002,
		4003, 4500, 4501, 4502, 4525, 5000, 5001, 5002, 5003
	};

	private int[] m_HairLut = new int[194]
	{
		0, 41, 25, 26, 46, 43, 29, 120, 72, 92,
		47, 114, 150, 1, 117, 28, 86, 16, 113, 45,
		65, 60, 132, 63, 88, 123, 133, 2, 82, 21,
		105, 77, 40, 112, 31, 122, 17, 144, 138, 115,
		100, 89, 19, 102, 129, 147, 141, 106, 54, 541,
		36, 30, 8, 93, 57, 66, 32, 131, 124, 149,
		18, 119, 42, 139, 78, 151, 140, 529, 521, 516,
		514, 111, 118, 67, 14, 68, 37, 90, 135, 137,
		39, 146, 61, 101, 148, 64, 70, 134, 15, 24,
		94, 121, 75, 38, 107, 127, 116, 58, 104, 145,
		108, 143, 83, 142, 98, 23, 522, 95, 502, 136,
		20, 62, 125, 85, 69, 35, 13, 59, 34, 10,
		520, 527, 74, 73, 103, 99, 12, 513, 55, 9,
		22, 87, 52, 79, 51, 44, 56, 523, 84, 80,
		91, 81, 76, 49, 33, 11, 509, 126, 128, 130,
		97, 27, 96, 3, 71, 4, 110, 109, 5, 6,
		53, 7, 48, 504, 515, 524, 533, 534, 535, 500,
		501, 503, 505, 506, 507, 508, 510, 511, 512, 517,
		518, 519, 525, 526, 528, 530, 531, 532, 536, 537,
		538, 539, 540, 50
	};

	private PictureBox[] m_PicturBox;

	private int[] m_Lut;

	private ImageList imageListMain;

	private ImageList imageListAux;

	private int m_SelctedKey;

	private IContainer components;

	private Label labelCurrentMain;

	private ImageList imageListHairSide;

	private ImageList imageListHairFront;

	private Label labelCurrentAux;

	private Label labelSelectedAux;

	private Label labelSelectedMain;

	private Button buttonOk;

	private Button buttonCancel;

	private ImageList imageListFace;

	private Label labelPlayerPicture;

	public int SelectedKey
	{
		get
		{
			return m_SelctedKey;
		}
		set
		{
			m_SelctedKey = value;
			labelSelectedMain.ImageIndex = FindIndex(m_SelctedKey);
			labelSelectedAux.ImageIndex = labelSelectedMain.ImageIndex;
		}
	}

	public void SetPlayerPicture(Bitmap playerPicture)
	{
		labelPlayerPicture.Image = playerPicture;
	}

	private int FindIndex(int key)
	{
		for (int i = 0; i < m_Lut.Length; i++)
		{
			if (m_Lut[i] == key)
			{
				return i;
			}
		}
		return 0;
	}

	public HairSelector(ESelectionType selectedType)
	{
		InitializeComponent();
		int num = 0;
		int num2 = 72;
		int num3 = 72;
		Padding padding = new Padding(0, 0, 0, 0);
		if (m_PicturBox == null)
		{
			if (selectedType == ESelectionType.Hair)
			{
				m_Lut = m_HairLut;
				m_PicturBox = new PictureBox[m_Lut.Length];
				imageListMain = imageListHairSide;
				imageListAux = imageListHairFront;
				for (int i = 0; i < m_PicturBox.Length; i++)
				{
					string text = m_Lut[i].ToString("d3");
					string sidePath = Environment.CurrentDirectory + "\\Templates\\HairSelector\\" + text + "b.png";
					Bitmap image = System.IO.File.Exists(sidePath) ? new Bitmap(sidePath) : new Bitmap(72, 72);
					imageListMain.Images.Add(text, image);
					string frontPath = Environment.CurrentDirectory + "\\Templates\\HairSelector\\" + text + "a.png";
					image = System.IO.File.Exists(frontPath) ? new Bitmap(frontPath) : new Bitmap(72, 72);
					imageListAux.Images.Add(text, image);
				}
			}
			else
			{
				m_Lut = m_FaceLut;
				m_PicturBox = new PictureBox[m_FaceLut.Length];
				imageListMain = imageListFace;
				for (int j = 0; j < m_PicturBox.Length; j++)
				{
					string text2 = m_FaceLut[j].ToString();
					string facePath = Environment.CurrentDirectory + "\\Templates\\FaceSelector\\" + text2 + ".png";
					Bitmap image2 = System.IO.File.Exists(facePath) ? new Bitmap(facePath) : new Bitmap(72, 72);
					imageListMain.Images.Add(text2, image2);
				}
			}
		}
		labelCurrentMain.ImageList = imageListMain;
		labelCurrentAux.ImageList = imageListAux;
		labelSelectedMain.ImageList = imageListMain;
		labelSelectedAux.ImageList = imageListAux;
		for (num = 0; num < m_PicturBox.Length; num++)
		{
			int num4 = num / 16;
			int num5 = num % 16;
			m_PicturBox[num] = new PictureBox();
			m_PicturBox[num].Location = new Point(num5 * num2, num4 * num3);
			m_PicturBox[num].Name = "pictureBox" + num;
			m_PicturBox[num].Text = "pictureBox" + num;
			m_PicturBox[num].Size = new Size(num2, num3);
			m_PicturBox[num].Dock = DockStyle.None;
			m_PicturBox[num].BorderStyle = BorderStyle.FixedSingle;
			m_PicturBox[num].Padding = padding;
			m_PicturBox[num].Cursor = Cursors.Default;
			if (num < imageListMain.Images.Count)
			{
				m_PicturBox[num].BackgroundImage = imageListMain.Images[num];
				m_PicturBox[num].BackgroundImageLayout = ImageLayout.Zoom;
			}
			m_PicturBox[num].MouseEnter += pictureBox_MouseEnter;
			m_PicturBox[num].Click += pictureBox_Click;
			base.Controls.Add(m_PicturBox[num]);
		}
	}

	private void pictureBox_MouseEnter(object sender, EventArgs e)
	{
		PictureBox pictureBox = (PictureBox)sender;
		int imageIndex = Convert.ToInt32(pictureBox.Name.Substring(10, pictureBox.Name.Length - 10));
		if (imageListAux != null)
		{
			labelCurrentAux.ImageIndex = imageIndex;
		}
		labelCurrentMain.ImageIndex = imageIndex;
	}

	private void pictureBox_Click(object sender, EventArgs e)
	{
		PictureBox pictureBox = (PictureBox)sender;
		int num = Convert.ToInt32(pictureBox.Name.Substring(10, pictureBox.Name.Length - 10));
		if (imageListAux != null)
		{
			labelSelectedAux.ImageIndex = num;
		}
		labelSelectedMain.ImageIndex = num;
		m_SelctedKey = m_Lut[num];
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.labelCurrentMain = new System.Windows.Forms.Label();
		this.imageListHairSide = new System.Windows.Forms.ImageList(this.components);
		this.imageListHairFront = new System.Windows.Forms.ImageList(this.components);
		this.labelCurrentAux = new System.Windows.Forms.Label();
		this.labelSelectedAux = new System.Windows.Forms.Label();
		this.labelSelectedMain = new System.Windows.Forms.Label();
		this.buttonOk = new System.Windows.Forms.Button();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.imageListFace = new System.Windows.Forms.ImageList(this.components);
		this.labelPlayerPicture = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.labelCurrentMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelCurrentMain.Location = new System.Drawing.Point(1234, 184);
		this.labelCurrentMain.Name = "labelCurrentMain";
		this.labelCurrentMain.Size = new System.Drawing.Size(181, 181);
		this.labelCurrentMain.TabIndex = 0;
		this.imageListHairSide.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
		this.imageListHairSide.ImageSize = new System.Drawing.Size(216, 181);
		this.imageListHairSide.TransparentColor = System.Drawing.Color.Transparent;
		this.imageListHairFront.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
		this.imageListHairFront.ImageSize = new System.Drawing.Size(216, 181);
		this.imageListHairFront.TransparentColor = System.Drawing.Color.Transparent;
		this.labelCurrentAux.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelCurrentAux.Location = new System.Drawing.Point(1234, 3);
		this.labelCurrentAux.Name = "labelCurrentAux";
		this.labelCurrentAux.Size = new System.Drawing.Size(181, 181);
		this.labelCurrentAux.TabIndex = 4;
		this.labelSelectedAux.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelSelectedAux.Location = new System.Drawing.Point(1234, 365);
		this.labelSelectedAux.Name = "labelSelectedAux";
		this.labelSelectedAux.Size = new System.Drawing.Size(181, 181);
		this.labelSelectedAux.TabIndex = 6;
		this.labelSelectedMain.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelSelectedMain.Location = new System.Drawing.Point(1234, 546);
		this.labelSelectedMain.Name = "labelSelectedMain";
		this.labelSelectedMain.Size = new System.Drawing.Size(181, 181);
		this.labelSelectedMain.TabIndex = 5;
		this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonOk.Location = new System.Drawing.Point(1252, 730);
		this.buttonOk.Name = "buttonOk";
		this.buttonOk.Size = new System.Drawing.Size(60, 23);
		this.buttonOk.TabIndex = 7;
		this.buttonOk.Text = "OK";
		this.buttonOk.UseVisualStyleBackColor = true;
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(1338, 730);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(60, 23);
		this.buttonCancel.TabIndex = 8;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		this.imageListFace.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
		this.imageListFace.ImageSize = new System.Drawing.Size(216, 181);
		this.imageListFace.TransparentColor = System.Drawing.Color.Transparent;
		this.labelPlayerPicture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.labelPlayerPicture.Location = new System.Drawing.Point(1234, 756);
		this.labelPlayerPicture.Name = "labelPlayerPicture";
		this.labelPlayerPicture.Size = new System.Drawing.Size(181, 181);
		this.labelPlayerPicture.TabIndex = 9;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1419, 941);
		base.Controls.Add(this.labelPlayerPicture);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonOk);
		base.Controls.Add(this.labelSelectedAux);
		base.Controls.Add(this.labelSelectedMain);
		base.Controls.Add(this.labelCurrentAux);
		base.Controls.Add(this.labelCurrentMain);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "HairSelector";
		this.Text = "Generic Hair Selector";
		base.ResumeLayout(false);
	}
}
