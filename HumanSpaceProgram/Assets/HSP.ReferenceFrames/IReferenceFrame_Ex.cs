using UnityEngine;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// Extension methods for transforming individual kinematic quantities between
    /// local frame space and parent frame space using <see cref="IReferenceFrame"/>.
    /// </summary>
    /// <remarks>
    /// Transform methods operate relative to the frame's immediate parent space.
    /// Root frames use absolute space as their parent space.
    /// </remarks>
    public static class IReferenceFrame_Ex
    {
        /// <summary>
        /// Transforms a position from local frame space into parent frame space.
        /// </summary>
        /// <param name="frame">The reference frame the position is defined in.</param>
        /// <param name="localPosition">The position expressed in local frame space.</param>
        /// <returns>The transformed position in parent frame space.</returns>
        public static Vector3Dbl TransformPosition( this IReferenceFrame frame, Vector3Dbl localPosition )
        {
            var state = new KinematicState( frame, localPosition, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );
            return frame.TransformState( state ).Position;
        }

        /// <summary>
        /// Transforms a position from parent frame space into local frame space.
        /// </summary>
        /// <param name="frame">The target reference frame.</param>
        /// <param name="parentPosition">The position expressed in parent frame space.</param>
        /// <returns>The transformed position in local frame space.</returns>
        public static Vector3Dbl InverseTransformPosition( this IReferenceFrame frame, Vector3Dbl parentPosition )
        {
            var state = new KinematicState( null, parentPosition, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );
            return frame.InverseTransformState( state ).Position;
        }

        /// <summary>
        /// Transforms a rotation from local frame space into parent frame space.
        /// </summary>
        /// <param name="frame">The reference frame the rotation is defined in.</param>
        /// <param name="localRotation">The rotation expressed in local frame space.</param>
        /// <returns>The transformed rotation in parent frame space.</returns>
        public static QuaternionDbl TransformRotation( this IReferenceFrame frame, QuaternionDbl localRotation )
        {
            var state = new KinematicState( frame, Vector3Dbl.zero, localRotation, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );
            return frame.TransformState( state ).Rotation;
        }

        /// <summary>
        /// Transforms a rotation from parent frame space into local frame space.
        /// </summary>
        /// <param name="frame">The target reference frame.</param>
        /// <param name="parentRotation">The rotation expressed in parent frame space.</param>
        /// <returns>The transformed rotation in local frame space.</returns>
        public static QuaternionDbl InverseTransformRotation( this IReferenceFrame frame, QuaternionDbl parentRotation )
        {
            var state = new KinematicState( null, Vector3Dbl.zero, parentRotation, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );
            return frame.InverseTransformState( state ).Rotation;
        }

        /// <summary>
        /// Transforms a direction vector from local frame space into parent frame space.
        /// Translation components are ignored.
        /// </summary>
        /// <param name="frame">The reference frame the direction is defined in.</param>
        /// <param name="localDirection">The direction expressed in local frame space.</param>
        /// <returns>The transformed direction in parent frame space.</returns>
        public static Vector3 TransformDirection( this IReferenceFrame frame, Vector3 localDirection )
        {
            var state = KinematicState.GetIdentity( frame );
            var origin = frame.TransformState( state ).Position;

            state = new KinematicState(
                frame,
                (Vector3Dbl)localDirection,
                QuaternionDbl.identity,
                Vector3Dbl.zero,
                Vector3Dbl.zero,
                Vector3Dbl.zero,
                Vector3Dbl.zero );

            return (Vector3)(frame.TransformState( state ).Position - origin);
        }

        /// <summary>
        /// Transforms a direction vector from parent frame space into local frame space.
        /// Translation components are ignored.
        /// </summary>
        /// <param name="frame">The target reference frame.</param>
        /// <param name="parentDirection">The direction expressed in parent frame space.</param>
        /// <returns>The transformed direction in local frame space.</returns>
        public static Vector3 InverseTransformDirection( this IReferenceFrame frame, Vector3 parentDirection )
        {
            var state = KinematicState.GetIdentity(); // null frame
            var localOrigin = frame.InverseTransformState( state ).Position;

            state = new KinematicState(
                null,
                (Vector3Dbl)parentDirection,
                QuaternionDbl.identity,
                Vector3Dbl.zero,
                Vector3Dbl.zero,
                Vector3Dbl.zero,
                Vector3Dbl.zero );

            return (Vector3)(frame.InverseTransformState( state ).Position - localOrigin);
        }

        /// <summary>
        /// Transforms a linear velocity vector from local frame space into parent frame space.
        /// </summary>
        /// <param name="frame">The reference frame the velocity is defined in.</param>
        /// <param name="localVelocity">The velocity expressed in local frame space.</param>
        /// <returns>The transformed velocity in parent frame space.</returns>
        public static Vector3Dbl TransformVelocity( this IReferenceFrame frame, Vector3Dbl localVelocity )
        {
            var state = new KinematicState( frame, Vector3Dbl.zero, QuaternionDbl.identity, localVelocity, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );
            return frame.TransformState( state ).Velocity;
        }

        /// <summary>
        /// Transforms a linear velocity vector from parent frame space into local frame space.
        /// </summary>
        /// <param name="frame">The target reference frame.</param>
        /// <param name="parentVelocity">The velocity expressed in parent frame space.</param>
        /// <returns>The transformed velocity in local frame space.</returns>
        public static Vector3Dbl InverseTransformVelocity( this IReferenceFrame frame, Vector3Dbl parentVelocity )
        {
            var state = new KinematicState( null, Vector3Dbl.zero, QuaternionDbl.identity, parentVelocity, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero );
            return frame.InverseTransformState( state ).Velocity;
        }

        /// <summary>
        /// Transforms an angular velocity vector from local frame space into parent frame space.
        /// </summary>
        /// <param name="frame">The reference frame the angular velocity is defined in.</param>
        /// <param name="localAngularVelocity">The angular velocity expressed in local frame space.</param>
        /// <returns>The transformed angular velocity in parent frame space.</returns>
        public static Vector3Dbl TransformAngularVelocity( this IReferenceFrame frame, Vector3Dbl localAngularVelocity )
        {
            var state = new KinematicState( frame, Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, localAngularVelocity, Vector3Dbl.zero, Vector3Dbl.zero );
            return frame.TransformState( state ).AngularVelocity;
        }

        /// <summary>
        /// Transforms an angular velocity vector from parent frame space into local frame space.
        /// </summary>
        /// <param name="frame">The target reference frame.</param>
        /// <param name="parentAngularVelocity">The angular velocity expressed in parent frame space.</param>
        /// <returns>The transformed angular velocity in local frame space.</returns>
        public static Vector3Dbl InverseTransformAngularVelocity( this IReferenceFrame frame, Vector3Dbl parentAngularVelocity )
        {
            var state = new KinematicState( null, Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, parentAngularVelocity, Vector3Dbl.zero, Vector3Dbl.zero );
            return frame.InverseTransformState( state ).AngularVelocity;
        }

        /// <summary>
        /// Transforms a linear acceleration vector from local frame space into parent frame space.
        /// </summary>
        /// <param name="frame">The reference frame the acceleration is defined in.</param>
        /// <param name="localAcceleration">The acceleration expressed in local frame space.</param>
        /// <returns>The transformed acceleration in parent frame space.</returns>
        public static Vector3Dbl TransformAcceleration( this IReferenceFrame frame, Vector3Dbl localAcceleration )
        {
            var state = new KinematicState( frame, Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero, localAcceleration, Vector3Dbl.zero );
            return frame.TransformState( state ).Acceleration;
        }

        /// <summary>
        /// Transforms a linear acceleration vector from parent frame space into local frame space.
        /// </summary>
        /// <param name="frame">The target reference frame.</param>
        /// <param name="parentAcceleration">The acceleration expressed in parent frame space.</param>
        /// <returns>The transformed acceleration in local frame space.</returns>
        public static Vector3Dbl InverseTransformAcceleration( this IReferenceFrame frame, Vector3Dbl parentAcceleration )
        {
            var state = new KinematicState( null, Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero, parentAcceleration, Vector3Dbl.zero );
            return frame.InverseTransformState( state ).Acceleration;
        }

        /// <summary>
        /// Transforms an angular acceleration vector from local frame space into parent frame space.
        /// </summary>
        /// <param name="frame">The reference frame the angular acceleration is defined in.</param>
        /// <param name="localAngularAcceleration">The angular acceleration expressed in local frame space.</param>
        /// <returns>The transformed angular acceleration in parent frame space.</returns>
        public static Vector3Dbl TransformAngularAcceleration( this IReferenceFrame frame, Vector3Dbl localAngularAcceleration )
        {
            var state = new KinematicState( frame, Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, localAngularAcceleration );
            return frame.TransformState( state ).AngularAcceleration;
        }

        /// <summary>
        /// Transforms an angular acceleration vector from parent frame space into local frame space.
        /// </summary>
        /// <param name="frame">The target reference frame.</param>
        /// <param name="parentAngularAcceleration">The angular acceleration expressed in parent frame space.</param>
        /// <returns>The transformed angular acceleration in local frame space.</returns>
        public static Vector3Dbl InverseTransformAngularAcceleration( this IReferenceFrame frame, Vector3Dbl parentAngularAcceleration )
        {
            var state = new KinematicState( null, Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero, Vector3Dbl.zero, parentAngularAcceleration );
            return frame.InverseTransformState( state ).AngularAcceleration;
        }
    }
}