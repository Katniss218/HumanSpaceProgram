using System;
using System.Collections.Generic;
using System.Linq;

namespace HSP.Vessels
{
    public class VesselAttachmentGraph : IReadonlyVesselAttachmentGraph
    {
        private readonly Dictionary<VesselPart, List<AttachmentEdge>> _adjacencyList;

        public IReadOnlyCollection<VesselPart> Nodes => _adjacencyList.Keys;

        private VesselAttachmentGraph( Dictionary<VesselPart, List<AttachmentEdge>> adjacencyList )
        {
            _adjacencyList = adjacencyList;
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
            if( nodeA == null || nodeB == null )
                return false;

            if( _adjacencyList.TryGetValue( nodeA, out var list ) )
            {
                return list.Any( e => e.Target == nodeB );
            }
            return false;
        }

        /// <summary>
        /// Creates a new graph from a single root part.
        /// </summary>
        public static VesselAttachmentGraph Create( VesselPart rootPart )
        {
            if( rootPart == null )
                throw new ArgumentNullException( nameof( rootPart ) );

            var adjacencyList = new Dictionary<VesselPart, List<AttachmentEdge>>();
            adjacencyList[rootPart] = new List<AttachmentEdge>();
            return new VesselAttachmentGraph( adjacencyList );
        }

        /// <summary>
        /// Creates a graph from an existing adjacency list, validating that it's a single connected component.
        /// </summary>
        public static VesselAttachmentGraph CreateValidated( Dictionary<VesselPart, List<AttachmentEdge>> adjacencyList )
        {
            if( adjacencyList == null || adjacencyList.Count == 0 )
                throw new ArgumentException( "Graph must have at least one node." );

            var visited = new HashSet<VesselPart>();
            var startNode = adjacencyList.Keys.First();

            var stack = new Stack<VesselPart>();
            stack.Push( startNode );

            while( stack.Count > 0 )
            {
                var current = stack.Pop();
                if( visited.Add( current ) )
                {
                    foreach( var edge in adjacencyList[current] )
                    {
                        stack.Push( edge.Target );
                    }
                }
            }

            if( visited.Count != adjacencyList.Count )
                throw new ArgumentException( "Graph must consist of exactly 1 connected component." );

            var copy = new Dictionary<VesselPart, List<AttachmentEdge>>();
            foreach( var kvp in adjacencyList )
            {
                copy[kvp.Key] = new List<AttachmentEdge>( kvp.Value );
            }

            return new VesselAttachmentGraph( copy );
        }

        /// <summary>
        /// Merges two graphs and adds a connecting edge.
        /// </summary>
        public static VesselAttachmentGraph Merge( IReadonlyVesselAttachmentGraph graphA, IReadonlyVesselAttachmentGraph graphB, VesselPart partA, VesselPart partB )
        {
            if( graphA == null )
                throw new ArgumentNullException( nameof( graphA ) );
            if( graphB == null )
                throw new ArgumentNullException( nameof( graphB ) );

            if( !graphA.HasNode( partA ) )
                throw new ArgumentException( "nodeA must be in graphA" );
            if( !graphB.HasNode( partB ) )
                throw new ArgumentException( "nodeB must be in graphB" );

            var mergedAdjacency = new Dictionary<VesselPart, List<AttachmentEdge>>();

            CopyEdges( graphA, mergedAdjacency );
            CopyEdges( graphB, mergedAdjacency );

            mergedAdjacency[partA].Add( new AttachmentEdge() { Target = partB } );
            mergedAdjacency[partB].Add( new AttachmentEdge() { Target = partA } );

            return new VesselAttachmentGraph( mergedAdjacency );
        }

