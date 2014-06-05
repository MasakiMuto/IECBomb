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
			public readonly int Score;
			
			public ItemScore(EffectItem item, int score)
			{
				Item = item;
				Score = score;
			}
		}

		List<ItemScore> scoredItems;
		const float QualityWeight = .5f;

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
			return GetQualityScore(item) * QualityWeight + item.GetQuantityScore() * (1 - QualityWeight);
		}

		float GetQualityScore(EffectItem item)
		{
			ItemScore? neaerst = GetNearest(item);
			const float DefaultScore = 0.5f;
			float max = Manager.Instance.ParameterCount;
			float Threadshold = max * .3f;
			if (!neaerst.HasValue)
			{
				return DefaultScore;
			}
			float near = item.Dot(neaerst.Value.Item);
			if (near < Threadshold)
			{
				return DefaultScore;
			}
			return DefaultScore + (neaerst.Value.Score - DefaultScore) * (near - Threadshold) / (max - Threadshold);
			
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

		public void RegistScore(EffectItem item, int score)
		{
			scoredItems.Add(new ItemScore(item, score));
		}
	}
}
