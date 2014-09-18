using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace IECSound
{
	public enum SoundType
	{
		PickUp,
		Laser,
		Explosion,
		Powerup,
		Hit,
		Jump,
		Blip,

	}

	public enum WaveType
	{
		Square,
		Saw,
		Sine,
		Noise
	}

	public enum ParamName
	{
		BaseFreq,
		FreqLimit,
		FreqRamp,
		FreqDRamp,
		Duty,
		DutyRamp,
		VibStrength,
		VibSpeed,
		VibDelay,
		EnvAttack,
		EnvSustain,
		EnvDecay,
		EnvPunch,
		//FilterOn,//?
		LpfResonance,
		LpfFreq,
		LpfRamp,
		HpfFreq,
		HpfRamp,
		PhaOffset,
		PhaRamp,
		RepeatSpeed,
		ArpSpeed,
		ArpMod,
	}

	public class SynthParam : Masa.IECBomb.ItemBase<ParamName>
	{
		public float base_freq { get { return this[ParamName.BaseFreq]; } set { this[ParamName.BaseFreq] = value; } }
		public float freq_limit { get { return this[ParamName.FreqLimit]; } set { this[ParamName.FreqLimit] = value; } }
		public float freq_ramp { get { return this[ParamName.FreqRamp]; } set { this[ParamName.FreqRamp] = value; } }
		public float freq_dramp { get { return this[ParamName.FreqDRamp]; } set { this[ParamName.FreqDRamp] = value; } }
		public float duty { get { return this[ParamName.Duty]; } set { this[ParamName.Duty] = value; } }
		public float duty_ramp { get { return this[ParamName.DutyRamp]; } set { this[ParamName.DutyRamp] = value; } }

		public float vib_strength { get { return this[ParamName.VibStrength]; } set { this[ParamName.VibStrength] = value; } }
		public float vib_speed { get { return this[ParamName.VibSpeed]; } set { this[ParamName.VibSpeed] = value; } }
		public float vib_delay { get { return this[ParamName.VibDelay]; } set { this[ParamName.VibDelay] = value; } }

		public float env_attack { get { return this[ParamName.EnvAttack]; } set { this[ParamName.EnvAttack] = value; } }
		public float env_sustain { get { return this[ParamName.EnvSustain]; } set { this[ParamName.EnvSustain] = value; } }
		public float env_decay { get { return this[ParamName.EnvDecay]; } set { this[ParamName.EnvDecay] = value; } }
		public float env_punch { get { return this[ParamName.EnvPunch]; } set { this[ParamName.EnvPunch] = value; } }

		//public bool filter_on;
		public float lpf_resonance { get { return this[ParamName.LpfResonance]; } set { this[ParamName.LpfResonance] = value; } }
		public float lpf_freq { get { return this[ParamName.LpfFreq]; } set { this[ParamName.LpfFreq] = value; } }
		public float lpf_ramp { get { return this[ParamName.LpfRamp]; } set { this[ParamName.LpfRamp] = value; } }
		public float hpf_freq { get { return this[ParamName.HpfFreq]; } set { this[ParamName.HpfFreq] = value; } }
		public float hpf_ramp { get { return this[ParamName.HpfRamp]; } set { this[ParamName.HpfRamp] = value; } }

		public float pha_offset { get { return this[ParamName.PhaOffset]; } set { this[ParamName.PhaOffset] = value; } }
		public float pha_ramp { get { return this[ParamName.PhaRamp]; } set { this[ParamName.PhaRamp] = value; } }

		public float repeat_speed { get { return this[ParamName.RepeatSpeed]; } set { this[ParamName.RepeatSpeed] = value; } }

		public float arp_speed { get { return this[ParamName.ArpSpeed]; } set { this[ParamName.ArpSpeed] = value; } }
		public float arp_mod { get { return this[ParamName.ArpMod]; } set { this[ParamName.ArpMod] = value; } }

		public WaveType wave_type;

		static Random rand = new Random();

		static int TotalIndex = 0;

		public SynthParam()
			: base(TotalIndex)
		{
			TotalIndex++;
			Params = new[]{
				CreateParam(ParamName.BaseFreq, 1.5f, -.5f),
				CreateParam(ParamName.FreqLimit, 1.5f, -1f),
				CreateParam(ParamName.FreqRamp, 1f, -1f),
				CreateParam(ParamName.FreqDRamp, 1f, -1f),
				CreateParam(ParamName.Duty, 1f, -1f),
				CreateParam(ParamName.DutyRamp, 1f, -1f),
				CreateParam(ParamName.VibStrength, 1f, -1f),
				CreateParam(ParamName.VibSpeed, 1f, -1f),
				CreateParam(ParamName.VibDelay, 1f, -1f),
				CreateParam(ParamName.EnvAttack, 1f, -1f),
				CreateParam(ParamName.EnvSustain, 1f, 0f),
				CreateParam(ParamName.EnvDecay, 1f, -1f),
				CreateParam(ParamName.EnvPunch, .8f, 0f),
				//CreateParam(ParamName.FilterOn,	  0, 0),
				CreateParam(ParamName.LpfResonance, 1, -1),
				CreateParam(ParamName.LpfFreq, 2, 0),
				CreateParam(ParamName.LpfRamp, 1, -1),
				CreateParam(ParamName.HpfFreq, 1, 0),
				CreateParam(ParamName.HpfRamp, 1, -1),
				CreateParam(ParamName.PhaOffset, 1, -1),
				CreateParam(ParamName.PhaRamp, 1, -1),
				CreateParam(ParamName.RepeatSpeed, 1, -1),
				CreateParam(ParamName.ArpSpeed, 1, -1),
				CreateParam(ParamName.ArpMod, 1, -1),
			};
		}

		public override bool Equals(object obj)
		{
			var p = obj as SynthParam;
			return base.Equals(obj) 
				//&& p.filter_on == this.filter_on 
				&& p.wave_type == this.wave_type;
		}

		static float frnd(float max)
		{
			return (float)rand.NextDouble() * max;
		}

		public static SynthParam Init(SoundType type)
		{
			var p = new SynthParam();
			p.base_freq = .3f;
			p.freq_limit = 0f;
			p.freq_ramp = 0;
			p.freq_dramp = 0;
			p.duty = 0;
			p.duty_ramp = 0;
			p.vib_strength = 0;
			p.vib_speed = 0;
			p.vib_delay = 0;

			p.env_attack = 0;
			p.env_sustain = .3f;
			p.env_decay = .4f;
			p.env_punch = 0;
			//p.filter_on = false;

			p.lpf_resonance = 0;
			p.lpf_freq = 1f;
			p.lpf_ramp = 0;
			p.hpf_freq = 0;
			p.hpf_ramp = 0;
			p.pha_offset = 0;
			p.pha_ramp = 0;
			p.repeat_speed = 0;
			p.arp_speed = 0;
			p.arp_mod = 0;
			p.wave_type = WaveType.Square;
			switch (type)
			{
				case SoundType.PickUp:
					p.base_freq = .4f + frnd(.5f);
					p.env_attack = 0f;
					p.env_sustain = frnd(.1f);
					p.env_decay = .1f + frnd(.4f);
					p.env_punch = .3f + frnd(.3f);
					if (rand.Next(2) == 0)
					{
						p.arp_speed = .5f + frnd(.2f);
						p.arp_mod = .2f + frnd(.4f);
					}
					break;
				case SoundType.Laser:
					p.wave_type = (WaveType)rand.Next(3);
					if (p.wave_type == WaveType.Sine && rand.Next(2) == 0)
					{
						p.wave_type = (WaveType)rand.Next(2);
					}
					p.base_freq = .5f + frnd(.5f);
					p.freq_limit = p.base_freq - .2f - frnd(.6f);
					if (p.freq_limit > .2f)
					{
						p.freq_limit = .2f;
					}
					p.freq_ramp = .15f - frnd(.2f);
					if (rand.Next(3) == 0)
					{
						p.base_freq = .3f + frnd(.6f);
						p.freq_limit = frnd(.1f);
						p.freq_ramp = .35f - frnd(.3f);
					}
					if (rand.Next(2) == 0)
					{
						p.duty = frnd(.5f);
						p.duty_ramp = frnd(.2f);
					}
					else
					{
						p.duty = .4f + frnd(.5f);
						p.duty_ramp = -frnd(.7f);
					}
					p.env_attack = 0f;
					p.env_sustain = .1f + frnd(.2f);
					p.env_decay = frnd(.4f);
					if (rand.Next(2) == 0)
					{
						p.env_punch = frnd(.3f);
					}
					if (rand.Next(3) == 0)
					{
						p.pha_offset = frnd(.2f);
						p.pha_ramp = frnd(.2f);
					}
					if (rand.Next(2) == 0)
					{
						p.hpf_freq = frnd(.3f);
					}
					break;
				case SoundType.Explosion:
					p.wave_type = WaveType.Noise;
					if (rand.Next(2) == 0)
					{
						p.base_freq = 0.1f + frnd(0.4f);
						p.freq_ramp = -0.1f + frnd(0.4f);
					}
					else
					{
						p.base_freq = 0.2f + frnd(0.7f);
						p.freq_ramp = -0.2f - frnd(0.2f);
					}
					p.base_freq *= p.base_freq;
					if (rand.Next(5) == 0)
						p.freq_ramp = 0.0f;
					if (rand.Next(3) == 0)
						p.repeat_speed = 0.3f + frnd(0.5f);
					p.env_attack = 0.0f;
					p.env_sustain = 0.1f + frnd(0.3f);
					p.env_decay = frnd(0.5f);
					if (rand.Next(2) == 0)
					{
						p.pha_offset = -0.3f + frnd(0.9f);
						p.pha_ramp = -frnd(0.3f);
					}
					p.env_punch = 0.2f + frnd(0.6f);
					if (rand.Next(2) == 0)
					{
						p.vib_strength = frnd(0.7f);
						p.vib_speed = frnd(0.6f);
					}
					if (rand.Next(3) == 0)
					{
						p.arp_speed = 0.6f + frnd(0.3f);
						p.arp_mod = 0.8f - frnd(1.6f);
					}
					break;
				case SoundType.Powerup:
					if (rand.Next(2) == 0)
						p.wave_type = WaveType.Saw;
					else
						p.duty = frnd(0.6f);
					if (rand.Next(2) == 0)
					{
						p.base_freq = 0.2f + frnd(0.3f);
						p.freq_ramp = 0.1f + frnd(0.4f);
						p.repeat_speed = 0.4f + frnd(0.4f);
					}
					else
					{
						p.base_freq = 0.2f + frnd(0.3f);
						p.freq_ramp = 0.05f + frnd(0.2f);
						if (rand.Next(2) == 0)
						{
							p.vib_strength = frnd(0.7f);
							p.vib_speed = frnd(0.6f);
						}
					}
					p.env_attack = 0.0f;
					p.env_sustain = frnd(0.4f);
					p.env_decay = 0.1f + frnd(0.4f);
					break;
				case SoundType.Hit:
					p.wave_type = (WaveType)rand.Next(3);
					if (p.wave_type == WaveType.Sine)
						p.wave_type = WaveType.Noise;
					if (p.wave_type == WaveType.Square)
						p.duty = frnd(0.6f);
					p.base_freq = 0.2f + frnd(0.6f);
					p.freq_ramp = -0.3f - frnd(0.4f);
					p.env_attack = 0.0f;
					p.env_sustain = frnd(0.1f);
					p.env_decay = 0.1f + frnd(0.2f);
					if (rand.Next(2) == 0)
						p.hpf_freq = frnd(0.3f);
					break;
				case SoundType.Jump:
					p.duty = frnd(0.6f);
					p.base_freq = 0.3f + frnd(0.3f);
					p.freq_ramp = 0.1f + frnd(0.2f);
					p.env_attack = 0.0f;
					p.env_sustain = 0.1f + frnd(0.3f);
					p.env_decay = 0.1f + frnd(0.2f);
					if (rand.Next(2) == 0)
						p.hpf_freq = frnd(0.3f);
					if (rand.Next(2) == 0)
						p.lpf_freq = 1.0f - frnd(0.6f);
					break;
				case SoundType.Blip:
					p.wave_type = (WaveType)rand.Next(2);
					if (p.wave_type == WaveType.Square)
						p.duty = frnd(0.6f);
					p.base_freq = 0.2f + frnd(0.4f);
					p.env_attack = 0.0f;
					p.env_sustain = 0.1f + frnd(0.1f);
					p.env_decay = frnd(0.2f);
					p.hpf_freq = 0.1f;
					break;
				default:
					break;
			}
			return p;
		}

		public override float GetQuantityScore()
		{
			throw new NotImplementedException();
		}

		protected override IEnumerable<ParamName> UnlockedParameters
		{
			get { throw new NotImplementedException(); }
		}

		public override float CurrentScore
		{
			get { throw new NotImplementedException(); }
		}

		SynthParam Clone()
		{
			var p = new SynthParam();
			p.wave_type = wave_type;
			//p.filter_on = filter_on;
			for (int i = 0; i < p.Params.Length; i++)
			{
				p.Params[i].NormalizedValue = Params[i].NormalizedValue;
					
			}

			return p;
		}

		public static SynthParam Mutate(SynthParam origin)
		{
			const double ParamMutateRatio = 0.08;
			const float ParamMutateAmount = 0.3f;
			var p = origin.Clone();
			do
			{
				for (int i = 0; i < p.Params.Length; i++)
				{
					if (rand.NextDouble() < ParamMutateRatio)
					{
						p.Params[i].NormalizedValue += (float)(rand.NextDouble() * 2 - 1) * ParamMutateAmount;
					}
				}
			} while (p == origin);
			
			return p;
		}

		public SynthParam CrossOver(Random rand, SynthParam item2)
		{
			Debug.Assert(this != item2);
			var child = Clone();
			int i1, i2;
			CreateCrossIndexs(rand, out i1, out i2);

			for (int i = 0; i < Params.Length; i++)
			{
				if (i1 <= i && i < i2)
				{
					child.Params[i].NormalizedValue = item2.Params[i].NormalizedValue;
				}
			}
			if (this != child && item2 != child)
			{
				return child;
			}
			else
			{
				return CrossOver(rand, item2);
			}
		}

		//public float AttackTime, SustainTime, SustainPunch, DecayTime;//env_***

		//public float StartFreq;//base_freq
		//public float MinFreq;//freq_limit
		//public float Slide;//freq_ramp
		//public float DeltaSlide;//freq_dramp

		//public float VibratoDepth;//vib_strength
		//public float VibratoSpeed;//vib_speed

		//public float ChangeAmount;//arp.mod
		//public float ChangeSpeed;//arp.speed
		//public float SquareDuty;//duty
		//public float DutySweep;//duty_damp

		//public float RepeatSpeed;//repeat_speed

		//public float PhaserOffset;//pha_offset
		//public float PhaserSweep;//pha_ramp

		//public float LpFilterCutoff;//lpf_freq
		//public float LpFilterCutoffSweep;//lpf_ramp
		//public float LpFilterResonance;//lpf_resonance

		//public float HpFilterCutoff;//hpf_freq
		//public float HpFilterCutoffSweep;//hpf_ramp
	}

	

}
