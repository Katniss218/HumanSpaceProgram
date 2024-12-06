using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    /// <summary>
    /// Represents the surface of a subdivided chunked sphere
    /// </summary>
    public class LODQuadTree
    {
        LODQuadTreeNode[] _nodes = null;

        /// <summary>
        /// Gets the maximum depth of the nodes (the allowed number of descendants).
        /// </summary>
        public int MaxDepth { get; }

        /// <summary>
        /// Gets the collection of 6 root nodes.
        /// </summary>
        /// <remarks>
        /// The returned value is null (for uninitialized tree), or 6 elements long - in the order: Xn, Xp, Yn, Yp, Zn, Zp (same as Direction3D enum).
        /// </remarks>
        public IEnumerable<LODQuadTreeNode> RootNodes => _nodes;

        public bool IsInitialized => _nodes != null;

        public LODQuadTree( int maxDepth )
        {
            this.MaxDepth = maxDepth;
        }

        public LODQuadTreeNode GetRootNode( Direction3D face )
        {
            return _nodes[(int)face];
        }

        internal void SetRootNodes( LODQuadTreeNode[] nodes )
        {
            if( nodes.Length != 6 )
                throw new ArgumentException( $"The nodes array must contain 6 orthogonal nodes.", nameof( nodes ) );

            for( int i = 0; i < nodes.Length; i++ )
            {
                if( nodes[i].Face != (Direction3D)i )
                    throw new ArgumentException( $"The nodes array must be in the order [Xn, Xp, Yn, Yp, Zn, Zp]. See '{typeof( Direction3D ).AssemblyQualifiedName}'.", nameof( nodes ) );
            }

            this._nodes = nodes;
        }
    }
}