using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Masa.ParticleEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EffectEditor
{
	public partial class XNAControl : Control
	{
		GraphicsDevice device;
		Effect effect;
		internal EffectProject EffectProject { get; private set; }
		public MainWindow Window { get; set; }

		public IEnumerable<PMIData> PMIDatas
		{
			get { return EffectProject.PMIDict.Values; }
		}

		public XNAControl()
		{
			InitializeComponent();
			Disposed += new EventHandler(XNAControl_Disposed);
		}

		void XNAControl_Disposed(object sender, EventArgs e)
		{
			DisposeDevice();
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);
		}


		public void Draw()
		{
			if (device == null) return;
			try
			{
				EffectProject.Update();
			}
			catch(Exception e)
			{
				ShowExceptionBox(e);
				StopEffect();
				return;
			}

			device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Util.FromWColor(Window.backgroundColor.SelectedColor), 1, 0);
			EffectProject.Draw();
			try
			{
				device.Present();
			}
			catch (DeviceLostException)
			{
				Reset();
				GC.Collect();
			}
		}

		protected override void OnCreateControl()
		{
			if (DesignMode == false)
			{
				InitDevice();
				LoadContent();
			}
			base.OnCreateControl();
		}

		void DisposeDevice()
		{
			if (device != null)
			{
				device.Dispose();
				device = null;
			}
		}

		void InitDevice()
		{
			var pp = new PresentationParameters()
			{
				BackBufferWidth = (int)this.Width,
				BackBufferHeight = (int)this.Height,
				DepthStencilFormat = DepthFormat.Depth24Stencil8,
				DeviceWindowHandle = this.Handle,
				RenderTargetUsage = RenderTargetUsage.PreserveContents,
				IsFullScreen = false,
				BackBufferFormat = SurfaceFormat.Color,

			};

			try
			{
				device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pp);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			//genPosition = new Vector2(200, 240);
		}

		public void ChangeSize(int width, int height)
		{
			Width = width;
			Height = height;
			Reset();
		}

		void Reset()
		{
			var dict = EffectProject.PMIDict;
			var texturePath = EffectProject.TexturePath;
			var scriptPath = this.ScriptPath;

			InitDevice();
			LoadContent();
			EffectProject.PMIDict = dict;

			TexturePath = texturePath;
			EffectProject.Reset();
			EffectProject.ScriptPath = scriptPath;
		}

		public string GetTextureFileName(string name)
		{
			return Path.Combine(GetAbsTexturePath(), name);
		}

		public string GetAbsTexturePath()
		{
			return Masa.Lib.Utility.ConvertAbsolutePath(Path.GetDirectoryName(Window.ProjectFileName), EffectProject.TexturePath);
		}

		void LoadContent()
		{
			EffectProject = new EffectProject(device, (s) => LoadTexture(GetTextureFileName(s)));
			ReloadContent();
		}

		/// <summary>
		/// デバイスロスト後のリロード
		/// </summary>
		void ReloadContent()
		{
			effect = new Effect(device, File.ReadAllBytes("particle.bin"));
			EffectProject.SetEffect(effect);
		}

		public static void ShowExceptionBox(Exception e)
		{
			//var box = new Microsoft.SqlServer.MessageBox.ExceptionMessageBox(e);
			//box.Show(new Handler());
			MessageBox.Show(e.ToString());
		}

		public void PlayScript(string lines)
		{
			string path = Path.GetTempFileName();
			File.WriteAllText(path, lines);
			try
			{
				EffectProject.PlayEffect(path);
			}
			catch (Exception e)
			{
				ShowExceptionBox(e);
			}
		}

		public void StopEffect()
		{
			EffectProject.StopEffect();
		}

		private void XNAControl_Paint(object sender, PaintEventArgs e)
		{
			Draw();
		}

		Texture2D LoadTexture(string fileName)
		{
			Stream file = null;
			try
			{
				file = File.OpenRead(fileName);
				var tex = Texture2D.FromStream(device, file);
				Color[] buffer = new Color[tex.Width * tex.Height];
				tex.GetData(buffer);
				tex.SetData(buffer.Select(c => c * (c.A / 255f)).ToArray());
				return tex;
			}
			catch (Exception)
			{
				return CreateDummyTexture();
			}
			finally
			{
				if (file != null)
				{
					file.Dispose();
				}
			}
		}

		Texture2D CreateDummyTexture()
		{
			var tex = new Texture2D(device, 1, 1);
			tex.SetData(new[] { Color.White });
			return tex;
		}

		public Action<string> onTextureLoaded;

		public PMIData AddParticleItem(string name, string texture, ushort mass, float r, float g, float b, float a, ParticleBlendMode blend, int layer)
		{
			return EffectProject.AddParticleManager(name, texture, mass, r, g, b, a, blend, layer);
		}

		public PMIData AddDefaultParticleItem(string textureName)
		{
			float c = 255f / 256f;
			c = 1;
			return AddParticleItem(Path.GetFileNameWithoutExtension(textureName), textureName, 256, c, c, c, c, ParticleBlendMode.Add, 0);
		}

		public void UpdateParticleItem(string baseName, string newName, PMIData item)
		{
			EffectProject.UpdateParticleManager(baseName, newName, item);
		}

		public void ResetParticle()
		{
			if (EffectProject != null)
				EffectProject.Reset();
		}

		public void RemovePartilceItem(string name)
		{
			EffectProject.RemoveParticleManager(name);
		}

		public string TexturePath
		{
			get { return EffectProject.TexturePath; }
			set { EffectProject.TexturePath = value; }
		}

		public string ScriptPath
		{
			get { return EffectProject.ScriptPath; }
			set { EffectProject.ScriptPath = value; }
		}

		public void SaveProject(string name)
		{
			EffectProject.SaveToFile(name);
		}

		public void OpenProject(string name)
		{
			EffectProject.Load(name);
		}

		public void InitProject()
		{
			LoadContent(); 
		}
	}

	class Handler : System.Windows.Forms.IWin32Window
	{
		public IntPtr Handle
		{
			get { return MainWindow.WindowHandle; }
		}
	}
}
