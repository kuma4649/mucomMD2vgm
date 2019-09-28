﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class YM2612X : YM2612
    {

        public YM2612X(ClsVgm parent, int chipID, string initialPartName, string stPath, bool isSecondary) : base(parent, chipID, initialPartName, stPath, isSecondary)
        {
            _chipType = enmChipType.YM2612X;
            _Name = "YM2612X";
            _ShortName = "OPN2X";
            _ChMax = 13;
            _canUsePcm = true;
            _canUsePI = false;
            FNumTbl = _FNumTbl;
            IsSecondary = isSecondary;

            Frequency = 7670454;

            if (string.IsNullOrEmpty(initialPartName)) return;

            Dictionary<string, List<double>> dic = MakeFNumTbl();
            if (dic != null && dic.Count>0)
            {
                int c = 0;
                foreach (double v in dic["FNUM_00"])
                {
                    FNumTbl[0][c++] = (int)v;
                    if (c == FNumTbl[0].Length) break;
                }
                FNumTbl[0][FNumTbl[0].Length - 1] = FNumTbl[0][0] * 2;
            }

            Ch = new ClsChannel[ChMax];
            char[] PART_OPN2 = new char[] {
                'A', 'B', 'C', //FM1-3ch
                'H', 'I', 'J', //FM4-6ch
                'K',//FM6chPCMEx1ch
                'L', 'M', 'N', //FM3chEx
                'O', 'P', 'Q' //FM6chPCMEx2-4ch
            };
            for (int i = 0; i < Ch.Length; i++)
            {
                if (Ch[i] == null) Ch[i] = new ClsChannel();
                Ch[i].Name = PART_OPN2[i].ToString();
            }

            foreach (ClsChannel ch in Ch)
            {
                ch.Type = enmChannelType.FMOPN;
                ch.isSecondary = chipID == 1;
            }

            Ch[2].Type = enmChannelType.FMOPNex;
            Ch[6].Type = enmChannelType.FMPCMex;
            Ch[7].Type = enmChannelType.FMOPNex;
            Ch[8].Type = enmChannelType.FMOPNex;
            Ch[9].Type = enmChannelType.FMOPNex;
            Ch[10].Type = enmChannelType.FMPCMex;
            Ch[11].Type = enmChannelType.FMPCMex;
            Ch[12].Type = enmChannelType.FMPCMex;

            pcmDataInfo = new clsPcmDataInfo[] { new clsPcmDataInfo() };
            pcmDataInfo[0].totalBuf = new byte[0];
            pcmDataInfo[0].totalBufPtr = 0L;
            pcmDataInfo[0].use = false;

        }

        public override void InitPart(ref partWork pw)
        {
            pw.slots = (byte)((pw.Type == enmChannelType.FMOPN || pw.ch == 2 || pw.ch == 5) ? 0xf : 0x0);
            pw.volume = 127;
            pw.MaxVolume = 127;
            pw.MaxVolumeEasy = 15;
            pw.port0 = 0x52;
            pw.port1 = 0x53;
            pw.pcm = (pw.ch == 6 || pw.ch > 13);
            pw.octaveNew = 6;
            pw.octaveNow = 6;
            pw.beforeTLOP1 = -1;
            pw.beforeTLOP3 = -1;
            pw.beforeTLOP2 = -1;
            pw.beforeTLOP4 = -1;
        }


        public void OutYM2612XPcmKeyON(partWork pw)
        {
            if (pw.instrument >= 63) return;

            if (pw.isPcmMap)
            {
                int n = Const.NOTE.IndexOf(pw.noteCmd);
                int f = pw.octaveNow * 12 + n + pw.shift + pw.keyShift;
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

            if (!parent.instPCM.ContainsKey(pw.instrument))
            {
                msgBox.setErrMsg(string.Format(msg.get("E21000"), pw.instrument));
                return;
            }

            int id = parent.instPCM[pw.instrument].seqNum + 1;

            int ch = Common.CheckRange(pw.ch == 6 ? 0 : Math.Max(0, pw.ch - 9), 0, 3);
            int priority = 0;

            parent.OutData(
                //mml,
                0x54 // original vgm command : YM2151
                , (byte)(0x50 + ((priority & 0x3) << 2) + (ch & 0x3))
                , (byte)id
                );

           // parent.info.samplesPerClock = parent.info.xgmSamplesPerSecond * 60.0 * 4.0 / (parent.info.tempo * parent.info.clockCount);

            //必要なサンプル数を算出し、保持しているサンプル数より大きい場合は更新
            double m = pw.waitCounter * 60.0 * 4.0 / (parent.info.tempo * parent.info.clockCount) * 14000.0;//14000(Hz) = xgm sampling Rate
            parent.instPCM[pw.instrument].xgmMaxSampleCount = Math.Max(parent.instPCM[pw.instrument].xgmMaxSampleCount, m);

            if (parent.instPCM[pw.instrument].status != enmPCMSTATUS.ERROR)
            {
                parent.instPCM[pw.instrument].status = enmPCMSTATUS.USED;
            }

        }

        public void OutYM2612XPcmKeyOFF(MML mml, partWork pw)
        {

            int id = 0;
            int ch = Math.Max(0, pw.ch - 8);
            int priority = 0;

            parent.OutData(
                0x54 // original vgm command : YM2151
                , (byte)(0x50 + ((priority & 0x3) << 2) + (ch & 0x3))
                , (byte)id
                );

            parent.info.samplesPerClock = parent.info.xgmSamplesPerSecond * 60.0 * 4.0 / (parent.info.tempo * parent.info.clockCount);

        }

        public override void StorePcm(Dictionary<int, clsPcm> newDic, KeyValuePair<int, clsPcm> v, byte[] buf, bool is16bit, int samplerate, params object[] option)
        {
            clsPcmDataInfo pi = pcmDataInfo[0];

            try
            {
                byte[] newBuf;
                long size = buf.Length;

                for (int i = 0; i < size; i++)
                {
                    buf[i] -= 0x80;//符号化
                }

                //Padding
                if (size % 0x100 != 0)
                {
                    newBuf = Common.PcmPadding(ref buf, ref size, 0x00, 0x100);
                    buf = newBuf;
                }

                if (newDic.ContainsKey(v.Key))
                {
                    newDic.Remove(v.Key);
                }
                newDic.Add(v.Key, new clsPcm(
                    v.Value.num
                    , v.Value.seqNum
                    , v.Value.chip
                    , false
                    , v.Value.fileName
                    , v.Value.freq != -1 ? v.Value.freq : samplerate
                    , v.Value.vol
                    , pi.totalBufPtr
                    , pi.totalBufPtr + size - 1
                    , size
                    , -1
                    , is16bit
                    , samplerate));
                pi.totalBufPtr += size;

                newBuf = new byte[pi.totalBuf.Length + buf.Length];
                Array.Copy(pi.totalBuf, newBuf, pi.totalBuf.Length);
                Array.Copy(buf, 0, newBuf, pi.totalBuf.Length, buf.Length);

                pi.totalBuf = newBuf;
                pi.use = true;
                pcmDataEasy = pi.use ? pi.totalBuf : null;
            }
            catch
            {
                pi.use = false;
            }
        }


        public override void CmdMode(partWork pw, MML mml)
        {
            int n = (int)mml.args[0];
            if (pw.Type == enmChannelType.FMPCMex)
            {
                n = Common.CheckRange(n, 0, 1);
                pw.chip.lstPartWork[5].pcm = (n == 1);
                pw.chip.lstPartWork[9].pcm = (n == 1);
                pw.chip.lstPartWork[10].pcm = (n == 1);
                pw.chip.lstPartWork[11].pcm = (n == 1);
                pw.freq = -1;//freqをリセット
                pw.instrument = -1;
                OutSetCh6PCMMode(
                    pw.chip.lstPartWork[5]
                    , pw.chip.lstPartWork[5].pcm
                    );

                return;
            }

            base.CmdMode(pw, mml);

        }

        public override void CmdPcmMapSw(partWork pw, MML mml)
        {
            bool sw = (bool)mml.args[0];
            if (pw.Type == enmChannelType.FMPCMex)
            {
                pw.isPcmMap = sw;
            }
        }


        public override void CmdInstrument(partWork pw, MML mml)
        {
            char type = (char)mml.args[0];
            //int n = (int)mml.args[1];
            int n = 0;
            if (mml.args[1].GetType() == typeof(int))
            {
                n = (int)mml.args[1];
            }
            else
            {
                //文字指定
                foreach (int instNo in parent.instFM.Keys)
                {
                    if (parent.instFM[instNo].Name == (string)mml.args[1])
                    {
                        n = instNo;
                    }
                }
            }

            if (type == 'n' || type == 'N' || type == 'R' || type == 'A')
            {
                if (pw.Type == enmChannelType.FMOPNex)
                {
                    pw.instrument = n;
                    lstPartWork[2].instrument = n;
                    lstPartWork[7].instrument = n;
                    lstPartWork[8].instrument = n;
                    lstPartWork[9].instrument = n;
                    OutFmSetInstrument(pw, n, pw.volume);
                    return;
                }
            }

            if (type == 'n')
            {
                if (pw.pcm)
                {

                    if (pw.isPcmMap)
                    {
                        pw.pcmMapNo = n;
                        if (!parent.instPCMMap.ContainsKey(n))
                        {
                            msgBox.setErrMsg(string.Format(msg.get("E10024"), n));
                        }
                        return;
                    }

                    pw.instrument = n;
                    //SetDummyData(pw, mml);
                    if (!parent.instPCM.ContainsKey(n))
                    {
                        msgBox.setErrMsg(string.Format(msg.get("E21000"), n));
                    }
                    else
                    {
                        if (parent.instPCM[n].chip != enmChipType.YM2612X)
                        {
                            msgBox.setErrMsg(string.Format(msg.get("E21001"), n));
                        }
                    }
                    return;
                }

            }

            base.CmdInstrument(pw, mml);
        }


    }
}
