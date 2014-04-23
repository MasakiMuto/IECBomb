using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Masa.IECBomb
{
	/// <summary>
	/// IECWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class IECWindow : Window
	{
		System.Windows.Threading.DispatcherTimer timer;

		public IECWindow()
		{
			InitializeComponent();
			timer = new System.Windows.Threading.DispatcherTimer()
			{
				Interval = new TimeSpan(167),
			};
			timer.Tick += timer_Tick;
		}

		void timer_Tick(object sender, EventArgs e)
		{
			ParticleControl.Draw();
			//windowsFormsHost1.InvalidateVisual();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			timer.Start();
		}

		protected override void OnClosed(EventArgs e)
		{
			timer.Stop();
			base.OnClosed(e);
		}
	}
}
