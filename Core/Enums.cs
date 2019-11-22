using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public enum enmFormat
    {
        VGM,
        XGM,
        ZGM
    }

    public enum enmChannelType
    {
        Multi,   // その他
        FMOPL,   // OPL系のFMCh
        FMOPN,   // OPN系のFMCh
        FMOPNex, // OPN系の拡張FMCh
        FMOPM,   // OPM系のFMCh
        DCSG,
        PCM,
        ADPCM,
        RHYTHM,
        FMPCM,
        DCSGNOISE,
        SSG,
        ADPCMA,
        ADPCMB,
        WaveForm,
        FMPCMex
    }

    public enum enmChipType : int
    {
        None = -1,
        YM2151 = 0,
        YM2203 = 1,
        YM2608 = 2,
        YM2610B = 3,
        YM2612 = 4,
        SN76489 = 5,
        RF5C164 = 6,
        SEGAPCM = 7,
        HuC6280 = 8,
        YM2612X = 9,
        YM2413 = 10,
        C140 = 11,
        AY8910 = 12,
        CONDUCTOR = 13,
        K051649 = 14
    }

    public enum enmPcmDefineType
    {
        /// <summary>
        /// 自動定義
        /// </summary>
        Easy,
        /// <summary>
        /// データのみ定義
        /// </summary>
        RawData,
        /// <summary>
        /// 音色情報のみ定義
        /// </summary>
        Set,
        /// <summary>
        /// Mucom88Adpcm定義
        /// </summary>
        Mucom88
    }

    public enum enmPCMSTATUS
    {
        NONE,
        USED,
        ERROR
    }

    //public enum ePartType
    //{
    //    YM2612, YM2612extend, SegaPSG, Rf5c164
    //}

    public enum enmLfoType
    {
        unknown,
        Tremolo,
        Vibrato,
        Hardware
    }

    public enum enmMMLType
    {
        unknown,
        Clock,            // C
        TimerB,           // t
        Tempo,            // T
        Instrument,       // @
        Volume,           // v
        TotalVolume,      // V
        Octave,           // o
        OctaveUp,         // >
        OctaveDown,       // <
        VolumeUp,         // )
        VolumeDown,       // (
        Length,           // l
        LengthClock,      // #
        Pan,              // p
        Detune,           // D
        PcmMode,          // m
        PcmMap,           // mon mof
        Gatetime,         // q
        GatetimeDiv,      // Q
        ExtendChannel,    // EX
        HardEnvelope,     // EH
        LoopPoint,        // L
        Porta,            // {
        PortaEnd,         // }
        Repeat,           // [
        RepeatEnd,        // ]
        Renpu,            // {
        RenpuEnd,         // }
        RepertExit,       // /
        Lfo,              // M
        LfoSwitch,        // S
        KeyShift,         // K
        RelativeVolume,   // V
        EchoMacro,        // \=
        Echo,             // \
        RelativeKeyShift, // k(v1.7)
        Shuffle,          // s(v1.7)
        HardLfo,          // H
        Reverb,           // R
        ReverbONOF,       // RF
        ReverbMode,       // Rm
        SoftLfo,          // M
        SoftLfoOnOff,     // MF
        SoftLfoDelay,     // MW
        SoftLfoClock,     // MC
        SoftLfoLength,    // ML
        SoftLfoDepth,     // MD
        SlotDetune,       // S
        Envelope,         // E
        MixerMode,        // P
        Noise,            // w
        //s(v1.5)
        //m(v1.5)
        Y,                // y
        PCMVolumeMode,    // vm
        Macro,            // *
        Comment,          // ;
        CompileSkip,      // :
        FillRest,         // !
        Jump,             // J
        None,             // |
        Note,             // c d e f g a b
        Rest,             // r
        RestNoWork,       // R
        Bend,             // _
        Tie,              // &
        TiePC,            // ^
        TieMC,            // ~
        ToneDoubler,      // , 0
        Lyric,            // "
        SusOnOff          // so sf
    }

    public enum enmLoopExStep
    {
        none,
        Inspect,
        Playing
    }

}
