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
		Manager manager;

		public IECWindow()
		{
			InitializeComponent();
			timer = new System.Windows.Threading.DispatcherTimer()
			{
				Interval = new TimeSpan(167),
			};
			timer.Tick += timer_Tick;
			manager = new Manager();
			RedirectOutput();
		}

		void RedirectOutput()
		{
			System.Diagnostics.Debug.AutoFlush = true;
			System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("log.txt"));
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

		void PlayButtonClick(object sender, EventArgs e)
		{
			manager.Play();
		}

		void ResetButtonClick(object sender, EventArgs e)
		{
			manager.Reset();
			ParticleControl.EffectManager.Clear();
		}

		void UpdateButtonClick(object sender, EventArgs e)
		{
			ItemPool.Pool.UpdateGeneration();
			Generation.Content = ItemPool.Pool.Generation;
			MaxScore.Content = ItemPool.Pool.GetMaxScore();
			//Console.WriteLine(MaxScore.Content);
			System.Diagnostics.Debug.WriteLine(MaxScore.Content);
		}
	}
}
