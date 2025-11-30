using System;
using System.Collections.Generic;
using System.Linq;

namespace HSP.ResourceFlow
{
    public class FlowNetworkBuilder
    {
        private readonly Dictionary<IBuildsFlowNetwork, List<IResourceConsumer>> _consumers = new();
        private readonly Dictionary<IBuildsFlowNetwork, List<IResourceProducer>> _producers = new();
        private readonly Dictionary<IBuildsFlowNetwork, List<FlowPipe>> _pipes = new();

        internal List<IResourceConsumer> ConsumerRemovals { get; } = new();
        internal List<IResourceProducer> ProducerRemovals { get; } = new();
        internal List<FlowPipe> PipeRemovals { get; } = new();

        // quick reverse lookup:
        private readonly Dictionary<object, object> _inverseOwner = new();
        private readonly Dictionary<object, object> _owner = new();

        public IReadOnlyDictionary<object, object> Owner => _inverseOwner;

        public bool TryAddFlowObj( object owner, object flowObj )
        {
            if( owner == null || flowObj == null )
                throw new ArgumentNullException();

            bool added = false;
            if( owner is IBuildsFlowNetwork component )
            {
                if( flowObj is IResourceConsumer c )
                {
                    if( !_consumers.TryGetValue( component, out var list ) )
                    {
                        list = new List<IResourceConsumer>();
                        _consumers[component] = list;
                    }
                    list.Add( c );
                    added = true;
                }
                if( flowObj is IResourceProducer p )
                {
                    if( !_producers.TryGetValue( component, out var list2 ) )
                    {
                        list2 = new List<IResourceProducer>();
                        _producers[component] = list2;
                    }
                    list2.Add( p );
                    added = true;
                }
                if( flowObj is FlowPipe pipe )
                {
                    if( !_pipes.TryGetValue( component, out var list3 ) )
                    {
                        list3 = new List<FlowPipe>();
                        _pipes[component] = list3;
                    }
                    list3.Add( pipe );
                    added = true;
                }
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

        internal IEnumerable<IResourceConsumer> Consumers => _consumers.Values.SelectMany( n => n );
        internal IEnumerable<IResourceProducer> Producers => _producers.Values.SelectMany( n => n );
        internal IEnumerable<FlowPipe> Pipes => _pipes.Values.SelectMany( n => n );
    }
}