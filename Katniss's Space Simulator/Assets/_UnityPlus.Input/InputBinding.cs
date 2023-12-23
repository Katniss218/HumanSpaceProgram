using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Input
{
    public abstract class InputBinding
    {
        /// <summary>
        /// Use this method to update any potential internal state with the input data from the parameter. <br/>
        /// Return value indicates whether the current state represents a successful condition or not.
        /// </summary>
        /// <param name="currentState">The input state in this frame.</param>
        /// <returns>True if the input action associated with this binding should trigger. False otherwise.</returns>
        public abstract void Update( InputState currentState );

        public abstract bool Check();
    }



    // we could do compound bindings, just pass the update to the inner children.
    [Obsolete( "this is just for educational purposes, I don't want the users to have to mess with combining channels and stuff." )]
    internal class CompoundBinding : InputBinding
    {
        InputBinding[] _bindings { get; }

        public CompoundBinding( IEnumerable<InputBinding> bindings )
        {
            this._bindings = bindings.ToArray();
        }

        public override void Update( InputState currentState )
        {
            foreach( var binding in _bindings )
                binding.Update( currentState );
        }

        public override bool Check()
        {
            bool wasTrue = false;

            foreach( var binding in _bindings )
                if( binding.Check() )
                    wasTrue = true;

            return wasTrue;
        }
    }
}