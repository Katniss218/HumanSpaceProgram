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
        }

        public Node Root { get; set; }


        List<LODQuad> GetLeafNodes( Node node )
        {
            List<LODQuad> leafNodes = new List<LODQuad>();

            if( node.Children == null ) // this node has no children, so it's a leaf node
            {
                leafNodes.Add( node.Value );
            }
            else
            {
                // recursively traverse each child node and add its leaf nodes to the list
                foreach( Node child in node.Children )
                {
                    leafNodes.AddRange( GetLeafNodes( child ) );
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
