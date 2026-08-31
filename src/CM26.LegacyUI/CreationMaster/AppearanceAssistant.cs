using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CreationMaster;

/// <summary>
/// Local, deterministic appearance suggestions for a front-facing portrait.
/// The assistant deliberately limits itself to generic FC26 appearance fields;
/// it never guesses or overwrites a licensed/specific Frostbite face asset.
/// </summary>
internal static class AppearanceAssistant
{
	internal sealed class Suggestion
	{
		public int SkinToneCode { get; set; }
		public int HairTypeCode { get; set; }
		public int HeadTypeCode { get; set; }
		public int FacialHairTypeCode { get; set; }
		public int Confidence { get; set; }
		public string HairCategory { get; set; }
		public string HeadCategory { get; set; }
		public string Notes { get; set; }
	}

	public static Suggestion Analyze(Image source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));

		using (var image = new Bitmap(180, 220, PixelFormat.Format24bppRgb))
		{
			using (var graphics = Graphics.FromImage(image))
			{
				graphics.Clear(Color.White);
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.DrawImage(source, new Rectangle(0, 0, image.Width, image.Height));
			}

			double skinLuma = 0;
			double skinVariance = 0;
			int skinCount = 0;
			for (int y = 48; y < 164; y += 2)
			{
				for (int x = 48; x < 132; x += 2)
				{
					Color c = image.GetPixel(x, y);
					if (!IsProbableSkin(c)) continue;
					double luma = Luma(c);
					skinCount++;
					double delta = luma - skinLuma;
					skinLuma += delta / skinCount;
					skinVariance += delta * (luma - skinLuma);
				}
			}

			if (skinCount < 80)
			{
				return new Suggestion
				{
					SkinToneCode = 5,
					HairTypeCode = 2,
					HeadTypeCode = 0,
					FacialHairTypeCode = 0,
					HairCategory = "Short (fallback)",
					HeadCategory = "Caucasic (fallback)",
					Confidence = 18,
					Notes = "Face/skin region was not clear enough. Use a front-facing, evenly lit portrait."
				};
			}

			int skinTone = Clamp(1 + (int)Math.Round((225 - skinLuma) / 17.0), 1, 10);
			double hairDark = DarkRatio(image, new Rectangle(45, 16, 90, 55), skinLuma - 28);
			double beardDark = DarkNonSkinRatio(image, new Rectangle(60, 125, 60, 47), skinLuma - 32);
			double sideDark = DarkNonSkinRatio(image, new Rectangle(40, 55, 100, 92), skinLuma - 30);

			int hairCode;
			string category;
			if (hairDark < 0.12) { hairCode = 0; category = "Shaven"; }
			else if (hairDark < 0.27) { hairCode = 26; category = "Very Short"; }
			else if (sideDark < 0.18) { hairCode = 2; category = "Short"; }
			else if (sideDark < 0.32) { hairCode = 36; category = "Medium"; }
			else { hairCode = 8; category = "Long"; }

			int facialHair = beardDark < 0.08 ? 0 : beardDark < 0.22 ? 6 : 8;
			int headCode = skinTone >= 9 ? 1000 : skinTone >= 7 ? 1500 : 0;
			string headCategory = skinTone >= 9 ? "African" : skinTone >= 7 ? "Latin" : "Caucasic";
			double deviation = Math.Sqrt(skinVariance / Math.Max(1, skinCount - 1));
			int confidence = Clamp((int)(42 + Math.Min(34, skinCount / 28.0) - Math.Min(24, deviation / 2.2)), 20, 88);

			return new Suggestion
			{
				SkinToneCode = skinTone,
				HairTypeCode = hairCode,
				HeadTypeCode = headCode,
				FacialHairTypeCode = facialHair,
				HairCategory = category,
				HeadCategory = headCategory,
				Confidence = confidence,
				Notes = "Image-based generic suggestion. Preview the 3D head before saving."
			};
		}
	}

	public static IReadOnlyList<Suggestion> AnalyzeAlternatives(Image source, int confederation)
	{
		var primary = Analyze(source);
		var hairCodes = primary.SkinToneCode >= 8
			? new[] { primary.HairTypeCode, 71, 26, 8 }
			: new[] { primary.HairTypeCode, 2, 17, 36 };
		var alternatives = hairCodes.Distinct().Select((hair, index) => new Suggestion
		{
			SkinToneCode = primary.SkinToneCode,
			HairTypeCode = hair,
			HeadTypeCode = confederation == 4 && primary.SkinToneCode < 7 ? 500 : primary.HeadTypeCode,
			FacialHairTypeCode = index == 2 && primary.FacialHairTypeCode == 0 ? 6 : primary.FacialHairTypeCode,
			Confidence = Math.Max(12, primary.Confidence - index * 7),
			HairCategory = index == 0 ? primary.HairCategory : "Alternative " + (index + 1),
			HeadCategory = confederation == 4 && primary.SkinToneCode < 7 ? "Asiatic" : primary.HeadCategory,
			Notes = index == 0 ? primary.Notes : "Alternative generic match using skin tone and nationality region as a tie-breaker."
		}).ToList();
		return alternatives;
	}

	public static bool ConfirmApply(IWin32Window owner, Image preview, Suggestion suggestion)
	{
		return ConfirmApply(owner, preview, new[] { suggestion }, out _);
	}

	public static bool ConfirmApply(IWin32Window owner, Image preview, IReadOnlyList<Suggestion> suggestions, out Suggestion selected)
	{
		selected = suggestions?.FirstOrDefault() ?? throw new ArgumentException("At least one appearance suggestion is required.", nameof(suggestions));
		using (var dialog = new Form())
		using (var picture = new PictureBox())
		using (var summary = new Label())
		using (var skin = new NumericUpDown())
		using (var hair = new ComboBox())
		using (var head = new ComboBox())
		using (var beard = new ComboBox())
		using (var profile = new ComboBox())
		using (var apply = new Button())
		using (var cancel = new Button())
		{
			dialog.Text = "FC26 Appearance Assistant — Preview";
			dialog.StartPosition = FormStartPosition.CenterParent;
			dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
			dialog.MaximizeBox = false;
			dialog.MinimizeBox = false;
			dialog.ClientSize = new Size(660, 440);
			picture.Location = new Point(18, 18);
			picture.Size = new Size(230, 270);
			picture.SizeMode = PictureBoxSizeMode.Zoom;
			picture.Image = preview;
			summary.Location = new Point(270, 22);
			summary.Size = new Size(365, 125);
			summary.Font = new Font(SystemFonts.MessageBoxFont.FontFamily, 10f);
			summary.Text = "Suggested FC26 generic appearance\r\n\r\n" +
				"Confidence: " + selected.Confidence + "%\r\n\r\n" + selected.Notes +
				"\r\n\r\nChoose alternatives or manually override before applying:";
			profile.Location = new Point(380, 148); profile.Size = new Size(235, 24); profile.DropDownStyle = ComboBoxStyle.DropDownList;
			for (var index = 0; index < suggestions.Count; index++) profile.Items.Add("Suggestion " + (index + 1) + " — " + suggestions[index].Confidence + "%");
			var fieldLabel = new Label { Location = new Point(270, 152), Size = new Size(105, 170),
				Text = "Profile\r\n\r\nSkin tone\r\n\r\nHair model\r\n\r\nGeneric head\r\n\r\nFacial hair" };
			skin.Location = new Point(380, 188);
			skin.Size = new Size(80, 24);
			skin.Minimum = 1; skin.Maximum = 10; skin.Value = selected.SkinToneCode;
			string[] hairNames = { "Shaven (0)", "Very Short (26)", "Short (2)", "Modern (17)", "Medium (36)", "Long (8)", "Afro (71)" };
			int[] hairCodes = { 0, 26, 2, 17, 36, 8, 71 };
			hair.Location = new Point(380, 228); hair.Size = new Size(235, 24); hair.DropDownStyle = ComboBoxStyle.DropDownList;
			hair.Items.AddRange(hairNames); hair.SelectedIndex = Math.Max(0, Array.IndexOf(hairCodes, selected.HairTypeCode));
			string[] headNames = { "Caucasic (0)", "Asiatic (500)", "African (1000)", "Latin (1500)", "Female (5500)" };
			int[] headCodes = { 0, 500, 1000, 1500, 5500 };
			head.Location = new Point(380, 268); head.Size = new Size(235, 24); head.DropDownStyle = ComboBoxStyle.DropDownList;
			head.Items.AddRange(headNames); head.SelectedIndex = Math.Max(0, Array.IndexOf(headCodes, selected.HeadTypeCode));
			string[] beardNames = { "None (0)", "Stubble (6)", "Full Beard (8)" };
			int[] beardCodes = { 0, 6, 8 };
			beard.Location = new Point(380, 308); beard.Size = new Size(235, 24); beard.DropDownStyle = ComboBoxStyle.DropDownList;
			beard.Items.AddRange(beardNames); beard.SelectedIndex = Math.Max(0, Array.IndexOf(beardCodes, selected.FacialHairTypeCode));
			apply.Text = "Apply Suggestions";
			apply.DialogResult = DialogResult.OK;
			apply.Location = new Point(393, 380);
			apply.Size = new Size(135, 34);
			cancel.Text = "Cancel";
			cancel.DialogResult = DialogResult.Cancel;
			cancel.Location = new Point(537, 380);
			cancel.Size = new Size(78, 34);
			profile.SelectedIndexChanged += (_, _) =>
			{
				var choice = suggestions[Math.Max(0, profile.SelectedIndex)];
				skin.Value = choice.SkinToneCode;
				hair.SelectedIndex = Math.Max(0, Array.IndexOf(hairCodes, choice.HairTypeCode));
				head.SelectedIndex = Math.Max(0, Array.IndexOf(headCodes, choice.HeadTypeCode));
				beard.SelectedIndex = Math.Max(0, Array.IndexOf(beardCodes, choice.FacialHairTypeCode));
			};
			profile.SelectedIndex = 0;
			dialog.Controls.AddRange(new Control[] { picture, summary, fieldLabel, profile, skin, hair, head, beard, apply, cancel });
			dialog.AcceptButton = apply;
			dialog.CancelButton = cancel;
			if (dialog.ShowDialog(owner) != DialogResult.OK) return false;
			selected = suggestions[Math.Max(0, profile.SelectedIndex)];
			selected.SkinToneCode = (int)skin.Value;
			selected.HairTypeCode = hairCodes[Math.Max(0, hair.SelectedIndex)];
			selected.HeadTypeCode = headCodes[Math.Max(0, head.SelectedIndex)];
			selected.FacialHairTypeCode = beardCodes[Math.Max(0, beard.SelectedIndex)];
			return true;
		}
	}

	private static bool IsProbableSkin(Color c)
	{
		int max = Math.Max(c.R, Math.Max(c.G, c.B));
		int min = Math.Min(c.R, Math.Min(c.G, c.B));
		return c.R > 42 && c.G > 28 && c.B > 18 && max - min > 12 &&
			c.R >= c.G * 0.92 && c.G >= c.B * 0.72 && c.R - c.B > 10;
	}

	private static double DarkRatio(Bitmap image, Rectangle region, double threshold)
	{
		int dark = 0, total = 0;
		for (int y = region.Top; y < region.Bottom; y += 2)
			for (int x = region.Left; x < region.Right; x += 2)
			{
				total++;
				if (Luma(image.GetPixel(x, y)) < threshold) dark++;
			}
		return total == 0 ? 0 : (double)dark / total;
	}

	private static double DarkNonSkinRatio(Bitmap image, Rectangle region, double threshold)
	{
		int dark = 0, total = 0;
		for (int y = region.Top; y < region.Bottom; y += 2)
			for (int x = region.Left; x < region.Right; x += 2)
			{
				Color c = image.GetPixel(x, y);
				total++;
				if (!IsProbableSkin(c) && Luma(c) < threshold) dark++;
			}
		return total == 0 ? 0 : (double)dark / total;
	}

	private static double Luma(Color c) => 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
	private static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));
}
