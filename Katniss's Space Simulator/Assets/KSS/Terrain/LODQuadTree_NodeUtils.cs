using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Terrain
{
    public static class LODQuadTree_NodeUtils
    {
        public static (int childXIndex, int childYIndex) GetChildIndex( int i )
        {
            int x = i % 2;
            int y = i / 2;

            return (x, y);
        }

        /// <summary>
        /// Calculates the center of a child with the specified index for a given node.
        /// </summary>
        public static Vector2 GetChildCenter( this LODQuadTree.Node node, int childXIndex, int childYIndex )
        {
            // For both x and y, it should return:
            // - childIndex == 0 => child < parent
            // - childIndex == 1 => child > parent

            float halfSize = node.Size / 2.0f;
            float quarterSize = node.Size / 4.0f;

            float x = node.Center.x - quarterSize + (childXIndex * halfSize);
            float y = node.Center.y - quarterSize + (childYIndex * halfSize);
            return new Vector2( x, y );
        }

        /// <summary>
        /// Inverse of <see cref="GetChildCenter"/>.
        /// </summary>
        public static (int childXIndex, int childYIndex) GetChildIndex( this LODQuadTree.Node node )
        {
            int x = node.minX == node.Parent.minX ? 0 : 1; // if min/max of a given node matches the min/max of the parent, it's in the min/max quadrant.
            int y = node.minY == node.Parent.minY ? 0 : 1;
            return (x, y);
        }

        public static bool Intersects( this LODQuadTree.Node node, float minX, float minY, float maxX, float maxY )
        {
            return (node.minX <= maxX && node.maxX >= minX)
                && (node.minY <= maxY && node.maxY >= minY);
        }

        public static int GetEdgeIndex( Vector2 selfToOther )
        {
            float x = selfToOther.x;
            float y = selfToOther.y;

            if( x < 0 && x < y ) // left
                return 0;
            if( x > 0 && x > y ) // right
                return 1;
            if( y < 0 && y < x ) // up
                return 2;
            if( y > 0 && y > x ) // down
                return 3;
            throw new ArgumentException( $"Invalid vector {selfToOther}" );
        }

        /// <summary>
        /// Calculates the quad size for a given subdivision level.
        /// </summary>
        public static float GetSize( int lN )
        {
            // For each subsequent subdivision, the size halves.
            // Starting at size = 2 for lN = 0.

            int pow = 1 << lN; // Fast 2 ^ n for integer types.

            return 2.0f / (float)pow;
        }

        [Obsolete( "Not tested" )]
        /// <summary>
        /// Calculates the (floored) subdivision level for a given quad size.
        /// </summary>
        public static int GetSubdivisionLevel( float size )
        {
            float value = 2f / size;
            int subdivisionLevel = (int)Mathf.Log( value, 2 );

            return subdivisionLevel;
        }
    }
}