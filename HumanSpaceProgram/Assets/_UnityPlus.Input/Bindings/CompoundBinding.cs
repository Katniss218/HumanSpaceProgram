using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Input.Bindings
{
    internal class CompoundBinding : IInputBinding
	{
		public bool IsValid
		{
			get => _bindings.All( b => b.IsValid );
		}

		public float Value
		{
			get => _bindings.Select( b => b.Value ).Average();
		}

		private readonly IInputBinding[] _bindings;

		public CompoundBinding( IEnumerable<IInputBinding> bindings )
		{
			this._bindings = bindings.ToArray();
		}
		
		public CompoundBinding( params IInputBinding[] bindings )
		{
			this._bindings = bindings;
		}

		public void Update( InputState currentState )
		{
			foreach( var binding in _bindings )
				binding.Update( currentState );
		}
	}
}