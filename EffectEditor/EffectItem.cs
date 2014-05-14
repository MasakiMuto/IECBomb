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
		}

		public Parameter[] Params;

		public static EffectItem RandomCreate(Random rand)
		{
			var p = new EffectItem();
			for (int i = 0; i < Enum.GetValues(typeof(ParameterName)).Length; i++)
			{
				p.Params[i].NormalizedValue = (float)rand.NextDouble();
			}
			return p;
		}

		public EffectItem()
		{
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
				new Parameter(ParameterName.AlphaAccel, 1, -1),
				new Parameter(ParameterName.AlphaAccelVar, 1, 0),
			};
		}

		public float this[ParameterName name]
		{
			get { return Params[(int)name].GetValue(); }
		}

		public Masa.ParticleEngine.ParticleParameter CreateParticleParameter(Random rand)
		{
			var v = MathUtilXNA.GetVector(NormalVariance(rand, ParameterName.Speed, ParameterName.SpeedVar), rand.Next());
			return new ParticleEngine.ParticleParameter(
				Vector2.Zero,
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

	}
}
