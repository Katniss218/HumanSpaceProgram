using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus
{
    public static class ITopologicallySortable_Ex
    {
        /// <summary>
        /// Sorts the specified values in topological order.
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
        /// Returns a value indicating whether any circular dependencies exist in the graph.
        /// </summary>
        public static List<ITopologicallySortable<TId>> SortDependencies<TId>( this IEnumerable<ITopologicallySortable<TId>> values, out bool hasCircularDependency )
        {
            return values.SortDependencies(
                node => node.ID,
                node => node.Before,
                node => node.After,
                out hasCircularDependency );
        }

        public static List<TNode> SortDependencies<TNode, TId>(
            this IEnumerable<TNode> values,
            Func<TNode, TId> idGetter,
            Func<TNode, IEnumerable<TId>> beforeIdsGetter,
            Func<TNode, IEnumerable<TId>> afterIdsGetter )
        {
            return SortDependencies( values, idGetter, beforeIdsGetter, afterIdsGetter, out _ );
        }

        public static List<TNode> SortDependencies<TNode, TId>(
            this IEnumerable<TNode> values,
            Func<TNode, TId> idGetter,
            Func<TNode, IEnumerable<TId>> beforeIdsGetter,
            Func<TNode, IEnumerable<TId>> afterIdsGetter,
            out bool hasCircularDependency )
        {
            if( values == null )
                throw new ArgumentNullException( nameof( values ) );
            if( idGetter == null )
                throw new ArgumentNullException( nameof( idGetter ) );
            if( beforeIdsGetter == null )
                throw new ArgumentNullException( nameof( beforeIdsGetter ) );
            if( afterIdsGetter == null )
                throw new ArgumentNullException( nameof( afterIdsGetter ) );

            Dictionary<TId, List<TId>> graph = new();
            Dictionary<TId, int> indegree = new();

            // Initialize the graph and in-degrees.
            foreach( var listener in values )
            {
                var listenerId = idGetter( listener );

                if( !graph.ContainsKey( listenerId ) )
                    graph[listenerId] = new List<TId>();

                if( !indegree.ContainsKey( listenerId ) )
                    indegree[listenerId] = 0;

                // Nodes that go before the current node.
                //   Naming is confusing here, because the nodes follow a more user-friendly convention than the graph.
                foreach( var beforeId in afterIdsGetter( listener ) ?? Enumerable.Empty<TId>() )
                {
                    if( !graph.ContainsKey( beforeId ) )
                        graph[beforeId] = new List<TId>();
                    graph[beforeId].Add( listenerId );

                    indegree[listenerId]++;
                }

                // Nodes that go after the current node.
                //   Naming is confusing here, because the nodes follow a more user-friendly convention than the graph.
                foreach( var afterId in beforeIdsGetter( listener ) ?? Enumerable.Empty<TId>() )
                {
                    graph[listenerId].Add( afterId );

                    if( !indegree.ContainsKey( afterId ) )
                        indegree[afterId] = 0;
                    indegree[afterId]++;
                }
            }

            var availableNodes = values.ToDictionary( idGetter );

            // Kahn's algorithm
            Queue<TId> zeroIndegreeQueue = new Queue<TId>( indegree.Where( kvp => kvp.Value == 0 ).Select( kvp => kvp.Key ) );
            List<TNode> sortedNodes = new();

            while( zeroIndegreeQueue.Count > 0 )
            {
                TId id = zeroIndegreeQueue.Dequeue();

                // The current node points to a node (id) that doesn't exist - skip the node.
                if( !availableNodes.TryGetValue( id, out var listener ) )
                    continue;

                sortedNodes.Add( listener );

                foreach( var neighbor in graph[id] )
                {
                    indegree[neighbor]--;

                    if( indegree[neighbor] == 0 )
                    {
                        zeroIndegreeQueue.Enqueue( neighbor );
                    }
                }
            }

            hasCircularDependency = indegree.Any( kvp => kvp.Value > 0 );
            return sortedNodes;
        }
    }
}