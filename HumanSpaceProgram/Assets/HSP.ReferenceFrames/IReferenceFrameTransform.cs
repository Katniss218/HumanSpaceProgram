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
        /// Gets the *scene-space* acceleration at this instant, in [m/s^2].
        /// </summary>
        Vector3 Acceleration { get; }
        /// <summary>
        /// Gets the acceleration at this instant, in [m/s^2].
        /// </summary>
        Vector3Dbl AbsoluteAcceleration { get; }

        /// <summary>
        /// Gets or sets the *scene-space* angular velocity, in [Rad/s].
        /// </summary>
        Vector3 AngularVelocity { get; set; }
        /// <summary>
        /// Gets or sets the angular velocity, in [Rad/s].
        /// </summary>
        Vector3Dbl AbsoluteAngularVelocity { get; set; }

        /// <summary>
        /// Gets the *scene-space* angular acceleration at this instant, in [Rad/s^2].
        /// </summary>
        Vector3 AngularAcceleration { get; }
        /// <summary>
        /// Gets the angular acceleration at this instant, in [Rad/s^2].
        /// </summary>
        Vector3Dbl AbsoluteAngularAcceleration { get; }
    }

    public interface IReferenceFrameSwitchResponder
    {
        /// <summary>
        /// Callback to the reference frame switch event.
        /// </summary>
        void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data );
    }
}