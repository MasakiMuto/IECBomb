using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSound
{
	public class Manager
	{
		ItemPool pool;
		EvalManager eval;

		public Manager()
		{
			pool = new ItemPool();
			eval = new EvalManager();
		}

		public void Start(SynthParam adam)
		{
			pool.Reset();
			eval.Reset();
			pool.Init(adam);
		}
	}
}
