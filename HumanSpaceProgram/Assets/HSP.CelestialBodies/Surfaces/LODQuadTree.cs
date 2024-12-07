using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    /// <summary>
    /// Represents the surface of a subdivided chunked sphere
    /// </summary>
    public class LODQuadTree
    {
        LODQuadTreeNode[] _roots = null;

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
        public IEnumerable<LODQuadTreeNode> RootNodes => _roots;

        public bool IsInitialized => _roots != null;

        internal LODQuadTree( int maxDepth )
        {
            this.MaxDepth = maxDepth;
        }

        private LODQuadTree( int maxDepth, LODQuadTreeNode[] roots )
        {
            this.MaxDepth = maxDepth;
            this._roots = roots;
        }

        public static LODQuadTree FromPois( int maxDepth, IEnumerable<Vector3Dbl> localPois )
        {
            LODQuadTreeNode[] newRoots;

            Queue<LODQuadTreeNode> nodesToProcess;

            LODQuadTreeNode xn = new LODQuadTreeNode( 0, new Vector3Dbl( -1, 0, 0 ), Direction3D.Xn, Vector2.zero );
            LODQuadTreeNode xp = new LODQuadTreeNode( 0, new Vector3Dbl( 1, 0, 0 ), Direction3D.Xp, Vector2.zero );
            LODQuadTreeNode yn = new LODQuadTreeNode( 0, new Vector3Dbl( 0, -1, 0 ), Direction3D.Yn, Vector2.zero );
            LODQuadTreeNode yp = new LODQuadTreeNode( 0, new Vector3Dbl( 0, 1, 0 ), Direction3D.Yp, Vector2.zero );
            LODQuadTreeNode zn = new LODQuadTreeNode( 0, new Vector3Dbl( 0, 0, -1 ), Direction3D.Zn, Vector2.zero );
            LODQuadTreeNode zp = new LODQuadTreeNode( 0, new Vector3Dbl( 0, 0, 1 ), Direction3D.Zp, Vector2.zero );

            xn.Xn = yn;
            xn.Xp = yp;
            xn.Yn = zn;
            xn.Yp = zp;

            xp.Xn = yp;
            xp.Xp = yn;
            xp.Yn = zn;
            xp.Yp = zp;

            yn.Xn = xp;
            yn.Xp = xn;
            yn.Yn = zn;
            yn.Yp = zp;

            yp.Xn = xn;
            yp.Xp = xp;
            yp.Yn = zn;
            yp.Yp = zp;

            zn.Xn = yn;
            zn.Xp = yp;
            zn.Yn = xp;
            zn.Yp = xn;

            zp.Xn = yn;
            zp.Xp = yp;
            zp.Yn = xn;
            zp.Yp = xp;

            newRoots = new LODQuadTreeNode[6];
            newRoots[0] = xn;
            newRoots[1] = xp;
            newRoots[2] = yn;
            newRoots[3] = yp;
            newRoots[4] = zn;
            newRoots[5] = zp;

            nodesToProcess = new Queue<LODQuadTreeNode>( newRoots );

            LODQuadTreeNode currentNode = nodesToProcess.Dequeue();

            int currentSubdivisionLevel = 0;
            List<LODQuadTreeNode> currentLevelParents = new();

            int totalNodeCount = 0;

            do
            {
                // Resolves neighbors after all nodes of a given level have been created (guarantees that the neighbors are created, since the neighbors can be same level or lower).
                if( currentNode.SubdivisionLevel > currentSubdivisionLevel )
                {
                    currentSubdivisionLevel++;
                    AssignNeighborsBFS( currentLevelParents );
                    currentLevelParents.Clear();
                }

                totalNodeCount++;

                // Subdivide
                if( currentNode.SubdivisionLevel < maxDepth && currentNode.ShouldSubdivide( localPois ) )
                {
                    var newChildren = LODQuadTreeNode.CreateChildren( currentNode );
                    currentLevelParents.Add( currentNode );
                    currentNode.Children = newChildren;

                    nodesToProcess.Enqueue( newChildren.xnyn );
                    nodesToProcess.Enqueue( newChildren.xpyn );
                    nodesToProcess.Enqueue( newChildren.xnyp );
                    nodesToProcess.Enqueue( newChildren.xpyp );
                }

            } while( nodesToProcess.TryDequeue( out currentNode ) );

            AssignNeighborsBFS( currentLevelParents );

            //Debug.Log( "node count: " + totalNodeCount );
            return new LODQuadTree( maxDepth, newRoots );
        }


        static void AssignNeighborsBFS( List<LODQuadTreeNode> currentLevelParents )
        {
            // resolve neighbors for all nodes of a given subdivision level.

            foreach( var parent in currentLevelParents )
            {
                (LODQuadTreeNode subXnYn, LODQuadTreeNode subXpYn, LODQuadTreeNode subXnYp, LODQuadTreeNode subXpYp) = parent.Children.Value;
                LODQuadTreeNode parentsXn = parent.Xn;
                LODQuadTreeNode parentsXp = parent.Xp;
                LODQuadTreeNode parentsYn = parent.Yn;
                LODQuadTreeNode parentsYp = parent.Yp;

                // Only need to go to the immediate child, since the nodes of the previous size will be already resolved.
                subXnYn.Xn = GetNeighborToUse( parentsXn, subXnYn.SphereCenter );
                subXnYn.Yn = GetNeighborToUse( parentsYn, subXnYn.SphereCenter );
                subXnYn.Xp = subXpYn;
                subXnYn.Yp = subXnYp;

                subXpYn.Xp = GetNeighborToUse( parentsXp, subXpYn.SphereCenter );
                subXpYn.Yn = GetNeighborToUse( parentsYn, subXpYn.SphereCenter );
                subXpYn.Xn = subXnYn;
                subXpYn.Yp = subXpYp;

                subXnYp.Xn = GetNeighborToUse( parentsXn, subXnYp.SphereCenter );
                subXnYp.Yp = GetNeighborToUse( parentsYp, subXnYp.SphereCenter );
                subXnYp.Xp = subXpYp;
                subXnYp.Yn = subXnYn;

                subXpYp.Xp = GetNeighborToUse( parentsXp, subXpYp.SphereCenter );
                subXpYp.Yp = GetNeighborToUse( parentsYp, subXpYp.SphereCenter );
                subXpYp.Xn = subXnYp;
                subXpYp.Yn = subXpYn;
            }
        }

        public static LODQuadTreeChanges GetDifferences( LODQuadTree old, LODQuadTree @new )
        {
            LODQuadTreeChanges changes = new LODQuadTreeChanges();

            Stack<(LODQuadTreeNode o, LODQuadTreeNode n)> nodesToProcess = new();
            int rootCount = old._roots?.Length ?? @new._roots.Length;
            for( int i = 0; i < rootCount; i++ )
            {
                nodesToProcess.Push( (old._roots?[i], @new._roots?[i]) );
            }

            (LODQuadTreeNode o, LODQuadTreeNode n) currentNodePair = nodesToProcess.Pop();

            do
            {
                var (o, n) = currentNodePair;

                if( o == null && n != null )
                {
                    changes.newNodes ??= new();
                    changes.newNodes.Add( n );
                }
                else if( o != null && n == null )
                {
                    changes.removedNodes ??= new();
                    changes.removedNodes.Add( o );
                }
                else if( o != null && n != null )
                {
                    if( o.Xn.SubdivisionLevel != n.Xn.SubdivisionLevel || o.Xp.SubdivisionLevel != n.Xp.SubdivisionLevel || o.Yn.SubdivisionLevel != n.Yn.SubdivisionLevel || o.Yp.SubdivisionLevel != n.Yp.SubdivisionLevel )
                    {
                        changes.differentNeighbors ??= new();
                        changes.differentNeighbors.Add( n );
                    }
                    if( o.IsLeaf && !n.IsLeaf )
                    {
                        changes.becameNonLeaf ??= new();
                        changes.becameNonLeaf.Add( o );
                    }
                    else if( !o.IsLeaf && n.IsLeaf )
                    {
                        changes.becameLeaf ??= new();
                        changes.becameLeaf.Add( o );
                    }
                }

                if( o == null || o.IsLeaf )
                {
                    if( n != null && !n.IsLeaf )
                    {
                        var (xnyn, xpyn, xnyp, xpyp) = n.Children.Value;
                        nodesToProcess.Push( (null, xnyn) );
                        nodesToProcess.Push( (null, xpyn) );
                        nodesToProcess.Push( (null, xnyp) );
                        nodesToProcess.Push( (null, xpyp) );
                    }
                }
                else
                {
                    if( n == null || n.IsLeaf )
                    {
                        var (xnyn, xpyn, xnyp, xpyp) = o.Children.Value;
                        nodesToProcess.Push( (xnyn, null) );
                        nodesToProcess.Push( (xpyn, null) );
                        nodesToProcess.Push( (xnyp, null) );
                        nodesToProcess.Push( (xpyp, null) );
                    }
                    else
                    {
                        var (xnyn, xpyn, xnyp, xpyp) = o.Children.Value;
                        var (xnyn2, xpyn2, xnyp2, xpyp2) = n.Children.Value;
                        nodesToProcess.Push( (xnyn, xnyn2) );
                        nodesToProcess.Push( (xpyn, xpyn2) );
                        nodesToProcess.Push( (xnyp, xnyp2) );
                        nodesToProcess.Push( (xpyp, xpyp2) );
                    }
                }

            } while( nodesToProcess.TryPop( out currentNodePair ) );

            return changes;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static LODQuadTreeNode GetNeighborToUse( LODQuadTreeNode replacementNeighbor, Vector3Dbl selfSpherePos )
        {
            // parent is a node in the new tree here.

            if( replacementNeighbor.IsLeaf ) // isleaf is correct, because if the node was subdivided, it is already subdivided.
            {
                return replacementNeighbor;
            }

            (LODQuadTreeNode xnyn, LODQuadTreeNode xpyn, LODQuadTreeNode xnyp, LODQuadTreeNode xpyp) = replacementNeighbor.Children.Value;

            LODQuadTreeNode closestNode = null;
            double closestDistance = double.MaxValue;

            double dist = Vector3Dbl.Distance( selfSpherePos, xnyn.SphereCenter );
            if( dist < closestDistance )
            {
                closestDistance = dist;
                closestNode = xnyn;
            }

            dist = Vector3Dbl.Distance( selfSpherePos, xpyn.SphereCenter );
            if( dist < closestDistance )
            {
                closestDistance = dist;
                closestNode = xpyn;
            }

            dist = Vector3Dbl.Distance( selfSpherePos, xnyp.SphereCenter );
            if( dist < closestDistance )
            {
                closestDistance = dist;
                closestNode = xnyp;
            }

            dist = Vector3Dbl.Distance( selfSpherePos, xpyp.SphereCenter );
            if( dist < closestDistance )
            {
                closestDistance = dist;
                closestNode = xpyp;
            }

            return closestNode;
        }
    }
}