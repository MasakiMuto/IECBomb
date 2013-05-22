using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EffectEditor
{
	public class ScriptControl
	{
		FileManager fileManager;
		readonly MainWindow window;
		string fileName;

		System.Windows.Controls.TextBox ScriptCode { get { return window.scriptCode; } }

		public ScriptControl(MainWindow window)
		{
			this.window = window;

			fileManager = new FileManager(".mss", "MaSa Script|*.mss|All Files|*.*");
			fileManager.Newed += new Action(scriptFileManager_Newed);
			fileManager.Opened += LoadScript;
			fileManager.Saved += s => SaveScript(s);
		}

		void scriptFileManager_Newed()
		{
			ScriptCode.Text = "";
			window.XNAControl.StopEffect();
			window.SetStatus("NewFile");
		}

		void LoadScript(string fileName)
		{
			if (fileName != null)
			{
				ScriptCode.Text = File.ReadAllText(fileName);
				window.SetStatus("Script Loaded : " + fileName);
			}
		}

		public void OpenFile()
		{
			fileManager.Open();
		}

		public void OpenFile(string name)
		{
			fileManager.Open(name);
		}

		public void NewFile()
		{
			fileManager.New();
			ScriptCode.Text = "";
			window.XNAControl.StopEffect();
			window.SetStatus("NewScriptFile");
		}

		public void Save()
		{
			fileManager.Save();
		}

		public void SaveAs()
		{
			fileManager.SaveAs();
		}

		public bool TryClose()
		{
			return fileManager.TryClose();
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
				File.WriteAllText(name, ScriptCode.Text);
				window.SetStatus("Script Saved : " + name);
				return true;
			}
			catch (Exception e)
			{
				System.Windows.Forms.MessageBox.Show("保存に失敗しました:" + e.Message);
				return false;
			}
		}

		public void Changed()
		{
			fileManager.Changed = true;
		}


		
	}
}
