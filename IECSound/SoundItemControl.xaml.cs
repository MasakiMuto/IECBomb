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
	/// ItemControl.xaml の相互作用ロジック
	/// </summary>
	public partial class SoundItemControl : UserControl
	{
		public int Index { get; private set; }
		readonly MainWindow window;
		public bool IsChecked { get { return check.IsChecked.GetValueOrDefault(false); } }

		public SoundItemControl(int index, MainWindow window)
		{
			Index = index;
			this.window = window;
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			window.Manager.Play(Index);
			//(MainWindow)
		}
	}
}
