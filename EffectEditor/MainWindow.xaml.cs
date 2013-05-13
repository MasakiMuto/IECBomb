using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace EffectEditor
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		public static IntPtr WindowHandle;
		StringWriter ErrorLogger;
		DispatcherTimer timer;
		FileManager projectFileManager, scriptFileManager;

		//[System.Runtime.InteropServices.DllImport("User32.dll")]
		//static extern IntPtr SendMessage(IntPtr hWnnd, int msg, int wParam, int[] lParam);

		public MainWindow()
		{
			InitializeComponent();
			//var src = System.Windows.Interop.HwndSource.FromVisual(scriptCode);
			//SendMessage(((System.Windows.Interop.HwndSource)src).Handle, 0x00cb, 1, new int[] { 16 });
			timer = new DispatcherTimer();
			timer.Interval = new TimeSpan(167);//16.7ms
			timer.Tick += new EventHandler(timer_Tick);
			scriptCode.CommandBindings.AddRange(new[]
				{
					new CommandBinding(ApplicationCommands.New, NewFile),
					new CommandBinding(ApplicationCommands.Save, (o,e)=>scriptFileManager.Save()),
					new CommandBinding(ApplicationCommands.SaveAs, (o,e)=>scriptFileManager.SaveAs()),
					new CommandBinding(ApplicationCommands.Open, OpenScript),
				});
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (o, e) => this.Close()));
			//XNAControl.onTextureLoaded += (s) => textureListBox.Items.Add(System.IO.Path.GetFileNameWithoutExtension(s));
			ErrorLogger = new StringWriter();
			var error = new System.Diagnostics.TextWriterTraceListener(ErrorLogger);
			//var op = (System.Diagnostics.DefaultTraceListener)System.Diagnostics.Debug.Listeners[0];
			//op.
			projectFileManager = new FileManager(".efprj", "Masa Effect Project|*.efprj|All Files|*.*");
			projectFileManager.Newed += new Action(projectFileManager_Newed);
			projectFileManager.Opened += OpenProject;
			projectFileManager.Saved += SaveProject;

			scriptFileManager = new FileManager(".mss", "MaSa Script|*.mss|All Files|*.*");
			scriptFileManager.Newed += new Action(scriptFileManager_Newed);
			scriptFileManager.Opened += LoadScript;
			scriptFileManager.Saved += s => SaveScript(s);

			blendModeSelector.ItemsSource = Enum.GetValues(typeof(Masa.ParticleEngine.ParticleBlendMode));
			System.Diagnostics.Debug.Listeners.Add(error);
			var last = LoadLatestProjects();
			//if (last != null)
			//{
			//	projectFileManager.Open(last);
			//}
			textureColor.SelectedColorChanged += textureColor_SelectedColorChanged;
		}

		void textureColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
		{
			(texturePreviewImage.Effect as ShaderEffectLibrary.MonochromeEffect).FilterColor = e.NewValue;
		}


		void scriptFileManager_Newed()
		{
			scriptCode.Text = "";
			playExitButton_Click(this, null);
			SetStatus("NewFile");
		}

		void projectFileManager_Newed()
		{
			SetStatus("New Project");
		}


		#region LatestItem

		readonly string HistoryPath = "history.xml";
		readonly string ProjectTag = "ProjectHistory";

		string LoadLatestProjects()
		{
			string ret = null;
			if (File.Exists(HistoryPath))
			{
				try
				{
					using (var str = File.OpenRead(HistoryPath))
					{
						XDocument xml = XDocument.Load(str);
						var items = xml.Root.Element(ProjectTag).Elements("item");
						latestProjects.ItemsSource = items
							.Select(i =>
							{
								var item = new MenuItem();
								item.Click += (obj, e) => projectFileManager.Open((string)item.Header);
								//item.Click += (obj, e) => OpenProject((string)item.Header);
								item.Header = i.Value;
								return item;
							});
						var first = items.FirstOrDefault();
						if (first != null)
						{
							ret = first.Value;
						}

					}
				}
				catch
				{
				}
			}
			return ret;
		}

		void SaveLatestProjects(string fileName)
		{
			XDocument xml;

			if (File.Exists(HistoryPath))
			{
				using (var str = File.OpenRead(HistoryPath))
				{
					xml = XDocument.Load(str);
				}
			}
			else
			{
				xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
				xml.Add(new XElement("history"));
			}
			var prj = xml.Root.Element(ProjectTag);
			if (prj == null)
			{
				prj = new XElement(ProjectTag);
				xml.Root.Add(prj);
			}
			var old = prj.Elements("item").FirstOrDefault(i => i.Value == fileName);

			if (old != null)
			{
				old.Remove();
			}
			prj.AddFirst(new XElement("item", fileName));

			xml.Save(HistoryPath);
			LoadLatestProjects();
		}

		public override void EndInit()
		{
			base.EndInit();
		}

		#endregion

		public string Code { get; set; }

		void timer_Tick(object sender, EventArgs e)
		{
			XNAControl.Draw();
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			WindowHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
			//new System.Windows.Interop.HwndSource(
			itemsList.ItemsSource = XNAControl.PMIDatas;
			
			timer.Start();
			if (latestProjects.Items.Count > 0)
			{
				projectFileManager.Open(latestProjects.Items.OfType<MenuItem>().First().Header as string);
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			timer.Stop();
			base.OnClosed(e);

		}

		private void exit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}


		private void playButton_Click(object sender, RoutedEventArgs e)
		{
			XNAControl.PlayScript(scriptCode.Text);
		}

		private void playExitButton_Click(object sender, RoutedEventArgs e)
		{
			XNAControl.StopEffect();
		}

		void RefreshButtonClick(object sender, RoutedEventArgs e)
		{
			XNAControl.ResetParticle();
		}

		#region ScriptCodeFile

		void OpenScript(Object sender, ExecutedRoutedEventArgs e)
		{
			scriptFileManager.Open();
		}

		void LoadScript(string fileName)
		{
			if (fileName != null)
			{
				scriptCode.Text = File.ReadAllText(fileName);
				SetStatus("Script Loaded : " + fileName);
			}
		}

		private void scriptCode_TextChanged(object sender, TextChangedEventArgs e)
		{
			SetStatus("Ready...");
			scriptFileManager.Changed = true;
			//changed = true;
		}

		void NewFile(object sender, ExecutedRoutedEventArgs e)
		{
			//if (!ShowNotSavedAlert())
			//{
			//    return;
			//}
			scriptFileManager.New();
			scriptCode.Text = "";
			playExitButton_Click(this, null);
			SetStatus("NewFile");
		}

		/// <summary>
		/// scriptFieManager.Saveから呼ばれる
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		bool SaveScript(string name)
		{
			try
			{
				File.WriteAllText(name, scriptCode.Text);
				SetStatus("Script Saved : " + name);
				return true;
			}
			catch (Exception e)
			{
				ShowMessage("保存に失敗しました:" + e.Message);
				return false;
			}
		}

		void ShowMessage(string text)
		{
			MessageBox.Show(text);
		}

		bool ShowNotSavedAlert()
		{
			var res = MessageBox.Show((scriptFileManager.FileName == null ? "新規ファイル" : System.IO.Path.GetFileName(scriptFileManager.FileName)) + "は変更が保存されていません。保存せずに続行しますか?", "確認",
				MessageBoxButton.YesNoCancel);
			if (res == MessageBoxResult.Yes)
			{
				return true;
			}
			if (res == MessageBoxResult.Cancel)
			{
				return false;
			}
			if (res == MessageBoxResult.No)
			{
				return scriptFileManager.Save();
			}
			return false;
		}


		#endregion

		#region ScriptFormat

		private void scriptCode_KeyDown(object sender, KeyEventArgs e)
		{

		}


		private void scriptCode_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			//if (e.Key == Key.Enter)
			//{

			//}
			//if (e.Key == Key.Tab)
			//{
			//    e.Handled = true;
			//    //scriptCode.Text += Tab;
			//    //scriptCode.CaretIndex += 4;
			//    //scriptCode.
			//    for (int i = 0; i < 4; i++)
			//    {
			//        var arg = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.Space)
			//            {
			//                Source = sender,
			//                RoutedEvent = e.RoutedEvent,
			//            };

			//        //scriptCode.RaiseEvent(arg);
			//    }
			//}
			////Keyboard.PrimaryDevice.

		}

		#endregion

		private void OpenTextureClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog()
			{
				DefaultExt = ".png",
				CheckPathExists = true,
				InitialDirectory = XNAControl.TexturePath

			};
			if (dlg.ShowDialog() == true)
			{
				texturePathText.Text = System.IO.Path.GetFileName(dlg.FileName);
				SetStatus("Texture Path : " + dlg.FileName);
				SetTextureList();
			}
		}

		private void addItemButtonClick(object sender, RoutedEventArgs e)
		{
			var tex = textureList.SelectedItem as string;
			if (tex == null)
			{
				return;
			}
			var item = XNAControl.AddDefaultParticleItem(tex + ".png");
			projectFileManager.Changed = true;
			UpdatePMIDatas();
			UpdatePMIProperty();
			//var item = AddItem();
			//if (item != null)
			//{
			//	//itemsList.Items.Add(item);
			//	UpdatePMIDatas();
			//}
		}

		Masa.ParticleEngine.PMIData AddItem()
		{
			string name = itemNameText.Text;
			if (String.IsNullOrWhiteSpace(name)) return null;
			string texture = texturePathText.Text;
			if (String.IsNullOrWhiteSpace(texture)) return null;
			projectFileManager.Changed = true;
			return XNAControl.AddParticleItem(name, texture, (ushort)particleMassNumber.Value,
				textureColor.R / 256f, textureColor.G / 256f,
				textureColor.B / 256f, textureColor.A / 256f,
				(Masa.ParticleEngine.ParticleBlendMode)blendModeSelector.SelectedItem,
				layerNumber.Value.HasValue ? layerNumber.Value.Value : 0);
		}

		void UpdatePMIItem(Masa.ParticleEngine.PMIData data)
		{

			data.Mass = (ushort)particleMassNumber.Value;
			data.TextureName = texturePathText.Text;
			data.Color = FromWColor(textureColor.SelectedColor).ToVector4();
			data.Layer = layerNumber.Value.HasValue ? layerNumber.Value.Value : 0;
			data.Blend = (Masa.ParticleEngine.ParticleBlendMode)blendModeSelector.SelectedItem;
			XNAControl.UpdateParticleItem(data.Name, itemNameText.Text, data);
			projectFileManager.Changed = true;
		}

		void UpdatePMIDatas()
		{
			//int last = itemsList.SelectedIndex;
			itemsList.ItemsSource = XNAControl.PMIDatas;
			itemsList.Items.Refresh();
			//itemsList.SelectedIndex = last;
			//UpdatePMIPropery();
			//itemsList.Items.Refresh();
		}

		void UpdatePMIItem()
		{
			var item = this.itemsList.SelectedItem as Masa.ParticleEngine.PMIData;
			if (item != null)
			{
				UpdatePMIItem(item);
			}
		}

		#region ProjectFile

		void openProjectClick(object sender, RoutedEventArgs e)
		{
			projectFileManager.Open();
		}

		void OpenProject(string fileName)
		{
			try
			{
				XNAControl.OpenProject(fileName);
				UpdatePMIDatas();
				SetTextureList();
				SetStatus("Project Loaded : " + fileName);
			}
			catch (Exception e)
			{
				EffectEditor.XNAControl.ShowExceptionBox(e);
			}

		}

		void SaveProject(string fileName)
		{
			//UpdatePMIItem();
			XNAControl.SaveProject(fileName);
			SaveLatestProjects(fileName);
			SetStatus("Project Saved : " + fileName);
		}

		void saveProjectClick(object sender, RoutedEventArgs e)
		{
			UpdatePMIItem();
			projectFileManager.Save();
		}

		void saveAsProjectClick(object sender, RoutedEventArgs e)
		{
			UpdatePMIItem();
			projectFileManager.SaveAs();
		}


		void setTexturePathClick(object sender, RoutedEventArgs e)
		{

			var dlg = new VistaFolderBrowserDialog()
			{
				Description = "Select Texture Directory",
				ShowNewFolderButton = false
			};
			dlg.ShowDialog();
			XNAControl.TexturePath = dlg.SelectedPath;
			projectFileManager.Changed = true;
		}

		void removeButtonClick(object sender, RoutedEventArgs e)
		{
			var item = itemsList.SelectedItem;
			if (item != null)
			{
				XNAControl.RemovePartilceItem(item.ToString());
				UpdatePMIDatas();
				projectFileManager.Changed = true;
			}
		}

		#endregion

		private void itemsList_SourceUpdated(object sender, DataTransferEventArgs e)
		{
			UpdatePMIProperty();
		}

		private void itemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = e.RemovedItems.OfType<Masa.ParticleEngine.PMIData>().FirstOrDefault();
			if (item != null)
			{
				UpdatePMIItem(item);
				UpdatePMIDatas();
			}
			//var item = AddItem();
			//if (item != null)
			//{
			//	UpdatePMIDatas();
			//}
			var n = e.AddedItems.OfType<Masa.ParticleEngine.PMIData>().FirstOrDefault();
			if (n != null)
			{
				UpdatePMIProperty(n);
			}
		}

		void UpdatePMIProperty(Masa.ParticleEngine.PMIData pmi)
		{
			itemNameText.Text = pmi.Name;
			texturePathText.Text = pmi.TextureName;
			particleMassNumber.Value = pmi.Mass;
			blendModeSelector.SelectedItem = pmi.Blend;
			textureColor.SelectedColor = FromXColor(pmi.Color);
			layerNumber.Value = pmi.Layer;
			try
			{
				var image = new System.Windows.Media.Imaging.BitmapImage(new Uri(this.XNAControl.GetTextureFileName(pmi.TextureName)));
				(texturePreviewImage.Effect as global::ShaderEffectLibrary.MonochromeEffect).FilterColor = FromXColor(pmi.Color);
				texturePreviewImage.Source = image.Clone();
				image = null;
				GC.Collect();
			}
			catch { }
			SetTextureList();
		}

		void UpdatePMIProperty()
		{
			var pmi = itemsList.SelectedItem as Masa.ParticleEngine.PMIData;
			if (pmi == null) return;
			UpdatePMIProperty(pmi);
		}

		public void SetTextureList()
		{
			try
			{
				textureList.ItemsSource = System.IO.Directory.EnumerateFiles(XNAControl.TexturePath)
					.Select(i => System.IO.Path.GetFileNameWithoutExtension(i));
			}
			catch { }

		}

		void SetStatus(string txt)
		{
			statusLabel.Content = txt;
		}


		Color FromXColor(Microsoft.Xna.Framework.Vector4 col)
		{
			Func<float, byte> convert = f => (byte)(f * 255);
			return new Color()
			{
				A = convert(col.W),
				R = convert(col.X),
				G = convert(col.Y),
				B = convert(col.Z)
			};
		}

		Microsoft.Xna.Framework.Color FromWColor(Color col)
		{
			return new Microsoft.Xna.Framework.Color(col.R, col.G, col.B, col.A);
		}

		private void scriptCode_Drop(object sender, DragEventArgs e)
		{
			string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
			if (files != null)
			{
				scriptFileManager.Open(files[0]);
				//LoadScript(files[0]);
			}
		}

		private void scriptCode_PreviewDragOver(object sender, DragEventArgs e)
		{
			e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
		}

		private void window_Activated(object sender, EventArgs e)
		{
			if (timer != null)
			{
				timer.Start();
			}
		}

		private void window_Deactivated(object sender, EventArgs e)
		{
			if (timer != null)
			{
				timer.Stop();
			}
		}

		private void is2DCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			EffectProject.Is2D = true;
		}

		private void is2DCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			EffectProject.Is2D = false;
		}

		private void controlSizeChanged(object sender, RoutedEventArgs e)
		{
			int width = controlWidth.Value.GetValueOrDefault(controlWidth.DefaultValue.Value);
			int height = controlHeight.Value.GetValueOrDefault(controlHeight.Value.Value);
			windowsFormsHost1.Width = width;
			windowsFormsHost1.Height = height;
			XNAControl.ChangeSize(width, height);
			UpdateLayout();

			//XNAControl.Width = controlWidth.Value.GetValueOrDefault(controlWidth.DefaultValue.Value);
			//XNAControl.Height = controlHeight.Value.GetValueOrDefault(controlHeight.Value.Value);
			//XNAControl.ResetParticle();

		}

		private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (scriptFileManager.TryClose() && projectFileManager.TryClose())
			{
				return;
			}
			else
			{
				e.Cancel = true;
			}

		}

		private void textureList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			var image = new System.Windows.Media.Imaging.BitmapImage(new Uri(XNAControl.GetTextureFileName(textureList.SelectedItem as string + ".png")));
			listTexturePreview.Source = image.Clone();
			image = null;
			GC.Collect();
			
		}




	}


}
