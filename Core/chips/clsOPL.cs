using System;

namespace Core
{
    public class ClsOPL : ClsChip
    {
        protected int[][] _FNumTbl = new int[1][] {
            //new int[13]
            new int[] {
            // OPL/2(FM) : Fnum = ftone*(2**19)/(M/ 72)/(2**B-1)       ftone:Hz M:MasterClock B:Block
            // OPL3(FM)  : Fnum = ftone*(2**19)/(M/288)/(2**B-1)       ftone:Hz M:MasterClock B:Block
            //   c    c+     d    d+     e     f    f+     g    g+     a    a+     b    >c
             0x158,0x16a,0x180,0x198,0x1b0,0x1ca,0x1e4,0x202,0x220,0x240,0x262,0x286,0x2b0
            }
        };

        public ClsOPL(ClsVgm parent, int chipID, string initialPartName, string stPath, bool isSecondary) : base(parent, chipID, initialPartName, stPath, isSecondary)
        {
        }

        public override void InitChip()
        {

            if (!use) return;

            //FM Off
            outAllKeyOff(null, lstPartWork[0]);
            rhythmStatus = 0x00;
            beforeRhythmStatus = 0xff;
            connectionSel = 0;
            beforeConnectionSel = -1;

        }

        public override void InitPart(ref partWork pw)
        {
            pw.beforeVolume = -1;
            pw.volume = 60;
            pw.MaxVolume = 63;
            pw.MaxVolumeEasy = 15;
            pw.beforeEnvInstrument = 0;
            pw.envInstrument = 0;
            pw.port0 = port[0];
            //pg.port1 = port[1];
            pw.mixer = 0;
            pw.noise = 0;
            pw.ipan = 3;
            pw.Type = enmChannelType.FMOPL;
            pw.isOp4Mode = false;
            if (pw.ch > 8) pw.Type = enmChannelType.RHYTHM;
        }

        public virtual byte ChnToBaseReg(int chn)
        {
            //Console.Write("Enter ChnToBaseReg: Ch{0}", chn + 1);
            chn %= 9; // A1=LでもA1=Hでもいっしょ。
            byte carrier = (byte)((chn / 3) * 8 + (chn % 3));
            return carrier;
        }

        public virtual byte getPortFromCh(int ch)
        {

            if (ch >= 9)
            {
                //Console.WriteLine("getPortFromCh port1");
                return port[1];
            }
            //Console.WriteLine("getPortFromCh port0");
            return port[0];
        }

        public virtual void outAllKeyOff(MML mml, partWork pw)
        {
            parent.OutData(port[0], 0xBD, 0);
            // Probably wise to reset Rhythm mode.
            for (byte adr = 0; adr <= 8; adr++)
            {
                //Ch Off
                parent.OutData(port[0], (byte)(0xB0 + adr), 0);
            }
        }

        public virtual void outOPLSetInstrument(partWork pw, int n, int modeBeforeSend)
        {
            pw.instrument = n;

            if (!parent.instFM.ContainsKey(n) || parent.instFM[n].type != 3)
            {
                msgBox.setWrnMsg(string.Format(msg.get("E17000"), n));//, mml.line.Lp);
                return;
            }

            if (pw.Type == enmChannelType.RHYTHM)
            {
                SetInstToRhythmChannel(pw, n, modeBeforeSend);
                return;
            }

            //if (parent.instFM[n].Length == Const.OPL_OP4_INSTRUMENT_SIZE)
            //{
            //    SetInst4Operator(pw, mml, n, modeBeforeSend, pw.ppg[pw.cpgNum].ch);
            //    return;
            //}

            SetInst2Operator(pw, n, modeBeforeSend, pw.ch);
        }

        protected virtual void SetInstToRhythmChannel(partWork pw, int n, int modeBeforeSend)
        {
            if (rhythmStatus == 0) return;

            if (pw.ch == 9)//BD
            {
                int vch = 6;
                SetInst2Operator(pw, n, modeBeforeSend, vch);
            }
            else if (pw.ch == 10)//SD
            {
                int opeNum = 16;
                SetInst1Operator(pw, n, modeBeforeSend, opeNum);
            }
            else if (pw.ch == 11)//TOM
            {
                int opeNum = 14;
                SetInst1Operator(pw, n, modeBeforeSend, opeNum);
            }
            else if (pw.ch == 12)//CYM
            {
                int opeNum = 17;
                SetInst1Operator(pw, n, modeBeforeSend, opeNum);
            }
            else if (pw.ch == 13)//HH
            {
                int opeNum = 13;
                SetInst1Operator(pw, n, modeBeforeSend, opeNum);
            }
        }

