using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public class LODQuadTreeChanges
    {
        /// <summary>
        /// An array of 6 root elements. Only non-null if the tree was not initialized yet.
        /// </summary>
        LODQuadTreeNode[] newRoots;

        Dictionary<LODQuadTreeNode, (LODQuadTreeNode xnyn, LODQuadTreeNode xpyn, LODQuadTreeNode xnyp, LODQuadTreeNode xpyp)> subdivided;

        /// <summary>
        /// Nodes to make into leaves.
        /// </summary>
        HashSet<LODQuadTreeNode> unSubdivided;

        public bool AnythingChanged => newRoots != null || subdivided != null || unSubdivided != null;

        public int AddedCount => ((newRoots?.Length) ?? 0) + ((subdivided?.Count * 4) ?? 0);

        private LODQuadTreeChanges()
        {
        }

        /// <summary>
        /// Gets the collection of new nodes that will be added by the changes.
        /// </summary>
        public IEnumerable<LODQuadTreeNode> GetAddedNodes()
        {
            if( newRoots != null )
            {
                foreach( var rootNode in newRoots )
                {
                    yield return rootNode;
                }
            }

            if( subdivided != null )
            {
                foreach( var (xnyn, xpyn, xnyp, xpyp) in subdivided.Values )
                {
                    yield return xnyn;
                    yield return xpyn;
                    yield return xnyp;
                    yield return xpyp;
                }
            }
        }

        /// <summary>
        /// Gets the collection of nodes that will become leaves due to the changes removing their children.
        /// </summary>
        public IEnumerable<LODQuadTreeNode> GetLeafNodesDueToRemoval()
        {
            return (IEnumerable<LODQuadTreeNode>)unSubdivided ?? new LODQuadTreeNode[] { };
        }

        /// <summary>
        /// Gets the collection of existing nodes that will be removed by the changes.
        /// </summary>
        public IEnumerable<LODQuadTreeNode> GetRemovedNodes()
        {
            if( unSubdivided != null )
            {
                Queue<LODQuadTreeNode> nodesToProcess = new Queue<LODQuadTreeNode>( unSubdivided.Count * 4 );
                foreach( var unsub in unSubdivided )
                {
                    var (xnyn, xpyn, xnyp, xpyp) = unsub.Children.Value;
                    nodesToProcess.Enqueue( xnyn );
                    nodesToProcess.Enqueue( xpyn );
                    nodesToProcess.Enqueue( xnyp );
                    nodesToProcess.Enqueue( xpyp );
                }

                LODQuadTreeNode node = nodesToProcess.Dequeue();
                while( true )
                {
                    if( !node.IsLeaf ) // if node has any previously existing children - add them.
                    {
                        var (xnyn, xpyn, xnyp, xpyp) = node.Children.Value;
                        nodesToProcess.Enqueue( xnyn );
                        nodesToProcess.Enqueue( xpyn );
                        nodesToProcess.Enqueue( xnyp );
                        nodesToProcess.Enqueue( xpyp );
                    }
                    if( !unSubdivided.Contains( node ) )
                        yield return node;

                    if( !nodesToProcess.TryDequeue( out node ) )
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Computes the changes to the specified tree to make it satisfy the specified points of interest.
        /// </summary>
        /// <remarks>
        /// This method is able to handle an uninitialized LODQuadTree (one without any faces).
        /// </remarks>
        /// <param name="normalizedPois">The collection of points of interest, normalized to LOD sphere radius = 1.</param>
        /// <returns>The computed changes to apply to the LOD sphere to make it to where the POIs are satisfied.</returns>
        public static LODQuadTreeChanges GetChanges( LODQuadTree tree, IEnumerable<Vector3Dbl> normalizedPois )
        {
            // so we walk the quadtree, if at any point in the traversal of a node:
            //   if the node is a leaf, and any PoI is within `size` of the node, AND in range of its would-be-children 
            //     - recursively subdivide, until the new would-be-children aren't in range.
            //   if the node is not a leaf, and any PoI is within `size` of the node, but not in range of any its children 
            //     - unsubdivide.

            LODQuadTreeChanges changes = new LODQuadTreeChanges();

            Queue<LODQuadTreeNode> nodesToProcess;
            if( tree.RootNodes == null )
            {
                LODQuadTreeNode[] roots = new LODQuadTreeNode[6];
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

                roots[0] = xn;
                roots[1] = xp;
                roots[2] = yn;
                roots[3] = yp;
                roots[4] = zn;
                roots[5] = zp;
                changes.newRoots = roots;

                nodesToProcess = new Queue<LODQuadTreeNode>( changes.newRoots );
            }
            else
            {
                nodesToProcess = new Queue<LODQuadTreeNode>( tree.RootNodes );
            }

            LODQuadTreeNode currentNode = nodesToProcess.Dequeue();

            int currentLevel = currentNode.SubdivisionLevel;
            List<LODQuadTreeNode> currentLevelNodes = new();

            while( true )
            {
                if( currentNode.SubdivisionLevel > currentLevel )
                {
                    currentLevel++;

                    // Resolve connectivity of the newly created nodes (one subdivision level at a time, i.e. breadth-first)
                    // This needs to be done here - after all nodes of the given level have been processed - because the neighbors will always be lower level or equal.
                    foreach( var subdividedNode in currentLevelNodes )
                    {
                        (LODQuadTreeNode subXnYn, LODQuadTreeNode subXpYn, LODQuadTreeNode subXnYp, LODQuadTreeNode subXpYp) = changes.subdivided[subdividedNode];
                        LODQuadTreeNode parentsXn = subdividedNode.Xn;
                        LODQuadTreeNode parentsXp = subdividedNode.Xp;
                        LODQuadTreeNode parentsYn = subdividedNode.Yn;
                        LODQuadTreeNode parentsYp = subdividedNode.Yp;

                        // Only need to go to the immediate child, since the nodes of the previous size will be already resolved.
                        subXnYn.Xn = changes.GetNeighborToUse( parentsXn, subXnYn.SphereCenter );
                        subXnYn.Yn = changes.GetNeighborToUse( parentsYn, subXnYn.SphereCenter );
                        subXnYn.Xp = subXpYn;
                        subXnYn.Yp = subXnYp;

                        subXpYn.Xp = changes.GetNeighborToUse( parentsXp, subXpYn.SphereCenter );
                        subXpYn.Yn = changes.GetNeighborToUse( parentsYn, subXpYn.SphereCenter );
                        subXpYn.Xn = subXnYn;
                        subXpYn.Yp = subXpYp;

                        subXnYp.Xn = changes.GetNeighborToUse( parentsXn, subXnYp.SphereCenter );
                        subXnYp.Yp = changes.GetNeighborToUse( parentsYp, subXnYp.SphereCenter );
                        subXnYp.Xp = subXpYp;
                        subXnYp.Yn = subXnYn;

                        subXpYp.Xp = changes.GetNeighborToUse( parentsXp, subXpYp.SphereCenter );
                        subXpYp.Yp = changes.GetNeighborToUse( parentsYp, subXpYp.SphereCenter );
                        subXpYp.Xn = subXnYp;
                        subXpYp.Yn = subXpYn;
                    }

                    currentLevelNodes.Clear();
                }

                // currentLevel = currentNode.SubdivisionLevel;

                if( currentNode.IsLeaf )
                {
                    if( currentNode.SubdivisionLevel < tree.MaxDepth && currentNode.ShouldSubdivide( normalizedPois ) )
                    {
                        if( changes.subdivided == null )
                        {
                            changes.subdivided = new();
                        }
                        var newNodes = LODQuadTreeNode.CreateChildren( currentNode );
                        changes.subdivided.Add( currentNode, newNodes );
                        currentLevelNodes.Add( currentNode );
                        nodesToProcess.Enqueue( newNodes.xnyn );
                        nodesToProcess.Enqueue( newNodes.xpyn );
                        nodesToProcess.Enqueue( newNodes.xnyp );
                        nodesToProcess.Enqueue( newNodes.xpyp );
                    }
                }
                else
                {
                    if( currentNode.ShouldUnsubdivide( normalizedPois ) )
                    {
                        if( changes.unSubdivided == null )
                        {
                            changes.unSubdivided = new();
                        }
                        changes.unSubdivided.Add( currentNode );
                    }
                    else
                    {
                        (LODQuadTreeNode xnyn, LODQuadTreeNode xpyn, LODQuadTreeNode xnyp, LODQuadTreeNode xpyp) = currentNode.Children.Value;
                        nodesToProcess.Enqueue( xnyn );
                        nodesToProcess.Enqueue( xpyn );
                        nodesToProcess.Enqueue( xnyp );
                        nodesToProcess.Enqueue( xpyp );
                    }
                }

                if( !nodesToProcess.TryDequeue( out currentNode ) )
                {
                    break;
                }
            }

            return changes;
        }

        public void ApplyTo( LODQuadTree tree )
        {
            if( this.newRoots != null )
            {
                tree.SetRootNodes( this.newRoots );
            }

            if( this.unSubdivided != null )
            {
                foreach( var unsubdivided in this.unSubdivided )
                {
                    unsubdivided.Children = null;
                }
            }

            if( this.subdivided != null )
            {
                foreach( var subdivided4Tuple in this.subdivided )
                {
                    subdivided4Tuple.Key.Children = subdivided4Tuple.Value;
                }
            }
#warning TODO - resolve the neighbors of the new nodes (new nodes might now be the neighbors of existing nodes, if eligible).
            // I might think I should just create a new tree for this... it would be a lot simpler. And probably fast enough... Since I'm doing so many allocations and stuff anyway
        }

        private LODQuadTreeNode GetNeighborToUse( LODQuadTreeNode parentsNeighbor, Vector3Dbl selfSpherePos )
        {
            // get the child (if exists), or self.

            if( parentsNeighbor.IsLeaf && this.subdivided != null && !this.subdivided.ContainsKey( parentsNeighbor ) ) // was a leaf, and still is a leaf
            {
                return parentsNeighbor;
            }
            else if( this.unSubdivided != null && this.unSubdivided.Contains( parentsNeighbor ) ) // was not a leaf, but is now a leaf.
            {
                return parentsNeighbor;
            }

            (LODQuadTreeNode xnyn, LODQuadTreeNode xpyn, LODQuadTreeNode xnyp, LODQuadTreeNode xpyp) = parentsNeighbor.IsLeaf
                ? this.subdivided[parentsNeighbor]
                : parentsNeighbor.Children.Value;

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