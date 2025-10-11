using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// Represents an arbitrary non-inertial (i.e. accelerating) reference frame.
    /// </summary>
    public interface INonInertialReferenceFrame : IReferenceFrame
    {
        /// <summary>
        /// Gets the velocity (in absolute space) at a point due to the rotation of the frame.
        /// </summary>
        /// <param name="localPosition">The position of the point in the reference frame's local space.</param>
        Vector3Dbl GetTangentialVelocity( Vector3Dbl localPosition );

        /// <summary>
        /// Gets the net fictitious linear acceleration (NOT force) (in the frame's space) acting on an object due to the reference frame.
        /// </summary>
        /// <param name="localPosition">The position of the object in the reference frame's local space.</param>
        /// <param name="localVelocity">The velocity of the object in the reference frame's local space.</param>
        Vector3Dbl GetFicticiousAcceleration( Vector3Dbl localPosition, Vector3Dbl localVelocity );

        /// <summary>
        /// Gets the net fictitious angular acceleration (NOT torque) (in the frame's space) acting on an object due to the reference frame.
        /// </summary>
        /// <param name="localPosition">The position of the object in the reference frame's local space.</param>
        /// <param name="localAngularVelocity">The angular velocity of the object in the reference frame's local space.</param>
        Vector3Dbl GetFictitiousAngularAcceleration( Vector3Dbl localPosition, Vector3Dbl localAngularVelocity );
    }
}