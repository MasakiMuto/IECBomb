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
	public class EffectItem : ItemBase<ParameterName>
	{
		public override float CurrentScore
		{
			get
			{
				return EvalManager.Instance.Eval(this);
			}
		}

		static int TotalIndex { get; set; }


		public static EffectItem RandomCreate(Random rand)
		{
			var p = RandomCreate<EffectItem>(rand);
			p.SetColorDefault();
			return p;
		}

		void SetColorDefault()
		{
			var zeroList = new[] { ParameterName.ColorH, ParameterName.ColorHVar, ParameterName.ColorS, ParameterName.ColorSVar, ParameterName.ColorVVar };
			var oneList = new[] { ParameterName.ColorV,  };
			foreach (int item in zeroList)
			{
				Params[item].NormalizedValue = 0;
			}
			foreach (int item in oneList)
			{
				Params[item].NormalizedValue = 1;
			}
		}


		public EffectItem()
			: base(TotalIndex)
		{
			TotalIndex++;
			Params = new []
			{
				CreateParam(ParameterName.Mass, 1024, 1),
				CreateParam(ParameterName.ColorH, 360, 0),
				CreateParam(ParameterName.ColorHVar, 180,  0),
				CreateParam(ParameterName.ColorS, 1, 0),
				CreateParam(ParameterName.ColorSVar, 1, 0),
				CreateParam(ParameterName.ColorV, 1, 0),
				CreateParam(ParameterName.ColorVVar, 1, 0),
				CreateParam(ParameterName.Speed, 16, 0),
				CreateParam(ParameterName.SpeedVar, 16, 0),
				CreateParam(ParameterName.Accel, 1, -1),
				CreateParam(ParameterName.AccelVar, 1, 0),
				CreateParam(ParameterName.Radius, 256, 1),
				CreateParam(ParameterName.RadiusVar, 128, 0),
				CreateParam(ParameterName.Alpha, 10, -10),
				CreateParam(ParameterName.AlphaVar, 10, 0),
				CreateParam(ParameterName.AlphaVel, 1, -1),
				CreateParam(ParameterName.AlphaVelVar, 1, 0),
				CreateParam(ParameterName.AlphaAccel, 0, -1),
				CreateParam(ParameterName.AlphaAccelVar, 1, 0),
			};
		}

		protected override IEnumerable<ParameterName> UnlockedParameters
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

	
		public override float GetQuantityScore()
		{
			const float Max = 1f;
			float score = Max;
			if (!IsValidAlpha())
			{
				score *= .5f;
			}
			if (!IsValidRadius())
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

		/// <summary>
		/// 大部分の粒の半径が十分大きいか
		/// </summary>
		/// <returns></returns>
		bool IsValidRadius()
		{
			return this[ParameterName.Radius] - this[ParameterName.RadiusVar] > 0;
		}

		#region ScriptOutput

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

		#endregion

	}
}
