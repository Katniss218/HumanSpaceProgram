using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus
{
    public static class ITopologicallySortable_Ex
    {
        /// <summary>
        /// Sorts the specified values in topological order. <br/>
        /// This sort is stable.
        /// </summary>
        public static List<ITopologicallySortable<TId>> SortDependencies<TId>( this IEnumerable<ITopologicallySortable<TId>> values )
        {
            return values.SortDependencies(
                node => node.ID,
                node => node.Before,
                node => node.After,
                out _ );
        }

        /// <summary>
        /// Sorts the specified values in topological order. <br/>
        /// Returns an enumerable that can enumerate over any circular dependencies found. <br/>
        /// This sort is stable.
        /// </summary>
        public static List<ITopologicallySortable<TId>> SortDependencies<TId>( this IEnumerable<ITopologicallySortable<TId>> values, out IEnumerable<ITopologicallySortable<TId>> circularDependencies )
        {
            return values.SortDependencies(
                node => node.ID,
                node => node.Before,
                node => node.After,
                out circularDependencies );
        }

        /// <summary>
        /// Sorts the specified values in topological order. <br/>
        /// This sort is stable.
        /// </summary>
        /// <param name="idGetter">The function that will return the ID for each element. Must not return null.</param>
        /// <param name="afterIdsGetter">The function that will return the IDs of elements that should come AFTER the CURRENT element. Must not return null.</param>
        /// <param name="beforeIdsGetter">The function that will return the IDs of elements that should come BEFORE the CURRENT element. Must not return null.</param>
        public static List<TNode> SortDependencies<TNode, TId>(
            this IEnumerable<TNode> values,
            Func<TNode, TId> idGetter,
            Func<TNode, IEnumerable<TId>> beforeIdsGetter,
            Func<TNode, IEnumerable<TId>> afterIdsGetter )
        {
            return SortDependencies( values, idGetter, beforeIdsGetter, afterIdsGetter, out _ );
        }

        /// <summary>
        /// Sorts the specified values in topological order. <br/>
        /// Returns an enumerable that can enumerate over any circular dependencies found. <br/>
        /// This sort is stable.
        /// </summary>
        /// <param name="idGetter">The function that will return the ID for each element. Must not return null.</param>
        /// <param name="afterIdsGetter">The function that will return the IDs of elements that should come AFTER the CURRENT element. Must not return null.</param>
        /// <param name="beforeIdsGetter">The function that will return the IDs of elements that should come BEFORE the CURRENT element. Must not return null.</param>
        public static List<TNode> SortDependencies<TNode, TId>(
            this IEnumerable<TNode> values,
            Func<TNode, TId> idGetter,
            Func<TNode, IEnumerable<TId>> beforeIdsGetter,
            Func<TNode, IEnumerable<TId>> afterIdsGetter,
            out IEnumerable<TNode> circularDependencies )
        {
            if( values == null )
                throw new ArgumentNullException( nameof( values ) );
            if( idGetter == null )
                throw new ArgumentNullException( nameof( idGetter ) );
            if( beforeIdsGetter == null )
                throw new ArgumentNullException( nameof( beforeIdsGetter ) );
            if( afterIdsGetter == null )
                throw new ArgumentNullException( nameof( afterIdsGetter ) );

            Dictionary<TId, TNode> availableNodes = new Dictionary<TId, TNode>();
            foreach( var node in values )
            {
                TId id = idGetter( node );
                if( id == null )
                    throw new ArgumentException( "idGetter returned null for a node; null ids are not supported.", nameof( idGetter ) );
                if( !availableNodes.TryAdd( id, node ) )
                    throw new ArgumentException( $"Duplicate node id detected: {id}. Each node must have a unique id.", nameof( idGetter ) );
            }

            Dictionary<TId, HashSet<TId>> graph = new( availableNodes.Count );
            Dictionary<TId, int> indegree = new( availableNodes.Count );

            foreach( var id in availableNodes.Keys )
            {
                graph[id] = new HashSet<TId>();
                indegree[id] = 0;
            }

            // Initialize the graph and in-degrees.
            foreach( var (id, node) in availableNodes )
            {
                // Nodes that go before the current node.
                //   The naming is swapped here, because the nodes follow a more user-friendly convention than the graph (graph is reversed).
                foreach( TId beforeId in afterIdsGetter( node ) ?? Enumerable.Empty<TId>() )
                {
                    if( !graph.ContainsKey( beforeId ) ) // missing nodes.
                        graph[beforeId] = new HashSet<TId>();
                    graph[beforeId].Add( id );

                    indegree[id] = indegree.TryGetValue( id, out var cur ) ? cur + 1 : 1;
                }

                // Nodes that go after the current node.
                //   The naming is swapped here, because the nodes follow a more user-friendly convention than the graph (graph is reversed).
                foreach( TId afterId in beforeIdsGetter( node ) ?? Enumerable.Empty<TId>() )
                {
                    graph[id].Add( afterId );

                    if( !indegree.ContainsKey( afterId ) ) // missing nodes.
                        indegree[afterId] = 0;

                    indegree[afterId] = indegree.TryGetValue( afterId, out var cur ) ? cur + 1 : 1;
                }
            }

            // Kahn's algorithm
            Queue<TId> zeroIndegreeQueue = new Queue<TId>( indegree.Where( kvp => kvp.Value == 0 ).Select( kvp => kvp.Key ) );
            List<TNode> sortedNodes = new();

            while( zeroIndegreeQueue.Count > 0 )
            {
                TId id = zeroIndegreeQueue.Dequeue();

                if( !availableNodes.TryGetValue( id, out var node ) )
                    continue;

                sortedNodes.Add( node );

                if( !graph.TryGetValue( id, out var neighbors ) )
                    continue;

                foreach( var neighbor in neighbors )
                {
                    if( !indegree.TryGetValue( neighbor, out var cur ) )
                        continue;

                    cur--;
                    indegree[neighbor] = cur;

                    if( cur == 0 )
                    {
                        zeroIndegreeQueue.Enqueue( neighbor );
                    }
                }
            }

            circularDependencies = indegree
                .Where( kvp => kvp.Value > 0 && availableNodes.ContainsKey( kvp.Key ) )
                .Select( kvp => availableNodes[kvp.Key] );

            return sortedNodes;
        }
    }
}