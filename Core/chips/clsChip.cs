﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Core
{
    abstract public class ClsChip
    {
        public enmChipType chipType
        {
            get
            {
                return _chipType;
            }
        }
        protected enmChipType _chipType = enmChipType.None;

        public string Name
        {
            get
            {
                return _Name;
            }
        }
        protected string _Name = "";

        public string ShortName
        {
            get
            {
                return _ShortName;
            }
        }
        protected string _ShortName = "";

        public int ChMax
        {
            get
            {
                return _ChMax;
            }
        }
        protected int _ChMax = 0;

        public int ChipID
        {
            get
            {
                return _ChipID;
            }
        }

        public bool CanUsePcm
        {
            get
            {
                return _canUsePcm;
            }

            set
            {
                _canUsePcm = value;
            }
        }
        protected bool _canUsePcm = false;

        public bool CanUsePI
        {
            get
            {
                return _canUsePI;
            }

            set
            {
                _canUsePI = value;
            }
        }
        protected bool _canUsePI = false;

        public bool IsSecondary
        {
            get
            {
                return _IsSecondary;
            }

            set
            {
                _IsSecondary = value;
            }
        }

        public bool SupportReversePartWork = false;
        public bool ReversePartWork = false;

        protected bool _IsSecondary = false;

        public Function Envelope = null;

        protected int _ChipID = -1;
        protected ClsVgm parent;
        protected byte dataType;
        public ClsChannel[] Ch;
        public int Frequency = 7670454;
        public bool use;
        public List<partWork> lstPartWork;
        public double[] noteTbl = new double[] {
            //   c       c+        d       d+        e        f       f+        g       g+        a       a+        b
            261.62 , 277.18 , 293.66 , 311.12 , 329.62 , 349.22 , 369.99 , 391.99 , 415.30 , 440.00 , 466.16 , 493.88
        };
        public int[][] FNumTbl;
        private string stPath = "";
        public clsPcmDataInfo[] pcmDataInfo;
        public byte[] pcmDataEasy = null;
        public List<byte[]> pcmDataDirect = new List<byte[]>();
        public bool isLoopEx;

        public ClsChip(ClsVgm parent, int chipID, string initialPartName, string stPath, bool isSecondary)
        {
            this.parent = parent;
            this._ChipID = chipID;
            this.stPath = stPath;
            this.IsSecondary = IsSecondary;
            MakeFNumTbl();
        }


        protected Dictionary<string, List<double>> MakeFNumTbl()
        {
            //for (int i = 0; i < noteTbl.Length; i++)
            //{
            //    FNumTbl[0][i] = (int)(Math.Round(((144.0 * noteTbl[i] * Math.Pow(2.0, 20) / Frequency) / Math.Pow(2.0, (4 - 1))), MidpointRounding.AwayFromZero));
            //}
            //FNumTbl[0][12] = FNumTbl[0][0] * 2;

            string fn = string.Format("FNUM_{0}.txt", Name);
            Stream stream = null;
            Dictionary<string, List<double>> dic = new Dictionary<string, List<double>>();

            //log.ForcedWrite(stPath);
            //log.ForcedWrite(fn);
            fn = Path.Combine(stPath, fn);
            if (File.Exists(fn))
            {
                stream = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            else
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                string[] resources = asm.GetManifestResourceNames();
                foreach (string resource in resources)
                {
                    if (resource.IndexOf(fn) >= 0)
                    {
                        fn = resource;
                    }
                }
                stream = asm.GetManifestResourceStream(fn);
            }

            if (stream == null)
            {
                return null;
            }

            try
            {

                using (System.IO.StreamReader sr = new System.IO.StreamReader(stream, Encoding.Unicode))
                {
                    stream = null;
                    while (!sr.EndOfStream)
                    {
                        //内容を読み込む
                        string[] s = sr.ReadLine().Split(new string[] { "=" }, StringSplitOptions.None);
                        if (s == null || s.Length != 2) continue;
                        if (s[0].Trim() == "" || s[0].Trim().Length < 1 || s[0].Trim()[0] == '\'') continue;
                        string[] val = s[1].Split(new string[] { "," }, StringSplitOptions.None);
                        s[0] = s[0].ToUpper().Trim();

                        if (!dic.ContainsKey(s[0]))
                        {
                            List<double> value = new List<double>();
                            dic.Add(s[0], value);
                        }

                        foreach (string v in val)
                        {
                            string vv = v.Trim();

                            if (vv[0] == '$' && vv.Length > 1)
                            {
                                int num16 = Convert.ToInt32(vv.Substring(1), 16);
                                dic[s[0]].Add(num16);
                            }
                            else
                            {
                                if (double.TryParse(vv, out double o))
                                {
                                    dic[s[0]].Add(o);
                                }
                            }
                        }

                    }
                }
            }
            catch
            {
                dic = null;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }

            }

            return dic;
        }

        public bool ChannelNameContains(string name)
        {
            foreach (ClsChannel c in Ch)
            {
                if (c.Name == name) return true;
            }
            return false;
        }

        public void SetPartToCh(ClsChannel[] Ch, string val)
        {
            if (val == null || (val.Length != 1 && val.Length != 2)) return;

            string f = val[0].ToString();
            string r = (val.Length == 2) ? val[1].ToString() : " ";

            for (int i = 0; i < Ch.Length; i++)
            {
                if (Ch[i] == null) Ch[i] = new ClsChannel();
                Ch[i].Name = string.Format("{0}{1}{2:00}", f, r, i + 1);
            }

            //checkDuplication(fCh);
        }

        public int SetEnvelopParamFromInstrument(partWork pw, int n, MML mml)
        {
            if (!parent.instSSG.ContainsKey(n))
            {
                msgBox.setErrMsg(string.Format(msg.get("E10000"), n)
                    , mml.line.Fn
                    , mml.line.Num);
            }
            else
            {
                //Set Envelope
                pw.envInstrument = n;
                pw.envIndex = -1;
                pw.envCounter = -1;
                for (int i = 0; i < parent.instSSG[n].E.Length; i++)
                {
                    pw.envelope[i] = parent.instSSG[n].E[i];
                }

                pw.envelopeMode = true;
            }

            return n;
        }

        public void SetLFOParamFromInstrument(partWork pw, int n, MML mml)
        {
            if (!parent.instSSG.ContainsKey(n))
            {
                msgBox.setErrMsg(string.Format(msg.get("E10000"), n)
                    , mml.line.Fn
                    , mml.line.Num);
                return;
            }

            if (parent.instSSG[n].M == null || parent.instSSG[n].M.Length < 1) return;

            pw.lfo[0].type = enmLfoType.Vibrato;
            pw.lfo[0].sw = true;
            for (int i = 0; i < Math.Min(parent.instSSG[n].M.Length, 4); i++)
                pw.lfo[0].param[i] = parent.instSSG[n].M[i];

        }

        private int AnalyzeBend(partWork pw, Note note, int ml)
        {
            int n = -1;
            int bendDelayCounter;
            pw.octaveNow = note.octave;// pw.octaveNew;
            pw.bendOctave = note.bendOctave;// pw.octaveNow;
            pw.bendNote = note.bendCmd;
            pw.bendShift = note.bendShift;
            pw.bendWaitCounter = -1;
            bendDelayCounter = 0;//TODO: bendDelay

                        n = note.bendOctave;
                        n = Common.CheckRange(n, 1, 8);
                        pw.bendOctave = n;

            //音符の変化量
            int ed = Const.NOTE.IndexOf(pw.bendNote) + 1 + (pw.bendOctave - 1) * 12 + pw.bendShift;// + pw.keyShift+pw.relKeyShift;// pw.bendShift;
            ed = Common.CheckRange(ed, 0, 8 * 12 - 1);
            int st = Const.NOTE.IndexOf(note.cmd) + 1 + (pw.octaveNow - 1) * 12 + note.shift;// + pw.keyShift + pw.relKeyShift;// note.shift;//
            st = Common.CheckRange(st, 0, 8 * 12 - 1);

            int delta = ed - st;
            if (delta == 0 || bendDelayCounter == ml)
            {
                pw.bendNote = 'r';
                pw.bendWaitCounter = -1;
            }
            else
            {
                //１音符当たりのウエイト
                float wait = (ml - bendDelayCounter - 1) / (float)delta;
                float tl = 0;
                float bf = Math.Sign(wait);
                List<int> lstBend = new List<int>();
                int toneDoublerShift = GetToneDoublerShift(
                    pw
                    , pw.octaveNow
                    , note.cmd
                    , note.shift);
                for (int i = 0; i < Math.Abs(delta); i++)
                {
                    bf += wait;
                    tl += wait;
                    GetFNumAtoB(
                        pw
                        , out int a
                        , pw.octaveNow
                        , note.cmd
                        //, note.shift + (i + 0) * Math.Sign(delta)
                        //, pw.keyShift + pw.relKeyShift + (i + 0) * Math.Sign(delta)
                        , (i + 0) * Math.Sign(delta)
                        , out int b
                        , pw.octaveNow
                        , note.cmd
                        //, note.shift + (i + 1) * Math.Sign(delta)
                        //, pw.keyShift + pw.relKeyShift + (i + 1) * Math.Sign(delta)
                        , (i + 1) * Math.Sign(delta)
                        , delta
                        );

                    if (Math.Abs(bf) >= 1.0f)
                    {
                        for (int j = 0; j < (int)Math.Abs(bf); j++)
                        {
                            int c = b - a;
                            int d = (int)Math.Abs(bf);
                            lstBend.Add((int)(a + ((float)c / (float)d) * (float)j));
                        }
                        bf -= (int)bf;
                    }

                }
                Stack<Tuple<int, int>> lb = new Stack<Tuple<int, int>>();
                int of = -1;
                int cnt = 1;
                foreach (int f in lstBend)
                {
                    if (of == f)
                    {
                        cnt++;
                        continue;
                    }
                    lb.Push(new Tuple<int, int>(f, cnt));
                    of = f;
                    cnt = 1;
                }
                pw.bendList = new Stack<Tuple<int, int>>();
                foreach (Tuple<int, int> lbt in lb)
                {
                    pw.bendList.Push(lbt);
                }
                Tuple<int, int> t = pw.bendList.Pop();
                pw.bendFnum = t.Item1;
                pw.bendWaitCounter = parent.GetWaitCounter(t.Item2);
            }

            return bendDelayCounter;
        }

        private bool CheckLFOParam(partWork pw, int c, MML mml)
        {
            if (pw.lfo[c].param == null)
            {
                msgBox.setErrMsg(msg.get("E10001")
                    , mml.line.Fn
                    , mml.line.Num);
                return false;
            }

            return true;
        }



        public virtual void InitChip()
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual void InitPart(ref partWork pw)
        {
            throw new NotImplementedException("継承先で要実装");
        }


        public bool CanUsePICommand()
        {
            return CanUsePI;
        }

        public virtual void StorePcm(Dictionary<int, clsPcm> newDic, KeyValuePair<int, clsPcm> v, byte[] buf, bool is16bit, int samplerate, params object[] option)
        {
            pcmDataInfo = null;
        }

        public virtual void StorePcmRawData(clsPcmDatSeq pds, byte[] buf, bool isRaw, bool is16bit, int samplerate, params object[] option)
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual bool StorePcmCheck()
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual void SetPCMDataBlock()
        {
            if (!CanUsePcm) return;
            if (!use) return;

            int maxSize = 0;
            if (pcmDataEasy != null && pcmDataEasy.Length > 0)
            {
                maxSize =
                    pcmDataEasy[7]
                    + (pcmDataEasy[8] << 8)
                    + (pcmDataEasy[9] << 16)
                    + (pcmDataEasy[10] << 24);
            }
            if (pcmDataDirect.Count > 0)
            {
                foreach (byte[] dat in pcmDataDirect)
                {
                    if (dat != null && dat.Length > 0)
                    {
                        int size =
                            dat[7]
                            + (dat[8] << 8)
                            + (dat[9] << 16)
                            + (dat[10] << 24);
                        if (maxSize < size) maxSize = size;
                    }
                }
            }
            if (pcmDataEasy != null && pcmDataEasy.Length > 0)
            {
                pcmDataEasy[7] = (byte)maxSize;
                pcmDataEasy[8] = (byte)(maxSize >> 8);
                pcmDataEasy[9] = (byte)(maxSize >> 16);
                pcmDataEasy[10] = (byte)(maxSize >> 24);
            }
            if (pcmDataDirect.Count > 0)
            {
                foreach (byte[] dat in pcmDataDirect)
                {
                    if (dat != null && dat.Length > 0)
                    {
                        dat[7] = (byte)maxSize;
                        dat[8] = (byte)(maxSize >> 8);
                        dat[9] = (byte)(maxSize >> 16);
                        dat[10] = (byte)(maxSize >> 24);
                    }
                }
            }

            if (pcmDataEasy != null && pcmDataEasy.Length > 0)
                parent.OutData(pcmDataEasy);

            if (pcmDataDirect.Count < 1) return;

            foreach (byte[] dat in pcmDataDirect)
            {
                if (dat != null && dat.Length > 0)
                    parent.OutData(dat);
            }
        }


        public virtual int GetToneDoublerShift(partWork pw, int octave, char noteCmd, int shift)
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual void SetToneDoubler(partWork pw)
        {
            throw new NotImplementedException("継承先で要実装");
        }


        public virtual int GetFNum(partWork pw, int octave, char cmd, int shift)
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual void GetFNumAtoB(partWork pw
            , out int a, int aOctaveNow, char aCmd, int aShift
            , out int b, int bOctaveNow, char bCmd, int bShift
            , int dir)
        {
            a = GetFNum(pw, aOctaveNow, aCmd, aShift);
            b = GetFNum(pw, bOctaveNow, bCmd, bShift);
        }

        public virtual void SetFNum(partWork pw)
        {
            throw new NotImplementedException("継承先で要実装");
        }


        public virtual void SetKeyOn(partWork pw)
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual void SetKeyOff(partWork pw)
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual void SetVolume(partWork pw)
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual void SetLfoAtKeyOn(partWork pw)
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual void SetEnvelopeAtKeyOn(partWork pw)
        {
            if (!pw.envelopeMode)
            {
                pw.envVolume = 0;
                pw.envIndex = -1;
                return;
            }

            pw.envIndex = 0;
            pw.envCounter = pw.envelope[0];
            int maxValue = pw.MaxVolume;
            //while (pw.envCounter == 0 && pw.envIndex != -1)
            //{
            //    switch (pw.envIndex)
            //    {
            //        case 0: // AR phase
            //            pw.envCounter = pw.envelope[2];
            //            if (pw.envelope[2] > 0 && pw.envelope[1] < maxValue)
            //            {
            //                pw.envVolume = pw.envelope[1];
            //            }
            //            else
            //            {
            //                pw.envVolume = maxValue;
            //                pw.envIndex++;
            //            }
            //            break;
            //        case 1: // DR phase
            //            pw.envCounter = pw.envelope[3];
            //            if (pw.envelope[3] > 0 && pw.envelope[4] < maxValue)
            //            {
            //                pw.envVolume = maxValue;
            //            }
            //            else
            //            {
            //                pw.envVolume = pw.envelope[4];
            //                pw.envIndex++;
            //            }
            //            break;
            //        case 2: // SR phase
            //            pw.envCounter = pw.envelope[5];
            //            if (pw.envelope[5] > 0 && pw.envelope[4] != 0)
            //            {
            //                pw.envVolume = pw.envelope[4];
            //            }
            //            else
            //            {
            //                pw.envVolume = 0;
            //                pw.envIndex = -1;
            //            }
            //            break;
            //    }
            //}
        }



        public virtual void CmdY(partWork pw, MML mml)
        {
            throw new NotImplementedException("継承先で要実装");
        }

        public virtual void CmdClock(partWork pw, MML mml)
        {
            pw.clock = (int)mml.args[0];
        }

        public virtual void CmdTimerB(partWork pw, MML mml)
        {
            int timerB = (int)mml.args[0];
            timerB = Common.CheckRange(timerB, 1, 255);
            parent.info.timerB = (int)timerB;

        }

        public virtual void CmdTempo(partWork pw, MML mml)
        {
            //pw.samplesPerClock = Information.VGM_SAMPLE_PER_SECOND * 60.0*4.0 / (parent.info.tempo * pw.clock);  //VGM
            int tempo = (int)mml.args[0];
            parent.info.tempo = Common.CheckRange(tempo, 1, 255);
            //1152.0          : TimerBDelta(From OPNA application manual)
            //7987200.0 / 2.0 : MasterClock / Div
            //60.0            : TEMPO係数(60秒間の割合)
            // 4.0            : Clock係数(4拍分の割合)
            //256             : byte最大値+1
            parent.info.timerB = 256 - (int)(60.0 * 4.0 / ( 1152.0 / (7987200.0 / 2.0) ) / (parent.info.tempo * pw.clock) );
        }

        public virtual void CmdKeyShift(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            pw.keyShift = Common.CheckRange(n, -128, 128);
        }

        public virtual void CmdRelKeyShift(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            pw.relKeyShift = Common.CheckRange(n, -128, 128);
        }

        public virtual void CmdNoise(partWork pw, MML mml)
        {
            msgBox.setErrMsg(msg.get("E10002")
                    , mml.line.Fn
                    , mml.line.Num
                    );
        }

        public virtual void CmdSusOnOff(partWork pw, MML mml)
        {
            msgBox.setErrMsg(msg.get("E10022")
                    , mml.line.Fn
                    , mml.line.Num
                    );
        }


        public virtual void CmdMPMS(partWork pw, MML mml)
        {
            msgBox.setErrMsg(msg.get("E10003")
                    , mml.line.Fn
                    , mml.line.Num
                    );
        }

        public virtual void CmdMAMS(partWork pw, MML mml)
        {
            msgBox.setErrMsg(msg.get("E10004")
                    , mml.line.Fn
                    , mml.line.Num
                    );
        }

        public virtual void CmdSoftLfo(partWork pw,MML mml)
        {
            for (int i = 0; i < Math.Min(mml.args.Count, 4); i++) pw.lfo[0].param[i] = (int)mml.args[i];
            pw.lfo[0].type = enmLfoType.Vibrato;
            pw.lfo[0].sw = true;
            pw.lfo[0].waitCounter = parent.GetWaitCounter(pw.lfo[0].param[0]);
            pw.lfo[0].direction = Math.Sign(pw.lfo[0].param[2]);
            if (pw.lfo[0].direction == 0) pw.lfo[0].direction = 1;
            pw.lfo[0].value = 0;
            pw.lfo[0].PeakLevelCounter = pw.lfo[0].param[3] >> 1;
        }

        public virtual void CmdSoftLfoOnOff(partWork pw, MML mml)
        {
            if ((int)mml.args[0] == 0)
            {
                pw.lfo[0].sw = false;
            }
            else
            {
                pw.lfo[0].sw = true;
            }
        }

        public virtual void CmdSoftLfoDelay(partWork pw, MML mml)
        {
        }

        public virtual void CmdSoftLfoClock(partWork pw, MML mml)
        {
        }

        public virtual void CmdSoftLfoDepth(partWork pw, MML mml)
        {
        }

        public virtual void CmdSoftLfoLength(partWork pw, MML mml)
        {
        }

        public virtual void CmdLfo(partWork pw, MML mml)
        {
        //    if (mml.args[0] is string)
        //    {
        //        if ((string)mml.args[0] == "MAMS")
        //        {
        //            CmdMAMS(pw, mml);
        //            return;
        //        }
        //        if ((string)mml.args[0] == "MPMS")
        //        {
        //            CmdMPMS(pw, mml);
        //            return;
        //        }
        //    }

        //    int c = (char)mml.args[0] - 'P';
        //    eLfoType t = (char)mml.args[1] == 'T' ? eLfoType.Tremolo
        //        : ((char)mml.args[1] == 'V' ? eLfoType.Vibrato : eLfoType.Hardware);

        //    pw.lfo[c].type = t;
        //    pw.lfo[c].sw = false;
        //    pw.lfo[c].isEnd = true;
        //    pw.lfo[c].param = new List<int>();
        //    for (int i = 2; i < mml.args.Count; i++) pw.lfo[c].param.Add((int)mml.args[i]);

        //    if (pw.lfo[c].type == eLfoType.Tremolo || pw.lfo[c].type == eLfoType.Vibrato)
        //    {
        //        if (pw.lfo[c].param.Count < 4)
        //        {
        //            msgBox.setErrMsg(msg.get("E10005")
        //            , mml.line.Fn
        //            , mml.line.Num
        //            );
        //            return;
        //        }
        //        if (pw.lfo[c].param.Count > 7)
        //        {
        //            msgBox.setErrMsg(msg.get("E10006")
        //            , mml.line.Fn
        //            , mml.line.Num
        //            );
        //            return;
        //        }

        //        pw.lfo[c].param[0] = Common.CheckRange(pw.lfo[c].param[0], 0, (int)pw.clock);
        //        pw.lfo[c].param[1] = Common.CheckRange(pw.lfo[c].param[1], 1, 255);
        //        pw.lfo[c].param[2] = Common.CheckRange(pw.lfo[c].param[2], -32768, 32787);
        //        if (pw.lfo[c].param.Count > 4)
        //        {
        //            pw.lfo[c].param[4] = Common.CheckRange(pw.lfo[c].param[4], 0, 4);
        //        }
        //        else
        //        {
        //            pw.lfo[c].param.Add(0);
        //        }

        //        if (pw.lfo[c].param[4] != 2) pw.lfo[c].param[3] = Math.Abs(Common.CheckRange(pw.lfo[c].param[3], 0, 32787));
        //        else pw.lfo[c].param[3] = Common.CheckRange(pw.lfo[c].param[3], -32768, 32787);

        //        if (pw.lfo[c].param.Count > 5)
        //        {
        //            pw.lfo[c].param[5] = Common.CheckRange(pw.lfo[c].param[5], 0, 1);
        //        }
        //        else
        //        {
        //            pw.lfo[c].param.Add(1);
        //        }
        //        if (pw.lfo[c].param.Count > 6)
        //        {
        //            pw.lfo[c].param[6] = Common.CheckRange(pw.lfo[c].param[6], -32768, 32787);
        //            //if (pw.lfo[c].param[6] == 0) pw.lfo[c].param[6] = 1;
        //        }
        //        else
        //        {
        //            pw.lfo[c].param.Add(0);
        //        }

        //        pw.lfo[c].sw = true;
        //        pw.lfo[c].isEnd = false;
        //        pw.lfo[c].value = (pw.lfo[c].param[0] == 0) ? pw.lfo[c].param[6] : 0;//ディレイ中は振幅補正は適用されない
        //        pw.lfo[c].waitCounter = pw.lfo[c].param[0];
        //        pw.lfo[c].direction = pw.lfo[c].param[2] < 0 ? -1 : 1;
        //        if (pw.lfo[c].param[4] == 2) pw.lfo[c].direction = -1; //矩形の場合は必ず-1(Val1から開始する)をセット
        //    }
        //    else
        //    {
        //        pw.lfo[c].sw = true;
        //        pw.lfo[c].isEnd = false;
        //        pw.lfo[c].value = 0;
        //        pw.lfo[c].waitCounter = -1;
        //        pw.lfo[c].direction = 0;
        //    }
        }

        public virtual void CmdLfoSwitch(partWork pw, MML mml)
        {
            //int c = (char)mml.args[0] - 'P';
            //int n = (int)mml.args[1];

            ////LFOの設定値をチェック
            //if (n != 0 && !CheckLFOParam(pw, (int)c, mml))
            //{
            //    return;
            //}

            //pw.lfo[c].sw = !(n == 0);

        }

        public virtual void CmdHardLfo(partWork pw, MML mml)
        {
        }


        public virtual void CmdEnvelope(partWork pw, MML mml)
        {
            pw.envInstrument = -1;
            pw.envIndex = -1;
            pw.envCounter = -1;
            for (int i = 0; i < mml.args.Count; i++)
            {
                pw.envelope[i] = (int)mml.args[i];
            }

            pw.envelopeMode = true;

            //if (!(mml.args[0] is string))
            //{
            //    msgBox.setErrMsg(msg.get("E10010")
            //        , mml.line.Fn
            //        , mml.line.Num);

            //    return;
            //}

            //string cmd = (string)mml.args[0];

            //switch (cmd)
            //{
            //    case "EON":
            //        pw.envelopeMode = true;
            //        break;
            //    case "EOF":
            //        pw.envelopeMode = false;
            //        if (pw.Type == enmChannelType.SSG)
            //        {
            //            pw.beforeVolume = -1;
            //        }
            //        break;
            //}
            //return;
        }

        public virtual void CmdHardEnvelope(partWork pw, MML mml)
        {
            msgBox.setWrnMsg(msg.get("E10011")
                    , mml.line.Fn
                    , mml.line.Num
                    );
        }


        public virtual void CmdTotalVolume(partWork pw, MML mml)
        {
            msgBox.setErrMsg(msg.get("E10007")
                    , mml.line.Fn
                    , mml.line.Num
                    );
        }

        public virtual void CmdVolume(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            pw.volume = Common.CheckRange(n, 0, pw.MaxVolume);
            SetVolume(pw);
        }

        public virtual void CmdVolumeUp(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 1, pw.MaxVolume);
            pw.volume += n;
            //n = Common.CheckRange(n, 0, pw.MaxVolume);
            pw.volume = Common.CheckRange(pw.volume, 0, pw.MaxVolume);
            SetVolume(pw);
        }

        public virtual void CmdVolumeDown(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 1, pw.MaxVolume);
            pw.volume -= n;
            pw.volume = Common.CheckRange(pw.volume, 0, pw.MaxVolume);
            SetVolume(pw);
        }

        public virtual void CmdRelativeVolume(partWork pw,MML mml)
        {
            //なにもしない(コンパイル時評価のため)
        }

        public virtual void CmdOctave(partWork pw, MML mml)
        {
            //なにもしない(コンパイル時評価のため)
            //int n = (int)mml.args[0];
            //n = Common.CheckRange(n, 1, 8);
            //pw.octaveNew = n;
        }

        public virtual void CmdOctaveUp(partWork pw, MML mml)
        {
            //なにもしない(コンパイル時評価のため)
            //pw.octaveNew += parent.info.octaveRev ? -1 : 1;
            //pw.octaveNew = Common.CheckRange(pw.octaveNew, 1, 8);
        }

        public virtual void CmdOctaveDown(partWork pw, MML mml)
        {
            //なにもしない(コンパイル時評価のため)
            //pw.octaveNew += parent.info.octaveRev ? 1 : -1;
            //pw.octaveNew = Common.CheckRange(pw.octaveNew, 1, 8);
        }


        public virtual void CmdLength(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 1, 65535);
            pw.length = n;
        }

        public virtual void CmdClockLength(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 1, 65535);
            pw.length = n;
        }


        public virtual void CmdPan(partWork pw, MML mml)
        {
            msgBox.setErrMsg(msg.get("E10008")
                    , mml.line.Fn
                    , mml.line.Num
                    );
        }


        public virtual void CmdDetune(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            //n = Common.CheckRange(n, -127, 127);
            if (mml.args.Count > 1 && (string)mml.args[1]=="+")
            {
                //相対指定
                pw.detune += n;
            }
            else
            {
                pw.detune = n;
            }
        }


        public virtual void CmdGatetime(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 0, 255);
            pw.gatetime = n;
            pw.gatetimePmode = false;
        }

        public virtual void CmdGatetime2(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            n = Common.CheckRange(n, 1, 8);
            pw.gatetime = n;
            pw.gatetimePmode = true;
        }


        public virtual void CmdMode(partWork pw, MML mml)
        {
            msgBox.setErrMsg(msg.get("E10009")
                    , mml.line.Fn
                    , mml.line.Num
                    );
        }

        public virtual void CmdPcmMapSw(partWork pw, MML mml)
        {
            msgBox.setErrMsg(msg.get("E10023")
                    , mml.line.Fn
                    , mml.line.Num);
        }

        public virtual void CmdNoiseToneMixer(partWork pw, MML mml)
        {
            msgBox.setErrMsg(msg.get("E10014")
                , mml.line.Fn
                , mml.line.Num);
        }


        public void CmdLoop(partWork pw, MML mml)
        {
            if (pw.chip.parent.isLoopEx)
            {
                pw.loopInfo.use = true;
                pw.loopInfo.clockPos = (long)parent.lClock;
                pw.loopInfo.mmlPos = pw.mmlPos;

                pw.incPos();
                if (pw.loopInfo.isLongMml && parent.unusePartEndCount == parent.loopUnusePartCount)
                {
                    //parent.loopKusabi = true;
                    //parent.loopKusabiOffset = (long)parent.dat.Count;
                    //parent.loopKusabiClock = (long)parent.lClock;
                    //parent.loopKusabiSamples = (long)parent.dSample;
                    //parent.loopKusabiXGM0x7ePtr = parent.OutDataLength();
                    parent.loopOffset = (long)parent.dat.Count;
                    parent.loopClock = (long)parent.lClock;
                    parent.loopSamples = (long)parent.dSample;
                    if (parent.info.format == enmFormat.XGM) parent.OutData(0x7e);
                }

                pw.reqFreqReset = true;
                pw.chip.CmdLoopExtProc(pw, mml);
                return;
            }

            pw.incPos();
            parent.loopOffset = (long)parent.dat.Count;
            parent.loopClock = (long)parent.lClock;
            parent.loopSamples = (long)parent.dSample;
            if (parent.info.format == enmFormat.XGM) parent.OutData(0x7e);

            foreach (KeyValuePair<enmChipType, ClsChip[]> kvp in parent.chips)
            {
                foreach (ClsChip chip in kvp.Value)
                {
                    if (!chip.use) continue;

                    foreach (partWork p in chip.lstPartWork)
                    {
                        p.reqFreqReset = true;
                        //p.beforeLVolume = -1;
                        //p.beforeRVolume = -1;
                        //p.beforeVolume = -1;
                        //p.pan = new dint(3);
                        //p.beforeTie = false;

                        chip.CmdLoopExtProc(p, mml);
                    }
                }
            }
        }

        public virtual void CmdLoopExtProc(partWork pw, MML mml)
        {
            throw new NotImplementedException("継承先で要実装");
        }


        public virtual void CmdInstrument(partWork pw, MML mml)
        {
            throw new NotImplementedException("継承先で要実装");
        }



        public virtual void CmdRenpuStart(partWork pw, MML mml)
        {
            List<int> lstRenpuLength = new List<int>();
            int noteCount = (int)mml.args[0];
            int len = (int)pw.length;
            if (mml.args.Count > 1)
            {
                int n = (int)mml.args[1];
                n = Common.CheckRange(n, 1, 65535);
                len = n;
            }
            if (pw.stackRenpu.Count > 0)
            {
                len = pw.stackRenpu.First().lstRenpuLength[0];
                pw.stackRenpu.First().lstRenpuLength.RemoveAt(0);
            }
            //TODO: ネストしている場合と、数値していないの場合

            //連符内の音符の長さを作成
            for (int p = 0; p < noteCount; p++)
            {
                int le = len / noteCount +
                    (
                      (len % noteCount) == 0
                      ? 0
                      : (
                          (len % noteCount) > p
                          ? 1
                          : 0
                        )
                    );

                lstRenpuLength.Add(le);
            }

            pw.renpuFlg = true;

            clsRenpu rp = new clsRenpu();
            rp.lstRenpuLength = lstRenpuLength;
            pw.stackRenpu.Push(rp);
        }

        public virtual void CmdRenpuEnd(partWork pw, MML mml)
        {
            //popしない内からスタックが空の場合は何もしない。
            if (pw.stackRenpu.Count == 0) return;

            pw.stackRenpu.Pop();

            if (pw.stackRenpu.Count == 0)
            {
                pw.renpuFlg = false;
            }

        }


        public virtual void CmdRepeatStart(partWork pw, MML mml)
        {
            //何もする必要なし
        }

        public virtual void CmdRepeatEnd(partWork pw, MML mml)
        {
            int count = (int)mml.args[0];
            int wkCount;
            int pos = (int)mml.args[1];
            if (mml.args.Count < 3)
            {
                wkCount = count;
                mml.args.Add(wkCount);
            }
            else
            {
                wkCount = (int)mml.args[2];
            }

            wkCount--;
            if (wkCount > 0)
            {
                pw.mmlPos = pos - 1;
                mml.args[2] = wkCount;
            }
            else
            {
                mml.args.RemoveAt(2);
            }
        }

        public virtual void CmdRepeatExit(partWork pw, MML mml)
        {
            int pos = (int)mml.args[0];
            MML repeatEnd = pw.mmlData[pos];
            int wkCount = (int)repeatEnd.args[0];
            if (repeatEnd.args.Count > 2)
            {
                wkCount = (int)repeatEnd.args[2];
            }

            //最終リピート中のみ]に飛ばす
            if (wkCount < 2)
            {
                pw.mmlPos = pos - 1;
            }
        }


        public virtual void CmdNote(partWork pw, MML mml)
        {
            Note note = (Note)mml.args[0];
            int ml = 0;

            if (note.tDblSw)
            {
                pw.TdA = pw.octaveNew * 12
                    + Const.NOTE.IndexOf(note.cmd)
                    + note.shift
                    //+ pw.keyShift + pw.relKeyShift
                    ;
                pw.octaveNow = pw.octaveNew;
            }

            ml = note.length;

            //ベンドの解析
            int bendDelayCounter = 0;
            if (note.bendSw)
            {
                bendDelayCounter = AnalyzeBend(pw, note, ml);
            }


            if (note.length < 1)
            {
                msgBox.setErrMsg(msg.get("E10013")
                    , mml.line.Fn
                    , mml.line.Num
                    );
                ml = (int)pw.length;
            }

            if (pw.renpuFlg)
            {
                if (pw.stackRenpu.Count > 0)
                {
                    ml = pw.stackRenpu.First().lstRenpuLength[0];
                    pw.stackRenpu.First().lstRenpuLength.RemoveAt(0);
                }
            }

            //WaitClockの決定
            pw.waitCounter = parent.GetWaitCounter(ml);

            if (pw.reqFreqReset)
            {
                pw.freq = -1;
                pw.reqFreqReset = false;
            }

            pw.octaveNow = note.octave;// pw.octaveNew;
            pw.noteCmd = note.cmd;
            pw.shift = note.shift;
            pw.tie = note.tieSw;

            //Tone Doubler
            SetToneDoubler(pw);

            //発音周波数
            if (pw.bendWaitCounter != -1)
            {
                pw.octaveNew = pw.bendOctave;//
                pw.octaveNow = pw.bendOctave;//
                pw.noteCmd = pw.bendNote;
                pw.shift = pw.bendShift;
            }

            //タイ指定では無い場合はキーオンする
            if (!pw.beforeTie)
            {
                if (pw.ReverbNowSwitch)
                {
                    SetKeyOff(pw);
                    pw.ReverbNowSwitch = false;
                }
                SetEnvelopeAtKeyOn(pw);
                SetLfoAtKeyOn(pw);
                if(pw.ReverbSwitch)
                    SetVolume(pw);
                //強制設定
                //pw.freq = -1;
                //発音周波数の決定
                SetFNum(pw);
                if (!pw.restMode)
                {
                    SetKeyOn(pw);
                }
            }
            else
            {
                //強制設定
                //pw.freq = -1;
                //発音周波数の決定
                SetFNum(pw);
                //if (pw.ReverbSwitch)
                    //SetVolume(pw);
            }

            //gateTimeの決定
            if (pw.gatetimePmode)
                pw.waitKeyOnCounter = pw.waitCounter * pw.gatetime / 8L;
            else
                pw.waitKeyOnCounter = pw.waitCounter - parent.GetWaitCounter(pw.gatetime);
            if (pw.waitKeyOnCounter < 1)
            {
                if ((pw.chip is YM2612)|| (pw.chip is YM2612X)) pw.waitKeyOnCounter = 1;
                else pw.waitKeyOnCounter = pw.waitCounter;
            }

            //pw.clockCounter += pw.waitCounter;
        }

        public virtual void CmdRest(partWork pw, MML mml)
        {
            Rest rest = (Rest)mml.args[0];
            int ml = 0;

            ml = rest.length;

            if (rest.length < 1)
            {
                msgBox.setErrMsg(msg.get("E10013")
                    , mml.line.Fn
                    , mml.line.Num
                    );
                ml = (int)pw.length;
            }

            //if ((pw.ReverbNowSwitch && pw.ReverbMode == 1) || pw.tie)
            if (pw.ReverbNowSwitch || pw.tie)
            {
                pw.ReverbNowSwitch = false;
                pw.beforeTie = false;
                pw.tie = false;
                //SetKeyOff(pw);
            }

            SetKeyOff(pw);

            //WaitClockの決定
            pw.waitCounter = parent.GetWaitCounter(ml); 

            //pw.octaveNow = pw.octaveNew;
            //pw.noteCmd = rest.cmd;
            //pw.shift = 0;

            //pw.clockCounter += pw.waitCounter;
        }

        public virtual void CmdLyric(partWork pw, MML mml)
        {
            string str = (string)mml.args[0];
            int ml = (int)mml.args[1];

            if (ml < 1)
            {
                msgBox.setErrMsg(msg.get("E10013")
                    , mml.line.Fn
                    , mml.line.Num
                    );
                ml = (int)pw.length;
            }

            str = string.Format("[{0}]{1}", parent.dSample.ToString(), str);
            parent.lyric += str;
            //WaitClockの決定
            pw.waitCounter = parent.GetWaitCounter(ml); 
            pw.tie = false;

            //pw.clockCounter += pw.waitCounter;
        }

        public virtual void CmdBend(partWork pw, MML mml)
        {
            //何もする必要なし
        }

        public virtual void CmdReverb(partWork pw, MML mml)
        {
            int val = (int)mml.args[0];
            pw.ReverbValue = val;
            pw.ReverbSwitch = true;
        }

        public virtual void CmdReverbONOF(partWork pw, MML mml)
        {
            int val = (int)mml.args[0];
            pw.ReverbSwitch = (val == 1);
            pw.ReverbNowSwitch = false;
            SetKeyOff(pw);
            SetVolume(pw);
        }

        public virtual void CmdReverbMode(partWork pw, MML mml)
        {
            int val = (int)mml.args[0];
            pw.ReverbMode = val;
        }

        public virtual void CmdSlotDetune(partWork pw, MML mml)
        {
        }

        public virtual void CmdExtendChannel(partWork pw, MML mml)
        {
            msgBox.setWrnMsg(msg.get("E10012")
                    , mml.line.Fn
                    , mml.line.Num
                    );
        }

        public virtual void MultiChannelCommand()
        { }


        public virtual string DispRegion(clsPcm pcm)
        {
            return "みじっそう";
        }

    }

    public class clsPcmDataInfo
    {
        public byte[] totalBuf;
        public long totalBufPtr;
        public bool use;
    }

    public class ClsChannel
    {
        public string Name;
        public enmChannelType Type;
        public bool isSecondary;
        public int MaxVolume;
    }

    public class Function
    {
        public int Max;
        public int Min;
    }

}
