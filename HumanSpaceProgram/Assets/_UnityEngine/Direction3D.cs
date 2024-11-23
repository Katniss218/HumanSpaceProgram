using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
    /// <summary>
    /// Represents a basis direction in 3D space.
    /// </summary>
    /// <remarks>
    /// Usable as an index into an array to keep track of things in each direction.
    /// </remarks>
    public enum Direction3D
    {
        // Not supposed to be combinable.
        Xn = 0,
        Xp = 1,

        Yn = 2,
        Yp = 3,

        Zn = 4,
        Zp = 5

        // Actually, maybe make this as a struct? that way it'd have more options, like operators (mult by direction for example), etc.
    }

    public static class Direction3DUtils
    {
        /// <summary>
        /// Calculates the <see cref="Direction3D"/> for a given vector.
        /// </summary>
        /// <returns>
        /// The QuadSphereFace corresponding to the vector's maximum axis.
        /// </returns>
        public static Direction3D BasisFromVector( Vector3 vector )
        {
            float x = vector.x;
            float y = vector.y;
            float z = vector.z; // What it does is a combo between getting the maximum axis, and then getting a direction from that.

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
                if( sign == 1 ) return Direction3D.Xp;
                if( sign == -1 ) return Direction3D.Xn;
            }
            if( maxIndex == 1 )
            {
                if( sign == 1 ) return Direction3D.Yp;
                if( sign == -1 ) return Direction3D.Yn;
            }
            if( maxIndex == 2 )
            {
                if( sign == 1 ) return Direction3D.Zp;
                if( sign == -1 ) return Direction3D.Zn;
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns a unit vector pointing along the face's axis.
        /// </summary>
        public static Vector3 ToVector3( this Direction3D dir )
        {
            switch( dir )
            {
                case Direction3D.Xp:
                    return new Vector3( 1, 0, 0 );
                case Direction3D.Xn:
                    return new Vector3( -1, 0, 0 );
                case Direction3D.Yp:
                    return new Vector3( 0, 1, 0 );
                case Direction3D.Yn:
                    return new Vector3( 0, -1, 0 );
                case Direction3D.Zp:
                    return new Vector3( 0, 0, 1 );
                case Direction3D.Zn:
                    return new Vector3( 0, 0, -1 );
            }
            throw new ArgumentException( $"Unknown {nameof( Direction3D )} '{dir}'.", nameof( dir ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 GetSpherePoint( this Direction3D face, Vector2 quadXY )
        {
            return GetSpherePoint( face, quadXY.x, quadXY.y );
        }

        /// <summary>
        /// Gets the direction where `to` is located in local space of `from`, and where `from` is located in local space of `to`
        /// </summary>
        /// <param name="first">The first 3D axis.</param>
        /// <param name="second">The second 3D axis.</param>
        /// <returns>A bidirectional mapping describing where in local space on the cube each axis is located.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="first"/> and <paramref name="second"/> are located on the same axis.</exception>
        public static (Direction2D fromFirstToSecond, Direction2D fromSecondToFirst) GetLocalDirection( Direction3D first, Direction3D second )
        {
            return first switch
            {
                Direction3D.Xn => second switch
                {
                    Direction3D.Yn => (Direction2D.Xn, Direction2D.Xp),// Xn(3) -> Yn(3) requires going along Xn(2) in Xn(3), Yn(3) -> Xn(3) requires going along Xp(2) in Yn(3)
                    Direction3D.Yp => (Direction2D.Xp, Direction2D.Xn),
                    Direction3D.Zp => (Direction2D.Yp, Direction2D.Yn),
                    Direction3D.Zn => (Direction2D.Yn, Direction2D.Yp),
                    _ => throw new ArgumentException( $"{first} doesn't share an edge with {second}.", nameof( second ) ),
                },
                Direction3D.Xp => second switch
                {
                    Direction3D.Yn => (Direction2D.Xp, Direction2D.Xn),
                    Direction3D.Yp => (Direction2D.Xn, Direction2D.Xp),
                    Direction3D.Zp => (Direction2D.Yp, Direction2D.Yp),
                    Direction3D.Zn => (Direction2D.Yn, Direction2D.Yn),
                    _ => throw new ArgumentException( $"{first} doesn't share an edge with {second}.", nameof( second ) ),
                },
                Direction3D.Yn => second switch
                {
                    Direction3D.Xn => (Direction2D.Xp, Direction2D.Xn),
                    Direction3D.Xp => (Direction2D.Xn, Direction2D.Xp),
                    Direction3D.Zn => (Direction2D.Yn, Direction2D.Xn),
                    Direction3D.Zp => (Direction2D.Yp, Direction2D.Xn),
                    _ => throw new ArgumentException( $"{first} doesn't share an edge with {second}.", nameof( second ) ),
                },
                Direction3D.Yp => second switch
                {
                    Direction3D.Xn => (Direction2D.Xn, Direction2D.Xp),
                    Direction3D.Xp => (Direction2D.Xp, Direction2D.Xn),
                    Direction3D.Zn => (Direction2D.Yn, Direction2D.Xp),
                    Direction3D.Zp => (Direction2D.Yp, Direction2D.Xp),
                    _ => throw new ArgumentException( $"{first} doesn't share an edge with {second}.", nameof( second ) ),
                },
                Direction3D.Zn => second switch
                {
                    Direction3D.Xn => (Direction2D.Yp, Direction2D.Yn),
                    Direction3D.Xp => (Direction2D.Yn, Direction2D.Yn),
                    Direction3D.Yn => (Direction2D.Xn, Direction2D.Yn),
                    Direction3D.Yp => (Direction2D.Xp, Direction2D.Yn),
                    _ => throw new ArgumentException( $"{first} doesn't share an edge with {second}.", nameof( second ) ),
                },
                Direction3D.Zp => second switch
                {
                    Direction3D.Xn => (Direction2D.Yn, Direction2D.Yp),
                    Direction3D.Xp => (Direction2D.Yp, Direction2D.Yp),
                    Direction3D.Yn => (Direction2D.Xn, Direction2D.Yp),
                    Direction3D.Yp => (Direction2D.Xp, Direction2D.Yp),
                    _ => throw new ArgumentException( $"{first} doesn't share an edge with {second}.", nameof( second ) ),
                },
                _ => throw new ArgumentException( $"{first} doesn't share an edge with {second}.", nameof( first ) ),
            };
        }


        /// <returns>Returns the point on the surface of a unit cube corresponding to the specified cube face and face coordinates.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3 GetSpherePoint( this Direction3D face, float quadX, float quadY )
        {
            // quad x, y go in range [-1..1]
            Contract.Assert( quadX >= -1 && quadX <= 1, $"{nameof( quadX )} has to be in range [-1..1]." );
            Contract.Assert( quadY >= -1 && quadY <= 1, $"{nameof( quadY )} has to be in range [-1..1]." );
            var pos = face switch
            {
                Direction3D.Xp => new Vector3( 1.0f, -quadX, quadY ),
                Direction3D.Xn => new Vector3( -1.0f, quadX, quadY ),
                Direction3D.Yp => new Vector3( quadX, 1.0f, quadY ),
                Direction3D.Yn => new Vector3( -quadX, -1.0f, quadY ),
                Direction3D.Zp => new Vector3( quadY, quadX, 1.0f ),
                Direction3D.Zn => new Vector3( -quadY, quadX, -1.0f ),
                _ => throw new ArgumentException( $"Invalid face orientation {face}", nameof( face ) ),
            };
            pos.Normalize(); // unit sphere.
            return pos;
        }

        /// <returns>Returns the point on the surface of a unit cube corresponding to the specified cube face and face coordinates.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl GetQuadPointDbl( this Direction3D face, float quadX, float quadY )
        {
            // quad x, y go in range [-1..1]
            Contract.Assert( quadX >= -1 && quadX <= 1, $"{nameof( quadX )} has to be in range [-1..1]." );
            Contract.Assert( quadY >= -1 && quadY <= 1, $"{nameof( quadY )} has to be in range [-1..1]." );

            Vector3Dbl pos;
            switch( face )
            {
                case Direction3D.Xp:
                    pos = new Vector3Dbl( 1.0, -quadX, quadY );
                    break;
                case Direction3D.Xn:
                    pos = new Vector3Dbl( -1.0, quadX, quadY );
                    break;
                case Direction3D.Yp:
                    pos = new Vector3Dbl( quadX, 1.0, quadY );
                    break;
                case Direction3D.Yn:
                    pos = new Vector3Dbl( -quadX, -1.0, quadY );
                    break;
                case Direction3D.Zp:
                    pos = new Vector3Dbl( quadY, quadX, 1.0 );
                    break;
                case Direction3D.Zn:
                    pos = new Vector3Dbl( -quadY, quadX, -1.0 );
                    break;
                default:
                    throw new ArgumentException( $"Invalid face orientation {face}", nameof( face ) );
            }

            return pos;
        }

        /// <returns>Returns the point on the surface of a unit cube corresponding to the specified cube face and face coordinates.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl GetSpherePointDbl( this Direction3D face, float quadX, float quadY )
        {
            Vector3Dbl pos = GetQuadPointDbl( face, quadX, quadY );
            pos.Normalize(); // unit sphere.
            return pos;
        }
    }
}