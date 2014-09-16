using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSound
{
	class ItemPool : Masa.IECBomb.ItemPoolBase<SynthParam, ParamName>
	{
		public static ItemPool Instance { get; private set; }

		protected override float Eval(SynthParam item)
		{
			return EvalManager.Instance.Eval(item);
		}

		public ItemPool()
			: base(10)
		{
			Instance = this;
		}

		public void Init(SynthParam p)
		{
			for (int i = 0; i < items.Length; i++)
			{
				items[i] = SynthParam.Mutate(p);
			}
		}
	}
}
