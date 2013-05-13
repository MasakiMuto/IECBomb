using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace EffectEditor
{
	/// <summary>
	/// XNADrawer.xaml の相互作用ロジック
	/// </summary>
	public partial class XNADrawer : System.Windows.Controls.UserControl
	{
		GraphicsDevice Device;
		Effect effect;
		
		public XNADrawer()
		{
			InitializeComponent();
		}

		public void Draw()
		{
			Device.Clear(Color.Black);
			Device.Present(null, null, MainWindow.WindowHandle);
		}

		~XNADrawer()
		{
			DisposeDevice();
		}

		private void XNADrawerArea_Unloaded(object sender, RoutedEventArgs e)
		{
			DisposeDevice();
		}

		void DisposeDevice()
		{
			if (Device != null)
			{
				Device.Dispose();
				Device = null;
			}
		}

		private void XNADrawerArea_Initialized(object sender, EventArgs e)
		{
			base.OnInitialized(e);
			var pp = new PresentationParameters()
			{
				BackBufferWidth = (int)this.ActualWidth,
				BackBufferHeight = (int)this.ActualHeight,
				DepthStencilFormat = DepthFormat.None,
				//DeviceWindowHandle = this.Parent.
			};
			Device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pp);
			effect = new Effect(Device, System.IO.File.ReadAllBytes("particle.fx"));
		}

		
	}
}
