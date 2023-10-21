using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.ReferenceFrames
{
    /// <summary>
    /// A reference frame aligned with the AIRF frame, and shifted (offset) by a certain amount. This class is immutable.
    /// </summary>
    public sealed class CenteredReferenceFrame : IReferenceFrame
    {
        Vector3Dbl _center;

        /// <summary>
        /// Creates a new instance of the <see cref="CenteredReferenceFrame"/> with the specified offset.
        /// </summary>
        /// <param name="center">The AIRF coordinates that will be mapped to (0,0,0) in the new frame.</param>
        public CenteredReferenceFrame( Vector3Dbl center )
        {
            this._center = center;
        }

        public IReferenceFrame Shift( Vector3Dbl airfDistanceDelta )
        {
            return new CenteredReferenceFrame( this._center + airfDistanceDelta );
        }

        public Vector3Dbl InverseTransformPosition( Vector3Dbl airfPosition )
        {
            return Vector3Dbl.Subtract( airfPosition, _center );
        }

        public Vector3Dbl TransformPosition( Vector3Dbl localPosition )
        {
            return Vector3Dbl.Add( _center, localPosition );
        }

        public Vector3 InverseTransformDirection( Vector3 airfDirection )
        {
            return airfDirection;
        }

        public Vector3 TransformDirection( Vector3 localDirection )
        {
            return localDirection;
        }

        public QuaternionDbl InverseTransformRotation( QuaternionDbl airfRotation )
        {
            return airfRotation;
        }

        public QuaternionDbl TransformRotation( QuaternionDbl localRotation )
        {
            return localRotation;
        }
    }
}
