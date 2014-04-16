using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EffectEditor
{
	internal static class Util
	{
		public static Color FromXColor(Microsoft.Xna.Framework.Vector4 col)
		{
			Func<float, byte> convert = f => (byte)(f * 255);
			return new Color()
			{
				A = convert(col.W),
				R = convert(col.X),
				G = convert(col.Y),
				B = convert(col.Z)
			};
		}

		public static Microsoft.Xna.Framework.Color FromWColor(Color col)
		{
			return new Microsoft.Xna.Framework.Color(col.R, col.G, col.B, col.A);
		}
	}
}
