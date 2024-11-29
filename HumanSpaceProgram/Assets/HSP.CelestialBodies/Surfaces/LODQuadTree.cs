using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public class LODQuadTree
    {
        LODQuadTreeNode[] _nodes = null; // Null, or 6 elements long - xn, xp, yn, yp, zn, zp (same order as Direction3D enum).

        /// <summary>
        /// The maximum nesting level of the leaf nodes.
        /// </summary>
        public int MaxDepth { get; }

        public IEnumerable<LODQuadTreeNode> Nodes => _nodes;

        public LODQuadTree( int maxDepth )
        {
            this.MaxDepth = maxDepth;
        }

        public LODQuadTreeNode GetRootNode( Direction3D face )
        {
            return _nodes[(int)face];
        }

        /*public void SetRootNode( Direction3D face, LODQuadTreeNode value )
        {
            if( value.Face != face )
            {
                throw new ArgumentException( $"The node's {nameof( LODQuadTreeNode.Face )} field must match the specified {nameof( face )} parameter.", nameof( value ) );
            }
            if( value.SubdivisionLevel != 0 )
            {
                throw new ArgumentException( $"The node must have subdivision level of 0 (i.e. be a root node).", nameof( value ) );
            }

            // resolve connectivity with the new node.

            _nodes[(int)face] = value;
        }*/

        internal void SetNodes( LODQuadTreeNode[] nodes )
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