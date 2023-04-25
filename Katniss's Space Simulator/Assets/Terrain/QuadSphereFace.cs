using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
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
        /// Returns the quad sphere face for a given vector.
        /// </summary>
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
    }
}
