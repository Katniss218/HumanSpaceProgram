using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Input.Bindings
{
    /// <summary>
    /// Behaves like an axis, with multiple bindings with multipliers for each. Will always call its associated actions with the value of the axis.
    /// </summary>
    public class AxisBinding : IInputBinding
    {
        public bool IsValid
        {
            get => true; // Force axes to be invoked every frame.
        }

        public float Value
        {
            get => _bindingTuples.Select( b => (b.binding.IsValid ? b.binding.Value : 0) * b.valueMultiplier ).Sum();
        }

        private readonly (IInputBinding binding, float valueMultiplier)[] _bindingTuples;

        public IEnumerable<(IInputBinding binding, float valueMultiplier)> BindingTuples => _bindingTuples;

        public AxisBinding( IEnumerable<(IInputBinding binding, float valueMultiplier)> bindingTuples )
        {
            this._bindingTuples = bindingTuples.ToArray();
        }

        public AxisBinding( params (IInputBinding binding, float valueMultiplier)[] bindingTuples )
        {
            this._bindingTuples = bindingTuples;
        }

        public AxisBinding( IEnumerable<IInputBinding> bindings )
        {
            this._bindingTuples = bindings.Select( b => (b, 1f) ).ToArray();
        }

        public AxisBinding( params IInputBinding[] bindings )
        {
            this._bindingTuples = bindings.Select( b => (b, 1f) ).ToArray();
        }

        public void Update( InputState currentState )
        {
            foreach( (var binding, _) in _bindingTuples )
                binding.Update( currentState );
        }
    }
}