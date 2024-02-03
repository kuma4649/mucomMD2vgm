using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ClsOPN : ClsChip
    {
        public byte SSGKeyOn = 0x3f;


        public ClsOPN(ClsVgm parent, int chipID, string initialPartName, string stPath, bool isSecondary) : base(parent, chipID, initialPartName, stPath, isSecondary)
        {
        }


        public void OutSsgKeyOn(partWork pw)
        {
            int m = 3;
            byte pch = (byte)(pw.ch - (m + 6));
            int n = (pw.mixer & 0x1) + ((pw.mixer & 0x2) << 2);
            byte data = 0;

            data = (byte)(((ClsOPN)pw.chip).SSGKeyOn | (9 << pch));
            data &= (byte)(~(n << pch));
            ((ClsOPN)pw.chip).SSGKeyOn = data;

            SetSsgVolume(pw);
            if (pw.HardEnvelopeSw)
            {
                pw.OutData(pw.port0, 0x0d, (byte)(pw.HardEnvelopeType & 0xf));
            }
            pw.OutData(pw.port0, 0x07, data);
        }

        public void OutSsgKeyOff(partWork pw)
        {
            int m = 3;
            byte pch = (byte)(pw.ch - (m + 6));
            int n = 9;
            byte data = 0;

            data = (byte)(((ClsOPN)pw.chip).SSGKeyOn | (n << pch));
            ((ClsOPN)pw.chip).SSGKeyOn = data;

            pw.OutData(pw.port0, (byte)(0x08 + pch), 0);
            pw.beforeVolume = -1;
            pw.OutData(pw.port0, 0x07, data);

        }

        public void SetSsgVolume(partWork pw)
        {
            int m = 3;
            byte pch = (byte)(pw.ch - (m + 6));

            int vol = pw.volume;
            if (pw.ReverbNowSwitch)
            {
                vol += pw.ReverbValue + 4;
                vol >>= 1;
                vol -= 4;
            }
            if (pw.envelopeMode)
            {
                vol = 0;
                if (pw.envIndex != -1)
                {
                    vol = pw.volume - (15 - pw.envVolume);
                }
            }

            for (int lfo = 0; lfo < 4; lfo++)
            {
                if (!pw.lfo[lfo].sw) continue;
                if (pw.lfo[lfo].type != enmLfoType.Tremolo) continue;

                vol += pw.lfo[lfo].value;// + pw.lfo[lfo].param[6];
            }

            vol = Common.CheckRange(vol, 0, 15) + (pw.HardEnvelopeSw ? 0x10 : 0x00);

            if (pw.beforeVolume != vol)
            {
                pw.OutData(pw.port0, (byte)(0x08 + pch), (byte)vol);
                //pw.beforeVolume = pw.volume;
                pw.beforeVolume = vol;
            }
        }

        public void OutSsgNoise(partWork pw, int n)
        {
            pw.OutData(pw.port0, 0x06, (byte)(n & 0x1f));
        }

        public void SetSsgFNum(partWork pw)
        {
            int f = GetSsgFNum(pw, pw.octaveNow, pw.noteCmd, pw.shift);// + pw.keyShift + pw.relKeyShift);//
            if (pw.bendWaitCounter != -1)
            {
                f = pw.bendFnum;
            }
            f = f + pw.detune;
            for (int lfo = 0; lfo < 4; lfo++)
            {
                if (!pw.lfo[lfo].sw)
                {
                    continue;
                }
                if (pw.lfo[lfo].type != enmLfoType.Vibrato)
                {
                    continue;
                }
                f += pw.lfo[lfo].value;// + pw.lfo[lfo].param[6];
            }

            f = Common.CheckRange(f, 0, 0xfff);
            if (pw.freq == f) return;

            pw.freq = f;

            byte data = 0;
            int n = 9;

            data = (byte)(f & 0xff);
            pw.OutData(pw.port0, (byte)(0 + (pw.ch - n) * 2), data);

            data = (byte)((f & 0xf00) >> 8);
            pw.OutData(pw.port0, (byte)(1 + (pw.ch - n) * 2), data);
        }

        public int GetSsgFNum(partWork pw, int octave, char noteCmd, int shift)
        {
            int o = octave - 1;
            int n = Const.NOTE.IndexOf(noteCmd) + shift;
            o += n / 12;
            o = Common.CheckRange(o, 0, 7);
            n %= 12;

            int f = o * 12 + n;
            if (f < 0) f = 0;
            if (f >= pw.chip.FNumTbl[1].Length) f = pw.chip.FNumTbl[1].Length - 1;

            return pw.chip.FNumTbl[1][f];
        }


        public void OutOPNSetPanAMSPMS(partWork pw, int pan, int ams, int pms)
        {
            partWork ppw = pw;
            if (pw.PartName == "K")
            {
                ppw = pw.chip.lstPartWork[5];
                ams = ppw.ams;
                pms = ppw.pms;
            }
            int vch = ppw.ch;
            byte port = ppw.ch > 2 ? ppw.port1 : ppw.port0;
            if ("LMN".IndexOf(pw.PartName) >= 0)
            {
                //効果音モードパートの場合はch3に補正
                vch = 2;
                port = ppw.port0;
            }
            vch = (byte)(vch > 2 ? vch - 3 : vch);

            pan &= 3;
            ams &= 3;
            pms &= 7;

            ppw.OutData(port, (byte)(0xb4 + vch), (byte)((pan << 6) | (ams << 4) | pms));
        }

        public void OutOPNSetHardLfo(partWork pw, bool sw, int lfoNum)
        {
            pw.OutData(
                pw.port0
                , 0x22
                , (byte)((lfoNum & 7) + (sw ? 8 : 0))
                );
        }

        public void OutOPNSetCh3SpecialMode(partWork pw, bool sw)
        {
            // ignore Timer ^^;
            pw.OutData(
                pw.port0
                , 0x27
                , (byte)((sw ? 0x40 : 0))
                );
        }

        public void OutFmSetFeedbackAlgorithm(partWork pw, int fb, int alg)
        {
            int vch = pw.ch;
            byte port = pw.ch > 2 ? pw.port1 : pw.port0;
            vch = (byte)(vch > 2 ? vch - 3 : vch);

            fb &= 7;
            alg &= 7;

            pw.OutData(port, (byte)(0xb0 + vch), (byte)((fb << 3) + alg));
        }

        public void OutFmSetDtMl(partWork pw, int ope, int dt, int ml)
        {
            int vch = pw.ch;
            byte port = vch > 2 ? pw.port1 : pw.port0;
            vch = (byte)(vch > 2 ? vch - 3 : vch);

            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            dt &= 7;
            ml &= 15;

            pw.OutData(port, (byte)(0x30 + vch + ope * 4), (byte)((dt << 4) + ml));
        }

        public void OutFmSetTl(partWork pw, int ope, int tl)
        {
            byte port = (pw.ch > 2 ? pw.port1 : pw.port0);
            int vch = (byte)(pw.ch > 2 ? pw.ch - 3 : pw.ch);

            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            tl &= 0x7f;

            switch (ope)
            {
                case 0:
                    if (pw.beforeTLOP1 == tl) return;
                    pw.beforeTLOP1 = tl;
                    break;
                case 1:
                    if (pw.beforeTLOP3 == tl) return;
                    pw.beforeTLOP3 = tl;
                    break;
                case 2:
                    if (pw.beforeTLOP2 == tl) return;
                    pw.beforeTLOP2 = tl;
                    break;
                case 3:
                    if (pw.beforeTLOP4 == tl) return;
                    pw.beforeTLOP4 = tl;
                    break;
            }
            pw.OutData(port, (byte)(0x40 + vch + ope * 4), (byte)tl);
        }

        public void OutFmSetKsAr(partWork pw, int ope, int ks, int ar)
        {
            int vch = pw.ch;
            byte port = (pw.ch > 2 ? pw.port1 : pw.port0);
            vch = (byte)(vch > 2 ? vch - 3 : vch);

            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            ks &= 3;
            ar &= 31;

            pw.OutData(port, (byte)(0x50 + vch + ope * 4), (byte)((ks << 6) + ar));
        }

        public void OutFmSetAmDr(partWork pw, int ope, int am, int dr)
        {
            int vch = pw.ch;
            byte port = (pw.ch > 2 ? pw.port1 : pw.port0);
            vch = (byte)(vch > 2 ? vch - 3 : vch);

            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            am &= 1;
            dr &= 31;

            pw.OutData(port, (byte)(0x60 + vch + ope * 4), (byte)((am << 7) + dr));
        }

        public void OutFmSetSr(partWork pw, int ope, int sr)
        {
            int vch = pw.ch;
            byte port = pw.ch > 2 ? pw.port1 : pw.port0;
            vch = (byte)(vch > 2 ? vch - 3 : vch);

            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            sr &= 31;

            pw.OutData(port, (byte)(0x70 + vch + ope * 4), (byte)(sr));
        }

        public void OutFmSetSlRr(partWork pw, int ope, int sl, int rr)
        {
            int vch = pw.ch;
            byte port = pw.ch > 2 ? pw.port1 : pw.port0;
            vch = (byte)(vch > 2 ? vch - 3 : vch);

            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            sl &= 15;
            rr &= 15;

            pw.OutData(port, (byte)(0x80 + vch + ope * 4), (byte)((sl << 4) + rr));
        }

        public void OutFmSetSSGEG(partWork pw, int ope, int n)
        {
            int vch = pw.ch;
            byte port = pw.ch > 2 ? pw.port1 : pw.port0;
            vch = (byte)(vch > 2 ? vch - 3 : vch);

            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            n &= 15;

            pw.OutData(port, (byte)(0x90 + vch + ope * 4), (byte)n);
        }

        /// <summary>
        /// FMボリュームの設定
        /// </summary>
        /// <param name="ch">チャンネル</param>
        /// <param name="vol">ボリューム値</param>
        public void OutFmSetVolume(partWork pw, int vol)//, int n)
        {
            //if (!parent.instFM.ContainsKey(n))
            //{
            //    msgBox.setWrnMsg(string.Format(msg.get("E11000"), n), pw.getSrcFn(), pw.getLineNumber());
            //    return;
            //}

            int alg = 0;
            int[] ope = null;

            alg = pw.algo;
            ope = new int[4] {
                        pw.v_tl[0]
                        ,pw.v_tl[1]
                        ,pw.v_tl[2]
                        ,pw.v_tl[3]
                    };

            //switch (parent.instFM[n].type)
            //{
            //    case 0:// @
            //        alg = pw.algo;// parent.instFM[n].data[1] & 0x7;
            //        ope = new int[4] {
            //            //parent.instFM[n].data[0*Const.INSTRUMENT_OPERATOR_SIZE + 5+2]
            //            //, parent.instFM[n].data[1 * Const.INSTRUMENT_OPERATOR_SIZE + 5+2]
            //            //, parent.instFM[n].data[2 * Const.INSTRUMENT_OPERATOR_SIZE + 5+2]
            //            //, parent.instFM[n].data[3 * Const.INSTRUMENT_OPERATOR_SIZE + 5+2]
            //            pw.v_tl[0]
            //            ,pw.v_tl[1]
            //            ,pw.v_tl[2]
            //            ,pw.v_tl[3]
            //        };
            //        break;
            //    case 1:// @%
            //        alg = pw.algo;// parent.instFM[n].data[24] & 0x7;
            //        ope = new int[4] {
            //            //parent.instFM[n].data[4]
            //            //, parent.instFM[n].data[6]
            //            //, parent.instFM[n].data[5]
            //            //, parent.instFM[n].data[7]
            //            pw.v_tl[0]
            //            ,pw.v_tl[1]
            //            ,pw.v_tl[2]
            //            ,pw.v_tl[3]
            //        };
            //        break;
            //    case 2:// @N
            //        alg = pw.algo;// parent.instFM[n].data[1] & 0x7;
            //        ope = new int[4] {
            //            //parent.instFM[n].data[0 * 11 + 5+2]
            //            //, parent.instFM[n].data[1 * 11 + 5+2]
            //            //, parent.instFM[n].data[2 * 11 + 5+2]
            //            //, parent.instFM[n].data[3 * 11 + 5+2]
            //            pw.v_tl[0]
            //            ,pw.v_tl[1]
            //            ,pw.v_tl[2]
            //            ,pw.v_tl[3]
            //        };
            //        break;
            //    case 3:// @L OPL
            //        msgBox.setErrMsg(string.Format(msg.get("E11000"), n), pw.getSrcFn(), pw.getLineNumber());
            //        return;
            //    case 4:// @M OPM
            //        alg = pw.algo;// parent.instFM[n].data[1] & 0x7;
            //        ope = new int[4] {
            //            //parent.instFM[n].data[0 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5 + 3]
            //            //, parent.instFM[n].data[1 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5 + 3]
            //            //, parent.instFM[n].data[2 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5 + 3]
            //            //, parent.instFM[n].data[3 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5 + 3]
            //            pw.v_tl[0]
            //            ,pw.v_tl[1]
            //            ,pw.v_tl[2]
            //            ,pw.v_tl[3]
            //        };
            //        break;
            //}

            int[][] algs = new int[8][]
            {
                new int[4] { 0,0,0,1}
                ,new int[4] { 0,0,0,1}
                ,new int[4] { 0,0,0,1}
                ,new int[4] { 0,0,0,1}
                ,new int[4] { 0,1,0,1}
                ,new int[4] { 0,1,1,1}
                ,new int[4] { 0,1,1,1}
                ,new int[4] { 1,1,1,1}
            };

            for (int i = 0; i < 4; i++)
            {
                if (algs[alg][i] == 0 || (pw.slots & (1 << i)) == 0)
                {
                    ope[i] = -1;
                    continue;
                }
                //ope[i] = ope[i] + (127 - vol);
                ope[i] = vol;
                ope[i] = Common.CheckRange(ope[i], 0, 127);
            }

            partWork vpw = pw;
            if (pw.chip.lstPartWork[2].Ch3SpecialMode && pw.ch >= 7 && pw.ch < 10)
            {
                vpw = pw.chip.lstPartWork[2];
            }

            if ((pw.slots & 1) != 0 && ope[0] != -1) ((ClsOPN)pw.chip).OutFmSetTl(vpw, 0, ope[0]);
            if ((pw.slots & 2) != 0 && ope[1] != -1) ((ClsOPN)pw.chip).OutFmSetTl(vpw, 1, ope[1]);
            if ((pw.slots & 4) != 0 && ope[2] != -1) ((ClsOPN)pw.chip).OutFmSetTl(vpw, 2, ope[2]);
            if ((pw.slots & 8) != 0 && ope[3] != -1) ((ClsOPN)pw.chip).OutFmSetTl(vpw, 3, ope[3]);
        }

        public void OutFmCh3SpecialModeSetFnum(partWork pw, byte ope, int octave, int num)
        {
            ope &= 3;
            if (ope == 0)
            {
                pw.OutData(pw.port0, 0xa6, (byte)(((num & 0x700) >> 8) + (((octave - 1) & 0x7) << 3)));
                pw.OutData(pw.port0, 0xa2, (byte)(num & 0xff));
            }
            else
            {
                pw.OutData(pw.port0, (byte)(0xac + ope), (byte)(((num & 0x700) >> 8) + (((octave - 1) & 0x7) << 3)));
                pw.OutData(pw.port0, (byte)(0xa8 + ope), (byte)(num & 0xff));
            }
        }

        public void OutFmSetInstrument(partWork pw, int n, int vol)
        {

            if (!parent.instFM.ContainsKey(n))
            {
                msgBox.setWrnMsg(string.Format(msg.get("E11001"), n), pw.getSrcFn(), pw.getLineNumber());
                return;
            }

            int m = 3;

            if (pw.ch >= m + 3 && pw.ch < m + 6)
            {
                msgBox.setWrnMsg(msg.get("E11002"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }

            switch (parent.instFM[n].type)
            {
                case 0:// @
                    for (int ope = 0; ope < 4; ope++) ((ClsOPN)pw.chip).OutFmSetSlRr(pw, ope, 0, 15);

                    for (int ope = 0; ope < 4; ope++)
                    {
                        ((ClsOPN)pw.chip).OutFmSetDtMl(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 8 + 2], parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 7 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetKsAr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 6 + 2], parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 0 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetAmDr(pw, ope, 1, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 1 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetSr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 2 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetSlRr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 4 + 2], parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 3 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetSSGEG(pw, ope, 0);
                        ((ClsOPN)pw.chip).OutFmSetTl(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 5 + 2]);
                        pw.v_tl[ope] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 5 + 2];
                    }

                    pw.op1ml = parent.instFM[n].data[0 * Const.INSTRUMENT_OPERATOR_SIZE + 7 + 2];
                    pw.op2ml = parent.instFM[n].data[1 * Const.INSTRUMENT_OPERATOR_SIZE + 7 + 2];
                    pw.op3ml = parent.instFM[n].data[2 * Const.INSTRUMENT_OPERATOR_SIZE + 7 + 2];
                    pw.op4ml = parent.instFM[n].data[3 * Const.INSTRUMENT_OPERATOR_SIZE + 7 + 2];
                    pw.op1dt2 = 0;
                    pw.op2dt2 = 0;
                    pw.op3dt2 = 0;
                    pw.op4dt2 = 0;

                    ((ClsOPN)pw.chip).OutFmSetFeedbackAlgorithm(pw, parent.instFM[n].data[0], parent.instFM[n].data[1]);
                    pw.feedback = parent.instFM[n].data[0];
                    pw.algo = parent.instFM[n].data[1];
                    break;
                case 1: // @%
                    int vch = pw.ch;
                    byte port = vch > 2 ? pw.port1 : pw.port0;
                    vch = (byte)(vch > 2 ? vch - 3 : vch);

                    for (int ope = 0; ope < 4; ope++) ((ClsOPN)pw.chip).OutFmSetSlRr(pw, ope, 0, 15);

                    for (int ope = 0; ope < 4; ope++)
                    {
                        pw.OutData(port, (byte)(0x30 + vch + ope * 4), parent.instFM[n].data[ope]); //DT/ML
                        pw.OutData(port, (byte)(0x40 + vch + ope * 4), parent.instFM[n].data[ope + 4]); //TL
                        pw.OutData(port, (byte)(0x50 + vch + ope * 4), parent.instFM[n].data[ope + 8]); //KS/AR
                        pw.OutData(port, (byte)(0x60 + vch + ope * 4), (byte)(parent.instFM[n].data[ope + 12] | 0x80)); //AMON/DR
                        pw.OutData(port, (byte)(0x70 + vch + ope * 4), parent.instFM[n].data[ope + 16]); //SR
                        pw.OutData(port, (byte)(0x80 + vch + ope * 4), parent.instFM[n].data[ope + 20]); //SL/RR
                        pw.v_tl[ope] = parent.instFM[n].data[ope + 4];
                    }
                    pw.OutData(port, (byte)(0xb0 + vch), parent.instFM[n].data[24]); //FB/AL
                    pw.feedback = (parent.instFM[n].data[24] & 0x38) >> 3;
                    pw.algo = parent.instFM[n].data[24] & 0x7;
                    break;
                case 2: //@N
                    for (int ope = 0; ope < 4; ope++) ((ClsOPN)pw.chip).OutFmSetSlRr(pw, ope, 0, 15);

                    for (int ope = 0; ope < 4; ope++)
                    {
                        ((ClsOPN)pw.chip).OutFmSetDtMl(pw, ope, parent.instFM[n].data[ope * 11 + 8 + 2], parent.instFM[n].data[ope * 11 + 7 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetKsAr(pw, ope, parent.instFM[n].data[ope * 11 + 6 + 2], parent.instFM[n].data[ope * 11 + 0 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetAmDr(pw, ope, parent.instFM[n].data[ope * 11 + 9 + 2], parent.instFM[n].data[ope * 11 + 1 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetSr(pw, ope, parent.instFM[n].data[ope * 11 + 2 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetSlRr(pw, ope, parent.instFM[n].data[ope * 11 + 4 + 2], parent.instFM[n].data[ope * 11 + 3 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetSSGEG(pw, ope, parent.instFM[n].data[ope * 11 + 10 + 2]);
                        ((ClsOPN)pw.chip).OutFmSetTl(pw, ope, parent.instFM[n].data[ope * 11 + 5 + 2]);
                        pw.v_tl[ope] = parent.instFM[n].data[ope * 11 + 5 + 2];
                    }

                    pw.op1ml = parent.instFM[n].data[0 * 11 + 7 + 2];
                    pw.op2ml = parent.instFM[n].data[1 * 11 + 7 + 2];
                    pw.op3ml = parent.instFM[n].data[2 * 11 + 7 + 2];
                    pw.op4ml = parent.instFM[n].data[3 * 11 + 7 + 2];
                    pw.op1dt2 = 0;
                    pw.op2dt2 = 0;
                    pw.op3dt2 = 0;
                    pw.op4dt2 = 0;

                    ((ClsOPN)pw.chip).OutFmSetFeedbackAlgorithm(pw, parent.instFM[n].data[0], parent.instFM[n].data[1]);
                    pw.feedback = parent.instFM[n].data[0];
                    pw.algo = parent.instFM[n].data[1];
                    break;
                case 3://@L OPL
                    msgBox.setErrMsg(msg.get("E11001"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                case 4://@M OPM

                    for (int ope = 0; ope < 4; ope++)
                    {
                        ((ClsOPN)pw.chip).OutFmSetDtMl(pw, ope, parent.instFM[n].data[ope * 11 + 8 + 3], parent.instFM[n].data[ope * 11 + 7 + 3]);
                        ((ClsOPN)pw.chip).OutFmSetKsAr(pw, ope, parent.instFM[n].data[ope * 11 + 6 + 3], parent.instFM[n].data[ope * 11 + 0 + 3]);
                        ((ClsOPN)pw.chip).OutFmSetAmDr(pw, ope, parent.instFM[n].data[ope * 11 + 10 + 3], parent.instFM[n].data[ope * 11 + 1 + 3]);
                        ((ClsOPN)pw.chip).OutFmSetSr(pw, ope, parent.instFM[n].data[ope * 11 + 2 + 3]);
                        ((ClsOPN)pw.chip).OutFmSetSlRr(pw, ope, parent.instFM[n].data[ope * 11 + 4 + 3], parent.instFM[n].data[ope * 11 + 3 + 3]);
                        ((ClsOPN)pw.chip).OutFmSetSSGEG(pw, ope, 0);
                        ((ClsOPN)pw.chip).OutFmSetTl(pw, ope, parent.instFM[n].data[ope * 11 + 5 + 3]);
                        pw.v_tl[ope] = parent.instFM[n].data[ope * 11 + 5 + 3];
                    }

                    pw.op1ml = parent.instFM[n].data[0 * 11 + 7 + 3];
                    pw.op2ml = parent.instFM[n].data[1 * 11 + 7 + 3];
                    pw.op3ml = parent.instFM[n].data[2 * 11 + 7 + 3];
                    pw.op4ml = parent.instFM[n].data[3 * 11 + 7 + 3];
                    pw.op1dt2 = 0;
                    pw.op2dt2 = 0;
                    pw.op3dt2 = 0;
                    pw.op4dt2 = 0;

                    ((ClsOPN)pw.chip).OutFmSetFeedbackAlgorithm(pw, parent.instFM[n].data[2], parent.instFM[n].data[1]);
                    pw.feedback = parent.instFM[n].data[2];
                    pw.algo = parent.instFM[n].data[1];
                    return;
            }

            pw.beforeTLOP1 = -1;
            pw.beforeTLOP3 = -1;
            pw.beforeTLOP2 = -1;
            pw.beforeTLOP4 = -1;
            OutFmSetVolume(pw, vol);//, n);
            //SetFmVolume(pw);
        }

        public void OutFmKeyOff(partWork pw)
        {
            int n = 3;

            if (pw.chip is YM2612X && (pw.ch > 9 || pw.ch == 6))// && pw.pcm)
            {
                ((YM2612X)pw.chip).OutYM2612XPcmKeyOFF(pw);
                return;
            }

            if (pw.ch != 6 || pw.Type != enmChannelType.FMPCM)
            {
                if (pw.chip.lstPartWork[2].Ch3SpecialMode && pw.Type == enmChannelType.FMOPNex)
                {
                    pw.Ch3SpecialModeKeyOn = false;

                    int slot = (pw.chip.lstPartWork[2].Ch3SpecialModeKeyOn ? pw.chip.lstPartWork[2].slots : 0x0)
                        | (pw.chip.lstPartWork[7].Ch3SpecialModeKeyOn ? pw.chip.lstPartWork[7].slots : 0x0)
                        | (pw.chip.lstPartWork[8].Ch3SpecialModeKeyOn ? pw.chip.lstPartWork[8].slots : 0x0)
                        | (pw.chip.lstPartWork[9].Ch3SpecialModeKeyOn ? pw.chip.lstPartWork[9].slots : 0x0);

                    if (pw.chip is YM2612X)
                        parent.xgmKeyOnData.Add((byte)((slot << 4) + 2));
                    else
                        pw.OutData(pw.port0, 0x28, (byte)((slot << 4) + 2));
                }
                else
                {
                    if (pw.ch >= 0 && pw.ch < n + 3)
                    {
                        byte vch = (byte)((pw.ch > 2) ? pw.ch + 1 : pw.ch);
                        //key off

                        if (parent.xgmKeyOnData != null && pw.chip is YM2612X)
                            parent.xgmKeyOnData.Add((byte)(0x00 + (vch & 7)));
                        else
                            pw.OutData(pw.port0, 0x28, (byte)(0x00 + (vch & 7)));
                    }
                }

                return;
            }

            if (parent.info.pcmRawMode)
            {
                pw.pcmSizeCounter = 0;
            }
            else
            {
                if (parent.info.Version == 1.51f)
                {
                }
                else
                {
                    if (pw.pcmWaitKeyOnCounter != -1)
                    {
                        //Stop Stream
                        pw.OutData(
                            0x94
                            , (byte)pw.streamID
                            );
                    }
                }
            }
            pw.pcmWaitKeyOnCounter = -1;

        }

        public void OutFmAllKeyOff()
        {

            foreach (partWork pw in lstPartWork)
            {
                if (pw.ch > 5) continue;

                OutFmKeyOff(pw);
                OutFmSetTl(pw, 0, 127);
                OutFmSetTl(pw, 1, 127);
                OutFmSetTl(pw, 2, 127);
                OutFmSetTl(pw, 3, 127);
            }

        }

        public void OutFmSetFnum(partWork pw, int octave, int num)
        {
            int freq;
            freq = ((num & 0x700) >> 8) + (((octave - 1) & 0x7) << 3);
            freq = (freq << 8) + (num & 0xff);

            if (freq == pw.freq) return;
            pw.freq = freq;

            if (pw.chip.lstPartWork[2].Ch3SpecialMode && pw.Type == enmChannelType.FMOPNex)
            {
                partWork vpw = pw.chip.lstPartWork[2];
                if ((pw.slots & 8) != 0)
                {
                    int f = pw.freq + vpw.slotDetune[3];
                    if (f != vpw.slotFreq[3])
                    {
                        vpw.slotFreq[3] = f;
                        pw.OutData(pw.port0, (byte)0xa6, (byte)(f >> 8));
                        pw.OutData(pw.port0, (byte)0xa2, (byte)f);
                    }
                }
                if ((pw.slots & 4) != 0)
                {
                    int f = pw.freq + vpw.slotDetune[2];
                    if (f != vpw.slotFreq[2])
                    {
                        vpw.slotFreq[2] = f;
                        pw.OutData(pw.port0, (byte)0xac, (byte)(f >> 8));
                        pw.OutData(pw.port0, (byte)0xa8, (byte)f);
                    }
                }
                if ((pw.slots & 1) != 0)
                {
                    int f = pw.freq + vpw.slotDetune[0];
                    if (f != vpw.slotFreq[0])
                    {
                        vpw.slotFreq[0] = f;
                        pw.OutData(pw.port0, (byte)0xad, (byte)(f >> 8));
                        pw.OutData(pw.port0, (byte)0xa9, (byte)f);
                    }
                }
                if ((pw.slots & 2) != 0)
                {
                    int f = pw.freq + vpw.slotDetune[1];
                    if (f != vpw.slotFreq[1])
                    {
                        vpw.slotFreq[1] = f;
                        pw.OutData(pw.port0, (byte)0xae, (byte)(f >> 8));
                        pw.OutData(pw.port0, (byte)0xaa, (byte)f);
                    }
                }
            }
            else
            {

                //拡張チャンネルの場合は処理しない
                if (pw.ch >= 7 && pw.ch < 9)
                {
                    return;
                }

                if (pw.ch > 5) return;
                if (pw.pcm) return;

                byte port = pw.ch > 2 ? pw.port1 : pw.port0;
                byte vch = (byte)(pw.ch > 2 ? pw.ch - 3 : pw.ch);

                pw.OutData(port, (byte)(0xa4 + vch), (byte)(pw.freq >> 8));
                pw.OutData(port, (byte)(0xa0 + vch), (byte)pw.freq);
            }
        }

        public void OutFmKeyOn(partWork pw)
        {
            //xgm pcmチャンネル処理
            if (pw.chip is YM2612X && (pw.ch > 9 || pw.ch == 6))// && pw.pcm)
            {
                if (!parent.PCMmode)
                {
                    pw.freq = -1;//freqをリセット
                    parent.PCMmode = true;
                    ((YM2612)(pw.chip)).OutSetCh6PCMMode(pw, true);
                }
                ((YM2612X)pw.chip).OutYM2612XPcmKeyON(pw);
                return;
            }

            if (pw.instrumentGradationSwitch)
            {
                if (pw.instrumentGradationReset)
                    InstrumentGradationReset(pw);
            }

            //Jパート(FM Ch.5)が発音時はpcmModeを解除する
            if (((pw.chip is YM2612) || (pw.chip is YM2612X)) && pw.ch == 5)
            {
                if (parent.PCMmode)
                {
                    pw.freq = -1;//freqをリセット
                    parent.PCMmode = false;
                    ((YM2612)(pw.chip)).OutSetCh6PCMMode(pw, false);
                }
            }

            int n = 3;

            if (pw.ch != 6)// || pw.Type != enmChannelType.FMPCM || pw.Type != enmChannelType.FMPCMex)
            {
                //if (pw.ch == 6)
                //{
                //    if (parent.PCMmode)
                //    {
                //        pw.pcm = true;
                //        pw.freq = -1;//freqをリセット
                //        parent.PCMmode = false;
                //        ((YM2612)(pw.chip)).OutSetCh6PCMMode(pw, false);
                //    }
                //}

                if (pw.chip.lstPartWork[2].Ch3SpecialMode && pw.Type == enmChannelType.FMOPNex)
                {
                    pw.Ch3SpecialModeKeyOn = true;

                    int slot = (pw.chip.lstPartWork[2].Ch3SpecialModeKeyOn ? pw.chip.lstPartWork[2].slots : 0x0)
                        | (pw.chip.lstPartWork[7].Ch3SpecialModeKeyOn ? pw.chip.lstPartWork[7].slots : 0x0)
                        | (pw.chip.lstPartWork[8].Ch3SpecialModeKeyOn ? pw.chip.lstPartWork[8].slots : 0x0)
                        | (pw.chip.lstPartWork[9].Ch3SpecialModeKeyOn ? pw.chip.lstPartWork[9].slots : 0x0);

                    if (pw.chip is YM2612X)
                        parent.xgmKeyOnData.Add((byte)((slot << 4) + 2));
                    else
                        pw.OutData(pw.port0, 0x28, (byte)((slot << 4) + 2));
                }
                else
                {
                    if (pw.ch >= 0 && pw.ch < n + 3)
                    {
                        byte vch = (byte)((pw.ch > 2) ? pw.ch + 1 : pw.ch);
                        //key on
                        if (pw.chip is YM2612X)
                        {
                            parent.xgmKeyOnData.Add((byte)((pw.slots << 4) + (vch & 7)));
                        }
                        else
                        {
                            pw.OutData(pw.port0, 0x28, (byte)((pw.slots << 4) + (vch & 7)));
                        }
                    }
                }

                return;
            }

            //以下vgm pcmCh処理

            if (pw.isPcmMap)
            {
                int nt = Const.NOTE.IndexOf(pw.noteCmd);
                int f = pw.octaveNow * 12 + nt + pw.shift + pw.keyShift;
                if (parent.instPCMMap.ContainsKey(pw.pcmMapNo))
                {
                    if (parent.instPCMMap[pw.pcmMapNo].ContainsKey(f))
                    {
                        pw.instrument = parent.instPCMMap[pw.pcmMapNo][f];
                    }
                    else
                    {
                        msgBox.setErrMsg(string.Format(msg.get("E10025"), pw.octaveNow, pw.noteCmd, pw.shift + pw.keyShift));
                        return;
                    }
                }
                else
                {
                    msgBox.setErrMsg(string.Format(msg.get("E10024"), pw.pcmMapNo));
                    return;
                }
            }

            if (!parent.instPCM.ContainsKey(pw.instrument)) return;

            if (!parent.PCMmode)
            {
                pw.pcm = true;
                pw.freq = -1;//freqをリセット
                parent.PCMmode = true;
                ((YM2612)(pw.chip)).OutSetCh6PCMMode(pw, true);
            }

            float m = Const.pcmMTbl[pw.pcmNote] * (float)Math.Pow(2, (pw.pcmOctave - 4));
            m = 1;
            pw.pcmBaseFreqPerFreq = Information.VGM_SAMPLE_PER_SECOND / ((float)parent.instPCM[pw.instrument].freq * m);
            pw.pcmFreqCountBuffer = 0.0f;
            long p = parent.instPCM[pw.instrument].stAdr;

            if (parent.info.pcmRawMode)
            {
                pw.pcmWaitKeyOnCounter = -1;
                if (parent.info.Version >= 1.51f)
                {
                    pw.OutData(
                        0xe0
                        , (byte)(p & 0xff)
                        , (byte)((p & 0xff00) / 0x100)
                        , (byte)((p & 0xff0000) / 0x10000)
                        , (byte)((p & 0xff000000) / 0x10000)
                        );

                    if (pw.gatetimePmode)
                        pw.waitKeyOnCounter = pw.waitCounter * pw.gatetime / 8L;
                    else
                        pw.waitKeyOnCounter = pw.waitCounter - parent.GetWaitCounter(pw.gatetime);
                    if (pw.waitKeyOnCounter < 1)
                    {
                        if ((pw.chip is YM2612) || (pw.chip is YM2612X)) pw.waitKeyOnCounter = 1;
                        else pw.waitKeyOnCounter = pw.waitCounter;
                    }

                    pw.pcmWaitKeyOnCounter = pw.waitKeyOnCounter;
                }

                pw.pcmSizeCounter = 0;
                if (parent.instPCM != null && parent.instPCM.ContainsKey(pw.instrument))
                {
                    pw.pcmSizeCounter = parent.instPCM[pw.instrument].size;
                }
            }
            else
            {
                if (parent.info.Version == 1.51f)
                {
                    pw.OutData(
                        0xe0
                        , (byte)(p & 0xff)
                        , (byte)((p & 0xff00) / 0x100)
                        , (byte)((p & 0xff0000) / 0x10000)
                        , (byte)((p & 0xff000000) / 0x10000)
                        );
                }
                else
                {
                    long s = parent.instPCM[pw.instrument].size;
                    long f = parent.instPCM[pw.instrument].freq;
                    long w = 0;
                    if (pw.gatetimePmode)
                    {
                        w = pw.waitCounter * parent.GetWaitCounter(pw.gatetime) / 8L;
                    }
                    else
                    {
                        w = pw.waitCounter - pw.gatetime;
                    }
                    if (w < 1) w = 1;

                    //s = Math.Min(s, (long)(w * pw.samplesPerClock * f / 44100.0));

                    if (!pw.streamSetup)
                    {
                        parent.newStreamID++;
                        pw.streamID = parent.newStreamID;

                        pw.OutData(
                            // setup stream control
                            0x90
                            , (byte)pw.streamID
                            , (byte)(0x02 + (pw.isSecondary ? 0x80 : 0x00))
                            , 0x00
                            , 0x2a
                            // set stream data
                            , 0x91
                            , (byte)pw.streamID
                            , 0x00
                            , 0x01
                            , 0x00
                            );

                        pw.streamSetup = true;
                    }

                    if (pw.streamFreq != f)
                    {
                        //Set Stream Frequency
                        pw.OutData(
                            0x92
                            , (byte)pw.streamID
                            , (byte)f
                            , (byte)(f >> 8)
                            , (byte)(f >> 16)
                            , (byte)(f >> 24)
                            );

                        pw.streamFreq = f;
                    }

                    //Start Stream
                    pw.OutData(
                        0x93
                        , (byte)pw.streamID
                        , (byte)p
                        , (byte)(p >> 8)
                        , (byte)(p >> 16)
                        , (byte)(p >> 24)
                        , 0x01
                        , (byte)s
                        , (byte)(s >> 8)
                        , (byte)(s >> 16)
                        , (byte)(s >> 24)
                        );
                }
            }

            if (parent.instPCM[pw.instrument].status != enmPCMSTATUS.ERROR)
            {
                parent.instPCM[pw.instrument].status = enmPCMSTATUS.USED;
            }

        }


        public void SetFmFNum(partWork pw)
        {
            if (pw.noteCmd == (char)0)
            {
                return;
            }

            int[] ftbl = pw.chip.FNumTbl[0];

            int f = GetFmFNum(ftbl, pw.octaveNow, pw.noteCmd, pw.shift);// + pw.keyShift + pw.relKeyShift + pw.toneDoublerKeyShift);//
            if (pw.bendWaitCounter != -1)
            {
                f = pw.bendFnum;
            }
            int o = (f & 0xf000) / 0x1000;
            f &= 0xfff;

            f = f + pw.detune;
            while (f < ftbl[0])
            {
                if (o == 1)
                {
                    break;
                }
                o--;
                f = ftbl[0] * 2 - (ftbl[0] - f);
            }
            while (f >= ftbl[0] * 2)
            {
                if (o == 8)
                {
                    break;
                }
                o++;
                f = f - ftbl[0] * 2 + ftbl[0];
            }

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
                f += pw.lfo[lfo].value;// + pw.lfo[lfo].param[6];
            }

            f = Common.CheckRange(f, 0, 0x7ff);
            OutFmSetFnum(pw, o, f);
            //Console.WriteLine("{0:x} {1:x}", o, f);
        }

        public int GetFmFNum(int[] ftbl, int octave, char noteCmd, int shift)
        {
            int o = octave;
            int n = Const.NOTE.IndexOf(noteCmd) + shift;
            if (n >= 0)
            {
                o += n / 12;
                o = Common.CheckRange(o, 1, 8);
                n %= 12;
            }
            else
            {
                o += n / 12 - ((n % 12 == 0) ? 0 : 1);
                o = Common.CheckRange(o, 1, 8);
                n %= 12;
                if (n < 0) { n += 12; }
            }

            int f = ftbl[n];

            return (f & 0xfff) + (o & 0xf) * 0x1000;
        }

        public override int GetFNum(partWork pw, int octave, char cmd, int shift)
        {
            if (pw.Type == enmChannelType.FMOPN || pw.Type == enmChannelType.FMOPNex)
            {
                return GetFmFNum(FNumTbl[0], octave, cmd, shift);
            }
            if (pw.Type == enmChannelType.SSG)
            {
                return GetSsgFNum(pw, octave, cmd, shift);
            }
            return 0;
        }

        public override void GetFNumAtoB(partWork pw
            , out int a, int aOctaveNow, char aCmd, int aShift
            , out int b, int bOctaveNow, char bCmd, int bShift
            , int dir)
        {
            a = GetFNum(pw, aOctaveNow, aCmd, aShift);
            b = GetFNum(pw, bOctaveNow, bCmd, bShift);

            int oa = (a & 0xf000) / 0x1000;
            int ob = (b & 0xf000) / 0x1000;
            if (oa != ob)
            {
                if ((a & 0xfff) == FNumTbl[0][0])
                {
                    oa += Math.Sign(ob - oa);
                    a = (a & 0xfff) * 2 + oa * 0x1000;
                }
                else if ((b & 0xfff) == FNumTbl[0][0])
                {
                    ob += Math.Sign(oa - ob);
                    b = (b & 0xfff) * ((dir > 0) ? 2 : 1) + ob * 0x1000;
                }
            }
        }


        public void SetFmVolume(partWork pw)
        {
            int vol = pw.volumeEasy;//.volume;
            if (pw.ReverbNowSwitch)
            {
                vol += pw.ReverbValue + 4;
                vol >>= 1;
                vol -= 4;
            }

            vol = (int)(sbyte)vol;//先ず-128～127の範囲にキャスト
            if (vol > 15) vol = -4;//16以上の場合は-4として扱う
            vol = Common.CheckRange(vol, -4, 15);//-4以下は-4へ、15以上は15へクリップ
            vol = FMVDAT[vol + 4];//ボリュームテーブル参照

            for (int lfo = 0; lfo < 1; lfo++)
            {
                if (!pw.lfo[lfo].sw)
                {
                    continue;
                }
                if (pw.lfo[lfo].type != enmLfoType.Tremolo)
                {
                    continue;
                }
                vol += pw.lfo[lfo].value;// + pw.lfo[lfo].param[6];
            }


            if (pw.beforeVolume != vol)
            {
                if (parent.instFM.ContainsKey(pw.instrument))
                {
                    OutFmSetVolume(pw, vol);//, pw.instrument);
                    pw.beforeVolume = vol;
                }
            }
        }

        public override void SetKeyOff(partWork pw)
        { }

        public override void SetVolume(partWork pw)
        {
            if (pw.Type == enmChannelType.FMOPN
                || pw.Type == enmChannelType.FMOPNex //効果音モード対応チャンネル
                || (pw.Type == enmChannelType.FMPCM && !pw.pcm) //OPN2PCMチャンネル
                || (pw.Type == enmChannelType.FMPCMex && !pw.pcm) //OPN2XPCMチャンネル
                )
            {
                SetFmVolume(pw);
            }
            else if (pw.Type == enmChannelType.SSG)
            {
                SetSsgVolume(pw);
            }
        }

        public override void SetLfoAtKeyOn(partWork pw)
        {
            //for (int lfo = 0; lfo < 4; lfo++)
            //{
            //    clsLfo pl = pw.lfo[lfo];
            //    if (!pl.sw)
            //        continue;

            //if (pl.param[5] != 1)
            //continue;

            //    pl.isEnd = false;
            //    pl.value = (pl.param[0] == 0) ? pl.param[6] : 0;//ディレイ中は振幅補正は適用されない
            //    pl.waitCounter = pl.param[0];
            //    pl.direction = pl.param[2] < 0 ? -1 : 1;

            //    if (pl.type == eLfoType.Vibrato)
            //    {
            //        if (pw.Type == enmChannelType.FMOPN
            //            || pw.Type == enmChannelType.FMOPNex)
            //            SetFmFNum(pw);
            //        else if (pw.Type == enmChannelType.SSG)
            //            SetSsgFNum(pw);

            //    }

            //    if (pl.type == eLfoType.Tremolo)
            //    {
            //        pw.beforeVolume = -1;
            //        if (pw.Type == enmChannelType.FMOPN
            //            || pw.Type == enmChannelType.FMOPNex)
            //            SetFmVolume(pw);
            //        else if (pw.Type == enmChannelType.SSG)
            //            SetSsgVolume(pw);
            //    }

            //}
        }

        public override void SetToneDoubler(partWork pw, MML mml)
        {
        }

        public override int GetToneDoublerShift(partWork pw, int octave, char noteCmd, int shift)
        {
            return 0;
        }


        private void CmdY_ToneParamOPN(byte adr, partWork pw, byte op, byte dat)
        {
            int ch;
            if (pw.Type == enmChannelType.FMOPNex) ch = 2;
            else if (pw.Type == enmChannelType.FMOPN) ch = pw.ch;
            else return;

            byte port = (ch > 2 ? pw.port1 : pw.port0);
            int vch = ch;
            vch = (byte)(vch > 2 ? vch - 3 : vch);
            op = (byte)Common.CheckRange(op, 1, 4);
            op = (byte)(op == 2 ? 3 : (op == 3 ? 2 : op));

            adr += (byte)(vch + ((op - 1) << 2));

            pw.OutData(port, adr, dat);
        }

        private void CmdY_ToneParamOPN_FBAL(partWork pw, byte dat)
        {
            int ch;
            if (pw.Type == enmChannelType.FMOPNex) ch = 2;
            else if (pw.Type == enmChannelType.FMOPN) ch = pw.ch;
            else return;

            byte port = (ch > 2 ? pw.port1 : pw.port0);
            int vch = ch;
            vch = (byte)(vch > 2 ? vch - 3 : vch);

            byte adr = (byte)(0xb0 + vch);

            pw.OutData(port, adr, dat);
        }


        public override void CmdNoiseToneMixer(partWork pw, MML mml)
        {
            if (pw.Type == enmChannelType.SSG)
            {
                int n = (int)mml.args[0];
                n = Common.CheckRange(n, 0, 3);
                pw.mixer = n;
            }
        }

        public override void CmdNoise(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 0, 31);
            pw.chip.lstPartWork[0].noise = n;//Chipの1Chに保存
            ((ClsOPN)pw.chip).OutSsgNoise(pw, n);
        }

        public override void CmdInstrument(partWork pw, MML mml)
        {
            if (mml.args.Count > 2) InstrumentGrad(pw, mml);

            char type = (char)mml.args[0];
            int n = 0;
            if (mml.args[1].GetType() == typeof(int))
            {
                n = (int)mml.args[1];
            }
            else
            {
                string name = (string)mml.args[1];
                foreach (KeyValuePair<int, mucomVoice> voi in parent.instFM)
                {
                    if (voi.Value.Name == name)
                    {
                        n = voi.Value.No;
                    }
                }
            }

            if (type == 'I')
            {
                msgBox.setErrMsg(msg.get("E11003"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }

            if (type == 'T')
            {
                n = Common.CheckRange(n, 0, 255);
                pw.toneDoubler = n;
                return;
            }

            if (type == 'E')
            {
                SetEnvelopParamFromInstrument(pw, n, mml);
                return;
            }

            if (pw.Type == enmChannelType.SSG)
            {
                SetEnvelopParamFromInstrument(pw, n, mml);
                return;
            }

            n = Common.CheckRange(n, 0, 255);
            pw.instrument = n;
            if (pw.Type == enmChannelType.FMOPNex)
            {
                pw.chip.lstPartWork[2].instrument = n;
                pw.chip.lstPartWork[7].instrument = n;
                pw.chip.lstPartWork[8].instrument = n;
                pw.chip.lstPartWork[9].instrument = n;
            }
            //if (pw.beforeInstrument == pw.instrument) return;
            pw.beforeInstrument = n;
            ((ClsOPN)pw.chip).OutFmSetInstrument(pw, n, pw.volume);
        }

        public override void CmdEnvelope(partWork pw, MML mml)
        {

            base.CmdEnvelope(pw, mml);

            if (!(mml.args[0] is string))
            {
                msgBox.setErrMsg(msg.get("E11004")
                    , mml.line.Fn
                    , mml.line.Num);

                return;
            }

            string cmd = (string)mml.args[0];

            switch (cmd)
            {
                case "EOF":
                    if (pw.Type == enmChannelType.SSG)
                    {
                        pw.beforeVolume = -1;
                    }
                    break;
            }
        }

        public byte[] FMVDAT = new byte[]{// ﾎﾞﾘｭｰﾑ ﾃﾞｰﾀ(FM)
        0x36,0x33,0x30,0x2D,
        0x2A,0x28,0x25,0x22,//  0,  1,  2,  3
        0x20,0x1D,0x1A,0x18,//  4,  5,  6,  7
        0x15,0x12,0x10,0x0D,//  8,  9, 10, 11
        0x0a,0x08,0x05,0x02 // 12, 13, 14, 15
        };

        public override void CmdVolume(partWork pw, MML mml)
        {
            int n;
            n = (mml.args != null && mml.args.Count > 0) ? (int)mml.args[0] : pw.latestVolume;
            pw.volumeEasy = n;
            pw.latestVolume = n;
            if (pw.Type == enmChannelType.FMOPN || pw.Type == enmChannelType.FMOPNex)
            {
                n = FMVDAT[n + 4];
                pw.volume = n;// Common.CheckRange(n, 0, pw.MaxVolume);
                SetFmVolume(pw);
            }
        }

        public override void CmdVolumeUp(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = pw.volumeEasy + n;
            //n = Common.CheckRange(n, 0, pw.MaxVolumeEasy);
            pw.volumeEasy = n;
            if (pw.Type == enmChannelType.FMOPN || pw.Type == enmChannelType.FMOPNex)
            {
                n = FMVDAT[n + 4];
                pw.volume = n;// Common.CheckRange(n, 0, pw.MaxVolume);
                SetFmVolume(pw);
            }
        }

        public override void CmdVolumeDown(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = pw.volumeEasy - n;
            //n = Common.CheckRange(n, 0, pw.MaxVolumeEasy);
            pw.volumeEasy = n;
            if (pw.Type == enmChannelType.FMOPN || pw.Type == enmChannelType.FMOPNex)
            {
                n = FMVDAT[n + 4];
                pw.volume = n;// Common.CheckRange(n, 0, pw.MaxVolume);
                SetFmVolume(pw);
            }
        }

        public override void CmdY(partWork pw, MML mml)
        {
            if (mml.args[0] is string toneparamName)
            {
                byte op = (byte)mml.args[1];
                byte dat = (byte)mml.args[2];

                switch (toneparamName)
                {
                    case "DM,":
                        CmdY_ToneParamOPN(0x30, pw, op, dat);
                        break;
                    case "TL,":
                        switch (op)
                        {
                            case 1:
                                pw.beforeTLOP1 = dat;
                                break;
                            case 2:
                                pw.beforeTLOP2 = dat;
                                break;
                            case 3:
                                pw.beforeTLOP3 = dat;
                                break;
                            case 4:
                                pw.beforeTLOP4 = dat;
                                break;
                        }
                        CmdY_ToneParamOPN(0x40, pw, op, dat);
                        break;
                    case "KA,":
                        CmdY_ToneParamOPN(0x50, pw, op, dat);
                        break;
                    case "DR,":
                        CmdY_ToneParamOPN(0x60, pw, op, dat);
                        break;
                    case "SR,":
                        CmdY_ToneParamOPN(0x70, pw, op, dat);
                        break;
                    case "SL,":
                        CmdY_ToneParamOPN(0x80, pw, op, dat);
                        break;
                    case "SE,":
                        CmdY_ToneParamOPN(0x90, pw, op, dat);
                        break;
                    case "FBAL":
                        CmdY_ToneParamOPN_FBAL(pw, dat);
                        break;
                }
            }
        }

        public override void CmdLoopExtProc(partWork pw, MML mml)
        {
            pw.beforeFNum = -1;
            pw.slotFreq[0] = -1;
            pw.slotFreq[1] = -1;
            pw.slotFreq[2] = -1;
            pw.slotFreq[3] = -1;
            //pw.freq = -1;
            //pw.beforeVolume = -1;
            //pw.beforeInstrument = -1;
            if (pw.ch == 2)
            {
                ((ClsOPN)pw.chip).OutOPNSetCh3SpecialMode(pw, pw.Ch3SpecialMode);
            }
        }

        public override void CmdHardEnvelope(partWork pw, MML mml)
        {
            if (pw.Type != enmChannelType.SSG) return;

            string cmd = (string)mml.args[0];
            int n = 0;

            switch (cmd)
            {
                case "EH":
                    n = (int)mml.args[1];
                    if (pw.HardEnvelopeSpeed != n)
                    {
                        pw.OutData(pw.port0, 0x0b, (byte)(n & 0xff));
                        pw.OutData(pw.port0, 0x0c, (byte)((n >> 8) & 0xff));
                        pw.HardEnvelopeSpeed = n;
                    }
                    break;
                case "EHON":
                    pw.HardEnvelopeSw = true;
                    break;
                case "EHOF":
                    pw.HardEnvelopeSw = false;
                    break;
                case "EHT":
                    n = (int)mml.args[1];
                    if (pw.HardEnvelopeType != n)
                    {
                        pw.OutData(pw.port0, 0x0d, (byte)(n & 0xf));
                        pw.HardEnvelopeType = n;
                    }
                    break;
            }
        }

        public override void CmdSlotDetune(partWork pw, MML mml)
        {
            pw.slotDetune = new int[4];
            pw.slotDetune[0] = (int)mml.args[2];
            pw.slotDetune[1] = (int)mml.args[3];
            pw.slotDetune[2] = (int)mml.args[1];
            pw.slotDetune[3] = (int)mml.args[0];
            pw.chip.lstPartWork[2].Ch3SpecialMode = true;
            pw.slotFreq[0] = -1;
            pw.slotFreq[1] = -1;
            pw.slotFreq[2] = -1;
            pw.slotFreq[3] = -1;
            ((ClsOPN)pw.chip).OutOPNSetCh3SpecialMode(pw, true);
            pw.slots = pw.slots4OP;
            pw.freq = -1;
            //SetFmFNum(pw);
        }

        public override void CmdExtendChannel(partWork pw, MML mml)
        {
            string cmd = (string)mml.args[0];

            switch (cmd)
            {
                case "EX":
                    int n = (int)mml.args[1];
                    byte res = 0;
                    while (n % 10 != 0)
                    {
                        if (n % 10 > 0 && n % 10 < 5)
                        {
                            res += (byte)(1 << (n % 10 - 1));
                        }
                        else
                        {
                            msgBox.setErrMsg(string.Format(msg.get("E11005"), n), pw.getSrcFn(), pw.getLineNumber());
                            break;
                        }
                        n /= 10;
                    }
                    if (res != 0)
                    {
                        pw.slotsEX = res;
                        if (pw.Ch3SpecialMode) pw.slots = pw.slotsEX;
                    }
                    break;
                case "EXON":
                    ((ClsOPN)pw.chip).OutOPNSetCh3SpecialMode(pw, true);
                    foreach (partWork p in pw.chip.lstPartWork)
                    {
                        if (p.Type == enmChannelType.FMOPNex)
                        {
                            p.Ch3SpecialMode = true;
                            p.slots = p.slotsEX;
                            p.beforeVolume = -1;
                            p.beforeFNum = -1;
                            pw.slotFreq[0] = -1;
                            pw.slotFreq[1] = -1;
                            pw.slotFreq[2] = -1;
                            pw.slotFreq[3] = -1;
                            p.freq = -1;
                            //SetFmFNum(p);
                        }
                    }
                    break;
                case "EXOF":
                    ((ClsOPN)pw.chip).OutOPNSetCh3SpecialMode(pw, false);
                    foreach (partWork p in pw.chip.lstPartWork)
                    {
                        if (p.Type == enmChannelType.FMOPNex)
                        {
                            if (p.ch != 2) p.slots = 0;
                            else p.slots = p.slots4OP;
                            p.Ch3SpecialMode = false;
                            p.beforeVolume = -1;
                            p.beforeFNum = -1;
                            pw.slotFreq[0] = -1;
                            pw.slotFreq[1] = -1;
                            pw.slotFreq[2] = -1;
                            pw.slotFreq[3] = -1;
                            p.freq = -1;
                            //SetFmFNum(p);
                        }
                    }
                    break;
                case "EXM":
                    int nm = (int)mml.args[1];
                    byte resm = 0;
                    while (nm % 10 != 0)
                    {
                        if (nm % 10 > 0 && nm % 10 < 5)
                        {
                            resm += (byte)(1 << (nm % 10 - 1));
                        }
                        else
                        {
                            msgBox.setErrMsg(string.Format(msg.get("E11005"), nm), pw.getSrcFn(), pw.getLineNumber());
                            break;
                        }
                        nm /= 10;
                    }
                    if (resm != 0)
                    {
                        pw.slots4OP = resm;
                        if (pw.ch != 2 && !pw.Ch3SpecialMode)
                        {
                            pw.slots = resm;
                        }
                    }
                    break;
                    //case "EXD":
                    //    pw.slotDetune[0] = (int)mml.args[1];
                    //    pw.slotDetune[1] = (int)mml.args[2];
                    //    pw.slotDetune[2] = (int)mml.args[3];
                    //    pw.slotDetune[3] = (int)mml.args[4];
                    //    break;
            }
        }



        private void InstrumentGrad(partWork pw, MML mml)
        {

            //パラメータの読み込み

            int n1 = 0;//src Inst
            int n2 = 0;//trg Inst
            int n3 = 1;//wait tick
            int n4 = 1;//reset mode

            char type1 = (char)mml.args[0];
            if (mml.args[1].GetType() == typeof(int))
            {
                n1 = (int)mml.args[1];
            }
            else
            {
                string name = (string)mml.args[1];
                foreach (KeyValuePair<int, mucomVoice> voi in parent.instFM)
                {
                    if (voi.Value.Name == name)
                    {
                        n1 = voi.Value.No;
                    }
                }
            }

            char type2 = (char)mml.args[2];
            if (mml.args[3].GetType() == typeof(int))
            {
                n2 = (int)mml.args[3];
            }
            else
            {
                string name = (string)mml.args[3];
                foreach (KeyValuePair<int, mucomVoice> voi in parent.instFM)
                {
                    if (voi.Value.Name == name)
                    {
                        n1 = voi.Value.No;
                    }
                }
            }

            if (mml.args.Count > 4)
            {
                n3 = (int)mml.args[4];
            }

            if (mml.args.Count > 5)
            {
                n4 = (int)mml.args[5];
            }

            pw.instrumentGradationSwitch = true;
            pw.instrumentGradationWait = n3;
            InstrumentGradationGetParamsFromVoice(pw, ref pw.instrumentGradationSt, n1);
            InstrumentGradationGetParamsFromVoice(pw, ref pw.instrumentGradationEd, n2);
            pw.instrumentGradationStNum = n1;
            pw.instrumentGradationEdNum = n2;
            pw.instrumentGradationReset = n4 == 1;

            InstrumentGradationReset(pw);

        }

        private static int instrumentGradParam = 50;

        private void InstrumentGradationReset(partWork pw)
        {
            pw.instrument = pw.instrumentGradationStNum;
            pw.beforeInstrument = -1;
            pw.instrumentGradationWaitCounter = pw.instrumentGradationWait;
            pw.instrumentGradationPointer = 0;
            for (int i = 0; i < instrumentGradParam; i++) pw.instrumentGradationWk[i] = pw.instrumentGradationSt[i];

            ((ClsOPN)pw.chip).OutFmSetInstrument(pw, pw.instrumentGradationStNum, pw.volume);
        }

        private void InstrumentGradationGetParamsFromVoice(partWork pw, ref int[] param, int n)
        {
            if (!parent.instFM.ContainsKey(n))
            {
                msgBox.setWrnMsg(string.Format(msg.get("E11001"), n), pw.getSrcFn(), pw.getLineNumber());
                return;
            }

            switch (parent.instFM[n].type)
            {
                case 0:// @
                    for (int ope = 0; ope < 4; ope++)
                    {
                        param[ope * 2] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 8 + 2];//      Det
                        param[ope * 2 + 1] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 7 + 2];//  Mul

                        param[ope + 8] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 5 + 2];//      TL

                        param[ope * 2 + 12] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 6 + 2];// KS
                        param[ope * 2 + 13] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 0 + 2];// AR

                        param[ope + 42] = 1;// AM
                        param[ope + 20] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 1 + 2];//     DR

                        param[ope + 24] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 2 + 2];//     SR

                        param[ope * 2 + 28] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 4 + 2];// SL
                        param[ope * 2 + 29] = parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 3 + 2];// RR

                        param[ope + 36] = 0;// DT2

                        param[ope + 46] = 0;// SSGEG
                    }
                    param[40] = parent.instFM[n].data[0];//FB
                    param[41] = parent.instFM[n].data[1];//ALG
                    break;
                case 1: // @%
                    for (int ope = 0; ope < 4; ope++)
                    {
                        param[ope * 2] = (parent.instFM[n].data[ope] & 0x70) >> 4;// Det
                        param[ope * 2 + 1] = parent.instFM[n].data[ope] & 0xf;//     Mul

                        param[ope + 8] = parent.instFM[n].data[ope + 4] & 0x7f;// TL

                        param[ope * 2 + 12] = (parent.instFM[n].data[ope + 8] & 0xc0) >> 6;// KS
                        param[ope * 2 + 13] = parent.instFM[n].data[ope + 8] & 0x1f;//        AR

                        param[ope + 42] = 1;// AM
                        param[ope + 20] = parent.instFM[n].data[ope + 12] & 0x1f;// DR

                        param[ope + 24] = parent.instFM[n].data[ope + 16] & 0x1f;// SR

                        param[ope * 2 + 28] = (parent.instFM[n].data[ope + 20] & 0xf0) >> 4;// SL
                        param[ope * 2 + 29] = parent.instFM[n].data[ope + 20] & 0x0f;//        RR

                        param[ope + 36] = (parent.instFM[n].data[ope + 16] & 0xc0) >> 6;// DT2

                        param[ope + 46] = 0;// SSGEG
                    }
                    param[40] = (parent.instFM[n].data[24] & 0x38) >> 3;//FB
                    param[41] = parent.instFM[n].data[24] & 0x07;//ALG
                    break;
                case 2: //@N
                    for (int ope = 0; ope < 4; ope++)
                    {
                        param[ope * 2] = parent.instFM[n].data[ope * 11 + 8 + 2];//      Det
                        param[ope * 2 + 1] = parent.instFM[n].data[ope * 11 + 7 + 2];//  Mul

                        param[ope + 8] = parent.instFM[n].data[ope * 11 + 5 + 2];//      TL

                        param[ope * 2 + 12] = parent.instFM[n].data[ope * 11 + 6 + 2];// KS
                        param[ope * 2 + 13] = parent.instFM[n].data[ope * 11 + 0 + 2];// AR

                        param[ope + 42] = parent.instFM[n].data[ope * 11 + 9 + 2];//     AM
                        param[ope + 20] = parent.instFM[n].data[ope * 11 + 1 + 2];//     DR

                        param[ope + 24] = parent.instFM[n].data[ope * 11 + 2 + 2];//     SR

                        param[ope * 2 + 28] = parent.instFM[n].data[ope * 11 + 4 + 2];// SL
                        param[ope * 2 + 29] = parent.instFM[n].data[ope * 11 + 3 + 2];// RR

                        param[ope + 36] = 0;// DT2

                        param[ope + 46] = parent.instFM[n].data[ope * 11 + 10 + 2];//    SSGEG
                    }
                    param[40] = parent.instFM[n].data[0];//FB
                    param[41] = parent.instFM[n].data[1];//ALG
                    break;
                case 3://@L OPL
                    msgBox.setErrMsg(msg.get("E11001"), pw.getSrcFn(), pw.getLineNumber());
                    break;
                case 4://@M OPM

                    for (int ope = 0; ope < 4; ope++)
                    {
                        param[ope * 2] = parent.instFM[n].data[ope * 11 + 8 + 3];//      Det
                        param[ope * 2 + 1] = parent.instFM[n].data[ope * 11 + 7 + 3];//  Mul

                        param[ope + 8] = parent.instFM[n].data[ope * 11 + 5 + 3];//      TL

                        param[ope * 2 + 12] = parent.instFM[n].data[ope * 11 + 6 + 3];// KS
                        param[ope * 2 + 13] = parent.instFM[n].data[ope * 11 + 0 + 3];// AR

                        param[ope + 42] = parent.instFM[n].data[ope * 11 + 10 + 3];//    AM
                        param[ope + 20] = parent.instFM[n].data[ope * 11 + 1 + 3];//     DR

                        param[ope + 24] = parent.instFM[n].data[ope * 11 + 2 + 3];//     SR

                        param[ope * 2 + 28] = parent.instFM[n].data[ope * 11 + 4 + 3];// SL
                        param[ope * 2 + 29] = parent.instFM[n].data[ope * 11 + 3 + 3];// RR

                        param[ope + 36] = 0;// DT2

                        param[ope + 46] = 0;// SSGEG
                    }
                    param[40] = parent.instFM[n].data[2];//FB
                    param[41] = parent.instFM[n].data[1];//ALG
                    break;
            }

        }

        public void prcInstrumentGradation(partWork pw)
        {
            //if (!CheckCh3SpecialMode() && pw.pageNo != work.cd.currentPageNo) return;

            if (!pw.instrumentGradationSwitch) return;

            //pw.instrumentGradationWaitCounter--;
            if (pw.instrumentGradationWaitCounter > 0) return;
            pw.instrumentGradationWaitCounter = pw.instrumentGradationWait;

            InstrumentGradationUpdate(pw);

            STENVGradation(pw);//KEYオフ、リリースカット無しの音色セット
            OutFmSetVolume(pw, pw.volume);
        }

        private void InstrumentGradationUpdate(partWork pw)
        {
            for (int i = 0; i < instrumentGradParam; i++)
            {
                if (pw.instrumentGradationWk[i] == pw.instrumentGradationEd[i])
                {
                    pw.instrumentGradationFlg[i] = false;
                    continue;
                }

                pw.instrumentGradationFlg[i] = true;
                if (pw.instrumentGradationWk[i] < pw.instrumentGradationEd[i])
                    pw.instrumentGradationWk[i]++;
                else
                    pw.instrumentGradationWk[i]--;
            }
        }

        private static byte[] GraSlot = new byte[4] { 1, 4, 2, 8 };

        private void STENVGradation(partWork pw)
        {
            bool CH3 = pw.Ch3SpecialMode;

            if (pw.instrumentGradationFlg[40] || pw.instrumentGradationFlg[41])
            {
                pw.feedback = pw.instrumentGradationWk[40];
                pw.algo = pw.instrumentGradationWk[41];

                ((ClsOPN)pw.chip).OutFmSetFeedbackAlgorithm(pw, pw.instrumentGradationWk[40], pw.instrumentGradationWk[41]);
            }

            // 6 PARAMATER(Det/Mul, Total, KS/AR, DR, SR, SL/RR)

            //Det/Mul
            for (int o = 0; o < 4; o++)
            {
                if (CH3 && (pw.slots & GraSlot[o]) == 0) continue;
                if (pw.instrumentGradationFlg[o * 2] || pw.instrumentGradationFlg[o * 2 + 1])
                    ((ClsOPN)pw.chip).OutFmSetDtMl(
                        pw,
                        o,
                        pw.instrumentGradationWk[o * 2],
                        pw.instrumentGradationWk[o * 2 + 1]);
            }

            //TL
            byte[] alg = new byte[] { 0x8, 0x8, 0x8, 0x8, 0xc, 0xe, 0xe, 0xf };
            byte c = alg[pw.algo];
            int[] op = new int[4] { 0, 2, 1, 3 };
            for (int o = 0; o < 4; o++)
            {
                if (CH3 && (pw.slots & GraSlot[o]) == 0) continue;
                if (pw.instrumentGradationFlg[8 + o])//TL
                {
                    if ((c & (1 << o)) == 0)
                        ((ClsOPN)pw.chip).OutFmSetTl(
                            pw,
                            op[o],
                            pw.instrumentGradationWk[o + 8]);
                    pw.v_tl[o] = (byte)pw.instrumentGradationWk[8 + o];
                }
            }

            //KS/AR
            for (int o = 0; o < 4; o++)
            {
                if (CH3 && (pw.slots & GraSlot[o]) == 0) continue;
                if (pw.instrumentGradationFlg[12 + o * 2] || pw.instrumentGradationFlg[12 + o * 2 + 1])
                    ((ClsOPN)pw.chip).OutFmSetKsAr(
                        pw,
                            op[o],
                        pw.instrumentGradationWk[12 + o * 2],
                        pw.instrumentGradationWk[12 + o * 2 + 1]);
            }

            //AM/DR
            for (int o = 0; o < 4; o++)
            {
                if (CH3 && (pw.slots & GraSlot[o]) == 0) continue;
                if (pw.instrumentGradationFlg[42 + o] || pw.instrumentGradationFlg[20 + o])
                    ((ClsOPN)pw.chip).OutFmSetAmDr(
                        pw,
                            op[o],
                        pw.instrumentGradationWk[42 + o],
                        pw.instrumentGradationWk[20 + o]);
            }

            //Dt2/SR
            for (int o = 0; o < 4; o++)
            {
                if (CH3 && (pw.slots & GraSlot[o]) == 0) continue;
                if (pw.instrumentGradationFlg[24 + o])
                    ((ClsOPN)pw.chip).OutFmSetSr(pw,
                            op[o],
                        pw.instrumentGradationWk[24 + o]);
            }

            //SL/RR
            for (int o = 0; o < 4; o++)
            {
                if (CH3 && (pw.slots & GraSlot[o]) == 0) continue;
                if (pw.instrumentGradationFlg[28 + o * 2] || pw.instrumentGradationFlg[28 + o * 2 + 1])
                    ((ClsOPN)pw.chip).OutFmSetSlRr(
                        pw,
                            op[o],
                        pw.instrumentGradationWk[28 + o * 2],
                        pw.instrumentGradationWk[28 + o * 2 + 1]);
            }

        }

    }
}
