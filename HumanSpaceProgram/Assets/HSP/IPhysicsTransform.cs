using UnityEngine;

namespace HSP
{
    /// <summary>
    /// Any object that interacts with the collision/physics system.
    /// </summary>
    public interface IPhysicsTransform : IComponent
    {
        /// <summary>
        /// Gets or sets the physics object's mass, in [kg].
        /// </summary>
        float Mass { get; set; }

        /// <summary>
        /// Gets or sets the physics object's local center of mass (in physics object's coordinate space).
        /// </summary>
        Vector3 LocalCenterOfMass { get; set; }

       /* /// <summary>
        /// Gets or sets the physics object's velocity in scene space, in [m/s].
        /// </summary>
        Vector3 Velocity { get; set; }

        /// <summary>
        /// Gets the acceleration that this physics object is under at this instant, in [m/s^2].
        /// </summary>
        Vector3 Acceleration { get; }

        /// <summary>
        /// Gets or sets the physics object's angular velocity in scene space, in [Rad/s].
        /// </summary>
        Vector3 AngularVelocity { get; set; }

        /// <summary>
        /// Gets the angular acceleration that this physics object is under at this instant, in [Rad/s^2].
        /// </summary>
        Vector3 AngularAcceleration { get; }*/
        
        /// <summary>
        /// Gets the principal moments of inertia.
        /// </summary>
        Vector3 MomentsOfInertia { get; }
        
        /// <summary>
        /// Gets or sets the moment of inertia tensor.
        /// </summary>
        Matrix3x3 MomentOfInertiaTensor { get; set; }

        /// <summary>
        /// True if the physics object is colliding with any other objects in the current frame, false otherwise.
        /// </summary>
        bool IsColliding { get; }

        /// <summary>
        /// Applies a force at the center of mass, in [N].
        /// </summary>
        void AddForce( Vector3 force );

        /// <summary>
        /// Applies a force at the specified position, in [N]. <br/>
        /// By extension, can organically apply torque.
        /// </summary>
        void AddForceAtPosition( Vector3 force, Vector3 position );

        /// <summary>
        /// Applies a torque through the center of mass, in [N*m].
        /// </summary>
        void AddTorque( Vector3 torque );
    }
}