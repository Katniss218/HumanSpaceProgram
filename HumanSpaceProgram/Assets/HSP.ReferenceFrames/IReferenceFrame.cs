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
        /// Transforms a point in the frame's local space to the Absolute (AIRF) space.
        /// </summary>
        Vector3Dbl TransformPosition( Vector3Dbl localPosition );
        /// <summary>
        /// Transforms a point in the Absolute (AIRF) space to the frame's space.
        /// </summary>
        Vector3Dbl InverseTransformPosition( Vector3Dbl absolutePosition );


        /// <summary>
        /// Transforms a direction in the frame's local space to the Absolute (AIRF) space.
        /// </summary>
        Vector3 TransformDirection( Vector3 localDirection );
        /// <summary>
        /// Transforms a direction in the Absolute (AIRF) space to the frame's space.
        /// </summary>
        Vector3 InverseTransformDirection( Vector3 absoluteDirection );


        /// <summary>
        /// Transforms rotation/orientation in the frame's local space to the Absolute (AIRF) space.
        /// </summary>
        QuaternionDbl TransformRotation( QuaternionDbl localRotation );
        /// <summary>
        /// Transforms rotation/orientation in the Absolute (AIRF) space to the frame's space.
        /// </summary>
        QuaternionDbl InverseTransformRotation( QuaternionDbl absoluteRotation );


        /// <summary>
        /// Transforms velocity in the frame's local space to the Absolute (AIRF) space.
        /// </summary>
        Vector3Dbl TransformVelocity( Vector3Dbl localVelocity );
        /// <summary>
        /// Transforms velocity in the Absolute (AIRF) space to the frame's space.
        /// </summary>
        Vector3Dbl InverseTransformVelocity( Vector3Dbl absoluteVelocity );


        /// <summary>
        /// Transforms angular velocity in the frame's local space to the Absolute (AIRF) space.
        /// </summary>
        Vector3Dbl TransformAngularVelocity( Vector3Dbl localAngularVelocity );
        /// <summary>
        /// Transforms angular velocity in the Absolute (AIRF) space to the frame's space.
        /// </summary>
        Vector3Dbl InverseTransformAngularVelocity( Vector3Dbl absoluteAngularVelocity );


        /// <summary>
        /// Transforms acceleration in the frame's local space to the Absolute (AIRF) space.
        /// </summary>
        Vector3Dbl TransformAcceleration( Vector3Dbl localAcceleration );
        /// <summary>
        /// Transforms acceleration in the Absolute (AIRF) space to the frame's space.
        /// </summary>
        Vector3Dbl InverseTransformAcceleration( Vector3Dbl absoluteAcceleration );


        /// <summary>
        /// Transforms angular acceleration in the frame's local space to the Absolute (AIRF) space.
        /// </summary>
        Vector3Dbl TransformAngularAcceleration( Vector3Dbl localAngularAcceleration );
        /// <summary>
        /// Transforms angular acceleration in the Absolute (AIRF) space to the frame's space.
        /// </summary>
        Vector3Dbl InverseTransformAngularAcceleration( Vector3Dbl absoluteAngularAcceleration );

        /// <summary>
        /// Brings the other reference frame to this frame's UT, and then checks for equality. Useful for inertial and non-inertial (moving) reference frames.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool EqualsIgnoreUT( IReferenceFrame other );
    }
}