using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input
{
    public class HierarchicalInputManager : SingletonMonoBehaviour<HierarchicalInputManager>
    {
        private class ChannelData
        {
            public HierarchicalActionChannel channel;
            public bool enabled;
        }

        static readonly KeyCode[] KEYS = Enum.GetValues( typeof( KeyCode ) )
            .Cast<KeyCode>()
            .ToArray();

        static Dictionary<string, ChannelData> _channels = new Dictionary<string, ChannelData>();
        static Dictionary<string, IInputBinding> _bindings = new Dictionary<string, IInputBinding>();

        static (IInputBinding, HierarchicalActionChannel)[] _channelCache;
        static HashSet<IInputBinding> _alreadyUpdatedBindings = new HashSet<IInputBinding>(); // prevents update from being invoked twice if the same instance is registered for two channel IDs.

        static bool _isChannelCacheStale = true;

        /// <summary>
        /// Registers a binding to the given channel ID.
        /// </summary>
        public static void BindInput( string channelId, IInputBinding binding )
        {
            _bindings[channelId] = binding;
            _isChannelCacheStale = true;
        }

        /// <summary>
        /// Enables the channel with the given channel ID.
        /// </summary>
        public static void EnableChannel( string channelId )
        {
            if( _channels.TryGetValue( channelId, out var channel ) )
            {
                channel.enabled = true;
                _isChannelCacheStale = true;
            }
        }

        /// <summary>
        /// Disables the channel with the given channel ID.
        /// </summary>
        public static void DisableChannel( string channelId )
        {
            if( _channels.TryGetValue( channelId, out var channel ) )
            {
                channel.enabled = false;
                _isChannelCacheStale = true;
            }
        }

        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public static void AddAction( string channelId, int priority, Func<bool> action )
        {
            if( !_channels.TryGetValue( channelId, out var channel ) )
            {
                channel = new ChannelData() { channel = new HierarchicalActionChannel(), enabled = true };
                _channels[channelId] = channel;
                _isChannelCacheStale = true;
            }

            channel.channel.AddAction( priority, action );
            // no need to mark as stale here because referenced instance is the same.
        }

        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public static void AddActions( string channelId, IEnumerable<(int priority, Func<bool>)> f )
        {
            if( !_channels.TryGetValue( channelId, out var channel ) )
            {
                channel = new ChannelData() { channel = new HierarchicalActionChannel(), enabled = true };
                _channels[channelId] = channel;
                _isChannelCacheStale = true;
            }

            channel.channel.AddActions( f );
            // no need to mark as stale here because referenced instance is the same.
        }

        public static void RemoveAction( string channelId, Func<bool> action )
        {
            if( _channels.TryGetValue( channelId, out var channel ) )
            {
                channel.channel.RemoveAction( action );
            }
        }

        public static void RemoveActions( string channelId, IEnumerable<Func<bool>> action )
        {
            if( _channels.TryGetValue( channelId, out var channel ) )
            {
                channel.channel.RemoveActions( action );
            }
        }

        static void FixChannelCacheStaleness()
        {
            List<(IInputBinding, HierarchicalActionChannel)> newChannelCache = new List<(IInputBinding, HierarchicalActionChannel)>( _channels.Count );

            foreach( var kvp in _bindings )
            {
                if( _channels.TryGetValue( kvp.Key, out var channel ) )
                {
                    if( !channel.enabled )
                        continue;

                    // It's important to use the same instance in the cache and the channel map.
                    // Otherwise the cache would need to be recalculated every time an action is added to one of the *existing* channels.
                    newChannelCache.Add( (kvp.Value, channel.channel) );
                }
            }

            _channelCache = newChannelCache.ToArray();
            _isChannelCacheStale = false;
        }

        IEnumerable<KeyCode> GetHeldKeys( IEnumerable<KeyCode> targetKeySet )
        {
            if( UnityEngine.Input.anyKey )
                return targetKeySet.Where( k => UnityEngine.Input.GetKey( k ) );

            return new KeyCode[] { };
        }

        void Update()
        {
            if( _isChannelCacheStale )
            {
                FixChannelCacheStaleness();
            }

            InputState keyState = new InputState( GetHeldKeys( KEYS ), UnityEngine.Input.mousePosition, UnityEngine.Input.mouseScrollDelta );

            _alreadyUpdatedBindings.Clear();
            foreach( var (binding, channel) in _channelCache )
            {
                if( !_alreadyUpdatedBindings.Contains( binding ) )
                {
                    binding.Update( keyState );
                    _alreadyUpdatedBindings.Add( binding );
                }

                if( binding.IsValid )
                {
                    channel.Invoke();
                }
            }
        }
    }
}