using HSP.ReferenceFrames;
using HSP.Time;
using System;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <summary>
    /// A physics transform that is free to move and collide with the environment.
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class KinematicPhysicsTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        public Vector3 Position
        {
            get => this.transform.position;
            set
            {
                this._absolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
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
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, value );
            }
        }

        public Quaternion Rotation
        {
            get => this.transform.rotation;
            set
            {
                this._absoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
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
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, value );
            }
        }

        public Vector3 Velocity
        {
            get => this._velocity;
            set
            {
                this._absoluteVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformVelocity( value );
                this._velocity = value;
            }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get => _absoluteVelocity;
            set
            {
                this._absoluteVelocity = value;

                Vector3 sceneVelocity = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( value );
                _velocity = sceneVelocity;
            }
        }

        public Vector3 AngularVelocity
        {
            get => this._angularVelocity;
            set
            {
                this._absoluteAngularVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformAngularVelocity( value );
                this._angularVelocity = value;
            }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get => _absoluteAngularVelocity;
            set
            {
                this._absoluteAngularVelocity = value;

                Vector3 sceneAngularVelocity = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularVelocity( value );
                _angularVelocity = sceneAngularVelocity;
            }
        }

        public Vector3 Acceleration => (Vector3)_acceleration;
        public Vector3Dbl AbsoluteAcceleration { get; private set; }

        public Vector3 AngularAcceleration => (Vector3)_angularAcceleration;
        public Vector3Dbl AbsoluteAngularAcceleration { get; private set; }

        [SerializeField] Vector3Dbl _acceleration;
        [SerializeField] Vector3Dbl _angularAcceleration;

        [SerializeField] Vector3Dbl _absolutePosition;
        [SerializeField] QuaternionDbl _absoluteRotation;

        [SerializeField] Vector3Dbl _absoluteVelocity;
        [SerializeField] Vector3Dbl _absoluteAngularVelocity;

        Vector3 _velocity;
        Vector3 _angularVelocity;

        Vector3 _oldVelocity;
        Vector3 _oldAngularVelocity;

        Vector3Dbl _absoluteAccelerationSum = Vector3.zero;
        Vector3Dbl _absoluteAngularAccelerationSum = Vector3.zero;

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
            //_absoluteAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );

            //this._rb.AddForce( force / Mass, ForceMode.VelocityChange );
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            /*Vector3 leverArm = position - this._rb.worldCenterOfMass;
            _absoluteAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );
            _absoluteAngularAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( Vector3Dbl.Cross( force, leverArm ) / Mass );

            // TODO - possibly cache the values across a frame and apply it once instead of n-times.
            this._rb.AddForceAtPosition( force / Mass, position, ForceMode.VelocityChange );*/
        }

        public void AddTorque( Vector3 torque )
        {
            //_absoluteAngularAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( (Vector3Dbl)torque / Mass );

            //this._rb.AddTorque( torque / Mass, ForceMode.VelocityChange );
        }

        /*private void MoveScenePositionAndRotation( IReferenceFrame referenceFrame )
        {
            var pos = (Vector3)referenceFrame.InverseTransformPosition( _absolutePosition );
            var rot = (Quaternion)referenceFrame.InverseTransformRotation( _absoluteRotation );
            this._rb.Move( pos, rot );

            //var vel = (Vector3)referenceFrame.InverseTransformVelocity( _absoluteVelocity );
            //var angVel = (Vector3)referenceFrame.InverseTransformAngularVelocity( _absoluteAngularVelocity );
            //this._rb.velocity = vel;
            //this._rb.angularVelocity = angVel;
        }*/

        private void RecalculateAbsoluteValues( IReferenceFrame referenceFrame )
        {
#warning TODO - after the scene position has been set, we lose track of what reference frame it's in. 
            // (after updating, the scene pos is now in the new frame instead of old, but this is still called with the old frame).

            // when transformposition is called first, it uses the 000 frame and correctly transforms the position.
            // when it's called again, it uses the 000 frame and transforms the ALREADY TRANSFORMED position into an incorrect position.

            this._absolutePosition = referenceFrame.TransformPosition( this._rb.position );
            if( Math.Abs( this._absolutePosition.magnitude ) > 0.01 )
            {
                Debug.Log( this._rb.position.magnitude + ":::::" + referenceFrame.TransformPosition( Vector3Dbl.zero ).magnitude );
            }
            this._absoluteRotation = referenceFrame.TransformRotation( this._rb.rotation );
            this._absoluteVelocity = referenceFrame.TransformVelocity( this._velocity );
            this._absoluteAngularVelocity = referenceFrame.TransformAngularVelocity( this._angularVelocity );

            this.AbsoluteAcceleration = referenceFrame.TransformAcceleration( this.Acceleration );
            this.AbsoluteAngularAcceleration = referenceFrame.TransformAcceleration( this.AngularAcceleration );
        }

        void Awake()
        {
            if( this.HasComponentOtherThan<IPhysicsTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( FreePhysicsTransform )} to a game object that already has a {nameof( IPhysicsTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Continuous (in any of its flavors) "jumps" when sitting on top of something when reference frame switches.
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
        }

        void FixedUpdate()
        {
            Quaternion deltaRotation = Quaternion.Euler( _angularVelocity * TimeManager.FixedDeltaTime * Mathf.Rad2Deg );
            _rb.Move( _rb.position + _velocity * TimeManager.FixedDeltaTime, deltaRotation * _rb.rotation );


            if( SceneReferenceFrameManager.ReferenceFrame is INonInertialReferenceFrame frame )
            {
                Vector3Dbl localPos = frame.InverseTransformPosition( this.AbsolutePosition );
                Vector3Dbl localVel = this.Velocity;
                Vector3Dbl localAngVel = this.AngularVelocity;
                Vector3 linAcc = (Vector3)frame.GetFicticiousAcceleration( localPos, localVel );
                Vector3 angAcc = (Vector3)frame.GetFictitiousAngularAcceleration( localPos, localAngVel );

                this._acceleration += linAcc;
                this._angularAcceleration += angAcc;
                //this._rb.AddForce( linAcc, ForceMode.Acceleration );
                //this._rb.AddTorque( angAcc, ForceMode.Acceleration );
            }


            // If the object is colliding, we will use its rigidbody accelerations, because we don't have access to the forces due to collisions.
            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.
            if( IsColliding )
            {
                this._acceleration = ((Vector3Dbl)(Velocity - _oldVelocity)) / TimeManager.FixedDeltaTime;
                this._angularAcceleration = ((Vector3Dbl)(AngularVelocity - _oldAngularVelocity)) / TimeManager.FixedDeltaTime;
            }
            else
            {
                // Acceleration sum will be whatever was accumulated between the previous frame (after it was zeroed out) and this frame. I think it should work fine.
                this._acceleration = _absoluteAccelerationSum;
                this._angularAcceleration = _absoluteAngularAccelerationSum;
            }

            // No need to sweep here, since this physics transform is simulated in scene space.
            RecalculateAbsoluteValues( SceneReferenceFrameManager.ReferenceFrame );

            this._oldVelocity = Velocity;
            this._oldAngularVelocity = AngularVelocity;
            this._absoluteAccelerationSum = Vector3.zero;
            this._absoluteAngularAccelerationSum = Vector3.zero;
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            // Enforces that subsequent calls of the function will not further transform the values into an incorrect value if the values has already been transformed.
            // - I.e. makes the method idempotent.
            // This allows calling this method to ensure that the absolute position/rotation/etc is correct.
            double value = Math.Abs( (data.NewFrame.TransformPosition( this._rb.position ) - this._absolutePosition).magnitude );
            //Debug.Log( this.gameObject.name + "    " + value );
            Debug.Log( this.gameObject.name + "    " + this._absolutePosition.magnitude ); // absoluteposition changes from 0 for some reason.
            if( value < 1 )
            {
                return;
            }
            else
            {

            }
#warning TODO - planets shouldn't have any part of their transform simulated in scene space because their centers are too far away from scene center.
            // and they don't even have to, because they won't have collision response.

            RecalculateAbsoluteValues( data.OldFrame );

            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _absolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );

            Vector3 sceneVelocity = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( _absoluteVelocity );
            _velocity = sceneVelocity;
            Vector3 sceneAngularVelocity = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularVelocity( _absoluteAngularVelocity );
            _angularVelocity = sceneAngularVelocity;
        }

        void OnEnable()
        {
            _rb.isKinematic = true; // Force kinematic.
        }

        void OnDisable()
        {
            _rb.isKinematic = true;
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

        [MapsInheritingFrom( typeof( KinematicPhysicsTransform ) )]
        public static SerializationMapping FreePhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<KinematicPhysicsTransform>()
            {
                ("mass", new Member<KinematicPhysicsTransform, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<KinematicPhysicsTransform, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<KinematicPhysicsTransform, bool>( o => true, (o, value) => o._rb.isKinematic = true)), // TODO - isKinematic member is a hack.

                ("absolute_position", new Member<KinematicPhysicsTransform, Vector3Dbl>( o => o.AbsolutePosition )),
                ("absolute_rotation", new Member<KinematicPhysicsTransform, QuaternionDbl>( o => o.AbsoluteRotation )),
                ("absolute_velocity", new Member<KinematicPhysicsTransform, Vector3Dbl>( o => o.AbsoluteVelocity )),
                ("absolute_angular_velocity", new Member<KinematicPhysicsTransform, Vector3Dbl>( o => o.AbsoluteAngularVelocity ))
            };
        }
    }
}