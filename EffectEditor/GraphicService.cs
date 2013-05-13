using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EffectEditor
{
	class GraphicService : IServiceProvider, IGraphicsDeviceService
	{
		GraphicsDevice device;

		public GraphicService(GraphicsDevice d)
		{
			device = d;
			
		}

		public object GetService(Type serviceType)
		{
			return this;
		}

		public event EventHandler<EventArgs> DeviceCreated;

		public event EventHandler<EventArgs> DeviceDisposing;

		public event EventHandler<EventArgs> DeviceReset;

		public event EventHandler<EventArgs> DeviceResetting;

		public GraphicsDevice GraphicsDevice
		{
			get { return device; }
		}
	}
}
