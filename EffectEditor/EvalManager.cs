using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

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
			public readonly float Score;
			
			public ItemScore(EffectItem item, float score)
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
			ItemScore? neaerst = GetNearest(item);
			const float Threadshold = 3;
			const float DefaultScore = 0.5f;
			const float Max = 19;
			if (!neaerst.HasValue)
			{
				return DefaultScore;
			}
			float near = item.Dot(neaerst.Value.Item);
			if (near < Threadshold)
			{
				return DefaultScore;
			}
			return DefaultScore + (neaerst.Value.Score - DefaultScore) * (near - Threadshold) / (Max - Threadshold);
			//return item.Params.Sum(x => x.NormalizedValue);
		}

		ItemScore? GetNearest(EffectItem item)
		{
			ItemScore? ret = null;
			float max = float.MinValue;
			float tmp;
			foreach (var i in scoredItems)
			{
				tmp = item.Dot(i.Item);
				if (tmp > max)
				{
					max = tmp;
					ret = i;
				}
			}
			return ret;
		}

		public void RegistScore(EffectItem item, float score)
		{
			scoredItems.Add(new ItemScore(item, score));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item1"></param>
		/// <param name="item2"></param>
		/// <param name="side">1が優秀=1、2が優秀=2、同じ=0</param>
		public void RegistScore(EffectItem item1, EffectItem item2, int side)
		{
			const float MixRatio = .5f;//もう一方のスコアを採用する率
			var score1 = Eval(item1);
			var score2 = Eval(item2);
			float tmp;
			switch (side)
			{
				case 1:
					score1 += score2 * MixRatio;
					score2 *= 1 - MixRatio;
					break;
				case 2:
					score2 += score1 * MixRatio;
					score1 *= 1 - MixRatio;
					break;
				case 0:
					tmp = score1;
					score1 = score1 * (1 - MixRatio) + score2 * MixRatio;
					score2 = score2 * (1 - MixRatio) + tmp * MixRatio;
					break;
				default:
					throw new ArgumentException();
			}
			RegistScore(item1, score1);
			RegistScore(item2, score2);
		}
	}
}
