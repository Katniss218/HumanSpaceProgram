using System.Collections.Generic;

namespace HSP.CelestialBodies.Surfaces
{
    /// <summary>
    /// Represents changes between two <see cref="LODQuadTree"/>s.
    /// </summary>
    public class LODQuadTreeChanges
    {
        internal List<LODQuadTreeNode> newNodes;
        internal List<LODQuadTreeNode> removedNodes;

        internal List<LODQuadTreeNode> differentNeighbors;
        internal List<LODQuadTreeNode> becameLeaf;
        internal List<LODQuadTreeNode> becameNonLeaf;

        /// <summary>
        /// Checks if the two trees are equivalent or if they have changed.
        /// </summary>
        public bool AnythingChanged => newNodes != null || removedNodes != null;
        
        /// <summary>
        /// Gets the collection of nodes that exist in the new tree, but don't exist in the old tree.
        /// </summary>
        public IReadOnlyList<LODQuadTreeNode> GetNewNodes() // nodes missing in old tree
        {
            return (IReadOnlyList<LODQuadTreeNode>)newNodes ?? new LODQuadTreeNode[] { };
        }

        /// <summary>
        /// Gets the collection of nodes that exist in the old tree, but don't exist in the new tree.
        /// </summary>
        public IReadOnlyList<LODQuadTreeNode> GetRemovedNodes() // nodes missing in new tree
        {
            return (IReadOnlyList<LODQuadTreeNode>)removedNodes ?? new LODQuadTreeNode[] { };
        }

        /// <summary>
        /// Gets the collection of nodes that have different neighbors between the old and new trees.
        /// </summary>
        public IReadOnlyList<LODQuadTreeNode> GetDifferentNeighbors()
        {
            return (IReadOnlyList<LODQuadTreeNode>)differentNeighbors ?? new LODQuadTreeNode[] { };
        }

        /// <summary>
        /// Gets the collection of nodes that are leaves in the new tree, but aren't in the old tree.
        /// </summary>
        public IReadOnlyList<LODQuadTreeNode> GetBecameLeaf()
        {
            return (IReadOnlyList<LODQuadTreeNode>)becameLeaf ?? new LODQuadTreeNode[] { };
        }

        /// <summary>
        /// Gets the collection of nodes that are leaves in the old tree, but aren't in the new tree.
        /// </summary>
        public IReadOnlyList<LODQuadTreeNode> GetBecameNonLeaf()
        {
            return (IReadOnlyList<LODQuadTreeNode>)becameNonLeaf ?? new LODQuadTreeNode[] { };
        }
    }
}