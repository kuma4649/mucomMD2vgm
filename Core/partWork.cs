﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class partWork
    {

        /// <summary>
        /// パートデータ
        /// </summary>
        public List<Line> pData = null;

        /// <summary>
        /// エイリアスデータ
        /// </summary>
        public Dictionary<int, Line> aData = null;

        /// <summary>
        /// mmlデータ
        /// </summary>
        public List<MML> mmlData = null;

        public int mmlPos = 0;

        /// <summary>
        /// データが最後まで演奏されたかどうかを示す(注意:trueでも演奏が終わったとは限らない)
        /// </summary>
        public bool dataEnd = false;

        /// <summary>
        /// 次に演奏されるデータの位置
        /// </summary>
        private clsPos pos = new clsPos();

        /// <summary>
        /// 位置情報のスタック
        /// </summary>
        private Stack<clsPos> stackPos = new Stack<clsPos>();

        /// <summary>
        /// リピート位置情報のスタック
        /// </summary>
        public Stack<clsRepeat> stackRepeat = new Stack<clsRepeat>();

        /// <summary>
        /// 連符位置情報のスタック
        /// </summary>
        public Stack<clsRenpu> stackRenpu = new Stack<clsRenpu>();

        /// <summary>
        /// パートごとの音源の種類
        /// </summary>
        public ClsChip chip = null;

        /// <summary>
        /// Secondary Chipか
        /// </summary>
        public bool isSecondary = false;

        /// <summary>
        /// 割り当てられた音源のチャンネル番号
        /// </summary>
        public int ch = 0;

        /// <summary>
        /// 未加工のf-num
        /// </summary>
        public int freq = 0;

        public int beforeFNum = -1;
        public int FNum = -1;

        /// <summary>
        /// Cコマンドの値を保持
        /// </summary>
        public int clock = 128;

        /// <summary>
        /// いままで演奏した総クロック数
        /// </summary>
        public long clockCounter = 0L;
        public long inspectedClockCounter = -1;

        ///// <summary>
        ///// Lコマンド使用フラグ
        ///// </summary>
        //public bool LSwitch = false;

        ///// <summary>
        ///// Lコマンドの位置
        ///// </summary>
        //public long LClock = 0;

        ///// <summary>
        ///// Lコマンド後の長さ
        ///// </summary>
        //public long LLength = 0;

        /// <summary>
        /// パート全体の長さ
        /// </summary>
        public long totalSamples = 0;

        /// <summary>
        /// あとどれだけ待機するかを示すカウンター(clock)
        /// </summary>
        public long waitCounter = 0L;

        /// <summary>
        /// キーオフコマンドを発行するまであとどれだけ待機するかを示すカウンター(clock)
        /// (waitCounterよりも大きい場合キーオフされない)
        /// </summary>
        public long waitKeyOnCounter = 0L;

        /// <summary>
        /// lコマンドで設定されている音符の長さ(clock)
        /// </summary>
        public long length = 24;

        /// <summary>
        /// oコマンドで設定されているオクターブ数
        /// </summary>
        public int octaveNow = 4;

        public int octaveNew = 4;

        public int latestOctave = 0;

        public int TdA = -1;
        public int op1ml = -1;
        public int op2ml = -1;
        public int op3ml = -1;
        public int op4ml = -1;
        public int op1dt2 = -1;
        public int op2dt2 = -1;
        public int op3dt2 = -1;
        public int op4dt2 = -1;
        public int toneDoubler = 0;
        public int toneDoublerKeyShift = 0;

        /// <summary>
        /// vコマンドで設定されている音量
        /// </summary>
        public int volume = 0;

        public int latestVolume = 0;

        /// <summary>
        /// 簡易ボリューム
        /// </summary>
        public int volumeEasy = 0;

        /// <summary>
        /// 相対音量調整
        /// </summary>
        public int RelVolume = 0;

        public int shuffle = 0;

        public int shuffleDirection = 1;

        public bool restMode = false;

        /// <summary>
        /// pコマンドで設定されている音の定位(1:R 2:L 3:C)
        /// </summary>
        //public int pan = 3;
        //public int beforePan = -1;
        public dint pan = new dint(3);

        /// <summary>
        /// 拡張パン(Left)
        /// </summary>
        /// <remarks>
        /// ボリュームが左右別管理の音源向け
        /// </remarks>
        public int panL = -1;

        /// <summary>
        /// 拡張パン(Right)
        /// </summary>
        /// <remarks>
        /// ボリュームが左右別管理の音源向け
        /// </remarks>
        public int panR = -1;

        /// <summary>
        /// 拡張ボリューム(Left)before
        /// </summary>
        /// <remarks>
        /// ボリュームが左右別管理の音源向け
        /// </remarks>
        public int beforeLVolume = -1;

        /// <summary>
        /// 拡張ボリューム(Right)before
        /// </summary>
        /// <remarks>
        /// ボリュームが左右別管理の音源向け
        /// </remarks>
        public int beforeRVolume = -1;

        /// <summary>
        /// @コマンドで設定されている音色
        /// </summary>
        public int instrument = -1;
        public int beforeInstrument = -2;

        /// <summary>
        /// OPLLのサスティン
        /// </summary>
        public bool sus = false;

        /// <summary>
        /// 使用中のエンベロープ定義番号
        /// </summary>
        public int envInstrument = -1;
        public int beforeEnvInstrument = 0;

        /// <summary>
        /// エンベロープの進捗位置
        /// </summary>
        public int envIndex = -1;

        /// <summary>
        /// エンベロープ向け汎用カウンター
        /// </summary>
        public int envCounter = -1;

        /// <summary>
        /// エンベロープ音量
        /// </summary>
        public int envVolume = -1;

        /// <summary>
        /// 使用中のエンベロープの定義
        /// </summary>
        public int[] envelope = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, -1 };

        /// <summary>
        /// エンベロープスイッチ
        /// </summary>
        public bool envelopeMode = false;

        /// <summary>
        /// リズムモードスイッチ
        /// </summary>
        public bool rhythmMode = false;

        /// <summary>
        /// Dコマンドで設定されているデチューン
        /// </summary>
        public int detune = 0;

        /// <summary>
        /// 発音される音程
        /// </summary>
        public char noteCmd = (char)0;//'c';

        /// <summary>
        /// 音程をずらす量
        /// </summary>
        public int shift = 0;

        /// <summary>
        /// PCMの音程
        /// </summary>
        public int pcmNote = 0;
        public int pcmOctave = 0;

        /// <summary>
        /// mコマンドで設定されているpcmモード(true:PCM false:FM)
        /// </summary>
        public bool pcm = false;

        /// <summary>
        /// PCM マッピングモードスイッチ
        /// </summary>
        public bool isPcmMap = false;

        public float pcmBaseFreqPerFreq = 0.0f;
        public float pcmFreqCountBuffer = 0.0f;
        public long pcmWaitKeyOnCounter = 0L;
        public long pcmSizeCounter = 0L;

        public bool streamSetup = false;
        public int streamID = -1;
        public long streamFreq = 0;


        /// <summary>
        /// q/Qコマンドで設定されているゲートタイム(clock/%)
        /// </summary>
        public int gatetime = 0;

        /// <summary>
        /// q/Qコマンドで最後に指定されたのはQコマンドかどうか
        /// </summary>
        public bool gatetimePmode = false;

        /// <summary>
        /// 使用する現在のスロット
        /// </summary>
        public byte slots = 0xf;

        /// <summary>
        /// 4OP(通常)時に使用するスロット
        /// </summary>
        public byte slots4OP = 0xf;

        /// <summary>
        /// EX時に使用するスロット
        /// </summary>
        public byte slotsEX = 0x0;

        /// <summary>
        /// タイ
        /// </summary>
        public bool tie = false;

        /// <summary>
        /// 前回発音時にタイ指定があったかどうか
        /// </summary>
        public bool beforeTie = false;

        public bool keyOn = false;
        public bool keyOff = false;
        public int rhythmKeyOnData = -1;

        /// <summary>
        /// 前回発音時の音量
        /// </summary>
        public int beforeVolume = -1;

        /// <summary>
        /// 効果音モード
        /// </summary>
        public bool Ch3SpecialMode = false;

        /// <summary>
        /// KeyOnフラグ
        /// </summary>
        public bool Ch3SpecialModeKeyOn = false;

        public bool HardEnvelopeSw = false;
        public int HardEnvelopeType = -1;
        public int HardEnvelopeSpeed = -1;

        /// <summary>
        /// Lfo(1つ)
        /// </summary>
        public clsLfo[] lfo = new clsLfo[1] { new clsLfo() };// new clsLfo[4] { new clsLfo(), new clsLfo(), new clsLfo(), new clsLfo() };

        /// <summary>
        /// エコー
        /// </summary>
        public clsEcho echo = new clsEcho();
        public int echo_PortaCounter = 0;
        public int echoBackStep = 1;
        public int echoDownVolume = 0;

        public bool mPortaSW = false;

        /// <summary>
        /// ベンド中のoコマンドで設定されているオクターブ数
        /// </summary>
        public int bendOctave = 4;

        /// <summary>
        /// ベンド中の音程
        /// </summary>
        public char bendNote = 'r';

        /// <summary>
        /// ベンド中の待機カウンター
        /// </summary>
        public long bendWaitCounter = -1;

        /// <summary>
        /// ベンド中に参照される周波数スタックリスト
        /// </summary>
        public Stack<Tuple<int, int>> bendList = new Stack<Tuple<int, int>>();

        /// <summary>
        /// ベンド中の発音周波数
        /// </summary>
        public int bendFnum = 0;

        /// <summary>
        /// ベンド中に音程をずらす量
        /// </summary>
        public int bendShift = 0;

        /// <summary>
        /// スロットごとのディチューン値
        /// </summary>
        public int[] slotDetune = new int[] { 0, 0, 0, 0 };

        public int[] slotFreq = new int[] { -1, -1, -1, -1 };

        /// <summary>
        /// ノイズモード値
        /// </summary>
        public int noise = 0;

        /// <summary>
        /// SSG Noise or Tone mixer 0:Silent 1:Tone 2:Noise 3:Tone&Noise
        /// OPM Noise 0:Disable 1:Enable
        /// </summary>
        public int mixer = 1;

        /// <summary>
        /// キーシフト
        /// </summary>
        public int keyShift = 0;
        public int relKeyShift = 0;

        public string PartName = "";

        public int rf5c164AddressIncrement = -1;
        public int rf5c164Envelope = -1;
        public int rf5c164Pan = -1;

        public int huc6280Envelope = -1;
        public int huc6280Pan = -1;

        public int pcmStartAddress = -1;
        public int beforepcmStartAddress = -1;
        public int pcmLoopAddress = -1;
        public int beforepcmLoopAddress = -1;
        public int pcmEndAddress = -1;
        public int beforepcmEndAddress = -1;
        public int pcmBank = 0;
        public int beforepcmBank = -1;

        public enmChannelType Type;
        public int MaxVolume = 0;
        public int MaxVolumeEasy = 0;
        public byte port0 = 0;
        public byte port1 = 0;
        public int ams = 0;
        public int fms = 0;
        public int pms = 0;
        public bool hardLfoSw = false;
        public int hardLfoNum = 0;

        public int hardLfoFreq = 0;
        public int hardLfoPMD = 0;
        public int hardLfoAMD = 0;

        public bool reqFreqReset = false;
        public bool reqKeyOffReset = false;

        public bool renpuFlg = false;
        public List<int> lstRenpuLength = null;

        public int ReverbValue = 0;
        public bool ReverbSwitch = false;
        public bool ReverbNowSwitch = false;
        public int ReverbMode = 0;

        public clsLoopInfo loopInfo = new clsLoopInfo();

        private List<byte> dataBuf = new List<byte>();

        public int pcmMapNo { get; set; } = 0;
        public int ipan { get; internal set; }
        public bool isOp4Mode { get; internal set; }

        public void OutData(params byte[] data)
        {
            foreach (byte b in data)
            {
                Log.Write(string.Format("name:{0} channel:{1} data:{2:x2}", chip.Name, ch, b));
                dataBuf.Add(b);
            }
        }
        public byte[] GetData()
        {
            return dataBuf.ToArray();
        }
        public void Flash()
        {
            dataBuf.Clear();
        }


        /// <summary>
        /// パート情報をリセットする
        /// </summary>
        public void resetPos()
        {
            pos = new clsPos();
            stackPos = new Stack<clsPos>();
            setPos(0);
        }

        /// <summary>
        /// 解析位置を取得する
        /// </summary>
        /// <returns></returns>
        public int getPos()
        {
            return pos.tCol;
        }

        public Line getLine()
        {
            if (pos.alies == null)
            {
                return pData[pos.row];
            }
            return aData[(int)pos.alies];
        }

        /// <summary>
        /// 解析位置に対するソースファイル上の行数を得る
        /// </summary>
        /// <returns></returns>
        public int getLineNumber()
        {
            if (pos.alies == null)
            {
                return pData[pos.row].Num;
            }
            return aData[(int)pos.alies].Num;
        }

        /// <summary>
        /// 解析位置に対するソースファイル名を得る
        /// </summary>
        /// <returns></returns>
        public string getSrcFn()
        {
            if (pos.alies == null)
            {
                return pData[pos.row].Fn;
            }
            return aData[(int)pos.alies].Fn;
        }

        /// <summary>
        /// 解析位置の文字を取得する
        /// </summary>
        /// <returns></returns>
        public char getChar()
        {
            //if (dataEnd) return (char)0;

            char ch;
            if (pos.alies == null)
            {
                if (pData[pos.row].Txt.Length <= pos.col)
                {
                    return (char)0;
                }
                ch = pData[pos.row].Txt[pos.col];
            }
            else
            {
                if (aData[(int)pos.alies].Txt.Length <= pos.col)
                {
                    return (char)0;
                }
                ch = aData[(int)pos.alies].Txt[pos.col];
            }
            //Console.Write(ch);
            return ch;
        }

        /// <summary>
        /// 解析位置を一つ進める(重い！)
        /// </summary>
        public void incPos()
        {
            setPos(pos.tCol + 1);
        }

        /// <summary>
        /// 解析位置を一つ戻す(重い！)
        /// </summary>
        public void decPos()
        {
            setPos(pos.tCol - 1);
        }

        /// <summary>
        /// 指定された文字数だけ読み出し、文字列を生成する
        /// </summary>
        /// <param name="len">文字数</param>
        /// <returns>文字列</returns>
        public string getString(int len)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                sb.Append(getChar());
                incPos();
            }
            return sb.ToString();
        }

        /// <summary>
        /// "でくくられた範囲を読み出し、文字列を生成する
        /// </summary>
        /// <returns></returns>
        public string getString()
        {
            incPos();
            StringBuilder sb = new StringBuilder();
            while (getChar() != '"')
            {
                sb.Append(getChar());
                incPos();
            }
            incPos();
            return sb.ToString();
        }

        /// <summary>
        /// 解析位置を指定する
        /// </summary>
        /// <param name="tCol">解析位置</param>
        public void setPos(int tCol)
        {
            if (pData == null)
            {
                return;
            }

            if (LstPos == null) MakeLstPos();

            int i = 0;
            while (i != LstPos.Count && tCol >= LstPos[i].tCol)
            {
                i++;
            }

            pos.tCol = tCol;
            pos.alies = LstPos[i - 1].alies;
            pos.col = LstPos[i - 1].col + tCol - LstPos[i - 1].tCol;
            pos.row = LstPos[i - 1].row;
            return;

        }

        private List<clsPos> LstPos = null;
        internal int beforeTLOP1=-1;
        internal int beforeTLOP3 = -1;
        internal int beforeTLOP2 = -1;
        internal int beforeTLOP4 = -1;
        public bool beforeKeyOff=false;

        //音色グラデーション
        public bool instrumentGradationSwitch = false;
        public int instrumentGradationWait = 0;
        public long instrumentGradationWaitCounter = 0;
        public int instrumentGradationPointer = 0;
        public int[] instrumentGradationSt = new int[50];
        public int[] instrumentGradationEd = new int[50];
        public int instrumentGradationStNum = 0;
        public int instrumentGradationEdNum = 0;
        public int[] instrumentGradationWk = new int[50];
        public bool[] instrumentGradationFlg = new bool[50];
        public bool instrumentGradationReset = true;
        public int feedback;
        public int algo;
        public byte[] v_tl = new byte[] { 0, 0, 0, 0 };

        public void MakeLstPos()
        {
            if (pData == null)
            {
                return;
            }

            int tCol = 0;
            int row = 0;
            int col = 0;
            int? aliesName = null;

            LstPos = new List<clsPos>();
            LstPos.Add(new clsPos());
            resetPos();

            while (true)
            {
                string data;
                char ch;

                //読みだすデータの頭出し
                if (aliesName == null)
                {
                    if (pData.Count == row)
                    {
                        return;
                    }
                    data = pData[row].Txt;
                }
                else
                {
                    data = aData[(int)aliesName].Txt;
                }

                //解析行の解析位置が終端に達したときの処理
                while (data.Length == col)
                {
                    if (aliesName == null)
                    {
                        row++;
                        if (pData.Count == row)
                        {
                            break;
                        }
                        else
                        {
                            data = pData[row].Txt;
                            col = 0;

                            clsPos p = new clsPos();
                            p.tCol = tCol;
                            p.alies = null;
                            p.col = 0;
                            p.row = row;
                            LstPos.Add(p);

                            break;
                        }
                    }
                    else
                    {
                        clsPos p = stackPos.Pop();
                        aliesName = p.alies;
                        col = p.col;
                        row = p.row;
                        if (aliesName == null)
                        {
                            data = pData[row].Txt;
                        }
                        else
                        {
                            data = aData[(int)aliesName].Txt;
                        }

                        p.tCol = tCol;
                        LstPos.Add(p);
                    }
                }

                ch = data[col];

                //解析位置でエイリアス指定されている場合
                while (ch == '*')
                {
                    int len = 0;
                    int? a = getAliesName(data, col,ref len);
                    if (a != null)
                    {
                        clsPos p = new clsPos();
                        p.alies = aliesName;
                        p.col = col + len + 1;
                        p.row = row;
                        stackPos.Push(p);

                        data = aData[(int)a].Txt;
                        col = 0;
                        aliesName = a;
                        row = 0;

                        p = new clsPos();
                        p.tCol = tCol;
                        p.alies = a;
                        p.col = 0;
                        p.row = 0;
                        LstPos.Add(p);
                    }
                    else
                    {
                        msgBox.setWrnMsg(msg.get("E06000")
                            , (aliesName == null) ? pData[row].Fn : aData[(int)aliesName].Fn
                            , (aliesName == null) ? pData[row].Num : aData[(int)aliesName].Num
                            );
                        col++;
                    }

                    ch = data[col];
                }

                tCol++;
                col++;
                //解析行の解析位置が終端に達したときの処理
                while (data.Length == col)
                {
                    if (aliesName == null)
                    {
                        row++;
                        if (pData.Count == row)
                        {
                            break;
                        }
                        else
                        {
                            data = pData[row].Txt;
                            col = 0;

                            clsPos p = new clsPos();
                            p.tCol = tCol;
                            p.alies = null;
                            p.col = 0;
                            p.row = row;
                            LstPos.Add(p);

                            break;
                        }
                    }
                    else
                    {
                        clsPos p = stackPos.Pop();
                        aliesName = p.alies;
                        col = p.col;
                        row = p.row;
                        if (aliesName == null)
                        {
                            data = pData[row].Txt;
                        }
                        else
                        {
                            data = aData[(int)aliesName].Txt;
                        }

                        p.tCol = tCol;
                        LstPos.Add(p);
                    }
                }

            }

        }

        /// <summary>
        /// 解析位置から数値を取得する。
        /// </summary>
        /// <param name="num">取得した数値が返却される</param>
        /// <returns>数値取得成功したかどうか</returns>
        public bool getNum(out int num)
        {

            string n = "";
            int ret = -1;

            //タブと空白は読み飛ばす
            while (getChar() == ' ' || getChar() == '\t')
            {
                incPos();
            }

            //符号を取得する(ない場合は正とする)
            if (getChar() == '-' || getChar() == '+')
            {
                n = getChar().ToString();
                incPos();
            }

            //タブと空白は読み飛ばす
            while (getChar() == ' ' || getChar() == '\t')
            {
                incPos();
            }

            //１６進数指定されているか
            if (getChar() != '$')
            {
                //数字でなくなるまで取得
                while (true)
                {
                    if (getChar() >= '0' && getChar() <= '9')
                    {
                        try
                        {
                            n += getChar();
                            incPos();
                        }
                        catch
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                //数値に変換できたら成功
                if (!int.TryParse(n, out ret))
                {
                    num = -1;
                    return false;
                }

                num = ret;
            }
            else
            {
                incPos();

                while (true)
                {
                    if ((getChar() >= '0' && getChar() <= '9')
                        || (getChar() >= 'a' && getChar() <= 'f')
                        || (getChar() >= 'A' && getChar() <= 'F'))
                    {
                        try
                        {
                            n += getChar();
                            incPos();
                        }
                        catch
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                //数値に変換できたら成功
                try
                {
                    num = Convert.ToInt32(n, 16);
                }
                catch
                {
                    num = -1;
                    return false;
                }
            }

            return true;
        }

        public bool getNumNoteLength(out int num, out bool flg)//,bool tieflg=false)
        {

            flg = false;
            bool sptab = false;
            //タブと空白は読み飛ばす
            while (getChar() == ' ' || getChar() == '\t')
            {
                sptab = true;
                incPos();
            }

            //クロック直接指定
            if (getChar() == '%')
            {
                //if (tieflg)
                //{
                //    num = -2;
                //    flg = false;
                //    return false;
                //}
                if (sptab)
                {
                    num = -1;
                    flg = false;
                    return false;
                }
                flg = true;
                incPos();
            }

            return getNum(out num);
        }

        /// <summary>
        /// エイリアス名を取得する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private int? getAliesName(string data, int col, ref int len)
        {
            if (data.Length <= col + 1)
            {
                return null;
            }

            int? wrd = Common.GetNumsFromString(data, col + 1, ref len);
            if (wrd == null) return null;

            //エイリアス集に存在しない場合はnull
            if (!aData.ContainsKey((int)wrd))
            {
                return null;
            }

            return wrd;
        }

        public void skipSpaceOrTab()
        {
            while (true)
            {
                char c = getChar();
                if ((c != ' ' && c != '\t') || c == 0) break;
                incPos();
            }
        }
    }

    public class clsPos
    {

        /// <summary>
        /// すべてのデータ行を１行としたときの次に演奏されるデータの何桁目か
        /// </summary>
        public int tCol = 0;

        /// <summary>
        /// 次に演奏されるデータの何行目か
        /// </summary>
        public int row = 0;

        /// <summary>
        /// 次に演奏されるデータの何桁目か
        /// </summary>
        public int col = 0;

        /// <summary>
        /// 次に演奏されるデータのエイリアス名
        /// </summary>
        public int? alies = null;
    }

    public class clsRepeat
    {
        /// <summary>
        /// 位置
        /// </summary>
        public int pos = 0;

        /// <summary>
        /// リピート向け回数
        /// </summary>
        public int repeatCount = 2;

    }

    public class clsRenpu
    {
        /// <summary>
        /// 位置
        /// </summary>
        public int pos = 0;

        /// <summary>
        /// リピートのスタック数
        /// </summary>
        public int repeatStackCount = 0;

        /// <summary>
        /// ノートの数
        /// </summary>
        public int noteCount = 0;

        public MML mml;

        public List<int> lstRenpuLength;
    }

    public class clsLfo
    {

        /// <summary>
        /// Lfoの種類
        /// </summary>
        public enmLfoType type = enmLfoType.unknown;

        /// <summary>
        /// Lfoの設定値
        /// </summary>
        public int[] param = new int[4];

        /// <summary>
        /// Lfoのスイッチ
        /// </summary>
        public bool sw = false;

        /// <summary>
        /// Lfoが完了したかどうか
        /// </summary>
        public bool isEnd = false;

        /// <summary>
        /// Lfoの待機カウンター
        /// </summary>
        public long waitCounter = 0;

        public long PeakLevelCounter = 0;

        /// <summary>
        /// Lfoの変化値
        /// </summary>
        public int value = 0;

        /// <summary>
        /// Lfoの変化する方向
        /// </summary>
        public int direction = 0;

    }

    public class clsPcm
    {
        public enmChipType chip = enmChipType.YM2612;
        public bool isSecondary = false;
        public int num = 0;
        public int seqNum = 0;
        public double xgmMaxSampleCount = 0;
        public string fileName = "";
        public int freq = 0;
        public int vol = 0;
        public long stAdr = 0;
        public long edAdr = 0;
        public long size = 0;
        public long loopAdr = -1;
        public bool is16bit = false;
        public int samplerate = 8000;
        public object[] option = null;
        public enmPCMSTATUS status = enmPCMSTATUS.NONE;

        public clsPcm(int num
            , int seqNum
            , enmChipType chip
            , bool isSecondary
            , string fileName
            , int freq
            , int vol
            , long stAdr
            , long edAdr
            , long size
            , long loopAdr
            , bool is16bit
            , int samplerate
            , params object[] option)
        {
            this.num = num;
            this.seqNum = seqNum;
            this.chip = chip;
            this.isSecondary = isSecondary;
            this.fileName = fileName;
            this.freq = freq;
            this.vol = vol;
            this.stAdr = stAdr;
            this.edAdr = edAdr;
            this.size = size;
            this.loopAdr = loopAdr;
            this.is16bit = is16bit;
            this.samplerate = samplerate;
            this.option = option;
            this.status = enmPCMSTATUS.NONE;
        }
    }

    public class clsPcmDatSeq
    {
        public enmPcmDefineType type;
        public int No = -1;
        public string FileName = "";
        public int BaseFreq = 8000;
        public int Volume = 100;
        public enmChipType chip = enmChipType.YM2612;
        public bool isSecondary = false;
        public object[] Option = null;
        public int SrcStartAdr = 0;
        public int DesStartAdr = 0;
        public int SrcLength = 0;
        public int DatStartAdr = 0;
        public int DatEndAdr = 0;
        public int DatLoopAdr = 0;

        public clsPcmDatSeq(
            enmPcmDefineType type
            , int No
            , string FileName
            , int BaseFreq
            , int Volume
            , enmChipType chip
            , bool isSecondary
            , int LoopAdr)
        {
            this.type = type;
            this.No = No;
            this.FileName = FileName;
            this.BaseFreq = BaseFreq;
            this.Volume = Volume;
            this.chip = chip;
            this.isSecondary = isSecondary;
            this.DatLoopAdr = LoopAdr;
        }

        public clsPcmDatSeq(
            enmPcmDefineType type
            , string FileName
            , enmChipType chip
            , bool isSecondary
            , int SrcStartAdr
            , int DesStartAdr
            , int Length
            , object[] Option)
        {
            this.type = type;
            this.FileName = FileName;
            this.chip = chip;
            this.isSecondary = isSecondary;
            this.SrcStartAdr = SrcStartAdr;
            this.DesStartAdr = DesStartAdr;
            this.SrcLength = Length;
            this.Option = Option;
            if (chip == enmChipType.YM2610B)
            {
                if (Option != null)
                {
                    this.DatLoopAdr = Option.ToString() == "0" ? 0 : 1;
                }
                else
                {
                    this.DatLoopAdr = 0;
                }
                ;
            }
        }

        public clsPcmDatSeq(
            enmPcmDefineType type
            , int No
            , enmChipType chip
            , bool isSecondary
            , int BaseFreq
            , int DatStartAdr
            , int DatEndAdr
            , int DatLoopAdr
            , object[] Option)
        {
            this.type = type;
            this.No = No;
            this.chip = chip;
            this.isSecondary = isSecondary;
            this.BaseFreq = BaseFreq;
            this.DatStartAdr = DatStartAdr;
            this.DatEndAdr = DatEndAdr;
            this.DatLoopAdr = DatLoopAdr;
            this.Option = Option;
        }
    }

    public class clsToneDoubler
    {
        public int num = 0;
        public List<clsTD> lstTD = null;

        public clsToneDoubler(int num, List<clsTD> lstTD)
        {
            this.num = num;
            this.lstTD = lstTD;
        }
    }

    public class clsTD
    {
        public int OP1ML = 0;
        public int OP2ML = 0;
        public int OP3ML = 0;
        public int OP4ML = 0;
        public int OP1DT2 = 0;
        public int OP2DT2 = 0;
        public int OP3DT2 = 0;
        public int OP4DT2 = 0;
        public int KeyShift = 0;

        public clsTD(int op1ml, int op2ml, int op3ml, int op4ml, int op1dt2, int op2dt2, int op3dt2, int op4dt2, int keyshift)
        {
            OP1ML = op1ml;
            OP2ML = op2ml;
            OP3ML = op3ml;
            OP4ML = op4ml;
            OP1DT2 = op1dt2;
            OP2DT2 = op2dt2;
            OP3DT2 = op3dt2;
            OP4DT2 = op4dt2;
            KeyShift = keyshift;
        }
    }

    public class clsEcho
    {
        private List<Note> buf = new List<Note>();
        private long length = 0;
        private int _backStep = 1;
        public int backStep
        {
            set
            {
                _backStep = Math.Min(Math.Max(value, 1), 9);
            }
            get
            {
                return _backStep;
            }
        }
        public int downVolume = 0;

        public clsEcho()
        {
            buf = new List<Note>();
            for (int i = 0; i < 10; i++)
            {
                Note note = new Note();
                note.cmd = 'c';
                note.octave = 1;
                buf.Add(note);
            }
        }

        public void Add(Note note)
        {
            buf.Add(note);
            buf.RemoveAt(0);
            //lengthは直前の値が常に使用される
            length = note.length;
        }

        public Note GetEchoNote(int bs)
        {
            Note note= buf[buf.Count - bs];
            note.length = (int)length;
            return note;
        }
    }
}
