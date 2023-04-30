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
            public Vector2 Center { get; set; }
            public float Size { get; set; }

            public Node Parent { get; set; }

            public Node[,] Children { get; set; }

            /// <summary>
            /// Contains itself (!)
            /// </summary>
            public Node[,] Siblings { get => this.Parent.Children; }
        }

        public Node Root { get; set; }


        List<LODQuad> GetLeafNodes( Node rootNode )
        {
            // This could be further optimized later by caching the list of nodes, if needed.

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

        public List<LODQuad> GetLeafNodes()
        {
            return GetLeafNodes( Root );
        }
    }
}