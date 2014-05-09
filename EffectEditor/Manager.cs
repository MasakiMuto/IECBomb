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
		EffectManager effectManager;

		public Manager(EffectManager manager)
		{
			pool = new ItemPool(10);
			effectManager = manager;
		}

		public void Play()
		{
			effectManager.Run(pool[0]);
		}

		public void Reset()
		{
			pool.Reset();
		}
	}
}
