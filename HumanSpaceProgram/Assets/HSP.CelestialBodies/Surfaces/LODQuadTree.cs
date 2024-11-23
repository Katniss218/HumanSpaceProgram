using HSP.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public class LODQuadTreeChanges
    {
        public List<LODQuadTreeNode[]> subdivided; // add new children to the parents (make them non-leaves)
                                                   // the new children to add might be recursive, but if so, then the children will already be fully constructed.


        public List<LODQuadTreeNode> unSubdivided; // remove children of these nodes (i.e. make them into leaves)

        // after it's applied, the connectivity into needs to be resolved between these, and the existing nodes. The new nodes will have that done.


        // returns a map of children to add to existing leaf nodes (subdivided), and existing leaf nodes to remove (unsubdivided)
        public static LODQuadTreeChanges GetChanges( LODQuadTree LODQuadTree, IEnumerable<Vector3Dbl> normalizedPois )
        {
            // so we walk the quadtree, if at any point in the traversal of a node:
            //   if the node is a leaf, and any PoI is within `size` of the node, AND in range of its would-be-children 
            //     - recursively subdivide, until the new would-be-children aren't in range.
            //   if the node is not a leaf, and any PoI is within `size` of the node, but not in range of any its children 
            //     - unsubdivide.

            LODQuadTreeChanges changes = new LODQuadTreeChanges();

            // Code ...
            Stack<LODQuadTreeNode> NodesToProcess = new Stack<LODQuadTreeNode>( LODQuadTree.Nodes );
            LODQuadTreeNode CurrentNode = NodesToProcess.Pop();

            while( CurrentNode != null )
            {
                if( CurrentNode.IsLeaf )
                {
                    if( CurrentNode.ShouldSubdivide( normalizedPois ) )
                    {
#warning TODO - force-subdivide neighbors to ensure 2:1 quad borders max?
                        var newNodes = LODQuadTreeNode.Create4SubdividedNodes( CurrentNode ); // non-recursive.
                        changes.subdivided.Add( newNodes );

                        // CHECKME - if I add the new nodes as nodes to process, they would then be processed for further subdivision, right?
                        // only downside that the patch needs to be ordered because there will be `nodes to add` as children of previous `nodes to add`?
                    }
                }
                else
                {
                    if( CurrentNode.ShouldUnsubdivide( normalizedPois ) )
                    {
                        changes.unSubdivided.Add( CurrentNode );
                    }
                    else
                    {
                        foreach( var child in CurrentNode.Children )
                            NodesToProcess.Push( child );
                    }
                }

                CurrentNode = NodesToProcess.Pop();
            }
#warning TODO - changes already need to have the neighbor connectivity info, because changes is the only thing that will be passed into the jobs
            // if we have that, resolving them when applying should be to go to the corresponding neighbor,
            // - and set its neighbors to us + whatever other neighbors it still has, so still nontrivial

            // everything will be a lot easier if I can manage to have a data structure that will allow us to query the 'overlapping leaves' easily, but including the other 5 quads in the sphere.

#warning maybe some sort of coordinate transformation that'll let me 'unfold' the quadtree, so that when changing face, the axis along which the faces meet is the same? (space is not rotated)

            // each pair of possible movements between different quadtrees needs a map that maps the directions so you know which member to access on the other quadtree.

            return changes;
        }

        public void ApplyChanges( LODQuadTree LODQuadTree )
        {
            foreach( var unsubdivided in this.unSubdivided )
            {
                unsubdivided.Children = null;

                // get leaves along each local direction
                // collect a set of distinct quads that are connected on that same local direction from the leaves.
            }
            foreach( var subdividedPair in this.subdivided )
            {
                subdividedPair[0].Parent.Children = subdividedPair;
            }

#error -
            //
            // TODO - resolve connectivity information - traverse leaf nodes, replace deleted children with parents, and replace now-non-leaf parents with their children
            //          while keeping in mind the xy orientation of the particular cube face.

            // can use the center and face direction3d to find the index into children array
#warning INFO - this will not apply the rebuilt quads (frontend), only the (backend) structure of the tree.

#warning TODO - subdivided quads can share the parent's edges, ensuring the sides (siblings) are correct.

#warning TODO - unsubdivided quads can use the siblings' edges, ensuring that we use the correctly positioned sibling for each edge.
            // we can use only 2 diagonally opposite siblings for that.

            // if we allow multiple unsubdivision, then we need to figure out the full list, because the resulting quad might be larger than its neighbors.
            // this might be able to be avoided by keeping track of neighbors on non-leaf nodes as well...
            // - but which level of neighbor we should keep track of, if neighbors are subdivided?

            // when unsubdividing a node patch so that the resulting node is larger than the neighbors, there's a problem.

            // subdivisions/unsubdivisions being in a recursive order would help a lot with this.
            // but simultaneous edit and neighbor resolution has order problems I think...

            // only keep track of *equal or larger* neighbors??? (as was done previously) - might be problematic for getting the meshes of neighbors.
            // - getting the meshes is still possible, but requires navigating the tree.
            // - what if we keep the meshes all the way up to the l0, just not display them??? - that would let us use the largest neighbor mesh when remeshing a node.

            // Storing all meshes instead of just the top ones increases the storage by 2 times, but also lets us do some optimizations when unsubdividing.
            // - and could be extended to precalculating the meshes for not-yet-subdivided quads.
            // storing all meshes of existing quads would also let us only deal with larger or equal meshes when remeshing, no need for mesh arrays and stuff.
            // - larger lod quads could be disabled and not render. Or be created on demand if the lodquadtree held the mesh reference.
        }
    }

    public class LODQuadTree
    {
        LODQuadTreeNode[] _nodes; // 6 elements long, xyz+-

        /// <summary>
        /// The maximum nesting level of the leaf nodes.
        /// </summary>
        public int MaxDepth { get; }

        public IEnumerable<LODQuadTreeNode> Nodes => _nodes;

        public LODQuadTree( int maxDepth )
        {
            this.MaxDepth = maxDepth;
            //this._nodes = new LODQuadTreeNode[6];

#warning TODO - initialize the 6 starting faces. WARNING, in this scheme, they would have to have a mesh immediately.
        }
    }

    public class LODQuadTreeNode
    {
        public LODQuadTreeNode[] Children; // null or 4, never more or less
        public LODQuadTreeNode Parent;

        /// <remarks>
        /// Contains itself (!)
        /// </remarks>
        public LODQuadTreeNode[] Siblings => this.Parent?.Children;

        public int SubdivisionLevel;
        public float Size => 1.0f / (float)(1 << SubdivisionLevel); // starts at 1, and halves at every subdivision (depth level)
        public Vector3Dbl Center; // position of the center of the node, normalized to radius = 1
        public Direction3D Face; // which of the 6 faces the quad is on. determines local space orientation.
        public Vector2 FaceCenter;

        public bool IsLeaf => Children == null;
        public bool IsRoot => Parent == null;

        // connectivity info in local xy space of the current node, can be 1, 2, 3, 4, 5, etc. elements long
        // These fields only exist on leaf nodes, and they only point to other leaf nodes.
        public LODQuadTreeNode[] Xn;
        public LODQuadTreeNode[] Xp;
        public LODQuadTreeNode[] Yn;
        public LODQuadTreeNode[] Yp;

        public LODQuadTreeNode GetChild( int childYIndex, int childXIndex )
        {
            return Children[GetChildIndex( childYIndex, childXIndex )];
        }

        public bool ShouldSubdivide( IEnumerable<Vector3Dbl> localPois )
        {
            foreach( var localPoi in localPois )
            {
                float distance = (float)(localPoi - Center).magnitude;

                if( distance < Size )
                {
                    return true;
                }
            }

            return false;
        }

        public bool ShouldUnsubdivide( IEnumerable<Vector3Dbl> localPois )
        {
            //foreach( var siblingNode in this.Siblings )
            //{
#warning INFO - I think it would be enough to just discard the 'old' leaves now, with the remesher.
            // Don't unsubdivide if one of the siblings is subdivided. That would require handling nested unsubdivisions, and is nasty, blergh and unnecessary.
            //    if( !siblingNode.IsLeaf )
            //    {
            //        return false;
            //    }
            //}

            foreach( var localPoi in localPois )
            {
                float distance = (float)(localPoi - Center).magnitude;

                // Inverse of ShouldSubdivide.
                if( distance >= this.Size )
                {
                    return true;
                }
            }

            return false;
        }

        public static LODQuadTreeNode[] Create4SubdividedNodes( LODQuadTreeNode node )
        {
            LODQuadTreeNode[] _4_quads = new LODQuadTreeNode[4];

            int newSubdivisionLevel = node.SubdivisionLevel + 1;

            for( int i = 0; i < 4; i++ )
            {
                (int xIndex, int yIndex) = GetChildIndex( i );

                Vector2 newFaceCenter = GetChildFaceCenter( node, xIndex, yIndex );
                Vector3Dbl newCenter = node.Face.GetSpherePoint( newFaceCenter.x, newFaceCenter.y );


                LODQuadTreeNode newNode = new LODQuadTreeNode();
                newNode.Parent = node;
                // do not set children.
                newNode.Center = newCenter;
                newNode.FaceCenter = newFaceCenter;
                newNode.SubdivisionLevel = newSubdivisionLevel;


                _4_quads[i] = newNode;
            }

            return _4_quads;
        }

        public IEnumerable<LODQuadTreeNode> GetXPlusLeafNeighbors()
        {

        }

        public static int GetChildIndex( int childYIndex, int childXIndex )
        {
            return childYIndex * 2 + childXIndex;
        }

        public static (int childXIndex, int childYIndex) GetChildIndex( int i )
        {
            int x = i % 2;
            int y = i / 2;

            return (x, y);
        }

        public static Vector2 GetChildFaceCenter( LODQuadTreeNode node, int childXIndex, int childYIndex )
        {
            // For both x and y, it should return:
            // - childIndex == 0 => child < parent
            // - childIndex == 1 => child > parent

            float halfSize = node.Size / 2.0f;
            float quarterSize = node.Size / 4.0f;

            float x = node.FaceCenter.x - quarterSize + (childXIndex * halfSize);
            float y = node.FaceCenter.y - quarterSize + (childYIndex * halfSize);
            return new Vector2( x, y );
        }

        public static (int childXIndex, int childYIndex) GetChildIndex( LODQuadTreeNode node )
        {
            int x = node.FaceCenter.x < node.Parent.FaceCenter.x ? 0 : 1;
            int y = node.FaceCenter.y < node.Parent.FaceCenter.y ? 0 : 1;
            return (x, y);
        }
    }
}