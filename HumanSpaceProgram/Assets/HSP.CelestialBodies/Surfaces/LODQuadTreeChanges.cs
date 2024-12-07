using System.Collections.Generic;

namespace HSP.CelestialBodies.Surfaces
{
    public class LODQuadTreeChanges
    {
        public List<LODQuadTreeNode> newNodes;
        public List<LODQuadTreeNode> removedNodes;

        //public List<LODQuadTreeNode> unSubdivided;

        public List<LODQuadTreeNode> differentNeighbors;
        public List<LODQuadTreeNode> becameLeaf;
        public List<LODQuadTreeNode> becameNonLeaf;

        public bool AnythingChanged => newNodes != null || removedNodes != null;
        
        public IEnumerable<LODQuadTreeNode> GetNewNodes() // nodes missing in old tree
        {
            return (IEnumerable<LODQuadTreeNode>)newNodes ?? new LODQuadTreeNode[] { };
        }

        public IEnumerable<LODQuadTreeNode> GetRemovedNodes() // nodes missing in new tree
        {
            return (IEnumerable<LODQuadTreeNode>)removedNodes ?? new LODQuadTreeNode[] { };
        }

        public IEnumerable<LODQuadTreeNode> GetDifferentNeighbors()
        {
            return (IEnumerable<LODQuadTreeNode>)differentNeighbors ?? new LODQuadTreeNode[] { };
        }

        public IEnumerable<LODQuadTreeNode> GetBecameLeaf()
        {
            return (IEnumerable<LODQuadTreeNode>)becameLeaf ?? new LODQuadTreeNode[] { };
        }

        public IEnumerable<LODQuadTreeNode> GetBecameNonLeaf()
        {
            return (IEnumerable<LODQuadTreeNode>)becameNonLeaf ?? new LODQuadTreeNode[] { };
        }
    }
}