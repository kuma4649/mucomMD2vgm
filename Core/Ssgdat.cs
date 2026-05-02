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
        private ILog log;
        private IFile file;

        public Ssgdat()
        {
            this.log = null;
            this.file = null;
        }

        public Ssgdat(ILog log, IFile file)
        {
            this.log = log;
            this.file = file;
        }

        public Ssgdat Copy()
        {
            Ssgdat ssgdat = new Ssgdat(log, file);
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

        public Ssgdat Load(string fn)
        {
            try
            {
                string fullPath = fn;

                if (!file.Exists(fullPath)) { return new Ssgdat(log,file); }
                XmlSerializer serializer = new XmlSerializer(typeof(Ssgdat));
                using (StreamReader sr = new StreamReader(fullPath, new UTF8Encoding(false)))
                {
                    return (Ssgdat)serializer.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                log.ForcedWrite(ex);
                return new Ssgdat(log, file);
            }
        }


    }
}
