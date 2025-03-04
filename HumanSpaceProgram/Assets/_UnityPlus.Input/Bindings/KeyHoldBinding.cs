using System;
using System.Linq;
using UnityEngine;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Binds to a specific key being currently held down, disregarding previous state.
    /// </summary>
    public sealed class KeyHoldBinding : IInputBinding
	{
		public bool IsValid { get; private set; }

		public float Value { get; }

		public KeyCode Key { get; set; }

		public KeyHoldBinding( float value, KeyCode key )
		{
			this.Key = key;
			this.Value = value;
		}

		public void Update( InputState currentState )
		{
			this.IsValid = currentState.CurrentHeldKeys.Contains( Key );
		}
	}
}