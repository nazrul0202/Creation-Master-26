using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Assimp;
using Material = System.Windows.Media.Media3D.Material;
using MediaColor = System.Windows.Media.Color;
using WpfVector3D = System.Windows.Media.Media3D.Vector3D;

namespace CM26.Studio.Controls;

/// <summary>
/// Shared 3D mesh preview panel (kit, face, stadium, ball, boot). Loads an
/// exported FBX via HelixToolkit's Assimp importer and renders it with
/// orbit/zoom controls. Fades the mesh into the viewport when ready.
/// </summary>
public partial class Mesh3DPreviewPanel : System.Windows.Controls.UserControl
{
    private ModelVisual3D? _modelVisual;

    public Mesh3DPreviewPanel()
    {
        InitializeComponent();
        ShowStatus("No 3D model loaded");
    }

    /// <summary>
    /// Loads an FBX mesh from <paramref name="fbxPath"/> and renders it in the
    /// viewport. Applies <paramref name="texturePath"/> as the diffuse material
    /// texture when supplied (and the file exists). Clears the previous model
    /// and resets the camera to frame the new geometry.
    /// </summary>
    public void LoadMesh(string fbxPath, string? texturePath = null)
    {
        if (!File.Exists(fbxPath))
        {
            ShowStatus("Model file not found: " + Path.GetFileName(fbxPath));
            return;
        }

        LoadingOverlay.Visibility = Visibility.Visible;
        StatusText.Visibility = Visibility.Collapsed;

        Task.Run(() => LoadMeshBackground(fbxPath, texturePath))
            .ContinueWith(OnMeshLoaded, TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>Unload the current model and show a status message.</summary>
    public void ShowStatus(string message)
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
        StatusText.Text = message;
        StatusText.Visibility = Visibility.Visible;
        ClearModel();
    }

    /// <summary>Clear the rendered model.</summary>
    public void ClearModel()
    {
        if (_modelVisual != null)
        {
            Viewport.Children.Remove(_modelVisual);
            _modelVisual = null;
        }
    }

    // ---------- Background loading ----------

    private Model3DGroup LoadMeshBackground(string fbxPath, string? texturePath)
    {
        // HelixToolkit's ModelImporter uses Assimp for FBX support.
        var importer = new ModelImporter();
        var model = importer.Load(fbxPath);
        if (model == null)
            throw new InvalidDataException("FBX contains no geometry.");

        // Apply sidecar texture when supplied (kit texture, face texture, etc.).
        Material material;
        if (!string.IsNullOrWhiteSpace(texturePath) && File.Exists(texturePath))
        {
            material = CreateTextureMaterial(texturePath);
        }
        else
        {
            // Use the FBX's own material; if missing, fall back to a neutral grey
            // so the mesh is visible on the dark card background.
            material = new DiffuseMaterial(new SolidColorBrush(MediaColor.FromRgb(180, 180, 180)));
        }
        ApplyMaterialToModel(model, material);

        // Normalize scale: Frostbite meshes are often exported in metres with a
        // centre far from origin; centre the model at the world origin so the
        // viewport frames it correctly.
        var bounds = CalculateBounds(model);
        var centre = bounds.Location + new WpfVector3D(bounds.SizeX / 2, bounds.SizeY / 2, bounds.SizeZ / 2);
        var translate = new TranslateTransform3D(-centre.X, -centre.Y, -centre.Z);
        var scale = CalculateScale(bounds);
        var scaleTransform = new ScaleTransform3D(scale, scale, scale);
        var group = new Transform3DGroup { Children = { translate, scaleTransform } };
        model.Transform = group;

        return model;
    }

    private void OnMeshLoaded(Task<Model3DGroup> task)
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;

        if (task.Status != TaskStatus.RanToCompletion)
        {
            var msg = task.Exception?.GetBaseException().Message ?? "Failed to load model.";
            ShowStatus("Could not load 3D model:\n" + msg);
            return;
        }

        var model = task.Result;
        _modelVisual = new ModelVisual3D { Content = model };
        Viewport.Children.Add(_modelVisual);
        Viewport.ZoomExtents();
        StatusText.Visibility = Visibility.Collapsed;
    }

    // ---------- Helpers ----------

    private static Material CreateTextureMaterial(string texturePath)
    {
        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(texturePath, UriKind.Absolute);
        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();

        var brush = new ImageBrush(bitmap);
        var material = new DiffuseMaterial(brush);
        return material;
    }

    private static void ApplyMaterialToModel(Model3DGroup model, Material material)
    {
        foreach (var child in model.Children)
        {
            if (child is GeometryModel3D geometry)
            {
                geometry.Material = material;
                geometry.BackMaterial = material;
            }
            else if (child is Model3DGroup group)
            {
                ApplyMaterialToModel(group, material);
            }
        }
    }

    private static Rect3D CalculateBounds(Model3DGroup model)
    {
        var min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
        var max = new Point3D(double.MinValue, double.MinValue, double.MinValue);

        WalkBounds(model, ref min, ref max);

        if (min.X > max.X) // no geometry
            return new Rect3D(0, 0, 0, 1, 1, 1);

        return new Rect3D(min.X, min.Y, min.Z,
                          Math.Max(0.001, max.X - min.X),
                          Math.Max(0.001, max.Y - min.Y),
                          Math.Max(0.001, max.Z - min.Z));
    }

    private static void WalkBounds(Model3DGroup group, ref Point3D min, ref Point3D max)
    {
        foreach (var child in group.Children)
        {
            if (child is GeometryModel3D geometry && geometry.Geometry is MeshGeometry3D mesh)
            {
                foreach (var pos in mesh.Positions)
                {
                    if (pos.X < min.X) min.X = pos.X;
                    if (pos.Y < min.Y) min.Y = pos.Y;
                    if (pos.Z < min.Z) min.Z = pos.Z;
                    if (pos.X > max.X) max.X = pos.X;
                    if (pos.Y > max.Y) max.Y = pos.Y;
                    if (pos.Z > max.Z) max.Z = pos.Z;
                }
            }
            else if (child is Model3DGroup subGroup)
            {
                WalkBounds(subGroup, ref min, ref max);
            }
        }
    }

    private static double CalculateScale(Rect3D bounds)
    {
        // Normalize the largest dimension to roughly 1.0 world unit.
        var largest = Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ));
        if (largest < 0.0001) return 1.0;
        return 1.0 / largest;
    }
}