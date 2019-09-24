using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Core
{
    /// <summary>
    /// コンパイラ
    /// </summary>
    public class MucomMD2vgm
    {

        private string srcFn;
        private string desFn;
        private string stPath;
        public ClsVgm desVGM = null;
        private Action<string> Disp = null;
        private int pcmDataSeqNum = 0;
        private bool isLoopEx = false;
        private int rendSecond=600;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="srcFn">ソースファイル</param>
        /// <param name="desFn">出力ファイル</param>
        public MucomMD2vgm(string srcFn, string desFn, string stPath, Action<string> disp,bool isLoopEx,int rendSecond)
        {
            this.srcFn = srcFn;
            this.desFn = desFn;
            this.stPath = stPath;
            this.Disp = disp;
            this.isLoopEx = isLoopEx;
            this.rendSecond = rendSecond;

            log.ForcedWrite(srcFn);
            log.ForcedWrite(desFn);
            log.ForcedWrite(stPath);
        }

        /// <summary>
        /// コンパイル開始
        /// </summary>
        /// <returns></returns>
        public int Start()
        {
            try
            {
                Disp(string.Format(msg.get("I04000"), "mucomMD2vgm"));
                Disp("");

                Disp(msg.get("I04001"));
                if (!File.Exists(srcFn))
                {
                    msgBox.setErrMsg(msg.get("E04000"));
                    return -1;
                }

                Disp(msg.get("I04002"));
                string path = Path.GetDirectoryName(Path.GetFullPath(srcFn));
                List<Line> src = GetSrc(File.ReadAllLines(srcFn,System.Text.Encoding.GetEncoding("Shift_JIS")), path);
                if (src == null)
                {
                    msgBox.setErrMsg(msg.get("E04001"));
                    return -1;
                }

                Disp(msg.get("I04003"));
                desVGM = new ClsVgm(stPath, srcFn, isLoopEx, rendSecond);
                if (desVGM.Analyze(src) != 0)
                {
                    msgBox.setErrMsg(string.Format(
                        msg.get("E04002")
                        , desVGM.lineNumber));
                    return -1;
                }

                Disp(msg.get("I04021"));
                desVGM.LoadVoicedat();

                Disp(msg.get("I04022"));
                desVGM.LoadSSGdat();

                Disp(msg.get("I04023"));
                desVGM.LoadAdpcmdat();

                Disp(msg.get("I04004"));
                if (desVGM.instPCMDatSeq.Count > 0) GetPCMData(path);

                byte[] desBuf = null;

                if (!isLoopEx)
                {
                    Disp(msg.get("I04005"));
                    MMLAnalyze mmlAnalyze = new MMLAnalyze(desVGM);
                    if (mmlAnalyze.Start() != 0)
                    {
                        msgBox.setErrMsg(string.Format(msg.get("E04003"), mmlAnalyze.lineNumber));
                        return -1;
                    }

                    desVGM.CutYM2612();

                    switch (desVGM.info.format)
                    {
                        case enmFormat.VGM:
                            Disp(msg.get("I04006"));
                            desBuf = desVGM.Vgm_getByteData(mmlAnalyze.mmlData, enmLoopExStep.none);
                            Disp(msg.get("I04007"));
                            break;
                        case enmFormat.XGM:
                            Disp(msg.get("I04008"));
                            desBuf = desVGM.Xgm_getByteData(mmlAnalyze.mmlData, enmLoopExStep.none);
                            Disp(msg.get("I04009"));
                            break;
                    }

                    if (desBuf == null)
                    {
                        msgBox.setErrMsg(string.Format(
                            msg.get("E04004")
                            , desVGM.lineNumber));
                        return -1;
                    }

                }
                else
                {
                    //Loopポイントその他の情報を採取するために一旦解析と1ループの演奏を行う。

                    Disp(msg.get("I04024"));
                    MMLAnalyze mmlAnalyze = new MMLAnalyze(desVGM);
                    if (mmlAnalyze.Start() != 0)
                    {
                        msgBox.setErrMsg(string.Format(msg.get("E04003"), mmlAnalyze.lineNumber));
                        return -1;
                    }

                    desVGM.CutYM2612();

                    switch (desVGM.info.format)
                    {
                        case enmFormat.VGM:
                            Disp(msg.get("I04025"));
                            desBuf = desVGM.Vgm_getByteData(mmlAnalyze.mmlData, enmLoopExStep.Inspect);
                            Disp(msg.get("I04026"));
                            break;
                        case enmFormat.XGM:
                            Disp(msg.get("I04030"));
                            desBuf = desVGM.Xgm_getByteData(mmlAnalyze.mmlData, enmLoopExStep.Inspect);
                            Disp(msg.get("I04031"));
                            break;
                    }

                    if (desBuf == null)
                    {
                        msgBox.setErrMsg(string.Format(
                            msg.get("E04004")
                            , desVGM.lineNumber));
                        return -1;
                    }


                    //情報収集
                    Dictionary<KeyValuePair<enmChipType, int>, clsLoopInfo> dicLoopInfo = GetLoopInfo();


                    //収集した情報をもとに 本解析と演奏を行う。

                    desBuf = null;
                    
                    Disp(msg.get("I04027"));
                    mmlAnalyze = new MMLAnalyze(desVGM);
                    if (mmlAnalyze.Start() != 0)
                    {
                        msgBox.setErrMsg(string.Format(msg.get("E04003"), mmlAnalyze.lineNumber));
                        return -1;
                    }

                    desVGM.CutYM2612();

                    SetLoopInfo(desVGM, dicLoopInfo);

                    //1.ループ無しのパートがすべて演奏完了するまで演奏する
                    //(この間、ループ有りのパートはループ回数を消化しながらループさせる)
                    //2.ループ無しのパートがすべて演奏完了したうえで、ループ有りのパートがループ回数を完全に消化したら、
                    //もう一度ループ回数を充てんし、ループ回数を全て消化するまでループする
                    switch (desVGM.info.format)
                    {
                        case enmFormat.VGM:
                            Disp(msg.get("I04028"));
                            desBuf = desVGM.Vgm_getByteData(mmlAnalyze.mmlData, enmLoopExStep.Playing);
                            Disp(msg.get("I04029"));
                            break;
                        case enmFormat.XGM:
                            Disp(msg.get("I04032"));
                            desBuf = desVGM.Xgm_getByteData(mmlAnalyze.mmlData, enmLoopExStep.Playing);
                            Disp(msg.get("I04033"));
                            break;
                    }

                    if (desBuf == null)
                    {
                        msgBox.setErrMsg(string.Format(
                            msg.get("E04004")
                            , desVGM.lineNumber));
                        return -1;
                    }

                }



                Disp(msg.get("I04010"));
                if (desVGM.info.format == enmFormat.VGM)
                    outFile(desBuf);
                else
                    OutXgmFile(desBuf);


                Result();

                return 0;
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
                msgBox.setErrMsg(string.Format(msg.get("E04005")
                    , (desVGM == null) ? -1 : desVGM.lineNumber
                    , ex.Message
                    , ex.StackTrace));
                return -1;
            }
            finally
            {
                Disp(msg.get("I04011"));
                Disp("");
            }
        }

        private void SetLoopInfo(ClsVgm desVGM, Dictionary<KeyValuePair<enmChipType, int>, clsLoopInfo> dicLoopInfo)
        {
            foreach (KeyValuePair<enmChipType, ClsChip[]> kvp in desVGM.chips)
            {
                foreach (ClsChip chip in kvp.Value)
                {
                    if (!chip.use) continue;

                    foreach (partWork pw in chip.lstPartWork)
                    {
                        if (pw.pData == null) continue;

                        KeyValuePair<enmChipType, int> k = new KeyValuePair<enmChipType, int>(kvp.Key, pw.ch);
                        if (dicLoopInfo.ContainsKey(k))
                        {
                            pw.loopInfo = dicLoopInfo[k];
                        }
                    }
                }
            }
        }

        private Dictionary<KeyValuePair<enmChipType, int>, clsLoopInfo> GetLoopInfo()
        {
            //  各パートのループの有無を取得
            //  ループありのパートのLコマンド後のLengthから最大公倍数を得る
            //  取得した最大公倍数から各パートのループ回数を算出する
            desVGM.partCount = 0;
            desVGM.loopUsePartCount = 0;

            List<long> lengths = new List<long>();
            long clockPos = 0;
            foreach (KeyValuePair<enmChipType, ClsChip[]> kvp in desVGM.chips)
            {
                foreach (ClsChip chip in kvp.Value)
                {
                    if (!chip.use) continue;

                    foreach (partWork pw in chip.lstPartWork)
                    {
                        if (pw.pData == null) continue;

                        desVGM.partCount++;
                        if (!pw.loopInfo.use) continue;
                        if (pw.loopInfo.length < 1)
                        {
                            pw.loopInfo.use = false;
                            desVGM.loopUsePartCount++;
                            continue;//Lコマンド後の長さが0のパートは対象に含めない
                        }

                        desVGM.loopUsePartCount++;
                        lengths.Add(pw.loopInfo.length);
                        if (clockPos < pw.loopInfo.clockPos)//.clockCounter)
                        {
                            clockPos = pw.loopInfo.clockPos;//.clockCounter;
                        }
                    }
                }
            }

            //
            long loopClockLength = -1;
            partWork p = null;
            foreach (KeyValuePair<enmChipType, ClsChip[]> kvp in desVGM.chips)
            {
                foreach (ClsChip chip in kvp.Value)
                {
                    if (!chip.use) continue;

                    foreach (partWork pw in chip.lstPartWork)
                    {
                        if (pw.pData == null) continue;
                        if (!pw.loopInfo.use) continue;
                        if (pw.loopInfo.length < 1) continue;//Lコマンド後の長さが0のパートは対象に含めない

                        if (clockPos == pw.loopInfo.clockPos)//.clockCounter)
                        {
                            if (loopClockLength < pw.loopInfo.length)
                            {
                                p = pw;
                                loopClockLength = pw.loopInfo.length;
                            }
                            //goto loopExit;
                        }
                    }
                }
            }
            if (p != null)
            {
                p.loopInfo.isLongMml = true;
            }

        //loopExit:
            long lcm = Common.aryLcm(lengths.ToArray());
            Dictionary<KeyValuePair<enmChipType, int>, clsLoopInfo> dicLoopInfo = new Dictionary<KeyValuePair<enmChipType, int>, clsLoopInfo>();

            foreach (KeyValuePair<enmChipType, ClsChip[]> kvp in desVGM.chips)
            {
                foreach (ClsChip chip in kvp.Value)
                {
                    if (!chip.use) continue;

                    foreach (partWork pw in chip.lstPartWork)
                    {
                        if (pw.pData == null) continue;
                        if (!pw.loopInfo.use) continue;
                        if (pw.loopInfo.length < 1) continue;//Lコマンド後の長さが0のパートは対象に含めない

                        pw.loopInfo.playingTimes = (int)(lcm / pw.loopInfo.length);
                        pw.loopInfo.loopCount = pw.loopInfo.playingTimes;

                        pw.loopInfo.totalCounter = pw.clockCounter;
                        pw.loopInfo.loopCounter = pw.loopInfo.length;

                        pw.loopInfo.startFlag = false;
                        pw.loopInfo.lastOne = false;

                        dicLoopInfo.Add(new KeyValuePair<enmChipType, int>(kvp.Key, pw.ch), pw.loopInfo);
                    }
                }
            }

            return dicLoopInfo;
        }

        private void OutXgmFile(byte[] desBuf)
        {
            List<byte> lstBuf = new List<byte>();

            int adr;
            int sampleDataBlockSize = desBuf[0x100] + desBuf[0x101] * 0x100;
            int sampleDataBlockAddr = 0x104;
            adr = sampleDataBlockAddr + sampleDataBlockSize * 256;
            int musicDataBlockSize = desBuf[adr] + desBuf[adr + 1] * 0x100 + desBuf[adr + 2] * 0x100_00 + desBuf[adr + 3] * 0x100_00_00;
            int musicDataBlockAddr = sampleDataBlockAddr + sampleDataBlockSize * 256 + 4;
            int gd3InfoStartAddr = musicDataBlockAddr + musicDataBlockSize;
            //int dumcnt = 0;

            for (int i = 0; i < desBuf.Length;)
            {

                byte od = desBuf[i];
                if (i < musicDataBlockAddr || i >= gd3InfoStartAddr)
                {
                    if (i == gd3InfoStartAddr)
                    {
                        int newGd3InfoStartAddr = lstBuf.Count;
                        int newMusicDataBlockSize = newGd3InfoStartAddr - musicDataBlockAddr;
                        lstBuf[adr] = (byte)newMusicDataBlockSize;
                        lstBuf[adr + 1] = (byte)(newMusicDataBlockSize >> 8);
                        lstBuf[adr + 2] = (byte)(newMusicDataBlockSize >> 16);
                        lstBuf[adr + 3] = (byte)(newMusicDataBlockSize >> 24);
                    }

                    i++;
                    lstBuf.Add(od);
                    continue;
                }

                byte L = (byte)(od & 0xf);
                byte H = (byte)(od & 0xf0);

                //dummyコマンド以外は書き込む
                if (H != 0x60) lstBuf.Add(od);

                i++;
                switch (H)
                {
                    case 0x00://waitコマンド
                        //Console.WriteLine("Wait command {0:x} adr:{1:x}", H | L, i - 1);
                        break;
                    case 0x10://DCSGコマンド
                        //Console.WriteLine("DCSG command {0:x} adr:{1:x}", H | L, i - 1);
                        for (int x = 0; x < L + 1; x++) lstBuf.Add(desBuf[i++]);
                        break;
                    case 0x20://OPN2 port0
                    case 0x30://OPN2 port1
                        //Console.WriteLine("OPN2 p01 command {0:x} adr:{1:x}", H | L, i - 1);
                        for (int x = 0; x < L + 1; x++)
                        {
                            lstBuf.Add(desBuf[i++]);
                            lstBuf.Add(desBuf[i++]);
                        }
                        break;
                    case 0x40://OPN2 KeyONコマンド
                        //Console.WriteLine("OPN2 keyon command {0:x} adr:{1:x}", H | L, i - 1);
                        for (int x = 0; x < L + 1; x++) lstBuf.Add(desBuf[i++]);
                        break;
                    case 0x50://OPN2 PCMコマンド
                        //Console.WriteLine("OPN2 pcm command {0:x} adr:{1:x}", H | L, i - 1);
                        lstBuf.Add(desBuf[i++]);
                        break;
                    //case 0x60://dummyChipコマンド　(第2引数：chipID 第３引数:isSecondary)
                    //    //TODO: Dummy Command
                    //    //Console.WriteLine("dummy command {0:x} adr:{1:x}", H | L, i - 1);
                    //    if (Common.CheckDummyCommand(od.type))//ここで指定できるmmlコマンドは元々はChipに送信することのないコマンドのみ(さもないと、通常のコマンドのデータと見分けがつかなくなる可能性がある)
                    //    {
                    //        //lstBuf.Add(desBuf[i++].val);
                    //        //lstBuf.Add(desBuf[i++].val);
                    //        i += 2;
                    //        dumcnt += 3;
                    //    }
                    //    break;
                    case 0x70:
                        if (L == 0xe)//loop
                        {
                            Console.WriteLine("loop command {0:x} adr:{1:x}", H | L, i - 1);
                            if (desVGM.loopOffset != -1)
                            {
                                lstBuf.Add((byte)desVGM.loopOffset);
                                lstBuf.Add((byte)(desVGM.loopOffset >> 8));
                                lstBuf.Add((byte)(desVGM.loopOffset >> 16));
                                i += 3;
                            }
                        }
                        else if (L == 0xf)//end
                        {
                            //Console.WriteLine("end command {0:x} adr:{1:x}", H | L, i - 1);
                        }
                        break;
                    default:
                        Console.WriteLine("Warning Unkown command {0:x} adr:{1:x}", H | L, i - 1);
                        break;
                }
            }

            byte[] bufs = lstBuf.ToArray();
            outFile(bufs);
        }

        private void outFile(byte[] desBuf)
        {
            if (Path.GetExtension(desFn).ToLower() != ".vgz")
            {
                if (desVGM.info.format == enmFormat.VGM)
                {
                    log.Write("VGMファイル出力");
                    File.WriteAllBytes(
                        desFn
                        , desBuf);
                }
                else
                {
                    log.Write("XGMファイル出力");
                    File.WriteAllBytes(
                        Path.Combine(
                            Path.GetDirectoryName(desFn)
                            , Path.GetFileNameWithoutExtension(desFn) + ".xgm"
                            )
                        , desBuf);
                }
                return;
            }

            log.Write("VGZファイル出力");

            int num;
            byte[] buf = new byte[1024];

            MemoryStream inStream = new MemoryStream(desBuf);
            FileStream outStream = new FileStream(desFn, FileMode.Create);
            GZipStream compStream = new GZipStream(outStream, CompressionMode.Compress);

            try
            {
                while ((num = inStream.Read(buf, 0, buf.Length)) > 0)
                {
                    compStream.Write(buf, 0, num);
                }
            }
            catch { }
            finally
            {
                if (compStream != null) compStream.Dispose();
                if (outStream != null) outStream.Dispose();
                if (inStream != null) inStream.Dispose();
            }
        }


        private List<Line> GetSrc(string[] srcBuf, string path)
        {
            List<Line> src = new List<Line>();
            int ln = 1;
            foreach (string s in srcBuf)
            {
                if (!string.IsNullOrEmpty(s)
                    && s.TrimStart().Length > 2
                    && s.TrimStart().Substring(0, 2) == "'+")
                {
                    string includeFn = s.Substring(2).Trim().Trim('"');
                    if (!File.Exists(includeFn))
                    {
                        includeFn = Path.Combine(path, includeFn);
                        if (!File.Exists(includeFn))
                        {
                            msgBox.setErrMsg(string.Format(
                                msg.get("E04006")
                                , includeFn));
                            return null;
                        }
                    }
                    string[] incBuf = File.ReadAllLines(includeFn);
                    int iln = 1;
                    foreach (string i in incBuf)
                    {
                        Line iline = new Line(includeFn, iln, i);
                        src.Add(iline);
                        iln++;
                    }

                    ln++;
                    continue;
                }

                Line line = new Line(srcFn, ln, s);
                src.Add(line);
                ln++;
            }

            return src;
        }

        private void GetPCMData(string path)
        {
            Dictionary<int, clsPcm> newDic = new Dictionary<int, clsPcm>();
            foreach (clsPcmDatSeq pds in desVGM.instPCMDatSeq)
            {
                byte[] buf;
                clsPcm v;
                bool isRaw;
                bool is16bit;
                int samplerate;

                if (pds.chip == enmChipType.None) continue;

                switch (pds.type)
                {
                    case enmPcmDefineType.Easy:
                        if (desVGM.instPCM.ContainsKey(pds.No))
                        {
                            desVGM.instPCM.Remove(pds.No);
                        }
                        v = new clsPcm(
                            pds.No
                            , pcmDataSeqNum++
                            , pds.chip
                            , pds.isSecondary
                            , pds.FileName
                            , pds.BaseFreq
                            , pds.Volume
                            , 0
                            , 0
                            , 0
                            , pds.DatLoopAdr
                            , false
                            , desVGM.info.format == enmFormat.VGM ? 8000 : 14000);
                        desVGM.instPCM.Add(pds.No, v);

                        //ファイルの読み込み
                        buf = Common.GetPCMDataFromFile(path, v, out isRaw, out is16bit, out samplerate);
                        if (buf == null)
                        {
                            //msgBox.setErrMsg(string.Format(
                            //    msg.get("E04007")
                            //    , v.fileName));
                            continue;
                        }

                        if (desVGM.info.format == enmFormat.XGM && v.isSecondary)
                        {
                            msgBox.setErrMsg(string.Format(
                                msg.get("E01017")
                                , v.fileName));
                            continue;
                        }

                        desVGM.chips[v.chip][v.isSecondary ? 1 : 0]
                            .StorePcm(
                            newDic
                            , new KeyValuePair<int, clsPcm>(pds.No, v)
                            , buf
                            , is16bit
                            , samplerate);

                        break;
                    case enmPcmDefineType.Mucom88:
                        if (desVGM.instPCM.ContainsKey(pds.No))
                        {
                            desVGM.instPCM.Remove(pds.No);
                        }
                        v = new clsPcm(
                            pds.No
                            , pcmDataSeqNum++
                            , pds.chip
                            , pds.isSecondary
                            , pds.FileName
                            , pds.BaseFreq
                            , pds.Volume
                            , 0
                            , 0
                            , 0
                            , pds.DatLoopAdr
                            , false
                            , desVGM.info.format == enmFormat.VGM ? 8000 : 14000);
                        desVGM.instPCM.Add(pds.No, v);

                        mucomADPCM2PCM.mucomPCMInfo info = null;
                        for (int i = 0; i < mucomADPCM2PCM.lstMucomPCMInfo.Count; i++)
                        {
                            mucomADPCM2PCM.mucomPCMInfo inf = mucomADPCM2PCM.lstMucomPCMInfo[i];
                            if (pds.No == inf.no)
                            {
                                info = inf;
                                break;
                            }
                        }
                        if (info == null) return;

                        //ファイルの読み込み
                        buf = mucomADPCM2PCM.GetPcmData(info, desVGM.info.PcmVolume);
                        if (buf == null)
                        {
                            msgBox.setErrMsg(string.Format(
                                msg.get("E04007")
                                , v.fileName));
                            continue;
                        }

                        if (desVGM.info.format == enmFormat.XGM && v.isSecondary)
                        {
                            msgBox.setErrMsg(string.Format(
                                msg.get("E01017")
                                , v.fileName));
                            continue;
                        }

                        desVGM.chips[v.chip][v.isSecondary ? 1 : 0]
                            .StorePcm(
                            newDic
                            , new KeyValuePair<int, clsPcm>(pds.No, v)
                            , buf
                            , false
                            , desVGM.info.format == enmFormat.VGM ? 8000 : 14000
                            );

                        break;
                    case enmPcmDefineType.RawData:
                        //ファイルの読み込み
                        buf = Common.GetPCMDataFromFile(path, pds.FileName, 100, out isRaw, out is16bit, out samplerate);
                        if (buf == null)
                        {
                            msgBox.setErrMsg(string.Format(
                                msg.get("E04007")
                                , pds.FileName));
                            continue;
                        }
                        desVGM.chips[pds.chip][pds.isSecondary ? 1 : 0]
                            .StorePcmRawData(
                            pds
                            , buf
                            , isRaw
                            , is16bit
                            , samplerate);
                        break;
                    case enmPcmDefineType.Set:
                        //if(!desVGM.chips[pds.chip][pds.isSecondary ? 1 : 0].StorePcmCheck())
                        //{
                        //    return;
                        //}
                        if (desVGM.instPCM.ContainsKey(pds.No))
                        {
                            desVGM.instPCM.Remove(pds.No);
                        }
                        v = new clsPcm(
                            pds.No
                            , pcmDataSeqNum++
                            , pds.chip
                            , pds.isSecondary
                            , ""
                            , pds.BaseFreq
                            , 100
                            , pds.DatStartAdr
                            , pds.DatEndAdr
                            , pds.chip != enmChipType.RF5C164
                                ? (pds.DatEndAdr - pds.DatStartAdr + 1)
                                : (pds.DatLoopAdr - pds.DatStartAdr + 1)
                            , pds.DatLoopAdr
                            , false
                            , 8000
                            , pds.Option);
                        newDic.Add(pds.No, v);

                        break;
                }
            }

            desVGM.instPCM = newDic;

        }

        private void Result()
        {
            Disp("");

            string res = "";
            foreach (ClsChip[] chips in desVGM.chips.Values)
            {
                foreach (ClsChip chip in chips)
                {
                    res += DispPCMRegion(chip);
                }
            }

            if (res != "")
            {
                Disp(msg.get("I04012"));
                Disp("");
                Disp(msg.get("I04013"));
                Disp("");
                Disp("");
                Disp("");

                Disp(msg.get("I04016"));
                Disp(msg.get("I04017"));
                Disp(res);
            }



            res = "";
            foreach (ClsChip[] chips in desVGM.chips.Values)
            {
                foreach (ClsChip chip in chips)
                {
                    res += DispPCMRegionDataBlock(chip);
                }
            }

            if (res != "")
            {
                Disp(msg.get("I04019"));
                Disp(msg.get("I04020"));
                Disp(res);
            }
        }

        private string DispPCMRegion(ClsChip c)
        {
                if (c.pcmDataEasy == null && c.pcmDataDirect.Count == 0) return "";

            string region = "";

            for (int i = 0; i < 256; i++)
            {
                if (!desVGM.instPCM.ContainsKey(i)) continue;
                if (desVGM.instPCM[i].chip != c.chipType) continue;
                if (desVGM.instPCM[i].isSecondary != c.IsSecondary) continue;

                region += c.DispRegion(desVGM.instPCM[i]);

            }

            long tl = 0;
            foreach (int i in desVGM.instPCM.Keys)
            {
                tl += desVGM.instPCM[i].size;
            }
            region += (string.Format(msg.get("I04018"), tl));
            region += "\r\n";

            return region;
        }

        private string DispPCMRegionDataBlock(ClsChip c)
        {
            string region = "";
            long tl = 0;

                if (c.pcmDataEasy == null && c.pcmDataDirect.Count == 0) return "";

                if (c.pcmDataEasy != null)
                {
                    region += string.Format("{0,-10} {1,-7} ${2,-7:X6} ${3,-7:X6} ${4,-7:X6}  {5}\r\n"
                        , c.Name
                        , c.IsSecondary ? "SEC" : "PRI"
                        , 0
                        , c.pcmDataEasy.Length - 1
                        , c.pcmDataEasy.Length
                        , "AUTO"
                        );
                    tl += c.pcmDataEasy.Length;
                }


            if (c.pcmDataDirect.Count > 0)
            {
                foreach (clsPcmDatSeq pds in desVGM.instPCMDatSeq)
                {
                    if (pds.type == enmPcmDefineType.Set) continue;
                    if (pds.type == enmPcmDefineType.Easy) continue;
                    if (pds.chip != c.chipType) continue;
                    if (pds.isSecondary != c.IsSecondary) continue;
                    if (!desVGM.chips[pds.chip][0].CanUsePICommand()) continue;

                    region += string.Format("{0,-10} {1,-7} ${2,-7:X6} ${3,-7:X6} ${4,-7:X6}  {5}\r\n"
                        , c.Name
                        , pds.isSecondary ? "SEC" : "PRI"
                        , pds.DatStartAdr
                        , pds.DatEndAdr
                        , pds.DatEndAdr - pds.DatStartAdr + 1
                        , pds.type == enmPcmDefineType.Easy ? "AUTO" : "MANUAL"
                        );
                    tl += pds.DatEndAdr - pds.DatStartAdr + 1;
                }
            }

            if (region != "")
            {
                region += (string.Format(msg.get("I04018"), tl));
                region += "\r\n";
            }

            return region;
        }



    }


}