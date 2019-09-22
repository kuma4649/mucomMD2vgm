using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class GD3maker
    {
        public void make(List<byte> dat, Information info, string lyric)
        {
            //'Gd3 '
            dat.Add(0x47);
            dat.Add(0x64);
            dat.Add(0x33);
            dat.Add(0x20);

            //GD3 Version
            dat.Add(0x00);
            dat.Add(0x01);
            dat.Add(0x00);
            dat.Add(0x00);

            //GD3 Length(dummy)
            dat.Add(0x00);
            dat.Add(0x00);
            dat.Add(0x00);
            dat.Add(0x00);

            //TrackName
            foreach (byte b in Encoding.Unicode.GetBytes(info.TitleName)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);
            foreach (byte b in Encoding.Unicode.GetBytes(info.TitleNameJ)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //GameName
            foreach (byte b in Encoding.Unicode.GetBytes(info.GameName)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);
            foreach (byte b in Encoding.Unicode.GetBytes(info.GameNameJ)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //SystemName
            foreach (byte b in Encoding.Unicode.GetBytes(info.SystemName)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);
            foreach (byte b in Encoding.Unicode.GetBytes(info.SystemNameJ)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //Composer
            foreach (byte b in Encoding.Unicode.GetBytes(info.Composer)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);
            foreach (byte b in Encoding.Unicode.GetBytes(info.ComposerJ)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //ReleaseDate
            foreach (byte b in Encoding.Unicode.GetBytes(info.ReleaseDate)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //Converted
            foreach (byte b in Encoding.Unicode.GetBytes(info.Converted)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //Notes
            foreach (byte b in Encoding.Unicode.GetBytes(info.Notes)) dat.Add(b);
            dat.Add(0x00);
            dat.Add(0x00);

            //歌詞
            if (lyric != "")
            {
                foreach (byte b in Encoding.Unicode.GetBytes(lyric)) dat.Add(b);
                dat.Add(0x00);
                dat.Add(0x00);
            }
        }

    }
}
