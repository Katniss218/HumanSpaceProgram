using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being pressed (after being not pressed).
    /// </summary>
    public sealed class KeyDownBinding : IInputBinding
    {
#warning TODO - add "alternate key binding" (down/hold/up) with a number of different keys.

        public KeyCode Key { get; set; }

        public bool IsValid { get; private set; }

        bool _previousFrameWasReleased = true;

        public KeyDownBinding( KeyCode key )
        {
            this.Key = key;
        }

        public void Update( InputState currentState )
        {
            bool currentFrameIsPressed = currentState.CurrentHeldKeys.Contains( Key );

            this.IsValid = _previousFrameWasReleased && currentFrameIsPressed;

            _previousFrameWasReleased = !currentFrameIsPressed;
        }
    }
}