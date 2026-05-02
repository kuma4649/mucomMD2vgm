using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IDv
    {
        bool Eq();
        void Rst();
    }

    public class Dbool : IDv
    {
        public bool Eq()
        {
            return s == v;
        }

        public void Rst()
        {
            s = v;
        }

        public bool? v;
        private bool? s;
    }

    public class Dint : IDv
    {
        public Dint(int val)
        {
            this.val = val;
            s = null;
        }

        public bool Eq()
        {
            return s == val;
        }

        public void Rst()
        {
            s = val;
        }

        public int? val;
        private int? s;

        public static implicit operator Dint(int v)
        {
            throw new NotImplementedException();
        }

    }

    public class Dlong : IDv
    {
        public bool Eq()
        {
            return s == v;
        }

        public void Rst()
        {
            s = v;
        }

        public long? v;
        private long? s;
    }

    public class Dfloat : IDv
    {
        public bool Eq()
        {
            return s == v;
        }

        public void Rst()
        {
            s = v;
        }

        public float? v;
        private float? s;
    }

    public class Dbyte : IDv
    {
        public bool Eq()
        {
            return s == v;
        }

        public void Rst()
        {
            s = v;
        }

        public byte? v;
        private byte? s;
    }

    public class Dchar : IDv
    {
        public bool Eq()
        {
            return s == v;
        }

        public void Rst()
        {
            s = v;
        }

        public char? v;
        private char? s;
    }

    public class Ddouble : IDv
    {
        public bool Eq()
        {
            return s == v;
        }

        public void Rst()
        {
            s = v;
        }

        public double? v;
        private double? s;
    }

}