        protected virtual void SetInst1Operator(partWork pw, int n, int modeBeforeSend, int opeNum)
        {
            mucomVoice inst = parent.instFM[n];
            int targetBaseReg = (opeNum / 6) * 8 + (opeNum % 6);
            byte port = this.port[opeNum / 18];
            int ope = (opeNum % 6) / 3;

            switch (modeBeforeSend)
            {
                case 0: // N)one
                    break;
                case 1: // R)R only
                    pw.OutData(port, (byte)(targetBaseReg + ope * 3 + 0x80)
                        , ((0 & 0xf) << 4) | (15 & 0xf));//SL RR
                    break;
                case 2: // A)ll
                    SetInstAtOneOpeWithoutKslTl(pw, opeNum
                        , 15, 15, 0, 15, 0, 0, 0, 0, 0, 0);
                    pw.OutData(port, (byte)(targetBaseReg + ope * 3 + 0x40)
                        , ((0 & 0x3) << 6) | 0x3f);  //KL(M) TL
                    break;
            }

            SetInstAtOneOpeWithoutKslTl(pw, opeNum,
                inst.data[ope * 12 + 1 + 0],//AR
                inst.data[ope * 12 + 1 + 1],//DR
                inst.data[ope * 12 + 1 + 2],//SL
                inst.data[ope * 12 + 1 + 3],//RR
                inst.data[ope * 12 + 1 + 6],//MT
                inst.data[ope * 12 + 1 + 7],//AM
                inst.data[ope * 12 + 1 + 8],//VIB
                inst.data[ope * 12 + 1 + 9],//EGT
                inst.data[ope * 12 + 1 + 10],//KSR
                inst.data[ope * 12 + 1 + 11]//WS
            );

            int cnt = inst.data[25];
            if (cnt == 0 || pw.Type == enmChannelType.RHYTHM)
            {
                if (ope == 0)
                {
                    //OP1
                    pw.OutData(port, (byte)(0x40 + targetBaseReg + 0)
                        , (byte)(((inst.data[12 * 0 + 5] & 0x3) << 6) | (inst.data[12 * 0 + 6] & 0x3f))); //KL(M) TL
                }
            }

            SetInstAtChannelPanFbCnt(pw, (opeNum % 6) % 3 + (opeNum / 6) * 3, (int)pw.ipan, inst.data[26], inst.data[25]);

            pw.beforeVolume = -1;
        }

        protected virtual void SetInst2Operator(partWork pw, int n, int modeBeforeSend, int vch)
        {
            mucomVoice inst = parent.instFM[n];
            byte targetBaseReg = ChnToBaseReg(vch);
            byte port = getPortFromCh(vch);

            switch (modeBeforeSend)
            {
                case 0: // N)one
                    break;
                case 1: // R)R only
                    for (int ope = 0; ope < 2; ope++)
                        pw.OutData(port, (byte)(targetBaseReg + ope * 3 + 0x80)
                            , ((0 & 0xf) << 4) | (15 & 0xf));//SL RR
                    break;
                case 2: // A)ll
                    for (byte ope = 0; ope < 2; ope++)
                    {
                        SetInstAtOneOpeWithoutKslTl(pw, (vch / 3 * 6) + (vch % 3) + ope * 3
                            , 15, 15, 0, 15, 0, 0, 0, 0, 0, 0);
                        pw.OutData(port, (byte)(targetBaseReg + ope * 3 + 0x40)
                            , ((0 & 0x3) << 6) | 0x3f);  //KL(M) TL
                    }
                    break;
            }

            int slot1_operatorNumber = (vch / 3 * 6) + (vch % 3) + 0;

            for (int ope = 0; ope < 2; ope++)
            {
                SetInstAtOneOpeWithoutKslTl(pw, slot1_operatorNumber + ope * 3,
                    inst.data[ope * 12 + 3 + 0],
                    inst.data[ope * 12 + 3 + 1],
                    inst.data[ope * 12 + 3 + 2],
                    inst.data[ope * 12 + 3 + 3],
                    inst.data[ope * 12 + 3 + 6],
                    inst.data[ope * 12 + 3 + 7],
                    inst.data[ope * 12 + 3 + 8],
                    inst.data[ope * 12 + 3 + 9],
                    inst.data[ope * 12 + 3 + 10],
                    inst.data[ope * 12 + 3 + 11]
                    );
            }

            //TLはvolumeの設定と一緒に行うがキャリアのみである。
            //そのため、CNT0の場合はモジュレータのパラメータをセットする必要がある
            int cnt = inst.data[1];
            if (cnt == 0)
            {
                //OP1
                pw.OutData(port, (byte)(0x40 + ChnToBaseReg(vch) + 0)
                    , (byte)(((inst.data[12 * 0 + 3 + 4] & 0x3) << 6) | (inst.data[12 * 0 + 3 + 5] & 0x3f))); //KL(M) TL
            }

            SetInstAtChannelPanFbCnt(pw, vch, (int)pw.ipan, inst.data[2], inst.data[1]);

            pw.beforeVolume = -1;
        }

