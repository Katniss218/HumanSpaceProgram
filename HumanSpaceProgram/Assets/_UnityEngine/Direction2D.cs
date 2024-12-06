using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    /// <summary>
    /// Represents a basis direction in 2D space.
    /// </summary>
    /// <remarks>
    /// Usable as an index into an array to keep track of things in each direction.
    /// </remarks>
    public enum Direction2D
    {
        // Not supposed to be combined into compound directions (like XY).
        Xn = 0, // DO NOT CHANGE THE VALUES.
        Xp = 1,

        Yn = 2,
        Yp = 3
    }

    public static class Direction2DUtils
    {
        static readonly Direction2D[] _inverseDir = new Direction2D[4]
        {
            Direction2D.Xp,
            Direction2D.Xn,
            Direction2D.Yp,
            Direction2D.Yn
        };

        static readonly Vector2[] _directionVectors = new Vector2[4]
        {
            new Vector2( -1, 0 ),
            new Vector2( 1, 0 ),
            new Vector2( 0, -1 ),
            new Vector2( 0, 1 ),
        };

        public static readonly Direction2D[] Every = new Direction2D[4]
        {
            Direction2D.Xn,
            Direction2D.Xp,
            Direction2D.Yn,
            Direction2D.Yp
        };

        /// <summary>
        /// Inverts the direction.
        /// </summary>
        public static Direction2D Inverse( this Direction2D dir )
        {
            return _inverseDir[(int)dir];
        }

        private static Direction2D FromX( float x )
        {
            if( x == 0 )
                throw new ArgumentOutOfRangeException( nameof( x ), $"Direction value can't be 0, because 0 doesn't point in any direction." );

            if( x < 0 )
            {
                return Direction2D.Xn;
            }

            return Direction2D.Xp;
        }

        private static Direction2D FromY( float y )
        {
            if( y == 0 )
                throw new ArgumentOutOfRangeException( nameof( y ), $"Direction value can't be 0, because 0 doesn't point in any direction." );

            if( y < 0 )
            {
                return Direction2D.Yn;
            }

            return Direction2D.Yp;
        }

        public static Vector2 ToVector2( this Direction2D dir )
        {
            return _directionVectors[(int)dir];
        }
    }
}