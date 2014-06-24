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

		readonly int PoolSize;
		EffectItem[] items;
		float[] scores;
		Random rand;
		public int Generation { get; private set; }

		readonly float CrossOverRatio = .05f;
		readonly float MutationRatio = .1f;

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
			EvalManager.Instance.Reset();
			for (int i = 0; i < PoolSize; i++)
			{
				items[i] = EffectItem.RandomCreate(rand);
			}
			Generation = 0;
		}

		void SetAverageScore()
		{
			EvalManager.Instance.AverageScore = items.Average(x => x.CurrentScore);
		}

		public EffectItem this[int i]
		{
			get { return items[i]; }
		}

		public EffectItem[] Items { get { return items; } }


		#region Differencial Evolution

		public void UpdateDifferencial()
		{
			SetAverageScore();
			Generation++;
			var next = new EffectItem[PoolSize];
			for (int i = 0; i < PoolSize; i++)
			{
				var n = CreateTripleRand(i);
				var target = EffectItem.CreateMutate(items[n[0]], items[n[1]], items[n[2]], 0.3f);
				var child = CreateCrossOver(items[i], target);
				if (child.CurrentScore > items[i].CurrentScore)
				{
					next[i] = child;
				}
				else
				{
					next[i] = items[i];
				}
			}
			items = next;
		}

		EffectItem CreateCrossOver(EffectItem parent, EffectItem child)
		{
			var ret = new EffectItem();
			var index = rand.Next(parent.Params.Length);
			for (int i = 0; i < index; i++)
			{
				ret.Params[i].NormalizedValue = parent.Params[i].NormalizedValue;
			}
			ret.Params[index].NormalizedValue = child.Params[index].NormalizedValue;
			for (int i = index + 1; i < ret.Params.Length; i++)
			{
				ret.Params[i].NormalizedValue = (rand.NextDouble() < CrossOverRatio ? child : parent).Params[i].NormalizedValue;
			}
			return ret;
		}

		/// <summary>
		/// 互いに重複しない3つの乱数
		/// </summary>
		/// <param name="a">除外する物</param>
		/// <returns></returns>
		List<int> CreateTripleRand(int a)
		{
			var l = new List<int>();
			l.Add(a);
			for (int i = 0; i < 3; i++)
			{
				l.Add(CreateRand(l));				
			}
			l.Remove(a);
			return l;
		}

		int CreateRand(List<int> a)
		{
			int x;
			do
			{
				x = rand.Next(PoolSize);
			} while (a.Contains(x));
			return x;
		}

		#endregion

		#region Standard GA

		public void UpdateGeneration()
		{
			SetAverageScore();
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
			float value = (float)rand.NextDouble() * scores.Sum();
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
			for (int i = 0; i < scores.Length; i++)
			{
				scores[i] = EvalManager.Instance.Eval(items[i]);
			}
			return scores.Max();
		}

		#endregion

		/// <summary>
		/// 指定の個体の特定パラメタを全個体にコピーする
		/// </summary>
		/// <param name="original"></param>
		/// <param name="targets"></param>
		public void CloneParams(EffectItem original, IEnumerable<ParameterName> targets)
		{
			foreach (var item in this.items)
			{
				foreach (int param in targets)
				{
					item.Params[param].NormalizedValue = original.Params[param].NormalizedValue;
				}
			}
		}

	}
}
