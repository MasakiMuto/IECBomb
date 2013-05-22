﻿using System;
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
		EffectProject manager;

		public IEnumerable<PMIData> PMIDatas
		{
			get { return manager.PMIDict.Values; }
		}

		public XNAControl()
		{
			InitializeComponent();
			Disposed += new EventHandler(XNAControl_Disposed);
		}

		void XNAControl_Disposed(object sender, EventArgs e)
		{
			//SaveTextureList();
			DisposeDevice();
		}

		protected override void OnPaint(PaintEventArgs pe)
		{
			base.OnPaint(pe);
		}


		public void Draw()
		{
			if (device == null) return;
			manager.Update();
			device.Clear(Color.Black);
			manager.Draw();
			device.Present();
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
				//device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pp);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			//genPosition = new Vector2(200, 240);
		}

		public void ChangeSize(int width, int height)
		{
			var dict = manager.PMIDict;
			var texturePath = manager.TexturePath;

			Width = width;
			Height = height;
			InitDevice();
			LoadContent();
			manager.TexturePath = TexturePath;
			//ReloadContent();
			ResetParticle();
			manager.PMIDict = dict;
			manager.TexturePath = texturePath;
			manager.Reset();
		}

		public string GetTextureFileName(string name)
		{
			return Path.Combine(manager.TexturePath, name);
		}

		void LoadContent()
		{
			//effect = new Effect(device, File.ReadAllBytes("particle.bin"));
			//manager = null;
			manager = new EffectProject(device, (s) => LoadTexture(GetTextureFileName(s)));
			//manager.SetEffect(effect);
			ReloadContent();
		}

		/// <summary>
		/// デバイスロスト後のリロード
		/// </summary>
		void ReloadContent()
		{
			effect = new Effect(device, File.ReadAllBytes("particle.bin"));
			manager.SetEffect(effect);

		}


		public static void ShowExceptionBox(Exception e)
		{
			var box = new Microsoft.SqlServer.MessageBox.ExceptionMessageBox(e);
			box.Show(new Handler());
		}

		public void PlayScript(string lines)
		{
			string path = Path.GetTempFileName();
			//string key = script.PathToKey(path);
			File.WriteAllText(path, lines);
			try
			{
				manager.PlayEffect(path);
			}
			catch (Exception e)
			{
				ShowExceptionBox(e);
				//Console.WriteLine(e.Message);
			}
		}

		public void StopEffect()
		{
			manager.StopEffect();
			//effectManager.ClearParticle();
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

				var tex = new Texture2D(device, 1, 1);
				tex.SetData(new[] { Color.White });
				return tex;
			}
			finally
			{
				if (file != null)
				{
					file.Dispose();
				}
			}
			
		}

		public Action<string> onTextureLoaded;

		public PMIData AddParticleItem(string name, string texture, ushort mass, float r, float g, float b, float a, ParticleBlendMode blend, int layer)
		{
			return manager.AddParticleManager(name, texture, mass, r, g, b, a, blend, layer);
		}

		public PMIData AddDefaultParticleItem(string textureName)
		{
			float c = 255f / 256f;
			c = 1;
			return AddParticleItem("name", textureName, 256, c, c, c, c, ParticleBlendMode.Add, 0);
		}

		public void UpdateParticleItem(string baseName, string newName, PMIData item)
		{
			manager.UpdateParticleManager(baseName, newName, item);
		}

		public void ResetParticle()
		{
			if (manager != null)
				manager.Reset();
		}

		public void RemovePartilceItem(string name)
		{
			manager.RemoveParticleManager(name);
		}

		public string TexturePath
		{
			get { return manager.TexturePath; }
			set { manager.TexturePath = value; }
		}

		public void SaveProject(string name)
		{
			manager.SaveToFile(name);
		}

		public void OpenProject(string name)
		{
			manager.Load(name);
		}
	}

	//public class ListTexture
	//{
	//	public readonly Texture2D Texture;
	//	public readonly string Name;
	//	public readonly string FileName;
	//	public Color Color;

	//	public ListTexture(Texture2D tex, string file)
	//	{
	//		Texture = tex;
	//		Name = Path.GetFileNameWithoutExtension(file);
	//		FileName = file;
	//		Color = Color.White;
	//	}
	//}

	class Handler : System.Windows.Forms.IWin32Window
	{
		public IntPtr Handle
		{
			get { return MainWindow.WindowHandle; }
		}
	}
}