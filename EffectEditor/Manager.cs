using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masa.IECBomb
{
	public class Manager
	{
		public static Manager Instance { get; private set; }

		ItemPool pool;
		EvalManager eval;
		int leftIndex, rightIndex;
		const int PoolSize = 20;
		Random rand;
		Dictionary<string, LockState> lockList;

		public Manager()
		{
			Instance = this;
			eval = new EvalManager();
			pool = new ItemPool(PoolSize);
			rand = new Random();
			lockList = new Dictionary<string, LockState>()
			{
				{"Mass", new LockState(ParameterName.Mass)},
				{"Vel", new LockState(ParameterName.Speed, ParameterName.SpeedVar, ParameterName.Accel, ParameterName.AccelVar)},
				{"Radius", new LockState(ParameterName.Radius, ParameterName.RadiusVar)},
				{"Alpha", new LockState(ParameterName.Alpha, ParameterName.AlphaVar, ParameterName.AlphaVel, ParameterName.AlphaVelVar, ParameterName.AlphaAccel, ParameterName.AlphaAccelVar)},
				{"H", new LockState(ParameterName.ColorH, ParameterName.ColorHVar)},
				{"S", new LockState(ParameterName.ColorS, ParameterName.ColorSVar)},
				{"V", new LockState(ParameterName.ColorV, ParameterName.ColorVVar)},
				
			};
			UnlockedParameters = new List<ParameterName>();
			UpdateParameterList();
			LockParams("H", true);
			LockParams("S", true);
			LockParams("V", true);
		}
		
		public void Play()
		{
			leftIndex = rand.Next(PoolSize);
			do
			{
				rightIndex = rand.Next(PoolSize);
			} while (rightIndex == leftIndex);
			EffectManager.Instance.Run(pool[leftIndex], 0);
			EffectManager.Instance.Run(pool[rightIndex], 1);
		}

		public void Reset()
		{
			pool.Reset();
			EvalManager.Instance.Reset();
		}

		public void Input(int left, int right)
		{
			int side = 0;
			if(left > right)
			{
				side = 1;
			}
			else if(left < right)
			{
				side = 2;
			}
			eval.RegistScore(pool[leftIndex], pool[rightIndex], side);
			//eval.RegistScore(pool[leftIndex], left);
			//eval.RegistScore(pool[rightIndex], right);
			//pool.UpdateGeneration();
			pool.UpdateDifferencial();
			EffectManager.Instance.Clear();
			Play();
		}

		class LockState
		{
			public ParameterName[] Params { get; private set; }
			public bool IsLocked { get; set; }

			public LockState(params ParameterName[] param)
			{
				IsLocked = false;
				Params = param;
			}
		}

		/// <summary>
		/// ロックされていないパラメタの数
		/// </summary>
		public int ParameterCount
		{
			get
			{
				return lockList.Where(x => !x.Value.IsLocked)
					.Sum(x => x.Value.Params.Length);
			}
		}

		public List<ParameterName> UnlockedParameters { get; private set; }

		void UpdateParameterList()
		{
			UnlockedParameters.Clear();
			UnlockedParameters.AddRange(lockList.Values.Where(x => !x.IsLocked).SelectMany(x => x.Params));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="locked">trueならロックする</param>
		public void LockParams(string name, bool locked)
		{
			lockList[name].IsLocked = locked;
			UpdateParameterList();
		}

		public void LockBy(bool isLeft)
		{
			pool.CloneParams(pool[isLeft ? leftIndex : rightIndex], lockList.Values.Where(x => x.IsLocked).SelectMany(x => x.Params));
		}

		public float[] GetScoreList()
		{
			return pool.Items.Select(x => eval.Eval(x)).ToArray();
		}
	}
}
