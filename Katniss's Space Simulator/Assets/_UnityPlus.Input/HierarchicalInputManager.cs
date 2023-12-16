using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input
{
    public static class HierarchicalInputManager
    {
        static Dictionary<string, HierarchicalInputChannel> _channelSet = new Dictionary<string, HierarchicalInputChannel>();
        static Dictionary<string, Func<InputState, bool>> _channelMap = new Dictionary<string, Func<InputState, bool>>();

        static Dictionary<Func<InputState, bool>, HierarchicalInputChannel> _channelCache = new Dictionary<Func<InputState, bool>, HierarchicalInputChannel>();

        static bool _isMapStale = false;

        internal static List<KeyCode> _keys = new List<KeyCode>();

        static Vector2 _previousMousePos;

        public static void BindInput( Func<InputState, bool> input, string channelId )
        {
            _channelMap[channelId] = input;
            _isMapStale = true;
        }

        /// <remarks>
        /// The input actions are invoked according to their priorities (highest first) until an input action returns true.
        /// </remarks>
        public static void AddAction( string channelId, int priority, Func<bool> action )
        {
            if( !_channelSet.TryGetValue( channelId, out var channel ) )
            {
                channel = new HierarchicalInputChannel();
                _channelSet[channelId] = channel;
            }

            channel.AddAction( priority, action );
        }


        // channel set consists of all existing channels.
        // channel map has all channels that are currently mapped to some input.

        // 2 different types of events - continuous, and clicks
        // continuous don't care if multiple are pressed
        // clicks only fire individually.

        internal static void Update()
        {
            InputState keyState = new InputState()
            {
                OrderedKeyPresses = _keys.ToArray(),
                MousePosition = (Vector2)UnityEngine.Input.mousePosition,
                MouseDelta = (Vector2)UnityEngine.Input.mousePosition - _previousMousePos,
                ScrollDelta = (Vector2)UnityEngine.Input.mouseScrollDelta
            };

            _previousMousePos = UnityEngine.Input.mousePosition;

            foreach( var channelKvp in _channelCache )
            {
                if( channelKvp.Key.Invoke( keyState ) )
                {
                    channelKvp.Value.Invoke();
                }
            }
        }
    }
}