using HSP.ReferenceFrames;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <summary>
    /// A physics transform that is fixed to a point in space and doesn't move (in the absolute frame).
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class FixedPhysicsTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        Vector3Dbl _absolutePosition;
        QuaternionDbl _absoluteRotation;

        public Vector3 Position
        {
            get => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( _absolutePosition );
            set
            {
                this._absolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _absolutePosition );
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
            get => (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( _absoluteRotation );
            set
            {
                this._absoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );
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
            get => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( Vector3Dbl.zero );
            set { }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get => Vector3Dbl.zero;
            set { }
        }

        public Vector3 AngularVelocity
        {
            get => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularVelocity( Vector3Dbl.zero );
            set { }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get => Vector3Dbl.zero;
            set { }
        }

        public Vector3 Acceleration
        {
            get => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAcceleration( Vector3Dbl.zero );
        }

        public Vector3Dbl AbsoluteAcceleration
        {
            get => Vector3Dbl.zero;
        }

        public Vector3 AngularAcceleration
        {
            get => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularAcceleration( Vector3Dbl.zero );
        }

        public Vector3Dbl AbsoluteAngularAcceleration
        {
            get => Vector3Dbl.zero;
        }

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
            var pos = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( _absolutePosition );
            var rot = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( _absoluteRotation );
            this._rb.Move( pos, rot );
        }

        void Awake()
        {
            if( this.HasComponentOtherThan<IPhysicsTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( FixedPhysicsTransform )} to a game object that already has a {nameof( IPhysicsTransform )}. This is not allowed. Remove the previous physics object first." );
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
            _rb.isKinematic = true;
            MoveScenePositionAndRotation(); // Move, because the scene might be moving, and move ensures that the body is swept instead of teleported.
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            // This one is already idempotent as it simply recalculates the same absolute values to scene space.
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _absolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _absoluteRotation );
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


        [MapsInheritingFrom( typeof( FixedPhysicsTransform ) )]
        public static SerializationMapping FixedPhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<FixedPhysicsTransform>()
            {
                ("mass", new Member<FixedPhysicsTransform, float>( o => o.Mass )),
                ("local_center_of_mass", new Member<FixedPhysicsTransform, Vector3>( o => o.LocalCenterOfMass )),

                ("DO_NOT_TOUCH", new Member<FixedPhysicsTransform, bool>( o => true, (o, value) => o._rb.isKinematic = true)), // TODO - isKinematic member is a hack.

                ("absolute_position", new Member<FixedPhysicsTransform, Vector3Dbl>( o => o.AbsolutePosition )),
                ("absolute_rotation", new Member<FixedPhysicsTransform, QuaternionDbl>( o => o.AbsoluteRotation ))
            };
        }
    }
}