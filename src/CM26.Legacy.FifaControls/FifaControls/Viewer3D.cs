using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FifaLibrary;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace FifaControls;

public class Viewer3D : UserControl
{
	private Device m_Device;

	private PresentParameters m_PresentParams = new PresentParameters();

	private Mesh m_XFileMesh;

	private Mesh[] m_Meshes;

	private Texture[] m_Textures;

	private Material[] m_Materials;

	private ExtendedMaterial[] m_ExtendedMaterials;

	private bool[] m_ZbufferRenderState;

	private bool pause;

	private int m_MouseState;

	private float m_RotationY;

	private float m_RotationX;

	private float m_ViewX;

	private float m_ViewY = 100f;

	private float m_ViewZ = 100f;

	private float m_LightDirectionX;

	private float m_LightDirectionY;

	private float m_LightDirectionZ;

	private float m_StartViewX;

	private float m_StartViewY = 100f;

	private float m_StartViewZ = 100f;

	private float m_StartRotationY;

	private float m_StartRotationX;

	private float m_LightX;

	private float m_LightY = 100f;

	private float m_LightZ = 100f;

	private float m_MinX;

	private float m_MinY;

	private float m_MinZ;

	private float m_MaxX;

	private float m_MaxY;

	private float m_MaxZ;

	private float m_RotationYCoeff = 0.01f;

	private string m_XFileName;

	private string m_XFilePath;

	private Color m_AmbientColor = Color.White;

	private int m_MouseX;

	private int m_MouseY;

	private IContainer components;

	public bool[] ZbufferRenderState
	{
		get
		{
			return m_ZbufferRenderState;
		}
		set
		{
			m_ZbufferRenderState = value;
		}
	}

	public float RotationYCoeff
	{
		get
		{
			return m_RotationYCoeff;
		}
		set
		{
			m_RotationYCoeff = value;
		}
	}

	public float ViewX
	{
		get
		{
			return m_StartViewX;
		}
		set
		{
			m_ViewX = value;
			m_StartViewX = value;
		}
	}

	public float ViewY
	{
		get
		{
			return m_StartViewY;
		}
		set
		{
			m_ViewY = value;
			m_StartViewY = value;
		}
	}

	public float ViewZ
	{
		get
		{
			return m_StartViewZ;
		}
		set
		{
			m_ViewZ = value;
			m_StartViewZ = value;
		}
	}

	public float RotationX
	{
		get
		{
			return m_StartRotationX;
		}
		set
		{
			m_RotationX = value;
			m_StartRotationX = value;
		}
	}

	public float RotationY
	{
		get
		{
			return m_StartRotationY;
		}
		set
		{
			m_RotationY = value;
			m_StartRotationY = value;
		}
	}

	public float LightDirectionZ
	{
		get
		{
			return m_LightDirectionZ;
		}
		set
		{
			m_LightDirectionZ = value;
		}
	}

	public float LightDirectionY
	{
		get
		{
			return m_LightDirectionY;
		}
		set
		{
			m_LightDirectionY = value;
		}
	}

	public float LightDirectionX
	{
		get
		{
			return m_LightDirectionX;
		}
		set
		{
			m_LightDirectionX = value;
		}
	}

	public float LightX
	{
		get
		{
			return m_LightX;
		}
		set
		{
			m_LightX = value;
		}
	}

	public float LightY
	{
		get
		{
			return m_LightY;
		}
		set
		{
			m_LightY = value;
		}
	}

	public float LightZ
	{
		get
		{
			return m_LightZ;
		}
		set
		{
			m_LightZ = value;
		}
	}

	public Color AmbientColor
	{
		get
		{
			return m_AmbientColor;
		}
		set
		{
			m_AmbientColor = value;
		}
	}

	public Viewer3D()
	{
		InitializeComponent();
		if (!InitializeHardwareGraphics())
		{
			InitializeSoftwareGraphics();
		}
	}

