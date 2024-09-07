using HSP.CelestialBodies;
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
    public class FreeReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        public Vector3 Position
        {
            get => this._rb.position;
            set
            {
                this._cachedAbsolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
                MakeCacheInvalid();
                this._rb.position = value;
                this.transform.position = value;
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get
            {
                // Recalculation is needed to fix update order - if something requests absolute position before it's cached in FixedUpdate.
                RecalculateCacheIfNeeded();
                return _cachedAbsolutePosition;
            }
            set
            {
                _cachedAbsolutePosition = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, value );
            }
        }

        public Quaternion Rotation
        {
            get => this._rb.rotation;
            set
            {
                this._cachedAbsoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
                MakeCacheInvalid();
                this._rb.rotation = value;
                this.transform.rotation = value;
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get
            {
                // Recalculation is needed to fix update order - if something requests absolute rotation before it's cached in FixedUpdate.
                RecalculateCacheIfNeeded();
                return _cachedAbsoluteRotation;
            }
            set
            {
                _cachedAbsoluteRotation = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, value );
            }
        }

        public Vector3 Velocity
        {
            get => this._rb.velocity;
            set
            {
                this._cachedAbsoluteVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformVelocity( value );
                MakeCacheInvalid();
                this._rb.velocity = value;
            }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get
            {
                // Recalculation is needed to fix update order - if something requests absolute velocity before it's cached in FixedUpdate.
                RecalculateCacheIfNeeded();
                return _cachedAbsoluteVelocity;
            }
            set
            {
                this._cachedAbsoluteVelocity = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( _rb, value );
            }
        }

        public Vector3 AngularVelocity
        {
            get => this._rb.angularVelocity;
            set
            {
                this._cachedAbsoluteAngularVelocity = SceneReferenceFrameManager.ReferenceFrame.TransformAngularVelocity( value );
                MakeCacheInvalid();
                this._rb.angularVelocity = value;
            }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get
            {
                // Recalculation is needed to fix update order - if something requests absolute angular velocity before it's cached in FixedUpdate.
                RecalculateCacheIfNeeded();
                return _cachedAbsoluteAngularVelocity;
            }
            set
            {
                this._cachedAbsoluteAngularVelocity = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( _rb, value );
            }
        }

        public Vector3 Acceleration => _cachedAcceleration;
        public Vector3Dbl AbsoluteAcceleration => _cachedAbsoluteAcceleration;
        public Vector3 AngularAcceleration => _cachedAngularAcceleration;
        public Vector3Dbl AbsoluteAngularAcceleration => _cachedAbsoluteAngularAcceleration;

        /// <summary> The scene frame in which the cached values are expressed. </summary>
        IReferenceFrame _cachedSceneReferenceFrame;
        Vector3Dbl _cachedAbsolutePosition;
        QuaternionDbl _cachedAbsoluteRotation;
        Vector3Dbl _cachedAbsoluteVelocity;
        Vector3Dbl _cachedAbsoluteAngularVelocity;

        Vector3 _cachedAcceleration;
        Vector3Dbl _cachedAbsoluteAcceleration;
        Vector3 _cachedAngularAcceleration;
        Vector3Dbl _cachedAbsoluteAngularAcceleration;

        Vector3 _oldPosition;

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
            _absoluteAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );

            this._rb.AddForce( force, ForceMode.Force );
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            _absoluteAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );
            _absoluteAngularAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( Vector3Dbl.Cross( force, leverArm ) / Mass );

            // TODO - possibly cache the values across a frame and apply it once instead of n-times.
            this._rb.AddForceAtPosition( force, position, ForceMode.Force );
        }

        public void AddTorque( Vector3 torque )
        {
            _absoluteAngularAccelerationSum += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( (Vector3Dbl)torque / Mass );

            this._rb.AddTorque( torque, ForceMode.Force );
        }

        /*private void MoveScenePositionAndRotation( IReferenceFrame referenceFrame )
        {
            var pos = (Vector3)referenceFrame.InverseTransformPosition( _absolutePosition );
            var rot = (Quaternion)referenceFrame.InverseTransformRotation( _absoluteRotation );
            this._rb.Move( pos, rot );

            var vel = (Vector3)referenceFrame.InverseTransformVelocity( _absoluteVelocity );
            var angVel = (Vector3)referenceFrame.InverseTransformAngularVelocity( _absoluteAngularVelocity );
            this._rb.velocity = vel;
            this._rb.angularVelocity = angVel;
        }*/

        private void RecalculateCacheIfNeeded()
        {
            if( IsCacheValid() )
                return;

            RecalculateCache( SceneReferenceFrameManager.ReferenceFrame );
            MakeCacheValid();
        }

        private void RecalculateCache( IReferenceFrame sceneReferenceFrame )
        {
            //Debug.LogWarning( "FREE RECALCULATING" );
            _cachedAbsolutePosition = sceneReferenceFrame.TransformPosition( _rb.position );
            _cachedAbsoluteRotation = sceneReferenceFrame.TransformRotation( _rb.rotation );
            _cachedAbsoluteVelocity = sceneReferenceFrame.TransformVelocity( _rb.velocity );
            _cachedAbsoluteAngularVelocity = sceneReferenceFrame.TransformAngularVelocity( _rb.angularVelocity );
            // Don't cache acceleration, since it's impossible to compute it here for a dynamic body. Acceleration is recalculated on every fixedupdate instead.
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

        // Exact comparison of the axes catches the most cases (and it's gonna be set to match exactly so it's okay)
        // Vector3's `==` operator does approximate comparison.
        private bool IsCacheValid() => (_rb.position.x == _oldPosition.x && _rb.position.y == _oldPosition.y && _rb.position.z == _oldPosition.z
            && SceneReferenceFrameManager.ReferenceFrame.Equals( _cachedSceneReferenceFrame ));

        private void MakeCacheValid() => _oldPosition = _rb.position;

        private void MakeCacheInvalid() => _oldPosition = -_rb.position + new Vector3( 1234.56789f, 12345678.9f, 1.23456789f );

        void Awake()
        {
            if( this.HasComponentOtherThan<IReferenceFrameTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( FreeReferenceFrameTransform )} to a game object that already has a {nameof( IReferenceFrameTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Continuous (in any of its flavors) "jumps" when sitting on top of something when reference frame switches.
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = false;
        }

        int i = 0;

        void FixedUpdate()
        {
            if( SceneReferenceFrameManager.ReferenceFrame is INonInertialReferenceFrame frame )
            {
                Vector3Dbl localPos = frame.InverseTransformPosition( this.AbsolutePosition );
                Vector3Dbl localVel = this.Velocity;
                Vector3Dbl localAngVel = this.AngularVelocity;
                Vector3 linAcc = (Vector3)frame.GetFicticiousAcceleration( localPos, localVel );
                Vector3 angAcc = (Vector3)frame.GetFictitiousAngularAcceleration( localPos, localAngVel );

                this._cachedAcceleration += linAcc;
                this._cachedAngularAcceleration += angAcc;
                this._rb.AddForce( linAcc, ForceMode.Acceleration );
                this._rb.AddTorque( angAcc, ForceMode.Acceleration );
            }

#warning TODO - timemanager.fixeddeltatime might not equal time.fixeddeltatime (at high warp values), it needs to be handled explicitly here (analogous to kinematic, but only when warp is not synced).

            // If the object is colliding, we will use its rigidbody accelerations, because we don't have access to the forces due to collisions.
            // Otherwise, we use our more precise method that relies on full encapsulation of the rigidbody.
            if( IsColliding )
            {
                _cachedAcceleration = (Velocity - _oldVelocity) / TimeManager.FixedDeltaTime;
                _cachedAngularAcceleration = (AngularVelocity - _oldAngularVelocity) / TimeManager.FixedDeltaTime;

                _cachedAbsoluteAcceleration = SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( _cachedAcceleration );
                _cachedAbsoluteAngularAcceleration = SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( _cachedAngularAcceleration );
            }
            else
            {
                // Acceleration sum will be whatever was accumulated between the previous frame (after it was zeroed out) and this frame. I think it should work fine.
                _cachedAbsoluteAcceleration = _absoluteAccelerationSum;
                _cachedAbsoluteAngularAcceleration = _absoluteAngularAccelerationSum;

                _cachedAcceleration = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAcceleration( _cachedAbsoluteAcceleration );
                _cachedAngularAcceleration = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularAcceleration( _cachedAbsoluteAngularAcceleration );
            }

            this._oldVelocity = Velocity;
            this._oldAngularVelocity = AngularVelocity;
            this._absoluteAccelerationSum = Vector3.zero;
            this._absoluteAngularAccelerationSum = Vector3.zero;
            i++;
        }

        // The faster something goes in scene space when colliding with another thing, it gets laggier for physics processing (computation of "contacts")

        // when switching while resting on something, the object jumps. possibly due to pinned updating before the celestial frame it uses to transform has correct values or something?
        // possibly the same or related to rovers in RSS/RO jumping while driving
        // - only happens with continuous speculative collision (continuous and continuous dynamic don't jump).

#warning TODO - needs something to enable continuous when a something in the scene is not resting and is moving fast relative to something else.

#warning TODO - celestial bodies need something that will replace the buildin parenting of colliders with 64-bit parents and update their scene position at all times (fixedupdate + update + lateupdate).

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
#warning TODO - removing this doesn't seem to do anything anymore. It's also not done properly.
            // Enforces that subsequent calls of the function will not further transform the values into an incorrect value if the values has already been transformed.
            // - I.e. makes the method idempotent.
            // This allows calling this method to ensure that the absolute position/rotation/etc is correct.
            if( Math.Abs( (data.NewFrame.TransformPosition( this._rb.position ) - this._cachedAbsolutePosition).magnitude ) < 1e-1 )
            {
                return;
            }
            var cachedFrame = _cachedSceneReferenceFrame;
            RecalculateCache( data.OldFrame ); // Old frame because the current scene-space data is still in the old frame.
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _cachedAbsolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _cachedAbsoluteRotation );
            ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( _rb, _cachedAbsoluteVelocity );
            ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( _rb, _cachedAbsoluteAngularVelocity );
            _cachedSceneReferenceFrame = cachedFrame;
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

        [MapsInheritingFrom( typeof( FreeReferenceFrameTransform ) )]
        public static SerializationMapping FreePhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<FreeReferenceFrameTransform>()
            {
                ("mass", new Member<FreeReferenceFrameTransform, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<FreeReferenceFrameTransform, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<FreeReferenceFrameTransform, bool>( o => false, (o, value) => o._rb.isKinematic = false)), // TODO - isKinematic member is a hack.

                ("absolute_position", new Member<FreeReferenceFrameTransform, Vector3Dbl>( o => o.AbsolutePosition )),
                ("absolute_rotation", new Member<FreeReferenceFrameTransform, QuaternionDbl>( o => o.AbsoluteRotation )),
                ("absolute_velocity", new Member<FreeReferenceFrameTransform, Vector3Dbl>( o => o.AbsoluteVelocity )),
                ("absolute_angular_velocity", new Member<FreeReferenceFrameTransform, Vector3Dbl>( o => o.AbsoluteAngularVelocity ))
            };
        }
    }
}