using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests.NUnit
{
    public class Vector3ApproximateComparer : IEqualityComparer<Vector3>
    {
        float tolerance = 0;

        public Vector3ApproximateComparer( float tolerance )
        {
            this.tolerance = tolerance;
        }

        public bool Equals( Vector3 actualVector, Vector3 expectedVector )
        {
            return Math.Abs( actualVector.x - expectedVector.x ) < tolerance
                && Math.Abs( actualVector.y - expectedVector.y ) < tolerance
                && Math.Abs( actualVector.z - expectedVector.z ) < tolerance;
        }

        public int GetHashCode( Vector3 obj )
        {
            return 0;
        }
    }
}