using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being currently held down, disregarding previous state.
    /// </summary>
    public class KeyHoldBinding : InputBinding
    {
        public KeyCode Key { get; }

        bool _isHeld = false;

        public KeyHoldBinding( KeyCode key )
        {
            this.Key = key;
        }

        public override void Update( InputState currentState )
        {
            _isHeld = currentState.CurrentHeldKeys.Contains( Key );
        }

        public override bool Check()
        {
            return _isHeld;
        }
    }
}