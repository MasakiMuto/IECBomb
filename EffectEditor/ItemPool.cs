using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.IECBomb
{
	class ItemPool
	{
		public static ItemPool Pool { get; private set; }

		readonly int PoolSize = 10;
		EffectItem[] items;
		float[] scores;
		Random rand;
		public int Generation { get; private set; }

		readonly float CrossOverRatio = .8f;
		readonly float MutationRatio = .05f;

		public ItemPool(int poolSize)
		{
			Pool = this;
			PoolSize = poolSize;
			rand = new Random();
			items = new EffectItem[PoolSize];
			scores = new float[PoolSize];
			Reset();
		}

		public void Reset()
		{
			for (int i = 0; i < PoolSize; i++)
			{
				items[i] = EffectItem.RandomCreate(rand);
			}
			Generation = 0;
		}

		public EffectItem this[int i]
		{
			get { return items[i]; }
		}

		public void UpdateGeneration()
		{
			Generation++;
			for (int i = 0; i < PoolSize; i++)
			{
				scores[i] = EvalManager.Instance.Eval(items[i]);
			}
			var elite = items[GetMaxIndex()];
			var next = new EffectItem[PoolSize];
			next[0] = elite.Clone();
			for (int i = 1; i < PoolSize; i++ )
			{
				if (rand.NextDouble() > MutationRatio)
				{
					next[i] = RandomSelect().Mutate(rand);
				}
				else if (i < PoolSize - 2 && rand.NextDouble() > CrossOverRatio)
				{
					var item1 = RandomSelect();
					var item2 = RandomSelect();
					var child = item1.CrossOver(rand, item2);
					next[i] = child[0];
					next[i + 1] = child[1];
					i++;
				}
				else
				{
					next[i] = RandomSelect().Clone();
				}
			}
			items = next;
		}

		EffectItem RandomSelect()
		{
			float value = (float)rand.NextDouble();
			float s = 0;
			for (int i = 0; i < PoolSize; i++)
			{
				if (s <= value && value <= s + scores[i])
				{
					return items[i];
				}
				else
				{
					s += scores[i];
				}
			}
			throw new Exception();
		}

		int GetMaxIndex()
		{
			float val = scores[0];
			int index = 0;
			for (int i = 0; i < scores.Length; i++)
			{
				if (scores[i] > val)
				{
					index = i;
					val = scores[i];
				}
			}
			return index;
		}

		public float GetMaxScore()
		{
			return scores.Max();
		}

	}
}
