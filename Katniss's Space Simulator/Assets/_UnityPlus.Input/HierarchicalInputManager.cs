using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input
{
    public class HierarchicalInputManager : SingletonMonoBehaviour<HierarchicalInputManager>
    {
        static readonly KeyCode[] KEYS = Enum.GetValues( typeof( KeyCode ) )
            .Cast<KeyCode>()
            .ToArray();

        static Dictionary<string, HierarchicalActionChannel> _channels = new Dictionary<string, HierarchicalActionChannel>();
        static Dictionary<string, InputBinding> _bindings = new Dictionary<string, InputBinding>();

        static (InputBinding, HierarchicalActionChannel)[] _channelCache;
        static HashSet<InputBinding> _alreadyUpdatedBindings = new HashSet<InputBinding>(); // prevents update from being invoked twice if the same instance is registered for two channel IDs.

        static bool _isChannelCacheStale = true;

        internal static List<KeyCode> _keys = new List<KeyCode>();

        public static void BindInput( string channelId, InputBinding binding )
        {
            _bindings[channelId] = binding;
            _isChannelCacheStale = true;
        }

        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public static void AddAction( string channelId, int priority, Func<bool> action )
        {
            if( !_channels.TryGetValue( channelId, out var channel ) )
            {
                channel = new HierarchicalActionChannel();
                _channels[channelId] = channel;
                _isChannelCacheStale = true;
            }

            channel.AddAction( priority, action );
            // no need to mark as stale here because referenced instance is the same.
        }

        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public static void AddActions( string channelId, IEnumerable<(int priority, Func<bool>)> f )
        {
            if( !_channels.TryGetValue( channelId, out var channel ) )
            {
                channel = new HierarchicalActionChannel();
                _channels[channelId] = channel;
                _isChannelCacheStale = true;
            }

            channel.AddActions( f );
            // no need to mark as stale here because referenced instance is the same.
        }

        public static void RemoveAction( string channelId, Func<bool> action )
        {
            if( _channels.TryGetValue( channelId, out var channel ) )
            {
                channel.RemoveAction( action );
            }
        }

        public static void RemoveActions( string channelId, IEnumerable<Func<bool>> action )
        {
            if( _channels.TryGetValue( channelId, out var channel ) )
            {
                channel.RemoveActions( action );
            }
        }

        static void FixChannelCacheStaleness()
        {
            List<(InputBinding, HierarchicalActionChannel)> newChannelCache = new List<(InputBinding, HierarchicalActionChannel)>( _channels.Count );

            foreach( var kvp in _bindings )
            {
                if( _channels.TryGetValue( kvp.Key, out var channel ) )
                {
                    // It's important to use the same instance in the cache and the channel map.
                    // Otherwise the cache would need to be recalculated every time an action is added to one of the *existing* channels.
                    newChannelCache.Add( (kvp.Value, channel) );
                }
            }
            _channelCache = newChannelCache.ToArray();
            _isChannelCacheStale = false;
        }

        void UpdateHeldKeys( IEnumerable<KeyCode> targetKeySet )
        {
            _keys.Clear();

            if( !UnityEngine.Input.anyKeyDown )
                return;

            foreach( var key in targetKeySet )
            {
                if( UnityEngine.Input.GetKey( key ) )
                {
                    _keys.Add( key );
                }
            }
        }

        void Update()
        {
            if( _isChannelCacheStale )
            {
                FixChannelCacheStaleness();
            }

            UpdateHeldKeys( KEYS );

            InputState keyState = new InputState( _keys, (Vector2)UnityEngine.Input.mousePosition, (Vector2)UnityEngine.Input.mouseScrollDelta );

            _alreadyUpdatedBindings.Clear();
            foreach( var (binding, channel) in _channelCache )
            {
                if( !_alreadyUpdatedBindings.Contains( binding ) )
                {
                    binding.Update( keyState );
                    _alreadyUpdatedBindings.Add( binding );
                }

                if( binding.Check() )
                {
                    channel.Invoke();
                }
            }
        }
    }
}