using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace CreationMaster;

/// <summary>
/// Connects the existing classic editor buttons to the FC26 Frostbite staging
/// service without changing their layout or their pre-FC26 behaviour.
/// </summary>
internal static class Fc26DirectAssetUi
{
	internal static bool Export(IWin32Window owner, string logicalPath, string exportDirectory, string description)
	{
		try
		{
			string source = Fc26HostBridge.ExportAsset(logicalPath);
			if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
			{
				MessageBox.Show(owner, description + " was not found in the loaded FC26 archives.", "CM26 Direct Asset", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return false;
			}

			Directory.CreateDirectory(exportDirectory);
			string fileName = Path.GetFileName(logicalPath.Replace('/', Path.DirectorySeparatorChar));
			if (string.IsNullOrWhiteSpace(fileName)) fileName = Path.GetFileName(source);
			string destination = Path.Combine(exportDirectory, fileName);
			File.Copy(source, destination, true);
			return true;
		}
		catch (Exception ex)
		{
			ShowError(owner, description, ex);
			return false;
		}
	}

	internal static bool ExportWithDialog(IWin32Window owner, string logicalPath, ref string currentDirectory, string description)
	{
		using (var dialog = new SaveFileDialog())
		{
			dialog.Title = "Export " + description;
			dialog.FileName = Path.GetFileName(logicalPath.Replace('/', Path.DirectorySeparatorChar));
			dialog.Filter = "Native FC26 asset (*" + Path.GetExtension(dialog.FileName) + ")|*" +
				Path.GetExtension(dialog.FileName) + "|All files (*.*)|*.*";
			if (!string.IsNullOrWhiteSpace(currentDirectory) && Directory.Exists(currentDirectory))
				dialog.InitialDirectory = currentDirectory;
			if (dialog.ShowDialog(owner) != DialogResult.OK) return false;

			try
			{
				string source = Fc26HostBridge.ExportAsset(logicalPath);
				if (string.IsNullOrWhiteSpace(source) || !File.Exists(source))
				{
					MessageBox.Show(owner, description + " was not found in the loaded FC26 archives.", "CM26 Direct Asset", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return false;
				}
				File.Copy(source, dialog.FileName, true);
				currentDirectory = Path.GetDirectoryName(dialog.FileName);
				return true;
			}
			catch (Exception ex)
			{
				ShowError(owner, description, ex);
				return false;
			}
		}
	}

	internal static bool Import(IWin32Window owner, string logicalPath, string sourcePath, string description)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
				throw new FileNotFoundException("The selected source file was not found.", sourcePath);
			Fc26HostBridge.StageFile(logicalPath, sourcePath);
			return true;
		}
		catch (Exception ex)
		{
			ShowError(owner, description, ex);
			return false;
		}
	}

	internal static bool ImportImage(IWin32Window owner, string logicalPath, Bitmap bitmap, int width, int height, string description)
	{
		string temporary = null;
		try
		{
			if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));
			temporary = Path.Combine(Path.GetTempPath(), "cm26-image-" + Guid.NewGuid().ToString("N") + ".png");
			bitmap.Save(temporary, ImageFormat.Png);
			Fc26HostBridge.StageImage(logicalPath, temporary, width, height);
			return true;
		}
		catch (Exception ex)
		{
			ShowError(owner, description, ex);
			return false;
		}
		finally
		{
			try { if (!string.IsNullOrWhiteSpace(temporary) && File.Exists(temporary)) File.Delete(temporary); } catch { }
		}
	}

	internal static bool Remove(IWin32Window owner, string logicalPath, string description)
	{
		try
		{
			Fc26HostBridge.RemoveStagedAsset(logicalPath);
			return true;
		}
		catch (Exception ex)
		{
			ShowError(owner, description, ex);
			return false;
		}
	}

	private static void ShowError(IWin32Window owner, string description, Exception ex)
	{
		MessageBox.Show(owner, description + " failed.\r\n\r\n" + ex.Message, "CM26 Direct Asset", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
}
