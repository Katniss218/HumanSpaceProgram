using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Terrain
{
    public class LODQuadTree
    {
        public class Node
        {
            public LODQuad Value { get; set; }

            public float minX { get; }
            public float maxX { get; }
            public float minY { get; }
            public float maxY { get; }

            public Vector2 Center => new Vector2( (minX + maxX) / 2.0f, (minY + maxY) / 2.0f );
            public float Size => maxX - minX;

            public Node Root { get; private set; }
            public Node Parent { get; private set; }

            public Node[,] Children { get; private set; }

            /// <summary>
            /// Contains itself (!)
            /// </summary>
            public Node[,] Siblings => this.Parent?.Children;

            public void MakeLeafNode()
            {
                foreach( var child in this.Children )
                {
                    child.Root = null;
                    child.Parent = null;
                }

                this.Children = null;
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

            public List<Node> QueryLeafNodes( float minX, float minY, float maxX, float maxY )
            {
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
                        nodes.AddRange( child.QueryLeafNodes( minX, minY, maxX, maxY ) );
                    }

                    return nodes;
                }

                return new List<Node>();
            }
        }

        public Node Root { get; set; }


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