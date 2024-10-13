using System;
using UnityEngine;

namespace HSP.ReferenceFrames
{
    public interface IReferenceFrameTransform : IComponent, IReferenceFrameSwitchResponder
    {
        /// <summary>
        /// Gets or sets the *scene space* position.
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        Vector3Dbl AbsolutePosition { get; set; }

        /// <summary>
        /// Gets or sets the *scene space* rotation.
        /// </summary>
        Quaternion Rotation { get; set; }
        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        QuaternionDbl AbsoluteRotation { get; set; }

        /// <summary>
        /// Gets or sets the *scene space* velocity, in [m/s].
        /// </summary>
        Vector3 Velocity { get; set; }
        /// <summary>
        /// Gets or sets the velocity, in [m/s].
        /// </summary>
        Vector3Dbl AbsoluteVelocity { get; set; }

        /// <summary>
        /// Gets or sets the *scene-space* angular velocity, in [Rad/s].
        /// </summary>
        Vector3 AngularVelocity { get; set; }
        /// <summary>
        /// Gets or sets the angular velocity, in [Rad/s].
        /// </summary>
        Vector3Dbl AbsoluteAngularVelocity { get; set; }

        /// <summary>
        /// Gets the *scene-space* acceleration at this instant, in [m/s^2].
        /// </summary>
        Vector3 Acceleration { get; }
        /// <summary>
        /// Gets the acceleration at this instant, in [m/s^2].
        /// </summary>
        Vector3Dbl AbsoluteAcceleration { get; }

        /// <summary>
        /// Gets the *scene-space* angular acceleration at this instant, in [Rad/s^2].
        /// </summary>
        Vector3 AngularAcceleration { get; }
        /// <summary>
        /// Gets the angular acceleration at this instant, in [Rad/s^2].
        /// </summary>
        Vector3Dbl AbsoluteAngularAcceleration { get; }

        /// <summary>
        /// Invoked when the absolute position property is set.
        /// </summary>
        event Action OnAbsolutePositionChanged;
        /// <summary>
        /// Invoked when the absolute rotation is set.
        /// </summary>
        event Action OnAbsoluteRotationChanged;
        /// <summary>
        /// Invoked when the absolute velocity is set.
        /// </summary>
        event Action OnAbsoluteVelocityChanged;
        /// <summary>
        /// Invoked when the absolute angular velocity is set.
        /// </summary>
        event Action OnAbsoluteAngularVelocityChanged;
        /// <summary>
        /// Invoked when any of the primary values (pos/rot/vel/angvel) are set.
        /// </summary>
        event Action OnAnyValueChanged;

#warning TODO - add methods for getting velocity/acceleration/forces of a point relative to the center.
        // maybe via getting the instantaneous non-inertial reference frame representing this physical body.
        // similarly to how it works for celestial bodies.
        // - They would have to have correct UT based on where they were called.
    }

    public interface IReferenceFrameSwitchResponder
    {
        /// <summary>
        /// Callback to the reference frame switch event.
        /// </summary>
        /// <remarks>
        /// This method will be called AFTER 'PhysicsProcessing', but still in the same fixedupdate as it.
        /// </remarks>
        void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data );
    }
}