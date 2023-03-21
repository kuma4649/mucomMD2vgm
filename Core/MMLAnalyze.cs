using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class MMLAnalyze
    {

        public Dictionary<string, List<MML>> mmlData = new Dictionary<string, List<MML>>();
        public int lineNumber = 0;

        private Dictionary<enmChipType, ClsChip[]> chips;
        private Information info;
        private ClsVgm desVGM;
        private List<MML> mmls;

        private clsEcho echoShare = new clsEcho();
       

        public MMLAnalyze(ClsVgm desVGM)
        {
            desVGM.PartInit();
            this.chips = desVGM.chips;
            this.info = desVGM.info;
            desVGM.useJumpCommand = 0;
            this.desVGM = desVGM;
            mmls = null;
        }

        public int Start()
        {
            foreach (KeyValuePair<enmChipType, ClsChip[]> kvp in chips)
            {
                foreach (ClsChip chip in kvp.Value)
                {
                    if (!chip.use) continue;
                    if (chip.chipType == enmChipType.YM2612X && info.format == enmFormat.VGM) continue;
                    if (chip.chipType == enmChipType.YM2612 && info.format == enmFormat.XGM) continue;
                    if (chip.chipType == enmChipType.YM3526 && !info.useOPL) continue;
                    if (chip.chipType == enmChipType.YM2151 && !info.useOPM) continue;

                    foreach (partWork pw in chip.lstPartWork)
                    {
                        if (pw.pData == null) continue;

                        Step1(pw);//mml全体のフォーマット解析
                        Step2(pw);//toneDoubler,bend,tieコマンドの解析
                        Step3(pw);//リピート、連符コマンドの解析

                        pw.dataEnd = false;
                    }
                }
            }
            return 0;

        }



        #region step1

        private void Step1(partWork pw)
        {
            pw.resetPos();
            pw.dataEnd = false;
            pw.mmlData = new List<MML>();

            while (!pw.dataEnd)
            {
                char cmd = pw.getChar();
                if (cmd == 0) pw.dataEnd = true;
                else
                {
                    lineNumber = pw.getLineNumber();
                    Commander(pw, cmd);
                }
            }
        }

        private bool swToneDoubler = false;

        private void Commander(partWork pw, char cmd)
        {
            MML mml = new MML();
            mml.line = pw.getLine();
            mml.column = pw.getPos();


            //コマンド解析

            switch (cmd)
            {
                case ' ':
                case '\t':
                    pw.incPos();
                    break;
                case 'C':
                    log.Write("Clock");
                    CmdClock(pw, mml);
                    break;
                case 't':
                    log.Write("TimerB");
                    CmdTimerB(pw, mml);
                    break;
                case 'T': // tempo
                    log.Write("Tempo");
                    CmdTempo(pw, mml);
                    break;
                case '@': // instrument
                    log.Write("instrument");
                    CmdInstrument(pw, mml);
                    break;
                case 'o': // octave
                    log.Write("octave");
                    CmdOctave(pw, mml);
                    break;
                case 'v': // volume
                    log.Write("volume");
                    CmdVolume(pw, mml);
                    break;
                case 'q': // gatetime
                    log.Write(" gatetime q");
                    CmdGatetime(pw, mml);
                    break;
                case 'p': // pan
                    log.Write(" pan");
                    CmdPan(pw, mml);
                    break;
                case 'l': // length
                    log.Write("length");
                    CmdLength(pw, mml);
                    break;
                case '%': // length(clock)
                    log.Write("length(clock)");
                    CmdClockLength(pw, mml);
                    break;
                case 'D': // Detune
                    log.Write("Detune");
                    CmdDetune(pw, mml);
                    break;
                case '>': // octave Up
                    log.Write("octave Up");
                    CmdOctaveUp(pw, mml);
                    break;
                case '<': // octave Down
                    log.Write("octave Down");
                    CmdOctaveDown(pw, mml);
                    break;
                case ')': // volume Up
                    log.Write(" volume Up");
                    CmdVolumeUp(pw, mml);
                    break;
                case '(': // volume Down
                    log.Write("volume Down");
                    CmdVolumeDown(pw, mml);
                    break;
                case '&':
                    log.Write("tie");
                    CmdTie(pw, mml);
                    break;
                case '^':
                    log.Write("tie plus clock");
                    CmdTiePC(pw, mml);
                    break;
                case '{':
                    log.Write("porta start");
                    CmdPortaStart(pw, mml);
                    break;
                case '}':
                    log.Write("porta start");
                    CmdPortaEnd(pw, mml);
                    break;
                case '[': // repeat
                    log.Write("repeat [");
                    CmdRepeatStart(pw, mml);
                    break;
                case ']': // repeat
                    log.Write("repeat ]");
                    CmdRepeatEnd(pw, mml);
                    break;
                case '/': // repeat
                    log.Write("repeat /");
                    CmdRepeatExit(pw, mml);
                    break;
                case 'L': // loop point
                    log.Write(" loop point");
                    CmdLoop(pw, mml);
                    break;
                case 'K': // key shift
                    log.Write("key shift");
                    CmdKeyShift(pw, mml);
                    break;
                case 'V': // Relative volume
                    log.Write("Relative volume");
                    CmdRelativeVolume(pw, mml);
                    break;
                case '\\': // Echo / EchoMacro
                    log.Write("Echo/EchoMacro");
                    CmdEchoMacro(pw, mml);
                    break;
                case 'k': // Relative key shift
                    log.Write("Relative key shift");
                    CmdRelativeKeyShift(pw, mml);
                    break;
                case 's': // Shuffle
                    log.Write("Shuffle");
                    CmdShuffle(pw, mml);
                    break;
                case 'H': // Hard lfo
                    log.Write("Hard lfo");
                    CmdHardLfo(pw, mml);
                    break;
                case 'R': // Reverb
                    log.Write("Reverb");
                    CmdReverb(pw, mml);
                    break;
                case 'M': // Soft Lfo
                    log.Write("Soft lfo");
                    CmdSoftLfo(pw, mml);
                    break;
                case 'S': // Slot Detune
                    log.Write("Slot Detune");
                    CmdSlotDetune(pw, mml);
                    break;
                case 'E': // envelope / extendChannel
                    log.Write("envelope / extendChannel");
                    CmdE(pw, mml);
                    break;
                case 'P': // MixerMode
                    log.Write("MixerMode");
                    CmdMixer(pw, mml);
                    break;
                case 'w': // noise
                    log.Write("Noise Freq");
                    CmdNoise(pw, mml);
                    break;
                //case 's': // SSG Hard Env
                //    log.Write("SSG Hard Env");
                //    CmdSlotDetune(pw, mml);
                //    break;
                case 'm': // mode
                    log.Write("Mode PCMMap");
                    CmdMode(pw, mml);
                    break;
                case 'y': // y
                    log.Write("y");
                    CmdY(pw, mml);
                    break;
                case '*': // macro
                    log.Write("macro");
                    CmdMacro(pw, mml);
                    break;
                case ';': // comment
                    log.Write("comment");
                    CmdComment(pw, mml);
                    break;
                case ':': // CompileSkip
                    log.Write("CompileSkip");
                    pw.dataEnd = true;
                    mml.type = enmMMLType.CompileSkip;
                    mml.args = null;
                    break;
                case '!': // fill Rest
                    log.Write("fill Rest");
                    CmdFillRest(pw, mml);
                    break;
                case 'J': // jump
                    log.Write("jump");
                    CmdJump(pw, mml);
                    break;
                case '|': // none
                    log.Write("none");
                    pw.incPos();
                    break;
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'a':
                case 'b':
                    log.Write(string.Format("note {0}", cmd));
                    CmdNote(pw, cmd, mml);
                    break;
                case 'r':
                    log.Write("rest");
                    CmdRest(pw, mml);
                    break;
                default:
                    msgBox.setErrMsg(string.Format(msg.get("E05000"), cmd), pw.getSrcFn(), pw.getLineNumber());
                    pw.incPos();
                    break;
            }



            //mmlコマンドの追加
            if (mml != null)
            {
                if (mml.type == enmMMLType.unknown) return;
                if (mml.type != enmMMLType.Echo)
                {
                    if (!mmlData.ContainsKey(pw.PartName))
                    {
                        mmlData.Add(pw.PartName, new List<MML>());
                    }

                    pw.mmlData.Add(mml);
                }
                else
                {
                    if (mmls != null)
                    {
                        foreach (MML m in mmls)
                        {
                            pw.mmlData.Add(m);
                        }
                        mmls = null;
                    }
                }
            }
            else
            {
            }
            if (swToneDoubler)
            {
                mml = new MML();
                mml.type = enmMMLType.ToneDoubler;
                mml.line = pw.getLine();
                mml.column = pw.getPos();
                pw.mmlData.Add(mml);
            }
        }

        private void CmdClock(partWork pw, MML mml)
        {
            pw.incPos();
            if (!pw.getNum(out int n))
            {
                msgBox.setErrMsg(msg.get("E05901"), pw.getSrcFn(), pw.getLineNumber());
                n = 120;
            }
            n = Common.CheckRange(n, 1, 255);
            pw.clock = n;
            mml.type = enmMMLType.Clock;
            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdTimerB(partWork pw, MML mml)
        {
            pw.incPos();
            if (!pw.getNum(out int n))
            {
                msgBox.setErrMsg(msg.get("E05902"), pw.getSrcFn(), pw.getLineNumber());
                n = 120;
            }
            n = Common.CheckRange(n, 1, 255);

            mml.type = enmMMLType.TimerB;
            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdTempo(partWork pw, MML mml)
        {
            pw.incPos();
            if (!pw.getNum(out int n))
            {
                msgBox.setErrMsg(msg.get("E05001"), pw.getSrcFn(), pw.getLineNumber());
                n = 120;
            }
            n = Common.CheckRange(n, 1, 1200);

            mml.type = enmMMLType.Tempo;
            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdInstrument(partWork pw, MML mml)
        {
            int n;
            pw.incPos();

            mml.type = enmMMLType.Instrument;
            mml.args = new List<object>();

            if (pw.getChar() == '"')
            {
                //名称指定
                mml.args.Add('"');
                string name=pw.getString();
                if (string.IsNullOrEmpty(name))
                {
                    msgBox.setErrMsg(msg.get("E05903"), pw.getSrcFn(), pw.getLineNumber());
                }
                mml.args.Add(name.PadRight(6));
            }
            else
            {
                //normal
                mml.args.Add('N');

                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(msg.get("E05002"), pw.getSrcFn(), pw.getLineNumber());
                    n = 0;
                }
                n = Common.CheckRange(n, 0, 255);
                mml.args.Add(n);
            }

            //音色グラデーション（モーフィング）機能向け解析
            pw.skipSpaceOrTab();
            if (pw.getChar() == ',')
            {
                pw.incPos();
                if (pw.getChar() == '"')
                {
                    //名称指定
                    mml.args.Add('"');
                    string name = pw.getString();
                    if (string.IsNullOrEmpty(name))
                    {
                        msgBox.setErrMsg(msg.get("E05903"), pw.getSrcFn(), pw.getLineNumber());
                    }
                    mml.args.Add(name.PadRight(6));//trg inst
                }
                else
                {
                    //normal
                    mml.args.Add('N');

                    if (!pw.getNum(out n))
                    {
                        msgBox.setErrMsg(msg.get("E05002"), pw.getSrcFn(), pw.getLineNumber());
                        n = 0;
                    }
                    n = Common.CheckRange(n, 0, 255);
                    mml.args.Add(n);//trg inst
                }

                pw.skipSpaceOrTab();
                if (pw.getChar() == ',')
                {
                    pw.incPos();
                    if (!pw.getNum(out n))
                    {
                        msgBox.setErrMsg(msg.get("E05002"), pw.getSrcFn(), pw.getLineNumber());
                        n = 0;
                    }
                    n = Common.CheckRange(n, 1, 255);//wait
                    mml.args.Add(n);

                    pw.skipSpaceOrTab();
                    if (pw.getChar() == ',')
                    {
                        pw.incPos();
                        if (!pw.getNum(out n))
                        {
                            msgBox.setErrMsg(msg.get("E05002"), pw.getSrcFn(), pw.getLineNumber());
                            n = 0;
                        }
                        n = Common.CheckRange(n, 0, 1);//reset sw
                        mml.args.Add(n);
                    }
                }
            }

        }

        private void CmdVolume(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            if (pw.getChar() == 'm')
            {
                pw.incPos();
                mml.type = enmMMLType.PCMVolumeMode;
                mml.args = new List<object>();

                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(string.Format(msg.get("E05912"),"vm"), pw.getSrcFn(), pw.getLineNumber());
                    n = 0;
                }
                mml.args.Add(n);
            }
            else
            {
                mml.type = enmMMLType.Volume;
                mml.args = new List<object>();

                if (pw.getNum(out n))
                {
                    //相対音量調整
                    n = n + pw.RelVolume;

                    n = Common.CheckRange(n, 0, pw.MaxVolumeEasy);
                    mml.args.Add(n);
                }
                else
                {
                    mml.args = null;
                }
            }
        }

        private void CmdTotalVolume(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            mml.type = enmMMLType.TotalVolume;
            mml.args = new List<object>();

            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05004"), pw.getSrcFn(), pw.getLineNumber());
                n = 0;
            }
            mml.args.Add(n);

            if (pw.getChar() == ',')
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(msg.get("E05004"), pw.getSrcFn(), pw.getLineNumber());
                    n = 0;
                }
                mml.args.Add(n);
            }

        }

        private void CmdOctave(partWork pw, MML mml)
        {
            pw.incPos();
            if (pw.getNum(out int n))
            {
                n = Common.CheckRange(n, 1, 8);

                mml.type = enmMMLType.Octave;
                mml.args = new List<object>();
                mml.args.Add(n);
                pw.octaveNow = n;
                pw.latestOctave = n;
            }
            else
            {
                mml.args = null;
                pw.octaveNow = pw.latestOctave;
            }
        }

        private void CmdOctaveUp(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.OctaveUp;
            mml.args = null;
            pw.octaveNow += info.octaveRev ? -1 : 1;
            pw.octaveNow = Common.CheckRange(pw.octaveNow, 1, 8);
        }

        private void CmdOctaveDown(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.OctaveDown;
            mml.args = null;
            pw.octaveNow += info.octaveRev ? 1 : -1;
            pw.octaveNow = Common.CheckRange(pw.octaveNow, 1, 8);
        }

        private void CmdVolumeUp(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            if (!pw.getNum(out n))
            {
                n = 1;
            }
            mml.type = enmMMLType.VolumeUp;
            mml.args = new List<object>();
            mml.args.Add(n);

        }

        private void CmdVolumeDown(partWork pw, MML mml)
        {
            pw.incPos();
            if (!pw.getNum(out int n))
            {
                n = 1;
            }
            mml.type = enmMMLType.VolumeDown;
            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdLength(partWork pw, MML mml)
        {
            pw.incPos();
            //数値の解析
            if (pw.getNumNoteLength(out int n, out bool directFlg))
            {
                if (!directFlg)
                {
                    if ((int)pw.clock % n != 0)
                    {
                        msgBox.setWrnMsg(string.Format(msg.get("E05008"), n), pw.getSrcFn(), pw.getLineNumber());
                    }
                    n = (int)pw.clock / n;
                }
                else
                {
                    n = Common.CheckRange(n, 1, 65535);
                }

                //.の解析
                int futen = 0;
                int fn = n;
                while (pw.getChar() == '.')
                {
                    if (fn % 2 != 0)
                    {
                        msgBox.setWrnMsg(msg.get("E05036")
                            , mml.line.Fn
                            , mml.line.Num);
                    }
                    fn = fn / 2;
                    futen += fn;
                    pw.incPos();
                }
                n += futen;

            }
            mml.type = enmMMLType.Length;
            mml.args = new List<object>();
            mml.args.Add(n);
            pw.length = n;
        }

        private void CmdClockLength(partWork pw, MML mml)
        {
            pw.incPos();
            if (!pw.getNum(out int n))
            {
                msgBox.setErrMsg(msg.get("E05009"), pw.getSrcFn(), pw.getLineNumber());
                n = 10;
            }
            n = Common.CheckRange(n, 1, 65535);
            mml.type = enmMMLType.LengthClock;
            mml.args = new List<object>();
            mml.args.Add(n);
            pw.length = n;
        }

        private void CmdPan(partWork pw, MML mml)
        {
            int n;

            pw.incPos();
            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05010"), pw.getSrcFn(), pw.getLineNumber());
            }
            mml.type = enmMMLType.Pan;
            mml.args = new List<object>();
            mml.args.Add(n);

            if (pw.getChar() == ',')
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(msg.get("E05010"), pw.getSrcFn(), pw.getLineNumber());
                }
                mml.args.Add(n);
            }
        }

        private void CmdDetune(partWork pw, MML mml)
        {
            int n;

            pw.incPos();
            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05011"), pw.getSrcFn(), pw.getLineNumber());
                n = 0;
            }
            mml.type = enmMMLType.Detune;
            mml.args = new List<object>();
            mml.args.Add(n);
            if (pw.getChar() == '+')
            {
                pw.incPos();
                mml.args.Add("+");//相対指定
            }
        }

        private void CmdMode(partWork pw, MML mml)
        {
            int n;
            pw.incPos();

            if (pw.getChar() == 'o')
            {
                //pcm mapMode ?
                pw.incPos();
                if (pw.getChar() == 'n')
                {
                    mml.type = enmMMLType.PcmMap;
                    mml.args = new List<object>();
                    mml.args.Add(true);
                    pw.incPos();
                }
                else if (pw.getChar() == 'f')
                {
                    mml.type = enmMMLType.PcmMap;
                    mml.args = new List<object>();
                    mml.args.Add(false);
                    pw.incPos();
                }
                else
                {
                    msgBox.setErrMsg(msg.get("E05055"), pw.getSrcFn(), pw.getLineNumber());
                }
            }
            else
            {
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(msg.get("E05012"), pw.getSrcFn(), pw.getLineNumber());
                    n = 0;
                }
                mml.type = enmMMLType.PcmMode;
                mml.args = new List<object>();
                mml.args.Add(n);
            }
        }

        private void CmdGatetime(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05013"), pw.getSrcFn(), pw.getLineNumber());
                n = 0;
            }
            n &= 0xff;// Common.CheckRange(n, 0, 255);
            mml.type = enmMMLType.Gatetime;
            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdGatetime2(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05014"), pw.getSrcFn(), pw.getLineNumber());
                n = 1;
            }
            n = Common.CheckRange(n, 1, 8);
            mml.type = enmMMLType.GatetimeDiv;
            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdE(partWork pw, MML mml)
        {

            pw.incPos();
            int n = -1;

            //効果音モード関連コマンド解析

            if (pw.getChar() == 'X')
            {
                pw.incPos();

                if (pw.getChar() == 'O')
                {
                    pw.incPos();
                    switch (pw.getChar())
                    {
                        case 'N':
                            pw.incPos();
                            mml.type = enmMMLType.ExtendChannel;
                            mml.args = new List<object>();
                            mml.args.Add("EXON");
                            break;
                        case 'F':
                            pw.incPos();
                            mml.type = enmMMLType.ExtendChannel;
                            mml.args = new List<object>();
                            mml.args.Add("EXOF");
                            break;
                        default:
                            msgBox.setErrMsg(string.Format(msg.get("E05019"), pw.getChar()), pw.getSrcFn(), pw.getLineNumber());
                            break;
                    }
                    return;
                }

                if (pw.getChar() == 'M')
                {
                    pw.incPos();
                    if (!pw.getNum(out n))
                    {
                        n = 1234;
                    }
                    mml.type = enmMMLType.ExtendChannel;
                    mml.args = new List<object>();
                    mml.args.Add("EXM");
                    mml.args.Add(n);
                    return;
                }

                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(msg.get("E05021"), pw.getSrcFn(), pw.getLineNumber());
                    n = 0;
                }
                if (n != -1)
                {
                    mml.type = enmMMLType.ExtendChannel;
                    mml.args = new List<object>();
                    mml.args.Add("EX");
                    mml.args.Add(n);
                }

                return;
            }


            //envelopeコマンド解析

            mml.type = enmMMLType.Envelope;
            mml.args = new List<object>();

            while (true)
            {
                if (pw.getNum(out n))
                {
                    mml.args.Add(n);
                }
                else
                {
                    msgBox.setErrMsg(msg.get("E05022"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }

                if (pw.getChar() != ',')
                {
                    break;
                }
                pw.incPos();
            }

            //do
            //{
            //    if (pw.getNum(out n))
            //    {
            //        mml.args.Add(n);
            //    }
            //    else
            //    {
            //        msgBox.setErrMsg(msg.get("E05022"), pw.getSrcFn(), pw.getLineNumber());
            //        break;
            //    }

            //    pw.incPos();
            //} while (pw.getChar() == ',');
        }

        private void CmdLoop(partWork pw, MML mml)
        {
            pw.incPos();
            if (desVGM.isLoopEx) {
                mml.type = enmMMLType.LoopPoint;
                mml.args = null;
            }
            else if(pw.Type == enmChannelType.FMOPN)
            {
                if (pw.ch == 0)
                {
                    mml.type = enmMMLType.LoopPoint;
                    mml.args = null;
                }
                else
                {
                    msgBox.setErrMsg(msg.get("E05054"));
                }
            }
            else
            {
                mml = null;
            }
        }

        private void CmdPortaStart(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.Porta;
            mml.args = null;
            //ポルタメント有効範囲開始
            pw.mPortaSW = true;
        }

        private void CmdPortaEnd(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.PortaEnd;
            mml.args = null;
            //ポルタメント有効範囲終了
            pw.mPortaSW = false;
            pw.echo_PortaCounter = 0;
        }

        private void CmdRepeatStart(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.Repeat;
            mml.args = null;
        }

        private void CmdRepeatEnd(partWork pw, MML mml)
        {
            pw.incPos();
            if (!pw.getNum(out int n))
            {
                n = 2;
            }
            n = Common.CheckRange(n, 1, 255);
            mml.type = enmMMLType.RepeatEnd;
            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdRenpuStart(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.Renpu;
            mml.args = null;
        }

        private void CmdRenpuEnd(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.RenpuEnd;
            mml.args = null;
            if (pw.getNumNoteLength(out int n, out bool directFlg))
            {
                if (!directFlg)
                {
                    if ((int)pw.clock % n != 0)
                    {
                        msgBox.setWrnMsg(string.Format(msg.get("E05023"), n), pw.getSrcFn(), pw.getLineNumber());
                    }
                    n = (int)pw.clock / n;
                }
                else
                {
                    n = Common.CheckRange(n, 1, 65535);
                }

                mml.args = new List<object>();
                mml.args.Add(n);
            }
        }

        private void CmdRepeatExit(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.RepertExit;
            mml.args = null;
        }

        private void CmdHardLfo(partWork pw,MML mml)
        {
            int n;
            mml.type = enmMMLType.HardLfo;
            mml.args = new List<object>();
            do
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(msg.get("E05908"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.args.Add(n);
            } while (pw.getChar() == ',');
        }

        private void CmdReverb(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            if (pw.getChar() == 'F')
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(msg.get("E05910"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.type = enmMMLType.ReverbONOF;
                mml.args = new List<object>();
                mml.args.Add(n);
            }
            else if (pw.getChar() == 'm')
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(msg.get("E05911"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.type = enmMMLType.ReverbMode;
                mml.args = new List<object>();
                mml.args.Add(n);
            }
            else
            {
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(msg.get("E05909"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.type = enmMMLType.Reverb;
                mml.args = new List<object>();
                mml.args.Add(n);
            }
        }

        private void CmdSoftLfo(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            char c = pw.getChar();
            if (c == 'F')
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(string.Format(msg.get("E05912"), "MF"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.type = enmMMLType.SoftLfoOnOff;
                mml.args = new List<object>();
                mml.args.Add(n);
            }
            else if (c == 'W')
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(string.Format(msg.get("E05912"), "MW"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.type = enmMMLType.SoftLfoDelay;
                mml.args = new List<object>();
                mml.args.Add(n);
            }
            else if (c == 'C')
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(string.Format(msg.get("E05912"), "MC"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.type = enmMMLType.SoftLfoClock;
                mml.args = new List<object>();
                mml.args.Add(n);
            }
            else if (c == 'L')
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(string.Format(msg.get("E05912"), "ML"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.type = enmMMLType.SoftLfoLength;
                mml.args = new List<object>();
                mml.args.Add(n);
            }
            else if (c == 'D')
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(string.Format(msg.get("E05912"), "MD"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.type = enmMMLType.SoftLfoDepth;
                mml.args = new List<object>();
                mml.args.Add(n);
            }
            else
            {
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(string.Format(msg.get("E05912"), "M"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.type = enmMMLType.SoftLfo;
                mml.args = new List<object>();
                mml.args.Add(n);
                while (pw.getChar() == ',')
                {
                    pw.incPos();
                    if (!pw.getNum(out n))
                    {
                        msgBox.setErrMsg(string.Format(msg.get("E05912"), "M"), pw.getSrcFn(), pw.getLineNumber());
                        return;
                    }
                    mml.args.Add(n);
                }
            }
        }

        private void CmdSlotDetune(partWork pw,MML mml)
        {
            int n;
            mml.type = enmMMLType.SlotDetune;
            mml.args = new List<object>();
            do
            {
                pw.incPos();
                if (!pw.getNum(out n))
                {
                    msgBox.setErrMsg(string.Format(msg.get("E05912"),"S"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                mml.args.Add(n);
            } while (pw.getChar() == ',');
            if (mml.args.Count != 4)
            {
                msgBox.setErrMsg(string.Format(msg.get("E05912"), "S"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }
        }

        private void CmdLfo(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            char c = pw.getChar();

            if (c == 'A')
            {
                pw.incPos();
                char d = pw.getChar();
                if (d == 'M')
                {
                    pw.incPos();
                    d = pw.getChar();
                    if (d == 'S')
                    {
                        pw.incPos();
                        if (!pw.getNum(out n))
                        {
                            msgBox.setErrMsg(msg.get("E05024"), pw.getSrcFn(), pw.getLineNumber());
                            return;
                        }
                        mml.type = enmMMLType.Lfo;
                        mml.args = new List<object>();
                        mml.args.Add("MAMS");
                        mml.args.Add(n);
                        return;
                    }
                }
                msgBox.setErrMsg(msg.get("E05025"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }

            if (c < 'P' && c > 'S')
            {
                msgBox.setErrMsg(msg.get("E05026"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }

            pw.incPos();
            char t = pw.getChar();

            if (c == 'P' && t == 'M')
            {
                pw.incPos();
                char d = pw.getChar();
                if (d == 'S')
                {
                    pw.incPos();
                    if (!pw.getNum(out n))
                    {
                        msgBox.setErrMsg(msg.get("E05027"), pw.getSrcFn(), pw.getLineNumber());
                        return;
                    }
                    mml.type = enmMMLType.Lfo;
                    mml.args = new List<object>();
                    mml.args.Add("MPMS");
                    mml.args.Add(n);
                    return;
                }
                msgBox.setErrMsg(msg.get("E05028"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }

            if (t != 'T' && t != 'V' && t != 'H')
            {
                msgBox.setErrMsg(msg.get("E05029"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }

            mml.type = enmMMLType.Lfo;
            mml.args = new List<object>();
            mml.args.Add(c);
            mml.args.Add(t);

            n = -1;
            do
            {
                pw.incPos();
                if (pw.getNum(out n))
                {
                    mml.args.Add(n);
                }
                else
                {
                    msgBox.setErrMsg(msg.get("E05030"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }

                while (pw.getChar() == '\t' || pw.getChar() == ' ') { pw.incPos(); }

            } while (pw.getChar() == ',');

        }

        private void CmdLfoSwitch(partWork pw, MML mml)
        {

            pw.incPos();
            char c = pw.getChar();
            if (c < 'P' || c > 'S')
            {
                msgBox.setErrMsg(msg.get("E05031"), pw.getSrcFn(), pw.getLineNumber());
                pw.incPos();
                return;
            }

            int n = -1;
            pw.incPos();
            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05032"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }
            n = Common.CheckRange(n, 0, 2);

            mml.type = enmMMLType.LfoSwitch;
            mml.args = new List<object>();
            mml.args.Add(c);
            mml.args.Add(n);
        }

        private void CmdSusOnOff(partWork pw, MML mml)
        {
            pw.incPos();
            char c = pw.getChar();
            pw.incPos();
            if (c != 'o' && c != 'f')
            {
                msgBox.setErrMsg(msg.get("E05031"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }

            mml.type = enmMMLType.SusOnOff;
            mml.args = new List<object>();
            mml.args.Add(c);
        }

        private void CmdY(partWork pw, MML mml)
        {
            int n = -1;
            byte adr = 0;
            byte dat = 0;
            //byte op = 0;
            string toneparamName = "";
            pw.incPos();

            char c = pw.getChar();
            if (c >= 'A' && c <= 'Z')
            {
                toneparamName = ""+c;
                pw.incPos();
                toneparamName += pw.getChar();
                pw.incPos();
                toneparamName += pw.getChar();
                pw.incPos();

                switch (toneparamName)
                {
                    case "DM,":
                        break;
                    case "TL,":
                        break;
                    case "KA,":
                        break;
                    case "DR,":
                        break;
                    case "SR,":
                        break;
                    case "SL,":
                        break;
                    case "SE,":
                        break;
                }
            }

            if (pw.getNum(out n))
            {
                adr = (byte)(n & 0xff);
            }
            pw.incPos();
            if (pw.getNum(out n))
            {
                dat = (byte)(n & 0xff);
            }

            mml.type = enmMMLType.Y;
            mml.args = new List<object>();
            if (!string.IsNullOrEmpty(toneparamName)) mml.args.Add(toneparamName);
            mml.args.Add(adr);
            mml.args.Add(dat);
        }

        private void CmdMacro(partWork pw,MML mml)
        {
            int n = -1;
            pw.incPos();
            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(string.Format(msg.get("E05912"),"*"), pw.getSrcFn(), pw.getLineNumber());
                return;
            }
            mml.type = enmMMLType.Macro;
            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdComment(partWork pw, MML mml)
        {
            int num = pw.getLineNumber();
            while (num == pw.getLineNumber())
            {
                pw.incPos();
                char c=pw.getChar();
                if (c == 0) break;
            }
        }

        private void CmdFillRest(partWork pw, MML mml)
        {
            pw.restMode = true;
            pw.incPos();
        }

        private void CmdJump(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.Jump;
            mml.args = null;
            desVGM.useJumpCommand++;
        }

        private void CmdNoise(partWork pw, MML mml)
        {
            int n = -1;
            pw.incPos();

            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05033"), pw.getSrcFn(), pw.getLineNumber());
                return;

            }
            mml.type = enmMMLType.Noise;
            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdMixer(partWork pw, MML mml)
        {
            int n = -1;
            pw.incPos();

            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05034"), pw.getSrcFn(), pw.getLineNumber());
                return;

            }
            mml.type = enmMMLType.MixerMode;
            mml.args = new List<object>();
            mml.args.Add(n);

        }

        private void CmdKeyShift(partWork pw, MML mml)
        {
            int n = -1;
            pw.incPos();

            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05035"), pw.getSrcFn(), pw.getLineNumber());
                return;

            }
            mml.type = enmMMLType.KeyShift;
            mml.args = new List<object>();
            mml.args.Add(n);
            n = Common.CheckRange(n, -128, 128);
            pw.keyShift = n;
        }

        private void CmdRelativeKeyShift(partWork pw, MML mml)
        {
            int n = -1;
            pw.incPos();

            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05035"), pw.getSrcFn(), pw.getLineNumber());
                return;

            }
            mml.type = enmMMLType.RelativeKeyShift;
            mml.args = new List<object>();
            mml.args.Add(n);
            n = Common.CheckRange(n, -128, 128);
            pw.relKeyShift = n;
        }

        private void CmdShuffle(partWork pw, MML mml)
        {
            int n = -1;
            pw.incPos();

            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05907"), pw.getSrcFn(), pw.getLineNumber());
                return;

            }
            mml.type = enmMMLType.Shuffle;
            mml.args = new List<object>();
            mml.args.Add(n);
            pw.shuffle = n;
            pw.shuffleDirection = 1;
        }

        private void CmdRelativeVolume(partWork pw, MML mml)
        {
            int n = -1;
            pw.incPos();

            if (!pw.getNum(out n))
            {
                msgBox.setErrMsg(msg.get("E05904"), pw.getSrcFn(), pw.getLineNumber());
                return;

            }
            mml.type = enmMMLType.RelativeVolume;
            mml.args = new List<object>();
            mml.args.Add(n);
            pw.RelVolume = n;
        }

        private void CmdEchoMacro(partWork pw, MML mml)
        {
            int n1 = -1;
            int n2 = -1;
            pw.incPos();

            if (pw.getChar() == '=')
            {
                pw.incPos();
                if (!pw.getNum(out n1))
                {
                    msgBox.setErrMsg(msg.get("E05905"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                pw.incPos();
                if (!pw.getNum(out n2))
                {
                    msgBox.setErrMsg(msg.get("E05905"), pw.getSrcFn(), pw.getLineNumber());
                    return;
                }
                //mml.type = enmMMLType.EchoMacro;
                //mml.args = new List<object>();
                n1 = Math.Min(Math.Max(n1, 1), 9);
                //mml.args.Add(n1);
                //mml.args.Add(n2);

                echoShare.backStep = n1;
                echoShare.downVolume = n2;
            }
            else
            {
                mml.type = enmMMLType.Echo;
                mmls = new List<MML>();
                MML m = null;
                //mml.type = enmMMLType.Echo;
                //mml.args = new List<object>();

                if (echoShare.downVolume != 0)
                {
                    m = new MML();
                    m.line = pw.getLine();
                    m.column = pw.getPos();
                    m.type = enmMMLType.VolumeDown;
                    m.args = new List<object>();
                    m.args.Add(echoShare.downVolume);
                    mmls.Add(m);
                }

                m = new MML();
                m.line = pw.getLine();
                m.column = pw.getPos();
                m.type = enmMMLType.Note;
                m.args = new List<object>();
                Note note= pw.echo.GetEchoNote(echoShare.backStep);
                if (note.length == 0)
                {
                    note.length = (int)pw.length;
                }
                m.args.Add(note);
                mmls.Add(m);

                if (echoShare.downVolume != 0)
                {
                    m = new MML();
                    m.line = pw.getLine();
                    m.column = pw.getPos();
                    m.type = enmMMLType.VolumeUp;
                    m.args = new List<object>();
                    m.args.Add(echoShare.downVolume);
                    mmls.Add(m);
                }
            }
        }

        private void CmdNote(partWork pw, char cmd, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.Note;
            mml.args = new List<object>();
            Note note = new Note();
            mml.args.Add(note);
            note.cmd = cmd;
            note.octave = pw.octaveNow;

            //+ -の解析
            int shift = 0;
            while (pw.getChar() == '+' || pw.getChar() == '-')
            {
                if (pw.getChar() == '+')
                    shift++;
                else
                    shift--;
                pw.incPos();
            }
            note.shift = shift + pw.keyShift + pw.relKeyShift;

            int n = -1;
            bool directFlg = false;

            //数値の解析
            if (pw.getNumNoteLength(out n, out directFlg))
            {
                if (!directFlg)
                {
                    if ((int)pw.clock % n != 0)
                    {
                        msgBox.setWrnMsg(string.Format(msg.get("E05023"), n), pw.getSrcFn(), pw.getLineNumber());
                    }
                    n = (int)pw.clock / n;
                }
                else
                {
                    n = Common.CheckRange(n, 1, 65535);
                }

                note.length = n;

                //ToneDoubler'0'指定の場合はここで解析終了
                if (n == 0)
                {
                    swToneDoubler = true;
                    return;
                }
            }
            else
            {
                note.length = (int)pw.length;

                //Tone Doubler','指定の場合はここで解析終了
                if (pw.getChar() == ',')
                {
                    pw.incPos();
                    swToneDoubler = true;
                    return;
                }
            }

            //.の解析
            int futen = 0;
            int fn = note.length;
            while (pw.getChar() == '.')
            {
                if (fn % 2 != 0)
                {
                    msgBox.setWrnMsg(msg.get("E05036")
                        , mml.line.Fn
                        , mml.line.Num);
                }
                fn = fn / 2;
                futen += fn;
                pw.incPos();
            }
            note.length += futen;

            //シャッフル効果
            note.length += pw.shuffle * pw.shuffleDirection;
            pw.shuffleDirection = -pw.shuffleDirection;

            //
            if (note.length < 1)
            {
                msgBox.setWrnMsg(msg.get("E05904"), pw.getSrcFn(), pw.getLineNumber());
                note.length = 1;
            }

            //Echo
            if (pw.echo_PortaCounter < 1)//ポルタメント中のノートははじめの一つだけ貯めるが以降バッファにためない。
            {
                Note enote = new Note();
                enote.cmd = cmd;
                enote.octave = pw.octaveNow;
                enote.shift = shift;
                enote.length = note.length;
                pw.echo.Add(enote);

                if (pw.mPortaSW) pw.echo_PortaCounter++;
            }

        }

        private void CmdRest(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.Rest;
            mml.args = new List<object>();
            Rest rest = new Rest();
            mml.args.Add(rest);

            rest.cmd = 'r';

            int n = -1;
            bool directFlg = false;

            //数値の解析
            if (pw.getNumNoteLength(out n, out directFlg))
            {
                if (!directFlg)
                {
                    if ((int)pw.clock % n != 0)
                    {
                        msgBox.setWrnMsg(string.Format(msg.get("E05023"), n), pw.getSrcFn(), pw.getLineNumber());
                    }
                    n = (int)pw.clock / n;
                }
                else
                {
                    n = Common.CheckRange(n, 1, 65535);
                }

                rest.length = n;

            }
            else
            {
                rest.length = (int)pw.length;
            }

            //.の解析
            int futen = 0;
            int fn = rest.length;
            while (pw.getChar() == '.')
            {
                if (fn % 2 != 0)
                {
                    msgBox.setWrnMsg(msg.get("E05036")
                        , mml.line.Fn
                        , mml.line.Num);
                }
                fn = fn / 2;
                futen += fn;
                pw.incPos();
            }
            rest.length += futen;

        }

        private void CmdRestNoWork(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.Rest;
            mml.args = new List<object>();
            Rest rest = new Rest();
            mml.args.Add(rest);

            rest.cmd = 'R';

            int n = -1;
            bool directFlg = false;

            //数値の解析
            if (pw.getNumNoteLength(out n, out directFlg))
            {
                if (!directFlg)
                {
                    if ((int)pw.clock % n != 0)
                    {
                        msgBox.setWrnMsg(string.Format(msg.get("E05023")
                            , n)
                            , pw.getSrcFn()
                            , pw.getLineNumber());
                    }
                    n = (int)pw.clock / n;
                }
                else
                {
                    n = Common.CheckRange(n, 1, 65535);
                }

                rest.length = n;

            }
            else
            {
                rest.length = 0;
            }

            //.の解析
            int futen = 0;
            int fn = rest.length;
            while (pw.getChar() == '.')
            {
                if (fn % 2 != 0)
                {
                    msgBox.setWrnMsg(msg.get("E05036")
                        , mml.line.Fn
                        , mml.line.Num);
                }
                fn = fn / 2;
                futen += fn;
                pw.incPos();
            }
            rest.length += futen;

        }

        private void CmdLyric(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.Lyric;
            mml.args = new List<object>();
            string str = "";
            while (true)
            {
                char ch = pw.getChar();
                if (ch == '"')
                {
                    pw.incPos();
                    break;
                }
                if (ch == '\\')
                {
                    pw.incPos();
                    if (ch != '"')
                    {
                        str += '\\';
                    }
                    ch = pw.getChar();
                }
                if (ch == '\0') break;

                str += ch;
                pw.incPos();
            }
            mml.args.Add(str);

            int n = -1;
            bool directFlg = false;
            int length = 0;

            //数値の解析
            if (pw.getNumNoteLength(out n, out directFlg))
            {
                if (!directFlg)
                {
                    if ((int)pw.clock % n != 0)
                    {
                        msgBox.setWrnMsg(string.Format(msg.get("E05023"), n), pw.getSrcFn(), pw.getLineNumber());
                    }
                    n = (int)pw.clock / n;
                }
                else
                {
                    n = Common.CheckRange(n, 1, 65535);
                }

                length = n;

            }
            else
            {
                length = (int)pw.length;
            }

            //.の解析
            int futen = 0;
            int fn = length;
            while (pw.getChar() == '.')
            {
                if (fn % 2 != 0)
                {
                    msgBox.setWrnMsg(msg.get("E05036")
                        , mml.line.Fn
                        , mml.line.Num);
                }
                fn = fn / 2;
                futen += fn;
                pw.incPos();
            }
            length += futen;
            mml.args.Add(length);
        }

        private void CmdBend(partWork pw, MML mml)
        {
            pw.incPos();
            mml.type = enmMMLType.Bend;
            mml.args = null;
        }

        private void CmdTie(partWork pw, MML mml)
        {
            pw.incPos();

            mml.type = enmMMLType.Tie;
            mml.args = null;

            //int n;
            //bool directFlg = false;
            //if (!pw.getNumNoteLength(out n, out directFlg))
            //{
            //    return;
            //}

            //mml.type = enmMMLType.TiePC;
            //if (!directFlg)
            //{
            //    if ((int)pw.clock % n != 0)
            //    {
            //        msgBox.setWrnMsg(string.Format(
            //            msg.get("E05023")
            //            , n), pw.getSrcFn(), pw.getLineNumber());
            //    }
            //    n = (int)pw.clock / n;
            //}
            //else
            //{
            //    n = Common.CheckRange(n, 1, 65535);
            //}

            ////.の解析
            //int futen = 0;
            //int fn = n;
            //while (pw.getChar() == '.')
            //{
            //    if (fn % 2 != 0)
            //    {
            //        msgBox.setWrnMsg(msg.get("E05036")
            //            , mml.line.Fn
            //            , mml.line.Num);
            //    }
            //    fn = fn / 2;
            //    futen += fn;
            //    pw.incPos();
            //}
            //n += futen;

            ////シャッフル効果
            //n += pw.shuffle * pw.shuffleDirection;
            //pw.shuffleDirection = -pw.shuffleDirection;

            //mml.args = new List<object>();
            //mml.args.Add(n);
        }

        private void CmdTiePC(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            mml.type = enmMMLType.TiePC;

            //数値の解析
            bool directFlg = false;
            if (pw.getNumNoteLength(out n, out directFlg))
            {
                if (!directFlg)
                {
                    if ((int)pw.clock % n != 0)
                    {
                        msgBox.setWrnMsg(string.Format(msg.get("E05023")
                            , n), pw.getSrcFn(), pw.getLineNumber());
                    }
                    n = (int)pw.clock / n;
                }
                else
                {
                    n = Common.CheckRange(n, 1, 65535);
                }

            }
            else
            {
                if (n == -1)
                {
                    n = (int)pw.length;
                }
                else
                {
                    n = 0;
                }
            }

            //.の解析
            int futen = 0;
            int fn = n;
            while (pw.getChar() == '.')
            {
                if (fn % 2 != 0)
                {
                    msgBox.setWrnMsg(msg.get("E05036")
                        , mml.line.Fn
                        , mml.line.Num);
                }
                fn = fn / 2;
                futen += fn;
                pw.incPos();
            }
            n += futen;

            //シャッフル効果
            n += pw.shuffle * pw.shuffleDirection;
            pw.shuffleDirection = -pw.shuffleDirection;

            mml.args = new List<object>();
            mml.args.Add(n);
        }

        private void CmdTieMC(partWork pw, MML mml)
        {
            int n;
            pw.incPos();
            mml.type = enmMMLType.TieMC;

            //数値の解析
            bool directFlg = false;
            if (pw.getNumNoteLength(out n, out directFlg))
            {
                if (!directFlg)
                {
                    if ((int)pw.clock % n != 0)
                    {
                        msgBox.setWrnMsg(string.Format(msg.get("E05023")
                            , n), pw.getSrcFn(), pw.getLineNumber());
                    }
                    n = (int)pw.clock / n;
                }
                else
                {
                    n = Common.CheckRange(n, 1, 65535);
                }

            }
            else
            {
                n = 0;
            }

            //.の解析
            int futen = 0;
            int fn = n;
            while (pw.getChar() == '.')
            {
                if (fn % 2 != 0)
                {
                    msgBox.setWrnMsg(msg.get("E05036")
                        , mml.line.Fn
                        , mml.line.Num);
                }
                fn = fn / 2;
                futen += fn;
                pw.incPos();
            }
            n += futen;

            //シャッフル効果
            n += pw.shuffle * pw.shuffleDirection;
            pw.shuffleDirection = -pw.shuffleDirection;

            mml.args = new List<object>();
            mml.args.Add(n);
        }

        #endregion

        #region step2

        private void Step2(partWork pw)
        {
            for (int i = 0; i < pw.mmlData.Count; i++)
            {
                if (pw.mmlData[i].type == enmMMLType.ToneDoubler)
                {
                    step2_CmdToneDoubler(pw, i);
                }
            }

            //ポルタメントコマンドをベンドに置き換える
            for (int i = 0; i < pw.mmlData.Count; i++)
            {
                if (pw.mmlData[i].type == enmMMLType.Porta)
                {
                    step2_CmdPorta(pw, i);
                }
            }


            for (int i = 0; i < pw.mmlData.Count; i++)
            {
                if (pw.mmlData[i].type == enmMMLType.Bend)
                {
                    step2_CmdBend(pw, i);
                }
            }

            for (int i = 0; i < pw.mmlData.Count; i++)
            {
                if (pw.mmlData[i].type == enmMMLType.TiePC)
                {
                    step2_CmdTiePC(pw, i);
                    pw.mmlData.RemoveAt(i);
                    i--;
                }
                if (pw.mmlData[i].type == enmMMLType.TieMC)
                {
                    step2_CmdTieMC(pw, i);
                    pw.mmlData.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < pw.mmlData.Count; i++)
            {
                if (pw.mmlData[i].type == enmMMLType.Tie)
                {
                    step2_CmdTie(pw, i);
                    pw.mmlData.RemoveAt(i);
                    i--;
                }
            }
        }

        private void step2_CmdToneDoubler(partWork pw, int pos)
        {
            if (pos < 1 || pw.mmlData[pos - 1].type != enmMMLType.Note)
            {
                msgBox.setErrMsg(msg.get("E05037")
                , pw.mmlData[pos].line.Fn
                , pw.mmlData[pos].line.Num);
                return;
            }

            Note note = (Note)pw.mmlData[pos - 1].args[0];

            //直前の音符コマンドへToneDoublerコマンドが続くことを知らせる
            note.tDblSw = true;

            //直後の音符コマンドまでサーチ
            Note toneDoublerNote = null;
            List<MML> toneDoublerMML = new List<MML>();
            for (int i = pos + 1; i < pw.mmlData.Count; i++)
            {
                switch (pw.mmlData[i].type)
                {
                    case enmMMLType.Note:
                        toneDoublerNote = (Note)pw.mmlData[i].args[0];
                        pw.mmlData.RemoveAt(i);
                        i--;
                        goto loop_exit;
                    case enmMMLType.Octave:
                    case enmMMLType.OctaveUp:
                    case enmMMLType.OctaveDown:
                        toneDoublerMML.Add(pw.mmlData[i]);
                        pw.mmlData.RemoveAt(i);
                        i--;
                        break;
                    default:
                        msgBox.setErrMsg(msg.get("E05038")
                        , pw.mmlData[i].line.Fn
                        , pw.mmlData[i].line.Num);
                        return;
                }
            }

            if (toneDoublerNote == null) return;

            loop_exit:

            note.tDblCmd = toneDoublerNote.cmd;
            note.tDblShift = toneDoublerNote.shift;
            note.length = toneDoublerNote.length;
            note.tDblOctave = toneDoublerMML;

            pw.mmlData[pos].args.Add(toneDoublerMML);
        }

        private void step2_CmdPorta(partWork pw,int pos)
        {
            int firstNotePos = 0;
            try
            {
                while (pw.mmlData[pos].type != enmMMLType.Note)
                {
                    pos++;
                }
                firstNotePos = pos;
                pos++;
                MML mml = new MML();
                mml.type = enmMMLType.Bend;
                pw.mmlData.Insert(pos, mml);
            }
            catch
            {
                msgBox.setErrMsg(msg.get("E05053")
                , pw.mmlData[firstNotePos].line.Fn
                , pw.mmlData[firstNotePos].line.Num);
                return;
            }
            try
            {
                while (pw.mmlData[pos].type != enmMMLType.PortaEnd)
                {
                    pos++;
                }
                pw.mmlData.RemoveAt(pos);
            }
            catch
            {
                msgBox.setErrMsg(msg.get("E05052")
                , pw.mmlData[firstNotePos].line.Fn
                , pw.mmlData[firstNotePos].line.Num);
            }
        }

        private void step2_CmdBend(partWork pw, int pos)
        {
            if (!(
                    (
                    pos > 0
                    && pw.mmlData[pos - 1].type == enmMMLType.Note
                    )
                ||
                    (
                    pos > 1
                    && pw.mmlData[pos - 1].type == enmMMLType.ToneDoubler
                    && pw.mmlData[pos - 2].type == enmMMLType.Note
                    )
                ))
            {
                msgBox.setErrMsg(msg.get("E05039")
                , pw.mmlData[pos].line.Fn
                , pw.mmlData[pos].line.Num);
                return;
            }

            Note note = (Note)pw.mmlData[pos - (pw.mmlData[pos - 1].type == enmMMLType.Note ? 1 : 2)].args[0];

            //直前の音符コマンドへベンドコマンドが続くことを知らせる
            note.bendSw = true;

            //直後の音符コマンドまでサーチ
            Note bendNote = null;
            //List<MML> bendMML = new List<MML>();
            int bendOctave = note.octave;
            for (int i = pos + 1; i < pw.mmlData.Count; i++)
            {
                switch (pw.mmlData[i].type)
                {
                    case enmMMLType.Note:
                        bendNote = (Note)pw.mmlData[i].args[0];
                        pw.mmlData.RemoveAt(i);
                        i--;
                        goto loop_exit;
                    case enmMMLType.Octave:
                        bendOctave = (int)pw.mmlData[i].args[0];
                        bendOctave = Common.CheckRange(bendOctave, 1, 8);
                        pw.mmlData.RemoveAt(i);
                        i--;
                        break;
                    case enmMMLType.OctaveUp:
                        bendOctave += desVGM.info.octaveRev ? -1 : 1;
                        bendOctave = Common.CheckRange(bendOctave, 1, 8);
                        pw.mmlData.RemoveAt(i);
                        i--;
                        break;
                    case enmMMLType.OctaveDown:
                        bendOctave += desVGM.info.octaveRev ? 1 : -1;
                        bendOctave = Common.CheckRange(bendOctave, 1, 8);
                        //bendMML.Add(pw.mmlData[i]);
                        pw.mmlData.RemoveAt(i);
                        i--;
                        break;
                    default:
                        msgBox.setErrMsg(msg.get("E05040")
                        , pw.mmlData[i].line.Fn
                        , pw.mmlData[i].line.Num);
                        return;
                }
            }

            if (bendNote == null) return;

            loop_exit:

            note.bendCmd = bendNote.cmd;
            note.bendShift = bendNote.shift;
            //note.length = bendNote.length;
            //note.futen = bendNote.futen;
            note.bendOctave = bendOctave;
            pw.mmlData[pos].args = new List<object>();
            pw.mmlData[pos].args.Add(bendOctave);
        }

        private void step2_CmdTiePC(partWork pw, int pos)
        {
            int nPos = 0;

            //遡ってnoteを探す
            for (int i = pos - 1; i > 0; i--)
            {
                switch (pw.mmlData[i].type)
                {
                    case enmMMLType.ToneDoubler:
                    case enmMMLType.Bend:
                        break;
                    case enmMMLType.Note:
                    case enmMMLType.Rest:
                    case enmMMLType.RestNoWork:
                        nPos = i;
                        goto loop_exit;
                    default:
                        msgBox.setErrMsg(msg.get("E05041")
                        , pw.mmlData[pos].line.Fn
                        , pw.mmlData[pos].line.Num);
                        return;
                }
            }

            msgBox.setErrMsg(msg.get("E05042")
            , pw.mmlData[pos].line.Fn
            , pw.mmlData[pos].line.Num);
            return;
        loop_exit:

            Rest rest = (Rest)pw.mmlData[nPos].args[0];//NoteはRestを継承している

            rest.length += (int)pw.mmlData[pos].args[0];
        }

        private void step2_CmdTieMC(partWork pw, int pos)
        {
            int nPos = 0;

            //遡ってnoteを探す
            for (int i = pos - 1; i > 0; i--)
            {
                switch (pw.mmlData[i].type)
                {
                    case enmMMLType.ToneDoubler:
                    case enmMMLType.Bend:
                        break;
                    case enmMMLType.Note:
                    case enmMMLType.Rest:
                    case enmMMLType.RestNoWork:
                        nPos = i;
                        goto loop_exit;
                    default:
                        msgBox.setErrMsg(msg.get("E05043")
                        , pw.mmlData[pos].line.Fn
                        , pw.mmlData[pos].line.Num);
                        return;
                }
            }

            msgBox.setErrMsg(msg.get("E05044")
            , pw.mmlData[pos].line.Fn
            , pw.mmlData[pos].line.Num);
            return;
        loop_exit:

            Rest rest = (Rest)pw.mmlData[nPos].args[0];//NoteはRestを継承している
            rest.length -= (int)pw.mmlData[pos].args[0];
        }

        private void step2_CmdTie(partWork pw, int pos)
        {
            int nPos = 0;

            //遡ってnoteを探す
            for (int i = pos - 1; i > 0; i--)
            {
                switch (pw.mmlData[i].type)
                {
                    case enmMMLType.ToneDoubler:
                    case enmMMLType.Bend:
                        break;
                    case enmMMLType.Note:
                        nPos = i;
                        goto loop_exit;
                        //連続した&を認めない場合は以下のケースを有効にする
                    //case enmMMLType.Tie:
                    //    msgBox.setErrMsg(msg.get("E05045")
                    //    , pw.mmlData[pos].line.Fn
                    //    , pw.mmlData[pos].line.Num);
                    //    return;
                        //default:
                        //    msgBox.setErrMsg(msg.get("E05045")
                        //    , pw.mmlData[pos].line.Fn
                        //    , pw.mmlData[pos].line.Num);
                        //    return;
                }
            }

            msgBox.setErrMsg(msg.get("E05046")
            , pw.mmlData[pos].line.Fn
            , pw.mmlData[pos].line.Num);
            return;
        loop_exit:
            //直前の音符コマンドへ&コマンドが続くことを知らせる
            Note note = (Note)pw.mmlData[nPos].args[0];
            note.tieSw = true;
        }

        #endregion

        #region step3

        private void Step3(partWork pw)
        {
            //リピート処理向けスタックのクリア
            pw.stackRepeat.Clear();
            pw.stackRenpu.Clear();

            for (int i = 0; i < pw.mmlData.Count; i++)
            {
                switch (pw.mmlData[i].type)
                {
                    case enmMMLType.Repeat:
                        step3_CmdRepeatStart(pw, i);
                        break;
                    case enmMMLType.RepertExit:
                        step3_CmdRepeatExit(pw, i);
                        break;
                    case enmMMLType.RepeatEnd:
                        step3_CmdRepeatEnd(pw, i);
                        break;
                    case enmMMLType.Renpu:
                        step3_CmdRenpuStart(pw, i);
                        break;
                    case enmMMLType.RenpuEnd:
                        step3_CmdRenpuEnd(pw, i);
                        break;
                    case enmMMLType.Note:
                    case enmMMLType.Rest:
                    case enmMMLType.RestNoWork:
                        step3_CmdNoteCount(pw, i);
                        break;
                }
            }
        }

        private void step3_CmdRepeatExit(partWork pw, int pos)
        {
            int nst = 0;

            for (int searchPos = pos; searchPos < pw.mmlData.Count; searchPos++)
            {
                if (pw.mmlData[searchPos].type == enmMMLType.Repeat)
                {
                    nst++;
                    continue;
                }
                if (pw.mmlData[searchPos].type != enmMMLType.RepeatEnd)
                {
                    continue;
                }
                if (nst == 0)
                {
                    pw.mmlData[pos].args = new List<object>();
                    pw.mmlData[pos].args.Add(searchPos);
                    return;
                }
                nst--;
            }

            msgBox.setWrnMsg(msg.get("E05047")
                , pw.mmlData[pos].line.Fn
                , pw.mmlData[pos].line.Num);

        }

        private void step3_CmdRepeatEnd(partWork pw, int pos)
        {
            try
            {
                clsRepeat re = pw.stackRepeat.Pop();
                pw.mmlData[pos].args.Add(re.pos);
            }
            catch
            {
                msgBox.setWrnMsg(msg.get("E05048")
                , pw.mmlData[pos].line.Fn
                , pw.mmlData[pos].line.Num);
            }
        }

        private void step3_CmdRepeatStart(partWork pw, int pos)
        {
            clsRepeat rs = new clsRepeat()
            {
                pos = pos,
                repeatCount = -1//初期値
            };
            pw.stackRepeat.Push(rs);
        }

        private void step3_CmdRenpuStart(partWork pw, int pos)
        {
            clsRenpu r = new clsRenpu();
            r.pos = pos;
            r.repeatStackCount = pw.stackRepeat.Count;
            r.noteCount = 0;
            r.mml = pw.mmlData[pos];
            pw.stackRenpu.Push(r);
        }

        private void step3_CmdRenpuEnd(partWork pw, int pos)
        {
            try
            {
                clsRenpu r = pw.stackRenpu.Pop();
                r.mml.args = new List<object>();
                r.mml.args.Add(r.noteCount);
                if (pw.mmlData[pos].args != null)
                {
                    r.mml.args.Add(pw.mmlData[pos].args[0]);//音長(クロック数)
                }

                if (r.repeatStackCount != pw.stackRepeat.Count)
                {
                    msgBox.setWrnMsg(msg.get("E05049")
                    , pw.mmlData[pos].line.Fn
                    , pw.mmlData[pos].line.Num);
                }

                if (r.noteCount > 0)
                {
                    if (pw.stackRenpu.Count > 0)
                    {
                        pw.stackRenpu.First().noteCount++;
                    }
                }
            }
            catch
            {
                msgBox.setWrnMsg(msg.get("E05050")
                , pw.mmlData[pos].line.Fn
                , pw.mmlData[pos].line.Num);
            }
        }

        private void step3_CmdNoteCount(partWork pw, int pos)
        {
            if (pw.stackRenpu.Count < 1) return;

            pw.stackRenpu.First().noteCount++;
        }

        #endregion



    }
}
