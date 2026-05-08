using System;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// Represents an arbitrary reference frame.
    /// </summary>
    public interface IReferenceFrame : IEquatable<IReferenceFrame>
    {
        // There's "Absolute" space, which is the new 64-bit "world space".
        // - We can't use Unity's world space for that because of 32-bit float precision issues.

        // Scene space is now whatever the scene reference frame says it is. It's not important.
        // - Every root object needs to have some implementation of IReferenceFrameTransform
        //   to behave correctly with this scene space.

        /// <summary>
        /// Returns the reference time for this reference frame.
        /// </summary>
        double ReferenceUT { get; }

        /// <summary>
        /// Calculates where the current reference frame will be at the specified reference time.
        /// </summary>
        IReferenceFrame AtUT( double ut );

        /// <summary>
        /// Transforms state UP: FROM this frame TO its immediate parent frame.
        /// </summary>
        KinematicState TransformState( in KinematicState localState );

        /// <summary>
        /// Transforms state DOWN: FROM the immediate parent frame TO this frame.
        /// </summary>
        KinematicState InverseTransformState( in KinematicState parentState );

        /// <summary>
        /// Brings the other reference frame to this frame's UT, and then checks for equality. Useful for inertial and non-inertial (moving) reference frames.
        /// </summary>
        bool EqualsIgnoreUT( IReferenceFrame other );
    }
}