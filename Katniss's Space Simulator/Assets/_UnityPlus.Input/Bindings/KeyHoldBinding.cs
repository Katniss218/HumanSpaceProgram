using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being currently held down, disregarding previous state.
    /// </summary>
    public sealed class KeyHoldBinding : IInputBinding
    {
        public KeyCode Key { get; set; }

        public bool IsValid { get; private set; }

        public KeyHoldBinding( KeyCode key )
        {
            this.Key = key;
        }

        public void Update( InputState currentState )
        {
            this.IsValid = currentState.CurrentHeldKeys.Contains( Key );
        }
    }
}