using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace CreationMaster.Controls;

/// <summary>
/// Hosts the WPF Mesh3DPreviewPanel inside a WinForms form. Exposes the
/// same LoadMesh/ShowStatus/ClearModel API as the WPF control so forms can
/// call it directly without touching the WPF layer.
/// </summary>
public sealed class Mesh3DPreviewHost : UserControl
{
    private readonly ElementHost _host;
    private readonly Mesh3DPreviewPanel _wpfPanel;

    public Mesh3DPreviewHost()
    {
        _wpfPanel = new Mesh3DPreviewPanel();
        _host = new ElementHost
        {
            Dock = DockStyle.Fill,
            Child = _wpfPanel,
        };
        Controls.Add(_host);
        BackColor = Color.FromArgb(30, 30, 30);
    }

    /// <summary>Load an exported FBX mesh into the in-app 3D preview.</summary>
    public void LoadMesh(string fbxPath, string texturePath = null)
    {
        _wpfPanel.LoadMesh(fbxPath, texturePath);
    }

    /// <summary>Show a status message instead of a model.</summary>
    public void ShowStatus(string message)
    {
        _wpfPanel.ShowStatus(message);
    }

    /// <summary>Clear the rendered model.</summary>
    public void ClearModel()
    {
        _wpfPanel.ClearModel();
    }
}
