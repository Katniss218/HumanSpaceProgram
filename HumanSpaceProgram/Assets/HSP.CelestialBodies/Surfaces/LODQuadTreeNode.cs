using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public class LODQuadTreeNode
    {
        /// <summary>
        /// The children of this node. <br/>
        /// Null for leaf nodes.
        /// </summary>
        public (LODQuadTreeNode xnyn, LODQuadTreeNode xpyn, LODQuadTreeNode xnyp, LODQuadTreeNode xpyp)? Children { get; internal set; }

        /// <summary>
        /// The parent of this node. <br/>
        /// Null for root nodes.
        /// </summary>
        public LODQuadTreeNode Parent { get; internal set; }

        /// <summary>
        /// Returns the 4-tuple of quads that share the common parent.
        /// </summary>
        /// <remarks>
        /// Contains itself (!) <br/>
        /// </remarks>
        public (LODQuadTreeNode xnyn, LODQuadTreeNode xpyn, LODQuadTreeNode xnyp, LODQuadTreeNode xpyp) Siblings
        {
            get
            {
                if( Parent == null )
                    throw new InvalidOperationException( $"A root node has no siblings." );

                return this.Parent.Children.Value;
            }
        }

        /// <summary>
        /// The depth of this quad. <br/>
        /// Starts at 0 for root nodes, increases by 1 for every subdivision.
        /// </summary>
        public int SubdivisionLevel { get; }

        /// <summary>
        /// The size of this node.
        /// </summary>
        /// <remarks>
        /// This is equal to the edge length of the quad on its cube face. <br/>
        /// Starts at 2 for subdivision level 0, and halves with every subdivision.
        /// </remarks>
        public float Size => 2.0f / (float)(1 << SubdivisionLevel);

        /// <summary>
        /// The 3D center of the quad. Normalized to the surface of the sphere.
        /// </summary>
        public Vector3Dbl SphereCenter { get; }

        /// <summary>
        /// The face of the cube. Determines which of the 6 cube faces this quad belongs to.
        /// </summary>
        public Direction3D Face { get; }

        /// <summary>
        /// The center of the quad on its cube face.
        /// </summary>
        public Vector2 FaceCenter { get; }

        /// <summary>
        /// True if the node is a leaf (has no children)
        /// </summary>
        public bool IsLeaf => Children == null;

        /// <summary>
        /// True if the node is a root (has no parent)
        /// </summary>
        public bool IsRoot => Parent == null;

        // Connectivity info in local xy space of the current node.
        // These will point to the largest existing neighbor (in the particular direction) that's the same size or larger than this node.
        // As a result, they don't necessarily point to nodes that are visible.
        /// <summary>
        /// The neighbor node along the negative X local direction.
        /// </summary>
        public LODQuadTreeNode Xn;
        /// <summary>
        /// The neighbor node along the positive X local direction.
        /// </summary>
        public LODQuadTreeNode Xp;
        /// <summary>
        /// The neighbor node along the negative Y local direction.
        /// </summary>
        public LODQuadTreeNode Yn;
        /// <summary>
        /// The neighbor node along the positive Y local direction.
        /// </summary>
        public LODQuadTreeNode Yp;

        public LODQuadTreeNode( int subdivisionLevel, Vector3Dbl center, Direction3D face, Vector2 faceCenter )
        {
            this.SubdivisionLevel = subdivisionLevel;
            this.SphereCenter = center;
            this.Face = face;
            this.FaceCenter = faceCenter;
        }

        public bool ShouldSubdivide( IEnumerable<Vector3Dbl> localPois )
        {
            foreach( var localPoi in localPois )
            {
                float distance = (float)(localPoi - SphereCenter).magnitude;

                if( distance < Size )
                {
                    return true;
                }
            }

            return false;
        }

        public bool ShouldUnsubdivide( IEnumerable<Vector3Dbl> localPois )
        {
            foreach( var localPoi in localPois )
            {
                float distance = (float)(localPoi - SphereCenter).magnitude;

                // Inverse of ShouldSubdivide.
                if( distance >= this.Size )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Subdivides the node, creating 4 new children.
        /// </summary>
        /// <remarks>
        /// IMPORTANT: It doesn't set the <see cref="Children"/> field. It only creates the instances.
        /// </remarks>
        /// <param name="node">The node to subdivide.</param>
        /// <returns>A 4-tuple with the newly instantiated children.</returns>
        public static (LODQuadTreeNode xnyn, LODQuadTreeNode xpyn, LODQuadTreeNode xnyp, LODQuadTreeNode xpyp) CreateChildren( LODQuadTreeNode node )
        {
            int subdivisionLevel = node.SubdivisionLevel + 1;

            Vector2 faceCenter = GetChildFaceCenter( node, 0, 0 );
            Vector3Dbl center = node.Face.GetSpherePoint( faceCenter.x, faceCenter.y );
            LODQuadTreeNode xnyn = new LODQuadTreeNode( subdivisionLevel, center, node.Face, faceCenter );
            xnyn.Parent = node;

            faceCenter = GetChildFaceCenter( node, 1, 0 );
            center = node.Face.GetSpherePoint( faceCenter.x, faceCenter.y );
            LODQuadTreeNode xpyn = new LODQuadTreeNode( subdivisionLevel, center, node.Face, faceCenter );
            xpyn.Parent = node;

            faceCenter = GetChildFaceCenter( node, 0, 1 );
            center = node.Face.GetSpherePoint( faceCenter.x, faceCenter.y );
            LODQuadTreeNode xnyp = new LODQuadTreeNode( subdivisionLevel, center, node.Face, faceCenter );
            xnyp.Parent = node;

            faceCenter = GetChildFaceCenter( node, 1, 1 );
            center = node.Face.GetSpherePoint( faceCenter.x, faceCenter.y );
            LODQuadTreeNode xpyp = new LODQuadTreeNode( subdivisionLevel, center, node.Face, faceCenter );
            xpyp.Parent = node;

            return (xnyn, xpyn, xnyp, xpyp);
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