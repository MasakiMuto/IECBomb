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

		public Manager()
		{
			pool = new ItemPool(10);
		}

		public void Play()
		{
			EffectManager.Instance.Run(pool[0]);
		}

		public void Reset()
		{
			pool.Reset();
		}
	}
}
