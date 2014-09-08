using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSound
{
	class EvalManager : Masa.IECBomb.EvalManagerBase<SynthParam, ParamName>
	{
		public static EvalManager Instance { get; private set; }

		public EvalManager()
			:base()
		{
			Instance = this;
		}
	}
}
