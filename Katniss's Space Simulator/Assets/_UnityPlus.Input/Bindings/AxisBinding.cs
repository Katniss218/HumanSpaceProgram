using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace UnityPlus.Input.Bindings
{
	/// <summary>
	/// Behaves like an axis, with multiple bindings with multipliers for each. Will always call its associated actions with the value of the axis.
	/// </summary>
	public class AxisBinding : IInputBinding
	{
		public bool IsValid
		{
			get => true;
		}

		public float Value
		{
			get => _bindingTuples.Select( b => b.binding.Value * b.valueMultiplier ).Sum();
		}

		private readonly (IInputBinding binding, float valueMultiplier)[] _bindingTuples;

		public AxisBinding( IEnumerable<(IInputBinding binding, float valueMultiplier)> bindingTuples )
		{
			this._bindingTuples = bindingTuples.ToArray();
		}

		public AxisBinding( params (IInputBinding binding, float valueMultiplier)[] bindingTuples )
		{
			this._bindingTuples = bindingTuples;
		}

		public void Update( InputState currentState )
		{
			foreach( (var binding, _) in _bindingTuples )
				binding.Update( currentState );
		}
	}
}