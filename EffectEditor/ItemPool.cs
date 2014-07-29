using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.IECBomb
{
	class ItemPool : ItemPoolBase<EffectItem, ParameterName>
	{
		public static ItemPool Pool { get; private set; }

		protected override float Eval(EffectItem item)
		{
			return EvalManager.Instance.Eval(item);
		}

		public ItemPool(int poolSize)
			: base(poolSize)
		{
			Pool = this;
		}
		
	}
}
