using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Types
{
    public struct Unum //: IComparable, IFormattable, IConvertible, IComparable<Unum>, IEquatable<Unum>
    {
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }


        public static Unum operator +(Unum left, Unum right)
        {
            return new Unum();
        }

        //public static Unum operator -(Unum left, Unum right)
        //{

        //}

        //public static Unum operator *(Unum left, Unum right)
        //{

        //}

        //public static Unum operator /(Unum left, Unum right)
        //{

        //}

        //public static bool operator ==(Unum left, Unum right)
        //{

        //}

        //public static bool operator !=(Unum left, Unum right)
        //{

        //}

        //public static bool operator <(Unum left, Unum right)
        //{

        //}

        //public static bool operator >(Unum left, Unum right)
        //{

        //}

        //public static bool operator <=(Unum left, Unum right)
        //{

        //}

        //public static bool operator >=(Unum left, Unum right)
        //{

        //}

        //public static implicit operator Unum(short x)
        //{
        //}

        //public static implicit operator Unum(ushort x)
        //{
        //}

        // Since there is no loss of information when converting from an int to an Unum we allow it implicitly.
        public static implicit operator Unum(int x)
        {
            return new Unum();
        }

        //public static implicit operator Unum(uint x)
        //{
        //}

        //public static implicit operator Unum(float x)
        //{
        //}

        //public static implicit operator Unum(double x)
        //{
        //}

        //public static implicit operator Unum(long x)
        //{
        //}

        //public static implicit operator Unum(ulong x)
        //{
        //}

        //public static implicit operator Unum(decimal x)
        //{
        //}

        // Converting from an Unum to int results in information loss, so only allowing it explicitly (with a cast).
        public static explicit operator int(Unum x)
        {
            return 1;
        }
    }
}
