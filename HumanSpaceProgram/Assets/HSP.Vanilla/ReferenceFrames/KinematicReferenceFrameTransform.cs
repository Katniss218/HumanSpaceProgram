using HSP.ReferenceFrames;
using HSP.Time;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityPlus;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <summary>
    /// A physics transform that is free to move around and respond to forces, but doesn't respond to collisions (other objects can still collide with it).
    /// </summary>
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class KinematicReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        private ISceneReferenceFrameProvider _sceneReferenceFrameProvider;
        public ISceneReferenceFrameProvider SceneReferenceFrameProvider
        {
            get => _sceneReferenceFrameProvider;
            set
            {
                _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
                _sceneReferenceFrameProvider = value;
                _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            }
        }

        public Vector3 Position
        {
            get
            {
                return _rb.position;
            }
            set
            {
                // Set both absolute and rigidbody because the call might happen after physics/fixedupdate.
                _rb.position = value;
                transform.position = value;
                var absolutePos = SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( value );
                _actualAbsolutePosition = absolutePos;
                _requestedAbsolutePosition = absolutePos;
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get
            {
                return _actualAbsolutePosition;
            }
            set
            {
                // When setting, both values are set.
                Vector3 scenePos = (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( value );
                _actualAbsolutePosition = value;
                _requestedAbsolutePosition = value;
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, value );

                OnAbsolutePositionChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return _rb.rotation;
            }
            set
            {
                // Set both absolute and rigidbody because the call might happen after physics/fixedupdate.
                _rb.rotation = value;
                transform.rotation = value;
                var absoluteRot = SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformRotation( value );
                _actualAbsoluteRotation = absoluteRot;
                _requestedAbsoluteRotation = absoluteRot;
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get
            {
                return _actualAbsoluteRotation;
            }
            set
            {
                Quaternion sceneRot = (Quaternion)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformRotation( value );
                _actualAbsoluteRotation = value;
                _requestedAbsoluteRotation = value;
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, value );

                OnAbsoluteRotationChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3 Velocity
        {
            get
            {
                return _rb.velocity;
            }
            set
            {
                _absoluteVelocity = SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformVelocity( value );

                OnAbsoluteVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get
            {
                return _absoluteVelocity;
            }
            set
            {
                _absoluteVelocity = value;

                OnAbsoluteVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                return _rb.angularVelocity;
            }
            set
            {
                _absoluteAngularVelocity = SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAngularVelocity( value );

                OnAbsoluteAngularVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get
            {
                return _absoluteAngularVelocity;
            }
            set
            {
                _absoluteAngularVelocity = value;

                OnAbsoluteAngularVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3 Acceleration => (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformAcceleration( _absoluteAcceleration );
        public Vector3Dbl AbsoluteAcceleration => _absoluteAcceleration;
        public Vector3 AngularAcceleration => (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformAngularAcceleration( _absoluteAngularAcceleration );
        public Vector3Dbl AbsoluteAngularAcceleration => _absoluteAngularAcceleration;

        Vector3Dbl _requestedAbsolutePosition;
        Vector3Dbl _actualAbsolutePosition;
        QuaternionDbl _requestedAbsoluteRotation = QuaternionDbl.identity;
        QuaternionDbl _actualAbsoluteRotation = QuaternionDbl.identity;

        Vector3Dbl _absoluteAcceleration;
        Vector3Dbl _absoluteAngularAcceleration;

        Vector3Dbl _absoluteVelocity;
        Vector3Dbl _absoluteAngularVelocity;


        public event Action OnAbsolutePositionChanged;
        public event Action OnAbsoluteRotationChanged;
        public event Action OnAbsoluteVelocityChanged;
        public event Action OnAbsoluteAngularVelocityChanged;
        public event Action OnAnyValueChanged;

        //
        //
        //

        public float Mass { get; set; }

        public Vector3 LocalCenterOfMass { get; set; }

        public Vector3 MomentsOfInertia
        {
            get => this._rb.inertiaTensor;
            set => this._rb.inertiaTensor = value;
        }

        public Quaternion MomentsOfInertiaRotation
        {
            get => this._rb.inertiaTensorRotation;
            set => this._rb.inertiaTensorRotation = value;
        }

        public bool IsColliding { get; private set; }

        Rigidbody ___rb;
        Rigidbody _rb
        {
            get
            {
                if( ___rb == null )
                    ___rb = this.GetComponent<Rigidbody>();
                return ___rb;
            }
        }

        public void AddForce( Vector3 force )
        {
            _absoluteAcceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAcceleration( (Vector3Dbl)force / Mass );
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            Vector3Dbl torque = Vector3Dbl.Cross( force, leverArm );
            _absoluteAcceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAcceleration( (Vector3Dbl)force / Mass );
            _absoluteAngularAcceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAngularAcceleration( torque / this.GetInertia( torque.NormalizeToVector3() ) );
        }

        public void AddTorque( Vector3 torque )
        {
            _absoluteAngularAcceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAngularAcceleration( (Vector3Dbl)torque / this.GetInertia( torque.normalized ) );
        }

        protected virtual void Awake()
        {
            if( this.HasComponentOtherThan<IReferenceFrameTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {this.GetType().Name} to a game object that already has a {nameof( IReferenceFrameTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Continuous (in any of its flavors) "jumps" when sitting on top of something when reference frame switches.
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
        }

        protected virtual void FixedUpdate()
        {
            IReferenceFrame sceneReferenceFrameAfterPhysicsProcessing = SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT );

            // `_actualAbsolutePosition` should be up to date due to the callback inside physics step, which was invoked in the previous frame.

            _requestedAbsolutePosition = _actualAbsolutePosition + _absoluteVelocity * TimeManager.FixedDeltaTime;
            QuaternionDbl deltaRotation = QuaternionDbl.Euler( _absoluteAngularVelocity * TimeManager.FixedDeltaTime * 57.29577951308232 );
            _requestedAbsoluteRotation = deltaRotation * _actualAbsoluteRotation;

            var requestedPos = (Vector3)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformPosition( _requestedAbsolutePosition );
            var requestedRot = (Quaternion)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformRotation( _requestedAbsoluteRotation );

            _rb.Move( requestedPos, requestedRot );
        }

        public virtual void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            Vector3Dbl absolutePosition = this.AbsolutePosition;
            Vector3 scenePos = (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( absolutePosition );
            _rb.position = scenePos;
            transform.position = scenePos;
            _actualAbsolutePosition = absolutePosition;

            QuaternionDbl absoluteRotation = this.AbsoluteRotation;
            Quaternion sceneRot = (Quaternion)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformRotation( absoluteRotation );
            _rb.rotation = sceneRot;
            transform.rotation = sceneRot;
            _actualAbsoluteRotation = absoluteRotation;
        }

        protected virtual void OnEnable()
        {
            _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            _activeKinematicTransforms.Add( this );
            _rb.isKinematic = true; // Force kinematic.
        }

        protected virtual void OnDisable()
        {
            _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
            _activeKinematicTransforms.Remove( this );
            _rb.isKinematic = true;
        }

        protected virtual void OnCollisionEnter( Collision collision )
        {
            IsColliding = true;
        }

        protected virtual void OnCollisionStay( Collision collision )
        {
            // `OnCollisionEnter` / Exit are called for every collider.
            // I've tried using an incrementing/decrementing int with enter/exit, but it wasn't updating correctly, and after some time, there were too many collisions.
            // Using `OnCollisionStay` prevents desynchronization.

            IsColliding = true;
        }

        protected virtual void OnCollisionExit( Collision collision )
        {
            IsColliding = false;
        }


        public const string ADD_PLAYER_LOOP_SYSTEM = "12431242132131";

        // Imo it's kind of ugly using HSPEvent_STARTUP_IMMEDIATELY to mess with player loop, but it is what it is.
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_PLAYER_LOOP_SYSTEM )]
        static void AddPlayerLoopSystem()
        {
            PlayerLoopUtils.AddSystem<FixedUpdate, FixedUpdate.PhysicsFixedUpdate>( in _playerLoopSystem );
        }

        private static PlayerLoopSystem _playerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( SceneReferenceFrameManager ),
            updateDelegate = InsidePhysicsStep,
            subSystemList = null
        };

        static List<KinematicReferenceFrameTransform> _activeKinematicTransforms = new();

        static void InsidePhysicsStep()
        {
            // Assume that other objects aren't allowed to get the absolute position/velocity during physics step, as it is undefined (changes) during it.
            foreach( var t in _activeKinematicTransforms )
            {
                t._absoluteVelocity += t._absoluteAcceleration * TimeManager.FixedDeltaTime;
                t._absoluteAngularVelocity += t._absoluteAngularAcceleration * TimeManager.FixedDeltaTime;


                t._absoluteAcceleration = Vector3Dbl.zero;
                t._absoluteAngularAcceleration = Vector3Dbl.zero;

                t._actualAbsolutePosition = t._requestedAbsolutePosition;
                t._actualAbsoluteRotation = t._requestedAbsoluteRotation;
            }
        }

        [MapsInheritingFrom( typeof( KinematicReferenceFrameTransform ) )]
        public static SerializationMapping FreePhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<KinematicReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "mass", o => o.Mass )
                .WithMember( "local_center_of_mass", o => o.LocalCenterOfMass )

                .WithMember( "DO_NOT_TOUCH", o => true, ( o, value ) => o._rb.isKinematic = true ) // TODO - isKinematic member is a hack.

                .WithMember( "absolute_position", o => o.AbsolutePosition )
                .WithMember( "absolute_rotation", o => o.AbsoluteRotation )
                .WithMember( "absolute_velocity", o => o.AbsoluteVelocity )
                .WithMember( "absolute_angular_velocity", o => o.AbsoluteAngularVelocity );
        }
    }
}