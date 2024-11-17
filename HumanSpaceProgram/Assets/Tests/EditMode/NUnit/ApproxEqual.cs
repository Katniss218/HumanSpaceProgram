using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_EditMode.NUnit
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

    public class Vector3DblApproximateComparer : IEqualityComparer<Vector3Dbl>
    {
        double tolerance = 0;

        public Vector3DblApproximateComparer( double tolerance )
        {
            this.tolerance = tolerance;
        }

        public bool Equals( Vector3Dbl actualVector, Vector3Dbl expectedVector )
        {
            return Math.Abs( actualVector.x - expectedVector.x ) < tolerance
                && Math.Abs( actualVector.y - expectedVector.y ) < tolerance
                && Math.Abs( actualVector.z - expectedVector.z ) < tolerance;
        }

        public int GetHashCode( Vector3Dbl obj )
        {
            return 0;
        }
    }

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