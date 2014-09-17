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
		public GAManager Manager { get; private set; }
		SoundItemControl[] soundControls;

		public MainWindow()
		{
			InitializeComponent();
			Manager = new GAManager();
			soundTypeList.ItemsSource = Enum.GetValues(typeof(SoundType)).OfType<SoundType>()
				.Select(x => CreateItem(x));
			soundControls = Enumerable.Range(0, 9)
				.Select(i =>
				{
					var item = new SoundItemControl(i, this);
					item.SetValue(Grid.RowProperty, i / 3);
					item.SetValue(Grid.ColumnProperty, i % 3);
					return item;
				})
				.ToArray();
			foreach (var item in soundControls)
			{
				grid.Children.Add(item);
			}
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

		private void StartButtonClick(object sender, RoutedEventArgs e)
		{
			if (currentParam != null)
			{
				Manager.Start(currentParam);
			}
		}

		private void SaveButtonClick(object sender, RoutedEventArgs e)
		{
			var target = soundControls.FirstOrDefault(x => x.IsChecked);
			if (target != null)
			{
				Manager.Save(target.Index);
			}
		}

		private void NextButtonClick(object sender, RoutedEventArgs e)
		{
			Manager.Update(soundControls.Where(x => x.IsChecked).Select(x=>x.Index));
		}

	}
}
