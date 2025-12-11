using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public class FlowNetworkBuilder
    {
        private readonly Dictionary<object, List<IResourceConsumer>> _consumers = new();
        private readonly Dictionary<object, List<IResourceProducer>> _producers = new();
        private readonly Dictionary<object, List<FlowPipe>> _pipes = new();

        internal List<IResourceConsumer> ConsumerRemovals { get; } = new();
        internal List<IResourceProducer> ProducerRemovals { get; } = new();
        internal List<FlowPipe> PipeRemovals { get; } = new();

        // quick reverse lookup:
        private readonly Dictionary<object, object> _inverseOwner = new();
        private readonly Dictionary<object, object> _owner = new();

        public IReadOnlyDictionary<object, object> Owner => _inverseOwner;

        // --- Data from GameObject discovery ---
        private GameObject _rootObject;
        private IBuildsFlowNetwork[] _components = Array.Empty<IBuildsFlowNetwork>();

        public static FlowNetworkBuilder CreateFromGameObject( GameObject obj )
        {
            // return the flow network for this object and its children.

            // iterate over all descendants and collect their pipes. collect the tanks that these pipes connect to.
            // - No point solving tanks that don't matter.
            // - Also don't solve pipes that are closed.

            // iterate over gameobjects that implement this.
            if( obj == null )
                return null;

            FlowNetworkBuilder builder = new FlowNetworkBuilder();
            builder._rootObject = obj;

            builder._components = obj.GetComponentsInChildren<IBuildsFlowNetwork>( true );

            List<IBuildsFlowNetwork> retryList = null;
            foreach( var comp in builder._components )
            {
                var result = comp.BuildFlowNetwork( builder );
                if( result == BuildFlowResult.Retry )
                {
                    retryList ??= new();
                    retryList.Add( comp );
                }
            }

            // retry until nothing changes or a deadlock is detected.
            while( retryList != null && retryList.Count > 0 )
            {
                bool progressMade = false;
                for( int i = retryList.Count - 1; i >= 0; i-- )
                {
                    var comp = retryList[i];
                    var result = comp.BuildFlowNetwork( builder );
                    if( result != BuildFlowResult.Retry )
                    {
                        retryList.RemoveAt( i );
                        progressMade = true;
                    }
                }

                if( !progressMade )
                {
                    // Deadlock detected
                    var componentNames = retryList.Select( c =>
                    {
                        if( c is MonoBehaviour mb )
                        {
                            return $"'{mb.gameObject.name}::{c.GetType().Name}'";
                        }
                        return $"'{c.GetType().Name}'";
                    } );
                    Debug.LogError( $"FlowNetworkBuilder: Deadlock detected. The following components are stuck in a retry loop: {string.Join( ", ", componentNames )}" );
                    break;
                }
            }
            return builder;
        }

        public FlowNetworkSnapshot BuildSnapshot()
        {
            return new FlowNetworkSnapshot( _rootObject, Owner, _components, Producers.ToList(), Consumers.ToList(), Pipes.ToList() );
        }

        public bool TryAddFlowObj( object owner, object flowObj )
        {
            if( owner == null || flowObj == null )
                throw new ArgumentNullException();

            bool added = false;
            if( flowObj is IResourceConsumer c )
            {
                if( !_consumers.TryGetValue( owner, out var list ) )
                {
                    list = new List<IResourceConsumer>();
                    _consumers[owner] = list;
                }
                list.Add( c );
                added = true;
            }
            if( flowObj is IResourceProducer p )
            {
                if( !_producers.TryGetValue( owner, out var list2 ) )
                {
                    list2 = new List<IResourceProducer>();
                    _producers[owner] = list2;
                }
                list2.Add( p );
                added = true;
            }
            if( flowObj is FlowPipe pipe )
            {
                if( !_pipes.TryGetValue( owner, out var list3 ) )
                {
                    list3 = new List<FlowPipe>();
                    _pipes[owner] = list3;
                }
                list3.Add( pipe );
                added = true;
            }

            _owner[flowObj] = owner;
            _inverseOwner[owner] = flowObj;
            return added;
        }

        public bool TryRemoveFlowObj( object flowObj )
        {
            if( flowObj == null )
                return false;

            bool removed = false;
            if( flowObj is IResourceConsumer c )
            {
                ConsumerRemovals.Add( c );
                removed = true;
            }
            if( flowObj is IResourceProducer p )
            {
                ProducerRemovals.Add( p );
                removed = true;
            }
            if( flowObj is FlowPipe pipe )
            {
                PipeRemovals.Add( pipe );
                removed = true;
            }

            if( _owner.TryGetValue( flowObj, out var owner ) )
            {
                _owner.Remove( flowObj );
                _inverseOwner.Remove( owner );
            }
            return removed;
        }

        public bool TryGetFlowObj<T>( object obj, out T flowObj )
        {
            if( _inverseOwner.TryGetValue( obj, out var rawOwner ) && rawOwner is T typedOwner )
            {
                flowObj = typedOwner;
                return true;
            }
            flowObj = default;
            return false;
        }

        public IEnumerable<IResourceConsumer> Consumers => _consumers.Values.SelectMany( n => n );
        public IEnumerable<IResourceProducer> Producers => _producers.Values.SelectMany( n => n );
        public IEnumerable<FlowPipe> Pipes => _pipes.Values.SelectMany( n => n );
    }
}