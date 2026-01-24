using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests.NUnit
{
    public class QuaternionApproximateComparer : IEqualityComparer<Quaternion>
    {
        float tolerance = 0;

        public QuaternionApproximateComparer( float tolerance )
        {
            this.tolerance = tolerance;
        }

        public bool Equals( Quaternion actualVector, Quaternion expectedVector )
        {
            return Math.Abs( actualVector.x - expectedVector.x ) < tolerance
                && Math.Abs( actualVector.y - expectedVector.y ) < tolerance
                && Math.Abs( actualVector.z - expectedVector.z ) < tolerance
                && Math.Abs( actualVector.w - expectedVector.w ) < tolerance;
        }

        public int GetHashCode( Quaternion obj )
        {
            return 0;
        }
    }
}