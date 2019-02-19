﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class Common
    {
        public static int CheckRange(int n, int min, int max)
        {
            int r = n;

            if (n < min)
            {
                r = min;
            }
            if (n > max)
            {
                r = max;
            }

            return r;
        }

        public static byte[] GetPCMDataFromFile(string path, clsPcm instPCM, out bool isRaw, out bool is16bit, out int samplerate)
        {
            return GetPCMDataFromFile(path, instPCM.fileName, instPCM.vol, out isRaw, out is16bit, out samplerate);
        }

        public static byte[] GetPCMDataFromFile(string path, string fileName, int vol, out bool isRaw, out bool is16bit, out int samplerate)
        {
            string fnPcm = Path.Combine(path, fileName);
            isRaw = false;
            is16bit = false;
            samplerate = 8000;

            if (!File.Exists(fnPcm))
            {
                msgBox.setErrMsg(string.Format(msg.get("E02000"), fileName));
                return null;
            }

            // ファイルの読み込み
            byte[] buf = File.ReadAllBytes(fnPcm);

            if (Path.GetExtension(fileName).ToUpper().Trim() != ".WAV")
            {
                isRaw = true;
                return buf;
            }

            if (buf.Length < 4)
            {
                msgBox.setErrMsg(msg.get("E02001"));
                return null;
            }
            if (buf[0] != 'R' || buf[1] != 'I' || buf[2] != 'F' || buf[3] != 'F')
            {
                msgBox.setErrMsg(msg.get("E02002"));
                return null;
            }

            // サイズ取得
            int fSize = buf[0x4] + buf[0x5] * 0x100 + buf[0x6] * 0x10000 + buf[0x7] * 0x1000000;

            if (buf[0x8] != 'W' || buf[0x9] != 'A' || buf[0xa] != 'V' || buf[0xb] != 'E')
            {
                msgBox.setErrMsg(msg.get("E02003"));
                return null;
            }

            try
            {
                int p = 12;
                byte[] des = null;

                while (p < fSize + 8)
                {
                    if (buf[p + 0] == 'f' && buf[p + 1] == 'm' && buf[p + 2] == 't' && buf[p + 3] == ' ')
                    {
                        p += 4;
                        int size = buf[p + 0] + buf[p + 1] * 0x100 + buf[p + 2] * 0x10000 + buf[p + 3] * 0x1000000;
                        p += 4;
                        int format = buf[p + 0] + buf[p + 1] * 0x100;
                        if (format != 1)
                        {
                            msgBox.setErrMsg(string.Format(msg.get("E02004"), format));
                            return null;
                        }

                        int channels = buf[p + 2] + buf[p + 3] * 0x100;
                        if (channels != 1)
                        {
                            msgBox.setErrMsg(string.Format(msg.get("E02005"), channels));
                            return null;
                        }

                        samplerate = buf[p + 4] + buf[p + 5] * 0x100 + buf[p + 6] * 0x10000 + buf[p + 7] * 0x1000000;
                        if (samplerate != 8000 && samplerate != 16000 && samplerate != 18500 && samplerate != 14000)
                        {
                            msgBox.setWrnMsg(string.Format(msg.get("E02006"), samplerate));
                            //return null;
                        }

                        int bytepersec = buf[p + 8] + buf[p + 9] * 0x100 + buf[p + 10] * 0x10000 + buf[p + 11] * 0x1000000;
                        if (bytepersec != 8000)
                        {
                            //    msgBox.setWrnMsg(string.Format("PCMファイル：仕様とは異なる平均データ割合です。({0})", bytepersec));
                            //    return null;
                        }

                        int bitswidth = buf[p + 14] + buf[p + 15] * 0x100;
                        if (bitswidth != 8 && bitswidth != 16)
                        {
                            msgBox.setErrMsg(string.Format(msg.get("E02007"), bitswidth));
                            return null;
                        }

                        is16bit = bitswidth == 16;

                        int blockalign = buf[p + 12] + buf[p + 13] * 0x100;
                        if (blockalign != (is16bit ? 2 : 1))
                        {
                            msgBox.setErrMsg(string.Format(msg.get("E02008"), blockalign));
                            return null;
                        }


                        p += size;
                    }
                    else if (buf[p + 0] == 'd' && buf[p + 1] == 'a' && buf[p + 2] == 't' && buf[p + 3] == 'a')
                    {
                        p += 4;
                        int size = buf[p + 0] + buf[p + 1] * 0x100 + buf[p + 2] * 0x10000 + buf[p + 3] * 0x1000000;
                        p += 4;

                        des = new byte[size];
                        Array.Copy(buf, p, des, 0x00, size);
                        p += size;
                    }
                    else
                    {
                        p += 4;
                        int size = buf[p + 0] + buf[p + 1] * 0x100 + buf[p + 2] * 0x10000 + buf[p + 3] * 0x1000000;
                        p += 4;

                        p += size;
                    }
                }

                // volumeの加工
                if (is16bit)
                {
                    for (int i = 0; i < des.Length; i += 2)
                    {
                        //16bitのwavファイルはsignedのデータのためそのままボリューム変更可能
                        int b = (int)((short)(des[i] | (des[i + 1] << 8)) * vol * 0.01);
                        b = (b > 0x7fff) ? 0x7fff : b;
                        b = (b < -0x8000) ? -0x8000 : b;
                        des[i] = (byte)(b & 0xff);
                        des[i + 1] = (byte)((b & 0xff00) >> 8);
                    }
                }
                else
                {
                    for (int i = 0; i < des.Length; i++)
                    {
                        //8bitのwavファイルはunsignedのデータのためsignedのデータに変更してからボリューム変更する
                        int d = des[i];
                        //signed化
                        d -= 0x80;
                        d = (int)(d * vol * 0.01);
                        //clip
                        d = (d > 127) ? 127 : d;
                        d = (d < -128) ? -128 : d;
                        //unsigned化
                        d += 0x80;

                        des[i] = (byte)d;
                    }
                }

                return des;
            }
            catch
            {
                msgBox.setErrMsg(msg.get("E02009"));
                return null;
            }
        }

        public static void SetUInt32bit31(byte[] buf, int ptr, UInt32 value, bool sw = false)
        {
            buf[ptr + 0] = (byte)(value & 0xff);
            buf[ptr + 1] = (byte)((value & 0xff00) >> 8);
            buf[ptr + 2] = (byte)((value & 0xff0000) >> 16);
            buf[ptr + 3] = (byte)((value & 0x7f000000) >> 24);
            if (sw) buf[ptr + 3] |= 0x80;
        }

        public static byte[] PcmPadding(ref byte[] buf, ref long size, byte paddingData, int paddingSize)
        {
            byte[] newBuf = new byte[size + (paddingSize - (size % paddingSize))];
            for (int i = (int)size; i < newBuf.Length; i++) newBuf[i] = paddingData;
            Array.Copy(buf, newBuf, size);
            buf = newBuf;
            size = buf.Length;
            return newBuf;
        }

        public static List<string> DivParts(string parts, Dictionary<enmChipType, ClsChip[]> chips)
        {
            List<string> ret = new List<string>();
            string a = "";
            int k = 1;
            int m = 0;
            string n0 = "";

            try
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (m == 0 && parts[i] >= 'A' && parts[i] <= 'Z')
                    {
                        a = parts[i].ToString();
                        if (i + 1 < parts.Length && parts[i + 1] >= 'a' && parts[i + 1] <= 'z')
                        {
                            a += parts[i + 1].ToString();
                            i++;
                        }
                        else
                        {
                            a += " ";
                        }

                        k = GetChMax(a, chips) > 9 ? 2 : 1;
                        n0 = "";

                    }
                    else if (m == 0 && parts[i] == ',')
                    {
                        n0 = "";
                    }
                    else if (m == 0 && parts[i] == '-')
                    {
                        m = 1;
                    }
                    else if (parts[i] >= '0' && parts[i] <= '9')
                    {
                        string n = parts.Substring(i, k);
                        if (k == 2 && i + 1 < parts.Length)
                        {
                            i++;
                        }

                        if (m == 0)
                        {
                            n0 = n;

                            if (!int.TryParse(n0, out int s)) return null;
                            string p = string.Format("{0}{1:00}", a, s);
                            ret.Add(p);
                        }
                        else
                        {
                            string n1 = n;

                            if (!int.TryParse(n0, out int s)) return null;
                            if (!int.TryParse(n1, out int e)) return null;
                            if (s >= e) return null;

                            do
                            {
                                s++;
                                string p = string.Format("{0}{1:00}", a, s);
                                if (ret.Contains(p)) return null;
                                ret.Add(p);
                            } while (s < e);

                            i++;
                            m = 0;
                            n0 = "";
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                //パート解析に失敗 
                msgBox.setErrMsg(string.Format(msg.get("E02010"), parts), "", 0);
            }

            return ret;
        }

        internal static void Add32bits(List<byte> desDat, uint v)
        {
            desDat.Add((byte)v);
            desDat.Add((byte)(v >> 8));
            desDat.Add((byte)(v >> 16));
            desDat.Add((byte)(v >> 24));
        }

        internal static void Add16bits(List<byte> desDat, uint v)
        {
            desDat.Add((byte)v);
            desDat.Add((byte)(v >> 8));
        }

        private static int GetChMax(string a, Dictionary<enmChipType, ClsChip[]> chips)
        {
            foreach (KeyValuePair<enmChipType, ClsChip[]> kvp in chips)
            {
                foreach (ClsChip chip in kvp.Value)
                {
                    if (chip.Ch[0].Name.Substring(0, 2) == a)
                    {
                        return chip.ChMax;
                    }
                }
            }

            return 0;
        }

        public static int ParseNumber(string s)
        {
            if (s.Trim().ToUpper().IndexOf("0x") == 0)
            {
                return Convert.ToInt32(s.Substring(2), 16);
            }
            else if (s.Trim().IndexOf("$") == 0)
            {
                return Convert.ToInt32(s.Substring(1), 16);
            }

            return int.Parse(s);
        }

        public static byte[] MakePCMDataBlock(byte dataType, clsPcmDatSeq pds, byte[] data)
        {
            List<byte> desDat = new List<byte>();

            desDat.Add(0x67); //Data block command
            desDat.Add(0x66); //compatibility command

            desDat.Add(dataType); //data type

            int length = pds.SrcLength == -1 ? data.Length : pds.SrcLength;

            if (data.Length < pds.SrcStartAdr + length)
            {
                length = data.Length - pds.SrcStartAdr;
            }
            byte[] dmy = new byte[length];
            Array.Copy(data, pds.SrcStartAdr, dmy, 0, length);

            Common.Add32bits(desDat, (uint)(length + 8) | (pds.isSecondary ? 0x8000_0000 : 0x0000_0000));//size of data, in bytes
            Common.Add32bits(desDat, (uint)(pds.DesStartAdr + length));//size of the entire ROM
            Common.Add32bits(desDat, (uint)pds.DesStartAdr);//start address of data

            desDat.AddRange(dmy);

            pds.DatStartAdr = pds.DesStartAdr;
            pds.DatEndAdr = pds.DatStartAdr + length - 1;

            return desDat.ToArray();
        }

        public static byte[] MakePCMDataBlockType2(byte dataType, clsPcmDatSeq pds, byte[] data)
        {
            List<byte> desDat = new List<byte>();

            desDat.Add(0x67); //Data block command
            desDat.Add(0x66); //compatibility command

            desDat.Add(dataType); //data type

            int length = pds.SrcLength == -1 ? data.Length : pds.SrcLength;

            if (data.Length < pds.SrcStartAdr + length)
            {
                length = data.Length - pds.SrcStartAdr;
            }
            byte[] dmy = new byte[length];
            Array.Copy(data, pds.SrcStartAdr, dmy, 0, length);

            Common.Add32bits(desDat, (uint)(length + 2) | (pds.isSecondary ? 0x8000_0000 : 0x0000_0000));//size of data, in bytes
            Common.Add16bits(desDat, (uint)pds.DesStartAdr);//start address of data

            desDat.AddRange(dmy);

            pds.DatStartAdr = pds.DesStartAdr;
            pds.DatEndAdr = pds.DatStartAdr + length - 1;

            return desDat.ToArray();
        }

        public static int? GetNumsFromString(string src, int ptr, ref int len)
        {
            try
            {
                string n = "";
                int stPtr = ptr;

                //タブと空白は読み飛ばす
                while (src[ptr] == ' ' || src[ptr] == '\t') ptr++;

                //符号を取得する(ない場合は正とする)
                if (src[ptr] == '-' || src[ptr] == '+')
                {
                    n += src[ptr];
                    ptr++;
                }

                //タブと空白は読み飛ばす
                while (src[ptr] == ' ' || src[ptr] == '\t') ptr++;

                //１６進数指定されているか
                if (src[ptr] != '$')
                {
                    //数字でなくなるまで取得
                    while (true)
                    {
                        if (src[ptr] >= '0' && src[ptr] <= '9')
                        {
                            n += src[ptr];
                            ptr++;
                        }
                        else
                        {
                            break;
                        }
                        if (ptr == src.Length) break;
                    }

                    //数値に変換できたら成功
                    int r;
                    if (!int.TryParse(n, out r))
                    {
                        return null;
                    }

                    len = ptr - stPtr;
                    return r;
                }
                else
                {
                    //数字でなくなるまで取得
                    while (true)
                    {
                        if (src[ptr] >= '0' && src[ptr] <= '9')
                        {
                            n += src[ptr];
                            ptr++;
                        }
                        else if ((src[ptr] >= 'a' && src[ptr] <= 'f')
                            || (src[ptr] >= 'A' && src[ptr] <= 'F'))
                        {
                            n += src[ptr];
                            ptr++;
                        }
                        else
                        {
                            break;
                        }
                        if (ptr == src.Length) break;
                    }

                    //数値に変換できたら成功
                    try
                    {
                        int r = Convert.ToInt32(n, 16);
                        len = ptr - stPtr;
                        return r;
                    }
                    catch
                    {
                        return null;
                    }
                }

            }
            catch
            {

            }

            return null;
        }

        public static long gcd(long a,long b)
        {
            if (a == b) return a;
            if (a == 0) return b;
            if (b == 0) return a;

            if (a > b) return gcd(a % b, b);
            return gcd(a, b % a);
        }

        public static long lcm(long a,long b)
        {
            long g = gcd(a, b);
            if (g == 0) return a;
            return (a * b) / g;
        }

        public static long aryLcm(long[] a)
        {
            Array.Sort(a);
            long ans = 1;
            foreach(long i in a)
            {
                ans = lcm(ans, i);
            }
            return ans;
        }

    }
}
