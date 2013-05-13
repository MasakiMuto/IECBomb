using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;


namespace EffectEditor
{
	class MyLogger : ContentBuildLogger
	{
		public override void LogMessage(string message, params object[] messageArgs)
		{
			
		}

		public override void LogImportantMessage(string message, params object[] messageArgs)
		{
			
		}

		public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
		{
			
		}
	}

	class MyImporterContext : ContentImporterContext
	{
		public override string IntermediateDirectory
		{
			get { return string.Empty; }
		}

		public override string OutputDirectory
		{
			get { return string.Empty; }
		}

		public override ContentBuildLogger Logger
		{
			get { return logger; }
		}

		ContentBuildLogger logger = new MyLogger();

		public override void AddDependency(string filename)
		{
			
		}
	}

	class ShaderProcesser
	{
		public ShaderProcesser()
		{
			var m = new CompiledEffectContent(null);
			
		}
	}
}