	public bool InitializeHardwareGraphics()
	{
		try
		{
			m_PresentParams.Windowed = true;
			m_PresentParams.SwapEffect = SwapEffect.Discard;
			m_PresentParams.EnableAutoDepthStencil = true;
			m_PresentParams.AutoDepthStencilFormat = DepthFormat.D16;
			m_Device = new Device(0, DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, m_PresentParams);
			m_Device.DeviceReset += OnResetDevice;
			OnResetDevice(m_Device, null);
			pause = false;
		}
		catch (DirectXException)
		{
			return false;
		}
		return true;
	}

	public bool InitializeSoftwareGraphics()
	{
		try
		{
			m_PresentParams.Windowed = true;
			m_PresentParams.SwapEffect = SwapEffect.Discard;
			m_PresentParams.EnableAutoDepthStencil = true;
			m_PresentParams.AutoDepthStencilFormat = DepthFormat.D16;
			m_Device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing | CreateFlags.FpuPreserve, m_PresentParams);
			m_Device.DeviceReset += OnResetDevice;
			OnResetDevice(m_Device, null);
			pause = false;
		}
		catch (DirectXException)
		{
			return false;
		}
		return true;
	}

	public void ShowEmpty()
	{
		m_Materials = null;
		Render();
	}

	public void Show3D(string xFileName)
	{
		try
		{
			m_XFileName = Path.GetFileName(xFileName);
			m_XFilePath = Path.GetDirectoryName(xFileName);
			string currentDirectory = Environment.CurrentDirectory;
			Directory.SetCurrentDirectory(m_XFilePath);
			if (m_XFileMesh != null)
			{
				m_XFileMesh.Dispose();
			}
			if (m_ExtendedMaterials != null && m_Textures != null)
			{
				for (int i = 0; i < m_ExtendedMaterials.Length; i++)
				{
					if (m_Textures[i] != null)
					{
						m_Textures[i].Dispose();
					}
				}
			}
			m_XFileMesh = Mesh.FromFile(m_XFileName, MeshFlags.Managed, m_Device, out m_ExtendedMaterials);
			m_Textures = new Texture[m_ExtendedMaterials.Length];
			m_Materials = new Material[m_ExtendedMaterials.Length];
			for (int j = 0; j < m_ExtendedMaterials.Length; j++)
			{
				m_Materials[j] = m_ExtendedMaterials[j].Material3D;
				m_Materials[j].Ambient = m_Materials[j].Diffuse;
				if (m_Textures[j] != null)
				{
					m_Textures[j].Dispose();
				}
				m_Textures[j] = TextureLoader.FromFile(m_Device, m_ExtendedMaterials[j].TextureFilename);
			}
			Render();
			Directory.SetCurrentDirectory(currentDirectory);
		}
		catch (DirectXException)
		{
			m_Materials = null;
			m_Textures = null;
			Render();
		}
	}

	public void Show3D(Bitmap bitmap, int partIndex)
	{
		if (partIndex < 0 || partIndex >= m_Textures.Length)
		{
			return;
		}
		try
		{
			m_Textures[partIndex] = Texture.FromBitmap(m_Device, bitmap, Usage.Points, Pool.Default);
			m_Textures[partIndex].Dispose();
			Render();
		}
		catch (DirectXException)
		{
			m_Materials = null;
			m_Textures = null;
			Render();
		}
	}

	public void Show3D(Bitmap bitmap)
	{
		try
		{
			Texture texture = new Texture(m_Device, bitmap, Usage.RenderTarget, Pool.Managed);
			for (int i = 0; i < m_Textures.Length; i++)
			{
				m_Textures[i] = texture;
			}
			Render();
		}
		catch (DirectXException)
		{
			m_Materials = null;
			m_Textures = null;
			Render();
		}
	}

	private void OnResetDevice(object sender, EventArgs e)
	{
		m_Device.RenderState.ZBufferEnable = true;
		m_Device.RenderState.ZBufferFunction = Compare.Less;
		m_Device.RenderState.ZBufferWriteEnable = true;
		m_Device.RenderState.AlphaBlendEnable = true;
		m_Device.RenderState.AlphaTestEnable = false;
		m_Device.RenderState.AlphaFunction = Compare.Always;
		m_Device.RenderState.BlendOperation = BlendOperation.Add;
		m_Device.RenderState.SourceBlend = Blend.SourceAlpha;
		m_Device.RenderState.DestinationBlend = Blend.InvSourceAlpha;
		m_Device.RenderState.AlphaBlendOperation = BlendOperation.Max;
		m_Device.RenderState.AlphaSourceBlend = Blend.One;
		m_Device.RenderState.AlphaDestinationBlend = Blend.One;
		m_Device.RenderState.StencilEnable = false;
		m_Device.RenderState.FillMode = FillMode.Solid;
		m_Device.RenderState.CullMode = Cull.None;
		m_Device.RenderState.SpecularEnable = false;
		m_Device.RenderState.SpecularMaterialSource = ColorSource.Material;
		m_Device.RenderState.Ambient = m_AmbientColor;
		m_Device.Lights[0].Type = LightType.Directional;
		m_Device.Lights[0].Position = new Vector3(m_LightX, m_LightY, m_LightZ);
		m_Device.Lights[0].Diffuse = Color.White;
		m_Device.Lights[0].Direction = new Vector3(m_LightDirectionX, m_LightDirectionY, m_LightDirectionZ);
		m_Device.Lights[0].Enabled = true;
		m_Device.RenderState.Lighting = true;
	}

	private void SetupMatrices()
	{
		m_Device.Transform.World = Matrix.RotationYawPitchRoll(m_RotationY, m_RotationX, 0f);
		m_Device.Transform.View = Matrix.LookAtLH(new Vector3(m_ViewX, m_ViewY, m_ViewZ), new Vector3(m_ViewX, m_ViewY, 0f), new Vector3(0f, 1f, 0f));
		float aspectRatio = (float)base.Width / (float)base.Height;
		m_Device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4f, aspectRatio, 1f, 1000f);
	}

	public void Render()
	{
		if (m_Device == null || pause)
		{
			return;
		}
		try
		{
			m_Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Gray, 1f, 0);
			m_Device.Lights[0].Type = LightType.Directional;
			m_Device.Lights[0].Position = new Vector3(m_LightX, m_LightY, m_LightZ);
			m_Device.Lights[0].Diffuse = Color.White;
			m_Device.Lights[0].Direction = new Vector3(m_LightDirectionX, m_LightDirectionY, m_LightDirectionZ);
			m_Device.Lights[0].Enabled = true;
			m_Device.RenderState.Ambient = m_AmbientColor;
			m_Device.BeginScene();
			SetupMatrices();
			if (m_XFileMesh != null && m_Materials != null)
			{
				m_Device.RenderState.ZBufferWriteEnable = true;
				for (int i = 0; i < m_Materials.Length; i++)
				{
					int num = i;
					m_Device.Material = m_Materials[num];
					m_Device.SetTexture(0, m_Textures[num]);
					if (m_Materials.Length == 6 && (i == 4 || i == 5))
					{
						m_Device.RenderState.ZBufferWriteEnable = false;
					}
					m_XFileMesh.DrawSubset(num);
				}
			}
			if (m_Meshes != null && m_Materials != null)
			{
				for (int j = 0; j < m_Meshes.Length; j++)
				{
					m_Device.RenderState.ZBufferWriteEnable = m_ZbufferRenderState[j];
					if (m_Meshes[j] != null)
					{
						m_Device.Material = m_Materials[j];
						m_Device.SetTexture(0, m_Textures[j]);
						m_Meshes[j].DrawSubset(0);
					}
				}
			}
			m_Device.EndScene();
			m_Device.Present();
		}
		catch (DirectXException)
		{
		}
	}

	private void Viewer3D_MouseDown(object sender, MouseEventArgs e)
	{
		switch (e.Button)
		{
		case MouseButtons.Left:
			m_MouseState = 1;
			m_MouseX = e.X;
			m_MouseY = e.Y;
			break;
		case MouseButtons.Right:
			m_MouseState = 2;
			m_MouseX = e.X;
			m_MouseY = e.Y;
			break;
		case MouseButtons.Middle:
			m_MouseState = 4;
			m_MouseX = e.X;
			m_MouseY = e.Y;
			break;
		}
		Render();
	}

	private void Viewer3D_MouseUp(object sender, MouseEventArgs e)
	{
		m_MouseState = 0;
	}

	private void Viewer3D_MouseMove(object sender, MouseEventArgs e)
	{
		if (m_MouseState == 0)
		{
			_ = e.Delta;
			return;
		}
		bool flag = false;
		switch (m_MouseState)
		{
		case 0:
			return;
		case 1:
		{
			int num = e.X - m_MouseX;
			int num2 = e.Y - m_MouseY;
			m_RotationY -= (float)num * 0.01f;
			while (m_RotationY < 0f)
			{
				m_RotationY += 6.28f;
			}
			while (m_RotationY > 6.28f)
			{
				m_RotationY -= 6.28f;
			}
			m_RotationX += (float)num2 * m_RotationYCoeff;
			while (m_RotationX < 0f)
			{
				m_RotationX += 6.28f;
			}
			while (m_RotationX > 6.28f)
			{
				m_RotationX -= 6.28f;
			}
			flag = true;
			break;
		}
		case 2:
		{
			int num = e.X - m_MouseX;
			int num2 = e.Y - m_MouseY;
			m_ViewZ -= (float)num2 * 0.2f;
			if (m_ViewZ < -1000f)
			{
				m_ViewZ = -1000f;
			}
			if (m_ViewZ > 1000f)
			{
				m_ViewZ = 1000f;
			}
			flag = true;
			break;
		}
		case 4:
		{
			int num = e.X - m_MouseX;
			int num2 = e.Y - m_MouseY;
			m_ViewX += (float)num * 0.2f;
			if (m_ViewX < -1000f)
			{
				m_ViewX = -1000f;
			}
			if (m_ViewX > 1000f)
			{
				m_ViewX = 1000f;
			}
			m_ViewY += (float)num2 * 0.2f;
			if (m_ViewY < -1000f)
			{
				m_ViewY = -1000f;
			}
			if (m_ViewY > 1000f)
			{
				m_ViewY = 1000f;
			}
			flag = true;
			break;
		}
		}
		m_MouseX = e.X;
		m_MouseY = e.Y;
		if (flag)
		{
			Render();
		}
	}

	private void Viewer3D_DoubleClick(object sender, EventArgs e)
	{
		m_RotationY = m_StartRotationY;
		m_RotationX = m_StartRotationX;
		m_ViewX = m_StartViewX;
		m_ViewY = m_StartViewY;
		m_ViewZ = m_StartViewZ;
		Render();
	}

	public Bitmap Photo()
	{
		try
		{
			Surface backBuffer = m_Device.GetBackBuffer(0, 0, BackBufferType.Mono);
			Bitmap result = new Bitmap(SurfaceLoader.SaveToStream(ImageFileFormat.Bmp, backBuffer));
			backBuffer.Dispose();
			return result;
		}
		catch
		{
			return null;
		}
	}

	public void Clean(int nMeshes)
	{
		if (m_XFileMesh != null)
		{
			m_XFileMesh.Dispose();
			m_XFileMesh = null;
		}
		if (m_Textures != null)
		{
			for (int i = 0; i < m_Textures.Length; i++)
			{
				if (m_Textures[i] != null)
				{
					m_Textures[i].Dispose();
				}
			}
			m_Textures = null;
		}
		if (m_Meshes != null)
		{
			for (int j = 0; j < m_Meshes.Length; j++)
			{
				if (m_Meshes[j] != null)
				{
					m_Meshes[j].Dispose();
				}
			}
		}
		m_Meshes = new Mesh[nMeshes];
		if (nMeshes != 0)
		{
			m_Textures = new Texture[nMeshes];
			m_Materials = new Material[nMeshes];
			m_ZbufferRenderState = new bool[nMeshes];
			for (int k = 0; k < nMeshes; k++)
			{
				m_ZbufferRenderState[k] = true;
				m_Materials[k].Ambient = Color.FromArgb(0, 255, 255, 255);
				m_Materials[k].Diffuse = Color.FromArgb(0, 255, 255, 255);
				m_Materials[k].Specular = Color.FromArgb(0, 255, 255, 255);
			}
		}
		CleanBoundingBox();
	}

	public void SetMesh(int meshIndex, Model3D model3D)
	{
		SetMesh(meshIndex, model3D, zBufferState: true);
	}

	public void SetMesh(int meshIndex, Model3D model3D, bool zBufferState)
	{
		if (model3D == null)
		{
			return;
		}
		try
		{
			UpdateBoundingBox(model3D.Vertex);
			m_Meshes[meshIndex] = new Mesh(model3D.NFaces, model3D.NVertex, MeshFlags.Managed, VertexFormats.PositionNormal | VertexFormats.Texture1, m_Device);
			m_Meshes[meshIndex].SetIndexBufferData(model3D.Index, LockFlags.None);
			m_Meshes[meshIndex].SetVertexBufferData(model3D.Vertex, LockFlags.None);
			Texture texture = null;
			if (model3D.TextureBitmap != null)
			{
				texture = Texture.FromBitmap(m_Device, model3D.TextureBitmap, Usage.None, Pool.Managed);
			}
			m_Textures[meshIndex] = texture;
			m_ZbufferRenderState[meshIndex] = zBufferState;
		}
		catch
		{
		}
	}

	private void UpdateBoundingBox(PositionNormalTextured[] Vertex)
	{
		for (int i = 0; i < Vertex.Length; i++)
		{
			if (Vertex[i].X < m_MinX)
			{
				m_MinX = Vertex[i].X;
			}
			if (Vertex[i].X > m_MaxX)
			{
				m_MaxX = Vertex[i].X;
			}
			if (Vertex[i].Y < m_MinY)
			{
				m_MinY = Vertex[i].Y;
			}
			if (Vertex[i].Y > m_MaxY)
			{
				m_MaxY = Vertex[i].Y;
			}
			if (Vertex[i].Z < m_MinZ)
			{
				m_MinZ = Vertex[i].Z;
			}
			if (Vertex[i].Z > m_MaxZ)
			{
				m_MaxZ = Vertex[i].Z;
			}
		}
	}

	private void CleanBoundingBox()
	{
		m_MinX = float.MaxValue;
		m_MinY = float.MaxValue;
		m_MinZ = float.MaxValue;
		m_MaxX = float.MinValue;
		m_MaxY = float.MinValue;
		m_MaxZ = float.MinValue;
	}

	private void AutoView()
	{
		m_ViewX = (m_MinX + m_MaxX) / 2f;
		m_ViewY = (m_MinY + m_MaxY) / 2f;
		m_ViewZ = m_MaxZ * 3f;
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
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Gray;
		base.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		base.Name = "Viewer3D";
		base.Size = new System.Drawing.Size(305, 284);
		base.DoubleClick += new System.EventHandler(Viewer3D_DoubleClick);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(Viewer3D_MouseDown);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(Viewer3D_MouseMove);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(Viewer3D_MouseUp);
		base.ResumeLayout(false);
	}
}