        protected virtual void SetInst4Operator(partWork pw, int n, int modeBeforeSend, int vch)
        {
            if (!pw.isOp4Mode)
            {
                msgBox.setErrMsg(string.Format(msg.get("E26000"), n));//, mml.line.Lp);
                return;
            }

            mucomVoice inst = parent.instFM[n];
            byte targetBaseReg = ChnToBaseReg(vch);
            byte port = getPortFromCh(vch);

            switch (modeBeforeSend)
            {
                case 0: // N)one
                    break;
                case 1: // R)R only
                    for (int ope = 0; ope < 2; ope++)
                        pw.OutData(port, (byte)(targetBaseReg + ope * 3 + 0x80)
                            , ((0 & 0xf) << 4) | (15 & 0xf));//SL RR
                    break;
                case 2: // A)ll
                    for (byte ope = 0; ope < 2; ope++)
                    {
                        SetInstAtOneOpeWithoutKslTl(pw, (vch / 3 * 6) + (vch % 3) + ope * 3
                            , 15, 15, 0, 15, 0, 0, 0, 0, 0, 0);
                        pw.OutData(port, (byte)(targetBaseReg + ope * 3 + 0x40)
                            , ((0 & 0x3) << 6) | 0x3f);  //KL(M) TL
                    }
                    break;
            }

            int slot1_operatorNumber = (vch / 3 * 6) + (vch % 3) + 0;

            for (int ope = 0; ope < 4; ope++)
            {
                SetInstAtOneOpeWithoutKslTl(pw, slot1_operatorNumber + ope * 3,
                    inst.data[ope * 12 + 1 + 0],
                    inst.data[ope * 12 + 1 + 1],
                    inst.data[ope * 12 + 1 + 2],
                    inst.data[ope * 12 + 1 + 3],
                    inst.data[ope * 12 + 1 + 6],
                    inst.data[ope * 12 + 1 + 7],
                    inst.data[ope * 12 + 1 + 8],
                    inst.data[ope * 12 + 1 + 9],
                    inst.data[ope * 12 + 1 + 10],
                    inst.data[ope * 12 + 1 + 11]
                    );
            }

            //TLはvolumeの設定と一緒に行うがキャリアのみである。
            //そのため、CNT0の場合はモジュレータのパラメータをセットする必要がある
            int cnt1 = inst.data[49];
            int cnt2 = inst.data[50];
            bool op1 = false;
            bool op2 = false;
            bool op3 = false;

            if (cnt1 == 0 && cnt2 == 0) { op1 = true; op2 = true; op3 = true; }
            else if (cnt1 == 0 && cnt2 == 1) { op1 = true; op3 = true; }
            else if (cnt1 == 1 && cnt2 == 0) { op2 = true; op3 = true; }
            else if (cnt1 == 1 && cnt2 == 1) { op2 = true; }

            if (op1)
                pw.OutData(port, (byte)(0x40 + ChnToBaseReg(vch) + 0)
                    , (byte)(((inst.data[12 * 0 + 5] & 0x3) << 6) | (inst.data[12 * 0 + 6] & 0x3f))); //KL(M) TL

            if (op2)
                pw.OutData(port, (byte)(0x40 + ChnToBaseReg(vch) + 3)
                    , (byte)(((inst.data[12 * 1 + 5] & 0x3) << 6) | (inst.data[12 * 1 + 6] & 0x3f))); //KL(M) TL

            if (op3)
                pw.OutData(port, (byte)(0x40 + ChnToBaseReg(vch) + 8)
                    , (byte)(((inst.data[12 * 2 + 5] & 0x3) << 6) | (inst.data[12 * 2 + 6] & 0x3f))); //KL(M) TL


            SetInstAtChannelPanFbCnt(pw, vch, (int)pw.ipan, inst.data[51], cnt1);
            SetInstAtChannelPanFbCnt(pw, vch + 3, (int)pw.ipan, inst.data[51], cnt2);

            pw.beforeVolume = -1;
        }

