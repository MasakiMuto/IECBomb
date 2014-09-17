using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Masa.IECBomb
{
	public class Parameter<T> : IEqualityComparer<Parameter<T>> where T : struct, IComparable, IConvertible
	{
		public readonly float Max, Min;
		float normalizedValue;
		public float NormalizedValue///0..1に正規化した値
		{
			get { return normalizedValue; }
			set
			{
				normalizedValue = MathHelper.Clamp(value, 0, 1);
			}
		}
		public readonly T Name;

		public Parameter(T name, float max, float min)
		{
			Name = name;
			Max = max;
			Min = min;
		}

		public float GetValue()
		{
			return Min + (Max - Min) * NormalizedValue;
		}

		public void SetValue(float val)
		{
			System.Diagnostics.Debug.Assert(Max >= val && val >= Min);
			NormalizedValue = (val - Min) / (Max - Min);
		}

		public override bool Equals(object obj)
		{
			return Equals(this, obj as Parameter<T>);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public bool Equals(Parameter<T> x, Parameter<T> y)
		{
			return x != null && y != null && x.Name.CompareTo(y.Name) == 0 && x.NormalizedValue == y.NormalizedValue;
		}

		public int GetHashCode(Parameter<T> obj)
		{
			throw new NotImplementedException();
		}
	}


	public abstract class ItemBase<T> where T : struct, IComparable, IConvertible
	{
		public Parameter<T>[] Params;

		public abstract float CurrentScore { get; }

		public readonly int Index;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index">個体通し番号</param>
		public ItemBase(int index)
		{
			Index = index;
		}

		public float this[T name]
		{
			get { return Params[name.ToInt32(null)].GetValue(); }
			set { Params[name.ToInt32(null)].SetValue(value); }
		}

		public override bool Equals(object obj)
		{
			var item = obj as ItemBase<T>;
			return item != null && (item.Index == this.Index || Params.SequenceEqual(item.Params));
		}

		public override int GetHashCode()
		{
			return Index;
		}

		protected Parameter<T> CreateParam(T name, float max, float min)
		{
			return new Parameter<T>(name, max, min);
		}

		protected abstract IEnumerable<T> UnlockedParameters { get; }

		public IEnumerable<int> UnlockedParameterIndexs
		{
			get { return UnlockedParameters.Select(x => x.ToInt32(null)); }
		}

		public static S RandomCreate<S>(Random rand) where S : ItemBase<T>, new()
		{
			var p = new S();
			for (int i = 0; i < Enum.GetValues(typeof(T)).Length; i++)
			{
				p.Params[i].NormalizedValue = (float)rand.NextDouble();
			}
			return p;
		}

		/// <summary>
		/// 完全なクローンを返す
		/// </summary>
		/// <returns></returns>
		public virtual S Clone<S>() where S : ItemBase<T>, new()
		{
			var item = new S();
			for (int i = 0; i < Params.Length; i++)
			{
				item.Params[i].NormalizedValue = Params[i].NormalizedValue;
			}
			return item;
		}

		#region Standard GA

		/// <summary>
		/// 1つのランダムなパラメタをランダムに変異させたクローンを返す
		/// </summary>
		/// <returns></returns>
		public S Mutate<S>(Random rand) where S : ItemBase<T>, new()
		{
			var clone = Clone<S>();
			var item = rand.Next(Params.Length);
			if (UnlockedParameters.Contains((T)Enum.Parse(typeof(T), item.ToString())))
			{
				clone.Params[item].NormalizedValue = (float)rand.NextDouble();
			}
			return clone;
		}

		/// <summary>
		/// 自分ともう一つの親から2点交叉で2つの子を作る
		/// </summary>
		/// <param name="rand"></param>
		/// <param name="item1"></param>
		/// <param name="item2"></param>
		/// <returns></returns>
		public S[] CrossOver<S>(Random rand, S item2) where S : ItemBase<T>, new()
		{
			var child1 = this.Clone<S>();
			var child2 = item2.Clone<S>();
			int i1, i2;
			CreateCrossIndexs(rand, out i1, out i2);

			for (int i = 0; i < Params.Length; i++)
			{
				if (i1 <= i && i < i2)
				{
					child1.Params[i].NormalizedValue = item2.Params[i].NormalizedValue;
					child2.Params[i].NormalizedValue = Params[i].NormalizedValue;
				}
			}
			return new S[] { child1, child2 };
		}

		/// <summary>
		/// 交差点2つの生成
		/// </summary>
		/// <param name="rand"></param>
		/// <param name="val1">小さい方</param>
		/// <param name="val2">大きい方</param>
		protected void CreateCrossIndexs(Random rand, out int val1, out int val2)
		{
			int tmp1 = rand.Next(Params.Length);
			int tmp2 = rand.Next(Params.Length);
			val1 = Math.Min(tmp1, tmp2);
			val2 = Math.Max(tmp1, tmp2);
		}

		#endregion

		/// <summary>
		/// 差分進化での変異ベクトル生成
		/// </summary>
		/// <param name="baseItem"></param>
		/// <param name="i1"></param>
		/// <param name="i2"></param>
		/// <param name="weight"></param>
		/// <returns></returns>
		public static S CreateMutate<S>(S baseItem, S i1, S i2, float weight) where S : ItemBase<T>, new()
		{
			var ret = new S();
			foreach (int item in ret.UnlockedParameterIndexs)
			{
				var val = baseItem.Params[item].NormalizedValue + weight * (i1.Params[item].NormalizedValue - i2.Params[item].NormalizedValue);
				ret.Params[item].NormalizedValue = MathHelper.Clamp(val, 0, 1);
			}
			return ret;
		}

		/// <summary>
		/// 項目間類似度
		/// </summary>
		/// <param name="item"></param>
		/// <returns>19..-19</returns>
		public float Dot<S>(S item) where S : ItemBase<T>
		{
			float s = 0;
			float v1 = 0, v2 = 0;
			foreach (int i in UnlockedParameterIndexs)
			{
				v1 += Params[i].NormalizedValue * Params[i].NormalizedValue;
				v2 += item.Params[i].NormalizedValue * item.Params[i].NormalizedValue;
				s += Params[i].NormalizedValue * item.Params[i].NormalizedValue;
			}
			return s / (float)Math.Sqrt(v1 * v2);
		}


		public abstract float GetQuantityScore();

	}

	public static class ItemExtension
	{
		

		
	}
}
