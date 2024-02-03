using System;
using System.Collections.Generic;

namespace Core
{
    public class SN76489 : ClsChip
    {

        public const string PSGF_NUM = "PSGF-NUM";
        protected int[][] _FNumTbl = new int[1][] {
            new int[96]
        };
        private int beforePanData = -1;
        private int[] VolTbl = new int[16];
        private double[] DetuneTbl = new double[8 * 4];

        public SN76489(ClsVgm parent, int chipID, string initialPartName, string stPath, bool isSecondary) : base(parent, chipID, initialPartName, stPath, isSecondary)
        {
            _Name = "SN76489";
            _ShortName = "DCSG";
            _ChMax = 4;
            _canUsePcm = false;
            _canUsePI = false;
            FNumTbl = _FNumTbl;

            Frequency = 3579545;

            Dictionary<string, List<double>> dic = MakeFNumTbl();
            if (dic != null)
            {
                int c;
                if (dic.ContainsKey("FNUM_00"))
                {
                    c = 0;
                    foreach (double v in dic["FNUM_00"])
                    {
                        FNumTbl[0][c++] = (int)v;
                        if (c == FNumTbl[0].Length) break;
                    }
                }

                for (c = 0; c < 16; c++) VolTbl[c] = c;
                if (dic.ContainsKey("VOL"))
                {
                    c = 0;
                    foreach (double v in dic["VOL"])
                    {
                        VolTbl[c++] = (int)v;
                        if (c == VolTbl.Length) break;
                    }
                }

                if (dic.ContainsKey("DETUNE"))
                {
                    c = 0;
                    foreach (double v in dic["DETUNE"])
                    {
                        DetuneTbl[c++] = v;
                        if (c == DetuneTbl.Length) break;
                    }
                }
            }

            Ch = new ClsChannel[ChMax];
            char[] PART_DCSG = new char[] { 'D', 'E', 'F', 'G' };
            for (int i = 0; i < Ch.Length; i++)
            {
                if (Ch[i] == null) Ch[i] = new ClsChannel();
                Ch[i].Name = PART_DCSG[i].ToString();
            }

            foreach (ClsChannel ch in Ch)
            {
                ch.Type = enmChannelType.DCSG;
                ch.isSecondary = chipID == 1;
            }
            Ch[3].Type = enmChannelType.DCSGNOISE;

            Envelope = new Function();
            Envelope.Max = 15;
            Envelope.Min = 0;

        }

        public override void InitChip()
        {
            if (!use) return;
            if (IsSecondary) parent.dat[0x0f] |= 0x40;

            OutAllKeyOff();
        }

        public override void InitPart(ref partWork pw)
        {
            pw.MaxVolume = 15;
            pw.MaxVolumeEasy = 15;
            pw.volume = 0;// pw.MaxVolume;
            pw.port0 = 0x50;
            pw.keyOn = false;
            pw.panL = 3;
        }


        public int GetDcsgFNum(int octave, char noteCmd, int shift)
        {
            int o = octave - 1;
            int n = Const.NOTE.IndexOf(noteCmd) + shift;
            o += n / 12;
            o = Common.CheckRange(o, 0, 7);
            n %= 12;

            int f = o * 12 + n;
            if (f < 0) f = 0;
            if (f >= FNumTbl[0].Length) f = FNumTbl[0].Length - 1;

            return FNumTbl[0][f];
        }

        public void OutGGPsgStereoPort(partWork pw, byte data)
        {
            pw.OutData(
                (byte)(pw.isSecondary ? 0x3f : 0x4f)
                , data
                );
        }

        public void OutPsgPort(partWork pw, byte data)
        {
            pw.OutData(
                (byte)(pw.isSecondary ? 0x30 : 0x50)
                , data
                );
        }

        public void OutPsgKeyOn(partWork pw)
        {

            pw.keyOn = true;
            SetFNum(pw);
            SetVolume(pw);

        }

