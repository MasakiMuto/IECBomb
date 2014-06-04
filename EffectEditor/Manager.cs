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
		int leftIndex, rightIndex;
		const int PoolSize = 10;
		Random rand;

		public Manager()
		{
			pool = new ItemPool(PoolSize);
			eval = new EvalManager();
			rand = new Random();
		}

		public void Play()
		{
			leftIndex = rand.Next(PoolSize);
			do
			{
				rightIndex = rand.Next(PoolSize);
			} while (rightIndex == leftIndex);
			EffectManager.Instance.Run(pool[leftIndex], 0);
			EffectManager.Instance.Run(pool[rightIndex], 1);
		}

		public void Reset()
		{
			pool.Reset();
		}

		public void Input(int left, int right)
		{
			eval.RegistScore(pool[leftIndex], left);
			eval.RegistScore(pool[rightIndex], right);
			pool.UpdateGeneration();
			EffectManager.Instance.Clear();
			Play();
		}
	}
}
