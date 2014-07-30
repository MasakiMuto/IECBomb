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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IECSound
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			var p = new IECSynth.SynthParam();
			p.base_freq = 0.4f;
			p.env_attack = 0f;
			p.env_sustain = .05f;
			p.env_decay = .2f;
			p.env_punch = .4f;

			p.lpf_freq = 1f;

			using(var s = new IECSynth.SynthEngine())
			{
				s.SynthFile(p);
			}
			
			
		}
	}
}
