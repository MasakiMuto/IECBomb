using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.IECBomb
{
	class ItemPool
	{
		readonly int PoolSize = 10;
		List<EffectItem> items;
		Random rand;


		public ItemPool(int poolSize)
		{
			PoolSize = poolSize;
			rand = new Random();
			items = new List<EffectItem>();
			Reset();
		}

		public void Reset()
		{
			items.Clear();
			for (int i = 0; i < PoolSize; i++)
			{
				items.Add(EffectItem.RandomCreate(rand));
			}
		}

		public EffectItem this[int i]
		{
			get { return items[i]; }
		}

	}
}
