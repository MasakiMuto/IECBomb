using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Masa.IECBomb
{
	public partial class ParticleControl : UserControl
	{
		EffectManager effect;
		GraphicsDevice device;

		public ParticleControl()
		{
			InitializeComponent();
			InitDevice();
			effect = new EffectManager(device);
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			
		
		}

		void InitDevice()
		{
			device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, new PresentationParameters() 
			{
				BackBufferWidth = Width,
				BackBufferHeight = Height,
				BackBufferFormat = SurfaceFormat.Color,
				DepthStencilFormat = DepthFormat.Depth24Stencil8,
				RenderTargetUsage = Microsoft.Xna.Framework.Graphics.RenderTargetUsage.PreserveContents,
				IsFullScreen = false,
				DeviceWindowHandle = this.Handle
			});
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (effect != null)
			{
				effect.Update();
				effect.Draw();
			}
			device.Present();
		}
	}
}
