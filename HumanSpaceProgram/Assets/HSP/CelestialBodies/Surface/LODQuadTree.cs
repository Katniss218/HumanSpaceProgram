using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.CelestialBodies.Surface
{
    public class LODQuadTree
    {
        public class Node
        {
            /// <summary>
            /// A way to retrieve the quad from the quadtree.
            /// </summary>
            public LODQuad Value { get; set; }

            public float minX { get; }
            public float maxX { get; }
            public float minY { get; }
            public float maxY { get; }

            /// <summary>
            /// Calculates and returns the center of the node.
            /// </summary>
            public Vector2 Center => new Vector2( (minX + maxX) / 2.0f, (minY + maxY) / 2.0f );
            /// <summary>
            /// Calculates and returns the (square) edge length of the node.
            /// </summary>
            public float Size => maxX - minX; // Nodes are supposed to be square, so we can use either coordinate.

            /// <summary>
            /// The root node of the entire quadtree.
            /// </summary>
            public Node Root { get; private set; }
            public Node Parent { get; private set; }

            public Node[,] Children { get; private set; }

            /// <remarks>
            /// Contains itself (!)
            /// </remarks>
            public Node[,] Siblings => this.Parent?.Children;

            public void MakeLeaf()
            {
                foreach( var child in this.Children )
                {
                    child.RemoveFromHierarchy();
                }

                this.Children = null;
            }

            private void RemoveFromHierarchy()
            {
                this.Root = null;
                this.Parent = null;
            }

            public Node( Node parent, Vector2 center, float size )
            {
                float halfSize = size / 2.0f;
                this.minX = center.x - halfSize;
                this.maxX = center.x + halfSize;
                this.minY = center.y - halfSize;
                this.maxY = center.y + halfSize;

                this.Parent = parent;
                if( parent == null )
                {
                    this.Root = this;
                }
                else
                {
                    this.Root = parent.Root;
                    if( parent.Children == null )
                    {
                        parent.Children = new Node[2, 2];
                    }
                    (int x, int y) = LODQuadTree_NodeUtils.GetChildIndex( this );
                    parent.Children[x, y] = this;
                }
            }

            /// <remarks>
            /// Includes nodes that are intersecting, as well as if the edges or corners touch.
            /// </remarks>
            public List<Node> QueryOverlappingLeaves( float minX, float minY, float maxX, float maxY )
            {
#warning TODO - Add a way to include other faces in the query. And not just the quad face of the current quad. Combine the 6 quadtrees into one datastructure.
                // return the list of nodes that overlap with the region.

                // we could also group the results based on direction from the center here.

                if( this.Intersects( minX, minY, maxX, maxY ) )
                {
                    if( this.Children == null )
                    {
                        return new List<Node>() { this };
                    }

                    List<Node> nodes = new List<Node>();

                    foreach( var child in this.Children )
                    {
                        nodes.AddRange( child.QueryOverlappingLeaves( minX, minY, maxX, maxY ) );
                    }

                    return nodes;
                }

                return new List<Node>();
            }
        }

        public Node Root { get; private set; }

        public LODQuadTree( Node root )
        {
            Root = root;
        }


        List<LODQuad> GetNonNullLeafNodes( Node rootNode )
        {
            // This could be further optimized later by caching the list of leaf nodes, if needed (update cache when node is added/removed).

            List<LODQuad> leafNodes = new List<LODQuad>();
            Stack<Node> stack = new Stack<Node>();
            stack.Push( rootNode );

            while( stack.Count > 0 )
            {
                Node currentNode = stack.Pop();
                if( currentNode.Children == null )
                {
                    if( currentNode.Value != null )
                    {
                        leafNodes.Add( currentNode.Value );
                    }
                }
                else
                {
                    foreach( Node child in currentNode.Children )
                    {
                        stack.Push( child );
                    }
                }
            }

            return leafNodes;
        }

        public List<LODQuad> GetNonNullLeafNodes()
        {
            return GetNonNullLeafNodes( Root );
        }
    }
}