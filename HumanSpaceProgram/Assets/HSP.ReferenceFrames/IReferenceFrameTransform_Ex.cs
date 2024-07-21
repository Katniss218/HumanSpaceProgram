using UnityEngine;

namespace HSP.ReferenceFrames
{
    public static class IReferenceFrameTransform_Ex
    {
        public static Vector3Dbl GetVelocity( this IReferenceFrameTransform transform, IReferenceFrame referenceFrame )
        {
            return referenceFrame.InverseTransformVelocity( transform.AbsoluteVelocity );
        }

        public static void SetVelocity( this IReferenceFrameTransform transform, IReferenceFrame referenceFrame, Vector3Dbl localAngularVelocity )
        {
            transform.AbsoluteVelocity = referenceFrame.TransformVelocity( localAngularVelocity );
        }

        public static Vector3Dbl GetAngularVelocity( this IReferenceFrameTransform transform, IReferenceFrame referenceFrame )
        {
            return referenceFrame.InverseTransformAngularVelocity( transform.AbsoluteVelocity );
        }

        public static void SetAngularVelocity( this IReferenceFrameTransform transform, IReferenceFrame referenceFrame, Vector3Dbl localAngularVelocity )
        {
            transform.AbsoluteVelocity = referenceFrame.TransformAngularVelocity( localAngularVelocity );
        }
    }
}