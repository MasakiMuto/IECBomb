using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Masa.ParticleEngine;
using Ookii.Dialogs.Wpf;

namespace EffectEditor
{
	using PMIDict = Dictionary<string, PMIData>;

	class ProjectControl
	{
		MainWindow window;
		XNAControl XnaControl { get { return window.XNAControl; } }
		FileManager projectFileManager;
		PMIDict lastState;
		public string ProjectFileName { get { return projectFileManager.FileName; } }


		public ProjectControl(MainWindow window)
		{
			this.window = window;
			projectFileManager = new FileManager(".efprj", "Masa Effect Project|*.efprj|All Files|*.*");
			projectFileManager.Newed += projectFileManager_Newed;
			projectFileManager.Opened += ProjectOpened;
			projectFileManager.Saved += SaveProject;
			projectFileManager.ChangeComparer = () => lastState != null && !lastState.SequenceEqual(GetCurrentParticleItems(), p=>p.Value);
		}

		PMIDict GetCurrentParticleItems()
		{
			if (XnaControl.EffectProject == null)
			{
				return null;
			}
			return XnaControl.EffectProject.PMIDict;
		}

		public void OpenProject(string name)
		{
			projectFileManager.Open(name);
		}

		public void Changed()
		{
			projectFileManager.Changed = true;
		}

		
		public void NewProject()
		{
			projectFileManager.New();
		}

		public void OpenProjectClick()
		{
			projectFileManager.Open();
		}

		public void SaveProject()
		{
			UpdatePMIDatas();
			projectFileManager.Save();
		}

		public void SaveAsProject()
		{
			UpdatePMIDatas();
			projectFileManager.SaveAs();
		}

		public void SetTexturePath()
		{
			var dlg = new VistaFolderBrowserDialog()
			{
				Description = "Select Texture Directory",
				ShowNewFolderButton = false
			};
			dlg.ShowDialog();
			XnaControl.TexturePath = dlg.SelectedPath;
			Changed();
		}

		public void RemoveParticleItem(PMIData item)
		{
			if (item == null)
			{
				return;
			}
			XnaControl.RemovePartilceItem(item.ToString());
			UpdatePMIDatas();
			Changed();
		}

		void SetLastState()
		{
			var cur = GetCurrentParticleItems();
			if (cur == null)
			{
				lastState = null;
				return;
			}
			lastState = new PMIDict(cur.Count);
			foreach (var item in cur)
			{
				lastState[item.Key] = item.Value.Clone();
			}
			//lastState = new PMIDict(GetCurrentParticleItems());
		}

		void projectFileManager_Newed()
		{
			XnaControl.InitProject();
			UpdatePMIDatas();
			SetTextureList();
			window.SetStatus("New Project");
			SetLastState();
			//projectFileManager.SaveAs();
		}

		void ProjectOpened(string fileName)
		{
			try
			{
				XnaControl.OpenProject(fileName);
				UpdatePMIDatas();
				SetTextureList();
				window.SetStatus("Project Loaded : " + fileName);
				SetLastState();
			}
			catch (Exception e)
			{
				EffectEditor.XNAControl.ShowExceptionBox(e);
			}

		}

		void SaveProject(string fileName)
		{
			//UpdatePMIItem();
			XnaControl.SaveProject(fileName);
			SaveLatestProjects(fileName);
			window.SetStatus("Project Saved : " + fileName);
			SetLastState();
		}

		void UpdatePMIDatas()
		{
			window.UpdateParticleItemList();
		}

		public void SetTextureList() 
		{
			try
			{
				window.textureList.ItemsSource = System.IO.Directory.EnumerateFiles(XnaControl.TexturePath)
					.Select(i => System.IO.Path.GetFileNameWithoutExtension(i));
			}
			catch
			{
				window.textureList.ItemsSource = new string[0];
			}
		}

		#region LatestItem

		readonly string HistoryPath = "history.xml";
		readonly string ProjectTag = "ProjectHistory";

		/// <summary>
		/// 「最近使ったプロジェクト」を読み込む
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> LoadLatestProjects()
		{
			if (File.Exists(HistoryPath))
			{
				FileStream str = null;
				try
				{
					str = File.OpenRead(HistoryPath);

					XDocument xml = XDocument.Load(str);
					var items = xml.Root.Element(ProjectTag).Elements("item").Select(i=>i.Value);
					return items;
				}
				catch
				{
				}
				finally
				{
					if (str != null)
					{
						str.Dispose();
						str = null;
					}
				}
			}
			return null;
		}

		void SaveLatestProjects(string fileName)
		{
			XDocument xml = null;

			try
			{
				using (var str = File.OpenRead(HistoryPath))
				{
					xml = XDocument.Load(str);
				}
			}
			catch
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

		#endregion

		public bool TryClose()
		{
			return projectFileManager.TryClose();
		}

	}
}
