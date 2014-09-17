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
		public bool Ready { get; private set; }

		public GAManager()
		{
			Ready = false;
			pool = new ItemPool();
			eval = new EvalManager();
		}

		public void Start(SynthParam adam)
		{
			Ready = true;
			pool.Reset();
			eval.Reset();
			pool.Init(adam);
		}

		public void Play(int index)
		{
			if (!Ready) return;
			var param = pool[index];
			using (var synth = new SynthEngine())
			{
				synth.SynthFile(param).Play();
			}
		}

		public Task PlaySync(int index)
		{
			return Task.Run(()=>{
				if (!Ready) return;
				using (var synth = new SynthEngine())
				{
					synth.SynthFile(pool[index]).PlaySync();
				}
			}
			);
		}

		public void Update(IEnumerable<int> saved)
		{

		}
		
		public void Save(int index)
		{
			if (!Ready) return;
			using (var synth = new SynthEngine())
			{
				synth.SaveTo(pool[index], "test.wav");
			}
		}


	}
}