        protected virtual void SetInstAtOneOpeWithoutKslTl(partWork pw, int opeNum,
            int ar, int dr, int sl, int rr,
            int mt, int am, int vib, int eg,
            int kr,
            int ws
            )
        {
            //portは18operator毎に切り替わる
            byte port = this.port[opeNum / 18];

            // % 18       ... port毎のoperator番号を得る --- (1)
            // / 6 ) * 8  ... (1) に対応するアドレスは6opeごとに8アドレス毎に分けられ、
            // % 6        ...                         0～5アドレスに割り当てられている
            int adr = ((opeNum % 18) / 6) * 8 + (opeNum % 6);

            ////slot1かslot2を求める
            //// % 6        ... slotは6opeの範囲で0か1を繰り返す
            //// / 3        ... slotは3ope毎に0か1を繰り返す
            //int slot = (opeNum % 6) / 3;

            pw.OutData(port, (byte)(0x80 + adr), (byte)(((sl & 0xf) << 4) | (rr & 0xf)));
            pw.OutData(port, (byte)(0x60 + adr), (byte)(((ar & 0xf) << 4) | (dr & 0xf)));
            SetInstAtOneOpeAmVibEgKsMl(pw, port, (byte)(0x20 + adr), mt, am, vib, eg, kr);
            //SOutData(page,mml, port, (byte)(0xe0 + adr), (byte)(ws & 0x7));
        }

        protected virtual void SetInstAtChannelPanFbCnt(partWork pw, int chNum, int pan, int fb, int cnt)
        {
            //portは9channel毎に切り替わる
            byte port = this.port[chNum / 9];

            pw.OutData(port,
                (byte)(chNum % 9 + 0xC0),
                (byte)(
                    ((fb & 0x07) << 1) | (cnt & 0x01) | (pan * 0x10) // PAN(CHA,CHB (CHC,CHDは未使用))
                )
            );
        }

        public virtual void SetInstAtOneOpeAmVibEgKsMl(partWork pw, byte port, byte adr, int ml, int am, int vib, int eg, int kr)
        {
            // 0x20
            pw.OutData(port,
                adr,
                 (byte)((am != 0 ? 0x80 : 0) + (vib != 0 ? 0x40 : 0) + (eg != 0 ? 0x20 : 0) + (kr != 0 ? 0x10 : 0) + (ml & 0xf))
                );
        }


        public virtual void OutFmSetFnum(partWork pw, int octave, int num)
        {
            int freq;
            freq = (int)((num & 0x3ff) | (((octave - 1) & 0x7) << 10));
            pw.freq = freq;
        }

        public virtual void SetFmFNum(partWork pw)
        {
            if (pw.noteCmd == (char)0)
            {
                return;
            }

            //if (pw.ppg[pw.cpgNum].Type == enmChannelType.RHYTHM) return;

            int[] ftbl = FNumTbl[0];

            int f = GetFmFNum(ftbl, pw.octaveNow, pw.noteCmd, pw.shift);// + pw.keyShift + pw.toneDoublerKeyShift );//
            if (pw.bendWaitCounter != -1)
            {
                f = pw.bendFnum;
                //Console.Write("ff:{0}   ", f);
            }
            int o = (f & 0x1c00) >> 10;
            f &= 0x3ff;

            f += pw.detune;
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
                f = f - ftbl[0] * 2;// + ftbl[0];
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

            f = Common.CheckRange(f, 0, 0x3ff);
            //Console.WriteLine("o:{0} f:{1}",o,f);
            OutFmSetFnum(pw, o, f);
        }

