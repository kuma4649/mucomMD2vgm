using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Information
    {

        public const string TITLENAME = "TITLE";
        public const string TITLENAMEJ = "TITLEJ";
        public const string GAMENAME = "GAMENAME";
        public const string GAMENAMEJ = "GAMENAMEJ";
        public const string SYSTEMNAME = "SYSTEMNAME";
        public const string SYSTEMNAMEJ = "SYSTEMNAMEJ";
        public const string COMPOSER = "COMPOSER";
        public const string COMPOSERJ = "COMPOSERJ";
        public const string RELEASEDATE = "DATE";
        public const string CONVERTED = "AUTHOR";
        public const string NOTES = "COMMENT";
        public const string PARTNAME = "PART";
        //public const string CLOCKCOUNT = "CLOCKCOUNT";
        public const string FORCEDMONOPARTYM2612 = "FORCEDMONOPARTYM2612";
        public const string VERSION = "MUCOM88";
        public const string FORMAT = "FILEFORMAT";
        public const string XGMBASEFRAME = "XGMBASEFRAME";
        public const string VSYNCRATE = "VSYNC";
        public const string OCTAVEREV = "OCTAVE-REV";
        //public const string ISK052539 = "ISK052539";
        public const string PRIMARY = "PRIMARY";
        public const string SECONDARY = "SECONDARY";
        public const string VOICE = "VOICE";
        public const string PCM = "PCM";
        public const string PCMVOLUME = "PCMVOLUME";
        readonly public static string[] IDName = new string[] { PRIMARY, SECONDARY };
        public const long DEFAULT_TEMPO = 120L;

        public const long DEFAULT_CLOCK_COUNT = 128L;
        public const long VGM_SAMPLE_PER_SECOND = 44100L;
        public const long DEFAULT_SAMPLES_PER_CLOCK = VGM_SAMPLE_PER_SECOND * 60 * 4 / (DEFAULT_TEMPO * DEFAULT_CLOCK_COUNT);

        public const long XGM_DEFAULT_CLOCK_COUNT = 120L;
        public const long XGM_SAMPLE_PER_SECOND = 60L;
        public const long XGM_DEFAULT_SAMPLES_PER_CLOCK = XGM_SAMPLE_PER_SECOND * 60 * 4 / (DEFAULT_TEMPO * XGM_DEFAULT_CLOCK_COUNT);



        public float Version = 1.61f;
        public string TitleName = "";
        public string TitleNameJ = "";
        public string GameName = "";
        public string GameNameJ = "";
        public string SystemName = "";
        public string SystemNameJ = "";
        public string Composer = "";
        public string ComposerJ = "";
        public string ReleaseDate = "";
        public string Converted = "";
        public string Notes = "";
        public long userClockCount = 0;
        public List<string> monoPart = null;
        public enmFormat format = enmFormat.VGM;
        public long tempo = DEFAULT_TEMPO;
        public long clockCount = DEFAULT_CLOCK_COUNT;
        public double samplesPerClock = DEFAULT_SAMPLES_PER_CLOCK;
        public long xgmSamplesPerSecond = 60L;
        public int vgmVsync = -1;
        public int timerB = 198;//198(default)
        public bool octaveRev = false;
        //public bool isK052539 = false;

        public string Voice = "";
        public string Pcm = "";
        public int PcmVolume = 255;


        public int AddInformation(string buf, int lineNumber, string fn, Dictionary<enmChipType, ClsChip[]> chips)
        {
            string[] settings = buf.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in settings)
            {
                try
                {
                    int p = s.IndexOf(" ");
                    if (p < 0)
                    {
                        p = s.IndexOf("\t");
                        if (p < 0)
                            continue;
                    }

                    string wrd = s.Substring(0, p).Trim().ToUpper();
                    string val = s.Substring(p + 1).Trim();

                    if (wrd == TITLENAMEJ) TitleNameJ = val;
                    else if (wrd == TITLENAME) TitleName = val;
                    else if (wrd == GAMENAME) GameName = val;
                    else if (wrd == GAMENAMEJ) GameNameJ = val;
                    else if (wrd == SYSTEMNAME) SystemName = val;
                    else if (wrd == SYSTEMNAMEJ) SystemNameJ = val;
                    else if (wrd == COMPOSER) Composer = val;
                    else if (wrd == COMPOSERJ) ComposerJ = val;
                    else if (wrd == RELEASEDATE) ReleaseDate = val;
                    else if (wrd == CONVERTED) Converted = val;
                    else if (wrd == NOTES) Notes = val;
                    else if (wrd == VOICE) Voice = val;
                    else if (wrd == PCMVOLUME)
                    {
                        if (!int.TryParse(val, out PcmVolume))
                        {
                            PcmVolume = 255;
                        }
                        PcmVolume = Common.CheckRange(PcmVolume, 0, 255);
                    }
                    else if (wrd == PCM) Pcm = val;
                    else if (wrd == VERSION)
                    {
                        //float.TryParse(val, out float v);
                        //if (v != 1.51f && v != 1.60f) v = 1.60f;
                        Version = 1.61f;// v;
                    }
                    //else if (wrd == CLOCKCOUNT) userClockCount = int.Parse(val);
                    else if (wrd == FORMAT) SetFormat(val);
                    else if (wrd == XGMBASEFRAME) SetXgmBaseFrame(val);
                    else if (wrd == VSYNCRATE) SetVsyncRate(val);
                    else if (wrd == OCTAVEREV) SetOctaveRev(val);
                    //else if (wrd == ISK052539) SetIsK052539(val);
                    else if (wrd == FORCEDMONOPARTYM2612) SetMonoPart(val, chips);
                    else
                    {
                        foreach (ClsChip[] aryChip in chips.Values)
                        {
                            foreach (ClsChip chip in aryChip)
                            {
                                if (wrd == PARTNAME + chip.Name + IDName[chip.ChipID]) chip.SetPartToCh(chip.Ch, val);
                                if (wrd == PARTNAME + chip.ShortName + IDName[chip.ChipID]) chip.SetPartToCh(chip.Ch, val);
                                if (chip.ChipID == 0)
                                {
                                    if (wrd == PARTNAME + chip.Name) chip.SetPartToCh(chip.Ch, val);
                                    if (wrd == PARTNAME + chip.ShortName) chip.SetPartToCh(chip.Ch, val);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    msgBox.setWrnMsg(string.Format(msg.get("E03000"), s), fn, lineNumber);
                }
            }
            
            SetTimers();

            return 0;
        }

        private void SetMonoPart(string val, Dictionary<enmChipType, ClsChip[]> chips)
        {
            monoPart = Common.DivParts(val, chips);
        }

        private void SetFormat(string val)
        {
            switch (val.ToUpper())
            {
                case "VGM":
                default:
                    format = enmFormat.VGM;
                    //tempo = DEFAULT_TEMPO;
                    //clockCount = DEFAULT_CLOCK_COUNT;
                    //samplesPerClock = DEFAULT_SAMPLES_PER_CLOCK;
                    break;
                case "XGM":
                    format = enmFormat.XGM;
                    //tempo = DEFAULT_TEMPO;
                    //clockCount = XGM_DEFAULT_CLOCK_COUNT;
                    //samplesPerClock = XGM_DEFAULT_SAMPLES_PER_CLOCK;
                    break;
            }
        }

        private void SetXgmBaseFrame(string val)
        {
            switch (val.ToUpper())
            {
                case "NTSC":
                default:
                    xgmSamplesPerSecond = 60;
                    break;
                case "PAL":
                    xgmSamplesPerSecond = 50;
                    break;
            }
        }
        
        private void SetVsyncRate(string val)
        {
            vgmVsync = -1;
            if(!int.TryParse(val,out vgmVsync))
            {
                vgmVsync = -1;
            }
        }

        public void SetTimers()
        {
            if (vgmVsync == -1)
            {
                tempo = DEFAULT_TEMPO;
                clockCount = DEFAULT_CLOCK_COUNT;
                switch (format)
                {
                    case enmFormat.VGM:
                    default:
                        samplesPerClock = Information.VGM_SAMPLE_PER_SECOND * 60.0 * 4.0 / (tempo * clockCount);
                        break;
                    case enmFormat.XGM:
                        samplesPerClock = xgmSamplesPerSecond * 60.0 * 4.0 / (tempo * clockCount);
                        break;
                }
            }
            else
            {
                switch (format)
                {
                    case enmFormat.VGM:
                    default:
                        samplesPerClock = 44100 / vgmVsync;
                        break;
                    case enmFormat.XGM:
                        vgmVsync = (int)xgmSamplesPerSecond;
                        samplesPerClock = 1;
                        break;
                }
            }
        }

        private void SetOctaveRev(string val)
        {
            switch (val.ToUpper())
            {
                case "TRUE":
                case "1":
                case "YES":
                case "Y":
                    octaveRev = true;
                    break;
                case "FALSE":
                case "0":
                case "NO":
                case "N":
                default:
                    octaveRev = false;
                    break;
            }
        }

        //private void SetIsK052539(string val)
        //{
        //    switch (val.ToUpper())
        //    {
        //        case "TRUE":
        //        case "1":
        //        case "YES":
        //        case "Y":
        //            isK052539 = true;
        //            break;
        //        case "FALSE":
        //        case "0":
        //        case "NO":
        //        case "N":
        //        default:
        //            isK052539 = false;
        //            break;
        //    }
        //}
    }
}