        public void OutPsgKeyOff(partWork pw)
        {

            if (!pw.envelopeMode) pw.keyOn = false;
            SetVolume(pw);

        }

        public void OutAllKeyOff()
        {

            foreach (partWork pw in lstPartWork)
            {
                pw.beforeFNum = -1;
                pw.beforeVolume = -1;

                pw.keyOn = false;
                OutPsgKeyOff(pw);
            }

        }

        public override void SetFNum(partWork pw)
        {
            if (pw.Type != enmChannelType.DCSGNOISE)
            {
                double f = -pw.detune;
                //f >>= pw.octaveNow - 1;
                int n = Const.NOTE.IndexOf(pw.noteCmd) + pw.shift;
                n = Common.CheckRange((pw.octaveNow - 1), 0, 7) * 4 + n / 4;
                f /= (DetuneTbl[n] == 0.0 ? 0.1 : DetuneTbl[n]);

                Log.Write(string.Format("Detune:n:{0}:f:{1}:DetuneTbl[n]:{2}", n, f, DetuneTbl[n]));


                int fl = 0;
                for (int lfo = 0; lfo < 1; lfo++)
                {
                    if (!pw.lfo[lfo].sw)
                    {
                        continue;
                    }
                    if (pw.lfo[lfo].type != enmLfoType.Vibrato)
                    {
                        continue;
                    }
                    fl = pw.lfo[lfo].value;
                    fl >>= pw.octaveNow - 1;
                }
                f -= fl;

                if (pw.bendWaitCounter != -1)
                {
                    f += pw.bendFnum;
                }
                else
                {
                    f += GetDcsgFNum(pw.octaveNow, pw.noteCmd, pw.shift);// + pw.keyShift + pw.relKeyShift);//
                }

                if (pw.freq == (int)f) return;
                pw.freq = (int)f;

                //OPN(AY) -> DCSG 変換
                double DMst = 3579545.0;
                double OMst = 7987200.0;
                int fi = (int)(Math.Round(DMst / OMst * 2.0 * f));
                fi = Common.CheckRange(fi, 0, 0x3ff);

                byte data = (byte)(0x80 + (pw.ch << 5) + (fi & 0xf));
                OutPsgPort(pw, data);

                data = (byte)((fi & 0x3f0) >> 4);
                OutPsgPort(pw, data);
            }
            else
            {
                int f = 0xe0 + (pw.noise & 7);
                if (pw.freq == f) return;
                pw.freq = f;
                byte data = (byte)f;
                OutPsgPort(pw, data);
            }

        }

        public override int GetFNum(partWork pw, int octave, char cmd, int shift)
        {
            return GetDcsgFNum(octave, cmd, shift);
        }

        public override void SetVolume(partWork pw)
        {
            byte data = 0;
            int vol = pw.volume;

            if (pw.keyOn)
            {
                if (pw.envelopeMode)
                {
                    if (pw.envIndex != -1)
                    {
                        vol = (int)((pw.volume + 1) * (pw.envCounter / 256.0));
                    }
                    else
                    {
                        pw.keyOn = false;
                        if (pw.ReverbNowSwitch)
                        {
                            vol += pw.ReverbValue + 4;
                            vol >>= 1;
                            vol -= 4;
                        }
                        else
                            vol = 0;
                    }
                }


                //for (int lfo = 0; lfo < 4; lfo++)
                //{
                //    if (!pw.lfo[lfo].sw) continue;
                //    if (pw.lfo[lfo].type != eLfoType.Tremolo) continue;

                //    vol += pw.lfo[lfo].value;// + pw.lfo[lfo].param[6];
                //}
            }
            else
            {
                if (pw.ReverbNowSwitch)
                {
                    //vol += pw.ReverbValue;
                    //vol >>= 1;
                    vol += pw.ReverbValue + 4;
                    vol >>= 1;
                    vol -= 4;
                }
                else
                {
                    vol = 0;
                }
            }

            vol = Common.CheckRange(vol, 0, 15);

            if (pw.beforeVolume != vol)
            {
                data = (byte)(0x80 + (pw.ch << 5) + 0x10 + (15 - VolTbl[vol]));
                Log.Write(string.Format("name:{0} channel:{1} vol:{2} volTbl:{3}", pw.chip.Name, pw.ch, vol, VolTbl[vol]));
                OutPsgPort(pw, data);
                pw.beforeVolume = vol;
            }
        }

