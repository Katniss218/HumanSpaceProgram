using System;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    public delegate void KinematicStateMutator( ref KinematicState state );

    public interface IReferenceFrameTransform : IComponent, IReferenceFrameSwitchResponder
    {
        /// <summary>
        /// Gets or sets the scene reference frame provider, defines which reference frame the *scene space* properties will use.
        /// </summary>
        ISceneReferenceFrameProvider SceneReferenceFrameProvider { get; set; }

        /// <summary>
        /// Gets the current state measured in the requested reference frame.
        /// </summary>
        KinematicState GetState( IReferenceFrame requestedFrame );

        /// <summary>
        /// Returns direct read-only access to the internal state (measured in the returned reference frame).
        /// This avoids a struct copy (160 bytes) and is preferred for performance-critical logic.
        /// </summary>
        ref readonly KinematicState GetStateRef( out IReferenceFrame referenceFrame );

        /// <summary>
        /// Sets the current state to the specified value.
        /// </summary>
        void SetState( in KinematicState state );

        /// <summary>
        /// Modifies the current state in-place using a mutator function, expressed in the requested reference frame.
        /// </summary>
        /// <param name="mutator">A function that takes a reference to the current state and modifies it.</param>
        /// <param name="referenceFrame">The reference frame in which the mutator function will express the state.</param>
        void ModifyState( IReferenceFrame referenceFrame, KinematicStateMutator mutator );

        /// <summary>
        /// Invoked when the state is set or modified.
        /// </summary>
        event Action OnStateChanged;
    }
}