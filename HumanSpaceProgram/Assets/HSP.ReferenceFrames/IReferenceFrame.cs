using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// An arbitrary reference frame.
    /// </summary>
    public interface IReferenceFrame
    {
        // "Absolute Inertial Reference Frame" (AIRF) is my invention.
        // It represents the absolute world space.
        // We can't use Unity's world space for that because of 32-bit float precision issues. So instead, we make the worldspace act like the current world space reference frame.




        // Bottom line is that we need to make the Unity's world space act like the local space of the selected reference frame.

        // frames of reference can be used for that.

        // a rotating frame of reference will impart forces on the object just because it is rotating.
        // the reference frame needs to keep high precision position / rotation of the reference object.
        // - Every object's position will be transformed by this frame to get its "true" position, which might have arbitrary precision (and in reverse too, inverse to get local position).

        // There is only one global reference frame for the scene.

        // objects that are not centered on the reference frame need to be updated every frame (possibly less if they're very distant and can't be seen) to remain correct.
        // - if the frame is centered on the active vessel, then "world" space in Unity needs to be transformed into local space for that frame.
        // - - This can be done by applying forces/changing positions manually.

        double ReferenceUT { get; }

        /// <summary>
        /// Returns a new reference frame that is shifted (translated) by a given distance in the Absolute Inertial Reference Frame (AIRF) space.
        /// </summary>
        IReferenceFrame Shift( Vector3Dbl absolutePositionDelta );

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
    }
}