using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.IECBomb
{
	public abstract class EvalManagerBase<T, S> where T : ItemBase<S>, new() where S : struct, IComparable, IConvertible
	{
		Dictionary<T, float> scoredItems;
		const float QualityWeight = 1f;
		public readonly float InitialScore = 1;
		public float AverageScore { get; set; }

		public EvalManagerBase()
		{
			scoredItems = new Dictionary<T, float>();
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
		public float Eval(T item)
		{
			return GetQualityScore(item) * QualityWeight + item.GetQuantityScore() * (1 - QualityWeight);
		}

		float GetQualityScore(T item)
		{
			var neaerst = GetNearest(item);
			float Threadshold = .3f;
			if (!neaerst.HasValue)
			{
				return AverageScore;
			}
			float near = item.Dot(neaerst.Value.Key);
			if (near < Threadshold)
			{
				return AverageScore;
			}
			return AverageScore + (neaerst.Value.Value - AverageScore) * (near - Threadshold) / (1 - Threadshold);
			
		}

		/// <summary>
		/// 評価済み個体のなかから最も似ている個体を探す
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		KeyValuePair<T, float>? GetNearest(T item)
		{
			KeyValuePair<T, float>? ret = null;
			float max = float.MinValue;
			float tmp;
			foreach (var i in scoredItems)
			{
				tmp = item.Dot(i.Key);
				if (tmp > max)
				{
					max = tmp;
					ret = i;
				}
			}
			return ret;
		}

		public void RegistScore(T item, float score)
		{
			scoredItems[item] = score;
			RemoveOld(item.Index);
		}

		void RemoveOld(int head)
		{
			const int Border = 400;
			var removeList = scoredItems.Keys.Where(x => head - x.Index > Border).ToArray();
			foreach (var item in removeList)
			{
				scoredItems.Remove(item);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item1"></param>
		/// <param name="item2"></param>
		/// <param name="side">1が優秀=1、2が優秀=2、同じ=0</param>
		public void RegistScore(T item1, T item2, int side)
		{
			const float MixRatio = .5f;//もう一方のスコアを採用する率
			var score1 = GetQualityScore(item1);
			var score2 = GetQualityScore(item2);
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
