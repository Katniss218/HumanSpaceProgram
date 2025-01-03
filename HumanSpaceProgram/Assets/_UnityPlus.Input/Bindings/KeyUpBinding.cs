using System;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being released (after being pressed).
    /// </summary>
    public sealed class KeyUpBinding : IInputBinding
	{
		public bool IsValid { get; private set; }

		public float Value { get; }

		public KeyCode Key { get; set; }

		bool _previousFrameWasPressed = false;

		public KeyUpBinding( float value, KeyCode key )
		{
			this.Key = key;
			this.Value = value;
		}

		public void Update( InputState currentState )
		{
			bool currentFrameIsReleased = !currentState.CurrentHeldKeys.Contains( Key );

			this.IsValid = _previousFrameWasPressed && currentFrameIsReleased;

			_previousFrameWasPressed = !currentFrameIsReleased;
		}
	}
}