using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.OverridableEvents;

namespace UnityPlus.OverridableEvents
{
    public static class OverridableEvent_Ex
    {
        public static IEnumerable<OverridableEventListener<T>> GetNonBlacklistedListeners<T>( this IEnumerable<OverridableEventListener<T>> listeners )
        {
            var blacklistedListeners = listeners
                .Where( l => l.Blacklist != null )
                .SelectMany( l => l.Blacklist )
                .ToHashSet();

            return listeners
                .Where( l => !blacklistedListeners.Contains( l.ID ) );
        }

        public static List<OverridableEventListener<T>> SortDependencies<T>( this IEnumerable<OverridableEventListener<T>> listeners )
        {
            return SortDependencies<T>( listeners, out _ );
        }

        public static List<OverridableEventListener<T>> SortDependencies<T>( this IEnumerable<OverridableEventListener<T>> listeners, out bool hasCircularDependency )
        {
            Dictionary<string, List<string>> graph = new();
            Dictionary<string, int> indegree = new();

            // Initialize the graph and in-degrees.
            foreach( var listener in listeners )
            {
                if( !graph.ContainsKey( listener.ID ) )
                    graph[listener.ID] = new List<string>();

                if( !indegree.ContainsKey( listener.ID ) )
                    indegree[listener.ID] = 0;

                // Nodes that go before the current node.
                //   Naming is confusing here, because the nodes follow a more user-friendly convention than the graph.
                foreach( var beforeId in listener.After )
                {
                    if( !graph.ContainsKey( beforeId ) )
                        graph[beforeId] = new List<string>();
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

            var listenerDict = listeners.ToDictionary( listener => listener.ID );

            // Kahn's algorithm.
            Queue<string> zeroIndegreeQueue = new Queue<string>( indegree.Where( kvp => kvp.Value == 0 ).Select( kvp => kvp.Key ) );
            List<OverridableEventListener<T>> sortedListeners = new();

            while( zeroIndegreeQueue.Count > 0 )
            {
                string id = zeroIndegreeQueue.Dequeue();
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
