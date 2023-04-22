using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    [Serializable]
    /// <summary>
    /// A struct that can represent vectors with extremely large magnitudes reasonably accurately.
    /// </summary>
    public struct Vector3Large
    {
        [SerializeField]
        public double x;
        [SerializeField]
        public double y;
        [SerializeField]
        public double z;

        public double sqMagnitude => x * x + y * y + z * z;

        public double magnitude => Math.Sqrt( sqMagnitude );

        public static readonly Vector3Large zero = new Vector3Large( 0, 0, 0 );
        public static readonly Vector3Large one = new Vector3Large( 1, 1, 1 );

        public Vector3Large( double x, double y, double z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 GetDirection( Vector3Large from, Vector3Large to )
        {
            Vector3Large dir = to - from;
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
        
        public Vector3 NormalizeToVector3()
        {
            double magn = magnitude;
            return new Vector3( (float)(this.x / magn), (float)(this.y / magn), (float)(this.z / magn) );
        }

        public static Vector3Large Add( Vector3Large v1, Vector3Large v2 )
        {
            return new Vector3Large( v1.x + v2.x, v1.y + v2.y, v1.z + v2.z );
        }
        public static Vector3Large Add( Vector3Large v1, Vector3 v2 )
        {
            return new Vector3Large( v1.x + v2.x, v1.y + v2.y, v1.z + v2.z );
        }
        public static Vector3Large Subtract( Vector3Large v1, Vector3Large v2 )
        {
            return new Vector3Large( v1.x - v2.x, v1.y - v2.y, v1.z - v2.z );
        }
        public static Vector3Large Subtract( Vector3Large v1, Vector3 v2 )
        {
            return new Vector3Large( v1.x - v2.x, v1.y - v2.y, v1.z - v2.z );
        }
        public static Vector3Large Multiply( Vector3Large v, double s )
        {
            return new Vector3Large( v.x * s, v.y * s, v.z * s );
        }
        public static Vector3Large Divide( Vector3Large v, double s )
        {
            return new Vector3Large( v.x / s, v.y / s, v.z / s );
        }

        public static Vector3Large operator +( Vector3Large v1, Vector3Large v2 )
        {
            return Add( v1,v2 );
        }
        public static Vector3Large operator +( Vector3Large v1, Vector3 v2 )
        {
            return Add( v1, v2 );
        }
        public static Vector3Large operator -( Vector3Large v1, Vector3Large v2 )
        {
            return Subtract( v1, v2 );
        }
        public static Vector3Large operator -( Vector3Large v1, Vector3 v2 )
        {
            return Subtract( v1, v2 );
        }

        public static Vector3Large operator *( Vector3Large v, double s )
        {
            return Multiply( v, s );
        }

        public static Vector3Large operator *( double s, Vector3Large v )
        {
            return Multiply( v, s );
        }

        public static Vector3Large operator /( Vector3Large v, double s )
        {
            return Divide( v, s );
        }
    }
}
