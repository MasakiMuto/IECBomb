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
using System.Media;

namespace IECSound
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		SynthParam currentParam;
		Manager manager;

		public MainWindow()
		{
			InitializeComponent();
			manager = new Manager();
			soundTypeList.ItemsSource = Enum.GetValues(typeof(SoundType)).OfType<SoundType>()
				.Select(x => CreateItem(x));
		}

		ListBoxItem CreateItem(SoundType x)
		{
			var item = new ListBoxItem()
			{
				Content = x
			};
			item.AddHandler(ListBoxItem.MouseLeftButtonDownEvent, new RoutedEventHandler(ItemSelect), true);
			return item;
		}

		void ItemSelect(object sender, RoutedEventArgs e)
		{
			var name = (SoundType)(sender as ListBoxItem).Content;
			PlaySound(name);
		}

		void PlaySound(SoundType type)
		{
			currentParam = SynthParam.Init((SoundType)type);
			using (var s = new SynthEngine())
			{
				var player = s.SynthFile(currentParam);
				player.Play();
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (currentParam != null)
			{
				manager.Start(currentParam);
			}
		}

	}
}
