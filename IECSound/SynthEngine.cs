using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IECSound
{
	public class SynthEngine : IDisposable
	{
		const uint ChunkSize = 16;
		const ushort CompressionCode = 1;
		const ushort Channels = 1;
		const uint SampleRate = 44100;
		const ushort Bit = 16;
		const uint Bps = SampleRate * Bit / 8;
		const ushort BlockAlign = Bit / 8;

		long sampleCount;
		int acc;
		float sample;
		BinaryWriter writer;

		public System.Media.SoundPlayer SynthFile(SynthParam p)
		{
			var path = Path.GetTempFileName();
			SaveTo(p, path);
			return new System.Media.SoundPlayer(path);
		}

		public void SaveTo(SynthParam p, string path)
		{
			writer = new BinaryWriter(File.OpenWrite(path));
			long size = WriteHeader();
			sampleCount = 0;
			acc = 0;
			sample = 0;
			playing_sample = true;
			this.p = p;
			ResetSample(false);
			while (playing_sample)
			{
				SynthSample(256);
			}

			WriteFooter(size);
			writer.Close();
			
		}

		float master_vol = 0.05f;

		float sound_vol = 0.5f;


		bool playing_sample = false;
		int phase;
		double fperiod;
		double fmaxperiod;
		double fslide;
		double fdslide;
		int period;
		double square_duty;
		double square_slide;
		int env_stage;
		int env_time;
		int[] env_length = new int[3];
		double env_vol;
		double fphase;
		double fdphase;
		int iphase;
		double[] phaser_buffer = new double[1024];
		int ipp;
		double[] noise_buffer = new double[32];
		double fltp;
		double fltdp;
		double fltw;
		double fltw_d;
		double fltdmp;
		double fltphp;
		double flthp;
		double flthp_d;
		double vib_phase;
		double vib_speed;
		double vib_amp;
		int rep_time;
		int rep_limit;
		int arp_time;
		int arp_limit;
		double arp_mod;

		SynthParam p;


		int wave_type;

		void ResetSample(bool restart)
		{
			if (!restart)
				phase = 0;
			fperiod = 100.0 / (p.base_freq * p.base_freq + 0.001);
			period = (int)fperiod;
			fmaxperiod = 100.0 / (p.freq_limit * p.freq_limit + 0.001);
			fslide = 1.0 - Math.Pow((double)p.freq_ramp, 3.0) * 0.01;
			fdslide = -Math.Pow((double)p.freq_dramp, 3.0) * 0.000001;
			square_duty = 0.5f - p.duty * 0.5f;
			square_slide = -p.duty_ramp * 0.00005f;
			if (p.arp_mod >= 0.0f)
				arp_mod = 1.0 - Math.Pow((double)p.arp_mod, 2.0) * 0.9;
			else
				arp_mod = 1.0 + Math.Pow((double)p.arp_mod, 2.0) * 10.0;
			arp_time = 0;
			arp_limit = (int)(Math.Pow(1.0f - p.arp_speed, 2.0f) * 20000 + 32);
			if (p.arp_speed == 1.0f)
				arp_limit = 0;
			wave_type = (int)p.wave_type;

			if (!restart)
			{
				// reset filter
				fltp = 0.0f;
				fltdp = 0.0f;
				fltw = Math.Pow(p.lpf_freq, 3.0f) * 0.1f;
				fltw_d = 1.0f + p.lpf_ramp * 0.0001f;
				fltdmp = 5.0f / (1.0f + Math.Pow(p.lpf_resonance, 2.0f) * 20.0f) * (0.01f + fltw);
				if (fltdmp > 0.8f) fltdmp = 0.8f;
				fltphp = 0.0f;
				flthp = Math.Pow(p.hpf_freq, 2.0f) * 0.1f;
				flthp_d = 1.0 + p.hpf_ramp * 0.0003f;
				// reset vibrato
				vib_phase = 0.0f;
				vib_speed = Math.Pow(p.vib_speed, 2.0f) * 0.01f;
				vib_amp = p.vib_strength * 0.5f;
				// reset envelope
				env_vol = 0.0f;
				env_stage = 0;
				env_time = 0;
				env_length[0] = (int)(p.env_attack * p.env_attack * 100000.0f);
				env_length[1] = (int)(p.env_sustain * p.env_sustain * 100000.0f);
				env_length[2] = (int)(p.env_decay * p.env_decay * 100000.0f);

				fphase = Math.Pow(p.pha_offset, 2.0f) * 1020.0f;
				if (p.pha_offset < 0.0f) fphase = -fphase;
				fdphase = Math.Pow(p.pha_ramp, 2.0f) * 1.0f;
				if (p.pha_ramp < 0.0f) fdphase = -fdphase;
				iphase = Math.Abs((int)fphase);
				ipp = 0;
				for (int i = 0; i < 1024; i++)
					phaser_buffer[i] = 0.0f;

				for (int i = 0; i < 32; i++)
					noise_buffer[i] = frnd(2.0f) - 1.0f;

				rep_time = 0;
				rep_limit = (int)(Math.Pow(1.0f - p.repeat_speed, 2.0f) * 20000 + 32);
				if (p.repeat_speed == 0.0f)
					rep_limit = 0;
			}
		}

		void SynthSample(int length)
		{
			for (int i = 0; i < length; i++)
			{
				if (!playing_sample)
					break;

				rep_time++;
				if (rep_limit != 0 && rep_time >= rep_limit)
				{
					rep_time = 0;
					ResetSample(true);
				}

				// frequency envelopes/arpeggios
				arp_time++;
				if (arp_limit != 0 && arp_time >= arp_limit)
				{
					arp_limit = 0;
					fperiod *= arp_mod;
				}
				fslide += fdslide;
				fperiod *= fslide;
				if (fperiod > fmaxperiod)
				{
					fperiod = fmaxperiod;
					if (p.freq_limit > 0.0f)
						playing_sample = false;
				}
				var rfperiod = fperiod;
				if (vib_amp > 0.0f)
				{
					vib_phase += vib_speed;
					rfperiod = fperiod * (1.0 + Math.Sin(vib_phase) * vib_amp);
				}
				period = (int)rfperiod;
				if (period < 8) period = 8;
				square_duty += square_slide;
				if (square_duty < 0.0f) square_duty = 0.0f;
				if (square_duty > 0.5f) square_duty = 0.5f;
				// volume envelope
				env_time++;
				if (env_time > env_length[env_stage])
				{
					env_time = 0;
					env_stage++;
					if (env_stage == 3)
						playing_sample = false;
				}
				if (env_stage == 0)
					env_vol = (float)env_time / env_length[0];
				if (env_stage == 1)
					env_vol = 1.0f + Math.Pow(1.0f - (float)env_time / env_length[1], 1.0f) * 2.0f * p.env_punch;
				if (env_stage == 2)
					env_vol = 1.0f - (float)env_time / env_length[2];

				// phaser step
				fphase += fdphase;
				iphase = Math.Abs((int)fphase);
				if (iphase > 1023) iphase = 1023;

				if (flthp_d != 0.0f)
				{
					flthp *= flthp_d;
					if (flthp < 0.00001f) flthp = 0.00001f;
					if (flthp > 0.1f) flthp = 0.1f;
				}

				float ssample = 0.0f;
				for (int si = 0; si < 8; si++) // 8x supersampling
				{
					double sample = 0.0f;
					phase++;
					if (phase >= period)
					{
						//				phase=0;
						phase %= period;
						if (wave_type == 3)
							for (int j = 0; j < 32; j++)
								noise_buffer[j] = frnd(2.0f) - 1.0f;
					}
					// base waveform
					float fp = (float)phase / period;
					switch (wave_type)
					{
						case 0: // square
							if (fp < square_duty)
								sample = 0.5f;
							else
								sample = -0.5f;
							break;
						case 1: // sawtooth
							sample = 1.0f - fp * 2;
							break;
						case 2: // sine
							sample = (float)Math.Sin(fp * 2 * Math.PI);
							break;
						case 3: // noise
							sample = noise_buffer[phase * 32 / period];
							break;
						default:
							throw new Exception();
					}
					// lp filter
					double pp = fltp;
					fltw *= fltw_d;
					if (fltw < 0.0f) fltw = 0.0f;
					if (fltw > 0.1f) fltw = 0.1f;
					if (p.lpf_freq != 1.0f)
					{
						fltdp += (sample - fltp) * fltw;
						fltdp -= fltdp * fltdmp;
					}
					else
					{
						fltp = sample;
						fltdp = 0.0f;
					}
					fltp += fltdp;
					// hp filter
					fltphp += fltp - pp;
					fltphp -= fltphp * flthp;
					sample = fltphp;
					// phaser
					phaser_buffer[ipp & 1023] = sample;
					sample += phaser_buffer[(ipp - iphase + 1024) & 1023];
					ipp = (ipp + 1) & 1023;
					// final accumulation and envelope application
					ssample += (float)(sample * env_vol);
				}
				ssample = ssample / 8 * master_vol;

				ssample *= 2.0f * sound_vol;

				WriteValue((float)ssample);


			}
		}

		Random rand = new Random();

		double frnd(double max)
		{
			return rand.NextDouble() * max;
		}

		void WriteValue(float val)
		{
			val *= 4f;
			if (val > 1f)
			{
				val = 1f;
			}
			else if (val < -1f)
			{
				val = -1f;
			}
			sample += val;
			acc++;
			if (SampleRate == 44100 || acc == 2)
			{
				sample /= acc;
				acc = 0;
				if (Bit == 16)
				{
					short isample = (short)(sample * 32000);
					writer.Write(isample);
				}
				else
				{
					byte isample = (byte)(sample * 127 + 128);
					writer.Write(isample);
				}
				sample = 0f;
			}
			sampleCount++;
		}

		void WriteString(string s)
		{
			writer.Write(s.ToArray(), 0, s.Length);
		}

		long WriteHeader()
		{
			WriteString("RIFF");
			writer.Write((uint)0);
			WriteString("WAVE");

			WriteString("fmt ");
			writer.Write(ChunkSize);
			writer.Write(CompressionCode);
			writer.Write(Channels);
			writer.Write(SampleRate);
			writer.Write(Bps);
			writer.Write(BlockAlign);
			writer.Write(Bit);

			WriteString("data");
			writer.Flush();
			var size = writer.BaseStream.Position;
			writer.Write((uint)0);

			return size;
		}

		void WriteFooter(long size)
		{
			writer.BaseStream.Seek(4, SeekOrigin.Begin);
			uint w = (uint)(size - 4 + sampleCount * Bit / 8);
			writer.Write(w);
			writer.BaseStream.Seek((int)size, SeekOrigin.Begin);
			w = (uint)(sampleCount * Bit / 8);
			writer.Write(w);
		}

		public void Dispose()
		{
			if (writer != null)
			{
				writer.Close();
				writer.Dispose();
				writer = null;
			}
			GC.SuppressFinalize(this);
		}

		~SynthEngine()
		{
			Dispose();
		}
	}
}
