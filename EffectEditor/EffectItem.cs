using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Masa.Lib.XNA;
using Microsoft.Xna.Framework;
using Masa.Lib;

namespace Masa.IECBomb
{
	public enum ParameterName
	{
		Mass,
		ColorH,
		ColorHVar,
		ColorS,
		ColorSVar,
		ColorV,
		ColorVVar,
		Speed,
		SpeedVar,
		Accel,
		AccelVar,
		Radius,
		RadiusVar,
		Alpha,
		AlphaVar,
		AlphaVel,
		AlphaVelVar,
		AlphaAccel,
		AlphaAccelVar,
	}

	/// <summary>
	/// 個体
	/// </summary>
	public class EffectItem
	{
		public class Parameter
		{
			public readonly float Max, Min;
			public float NormalizedValue;//0..1に正規化した値
			public readonly ParameterName Name;

			public Parameter(ParameterName name, float max, float min)
			{
				Name = name;
				Max = max;
				Min = min;
			}

			public float GetValue()
			{
				return Min + (Max - Min) * NormalizedValue;
			}

			public override bool Equals(object obj)
			{
				var p = obj as Parameter;
				return p != null && p.Name == this.Name && p.NormalizedValue == this.NormalizedValue;
			}
		}

		public Parameter[] Params;

		public float CurrentScore { get; set; }

		static int TotalIndex { get; set; }

		public readonly int Index;

		public static EffectItem RandomCreate(Random rand)
		{
			var p = new EffectItem();
			for (int i = 0; i < Enum.GetValues(typeof(ParameterName)).Length; i++)
			{
				p.Params[i].NormalizedValue = (float)rand.NextDouble();
			}
			return p;
		}

		public override int GetHashCode()
		{
			return Index;
		}

		public override bool Equals(object obj)
		{
			var item = obj as EffectItem;
			return item != null && (item.Index == this.Index || Params.SequenceEqual(item.Params));
		}

		public EffectItem()
		{
			Index = TotalIndex;
			TotalIndex++;
			Params = new Parameter[]
			{
				new Parameter(ParameterName.Mass, 1024, 1),
				new Parameter(ParameterName.ColorH, 360, 0),
				new Parameter(ParameterName.ColorHVar, 180,  0),
				new Parameter(ParameterName.ColorS, 1, 0),
				new Parameter(ParameterName.ColorSVar, 1, 0),
				new Parameter(ParameterName.ColorV, 1, 0),
				new Parameter(ParameterName.ColorVVar, 1, 0),
				new Parameter(ParameterName.Speed, 16, 0),
				new Parameter(ParameterName.SpeedVar, 16, 0),
				new Parameter(ParameterName.Accel, 1, -1),
				new Parameter(ParameterName.AccelVar, 1, 0),
				new Parameter(ParameterName.Radius, 256, 1),
				new Parameter(ParameterName.RadiusVar, 128, 0),
				new Parameter(ParameterName.Alpha, 10, -10),
				new Parameter(ParameterName.AlphaVar, 10, 0),
				new Parameter(ParameterName.AlphaVel, 1, -1),
				new Parameter(ParameterName.AlphaVelVar, 1, 0),
				new Parameter(ParameterName.AlphaAccel, 0, -1),
				new Parameter(ParameterName.AlphaAccelVar, 1, 0),
			};
		}

		public float this[ParameterName name]
		{
			get { return Params[(int)name].GetValue(); }
		}

		IEnumerable<ParameterName> UnlockedParameters
		{
			get
			{
				return Manager.Instance.UnlockedParameters;
			}
		}

		public Masa.ParticleEngine.ParticleParameter CreateParticleParameter(Random rand, Vector2 pos)
		{
			var v = MathUtilXNA.GetVector(NormalVariance(rand, ParameterName.Speed, ParameterName.SpeedVar), rand.Next());
			return new ParticleEngine.ParticleParameter(
				pos,
				v,
				v * NormalVariance(rand, ParameterName.Accel, ParameterName.AccelVar),
				new Vector3(NormalVariance(rand, ParameterName.Alpha, ParameterName.AlphaVar), NormalVariance(rand, ParameterName.AlphaVel, ParameterName.AlphaVelVar), NormalVariance(rand, ParameterName.AlphaAccel, ParameterName.AlphaAccelVar)),
				new Vector2(NormalVariance(rand, ParameterName.Radius, ParameterName.RadiusVar)),
				Vector2.Zero,
				HSVColor.HSVToRGB(NormalVariance(rand, ParameterName.ColorH, ParameterName.ColorHVar), NormalVariance(rand, ParameterName.ColorS, ParameterName.ColorSVar), NormalVariance(rand, ParameterName.ColorV, ParameterName.ColorVVar))
				);
		}

		/// <summary>
		/// 正規分布乱数を返す
		/// </summary>
		/// <param name="rand"></param>
		/// <param name="average"></param>
		/// <param name="variance"></param>
		/// <returns></returns>
		float NormalVariance(Random rand, ParameterName average, ParameterName variance)
		{
			return (float)rand.NextNormal(this[average], this[variance]);
		}

		#region Standard GA

