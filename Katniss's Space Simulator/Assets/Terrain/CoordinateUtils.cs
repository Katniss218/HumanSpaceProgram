using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    /// <summary>
    /// A class grouping methods related specifically to coordinates that are relevant to the terrain system.
    /// </summary>
    public static class CoordinateUtils
    {
        /// <summary>
        /// Z+ is up.
        /// </summary>
        public static Vector3 UVToCartesian( float u, float v, float radius )
        {
            float theta = u * (2 * Mathf.PI) - Mathf.PI; // Multiplying by 2 because the input is in range [0..1]
            float phi = v * Mathf.PI;

            float x = radius * Mathf.Sin( phi ) * Mathf.Cos( theta );
            float y = radius * Mathf.Sin( phi ) * Mathf.Sin( theta );
            float z = radius * Mathf.Cos( phi );

            return new Vector3( x, y, z );
        }

        /// <summary>
        /// Z+ is up (V+), seam is in the direction of X-.
        /// U increases from 0 on the Y- side of the seam, decreases from 1 on the Y+ side.
        /// </summary>
        public static Vector2 CartesianToUV( float x, float y, float z )
        {
            // If we know for sure that the coordinates are for a unit sphere, we can remove the radius calculation entirely.
            // Also remove the division by radius, since division by 1 (unit-sphere) doesn't change the divided number.
            float radius = Mathf.Sqrt( x * x + y * y + z * z );
            float theta = Mathf.Atan2( y, x );
            float phi = Mathf.Acos( z / radius );

            // The thing returned here seems to also be the lat/lon but normalized to the range [0..1]
            // The outputs are normalized by the respective denominators.
            float u = (theta + Mathf.PI) / (2 * Mathf.PI); // dividing by 2 * pi ensures that the output is in [0..1]
            float v = phi / Mathf.PI;
            return new Vector2( u, v );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns the normalized point on the surface of a cube, and the vector transforming the center-origin coordinates into face-origin.</returns>
        public static (Vector3 pos, Vector3 posOffset) GetSpherePoint( int i, int j, float edgeLength, float radius, QuadSphereFace face )
        {
            Vector3 pos;
            Vector3 posOffset;
            switch( face )
            {
                case QuadSphereFace.Xp:
                    pos = new Vector3( radius, (j * edgeLength) - radius, (i * edgeLength) - radius );
                    posOffset = new Vector3( radius, 0, 0 );
                    break;
                case QuadSphereFace.Xn:
                    pos = new Vector3( -radius, (i * edgeLength) - radius, (j * edgeLength) - radius );
                    posOffset = new Vector3( -radius, 0, 0 );
                    break;
                case QuadSphereFace.Yp:
                    pos = new Vector3( (i * edgeLength) - radius, radius, (j * edgeLength) - radius );
                    posOffset = new Vector3( 0, radius, 0 );
                    break;
                case QuadSphereFace.Yn:
                    pos = new Vector3( (j * edgeLength) - radius, -radius, (i * edgeLength) - radius );
                    posOffset = new Vector3( 0, -radius, 0 );
                    break;
                case QuadSphereFace.Zp:
                    pos = new Vector3( (j * edgeLength) - radius, (i * edgeLength) - radius, radius );
                    posOffset = new Vector3( 0, 0, radius );
                    break;
                case QuadSphereFace.Zn:
                    pos = new Vector3( (i * edgeLength) - radius, (j * edgeLength) - radius, -radius );
                    posOffset = new Vector3( 0, 0, -radius );
                    break;
                default:
                    throw new ArgumentException( $"Invalid face orientation {face}", nameof( face ) );
            }

            pos.Normalize(); // unit sphere.
            return (pos, posOffset);
        }
    }
}