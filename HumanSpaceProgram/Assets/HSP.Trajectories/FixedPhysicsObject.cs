using HSP.ReferenceFrames;
using HSP.Time;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories
{
    /// <remarks>
    /// This is a wrapper for a rigidbody.
    /// </remarks>
    [RequireComponent( typeof( ReferenceFrameTransform ) )]
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class FixedPhysicsObject : MonoBehaviour, IPhysicsObject
    {
        public float Mass
        {
            get => this._rb.mass;
            set => this._rb.mass = value;
        }

        public Vector3 LocalCenterOfMass
        {
            get => this._rb.centerOfMass;
            set => this._rb.centerOfMass = value;
        }

        public Vector3 Velocity
        {
            get => Vector3.zero;
            set { return; }
        }

        public Vector3 Acceleration { get; private set; }

        public Vector3 AngularVelocity
        {
            get => Vector3.zero;
            set { return; }
        }

        public Vector3 AngularAcceleration { get; private set; }

        public Vector3 MomentsOfInertia => this._rb.inertiaTensor;

        public Matrix3x3 MomentOfInertiaTensor
        {
            get
            {
                Matrix3x3 R = Matrix3x3.Rotate( this._rb.inertiaTensorRotation );
                Matrix3x3 S = Matrix3x3.Scale( this._rb.inertiaTensor );
                return R * S * R.transpose;
            }
            set
            {
                (Vector3 eigenvector, float eigenvalue)[] eigen = value.Diagonalize().OrderByDescending( m => m.eigenvalue ).ToArray();
                this._rb.inertiaTensor = new Vector3( eigen[0].eigenvalue, eigen[1].eigenvalue, eigen[2].eigenvalue );
                Matrix3x3 m = new Matrix3x3( eigen[0].eigenvector.x, eigen[0].eigenvector.y, eigen[0].eigenvector.z,
                    eigen[1].eigenvector.x, eigen[1].eigenvector.y, eigen[1].eigenvector.z,
                    eigen[2].eigenvector.x, eigen[2].eigenvector.y, eigen[2].eigenvector.z );
                this._rb.inertiaTensorRotation = m.rotation;
            }
        }

        public bool IsColliding { get; private set; }

        ReferenceFrameTransform _rootObjTransform;
        Rigidbody _rb;

        public void AddForce( Vector3 force )
        {
            return;
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            return;
        }

        public void AddTorque( Vector3 force )
        {
            return;
        }

        void Awake()
        {
            if( this.HasComponentOtherThan<IPhysicsObject>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( FixedPhysicsObject )} to a game object that already has a {nameof( IPhysicsObject )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();
            _rootObjTransform = this.gameObject.GetOrAddComponent<ReferenceFrameTransform>();
            _rootObjTransform.RefreshCachedRigidbody();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
        }

        void FixedUpdate()
        {
            _rb.isKinematic = true;
            this.Acceleration = Vector3.zero;
            this.AngularAcceleration = Vector3.zero;
        }

        void OnCollisionEnter( Collision collision )
        {
            IsColliding = true;
        }

        void OnCollisionStay( Collision collision )
        {
            // `OnCollisionEnter` / Exit are called for every collider.
            // I've tried using an incrementing/decrementing int with enter/exit, but it wasn't updating correctly, and after some time, there were too many collisions.
            // Using `OnCollisionStay` prevents desynchronization.

            IsColliding = true;
        }

        void OnCollisionExit( Collision collision )
        {
            IsColliding = false;
        }

        void OnEnable()
        {
            _rb.isKinematic = true;
        }

        void OnDisable()
        {
            _rb.isKinematic = true;
        }


        [MapsInheritingFrom( typeof( FixedPhysicsObject ) )]
        public static SerializationMapping FixedPhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<FixedPhysicsObject>()
            {
                ("mass", new Member<FixedPhysicsObject, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<FixedPhysicsObject, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<FixedPhysicsObject, bool>( o => true, (o, value) => o._rb.isKinematic = true)), // TODO - isKinematic member is a hack.
            };
        }
    }
}