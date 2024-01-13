using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Input
{
    public interface IInputBinding
    {
        /// <summary>
        /// Stores the value from last <see cref="Update"/>. <br/>
        /// True if the input action associated with this binding should trigger. False otherwise.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Use this method to update any potential internal state with the input data from the parameter. <br/>
        /// Return value indicates whether the current state represents a successful condition or not.
        /// </summary>
        /// <remarks>
        /// Guaranteed to be called once every frame.
        /// </remarks>
        /// <param name="currentState">The input state in this frame.</param>
        void Update( InputState currentState );
    }



    // we could do compound bindings, just pass the update to the inner children.
    [Obsolete( "this is just for educational purposes, I don't want the users to have to mess with combining channels and stuff." )]
    internal class CompoundBinding : IInputBinding
    {
        public bool IsValid
        {
            get
            {
                bool allValid = true;

                foreach( var binding in _bindings )
                    if( !binding.IsValid )
                        allValid = false;

                return allValid;
            }
        }

        IInputBinding[] _bindings { get; }

        public CompoundBinding( IEnumerable<IInputBinding> bindings )
        {
            this._bindings = bindings.ToArray();
        }

        public void Update( InputState currentState )
        {
            foreach( var binding in _bindings )
                binding.Update( currentState );
        }
    }
}