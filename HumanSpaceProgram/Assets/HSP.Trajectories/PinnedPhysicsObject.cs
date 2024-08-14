using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Time;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories
{
    /// <remarks>
    /// A physobj that is pinned to a fixed pos/rot in the local coordinate system of a celestial body.
    /// </remarks>
	[RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class PinnedPhysicsObject : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        CelestialBody _referenceBody = null;
        Vector3Dbl _referencePosition = Vector3.zero;
        QuaternionDbl _referenceRotation = QuaternionDbl.identity;

        public CelestialBody ReferenceBody
        {
            get => _referenceBody;
            set
            {
                _referenceBody = value;
                if( value != null )
                {
                    ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
                    ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
                }
            }
        }

        public Vector3Dbl ReferencePosition
        {
            get => _referencePosition;
            set { _referencePosition = value; ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition ); }
        }

        public QuaternionDbl ReferenceRotation
        {
            get => _referenceRotation;
            set { _referenceRotation = value; ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation ); }
        }


        public Vector3 Position
        {
            get => this.transform.position;
            set
            {
                Vector3Dbl airfPos = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
                _referencePosition = _referenceBody.OrientedReferenceFrame.InverseTransformPosition( airfPos );

                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
            }
        }

        public Quaternion Rotation
        {
            get => this.transform.rotation;
            set
            {
                QuaternionDbl airfRot = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
                _referenceRotation = _referenceBody.OrientedReferenceFrame.InverseTransformRotation( airfRot );

                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get => _referenceBody.OrientedReferenceFrame.TransformPosition( _referencePosition );
            set
            {
                _referencePosition = _referenceBody.OrientedReferenceFrame.InverseTransformPosition( value );
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, value );
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get => _referenceBody.OrientedReferenceFrame.TransformRotation( _referenceRotation );
            set
            {
                _referenceRotation = _referenceBody.OrientedReferenceFrame.InverseTransformRotation( value );

                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, value );
            }
        }


        public Vector3 Velocity
        {
            get => this._rb.velocity;
            set { }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get => SceneReferenceFrameManager.ReferenceFrame.TransformVelocity( this._rb.velocity );
            set { }
        }

        public Vector3 AngularVelocity
        {
            get => this._rb.angularVelocity;
            set { }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get => SceneReferenceFrameManager.ReferenceFrame.TransformAngularVelocity( this._rb.angularVelocity );
            set { }
        }

        public Vector3 Acceleration { get; private set; }
        public Vector3Dbl AbsoluteAcceleration { get; private set; }

        public Vector3 AngularAcceleration { get; private set; }
        public Vector3Dbl AbsoluteAngularAcceleration { get; private set; }


        //

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

        Vector3 _oldVelocity;
        Vector3 _oldAngularVelocity;

        Vector3 _accelerationSum = Vector3.zero;
        Vector3 _angularAccelerationSum = Vector3.zero;

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

        private void MoveScenePositionAndRotation()
        {
            if( ReferenceBody == null )
                return;

            var frame = ReferenceBody.OrientedReferenceFrame;
            var pos = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( frame.TransformPosition( _referencePosition ) );
            var rot = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( frame.TransformRotation( _referenceRotation ) );
            this._rb.Move( pos, rot );
        }

        void Awake()
        {
            if( this.HasComponentOtherThan<IPhysicsTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( PinnedPhysicsObject )} to a game object that already has a {nameof( IPhysicsTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
        }

        void FixedUpdate()
        {
            // Reference can be moving, and we aren't parented (due to precision), thus we continuously update.
            this.MoveScenePositionAndRotation();

            // If the object is colliding, we will use its rigidbody accelerations, because we don't have access to the forces due to collisions.
            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.
            if( IsColliding )
            {
                this.Acceleration = (Velocity - _oldVelocity) / TimeManager.FixedDeltaTime;
                this.AngularAcceleration = (AngularVelocity - _oldAngularVelocity) / TimeManager.FixedDeltaTime;
            }
            else
            {
                // Acceleration sum will be whatever was accumulated between the previous frame (after it was zeroed out) and this frame. I think it should work fine.
                this.Acceleration = _accelerationSum;
                this.AngularAcceleration = _angularAccelerationSum;
            }

            this._oldVelocity = Velocity;
            this._oldAngularVelocity = AngularVelocity;
            this._accelerationSum = Vector3.zero;
            this._angularAccelerationSum = Vector3.zero;
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            // Guarantees that the reference body has up-to-date reference frame, regardless of update order.
            // Simply calling `OnSceneReferenceFrameSwitch` on it should be safe,
            //   it'll just set the position twice (both times to the same value) in the same frame.
            _referenceBody.ReferenceFrameTransform.OnSceneReferenceFrameSwitch( data );

            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
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

        [MapsInheritingFrom( typeof( PinnedPhysicsObject ) )]
        public static SerializationMapping PinnedPhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<PinnedPhysicsObject>()
            {
                ("mass", new Member<PinnedPhysicsObject, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<PinnedPhysicsObject, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<PinnedPhysicsObject, bool>( o => true, (o, value) => o._rb.isKinematic = true)), // TODO - isKinematic member is a hack.

                ("reference_body", new Member<PinnedPhysicsObject, string>( o => o.ReferenceBody == null ? null : o.ReferenceBody.ID, (o, value) => o.ReferenceBody = value == null ? null : CelestialBodyManager.Get( value ) )),
                ("reference_position", new Member<PinnedPhysicsObject, Vector3Dbl>( o => o.ReferencePosition )),
                ("reference_rotation", new Member<PinnedPhysicsObject, QuaternionDbl>( o => o.ReferenceRotation ))
            };
        }
    }
}