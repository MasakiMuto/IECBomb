using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.IECBomb
{
	public class Manager
	{
		ItemPool pool;
		EvalManager eval;

		public Manager()
		{
			pool = new ItemPool(10);
			eval = new EvalManager();
		}

		public void Play()
		{
			EffectManager.Instance.Run(pool[0], 0);
			EffectManager.Instance.Run(pool[1], 1);
		}

		public void Reset()
		{
			pool.Reset();
		}
	}
}
