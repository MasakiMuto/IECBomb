using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Masa.ParticleEngine;
using Particle = Masa.ParticleEngine.ParticleEngine;
using Masa.Lib.XNA;

namespace Masa.IECBomb
{
	/// <summary>
	/// 再生器
	/// </summary>
	public class EffectManager
	{
		public static EffectManager Instance { get; private set; }

		readonly GraphicsDevice Device;
		Particle particle;
		Random rand;
		Effect effect;

		Matrix view;

		public EffectManager(GraphicsDevice device)
		{
			Instance = this;
			rand = new Random();
			Device = device;
			int w = device.Viewport.Width;
			int h = device.Viewport.Height;
			effect = LoadEffect();
			particle = new Particle(effect, device, LoadTexture(), 4096, ParticleMode.TwoD, Matrix.CreateOrthographic(w, h, .1f, 100f), new Vector2(w, h));

			view = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
		}

		Effect LoadEffect()
		{
			return new Effect(Device, System.IO.File.ReadAllBytes("particle.bin"));
		}

		Texture2D LoadTexture()
		{
			using(var file = System.IO.File.OpenRead("white2.png"))
			{
				var tex = Texture2D.FromStream(Device, file);
				Color[] buffer = new Color[tex.Width * tex.Height];
				tex.GetData(buffer);
				tex.SetData(buffer.Select(c => c * (c.A / 255f)).ToArray());
				return tex;
			}
		}

		public void Run(EffectItem item)
		{
			var n = (int)item[ParameterName.Mass];
			for (int i = 0; i < n; i++)
			{
				particle.Make(item.CreateParticleParameter(rand));
			}
		}

		public void Update()
		{
			particle.Update();
		}

		int count;

		public void Draw()
		{
			count++;
			Device.Clear(new Color(0, 0, 0));
			SetEffectParams();
			Device.DepthStencilState = DepthStencilState.None;
			Device.RasterizerState = RasterizerState.CullNone;
			Device.BlendState = BlendState.Additive;
			particle.Draw(view);
		}

		void SetEffectParams()
		{
			effect.Parameters["Offset"].SetValue(new Vector2(Device.Viewport.Width, Device.Viewport.Height) * .5f);
		}
	}
}
