using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    /// <summary>
    /// A double-precision Vector3.
    /// </summary>
    [Serializable]
    public struct Vector3Dbl
    {
        [SerializeField]
        public double x;
        [SerializeField]
        public double y;
        [SerializeField]
        public double z;

        public double sqMagnitude => x * x + y * y + z * z;

        public double magnitude => Math.Sqrt( sqMagnitude );

        public static readonly Vector3Dbl zero = new Vector3Dbl( 0, 0, 0 );
        public static readonly Vector3Dbl one = new Vector3Dbl( 1, 1, 1 );

        public Vector3Dbl( double x, double y, double z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 GetDirection( Vector3Dbl from, Vector3Dbl to )
        {
            Vector3Dbl dir = to - from;
            dir.Normalize();

            return new Vector3( (float)dir.x, (float)dir.y, (float)dir.z );
        }

        public void Normalize()
        {
            double magn = magnitude;
            this.x /= magn;
            this.y /= magn;
            this.z /= magn;
        }

        public Vector3Dbl normalized
        {
            get
            {
                double magn = magnitude;
                return new Vector3Dbl( this.x / magn, this.y / magn, this.z / magn );
            }
        }

        public Vector3 NormalizeToVector3()
        {
            double magn = magnitude;
            return new Vector3( (float)(this.x / magn), (float)(this.y / magn), (float)(this.z / magn) );
        }

        public static Vector3Dbl Add( Vector3Dbl v1, Vector3Dbl v2 )
        {
            return new Vector3Dbl( v1.x + v2.x, v1.y + v2.y, v1.z + v2.z );
        }
        public static Vector3Dbl Add( Vector3Dbl v1, Vector3 v2 )
        {
            return new Vector3Dbl( v1.x + v2.x, v1.y + v2.y, v1.z + v2.z );
        }
        public static Vector3Dbl Subtract( Vector3Dbl v1, Vector3Dbl v2 )
        {
            return new Vector3Dbl( v1.x - v2.x, v1.y - v2.y, v1.z - v2.z );
        }
        public static Vector3Dbl Subtract( Vector3Dbl v1, Vector3 v2 )
        {
            return new Vector3Dbl( v1.x - v2.x, v1.y - v2.y, v1.z - v2.z );
        }
        public static Vector3Dbl Multiply( Vector3Dbl v, double s )
        {
            return new Vector3Dbl( v.x * s, v.y * s, v.z * s );
        }
        public static Vector3Dbl Divide( Vector3Dbl v, double s )
        {
            return new Vector3Dbl( v.x / s, v.y / s, v.z / s );
        }

        public static Vector3Dbl operator +( Vector3Dbl v1, Vector3Dbl v2 )
        {
            return Add( v1, v2 );
        }
        public static Vector3Dbl operator +( Vector3Dbl v1, Vector3 v2 )
        {
            return Add( v1, v2 );
        }
        public static Vector3Dbl operator -( Vector3Dbl v1, Vector3Dbl v2 )
        {
            return Subtract( v1, v2 );
        }
        public static Vector3Dbl operator -( Vector3Dbl v1, Vector3 v2 )
        {
            return Subtract( v1, v2 );
        }

        public static Vector3Dbl operator *( Vector3Dbl v, double s )
        {
            return Multiply( v, s );
        }

        public static Vector3Dbl operator *( double s, Vector3Dbl v )
        {
            return Multiply( v, s );
        }

        public static Vector3Dbl operator /( Vector3Dbl v, double s )
        {
            return Divide( v, s );
        }

        public static explicit operator Vector3( Vector3Dbl v )
        {
            return new Vector3( (float)v.x, (float)v.y, (float)v.z );
        }

        public static implicit operator Vector3Dbl( Vector3 v )
        {
            return new Vector3Dbl( v.x, v.y, v.z );
        }
    }
}