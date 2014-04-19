using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Masa.ParticleEngine;
using Particle = Masa.ParticleEngine.ParticleEngine;

namespace Masa.IECBomb
{
	public class EffectManager
	{
		readonly GraphicsDevice Device;
		Particle particle;
		Random rand;
		Effect effect;

		public EffectManager(GraphicsDevice device)
		{
			rand = new Random();
			Device = device;
			int w = device.Viewport.Width;
			int h = device.Viewport.Height;
			effect = LoadEffect();
			particle = new Particle(effect, device, LoadTexture(), 4096, ParticleMode.TwoD, Matrix.CreateOrthographic(w, h, .1f, 100f), new Vector2(w, h));

			Run(EffectItem.RandomCreate(rand));
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
				particle.Make(new ParticleParameter(new Vector2(Device.Viewport.Width / 2, Device.Viewport.Height / 2),
					new Vector2(),
					new Vector2(),
					new Vector3(1, 0, 0),
					new Vector2(32, 32),
					new Vector2(),
					new Vector3(1, 1, 1)
					));
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
			Device.Clear(new Color(255, 0, count));
			SetEffectParams();
			particle.Draw();
		}

		void SetEffectParams()
		{
			effect.Parameters["Time"].SetValue(0);
			effect.Parameters["TargetSize"].SetValue(new Vector2(1f / Device.Viewport.Width, 1f / Device.Viewport.Height));
			effect.Parameters["Projection"].SetValue(particle.Projection);
			effect.Parameters["ViewProjection"].SetValue(Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up) * particle.Projection);
		}
	}
}
