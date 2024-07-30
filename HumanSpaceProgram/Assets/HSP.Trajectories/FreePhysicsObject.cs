using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Time;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories
{
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class FreePhysicsObject : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        public Vector3 Position
        {
            get => this.transform.position;
            set
            {
                this._rb.position = value;
                this.transform.position = value;
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get => _absolutePosition;
            set
            {
                _absolutePosition = value;
                ReferenceFrameTransformUtils.UpdateScenePositionFromAbsolute( transform, _rb, value );
            }
        }

        public Quaternion Rotation
        {
            get => this.transform.rotation;
            set
            {
                this._rb.rotation = value;
                this.transform.rotation = value;
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get => _absoluteRotation;
            set
            {
                _absoluteRotation = value;
                ReferenceFrameTransformUtils.UpdateSceneRotationFromAbsolute( transform, _rb, value );
            }
        }

        public Vector3 Velocity
        {
            get => this._rb.velocity;
            set => this._rb.velocity = value;
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get => _absoluteVelocity;
            set
            {
                this._absoluteVelocity = value;
                ReferenceFrameTransformUtils.UpdateSceneVelocityFromAbsolute( _rb, value );
            }
        }

        public Vector3 Acceleration { get; private set; }
        public Vector3Dbl AbsoluteAcceleration { get; private set; }

        public Vector3 AngularVelocity
        {
            get => this._rb.angularVelocity;
            set => this._rb.angularVelocity = value;
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get => _absoluteAngularVelocity;
            set
            {
                this._absoluteAngularVelocity = value;
                ReferenceFrameTransformUtils.UpdateSceneAngularVelocityFromAbsolute( _rb, value );
            }
        }

        public Vector3 AngularAcceleration { get; private set; }

        public Vector3Dbl AbsoluteAngularAcceleration { get; private set; }

        [SerializeField] Vector3Dbl _absolutePosition;
        [SerializeField] QuaternionDbl _absoluteRotation;

        [SerializeField] Vector3Dbl _absoluteVelocity;
        [SerializeField] Vector3Dbl _absoluteAngularVelocity;

        Vector3 _oldVelocity;
        Vector3 _oldAngularVelocity;

        Vector3 _accelerationSum = Vector3.zero;
        Vector3 _angularAccelerationSum = Vector3.zero;

        //
        //
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

        Rigidbody _rb;

        public void AddForce( Vector3 force )
        {
            _accelerationSum += force / Mass;
            this._rb.AddForce( force, ForceMode.Force );
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            // I think force / mass is still correct for this,
            // - because 2 consecutive impulses in the same direction, but opposite positions (so that the angular accelerations cancel out)
            // - should still produce linear acceleration of 2 * force / mass.
            _accelerationSum += force / Mass;

            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            _angularAccelerationSum += Vector3.Cross( force, leverArm ) / Mass;

            // TODO - possibly cache the values across a frame and apply it once instead of n-times.
            this._rb.AddForceAtPosition( force, position, ForceMode.Force );
        }

        public void AddTorque( Vector3 force )
        {
            _accelerationSum += force / Mass;
            this._rb.AddTorque( force, ForceMode.Force );
        }

        private void RecacheAirfPosRot()
        {
            this._absolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( this._rb.position );
            this._absoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( this._rb.rotation );
        }

        void Awake()
        {
            if( this.HasComponentOtherThan<IPhysicsTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( FreePhysicsObject )} to a game object that already has a {nameof( IPhysicsTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = false;
        }

        void FixedUpdate()
        {
#warning TODO - this should act on the trajectory, vessel shouldn't care.
            Vector3Dbl airfGravityForce = GravityUtils.GetNBodyGravityForce( this.AbsolutePosition, this.Mass );
            this.AddForce( (Vector3)airfGravityForce );

            /*if( SceneReferenceFrameManager.ReferenceFrame is INonInertialReferenceFrame frame )
            {
                Vector3Dbl localPos = frame.InverseTransformPosition( this.AbsolutePosition );
                Vector3Dbl localVel = this.Velocity;
                Vector3Dbl localAngVel = this.AngularVelocity;
                Vector3Dbl linAcc = frame.GetFicticiousAcceleration( localPos, localVel );
                Vector3Dbl angAcc = frame.GetFictitiousAngularAcceleration( localPos, localAngVel );

                this.Acceleration += (Vector3)linAcc;
                this.AngularAcceleration += (Vector3)angAcc;
            }*/


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

            RecacheAirfPosRot();
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            // since we have the previous frame, we can just 
            //RecacheAirfPosRot( data.OldFrame );
            ReferenceFrameTransformUtils.UpdateScenePositionFromAbsolute( transform, _rb, _absolutePosition );
            ReferenceFrameTransformUtils.UpdateSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );
        }

        void OnEnable()
        {
            _rb.isKinematic = false; // Can't do `enabled = false` (doesn't exist) for a rigidbody, so we set it to kinematic instead.
        }

        void OnDisable()
        {
            _rb.isKinematic = true; // Can't do `enabled = false` (doesn't exist) for a rigidbody, so we set it to kinematic instead.
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


        [MapsInheritingFrom( typeof( FreePhysicsObject ) )]
        public static SerializationMapping FreePhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<FreePhysicsObject>()
            {
                ("mass", new Member<FreePhysicsObject, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<FreePhysicsObject, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<FreePhysicsObject, bool>( o => false, (o, value) => o._rb.isKinematic = false)), // TODO - isKinematic member is a hack.

                ("velocity", new Member<FreePhysicsObject, Vector3>( o => o.Velocity )),
                ("angular_velocity", new Member<FreePhysicsObject, Vector3>( o => o.AngularVelocity ))
            };
        }
    }
}