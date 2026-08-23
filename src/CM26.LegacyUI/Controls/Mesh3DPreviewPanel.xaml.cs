using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Assimp;
using WpfMaterial = System.Windows.Media.Media3D.Material;
using WpfVector3D = System.Windows.Media.Media3D.Vector3D;

namespace CreationMaster.Controls;

/// <summary>
/// WPF 3D mesh preview panel for FC26 Frostbite meshes. Loads an exported FBX
/// via AssimpNet and renders it with HelixToolkit orbit/zoom controls.
/// Designed to be hosted inside a WinForms ElementHost.
/// </summary>
public partial class Mesh3DPreviewPanel : System.Windows.Controls.UserControl
{
    private ModelVisual3D _modelVisual;
    private int _loadGeneration;

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
    public void LoadMesh(string fbxPath, string texturePath = null)
    {
        var generation = Interlocked.Increment(ref _loadGeneration);
        if (!File.Exists(fbxPath))
        {
            ShowStatus("Model file not found: " + Path.GetFileName(fbxPath));
            return;
        }

        ClearModel();
        LoadingOverlay.Visibility = Visibility.Visible;
        StatusText.Visibility = Visibility.Collapsed;

        Task.Run(() => LoadMeshBackground(fbxPath, texturePath))
            .ContinueWith(task => OnMeshLoaded(task, generation),
                TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>Unload the current model and show a status message.</summary>
    public void ShowStatus(string message)
    {
        Interlocked.Increment(ref _loadGeneration);
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

    private Model3DGroup LoadMeshBackground(string fbxPath, string texturePath)
    {
        // Use AssimpNet to import the FBX (HelixToolkit.Wpf ModelImporter does not support FBX).
        Model3DGroup model;
        using (var ctx = new AssimpContext())
        {
            var scene = ctx.ImportFile(fbxPath,
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.JoinIdenticalVertices |
                PostProcessSteps.FlipUVs);

            if (scene == null || !scene.HasMeshes)
                throw new InvalidDataException("FBX contains no geometry.");

            model = ConvertToWpfModel(scene);
        }

        // Apply sidecar texture when supplied (face texture, kit texture, etc.).
        WpfMaterial material;
        if (!string.IsNullOrWhiteSpace(texturePath) && File.Exists(texturePath))
        {
            material = CreateTextureMaterial(texturePath);
        }
        else
        {
            // Use a neutral grey so the mesh is visible on the dark background.
            material = CreateSolidMaterial(Color.FromRgb(180, 180, 180));
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

        // The model is created on a worker thread. Freezing it makes the WPF
        // Freezable graph safe to hand to the UI thread and avoids intermittent
        // "different thread owns it" failures on larger FC26 meshes.
        if (model.CanFreeze)
            model.Freeze();

        return model;
    }

    private void OnMeshLoaded(Task<Model3DGroup> task, int generation)
    {
        // A player/team may have changed while Assimp was importing the FBX.
        // Never let that stale result replace the latest requested preview.
        if (generation != Volatile.Read(ref _loadGeneration))
            return;

        LoadingOverlay.Visibility = Visibility.Collapsed;

        if (task.Status != TaskStatus.RanToCompletion)
        {
            var msg = task.Exception?.GetBaseException().Message ?? "Failed to load model.";
            ShowStatus("Could not load 3D model:\n" + msg);
            return;
        }

        var model = task.Result;
        ClearModel();
        _modelVisual = new ModelVisual3D { Content = model };
        Viewport.Children.Add(_modelVisual);
        Viewport.ZoomExtents();
        StatusText.Visibility = Visibility.Collapsed;
    }

    // ---------- Assimp → WPF conversion ----------

    private static Model3DGroup ConvertToWpfModel(Scene scene)
    {
        var group = new Model3DGroup();
        ConvertNode(scene.RootNode, scene, group, Matrix3D.Identity);
        return group;
    }

    private static void ConvertNode(Node node, Scene scene, Model3DGroup parent, Matrix3D parentTransform)
    {
        // Accumulate the node transform.
        var nodeMatrix = ToMatrix3D(node.Transform);
        var worldMatrix = Matrix3D.Multiply(nodeMatrix, parentTransform);

        // Convert each mesh referenced by this node.
        foreach (var meshIndex in node.MeshIndices)
        {
            var mesh = scene.Meshes[meshIndex];
            var geometry = ConvertMesh(mesh, worldMatrix);
            if (geometry != null)
                parent.Children.Add(geometry);
        }

        // Recurse into children.
        foreach (var child in node.Children)
            ConvertNode(child, scene, parent, worldMatrix);
    }

    private static GeometryModel3D ConvertMesh(Assimp.Mesh mesh, Matrix3D transform)
    {
        if (!mesh.HasFaces || !mesh.HasVertices)
            return null;

        var positions = new Point3DCollection();
        var normals = new System.Windows.Media.Media3D.Vector3DCollection();
        var texCoords = new PointCollection();
        var indices = new Int32Collection();

        // Vertices
        foreach (var v in mesh.Vertices)
        {
            var p = new Point3D(v.X, v.Y, v.Z);
            p = transform.Transform(p);
            positions.Add(p);
        }

        // Normals
        if (mesh.HasNormals)
        {
            foreach (var n in mesh.Normals)
            {
                var v = new WpfVector3D(n.X, n.Y, n.Z);
                v = transform.Transform(v);
                v.Normalize();
                normals.Add(v);
            }
        }

        // Texture coordinates (channel 0)
        if (mesh.TextureCoordinateChannelCount > 0)
        {
            var channel = mesh.TextureCoordinateChannels[0];
            foreach (var tc in channel)
                texCoords.Add(new System.Windows.Point(tc.X, tc.Y));
        }

        // Faces → triangles
        foreach (var face in mesh.Faces)
        {
            if (face.IndexCount == 3)
            {
                indices.Add(face.Indices[0]);
                indices.Add(face.Indices[1]);
                indices.Add(face.Indices[2]);
            }
            else if (face.IndexCount == 4)
            {
                // Quad → two triangles
                indices.Add(face.Indices[0]);
                indices.Add(face.Indices[1]);
                indices.Add(face.Indices[2]);
                indices.Add(face.Indices[0]);
                indices.Add(face.Indices[2]);
                indices.Add(face.Indices[3]);
            }
        }

        if (indices.Count == 0)
            return null;

        var geometry = new MeshGeometry3D
        {
            Positions = positions,
            TriangleIndices = indices,
        };
        if (normals.Count == positions.Count)
            geometry.Normals = normals;
        if (texCoords.Count == positions.Count)
            geometry.TextureCoordinates = texCoords;

        return new GeometryModel3D
        {
            Geometry = geometry,
            Material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(180, 180, 180))),
            BackMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(160, 160, 160)))
        };
    }

    private static Matrix3D ToMatrix3D(Assimp.Matrix4x4 m)
    {
        // Assimp uses row-major; WPF uses row-major too but with different layout.
        return new Matrix3D(
            m.A1, m.B1, m.C1, m.D1,
            m.A2, m.B2, m.C2, m.D2,
            m.A3, m.B3, m.C3, m.D3,
            m.A4, m.B4, m.C4, m.D4);
    }

    // ---------- Helpers ----------

    private static WpfMaterial CreateTextureMaterial(string texturePath)
    {
        var bitmap = new System.Windows.Media.Imaging.BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(texturePath, UriKind.Absolute);
        bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();

        var brush = new ImageBrush(bitmap);
        brush.Freeze();
        var material = new DiffuseMaterial(brush);
        material.Freeze();
        return material;
    }

    private static WpfMaterial CreateSolidMaterial(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        var material = new DiffuseMaterial(brush);
        material.Freeze();
        return material;
    }

    private static void ApplyMaterialToModel(Model3DGroup model, WpfMaterial material)
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
