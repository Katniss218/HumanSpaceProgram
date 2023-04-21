using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public struct Vector3Dbl
    {
        public double x;
        public double y;
        public double z;

        public static readonly Vector3Dbl zero = new Vector3Dbl( 0, 0, 0 );
        public static readonly Vector3Dbl one = new Vector3Dbl( 1, 1, 1 );

        public Vector3Dbl( double x, double y, double z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
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
            return Add( v1,v2 );
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
    }
}
