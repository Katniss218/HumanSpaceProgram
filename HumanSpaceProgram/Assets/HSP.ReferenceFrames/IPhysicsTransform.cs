using UnityEngine;

namespace HSP.ReferenceFrames
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

        /// <summary>
        /// Gets the principal moments of inertia, in [kg*m^2].
        /// </summary>
        /// <remarks>
        /// The principal moments of inertia are orthogonal to each other, centered at the center of mass, and rotated by <see cref="MomentsOfInertiaRotation"/> relative to the local space of the object.
        /// </remarks>
        Vector3 MomentsOfInertia { get; set; }

        /// <summary>
        /// Gets or sets the orientation of the principal moments of inertia. <br/>
        /// This orientation is relative to the local space of the physics object.
        /// </summary>
        Quaternion MomentsOfInertiaRotation { get; set; }

        /// <summary>
        /// True if the physics object is colliding with any other objects in the current frame, false otherwise.
        /// </summary>
        bool IsColliding { get; }

        /// <summary>
        /// Applies a force (in scene space) through the center of mass, in [N].
        /// </summary>
        void AddForce( Vector3 force );

        /// <summary>
        /// Applies a force (in scene space) at the specified position, in [N]. <br/>
        /// By extension, can organically apply torque.
        /// </summary>
        void AddForceAtPosition( Vector3 force, Vector3 position );

        /// <summary>
        /// Applies a torque (in scene space) through the center of mass, in [N*m].
        /// </summary>
        void AddTorque( Vector3 torque );
    }
}