        public override void SetKeyOn(partWork pw)
        {
            OutPsgKeyOn(pw);
        }

        public override void SetKeyOff(partWork pw)
        {
            pw.beforeVolume = -1;
            OutPsgKeyOff(pw);
        }

        public override void SetLfoAtKeyOn(partWork pw)
        {
            for (int lfo = 0; lfo < 1; lfo++)
            {
                clsLfo pl = pw.lfo[lfo];
                if (!pl.sw)
                    continue;

                //if (pl.param[5] != 1)
                  //  continue;

                pl.isEnd = false;
                //pl.value = (pl.param[0] == 0) ? pl.param[6] : 0;//ディレイ中は振幅補正は適用されない
                pl.waitCounter = parent.GetWaitCounter(pl.param[0]);
                pl.direction = Math.Sign(pl.param[2]);
                if (pl.direction == 0) pl.direction = 1;
                pl.value = 0;
                pl.PeakLevelCounter = pl.param[3]>>1;
            }
        }

        public override void SetToneDoubler(partWork pw,MML mml)
        {
            //実装不要
        }

        public override int GetToneDoublerShift(partWork pw, int octave, char noteCmd, int shift)
        {
            return 0;
        }


        public override void CmdY(partWork pw, MML mml)
        {
            if (mml.args[0] is string) return;

            byte adr = (byte)mml.args[0];
            byte dat = (byte)mml.args[1];

            OutPsgPort(pw, dat);
        }

        public override void CmdNoiseToneMixer(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 0, 7);
            pw.noise = n;
        }

        public override void CmdNoise(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 0, 0x3ff);

            byte data = (byte)(0xc0 + (n & 0xf));
            OutPsgPort(pw, data);

            data = (byte)(n >> 4);
            OutPsgPort(pw, data);
        }


        public override void CmdLoopExtProc(partWork pw, MML mml)
        {
        }

        public override void CmdRest(partWork pw, MML mml)
        {
            base.CmdRest(pw, mml);

            if (pw.envelopeMode)
            {
                if (pw.envIndex != -1)
                {
                    pw.envIndex = 3;
                }
            }
            else
            {
                SetKeyOff(pw);
            }

        }

        public override void CmdInstrument(partWork pw, MML mml)
        {
            char type = (char)mml.args[0];
            int n = 0;
            if (mml.args[1].GetType() == typeof(int))
            {
                n = (int)mml.args[1];
            }

            //Set Env
            SetEnvelopParamFromInstrument(pw, n, mml);
            //Set Mode
            ;
            //Set LFO
            SetLFOParamFromInstrument(pw, n, mml);

        }

        public override void CmdPan(partWork pw, MML mml)
        {
            int l = (int)mml.args[0];

            l = Common.CheckRange(l, 0, 3);
            pw.panL = l;
        }

        public override void MultiChannelCommand()
        {
            if (!use) return;
            int dat = 0;

            foreach (partWork pw in lstPartWork)
            {
                int p = pw.panL;
                dat |= (((p & 2) == 0 ? 0x00 : 0x10) | ((p & 1) == 0 ? 0x00 : 0x01)) << pw.ch;
            }

            if (beforePanData == dat) return;
            OutGGPsgStereoPort(lstPartWork[0], (byte)dat);
            beforePanData = dat;

        }

    }
}
