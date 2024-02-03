using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class YM2151 : ClsChip
    {
        public YM2151(ClsVgm parent, int chipID, string initialPartName, string stPath, bool isSecondary) : base(parent, chipID, initialPartName, stPath, isSecondary)
        {
            _chipType = enmChipType.YM2151;
            _Name = "YM2151";
            _ShortName = "OPM";
            _ChMax = 8;
            _canUsePcm = false;

            Frequency = 3579545;
            port = new byte[] { (byte)(IsSecondary ? 0xa4 : 0x54) };

            if (string.IsNullOrEmpty(initialPartName)) return;

            MakeFNumTbl();

            //Ch = new ClsChannel[ChMax];
            //SetPartToCh(Ch, initialPartName);
            Ch = new ClsChannel[ChMax];
            char[] PART_OPM = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H' };
            for (int i = 0; i < Ch.Length; i++)
            {
                if (Ch[i] == null) Ch[i] = new ClsChannel();
                Ch[i].Name = PART_OPM[i].ToString();
                Ch[i].Type = enmChannelType.FMOPM;
                Ch[i].isSecondary = IsSecondary;
            }

        }

        public override void InitChip()
        {
            if (!use) return;

            //initialize shared param

            //FM Off
            OutAllKeyOff();

            foreach (partWork pw in lstPartWork)
            {
                if (pw.ch == 0)
                {
                    pw.hardLfoFreq = 0;
                    pw.hardLfoPMD = 0;
                    pw.hardLfoAMD = 0;

                    //Reset Hard LFO
                    OutSetHardLfoFreq(pw, pw.hardLfoFreq);
                    OutSetHardLfoDepth(pw, false, pw.hardLfoAMD);
                    OutSetHardLfoDepth(pw, true, pw.hardLfoPMD);
                }

                pw.ams = 0;
                pw.pms = 0;
                if (!pw.dataEnd) OutSetPMSAMS(pw, 0, 0);
            }


            //if (ChipID != 0 && parent.info.format != enmFormat.ZGM)
            //{
            //    parent.dat[0x33] = new outDatum(enmMMLType.unknown, null, null, (byte)(parent.dat[0x33].val | 0x40));//use Secondary
            //}
        }

        public override void InitPart(ref partWork pw)
        {
            pw.slots = 0xf;
            pw.volume = 127;
            pw.MaxVolume = 127;
            pw.MaxVolumeEasy = 15;
            pw.port0 = port[0];
            pw.mixer = 0;
            pw.noise = 0;
            pw.ipan = 3;
            pw.Type = enmChannelType.FMOPM;
        }


        public void OutSetFnum(partWork pw, int octave, int note, int kf)
        {
            octave &= 0x7;
            note &= 0xf;
            note = note < 3 ? note : (note < 6 ? (note + 1) : (note < 9 ? (note + 2) : (note + 3)));
            pw.OutData(pw.port0, (byte)(0x28 + pw.ch), (byte)((octave << 4) | note));
            pw.OutData(pw.port0, (byte)(0x30 + pw.ch), (byte)(kf << 2));
        }

        public void OutSetVolume(partWork pw, int vol, int n)
        {
            if (!parent.instFM.ContainsKey(n))
            {
                msgBox.setWrnMsg(string.Format(msg.get("E16000"), n));//, mml.line.Lp);
                return;
            }

            int alg = 0;
            int[] ope = null;
            switch (parent.instFM[n].type)
            {
                case 0:// @
                    alg = parent.instFM[n].data[1] & 0x7;
                    ope = new int[4] {
                        parent.instFM[n].data[0*Const.INSTRUMENT_OPERATOR_SIZE + 5+2]
                        , parent.instFM[n].data[1 * Const.INSTRUMENT_OPERATOR_SIZE + 5+2]
                        , parent.instFM[n].data[2 * Const.INSTRUMENT_OPERATOR_SIZE + 5+2]
                        , parent.instFM[n].data[3 * Const.INSTRUMENT_OPERATOR_SIZE + 5+2]
                    };
                    break;
                case 1:// @%
                    alg = parent.instFM[n].data[24] & 0x7;
                    ope = new int[4] {
                        parent.instFM[n].data[4]
                        , parent.instFM[n].data[6]
                        , parent.instFM[n].data[5]
                        , parent.instFM[n].data[7]
                    };
                    break;
                case 2:// @N
                    alg = parent.instFM[n].data[1] & 0x7;
                    ope = new int[4] {
                        parent.instFM[n].data[0 * 11 + 5+2]
                        , parent.instFM[n].data[1 * 11 + 5+2]
                        , parent.instFM[n].data[2 * 11 + 5+2]
                        , parent.instFM[n].data[3 * 11 + 5+2]
                    };
                    break;
                case 3:// @L OPL
                    msgBox.setErrMsg(string.Format(msg.get("E11000"), n), pw.getSrcFn(), pw.getLineNumber());
                    return;
                case 4:// @M OPM
                    alg = parent.instFM[n].data[1] & 0x7;
                    ope = new int[4] {
                        parent.instFM[n].data[0 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5 + 3]
                        , parent.instFM[n].data[1 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5 + 3]
                        , parent.instFM[n].data[2 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5 + 3]
                        , parent.instFM[n].data[3 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5 + 3]
                    };
                    break;
            }
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

            //int minV = 127;
            //for (int i = 0; i < 4; i++)
            //{
            //    if (algs[alg][i] == 1 && (pw.ppg[pw.cpgNum].slots & (1 << i)) != 0)
            //    {
            //        minV = Math.Min(minV, ope[i]);
            //    }
            //}

            for (int i = 0; i < 4; i++)
            {
                if (algs[alg][i] == 0 || (pw.slots & (1 << i)) == 0)
                {
                    ope[i] = -1;
                    continue;
                }
                //ope[i] = ope[i] - minV + (127 - vol);
                ope[i] = ope[i] + vol;
                if (ope[i] < 0)
                {
                    ope[i] = 0;
                }
                if (ope[i] > 127)
                {
                    ope[i] = 127;
                }
            }

            if ((pw.slots & 1) != 0 && ope[0] != -1) OutSetTl(pw, 0, ope[0]);
            if ((pw.slots & 2) != 0 && ope[1] != -1) OutSetTl(pw, 1, ope[1]);
            if ((pw.slots & 4) != 0 && ope[2] != -1) OutSetTl(pw, 2, ope[2]);
            if ((pw.slots & 8) != 0 && ope[3] != -1) OutSetTl(pw, 3, ope[3]);
        }

        public void OutSetTl(partWork pw, int ope, int tl)
        {
            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            tl &= 0x7f;

            pw.OutData(
                pw.port0
                , (byte)(0x60 + pw.ch + ope * 8)
                , (byte)tl
                );
        }

        public void OutSetHardLfoFreq(partWork pw, int freq)
        {
            pw.OutData(
                pw.port0
                , 0x18
                , (byte)(freq & 0xff)
                );
        }

        public void OutSetHardLfoDepth(partWork pw, bool isPMD, int depth)
        {
            pw.OutData(
                pw.port0
                , 0x19
                , (byte)((isPMD ? 0x80 : 0x00) | (depth & 0x7f))
                );
        }

        public void OutSetPMSAMS(partWork pw, int PMS, int AMS)
        {
            pw.OutData(
                pw.port0
                , (byte)(0x38 + pw.ch)
                , (byte)(((PMS & 0x7) << 4) | (AMS & 0x3))
                );
        }

        public void OutSetPanFeedbackAlgorithm(partWork pw, int pan, int fb, int alg)
        {
            pan &= 3;
            fb &= 7;
            alg &= 7;

            pw.OutData(pw.port0, (byte)(0x20 + pw.ch), (byte)((pan << 6) | (fb << 3) | alg));
        }

        public void OutSetDtMl(partWork pw, int ope, int dt, int ml)
        {
            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            dt &= 7;
            ml &= 15;

            pw.OutData(pw.port0, (byte)(0x40 + pw.ch + ope * 8), (byte)((dt << 4) | ml));
        }

        public void OutSetKsAr(partWork pw, int ope, int ks, int ar)
        {
            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            ks &= 3;
            ar &= 31;

            pw.OutData(pw.port0, (byte)(0x80 + pw.ch + ope * 8), (byte)((ks << 6) | ar));
        }

        public void OutSetAmDr(partWork pw, int ope, int am, int dr)
        {
            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            am &= 1;
            dr &= 31;

            pw.OutData(pw.port0, (byte)(0xa0 + pw.ch + ope * 8), (byte)((am << 7) | dr));
        }

        public void OutSetDt2Sr(partWork pw, int ope, int dt2, int sr)
        {
            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            dt2 &= 3;
            sr &= 31;

            pw.OutData(pw.port0, (byte)(0xc0 + pw.ch + ope * 8), (byte)((dt2 << 6) | sr));
        }

        public void OutSetSlRr(partWork pw, int ope, int sl, int rr)
        {
            ope = (ope == 1) ? 2 : ((ope == 2) ? 1 : ope);
            sl &= 15;
            rr &= 15;

            pw.OutData(pw.port0, (byte)(0xe0 + pw.ch + ope * 8), (byte)((sl << 4) | rr));
        }

        public void OutSetHardLfo(partWork pw, bool sw, List<int> param)
        {
            if (sw)
            {
                pw.OutData(pw.port0, 0x1b, (byte)(param[0] & 0x3));//type
                pw.OutData(pw.port0, 0x18, (byte)(param[1] & 0xff));//LFRQ
                pw.OutData(pw.port0, 0x19, (byte)((param[2] & 0x7f) | 0x80));//PMD
                pw.OutData(pw.port0, 0x19, (byte)((param[3] & 0x7f) | 0x00));//AMD
            }
            else
            {
                pw.OutData(pw.port0, 0x1b, 0);//type
                pw.OutData(pw.port0, 0x18, 0);//LFRQ
                pw.OutData(pw.port0, 0x19, 0x80);//PMD
                pw.OutData(pw.port0, 0x19, 0x00);//AMD
            }
        }

        public void OutSetInstrument(partWork pw, int n, int vol, int modeBeforeSend)
        {

            if (!parent.instFM.ContainsKey(n))
            {
                msgBox.setWrnMsg(string.Format(msg.get("E16001"), n));//, mml.line.Lp);
                return;
            }

            switch (modeBeforeSend)
            {
                case 0: // N)one
                    break;
                case 1: // R)R only
                    for (int ope = 0; ope < 4; ope++) OutSetSlRr(pw, ope, 0, 15);
                    break;
                case 2: // A)ll
                    for (int ope = 0; ope < 4; ope++)
                    {
                        OutSetDtMl(pw, ope, 0, 0);
                        OutSetKsAr(pw, ope, 3, 31);
                        OutSetAmDr(pw, ope, 1, 31);
                        OutSetDt2Sr(pw, ope, 0, 31);
                        OutSetSlRr(pw, ope, 0, 15);
                    }
                    OutSetPanFeedbackAlgorithm(pw, pw.ipan, 7, 7);
                    break;
            }

            int alg = 0;
            int[] op = null;
            switch (parent.instFM[n].type)
            {
                case 0:// @
                    for (int ope = 0; ope < 4; ope++) ((ClsOPN)pw.chip).OutFmSetSlRr(pw, ope, 0, 15);

                    for (int ope = 0; ope < 4; ope++)
                    {

                        OutSetDtMl(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 8 + 2], parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 7 + 2]);
                        OutSetKsAr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 6 + 2], parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 0 + 2]);
                        OutSetAmDr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 9 + 2], parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 1 + 2]);
                        OutSetDt2Sr(pw, ope, 0 , parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 2 + 2]);
                        OutSetSlRr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_OPERATOR_SIZE + 4 + 2], parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 3 + 2]);

                    }

                    pw.op1ml = parent.instFM[n].data[0 * Const.INSTRUMENT_OPERATOR_SIZE + 7+2];
                    pw.op2ml = parent.instFM[n].data[1 * Const.INSTRUMENT_OPERATOR_SIZE + 7 + 2];
                    pw.op3ml = parent.instFM[n].data[2 * Const.INSTRUMENT_OPERATOR_SIZE + 7 + 2];
                    pw.op4ml = parent.instFM[n].data[3 * Const.INSTRUMENT_OPERATOR_SIZE + 7 + 2];
                    pw.op1dt2 = 0;
                    pw.op2dt2 = 0;
                    pw.op3dt2 = 0;
                    pw.op4dt2 = 0;

                    ((ClsOPN)pw.chip).OutFmSetFeedbackAlgorithm(pw, parent.instFM[n].data[0], parent.instFM[n].data[1]);
                    break;
                case 1: // @%
                    int vch = pw.ch;
                    byte port = pw.port0;

                    for (int ope = 0; ope < 4; ope++) OutSetSlRr(pw, ope, 0, 15);

                    for (int ope = 0; ope < 4; ope++)
                    {
                        int ops = ope;
                        if (ope == 1) ops = 2;
                        else if (ope == 2) ops = 1;
                        OutSetDtMl(pw, ope, (parent.instFM[n].data[ops] & 0x70) >> 4, parent.instFM[n].data[ops] & 0x0f);
                        OutSetKsAr(pw, ope, (parent.instFM[n].data[ops + 8] & 0xc0) >> 6, parent.instFM[n].data[ops + 8] & 0x1f);
                        OutSetAmDr(pw, ope, 1, parent.instFM[n].data[ops + 12] & 0x1f);
                        OutSetDt2Sr(pw, ope, 0, parent.instFM[n].data[ops + 16] & 0x1f);
                        OutSetSlRr(pw, ope, (parent.instFM[n].data[ops + 20] & 0xf0) >> 4, parent.instFM[n].data[ops + 20] & 0x0f);
                    }

                    OutSetPanFeedbackAlgorithm(pw, pw.ipan, (parent.instFM[n].data[24] & 0x38) >> 3, parent.instFM[n].data[24] & 0x7);

                    alg = parent.instFM[n].data[24] & 0x7;
                    op = new int[4] {
                        parent.instFM[n].data[4]
                        , parent.instFM[n].data[6]
                        , parent.instFM[n].data[5]
                        , parent.instFM[n].data[7]
                    };
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
                    }

                    pw.op1ml = parent.instFM[n].data[0 * 11 + 7];
                    pw.op2ml = parent.instFM[n].data[1 * 11 + 7];
                    pw.op3ml = parent.instFM[n].data[2 * 11 + 7];
                    pw.op4ml = parent.instFM[n].data[3 * 11 + 7];
                    pw.op1dt2 = 0;
                    pw.op2dt2 = 0;
                    pw.op3dt2 = 0;
                    pw.op4dt2 = 0;

                    ((ClsOPN)pw.chip).OutFmSetFeedbackAlgorithm(pw, parent.instFM[n].data[0], parent.instFM[n].data[1]);
                    break;
                case 3://@L OPL
                    msgBox.setErrMsg(msg.get("E11001"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                case 4://@M OPM
                    for (int ope = 0; ope < 4; ope++)
                    {

                        OutSetDtMl(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 8 + 3], parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 7 + 3]);
                        OutSetKsAr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 6 + 3], parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 0 + 3]);
                        OutSetAmDr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 10 + 3], parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 1 + 3]);
                        OutSetDt2Sr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 9 + 3], parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 2 + 3]);
                        OutSetSlRr(pw, ope, parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 4 + 3], parent.instFM[n].data[ope * Const.INSTRUMENT_M_OPERATOR_SIZE + 3 + 3]);

                    }
                    pw.op1ml = parent.instFM[n].data[0 * Const.INSTRUMENT_M_OPERATOR_SIZE + 7 + 3];
                    pw.op2ml = parent.instFM[n].data[1 * Const.INSTRUMENT_M_OPERATOR_SIZE + 7 + 3];
                    pw.op3ml = parent.instFM[n].data[2 * Const.INSTRUMENT_M_OPERATOR_SIZE + 7 + 3];
                    pw.op4ml = parent.instFM[n].data[3 * Const.INSTRUMENT_M_OPERATOR_SIZE + 7 + 3];
                    pw.op1dt2 = parent.instFM[n].data[0 * Const.INSTRUMENT_M_OPERATOR_SIZE + 9 + 3];
                    pw.op2dt2 = parent.instFM[n].data[1 * Const.INSTRUMENT_M_OPERATOR_SIZE + 9 + 3];
                    pw.op3dt2 = parent.instFM[n].data[2 * Const.INSTRUMENT_M_OPERATOR_SIZE + 9 + 3];
                    pw.op4dt2 = parent.instFM[n].data[3 * Const.INSTRUMENT_M_OPERATOR_SIZE + 9 + 3];

                    OutSetPanFeedbackAlgorithm(pw, pw.ipan, parent.instFM[n].data[2], parent.instFM[n].data[1]);

                    alg = parent.instFM[n].data[1] & 0x7;
                    op = new int[4] {
                        parent.instFM[n].data[0*Const.INSTRUMENT_M_OPERATOR_SIZE + 5+3]
                        , parent.instFM[n].data[1 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5+3]
                        , parent.instFM[n].data[2 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5+3]
                        , parent.instFM[n].data[3 * Const.INSTRUMENT_M_OPERATOR_SIZE + 5+3]
                    };
                    break;
            }

            int[][] algs = new int[8][]
            {
                new int[4] { 1,1,1,0}
                ,new int[4] { 1,1,1,0}
                ,new int[4] { 1,1,1,0}
                ,new int[4] { 1,1,1,0}
                ,new int[4] { 1,0,1,0}
                ,new int[4] { 1,0,0,0}
                ,new int[4] { 1,0,0,0}
                ,new int[4] { 0,0,0,0}
            };

            for (int i = 0; i < 4; i++)
            {
                if (algs[alg][i] == 0 || (pw.slots & (1 << i)) == 0)
                {
                    op[i] = -1;
                    continue;
                }
                if (op[i] < 0)
                {
                    op[i] = 0;
                }
                if (op[i] > 127)
                {
                    op[i] = 127;
                }
            }

            if ((pw.slots & 1) != 0 && op[0] != -1) OutSetTl(pw, 0, op[0]);
            if ((pw.slots & 2) != 0 && op[1] != -1) OutSetTl(pw, 1, op[1]);
            if ((pw.slots & 4) != 0 && op[2] != -1) OutSetTl(pw, 2, op[2]);
            if ((pw.slots & 8) != 0 && op[3] != -1) OutSetTl(pw, 3, op[3]);

            ((YM2151)pw.chip).OutSetVolume(pw, vol, n);

        }

        public void OutKeyOn(partWork pw)
        {

            if (pw.ch == 7 && pw.mixer == 1)
            {
                pw.OutData(pw.port0, 0x0f, (byte)((pw.mixer << 7) | (pw.noise & 0x1f)));
            }
            //key on
            pw.OutData(pw.port0, 0x08, (byte)((pw.slots << 3) + pw.ch));
        }

        public void OutKeyOff(partWork pw)
        {

            //key off
            pw.OutData(pw.port0, 0x08, (byte)(0x00 + (pw.ch & 7)));
            if (pw.ch == 7 && pw.mixer == 1)
            {
                pw.OutData(pw.port0, 0x0f, 0x00);
            }

        }

        public void OutAllKeyOff()
        {

            foreach (partWork pw in lstPartWork)
            {
                if (pw.dataEnd) continue;

                OutKeyOff(pw);
                OutSetTl(pw, 0, 127);
                OutSetTl(pw, 1, 127);
                OutSetTl(pw, 2, 127);
                OutSetTl(pw, 3, 127);
            }

        }


        public override void SetFNum(partWork pw)
        {

            int f = GetFNum(pw, pw.octaveNow, pw.noteCmd, pw.shift + pw.keyShift + pw.toneDoublerKeyShift);// + pw.arpDelta);//

            if (pw.bendWaitCounter != -1)
            {
                f = pw.bendFnum;
            }

            f = f + pw.detune;
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

            f = Common.CheckRange(f, 0, 9 * 12 * 64 - 1);
            int oct = f / (12 * 64);
            int note = (f - oct * 12 * 64) / 64;
            int kf = f - oct * 12 * 64 - note * 64;

            OutSetFnum(pw, oct, note, kf);
        }

        public override int GetFNum(partWork pw, int octave, char noteCmd, int shift)
        {
            int o = octave;
            int n = Const.NOTE.IndexOf(noteCmd) + shift - 1;

            o += n / 12;
            n %= 12;
            if (n < 0)
            {
                n += 12;
                o = Common.CheckRange(--o, 1, 8);
            }
            //if (n >= 0)
            //{
            //    o += n / 12;
            //    o = Common.CheckRange(o, 1, 8);
            //    n %= 12;
            //}
            //else
            //{
            //    o += n / 12 - ((n % 12 == 0) ? 0 : 1);
            //    if (o == 0 && n < 0)
            //    {
            //        o = 1;
            //        n = 0;
            //    }
            //    else
            //    {
            //        o = Common.CheckRange(o, 1, 8);
            //        n %= 12;
            //        if (n < 0) { n += 12; }
            //    }
            //}
            o--;

            return n * 64 + o * 12 * 64;
        }

        public byte[] FMVDAT = new byte[]{// ﾎﾞﾘｭｰﾑ ﾃﾞｰﾀ(FM)
        0x36,0x33,0x30,0x2D,
        0x2A,0x28,0x25,0x22,//  0,  1,  2,  3
        0x20,0x1D,0x1A,0x18,//  4,  5,  6,  7
        0x15,0x12,0x10,0x0D,//  8,  9, 10, 11
        0x0a,0x08,0x05,0x02 // 12, 13, 14, 15
        };

        public override void SetVolume(partWork pw)
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
            vol = Common.CheckRange(vol, 0, FMVDAT.Length - 1) - 4;//-4以下は-4へ、15以上は15へクリップ
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
                vol += pw.lfo[lfo].value + pw.lfo[lfo].param[6];
            }

            if (pw.beforeVolume != vol)
            {
                if (parent.instFM.ContainsKey(pw.instrument))
                {
                    OutSetVolume(pw, vol, pw.instrument);
                    pw.beforeVolume = vol;
                }
            }
        }

        public override void SetKeyOn(partWork pw)
        {
            pw.keyOn = true;
            //OutKeyOn(mml, page);
        }

        public override void SetKeyOff(partWork pw)
        {
            OutKeyOff(pw);
        }

        public override void SetLfoAtKeyOn(partWork pw)
        {
            for (int lfo = 0; lfo < 1; lfo++)
            {
                clsLfo pl = pw.lfo[lfo];

                if (!pl.sw)
                    continue;
                if (pl.type == enmLfoType.Hardware)
                    continue;
                //if (pl.param[5] != 1)
                //    continue;

                pl.isEnd = false;
                //pl.value = (pl.param[0] == 0) ? pl.param[6] : 0;//ディレイ中は振幅補正は適用されない
                pl.waitCounter = parent.GetWaitCounter(pl.param[0]);
                pl.direction = Math.Sign(pl.param[2]);
                if (pl.direction == 0) pl.direction = 1;
                pl.value = 0;
                pl.PeakLevelCounter = pl.param[3] >> 1;

                if (pl.type == enmLfoType.Vibrato)
                {
                    SetFNum(pw);

                }

                //if (pl.type == eLfoType.Tremolo)
                //{
                //    pw.beforeVolume = -1;
                //    SetFmVolume(pw);

                //}

            }
        }

        public override int GetToneDoublerShift(partWork pw, int octave, char noteCmd, int shift)
        {
            int i = pw.instrument;
            if (pw.TdA == -1)
            {
                return 0;
            }

            int TdB = octave * 12 + Const.NOTE.IndexOf(noteCmd) + shift;
            int s = pw.TdA - TdB;
            int us = Math.Abs(s);
            int n = pw.toneDoubler;
            if (us >= parent.instToneDoubler[n].lstTD.Count)
            {
                return 0;
            }

            return ((s < 0) ? s : 0) + parent.instToneDoubler[n].lstTD[us].KeyShift;
        }

        public override void SetToneDoubler(partWork pw,MML mml)
        {
            return;

        }


        public override void CmdNoiseToneMixer(partWork pw,MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 0, 1);
            pw.mixer = n;
        }

        public override void CmdNoise(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 0, 31);
            if (pw.noise != n)
            {
                pw.noise = n;
            }
        }

        public override void CmdMPMS(partWork pw, MML mml)
        {
            int n = (int)mml.args[1];
            n = Common.CheckRange(n, 0, 7);
            pw.pms = n;
            ((YM2151)pw.chip).OutSetPMSAMS(pw, pw.pms, pw.ams);
        }

        public override void CmdMAMS(partWork pw, MML mml)
        {
            int n = (int)mml.args[1];
            n = Common.CheckRange(n, 0, 3);
            pw.ams = n;
            ((YM2151)pw.chip).OutSetPMSAMS(pw, pw.pms, pw.ams);
        }

        public override void CmdLfo(partWork pw, MML mml)
        {
            base.CmdLfo(pw, mml);

            if (mml.args[0] is string) return;

            int c = (char)mml.args[0] - 'P';
            if (pw.lfo[c].type == enmLfoType.Hardware)
            {
                if (pw.lfo[c].param.Length < 4)
                {
                    msgBox.setErrMsg(msg.get("E16002"));//, mml.line.Lp);
                    return;
                }
                if (pw.lfo[c].param.Length > 5)
                {
                    msgBox.setErrMsg(msg.get("E16003"));//, mml.line.Lp);
                    return;
                }

                pw.lfo[c].param[0] = Common.CheckRange(pw.lfo[c].param[0], 0, 3); //Type
                pw.lfo[c].param[1] = Common.CheckRange(pw.lfo[c].param[1], 0, 255); //LFRQ
                pw.lfo[c].param[2] = Common.CheckRange(pw.lfo[c].param[2], 0, 127); //PMD
                pw.lfo[c].param[3] = Common.CheckRange(pw.lfo[c].param[3], 0, 127); //AMD
                if (pw.lfo[c].param.Length == 5)
                {
                    pw.lfo[c].param[4] = Common.CheckRange(pw.lfo[c].param[4], 0, 1);
                }
                else
                {
                    List<int> tmp = pw.lfo[c].param.ToList();
                    tmp.Add(0);
                    pw.lfo[c].param = tmp.ToArray();
                }
            }
        }

        public override void CmdLfoSwitch(partWork pw, MML mml)
        {
            base.CmdLfoSwitch(pw, mml);

            int c = (char)mml.args[0] - 'P';
            int n = (int)mml.args[1];
            if (pw.lfo[c].type == enmLfoType.Hardware)
            {
                ((YM2151)pw.chip).OutSetHardLfo(pw, (n == 0) ? false : true, pw.lfo[c].param.ToList());
            }
        }

        public override void CmdPan(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 0, 3);
            pw.ipan = (n == 1) ? 2 : (n == 2 ? 1 : n);
            if (pw.instrument < 0)
            {
                msgBox.setErrMsg(msg.get("E16004")
                    );//, mml.line.Lp);
            }
            //E10021
            else if (!parent.instFM.ContainsKey(pw.instrument))
            {
                msgBox.setErrMsg(string.Format(msg.get("E10021")
                    , pw.instrument));//, mml.line.Lp);
            }
            else
            {
                switch (parent.instFM[n].type)
                {
                    case 1: // @%

                        OutSetPanFeedbackAlgorithm(
                            pw
                            , pw.ipan
                            , (parent.instFM[n].data[24] & 0x38) >> 3
                            , parent.instFM[n].data[24] & 0x7
                            );
                        break;
                    case 4:
                        OutSetPanFeedbackAlgorithm(
                            pw
                            , (int)pw.ipan
                            , parent.instFM[pw.instrument].data[2]
                            , parent.instFM[pw.instrument].data[1]
                            );
                        break;
                }
            }
        }

        public override void CmdInstrument(partWork pw, MML mml)
        {
            char type = (char)mml.args[0];
            int n = (int)mml.args[1];

            if (type == 'I')
            {
                msgBox.setErrMsg(msg.get("E16005"));//, mml.line.Lp);
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

            n = Common.CheckRange(n, 0, 255);
            if (pw.instrument == n) return;

            pw.instrument = n;
            int modeBeforeSend = 0;// parent.info.modeBeforeSend;
            if (type == 'N')
            {
                modeBeforeSend = 0;
            }
            else if (type == 'R')
            {
                modeBeforeSend = 1;
            }
            else if (type == 'A')
            {
                modeBeforeSend = 2;
            }

            OutSetInstrument(pw, n, pw.volume, modeBeforeSend);
        }

        public override void CmdVolume(partWork pw, MML mml)
        {
            int n;
            n = (mml.args != null && mml.args.Count > 0) ? (int)mml.args[0] : pw.latestVolume;
            pw.volumeEasy = n;
            pw.latestVolume = n;
            {
                n = FMVDAT[n + 4];
                pw.volume = Common.CheckRange(n, 0, pw.MaxVolume);
                SetVolume(pw);
            }
        }

        public override void CmdVolumeUp(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = pw.volumeEasy + n;
            n = Common.CheckRange(n, 0, pw.MaxVolumeEasy);
            pw.volumeEasy = n;
            
            {
                n = FMVDAT[n + 4];
                pw.volume = Common.CheckRange(n, 0, pw.MaxVolume);
                SetVolume(pw);
            }
        }

        public override void CmdVolumeDown(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = pw.volumeEasy - n;
            n = Common.CheckRange(n, 0, pw.MaxVolumeEasy);
            pw.volumeEasy = n;
            
            {
                n = FMVDAT[n + 4];
                pw.volume = Common.CheckRange(n, 0, pw.MaxVolume);
                SetVolume(pw);
            }
        }

        public override void CmdY(partWork pw, MML mml)
        {
            if (mml.args[0] is string toneparamName)
            {
                byte op = (byte)(int)mml.args[1];
                op = (byte)(op == 1 ? 2 : (op == 2 ? 1 : op));
                byte dat = (byte)(int)mml.args[2];

                switch (toneparamName)
                {
                    case "PANFBAL":
                    case "PANFLCON":
                        pw.OutData(pw.port0, (byte)(0x20 + pw.ch), dat);
                        break;
                    case "PMSAMS":
                        pw.OutData(pw.port0, (byte)(0x38 + pw.ch), dat);
                        break;
                    case "DTML":
                    case "DTMUL":
                    case "DT1ML":
                    case "DT1MUL":
                        pw.OutData(pw.port0, (byte)(0x40 + pw.ch + op * 8), dat);
                        break;
                    case "TL":
                        pw.OutData(pw.port0, (byte)(0x60 + pw.ch + op * 8), dat);
                        break;
                    case "KSAR":
                        pw.OutData(pw.port0, (byte)(0x80 + pw.ch + op * 8), dat);
                        break;
                    case "AMDR":
                    case "AMED1R":
                        pw.OutData(pw.port0, (byte)(0xa0 + pw.ch + op * 8), dat);
                        break;
                    case "DT2SR":
                    case "DT2D2R":
                        pw.OutData(pw.port0, (byte)(0xc0 + pw.ch + op * 8), dat);
                        break;
                    case "SLRR":
                    case "D1LRR":
                        pw.OutData(pw.port0, (byte)(0xe0 + pw.ch + op * 8), dat);
                        break;
                }
            }
            else
            {
                byte adr = (byte)(int)mml.args[0];
                byte dat = (byte)(int)mml.args[1];
                pw.OutData(pw.port0, adr, dat);
            }
        }

        public override void CmdLoopExtProc(partWork pw, MML mml)
        {
        }

        public override void CmdDetune(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, -(9 * 12 * 64 - 1), (9 * 12 * 64 - 1));
            pw.detune = n;
            //SetDummyData(pw, mml);
        }

        //public override void SetupPageData(partWork pw, partPage page)
        //{

        //    OutKeyOff(null, page);

        //    //音色
        //    page.spg.instrument = -1;
        //    OutSetInstrument(page, null, page.instrument, page.volume, 'n');

        //    //周波数
        //    page.spg.freq = -1;
        //    SetFNum(page, null);

        //    //音量
        //    page.spg.beforeVolume = -1;
        //    SetVolume(page, null);

        //    //panは音色設定時に再設定されるので不要
        //}

        public override void MultiChannelCommand()
        {
            if (!use) return;
            //int dat = 0;

            foreach (partWork pw in lstPartWork)
            {
                if (pw.keyOn)
                {
                    pw.keyOn = false;
                    OutKeyOn(pw);
                }

            }


        }

    }
}