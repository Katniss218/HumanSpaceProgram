using System;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being pressed (after being not pressed).
    /// </summary>
    public sealed class KeyDownBinding : IInputBinding
	{
		public bool IsValid { get; private set; }

		public float Value { get; }

		public KeyCode Key { get; set; }

		bool _previousFrameWasReleased = true;

		public KeyDownBinding( float value, KeyCode key )
		{
			this.Key = key;
			this.Value = value;
		}

		public void Update( InputState currentState )
		{
			bool currentFrameIsPressed = currentState.CurrentHeldKeys.Contains( Key );

			this.IsValid = _previousFrameWasReleased && currentFrameIsPressed;

			_previousFrameWasReleased = !currentFrameIsPressed;
		}
	}
}