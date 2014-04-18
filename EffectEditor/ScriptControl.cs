using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Masa.IECBomb
{
	public class ScriptControl
	{
		FileManager fileManager;
		readonly MainWindow window;
		string lastText = "";


		System.Windows.Controls.TextBox ScriptCode { get { return window.scriptCode; } }

		public ScriptControl(MainWindow window)
		{
			this.window = window;

			fileManager = new FileManager(".mss", "MaSa Script|*.mss|All Files|*.*");
			fileManager.Newed += new Action(scriptFileManager_Newed);
			fileManager.Opened += LoadScript;
			fileManager.Saved += s => SaveScript(s);
			fileManager.ChangeComparer = () => ScriptCode.Text != lastText;
			fileManager.DefaultDirectory = () => Masa.Lib.Utility.ConvertAbsolutePath(window.ProjectFileDirectory, window.XNAControl.ScriptPath);
		}

		void SetLastState()
		{
			lastText = ScriptCode.Text;
		}

		void scriptFileManager_Newed()
		{
			ScriptCode.Text = "";
			window.XNAControl.StopEffect();
			window.SetStatus("NewScriptFile");
			SetLastState();
		}

		void LoadScript(string fileName)
		{
			if (fileName != null)
			{
				ScriptCode.Text = File.ReadAllText(fileName);
				window.SetStatus("Script Loaded : " + fileName);
				SetLastState();
				UpdateProjectScriptPath(fileName);
			}
		}

		void UpdateProjectScriptPath(string fileName)
		{
			var path = Masa.Lib.Utility.ConvertRelativePath(window.ProjectFileName, Path.GetDirectoryName(fileName));
			window.XNAControl.ScriptPath = path;
			
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
				SetLastState();
				UpdateProjectScriptPath(name);
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
