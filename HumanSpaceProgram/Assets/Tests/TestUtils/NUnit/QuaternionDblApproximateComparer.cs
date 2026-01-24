using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests.NUnit
{
    public class QuaternionDblApproximateComparer : IEqualityComparer<QuaternionDbl>
    {
        double tolerance = 0;

        public QuaternionDblApproximateComparer( double tolerance )
        {
            this.tolerance = tolerance;
        }

        public bool Equals( QuaternionDbl actualVector, QuaternionDbl expectedVector )
        {
            return Math.Abs( actualVector.x - expectedVector.x ) < tolerance
                && Math.Abs( actualVector.y - expectedVector.y ) < tolerance
                && Math.Abs( actualVector.z - expectedVector.z ) < tolerance
                && Math.Abs( actualVector.w - expectedVector.w ) < tolerance;
        }

        public int GetHashCode( QuaternionDbl obj )
        {
            return 0;
        }
    }
}