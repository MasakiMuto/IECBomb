using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSound
{
	public class GAManager
	{
		ItemPool pool;
		EvalManager eval;
		bool ready;

		public GAManager()
		{
			ready = false;
			pool = new ItemPool();
			eval = new EvalManager();
		}

		public void Start(SynthParam adam)
		{
			ready = true;
			pool.Reset();
			eval.Reset();
			pool.Init(adam);
		}

		public void Play(int index)
		{
			if (!ready) return;
			var param = pool[index];
			using (var synth = new SynthEngine())
			{
				synth.SynthFile(param).Play();
			}
		}

		public void Update(IEnumerable<int> saved)
		{

		}
		
		public void Save(int index)
		{
			if (!ready) return;
			using (var synth = new SynthEngine())
			{
				synth.SaveTo(pool[index], "test.wav");
			}
		}


	}
}
