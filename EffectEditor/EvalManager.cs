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
		const float QualityWeight = 1f;
		public readonly float InitialScore = 1;
		public float AverageScore { get; set; }

		public EvalManager()
		{
			Instance = this;
			scoredItems = new List<ItemScore>();
		}

		public void Reset()
		{
			scoredItems.Clear();
			AverageScore = InitialScore;
		}

		/// <summary>
		/// エフェクトの適応度
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public float Eval(EffectItem item)
		{
			return GetQualityScore(item) * QualityWeight + item.GetQuantityScore() * (1 - QualityWeight);
		}

		float GetQualityScore(EffectItem item)
		{
			ItemScore? neaerst = GetNearest(item);
			float max = Manager.Instance.ParameterCount;
			float Threadshold = max * .3f;
			if (!neaerst.HasValue)
			{
				return AverageScore;
			}
			float near = item.Dot(neaerst.Value.Item);
			if (near < Threadshold)
			{
				return AverageScore;
			}
			return AverageScore + (neaerst.Value.Score - AverageScore) * (near - Threadshold) / (max - Threadshold);
			
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
			var score1 = item1.CurrentScore;
			var score2 = item2.CurrentScore;
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
