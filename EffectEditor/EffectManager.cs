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
		Particle[] particle;
		Random rand;
		Effect effect;

		Matrix view;

		Viewport[] viewports;

		readonly int Width, Height;

		public EffectManager(GraphicsDevice device)
		{
			Instance = this;
			Width = device.Viewport.Width / 2;
			Height = device.Viewport.Height;
			viewports = new[]
				{
					new Viewport(0, 0, Width, Height),
					new Viewport(Width, 0, Width, Height)
				};
			rand = new Random();
			Device = device;
			effect = LoadEffect();
			particle = Enumerable.Range(0, 2)
				.Select(x=> new Particle(effect, device, LoadTexture(), 4096, ParticleMode.TwoD, Matrix.CreateOrthographic(Width, Height, .1f, 100f), new Vector2(Width, Height))).ToArray();

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

		public void Run(EffectItem item, int pos)
		{
			particle[pos].Clear();
			var n = (int)item[ParameterName.Mass];
			for (int i = 0; i < n; i++)
			{
				particle[pos].Make(item.CreateParticleParameter(rand, new Vector2(0, 0)));
			}
		}

		public void Update()
		{
			Array.ForEach(particle, x => x.Update());
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
			for (int i = 0; i < particle.Length; i++)
			{
				Device.Viewport = viewports[i];
				particle[i].Draw(view);
			}
		}

		public void Clear()
		{
			Array.ForEach(particle, x => x.Clear());
			count = 0;
		}

		void SetEffectParams()
		{
			effect.Parameters["Offset"].SetValue(new Vector2(Width, Height) * .5f);
		}
	}
}
