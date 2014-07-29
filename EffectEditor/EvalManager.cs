using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace Masa.IECBomb
{
	/// <summary>
	/// エフェクトの評価器
	/// </summary>
	public class EvalManager : EvalManagerBase<EffectItem, ParameterName>
	{
		public static EvalManager Instance { get; private set; }

		public EvalManager()
			: base()
		{
			Instance = this;
		}

		
	}
}
