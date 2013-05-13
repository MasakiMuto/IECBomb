using System;
using System.Windows;
using Microsoft.Win32;
using System.IO;


namespace EffectEditor
{
	class FileManager
	{

		public string FileName { get; protected set; }
		public bool Changed { get; set; }
		readonly string DefaultExtension;
		readonly string Filter;
		public event Action<string> Saved;
		public event Action<string> Opened;
		public event Action Newed;

		public FileManager(string defaultExt, string filter)
		{
			DefaultExtension = defaultExt;
			Filter = filter;
		}

		public void New()
		{
			if (!TryClose())
			{
				return;
			}
			Changed = false;
			FileName = null;
			if (Newed != null) Newed();
		}

		public bool Open()
		{
			string res = ShowOpenFileDialog();
			if (res != null)
			{
				return Open(res);
			}
			return false;
		}

		public bool Open(string name)
		{
			if (!TryClose())
			{
				return false;
			}
			if (name != null)
			{
				FileName = name;
				Changed = false;
				if (Opened != null) Opened(name);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool TryClose()
		{
			if (!Changed)
			{
				return true;
			}

			return AskNotSavedDialog();
		}

		/// <summary>
		/// trueなら許可、falseなら不許可
		/// </summary>
		/// <returns></returns>
		bool AskNotSavedDialog()
		{
			var res = MessageBox.Show((FileName == null ? "新規ファイル" : Path.GetFileName(FileName)) + "は変更が保存されていません。終了する前に保存しますか?", "確認",
				MessageBoxButton.YesNoCancel);
			if (res == MessageBoxResult.Yes)
			{
				return Save();
			}
			if (res == MessageBoxResult.Cancel)
			{
				return false;
			}
			if (res == MessageBoxResult.No)
			{
				return true;
			}
			return false;
		}

		public bool Save()
		{
			if (FileName == null)
			{
				return SaveAs();
			}
			else
			{
				SaveAs(FileName);
				return true;
			}

		}

		/// <summary>
		/// 保存キャンセルでfalse
		/// </summary>
		/// <returns></returns>
		public bool SaveAs()
		{
			string res = ShowSaveDialog();
			if (res != null)
			{
				SaveAs(res);
				return true;
			}
			return false;

		}

		string ShowOpenFileDialog()
		{
			var dlg = new OpenFileDialog()
			{
				AddExtension = true,
				DefaultExt = this.DefaultExtension,
				Filter = this.Filter
			};
			//if (dlg.ShowDialog() == DialogResult.OK)
			if(dlg.ShowDialog() == true)
			{
				return dlg.FileName;
			}
			else return null;
		}

		string ShowSaveDialog()
		{
			var dlg = new SaveFileDialog()
			{
				AddExtension = true,
				DefaultExt = this.DefaultExtension,
				OverwritePrompt = true,
				Filter = this.Filter,
			};
			var res = dlg.ShowDialog();
			if (res == true)
			{
				return dlg.FileName;
			}
			else
			{
				return null;
			}
		}

		public void SaveAs(string name)
		{
			Changed = false;
			FileName = name;
			if (Saved != null) Saved(name);
		}
	}
}
