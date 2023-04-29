using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    public static class LODQuadUtils
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

            float halfSize = node.Size / 2f;
            float quarterSize = node.Size / 4f;

            float x = node.Center.x - quarterSize + (childXIndex * halfSize);
            float y = node.Center.y - quarterSize + (childYIndex * halfSize);
            return new Vector2( x, y );
        }

        /// <summary>
        /// Inverse of <see cref="GetChildCenter"/>.
        /// </summary>
        public static (int childXIndex, int childYIndex) GetChildIndex( Vector2 parentCenter, Vector2 childCenter )
        {
            int x = childCenter.x < parentCenter.x ? 0 : 1;
            int y = childCenter.y < parentCenter.y ? 0 : 1;
            return (x, y);
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
    }
}