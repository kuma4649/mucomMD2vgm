using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class MML
    {
        public Line line;
        public int column;

        public enmMMLType type;
        public List<object> args;
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
}
