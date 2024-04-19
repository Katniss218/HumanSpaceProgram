using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.ReferenceFrames
{
    /// <summary>
    /// A reference frame centered on a given point, and with a given orientation. This class is immutable.
    /// </summary>
    public sealed class OrientedReferenceFrame : IReferenceFrame
    {
        private Vector3Dbl _position;
        private QuaternionDbl _rotation;
        private QuaternionDbl _inverseRotation;

        public OrientedReferenceFrame( Vector3Dbl center, QuaternionDbl rotation )
        {
            this._position = center;
            this._rotation = rotation;
            this._inverseRotation = QuaternionDbl.Inverse( rotation );
        }

        public IReferenceFrame Shift( Vector3Dbl airfDistanceDelta )
        {
            return new OrientedReferenceFrame( this._position + airfDistanceDelta, this._rotation );
        }

        public Vector3Dbl InverseTransformPosition( Vector3Dbl airfPosition )
        {
            return _inverseRotation * Vector3Dbl.Subtract( airfPosition, _position );
        }

        public Vector3Dbl TransformPosition( Vector3Dbl localPosition )
        {
            return _rotation * Vector3Dbl.Subtract( localPosition, _position );
        }

        public Vector3 InverseTransformDirection( Vector3 airfDirection )
        {
            return (Vector3)(_inverseRotation * airfDirection);
        }

        public Vector3 TransformDirection( Vector3 localDirection )
        {
            return (Vector3)(_rotation * localDirection);
        }

        public QuaternionDbl InverseTransformRotation( QuaternionDbl airfRotation )
        {
            return _inverseRotation * airfRotation;
        }

        public QuaternionDbl TransformRotation( QuaternionDbl localRotation )
        {
            return _rotation * localRotation;
        }
    }
}