
using UnityEngine;

namespace HSP.ReferenceFrames
{
    public interface INonInertialReferenceFrame : IReferenceFrame
    {
        /// <summary>
        /// Gets the net fictitious linear acceleration (not force) acting on an object due to the reference frame.
        /// </summary>
        Vector3Dbl GetFicticiousAcceleration( Vector3Dbl localPosition, Vector3Dbl localVelocity );

        /// <summary>
        /// Gets the net fictitious angular acceleration (not torque) acting on an object due to the reference frame.
        /// </summary>
        Vector3Dbl GetFictitiousAngularAcceleration( Vector3Dbl localPosition, Vector3Dbl localAngularVelocity );
    }
}
