using System.Collections.Generic;

namespace HSP.Vessels
{
    public class VesselAttachmentGraph : IReadonlyVesselAttachmentGraph
    {
        // Adjacency list for undirected graph
        private Dictionary<VesselPart, List<AttachmentEdge>> _adjacencyList = new();

        public IReadOnlyCollection<VesselPart> Nodes => _adjacencyList.Keys;

        public bool HasNode( VesselPart part )
        {
            return _adjacencyList.ContainsKey( part );
        }

        public IReadOnlyList<AttachmentEdge> GetEdges( VesselPart part )
        {
            if( _adjacencyList.TryGetValue( part, out var list ) )
            {
                return list;
            }

            return System.Array.Empty<AttachmentEdge>();
        }

        public void AddNode( VesselPart part )
        {
            if( !_adjacencyList.ContainsKey( part ) )
            {
                _adjacencyList[part] = new List<AttachmentEdge>();
            }
        }

        public void AddEdge( VesselPart a, VesselPart b, bool isRigid )
        {
            AddNode( a );
            AddNode( b );

            _adjacencyList[a].Add( new AttachmentEdge() { Target = b, IsRigid = isRigid } );
            _adjacencyList[b].Add( new AttachmentEdge() { Target = a, IsRigid = isRigid } );
        }

        public List<VesselIsland> DetectIslands()
        {
            List<VesselIsland> islands = new();
            HashSet<VesselPart> visited = new();

            foreach( var node in _adjacencyList.Keys )
            {
                if( !visited.Contains( node ) )
                {
                    var island = new VesselIsland();
                    ExploreIsland( node, visited, island );
                    islands.Add( island );
                }
            }

            return islands;
        }

        private void ExploreIsland( VesselPart current, HashSet<VesselPart> visited, VesselIsland island )
        {
            visited.Add( current );
            island.AddPart( current );

            foreach( var edge in _adjacencyList[current] )
            {
                if( edge.IsRigid && !visited.Contains( edge.Target ) )
                {
                    ExploreIsland( edge.Target, visited, island );
                }
            }
        }
    }
}