        /// <summary>
        /// Adds an edge to the graph (which must already contain both nodes).
        /// </summary>
        public static VesselAttachmentGraph AddEdge( IReadonlyVesselAttachmentGraph graph, VesselPart partA, VesselPart partB )
        {
            if( graph == null ) throw new ArgumentNullException( nameof( graph ) );
            if( !graph.HasNode( partA ) || !graph.HasNode( partB ) )
                throw new ArgumentException( "Both nodes must already exist in the graph to add an internal edge." );

            var newAdjacency = new Dictionary<VesselPart, List<AttachmentEdge>>();
            CopyEdges( graph, newAdjacency );

            newAdjacency[partA].Add( new AttachmentEdge() { Target = partB } );
            newAdjacency[partB].Add( new AttachmentEdge() { Target = partA } );

            return new VesselAttachmentGraph( newAdjacency );
        }

        /// <summary>
        /// Removes an edge and returns the resulting graph(s).
        /// Returns 1 graph if the removed edge was part of a cycle.
        /// Returns 2 graphs if the removed edge was a bridge.
        /// </summary>
        /// <remarks>
        /// If the graph was split, the graph containing nodeA is returned first, and the graph containing nodeB is returned second.
        /// </remarks>
        public static (VesselAttachmentGraph graph, VesselAttachmentGraph extraGraph) RemoveEdge( IReadonlyVesselAttachmentGraph graph, VesselPart partA, VesselPart partB )
        {
            if( graph == null )
                throw new ArgumentNullException( nameof( graph ) );

            if( !graph.HasNode( partA ) )
                throw new ArgumentException( "NodeA not found in the graph." );
            if( !graph.HasNode( partB ) )
                throw new ArgumentException( "NodeB not found in the graph." );

            var newAdjacency = new Dictionary<VesselPart, List<AttachmentEdge>>();
            CopyEdges( graph, newAdjacency );

            newAdjacency[partA].RemoveAll( e => e.Target == partB );
            newAdjacency[partB].RemoveAll( e => e.Target == partA );

            var visitedA = new HashSet<VesselPart>();
            var stack = new Stack<VesselPart>();
            stack.Push( partA );

            bool connected = false;

            while( stack.Count > 0 )
            {
                var current = stack.Pop();
                if( current == partB )
                {
                    connected = true;
                }

                if( visitedA.Add( current ) )
                {
                    foreach( var edge in newAdjacency[current] )
                    {
                        stack.Push( edge.Target );
                    }
                }
            }

            if( connected )
            {
                return (new VesselAttachmentGraph( newAdjacency ), null);
            }
            else
            {
                var graph1Adjacency = new Dictionary<VesselPart, List<AttachmentEdge>>();
                var graph2Adjacency = new Dictionary<VesselPart, List<AttachmentEdge>>();

                foreach( var node in newAdjacency.Keys )
                {
                    if( visitedA.Contains( node ) )
                    {
                        graph1Adjacency[node] = newAdjacency[node];
                    }
                    else
                    {
                        graph2Adjacency[node] = newAdjacency[node];
                    }
                }

                return (new VesselAttachmentGraph( graph1Adjacency ), new VesselAttachmentGraph( graph2Adjacency ));
            }
        }

        public static List<VesselIsland> DetectIslands( IReadonlyVesselAttachmentGraph graph )
        {
            if( graph == null ) return new List<VesselIsland>();

            var islands = new List<VesselIsland>();
            var visited = new HashSet<VesselPart>();

            foreach( var node in graph.Nodes )
            {
                if( !visited.Contains( node ) )
                {
                    var island = new VesselIsland();
                    ExploreIsland( graph, node, visited, island );
                    islands.Add( island );
                }
            }

            return islands;
        }

        private static void ExploreIsland( IReadonlyVesselAttachmentGraph graph, VesselPart current, HashSet<VesselPart> visited, VesselIsland island )
        {
            visited.Add( current );
            island.AddPart( current );

            foreach( var edge in graph.GetEdges( current ) )
            {
                if( edge.Type == AttachmentEdgeType.Rigid && !visited.Contains( edge.Target ) )
                {
                    ExploreIsland( graph, edge.Target, visited, island );
                }
            }
        }

        private static void CopyEdges( IReadonlyVesselAttachmentGraph source, Dictionary<VesselPart, List<AttachmentEdge>> destination )
        {
            foreach( var node in source.Nodes )
            {
                destination[node] = new List<AttachmentEdge>( source.GetEdges( node ) );
            }
        }
    }
}