using System;
using System.Collections.Generic;
using System.Linq;

namespace HSP.Vessels
{
    public class DirectedVesselAttachmentGraph : IReadonlyVesselAttachmentGraph
    {
        private readonly Dictionary<VesselPart, List<AttachmentEdge>> _adjacencyList;
        public VesselPart Root { get; }

        public IReadOnlyCollection<VesselPart> Nodes => _adjacencyList.Keys;

        private DirectedVesselAttachmentGraph( Dictionary<VesselPart, List<AttachmentEdge>> adjacencyList, VesselPart root )
        {
            _adjacencyList = adjacencyList;
            Root = root;
        }

        public bool HasNode( VesselPart part )
        {
            return part != null && _adjacencyList.ContainsKey( part );
        }

        public IReadOnlyList<AttachmentEdge> GetEdges( VesselPart part )
        {
            if( part != null && _adjacencyList.TryGetValue( part, out var list ) )
            {
                return list;
            }
            return Array.Empty<AttachmentEdge>();
        }

        public bool HasEdge( VesselPart nodeA, VesselPart nodeB )
        {
            if( nodeA == null || nodeB == null ) return false;
            if( _adjacencyList.TryGetValue( nodeA, out var list ) )
            {
                return list.Any( e => e.Target == nodeB );
            }
            return false;
        }

        /// <summary>
        /// Creates a directed graph from an undirected graph by performing a breadth-first search starting at the given root part.
        /// </summary>
        public static DirectedVesselAttachmentGraph Create( IReadonlyVesselAttachmentGraph undirectedGraph, VesselPart rootPart )
        {
            if( undirectedGraph == null ) throw new ArgumentNullException( nameof( undirectedGraph ) );
            if( rootPart == null ) throw new ArgumentNullException( nameof( rootPart ) );
            if( !undirectedGraph.HasNode( rootPart ) ) throw new ArgumentException( "The provided root part is not in the graph." );

            var directedAdjacency = new Dictionary<VesselPart, List<AttachmentEdge>>();
            foreach( var node in undirectedGraph.Nodes )
            {
                directedAdjacency[node] = new List<AttachmentEdge>();
            }

            var visited = new HashSet<VesselPart>();
            var queue = new Queue<VesselPart>();

            queue.Enqueue( rootPart );
            visited.Add( rootPart );

            while( queue.Count > 0 )
            {
                var current = queue.Dequeue();

                foreach( var edge in undirectedGraph.GetEdges( current ) )
                {
                    if( !visited.Contains( edge.Target ) )
                    {
                        visited.Add( edge.Target );
                        queue.Enqueue( edge.Target );
                        directedAdjacency[current].Add( new AttachmentEdge { Target = edge.Target } );
                    }
                }
            }

            return new DirectedVesselAttachmentGraph( directedAdjacency, rootPart );
        }

        /// <summary>
        /// Gets all parts reachable from the given part in this directed graph.
        /// This is useful to find all "children" of a part.
        /// </summary>
        public HashSet<VesselPart> GetSubgraph( VesselPart startPart )
        {
            var subgraph = new HashSet<VesselPart>();
            if( !HasNode( startPart ) ) return subgraph;

            var queue = new Queue<VesselPart>();
            queue.Enqueue( startPart );
            subgraph.Add( startPart );

            while( queue.Count > 0 )
            {
                var current = queue.Dequeue();
                foreach( var edge in GetEdges( current ) )
                {
                    if( subgraph.Add( edge.Target ) )
                    {
                        queue.Enqueue( edge.Target );
                    }
                }
            }

            return subgraph;
        }
    }
}
