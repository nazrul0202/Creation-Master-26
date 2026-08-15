using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CreationMaster;

public class UgcForm : Form
{
	private IContainer components;

	private Button buttonImport;

	private Button buttonCancel;

	public UgcForm()
	{
		InitializeComponent();
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
		this.buttonImport = new System.Windows.Forms.Button();
		this.buttonCancel = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.buttonImport.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.buttonImport.Location = new System.Drawing.Point(133, 334);
		this.buttonImport.Name = "buttonImport";
		this.buttonImport.Size = new System.Drawing.Size(75, 23);
		this.buttonImport.TabIndex = 0;
		this.buttonImport.Text = "Import";
		this.buttonImport.UseVisualStyleBackColor = true;
		this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.buttonCancel.Location = new System.Drawing.Point(561, 333);
		this.buttonCancel.Name = "buttonCancel";
		this.buttonCancel.Size = new System.Drawing.Size(75, 23);
		this.buttonCancel.TabIndex = 1;
		this.buttonCancel.Text = "Cancel";
		this.buttonCancel.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(764, 389);
		base.Controls.Add(this.buttonCancel);
		base.Controls.Add(this.buttonImport);
		base.Name = "UgcForm";
		this.Text = "UgcForm";
		base.ResumeLayout(false);
	}
}
