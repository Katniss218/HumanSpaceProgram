using UnityEngine;

namespace HSP.ReferenceFrames
{
    public interface IReferenceFrameTransform : IComponent, IReferenceFrameSwitchResponder
    {
        Vector3 Position { get; set; }
        Vector3Dbl AbsolutePosition { get; set; }

        Quaternion Rotation { get; set; }
        QuaternionDbl AbsoluteRotation { get; set; }

        /// <summary>
        /// Gets or sets the physics object's velocity in scene space, in [m/s].
        /// </summary>
        Vector3 Velocity { get; set; }
        Vector3Dbl AbsoluteVelocity { get; set; }

        /// <summary>
        /// Gets the acceleration that this physics object is under at this instant, in [m/s^2].
        /// </summary>
        Vector3 Acceleration { get; }
        Vector3Dbl AbsoluteAcceleration { get; }

        /// <summary>
        /// Gets or sets the physics object's angular velocity in scene space, in [Rad/s].
        /// </summary>
        Vector3 AngularVelocity { get; set; }
        Vector3Dbl AbsoluteAngularVelocity { get; set; }

        /// <summary>
        /// Gets the angular acceleration that this physics object is under at this instant, in [Rad/s^2].
        /// </summary>
        Vector3 AngularAcceleration { get; }
        Vector3Dbl AbsoluteAngularAcceleration { get; }
    }

    public interface IReferenceFrameSwitchResponder
    {
        void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data );
    }
}