        public virtual int GetFmFNum(int[] ftbl, int octave, char noteCmd, int shift)
        {
            int o = octave;
            int n = Const.NOTE.IndexOf(noteCmd) + shift;

            o += n / 12;
            n %= 12;
            if (n < 0)
            {
                n += 12;
                o = Common.CheckRange(--o, 1, 8);
            }

            int f = ftbl[n];

            return (f & 0x3ff) + ((o & 0x7) << 10);
        }

        public byte[] FMVDAT = new byte[]{// ﾎﾞﾘｭｰﾑ ﾃﾞｰﾀ(FM)
        0x36/2 , 0x33/2 , 0x30/2 , 0x2D/2 ,
        0x2A/2 , 0x28/2 , 0x25/2 , 0x22/2 ,//  0,  1,  2,  3
        0x20/2 , 0x1D/2 , 0x1A/2 , 0x18/2 ,//  4,  5,  6,  7
        0x15/2 , 0x12/2 , 0x10/2 , 0x0D/2 ,//  8,  9, 10, 11
        0x0a/2 , 0x08/2 , 0x05/2 , 0x02/2  // 12, 13, 14, 15
        };

        public override void CmdVolume(partWork pw, MML mml)
        {
            int n;
            n = (mml.args != null && mml.args.Count > 0) ? (int)mml.args[0] : pw.latestVolume;
            pw.volumeEasy = n;
            pw.latestVolume = n;
            if (pw.Type == enmChannelType.FMOPL || pw.Type == enmChannelType.RHYTHM)
            {
                n = (int)(sbyte)n;//先ず-128～127の範囲にキャスト
                if (n > 15) n = -4;//16以上の場合は-4として扱う
                n = Common.CheckRange(n, -4, 15);//-4以下は-4へ、15以上は15へクリップ
                n = FMVDAT[n + 4];//ボリュームテーブル参照

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
            if (pw.Type == enmChannelType.FMOPL || pw.Type == enmChannelType.RHYTHM)
            {
                n = (int)(sbyte)n;//先ず-128～127の範囲にキャスト
                if (n > 15) n = -4;//16以上の場合は-4として扱う
                n = Common.CheckRange(n, -4, 15);//-4以下は-4へ、15以上は15へクリップ
                n = FMVDAT[n + 4];//ボリュームテーブル参照

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
            if (pw.Type == enmChannelType.FMOPL || pw.Type == enmChannelType.RHYTHM)
            {
                n = (int)(sbyte)n;//先ず-128～127の範囲にキャスト
                if (n > 15) n = -4;//16以上の場合は-4として扱う
                n = Common.CheckRange(n, -4, 15);//-4以下は-4へ、15以上は15へクリップ
                n = FMVDAT[n + 4];//ボリュームテーブル参照

                pw.volume = n;// Common.CheckRange(n, 0, pw.MaxVolume);
                SetFmVolume(pw);
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
                vol += pw.lfo[lfo].value + pw.lfo[lfo].param[6];
            }

            //if (pw.ppg[pw.cpgNum].beforeVolume != vol)
            //{
            //if (parent.instFM.ContainsKey(pw.ppg[pw.cpgNum].instrument))
            //{
            pw.volume = vol;
            //outYM2413SetInstVol(pw, pw.ppg[pw.cpgNum].envInstrument, vol);
            //pw.ppg[pw.cpgNum].beforeVolume = vol;
            //}
            //}
        }

        public override void GetFNumAtoB(partWork pw
            , out int a, int aOctaveNow, char aCmd, int aShift
            , out int b, int bOctaveNow, char bCmd, int bShift
            , int dir)
        {
            a = GetFNum(pw, aOctaveNow, aCmd, aShift);
            b = GetFNum(pw, bOctaveNow, bCmd, bShift);

            int oa = (a & 0x1c00) >> 10;
            int ob = (b & 0x1c00) >> 10;
            if (oa != ob)
            {
                if ((a & 0x3ff) == FNumTbl[0][0])
                {
                    oa += Math.Sign(ob - oa);
                    a = (a & 0x3ff) * 2 + (oa << 10);
                }
                else if ((b & 0x3ff) == FNumTbl[0][0])
                {
                    ob += Math.Sign(oa - ob);
                    b = (b & 0x3ff) * ((dir > 0) ? 2 : 1) + (ob << 10);
                }
            }
        }

        public override void SetFNum(partWork pw)
        {
            SetFmFNum(pw);
        }

        public override int GetFNum(partWork pw, int octave, char cmd, int shift)
        {
            int[] ftbl = FNumTbl[0];
            return GetFmFNum(ftbl, octave, cmd, shift);
        }

        public override void SetVolume(partWork pw)
        {
            SetFmVolume(pw);
        }

        public override void SetKeyOn(partWork pw)
        {
            pw.keyOn = true;
            //SetDummyData(pw);
        }

        public override void SetKeyOff(partWork pw)
        {
            pw.keyOn = false;
            pw.keyOff = true;
            pw.beforeFNum = -1;
        }

        public override void SetLfoAtKeyOn(partWork pw)
        {
        }

        public override void SetToneDoubler(partWork pw,MML mml)
        {
            //実装不要
        }

        public override int GetToneDoublerShift(partWork pw, int octave, char noteCmd, int shift)
        {
            return 0;
        }

        public override void CmdInstrument(partWork pw, MML mml)
        {
            char type = (char)mml.args[0];
            int n = (int)mml.args[1];

            if (type == 'T')
            {
                msgBox.setErrMsg(msg.get("E17001"));//, mml.line.Lp);
                return;
            }

            if (type == 'E')
            {
                n = SetEnvelopParamFromInstrument(pw, n, mml);
                return;
            }

            if (type == 'I')
            {
                n = Common.CheckRange(n, 1, 15);
                if (pw.envInstrument != n)
                {
                    pw.envInstrument = n;
                }
                //SetDummyData(page, mml);
                return;
            }

            n = Common.CheckRange(n, 0, 255);
            if (pw.instrument == n) return;

            pw.instrument = n;
            //int modeBeforeSend = parent.info.modeBeforeSend;
            //if (type == 'N')
            //{
            //    modeBeforeSend = 0;
            //}
            //else if (type == 'R')
            //{
            //    modeBeforeSend = 1;
            //}
            //else if (type == 'A')
            //{
            //    modeBeforeSend = 2;
            //}

            outOPLSetInstrument(pw, n, 0); //音色のセット
            pw.envInstrument = 0;

        }

        public override void CmdMode(partWork pw, MML mml)
        {
            //Console.WriteLine("CmdMode()");

            int n = (int)mml.args[0];

            if ((pw.ch > 5 && pw.ch < 9) || pw.Type == enmChannelType.RHYTHM)
            {
                if (n == 0) pw.chip.rhythmStatus &= 0xdf;
                else pw.chip.rhythmStatus |= 0x20;

                return;
            }

            if (pw.Type == enmChannelType.FMOPL)
            {
                if (pw.ch >= 0 && pw.ch < 6)
                {
                    int tch = pw.ch % 3;

                    if (n == 0)
                    {
                        pw.chip.connectionSel &= (~(1 << tch)) & 0x3f;

                        tch += (pw.ch > 8 ? 6 : 0);
                        //pw.chip.lstPartWork[tch].cpg.isOp4Mode = false;
                        //pw.chip.lstPartWork[tch + 3].cpg.isOp4Mode = false;
                    }
                    else
                    {
                        pw.chip.connectionSel |= (1 << tch);

                        tch += (pw.ch > 8 ? 6 : 0);
                        //pw.chip.lstPartWork[tch].cpg.isOp4Mode = true;
                        //pw.chip.lstPartWork[tch + 3].cpg.isOp4Mode = true;
                    }

                }
            }
        }

        public override void CmdPan(partWork pw, MML mml)
        {
            throw new NotImplementedException();
        }

        public override void CmdLoopExtProc(partWork pw, MML mml)
        {
        }

        public override void CmdY(partWork pw, MML mml)
        {
            if (mml.args[0] is string) return;

            byte adr = (byte)(int)mml.args[0];
            byte dat = (byte)(int)mml.args[1];
            //int p = 0;

            pw.OutData(pw.port0, adr, dat);
        }

        public override void CmdDetune(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, -0x3ff, 0x3ff);
            pw.detune = n;
            //SetDummyData(pw, mml);
        }

        public override void MultiChannelCommand()
        {

            foreach (partWork pw in lstPartWork)
            {
                if (pw.Type == enmChannelType.FMOPL)
                {
                    if (pw.beforeVolume != pw.volume && parent.instFM.ContainsKey(pw.instrument))
                    {
                        pw.beforeVolume = pw.volume;


                        int cnt = parent.instFM[pw.instrument].data[1];
                        if (cnt != 0)
                        {
                            //OP1
                            pw.OutData(pw.port0,
                                (byte)(0x40 + ChnToBaseReg(pw.ch) + 0),
                                (byte)(
                                        ((parent.instFM[pw.instrument].data[12 * 0 + 3 + 4] & 0x3) << 6)  //KL(M)
                                        | Common.CheckRange(((parent.instFM[pw.instrument].data[12 * 0 + 3 + 5] & 0x3f) + (pw.volume & 0x3f)), 0, 63) //TL
                                    )
                                );
                        }
                        //OP2
                        pw.OutData(pw.port0,
                            (byte)(0x40 + ChnToBaseReg(pw.ch) + 3),
                            (byte)(
                                    ((parent.instFM[pw.instrument].data[12 * 1 + 3 + 4] & 0x3) << 6)  //KL(M)
                                    | Common.CheckRange(((parent.instFM[pw.instrument].data[12 * 1 + 3 + 5] & 0x3f) + (pw.volume & 0x3f)), 0, 63) //TL
                                )
                            );
                    }

                    if (pw.keyOff)
                    {
                        //該当チャンネルのbit5をオフにする
                        pw.keyOff = false;
                        pw.OutData(getPortFromCh(pw.ch)
                            , (byte)(0xB0 + pw.ch)
                            , (byte)(
                                ((pw.freq >> 8) & 0x1f)
                              )
                            );
                    }

                    if (pw.beforeFNum != (pw.freq | (pw.keyOn ? 0x4000 : 0x0000)))
                    {
                        pw.beforeFNum = pw.freq | (pw.keyOn ? 0x4000 : 0x0000);
                        //Console.WriteLine("CalcPitch {0} {1}_{2}", pw.ppg[pw.cpgNum].freq, pw.ppg[pw.cpgNum].freq >> 8 & 0x1F, pw.ppg[pw.cpgNum].freq & 0xFF);
                        pw.OutData(getPortFromCh(pw.ch), (byte)(0xa0 + pw.ch), (byte)pw.freq);
                        pw.OutData(getPortFromCh(pw.ch)
                            , (byte)(0xB0 + pw.ch)
                            , (byte)(
                                ((pw.freq >> 8) & 0x1f)
                                | (pw.keyOn ? 0x20 : 0x00)
                              )
                            );
                    }
                }

                else if (pw.Type == enmChannelType.RHYTHM)
                {
                    if (pw.beforeVolume != pw.volume && parent.instFM.ContainsKey(pw.instrument))
                    {
                        pw.beforeVolume = pw.volume;

                        if (pw.ch == 9)
                        {
                            int vch = 6;

                            int cnt = parent.instFM[pw.instrument].data[25];
                            if (cnt != 0)
                            {
                                //OP1
                                pw.OutData(pw.port0,
                                    (byte)(0x40 + ChnToBaseReg(vch) + 0),
                                    (byte)(
                                            ((parent.instFM[pw.instrument].data[12 * 0 + 5] & 0x3) << 6)  //KL(M)
                                            | Common.CheckRange(((parent.instFM[pw.instrument].data[12 * 0 + 6] & 0x3f) + (63 - (pw.volume & 0x3f))), 0, 63) //TL
                                        )
                                    );
                            }
                            //OP2
                            pw.OutData(pw.port0,
                                (byte)(0x40 + ChnToBaseReg(vch) + 3),
                                (byte)(
                                        ((parent.instFM[pw.instrument].data[12 * 1 + 5] & 0x3) << 6)  //KL(M)
                                        | Common.CheckRange(((parent.instFM[pw.instrument].data[12 * 1 + 6] & 0x3f) + (63 - (pw.volume & 0x3f))), 0, 63) //TL
                                    )
                                );
                        }
                        else if (pw.ch == 10)
                        {
                            int vch = 7;
                            //OP2
                            pw.OutData(pw.port0,
                                (byte)(0x40 + ChnToBaseReg(vch) + 3),
                                (byte)(
                                        ((parent.instFM[pw.instrument].data[12 * 1 + 5] & 0x3) << 6)  //KL(M)
                                        | Common.CheckRange(((parent.instFM[pw.instrument].data[12 * 1 + 6] & 0x3f) + (63 - (pw.volume & 0x3f))), 0, 63) //TL
                                    )
                                );
                        }
                        else if (pw.ch == 11)
                        {
                            int vch = 8;
                            //int cnt = parent.instFM[pw.ppg[pw.cpgNum].instrument][25];
                            //if (cnt != 0)
                            {
                                //OP1
                                pw.OutData(pw.port0,
                                    (byte)(0x40 + ChnToBaseReg(vch) + 0),
                                    (byte)(
                                            ((parent.instFM[pw.instrument].data[12 * 0 + 5] & 0x3) << 6)  //KL(M)
                                            | Common.CheckRange(((parent.instFM[pw.instrument].data[12 * 0 + 6] & 0x3f) + (63 - (pw.volume & 0x3f))), 0, 63) //TL
                                        )
                                    );
                            }
                        }
                        else if (pw.ch == 12)
                        {
                            int vch = 8;
                            //OP2
                            pw.OutData(pw.port0,
                                (byte)(0x40 + ChnToBaseReg(vch) + 3),
                                (byte)(
                                        ((parent.instFM[pw.instrument].data[12 * 1 + 5] & 0x3) << 6)  //KL(M)
                                        | Common.CheckRange(((parent.instFM[pw.instrument].data[12 * 1 + 6] & 0x3f) + (63 - (pw.volume & 0x3f))), 0, 63) //TL
                                    )
                                );
                        }
                        else if (pw.ch == 13)
                        {
                            int vch = 7;
                            //int cnt = parent.instFM[pw.ppg[pw.cpgNum].instrument][25];
                            //if (cnt != 0)
                            {
                                //OP1
                                pw.OutData(pw.port0,
                                    (byte)(0x40 + ChnToBaseReg(vch) + 0),
                                    (byte)(
                                            ((parent.instFM[pw.instrument].data[12 * 0 + 5] & 0x3) << 6)  //KL(M)
                                            | Common.CheckRange(((parent.instFM[pw.instrument].data[12 * 0 + 6] & 0x3f) + (63 - (pw.volume & 0x3f))), 0, 63) //TL
                                        )
                                    );
                            }
                        }
                    }

                    if (pw.beforeFNum != (pw.freq | (pw.keyOn ? 0x4000 : 0x0000)))
                    {
                        pw.beforeFNum = pw.freq | (pw.keyOn ? 0x4000 : 0x0000);
                        //Console.WriteLine("CalcPitch {0} {1}_{2}", pw.ppg[pw.cpgNum].freq, pw.ppg[pw.cpgNum].freq >> 8 & 0x1F, pw.ppg[pw.cpgNum].freq & 0xFF);

                        int vch = 0;
                        if (pw.ch == 9)//bd
                        {
                            vch = 6;
                        }
                        else if (pw.ch == 10)//sd
                        {
                            vch = 7;
                        }
                        else if (pw.ch == 11)//tom
                        {
                            vch = 8;
                        }
                        else if (pw.ch == 12)//CYM
                        {
                            vch = 8;
                        }
                        else if (pw.ch == 13)//HH
                        {
                            vch = 7;
                        }

                        pw.OutData(getPortFromCh(vch), (byte)(0xa0 + vch), (byte)pw.freq);
                        pw.OutData(getPortFromCh(vch)
                            , (byte)(0xB0 + vch)
                            , (byte)(
                                ((pw.freq >> 8) & 0x1f)
                              //| (pw.ppg[pw.cpgNum].keyOn ? 0x20 : 0x00)
                              )
                            );
                    }

                }
            }


            rhythmStatus &= 0xe0;
            rhythmStatus |= (byte)(
                (lstPartWork[9].keyOn ? 0x10 : 0x00)
                | (lstPartWork[10].keyOn ? 0x08 : 0x00)
                | (lstPartWork[11].keyOn ? 0x04 : 0x00)
                | (lstPartWork[12].keyOn ? 0x02 : 0x00)
                | (lstPartWork[13].keyOn ? 0x01 : 0x00)
                );

            if (beforeRhythmStatus != rhythmStatus)
            {
                beforeRhythmStatus = rhythmStatus;
                lstPartWork[9].OutData(lstPartWork[9].port0, 0xbd, rhythmStatus);
            }

        }



    }
}
