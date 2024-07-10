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

        /// <summary>
        /// Linearly maps a value from one range onto another range.
        /// </summary>
        /// <param name="value">The value from the original range to map.</param>
        /// <param name="inMin">The min value of the original range.</param>
        /// <param name="inMax">The max value of the original range.</param>
        /// <param name="outMin">The min value of the new range.</param>
        /// <param name="outMax">The max value of the new range.</param>
        /// <returns>The value mapped onto the new range.</returns>
        public static double Map( double value, double inMin, double inMax, double outMin, double outMax )
        {
            // This is related to linear interpolation.

            // First shift the value so that the original range now starts at 0.
            // Then divide to get normalized range (i.e. [0..1]) and multiply to map onto the new range.
            // And lastly, unshift the value so that the new range starts at `outMin`.
            return (((value - inMin) / (inMax - inMin)) * (outMax - outMin)) + outMin;
        }
    }
}
