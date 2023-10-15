using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class MathD
    {
        public static double ASinh( double d )
        {
            return Math.Log( d + Math.Sqrt( d * d + 1.0 ) );
        }

        public static double ACosh( double d )
        {
            return Math.Log( d + Math.Sqrt( d * d - 1.0 ) );
        }

        public static double ATanh( double d )
        {
            return Math.Log( (1.0 + d) / (1.0 - d) ) / 2.0;
        }

        public static double ACoth( double d )
        {
            return ATanh( 1.0 / d );
        }

        public static double ASech( double d )
        {
            return ACosh( 1.0 / d );
        }

        public static double ACsch( double d )
        {
            return ASinh( 1.0 / d );
        }

        public static double Sech( double d )
        {
            return 1.0 / Math.Cosh( d );
        }

        public static double Csch( double d )
        {
            return 1.0 / Math.Sinh( d );
        }

        public static double Coth( double d )
        {
            return Math.Cosh( d ) / Math.Sinh( d );
        }

    }
}
