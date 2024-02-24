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
        /// The value of this binding.
        /// </summary>
        float Value { get; }

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
}