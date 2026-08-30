using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary;

public class UserMessage : Form
{
	private int m_CurrentIndex = -1;

	private string m_XmlFileName;

	private IContainer components;

	private Button buttonOK;

	private TextBox textMessage;

	private CheckBox checkSuppressMessage;

	private TextBox textErrorNumber;

	private Messages setMessages;

	private Button buttonCancel;

	private Button buttonNo;

	private Button buttonYes;

	private PictureBox pictureBox;

	private ImageList imageList;

	public UserMessage()
	{
		InitializeComponent();
		string currentDirectory = Environment.CurrentDirectory;
		string text = CultureInfo.CurrentUICulture.Name.Substring(0, 2);
		string text2 = currentDirectory + "\\" + text;
		m_XmlFileName = null;
		if (Directory.Exists(text2))
		{
			string text3 = text2 + "\\Messages.xml";
			if (File.Exists(text3))
			{
				m_XmlFileName = text3;
			}
		}
		if (m_XmlFileName == null)
		{
			m_XmlFileName = currentDirectory + "\\Messages.xml";
		}
		if (File.Exists(m_XmlFileName))
		{
			setMessages.ReadXml(m_XmlFileName);
		}
	}

	public DialogResult ShowMessage(int id)
	{
		string text = null;
		bool flag = true;
		m_CurrentIndex = -1;
		for (int i = 0; i < setMessages.DataTableMex.Count; i++)
		{
			if (id == setMessages.DataTableMex[i].MexId)
			{
				text = setMessages.DataTableMex[i].MexText;
				flag = !setMessages.DataTableMex[i].MexSuppressed;
				m_CurrentIndex = i;
				break;
			}
		}
		textMessage.Text = text;
		if (flag)
		{
			SetUpLook(id);
			return ShowDialog();
		}
		return DialogResult.OK;
	}

	public DialogResult ShowMessage(int id, int reference)
	{
		string text = null;
		bool flag = true;
		m_CurrentIndex = -1;
		for (int i = 0; i < setMessages.DataTableMex.Count; i++)
		{
			if (id == setMessages.DataTableMex[i].MexId)
			{
				text = setMessages.DataTableMex[i].MexText;
				flag = !setMessages.DataTableMex[i].MexSuppressed;
				m_CurrentIndex = i;
				break;
			}
		}
		textMessage.Text = text + " Reference: " + reference;
		if (flag)
		{
			SetUpLook(id);
			return ShowDialog();
		}
		return DialogResult.OK;
	}

	private void SetUpLook(int id)
	{
		if (id < 1000)
		{
			textErrorNumber.Text = "Please select your choice";
			pictureBox.Image = imageList.Images[3];
		}
		else if (id < 3000)
		{
			textErrorNumber.Text = "Warning: " + id;
			pictureBox.Image = imageList.Images[1];
		}
		else if (id < 5000)
		{
			textErrorNumber.Text = "Info: " + id;
			pictureBox.Image = imageList.Images[2];
		}
		else if (id < 15000)
		{
			textErrorNumber.Text = "Error: " + id;
			pictureBox.Image = imageList.Images[0];
		}
		else
		{
			textErrorNumber.Text = "Info";
			pictureBox.Image = imageList.Images[2];
		}
		checkSuppressMessage.Visible = id < 10000;
		checkSuppressMessage.Checked = false;
		buttonOK.Visible = id >= 1000;
		buttonNo.Visible = id < 1000;
		buttonYes.Visible = id < 1000;
		buttonCancel.Visible = id < 1000;
	}

	public DialogResult ShowMessage(int id, string messageText)
	{
		bool flag = true;
		m_CurrentIndex = -1;
		for (int i = 0; i < setMessages.DataTableMex.Count; i++)
		{
			if (id == setMessages.DataTableMex[i].MexId)
			{
				_ = setMessages.DataTableMex[i].MexText;
				flag = !setMessages.DataTableMex[i].MexSuppressed;
				m_CurrentIndex = i;
				break;
			}
		}
		textMessage.Text = messageText;
		if (flag)
		{
			SetUpLook(id);
			return ShowDialog();
		}
		return DialogResult.OK;
	}

	public DialogResult ShowMessage(int id, string messageText, bool merge)
	{
		if (!merge)
		{
			return ShowMessage(id, messageText);
		}
		string text = null;
		for (int i = 0; i < setMessages.DataTableMex.Count; i++)
		{
			if (id == setMessages.DataTableMex[i].MexId)
			{
				text = setMessages.DataTableMex[i].MexText;
				m_CurrentIndex = i;
				break;
			}
		}
		return ShowMessage(id, text + "\r\n" + messageText);
	}

	public void EnableMessages(bool enable)
	{
		for (int i = 0; i < setMessages.DataTableMex.Count; i++)
		{
			if (setMessages.DataTableMex[i].MexId < 10000)
			{
				setMessages.DataTableMex[i].MexSuppressed = !enable;
			}
		}
		setMessages.WriteXml(m_XmlFileName);
	}

