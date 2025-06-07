using System.Collections.Generic;
using System.Linq;

namespace UnityPlus
{
    public static class ITopologicallySortable_Ex
    {
        /// <summary>
        /// Sorts the specified values in topological order.
        /// </summary>
        public static List<ITopologicallySortable<T>> SortDependencies<T>( this IEnumerable<ITopologicallySortable<T>> values )
        {
            return SortDependencies<T>( values, out _ );
        }

        /// <summary>
        /// Sorts the specified values in topological order. <br/>
        /// Returns a value indicating whether any circular dependencies exist in the graph.
        /// </summary>
        public static List<ITopologicallySortable<T>> SortDependencies<T>( this IEnumerable<ITopologicallySortable<T>> values, out bool hasCircularDependency )
        {
            Dictionary<T, List<T>> graph = new();
            Dictionary<T, int> indegree = new();

            // Initialize the graph and in-degrees.
            foreach( var listener in values )
            {
                if( !graph.ContainsKey( listener.ID ) )
                    graph[listener.ID] = new List<T>();

                if( !indegree.ContainsKey( listener.ID ) )
                    indegree[listener.ID] = 0;

                // Nodes that go before the current node.
                //   Naming is confusing here, because the nodes follow a more user-friendly convention than the graph.
                foreach( var beforeId in listener.After )
                {
                    if( !graph.ContainsKey( beforeId ) )
                        graph[beforeId] = new List<T>();
                    graph[beforeId].Add( listener.ID );

                    indegree[listener.ID]++;
                }

                // Nodes that go after the current node.
                //   Naming is confusing here, because the nodes follow a more user-friendly convention than the graph.
                foreach( var afterId in listener.Before )
                {
                    graph[listener.ID].Add( afterId );

                    if( !indegree.ContainsKey( afterId ) )
                        indegree[afterId] = 0;
                    indegree[afterId]++;
                }
            }

            var listenerDict = values.ToDictionary( listener => listener.ID );

            // Kahn's algorithm.
            Queue<T> zeroIndegreeQueue = new Queue<T>( indegree.Where( kvp => kvp.Value == 0 ).Select( kvp => kvp.Key ) );
            List<ITopologicallySortable<T>> sortedListeners = new();

            while( zeroIndegreeQueue.Count > 0 )
            {
                T id = zeroIndegreeQueue.Dequeue();
                sortedListeners.Add( listenerDict[id] );

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

            if( hasCircularDependency )
            {
                // Optionally, handle circular dependencies
                // For example, log the issue or throw an exception
            }

            return sortedListeners;
        }

    }
}
