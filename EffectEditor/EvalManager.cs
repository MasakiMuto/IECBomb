using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.IECBomb
{
	/// <summary>
	/// エフェクトの評価器
	/// </summary>
	public class EvalManager
	{
		public static EvalManager Instance { get; private set; }

		struct ItemScore
		{
			public readonly EffectItem Item;
			public readonly int Score;
			
			public ItemScore(EffectItem item, int score)
			{
				Item = item;
				Score = score;
			}
		}

		List<ItemScore> scoredItems;

		public EvalManager()
		{
			Instance = this;
			scoredItems = new List<ItemScore>();
		}

		/// <summary>
		/// エフェクトの適応度
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public float Eval(EffectItem item)
		{
			return item.Params.Sum(x => x.NormalizedValue);
		}

		public void RegistScore(EffectItem item, int score)
		{
			scoredItems.Add(new ItemScore(item, score));
		}
	}
}
