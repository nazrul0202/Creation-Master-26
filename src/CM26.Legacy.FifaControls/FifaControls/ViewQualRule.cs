using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FifaControls;

public class ViewQualRule : UserControl
{
	private IContainer components;

	private ComboBox comboRule;

	private ComboBox comboTrophy;

	private ComboBox comboLeague1;

	private ComboBox comboTropht2;

	private ComboBox comboTeam;

	private NumericUpDown numeric;

	public ViewQualRule()
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
		this.comboRule = new System.Windows.Forms.ComboBox();
		this.comboTrophy = new System.Windows.Forms.ComboBox();
		this.comboLeague1 = new System.Windows.Forms.ComboBox();
		this.comboTropht2 = new System.Windows.Forms.ComboBox();
		this.comboTeam = new System.Windows.Forms.ComboBox();
		this.numeric = new System.Windows.Forms.NumericUpDown();
		((System.ComponentModel.ISupportInitialize)this.numeric).BeginInit();
		base.SuspendLayout();
		this.comboRule.FormattingEnabled = true;
		this.comboRule.Items.AddRange(new object[7] { "Fill From League", "Fill From League with Country Limit", "Fill From Competition ", "Fill From Competition with Backup Rule", "Fill From Competition with  Backup League", "Fill with  Specific Team", "Fill with  Special Team" });
		this.comboRule.Location = new System.Drawing.Point(5, 2);
		this.comboRule.Name = "comboRule";
		this.comboRule.Size = new System.Drawing.Size(161, 21);
		this.comboRule.TabIndex = 0;
		this.comboTrophy.FormattingEnabled = true;
		this.comboTrophy.Location = new System.Drawing.Point(172, 1);
		this.comboTrophy.Name = "comboTrophy";
		this.comboTrophy.Size = new System.Drawing.Size(121, 21);
		this.comboTrophy.TabIndex = 1;
		this.comboLeague1.FormattingEnabled = true;
		this.comboLeague1.Location = new System.Drawing.Point(299, 1);
		this.comboLeague1.Name = "comboLeague1";
		this.comboLeague1.Size = new System.Drawing.Size(121, 21);
		this.comboLeague1.TabIndex = 2;
		this.comboTropht2.FormattingEnabled = true;
		this.comboTropht2.Location = new System.Drawing.Point(426, 1);
		this.comboTropht2.Name = "comboTropht2";
		this.comboTropht2.Size = new System.Drawing.Size(121, 21);
		this.comboTropht2.TabIndex = 3;
		this.comboTeam.FormattingEnabled = true;
		this.comboTeam.Location = new System.Drawing.Point(553, 1);
		this.comboTeam.Name = "comboTeam";
		this.comboTeam.Size = new System.Drawing.Size(121, 21);
		this.comboTeam.TabIndex = 4;
		this.numeric.Location = new System.Drawing.Point(680, 1);
		this.numeric.Name = "numeric";
		this.numeric.Size = new System.Drawing.Size(75, 20);
		this.numeric.TabIndex = 5;
		this.numeric.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.numeric);
		base.Controls.Add(this.comboTeam);
		base.Controls.Add(this.comboTropht2);
		base.Controls.Add(this.comboLeague1);
		base.Controls.Add(this.comboTrophy);
		base.Controls.Add(this.comboRule);
		base.Name = "ViewQualRule";
		base.Size = new System.Drawing.Size(784, 23);
		((System.ComponentModel.ISupportInitialize)this.numeric).EndInit();
		base.ResumeLayout(false);
	}
}
