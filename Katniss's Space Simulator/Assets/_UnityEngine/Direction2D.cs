using System;
using System.Collections.Generic;
using System.Linq;

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
        // Not supposed to be combinable.

        // If N-dimensional directions ever get created, they should have the indices [0..2N-1], with x first, then y, then z, then w, etc...
        // - This will keep them directly usable as edge indices.
        Xn = 0,
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

        // move this somewhere, potentially add an enum to represent directions.
        static readonly Vector2[] _directionVectors = new Vector2[4]
        {
            new Vector2( -1f, 0f ),
            new Vector2( 1f, 0f ),
            new Vector2( 0f, -1f ),
            new Vector2( 0f, 1f ),
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

        /// <summary>
        /// Inverts an array of directions.
        /// </summary>
        /// <returns>The directions that were not present in the array.</returns>
        public static Direction2D[] Inverse( this Direction2D[] dir )
        {
            List<Direction2D> r = new List<Direction2D>();
            if( !dir.Contains( Direction2D.Xn ) )
                r.Add( Direction2D.Xn );
            if( !dir.Contains( Direction2D.Xp ) )
                r.Add( Direction2D.Xp );
            if( !dir.Contains( Direction2D.Yn ) )
                r.Add( Direction2D.Yn );
            if( !dir.Contains( Direction2D.Yp ) )
                r.Add( Direction2D.Yp );
            return r.ToArray();
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

        public static Direction2D[] FromVector2( Vector2 dir )
        {
            // Handle cases where the vector returns one direction.
            if( dir.x == 0 )
                return new Direction2D[] { FromY( dir.y ) };
            if( dir.y == 0 )
                return new Direction2D[] { FromY( dir.x ) };

            // The general case with 2 directions.
            Direction2D[] r = new Direction2D[2]
            {
                 FromX( dir.x ),
                 FromY( dir.y )
            };

            return r;
        }

        public static Vector2 ToVector2( this Direction2D dir )
        {
            return _directionVectors[(int)dir];
        }
    }
}