	public void EnableWarnings(bool enable)
	{
		for (int i = 0; i < setMessages.DataTableMex.Count; i++)
		{
			if (setMessages.DataTableMex[i].MexId < 5000)
			{
				setMessages.DataTableMex[i].MexSuppressed = !enable;
			}
		}
		setMessages.WriteXml(m_XmlFileName);
	}

	public void EnableErrors(bool enable)
	{
		for (int i = 0; i < setMessages.DataTableMex.Count; i++)
		{
			if (setMessages.DataTableMex[i].MexId >= 5000 && setMessages.DataTableMex[i].MexId < 10000)
			{
				setMessages.DataTableMex[i].MexSuppressed = !enable;
			}
		}
		setMessages.WriteXml(m_XmlFileName);
	}

	private void checkSuppressMessage_CheckedChanged(object sender, EventArgs e)
	{
	}

	private void buttonOK_Click(object sender, EventArgs e)
	{
		SuppressCurrentMessageIfRequested();
	}

	private void buttonYes_Click(object sender, EventArgs e)
	{
		SuppressCurrentMessageIfRequested();
	}

	private void buttonNo_Click(object sender, EventArgs e)
	{
		SuppressCurrentMessageIfRequested();
	}

	private void SuppressCurrentMessageIfRequested()
	{
		if (!checkSuppressMessage.Checked || m_CurrentIndex < 0 ||
			m_CurrentIndex >= setMessages.DataTableMex.Count)
			return;
		setMessages.DataTableMex[m_CurrentIndex].MexSuppressed = true;
		if (!string.IsNullOrWhiteSpace(m_XmlFileName))
			setMessages.WriteXml(m_XmlFileName);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaLibrary.UserMessage));
		this.buttonOK = new System.Windows.Forms.Button();
		this.textMessage = new System.Windows.Forms.TextBox();
		this.checkSuppressMessage = new System.Windows.Forms.CheckBox();
		this.textErrorNumber = new System.Windows.Forms.TextBox();
		this.buttonCancel = new System.Windows.Forms.Button();
		this.buttonNo = new System.Windows.Forms.Button();
		this.buttonYes = new System.Windows.Forms.Button();
		this.pictureBox = new System.Windows.Forms.PictureBox();
		this.imageList = new System.Windows.Forms.ImageList(this.components);
		this.setMessages = new FifaLibrary.Messages();
		((System.ComponentModel.ISupportInitialize)this.pictureBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.setMessages).BeginInit();
		base.SuspendLayout();
		this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		resources.ApplyResources(this.buttonOK, "buttonOK");
		this.buttonOK.Name = "buttonOK";
		this.buttonOK.UseVisualStyleBackColor = true;
		this.buttonOK.Click += new System.EventHandler(buttonOK_Click);
		this.textMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
		resources.ApplyResources(this.textMessage, "textMessage");
		this.textMessage.Name = "textMessage";
		this.textMessage.ReadOnly = true;
		resources.ApplyResources(this.checkSuppressMessage, "checkSuppressMessage");
		this.checkSuppressMessage.Name = "checkSuppressMessage";
		this.checkSuppressMessage.UseVisualStyleBackColor = true;
		this.checkSuppressMessage.CheckedChanged += new System.EventHandler(checkSuppressMessage_CheckedChanged);
		resources.ApplyResources(this.textErrorNumber, "textErrorNumber");
		this.textErrorNumber.Name = "textErrorNumber";
		this.textErrorNumber.ReadOnly = true;
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		resources.ApplyResources(this.buttonCancel, "buttonCancel");
		this.buttonCancel.Name = "buttonCancel";
		this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
		resources.ApplyResources(this.buttonNo, "buttonNo");
		this.buttonNo.Name = "buttonNo";
		this.buttonNo.Click += new System.EventHandler(buttonNo_Click);
		this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
		resources.ApplyResources(this.buttonYes, "buttonYes");
		this.buttonYes.Name = "buttonYes";
		this.buttonYes.Click += new System.EventHandler(buttonYes_Click);
		resources.ApplyResources(this.pictureBox, "pictureBox");
		this.pictureBox.Name = "pictureBox";
		this.pictureBox.TabStop = false;
		this.imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList.ImageStream");
		this.imageList.TransparentColor = System.Drawing.Color.Fuchsia;
		this.imageList.Images.SetKeyName(0, "Error_16.PNG");
		this.imageList.Images.SetKeyName(1, "Warning_16.PNG");
		this.imageList.Images.SetKeyName(2, "Info_16.PNG");
		this.imageList.Images.SetKeyName(3, "Help.PNG");
		this.setMessages.DataSetName = "Messages";
		this.setMessages.Locale = new System.Globalization.CultureInfo("");
		this.setMessages.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.pictureBox);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonNo);
		base.Controls.Add(this.buttonYes);
		base.Controls.Add(this.textErrorNumber);
		base.Controls.Add(this.checkSuppressMessage);
		base.Controls.Add(this.textMessage);
		base.Controls.Add(this.buttonOK);
		base.Name = "UserMessage";
		((System.ComponentModel.ISupportInitialize)this.pictureBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.setMessages).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
