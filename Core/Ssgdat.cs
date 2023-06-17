using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Core
{
    public class Ssgdat
    {
        public class Instrument
        {
            public int No = 0;
            public int[] E;
            public int P = 0;
            public int[] M;
        }

        public Instrument[] Instruments;

        public Ssgdat Copy()
        {
            Ssgdat ssgdat = new Ssgdat();
            ssgdat.Instruments = (Instrument[])this.Instruments.Clone();

            return ssgdat;
        }

        public void Save()
        {
            string fullPath = "ssgdat.xml";

            XmlSerializer serializer = new XmlSerializer(typeof(Ssgdat));
            using (StreamWriter sw = new StreamWriter(fullPath, false, new UTF8Encoding(false)))
            {
                serializer.Serialize(sw, this);
            }
        }

        public static Ssgdat Load(string fn)
        {
            try
            {
                string fullPath = fn;

                if (!File.Exists(fullPath)) { return new Ssgdat(); }
                XmlSerializer serializer = new XmlSerializer(typeof(Ssgdat));
                using (StreamReader sr = new StreamReader(fullPath, new UTF8Encoding(false)))
                {
                    return (Ssgdat)serializer.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                Log.ForcedWrite(ex);
                return new Ssgdat();
            }
        }


    }
}
