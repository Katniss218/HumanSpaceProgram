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
        Xp = 0,
        Xn = 1,
        Yp = 2,
        Yn = 3,
        Zp = 4,
        Zn = 5
    }

    public static class QuadSphereFaceEx
    {
        public static QuadSphereFace FromVector( Vector3Dbl preciseVector )
        {
            Contract.Assert( preciseVector.magnitude == 1.0 );
            // Find which axis is the largest.

            double x = preciseVector.x;
            double y = preciseVector.y;
            double z = preciseVector.z;

            if( x > y && x > z )
                return QuadSphereFace.Xp;
            if( x < y && x < z )
                return QuadSphereFace.Xn;

            if( y > x && y > z )
                return QuadSphereFace.Yp;
            if( y < x && y < z )
                return QuadSphereFace.Yn;

            if( z > x && z > y )
                return QuadSphereFace.Zp;
            if( z < x && z < y )
                return QuadSphereFace.Zn;

            throw new ArgumentException( $"Invalid vector {preciseVector}.", nameof( preciseVector ) );
        }
    }
}
