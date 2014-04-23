using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.IECBomb
{
	public enum ParameterName
	{
		Mass,
		ColorH,
		ColorS,
		ColorV,

	}

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
			Params = new[]
			{
				new Parameter(ParameterName.Mass, 1024, 1),
				new Parameter(ParameterName.ColorH, 360, 0),
				new Parameter(ParameterName.ColorS, 1, 0),
				new Parameter(ParameterName.ColorV, 1, 0)
			};
		}

		public float this[ParameterName name]
		{
			get { return Params[(int)name].GetValue(); }
		}

	}
}
