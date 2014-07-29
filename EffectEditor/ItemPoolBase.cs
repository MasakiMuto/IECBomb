using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.IECBomb
{
	public abstract class ItemPoolBase<T, S>  where T : ItemBase<S>, new() where S : struct, IComparable, IConvertible
	{
		readonly int PoolSize;
		T[] items;
		float[] scores;
		Random rand;
		public int Generation { get; private set; }

		readonly float CrossOverRatio = .05f;
		readonly float MutationRatio = .1f;

		public ItemPoolBase(int poolSize)
		{
			PoolSize = poolSize;
			rand = new Random();
			items = new T[PoolSize];
			scores = new float[PoolSize];
			Reset();
		}

		public void Reset()
		{
			EvalManager.Instance.Reset();
			for (int i = 0; i < PoolSize; i++)
			{
				items[i] = ItemBase<S>.RandomCreate<T>(rand);
			}
			Generation = 0;
		}

		void SetAverageScore()
		{
			EvalManager.Instance.AverageScore = items.Average(x => x.CurrentScore);
		}

		public T this[int i]
		{
			get { return items[i]; }
		}

		public T[] Items { get { return items; } }


		#region Differencial Evolution

		public void UpdateDifferencial()
		{
			SetAverageScore();
			Generation++;
			var next = new T[PoolSize];
			for (int i = 0; i < PoolSize; i++)
			{
				var n = CreateTripleRand(i);
				var target = ItemBase<S>.CreateMutate<T>(items[n[0]], items[n[1]], items[n[2]], 0.3f);
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

		T CreateCrossOver(T parent, T child)
		{
			var ret = new T();
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
				scores[i] = Eval(items[i]);
			}
			var elite = items[GetMaxIndex()];
			var next = new T[PoolSize];
			//next[0] = elite.Clone();
			for (int i = 1; i < PoolSize; i++)
			{
				if (rand.NextDouble() > MutationRatio)
				{
					next[i] = RandomSelect().Mutate<T>(rand);
				}
				else if (i < PoolSize - 2 && rand.NextDouble() > CrossOverRatio)
				{
					var item1 = RandomSelect();
					var item2 = RandomSelect();
					var child = item1.CrossOver<T>(rand, item2);
					next[i] = child[0];
					next[i + 1] = child[1];
					i++;
				}
				else
				{
					next[i] = RandomSelect().Clone<T>();
				}
			}
			items = next;
		}

		T RandomSelect()
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
				scores[i] = Eval(items[i]);
			}
			return scores.Max();
		}

		#endregion

		/// <summary>
		/// 指定の個体の特定パラメタを全個体にコピーする
		/// </summary>
		/// <param name="original"></param>
		/// <param name="targets"></param>
		public void CloneParams(T original, IEnumerable<S> targets)
		{
			foreach (var item in this.items)
			{
				foreach (int param in targets.Select(x=>Convert.ToInt32(x)))
				{
					item.Params[param].NormalizedValue = original.Params[param].NormalizedValue;
				}
			}
		}

		protected abstract float Eval(T item);
	}
}