		/// <summary>
		/// 完全なクローンを返す
		/// </summary>
		/// <returns></returns>
		public EffectItem Clone()
		{
			var item = new EffectItem();
			for (int i = 0; i < Params.Length; i++)
			{
				item.Params[i].NormalizedValue = Params[i].NormalizedValue;
			}
			return item;
		}

		/// <summary>
		/// 1つのランダムなパラメタをランダムに変異させたクローンを返す
		/// </summary>
		/// <returns></returns>
		public EffectItem Mutate(Random rand)
		{
			var clone = Clone();
			var item = rand.Next(Params.Length);
			if(UnlockedParameters.Contains((ParameterName)item))
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
		public EffectItem[] CrossOver(Random rand, EffectItem item2)
		{
			var child1 = new EffectItem();
			var child2 = new EffectItem();
			int i1, i2;
			CreateCrossIndexs(rand, out i1, out i2);

			for (int i = 0; i < Params.Length; i++)
			{
				if (i1 <= i && i < i2)
				{
					child1.Params[i].NormalizedValue = Params[i].NormalizedValue;
					child2.Params[i].NormalizedValue = item2.Params[i].NormalizedValue;
				}
				else
				{
					child1.Params[i].NormalizedValue = item2.Params[i].NormalizedValue;
					child2.Params[i].NormalizedValue = Params[i].NormalizedValue;
				}
			}
			return new EffectItem[] { child1, child2 };
		}

		/// <summary>
		/// 交差点2つの生成
		/// </summary>
		/// <param name="rand"></param>
		/// <param name="val1">小さい方</param>
		/// <param name="val2">大きい方</param>
		void CreateCrossIndexs(Random rand, out int val1, out int val2)
		{
			int tmp1 = rand.Next(Params.Length);
			int tmp2 = rand.Next(Params.Length);
			val1 = Math.Min(tmp1, tmp2);
			val2 = Math.Max(tmp1, tmp2);
		}

		#endregion

		/// <summary>
		/// 項目間類似度
		/// </summary>
		/// <param name="item"></param>
		/// <returns>19..-19</returns>
		public float Dot(EffectItem item)
		{
			float s = 0;
			foreach (int i in UnlockedParameters)
			{
				s += Params[i].NormalizedValue * item.Params[i].NormalizedValue;
			}
			return s;
		}

		/// <summary>
		/// 差分進化での変異ベクトル生成
		/// </summary>
		/// <param name="baseItem"></param>
		/// <param name="i1"></param>
		/// <param name="i2"></param>
		/// <param name="weight"></param>
		/// <returns></returns>
		public static EffectItem CreateMutate(EffectItem baseItem, EffectItem i1, EffectItem i2, float weight)
		{
			var ret = new EffectItem();
			foreach (int item in ret.UnlockedParameters)
			{
				var val = baseItem.Params[item].NormalizedValue + weight * (i1.Params[item].NormalizedValue - i2.Params[item].NormalizedValue);
				ret.Params[item].NormalizedValue = MathHelper.Clamp(val, 0, 1);
			}
			return ret;
		}

		public float GetQuantityScore()
		{
			const float Max = 1f;
			float score = Max;
			if (!IsValidAlpha())
			{
				score *= .5f;
			}
			return score;
		}

		/// <summary>
		/// 時間経過で消えるか
		/// </summary>
		/// <returns></returns>
		bool IsValidAlpha()
		{
			return this[ParameterName.AlphaAccel] + this[ParameterName.AlphaAccelVar] < 0//加速度の1σが負
				&& this[ParameterName.Alpha] - this[ParameterName.AlphaVel] * this[ParameterName.AlphaVel] / 4 / this[ParameterName.AlphaAccel] > 0;//平均でのAlpha最大値が正
		}

		public string ToScript(string itemName)
		{
			return string.Format(
@"
var i = {1}
while (i > 0)
	i --
	{0}
vanish
",
				ToMakeScript(itemName), this[ParameterName.Mass]);
		}

		string ToMakeScript(string itemName) 
		{
			return "make " + itemName +
				" : r " + CreateRandNmlStatement(ParameterName.Radius, ParameterName.RadiusVar) +
				" : vela " + CreateRandNmlStatement(ParameterName.Speed, ParameterName.SpeedVar) + "(rand : max 100)" +
				" : acv " + CreateRandNmlStatement(ParameterName.Accel, ParameterName.AccelVar) +
				" : alp " + CreateRandNmlStatement(ParameterName.Alpha, ParameterName.AlphaVar) + CreateRandNmlStatement(ParameterName.AlphaVel, ParameterName.AlphaVelVar) + CreateRandNmlStatement(ParameterName.AlphaAccel, ParameterName.AlphaAccelVar) +
				" : color (hsv " + CreateRandNmlStatement(ParameterName.ColorH, ParameterName.ColorHVar) + CreateRandNmlStatement(ParameterName.ColorS, ParameterName.ColorSVar) + CreateRandNmlStatement(ParameterName.ColorV, ParameterName.ColorVVar) + ")"; 
 
		}

		string CreateRandNmlStatement(ParameterName avg, ParameterName var)
		{
			return string.Format(" (randnml {0} {1}) ", this[avg], this[var]);
		}
	}
}
