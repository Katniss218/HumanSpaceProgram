using System.Collections.Generic;
using UnityEngine;

namespace HSP.CelestialBodies.Surfaces
{
    public class LODQuadTreeChanges
    {
        /// <summary>
        /// An array of 6 root elements. Only non-null if the tree was not initialized yet.
        /// </summary>
        public LODQuadTreeNode[] newRoots;

        /// <summary>
        /// List of 4-tuple nodes that were created.
        /// </summary>
        public List<(LODQuadTreeNode xnyn, LODQuadTreeNode xpyn, LODQuadTreeNode xnyp, LODQuadTreeNode xpyp)> subdivided;

        /// <summary>
        /// Nodes to make into leaves.
        /// </summary>
        public List<LODQuadTreeNode> unSubdivided;

        public bool AnythingChanged => newRoots != null || subdivided != null || unSubdivided != null;

        /// <summary>
        /// Computes a map of children to add to existing leaf nodes (subdivided), and existing leaf nodes to remove (unsubdivided), for a given collection of points of interest.
        /// </summary>
        /// <remarks>
        /// Can handle an uninitialized LOD sphere (without nay faces) just fine.
        /// </remarks>
        /// <param name="tree">The existing LOD sphere.</param>
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
            if( tree.Nodes == null )
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
                nodesToProcess = new Queue<LODQuadTreeNode>( tree.Nodes );
            }

#warning TODO
            return changes;

            LODQuadTreeNode currentNode = nodesToProcess.Dequeue();
            int lastProcessedDepth = currentNode.SubdivisionLevel;
            int lastLevelIndex = 0;

            while( currentNode != null )
            {
                if( currentNode.SubdivisionLevel > lastProcessedDepth )
                {
                    // resolve connectivity of subdivided (new) quads.
                    // It's important to do this *between* going deeper and not immediately, because the neighbor we need might not have been subdivided (created) yet.

                    for( int i = lastLevelIndex; i < changes.subdivided.Count; i++ )
                    {
                        // still do BFS and resolve connectivity between going to a smaller node.
                        // unsubdivided nodes will have their connectivity already resolved.
                        // I think it's still enough to check 2 neighbors?
                        // if subdivided, set connectivity to subdivided neighbors immediately.
                        // set non-null from parent.
                        // - when walking *up* from parent, we have to walk towards the child that borders the current child. Only have to walk a single child chain. The direction can change during the walk up.
                        // - - it may walk a different amount than 1, if a node borders a highly subdivided node at the start.
                        // - - we can just compare the distance to the 4 children, and pick the closest

                        // ---------------


                        // get node that was subdivided into these 4, get its neighbor in each direction,
                        // and follow down a single step to wchichever child is closest - unless that node is a leaf.
                        (LODQuadTreeNode subXnYn, LODQuadTreeNode subXpYn, LODQuadTreeNode subXnYp, LODQuadTreeNode subXpYp) = changes.subdivided[i];
                        LODQuadTreeNode parent = subXnYn.Parent;
                        LODQuadTreeNode parentXn = parent.Xn;
                        LODQuadTreeNode parentXp = parent.Xp;
                        LODQuadTreeNode parentYn = parent.Yn;
                        LODQuadTreeNode parentYp = parent.Yp;

                        // Only need to go to the immediate child, since the nodes of the previous size will be already resolved.
                        subXnYn.Xn = parentXn.IsLeaf ? parentXn : GetClosestChild( parentXn, subXnYn.SphereCenter );
                        subXnYn.Yn = parentYn.IsLeaf ? parentYn : GetClosestChild( parentYn, subXnYn.SphereCenter );

                        subXpYn.Xp = parentXp.IsLeaf ? parentXp : GetClosestChild( parentXp, subXpYn.SphereCenter );
                        subXpYn.Yn = parentYn.IsLeaf ? parentYn : GetClosestChild( parentYn, subXpYn.SphereCenter );

                        subXnYp.Xn = parentXn.IsLeaf ? parentXn : GetClosestChild( parentXn, subXnYp.SphereCenter );
                        subXnYp.Yp = parentYp.IsLeaf ? parentYp : GetClosestChild( parentYp, subXnYp.SphereCenter );

                        subXpYp.Xp = parentXp.IsLeaf ? parentXp : GetClosestChild( parentXp, subXpYp.SphereCenter );
                        subXpYp.Yp = parentYp.IsLeaf ? parentYp : GetClosestChild( parentYp, subXpYp.SphereCenter );
                    }
                }

                lastProcessedDepth = currentNode.SubdivisionLevel;

                if( currentNode.IsLeaf )
                {
                    if( currentNode.ShouldSubdivide( normalizedPois ) )
                    {
                        var newNodes = LODQuadTreeNode.CreateChildren( currentNode ); // non-recursive.
                        changes.subdivided.Add( newNodes );
                    }
                }
                else
                {
                    if( currentNode.ShouldUnsubdivide( normalizedPois ) )
                    {
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

                currentNode = nodesToProcess.Dequeue();
            }

            return changes;
        }

        public void ApplyChanges( LODQuadTree tree )
        {
            if( this.newRoots == null )
            {
                foreach( var unsubdivided in this.unSubdivided )
                {
                    unsubdivided.Children = null;
                }

                foreach( var subdivided4Tuple in this.subdivided )
                {
                    subdivided4Tuple.xnyn.Parent.Children = subdivided4Tuple;
                }
            }
            else
            {
                tree.SetNodes( this.newRoots );
            }
        }

        public static LODQuadTreeNode GetClosestChild( LODQuadTreeNode node, Vector3Dbl spherePos )
        {
            (LODQuadTreeNode xnyn, LODQuadTreeNode xpyn, LODQuadTreeNode xnyp, LODQuadTreeNode xpyp) = node.Children.Value;
            LODQuadTreeNode closestNode = null;
            double closestDistance = double.MaxValue;

            double dist = Vector3Dbl.Distance( spherePos, xnyn.SphereCenter );
            if( dist < closestDistance )
            {
                closestDistance = dist;
                closestNode = xnyn;
            }

            dist = Vector3Dbl.Distance( spherePos, xpyn.SphereCenter );
            if( dist < closestDistance )
            {
                closestDistance = dist;
                closestNode = xpyn;
            }

            dist = Vector3Dbl.Distance( spherePos, xnyp.SphereCenter );
            if( dist < closestDistance )
            {
                closestDistance = dist;
                closestNode = xnyp;
            }

            dist = Vector3Dbl.Distance( spherePos, xpyp.SphereCenter );
            if( dist < closestDistance )
            {
                closestDistance = dist;
                closestNode = xpyp;
            }

            return closestNode;
        }
    }
}