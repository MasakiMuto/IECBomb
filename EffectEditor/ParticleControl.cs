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
		public EffectManager EffectManager { get; private set; }
		GraphicsDevice device;

		public ParticleControl()
		{
			InitializeComponent();
			InitDevice();
			EffectManager = new EffectManager(device);
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

		
		public void Draw()
		{
			if (EffectManager != null)
			{
				EffectManager.Update();
				EffectManager.Draw();
			}
			device.Present();
		
		}
	}
}
