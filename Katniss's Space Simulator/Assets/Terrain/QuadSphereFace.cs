using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    /// <summary>
    /// Contains information about which of the cube's faces a given quad represents.
    /// </summary>
    public enum QuadSphereFace
    {
        // Do not change the values, there are things that rely on this.
        Xp = 0,
        Xn = 1,
        Yp = 2,
        Yn = 3,
        Zp = 4,
        Zn = 5
    }

    public static class QuadSphereFaceEx
    {
        /// <summary>
        /// Calculates the <see cref="QuadSphereFace"/> for a given vector.
        /// </summary>
        /// <returns>
        /// The QuadSphereFace corresponding to the vector's maximum axis.
        /// </returns>
        public static QuadSphereFace FromVector( Vector3 vector )
        {
            float x = vector.x;
            float y = vector.y;
            float z = vector.z;

            int maxIndex = 0;
            float maxValue = Math.Abs( x );
            if( Math.Abs( y ) > maxValue )
            {
                maxIndex = 1;
                maxValue = Math.Abs( y );
            }
            if( Math.Abs( z ) > maxValue )
            {
                maxIndex = 2;
            }
            float sign = Mathf.Sign( vector[maxIndex] );

            if( maxIndex == 0 )
            {
                if( sign == 1 ) return QuadSphereFace.Xp;
                if( sign == -1 ) return QuadSphereFace.Xn;
            }
            if( maxIndex == 1 )
            {
                if( sign == 1 ) return QuadSphereFace.Yp;
                if( sign == -1 ) return QuadSphereFace.Yn;
            }
            if( maxIndex == 2 )
            {
                if( sign == 1 ) return QuadSphereFace.Zp;
                if( sign == -1 ) return QuadSphereFace.Zn;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns a unit vector pointing along the face's axis.
        /// </summary>
        public static Vector3 ToVector3( this QuadSphereFace v )
        {
            switch( v )
            {
                case QuadSphereFace.Xp:
                    return new Vector3( 1, 0, 0 );
                case QuadSphereFace.Xn:
                    return new Vector3( -1, 0, 0 );
                case QuadSphereFace.Yp:
                    return new Vector3( 0, 1, 0 );
                case QuadSphereFace.Yn:
                    return new Vector3( 0, -1, 0 );
                case QuadSphereFace.Zp:
                    return new Vector3( 0, 0, 1 );
                case QuadSphereFace.Zn:
                    return new Vector3( 0, 0, -1 );
            }
            throw new ArgumentException( $"Unknown {nameof( QuadSphereFace )} '{v}'.", nameof( v ) );
        }

        /// <returns>Returns the point on the surface of a unit cube corresponding to the specified cube face and face coordinates.</returns>
        public static Vector3 GetSpherePoint( this QuadSphereFace face, float quadX, float quadY )
        {
            // quad x, y go in range [-1..1]
            Contract.Assert( quadX >= -1 && quadX <= 1, $"{nameof( quadX )} has to be in range [-1..1]." );
            Contract.Assert( quadY >= -1 && quadY <= 1, $"{nameof( quadY )} has to be in range [-1..1]." );

            Vector3 pos;
            switch( face )
            {
                case QuadSphereFace.Xp:
                    pos = new Vector3( 1.0f, quadY, quadX );
                    break;
                case QuadSphereFace.Xn:
                    pos = new Vector3( -1.0f, quadX, quadY );
                    break;
                case QuadSphereFace.Yp:
                    pos = new Vector3( quadX, 1.0f, quadY );
                    break;
                case QuadSphereFace.Yn:
                    pos = new Vector3( quadY, -1.0f, quadX );
                    break;
                case QuadSphereFace.Zp:
                    pos = new Vector3( quadY, quadX, 1.0f );
                    break;
                case QuadSphereFace.Zn:
                    pos = new Vector3( quadX, quadY, -1.0f );
                    break;
                default:
                    throw new ArgumentException( $"Invalid face orientation {face}", nameof( face ) );
            }

            pos.Normalize(); // unit sphere.
            return pos;
        }

        /// <returns>Returns the point on the surface of a unit cube corresponding to the specified cube face and face coordinates.</returns>
        public static Vector3Dbl GetSpherePointDbl( this QuadSphereFace face, float quadX, float quadY )
        {
            // quad x, y go in range [-1..1]
            Contract.Assert( quadX >= -1 && quadX <= 1, $"{nameof( quadX )} has to be in range [-1..1]." );
            Contract.Assert( quadY >= -1 && quadY <= 1, $"{nameof( quadY )} has to be in range [-1..1]." );

            Vector3Dbl pos;
            switch( face )
            {
                case QuadSphereFace.Xp:
                    pos = new Vector3Dbl( 1.0, quadY, quadX );
                    break;
                case QuadSphereFace.Xn:
                    pos = new Vector3Dbl( -1.0, quadX, quadY );
                    break;
                case QuadSphereFace.Yp:
                    pos = new Vector3Dbl( quadX, 1.0, quadY );
                    break;
                case QuadSphereFace.Yn:
                    pos = new Vector3Dbl( quadY, -1.0, quadX );
                    break;
                case QuadSphereFace.Zp:
                    pos = new Vector3Dbl( quadY, quadX, 1.0 );
                    break;
                case QuadSphereFace.Zn:
                    pos = new Vector3Dbl( quadX, quadY, -1.0 );
                    break;
                default:
                    throw new ArgumentException( $"Invalid face orientation {face}", nameof( face ) );
            }

            pos.Normalize(); // unit sphere.
            return pos;
        }
    }
}