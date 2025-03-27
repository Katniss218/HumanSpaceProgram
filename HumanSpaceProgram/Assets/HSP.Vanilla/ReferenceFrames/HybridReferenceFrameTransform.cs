using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityPlus;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class HybridReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        private bool _allowSceneSimulation = false;
        /// <summary>
        /// If true, the object is allowed to simulate using scene space, allowing for collisions, when the position and velocity are within the range allowed for scene space simulation.
        /// </summary>
        public bool AllowSceneSimulation
        {
            get => _allowSceneSimulation;
            set
            {
                _allowSceneSimulation = value;
                if( value )
                {
                    SwitchToSceneMode();
                }
                else
                {
                    SwitchToAbsoluteMode();
                }
            }
        }

        /// <summary>
        /// The allowed values for scene position, in [m]. <br/>
        /// Outside of this range the object will be simulated using absolute space.
        /// </summary>
        public float PositionRange { get; set; }
        /// <summary>
        /// The allowed values for scene velocity, in [m/s]. <br/>
        /// Outside of this range the object will be simulated using absolute space.
        /// </summary>
        public float VelocityRange { get; set; }
        /// <summary>
        /// The maximum allowed timescale. <br/>
        /// When the timescale is higher than this value, the object will be simulated using absolute space.
        /// </summary>
        public float MaxTimeScale { get; set; }

        // absolute space simulation variables

        Vector3Dbl _requestedAbsolutePosition;
        Vector3Dbl _actualAbsolutePosition;
        QuaternionDbl _requestedAbsoluteRotation = QuaternionDbl.identity;
        QuaternionDbl _actualAbsoluteRotation = QuaternionDbl.identity;

        Vector3Dbl _absoluteAcceleration;
        Vector3Dbl _absoluteAngularAcceleration;

        Vector3Dbl _absoluteVelocity;
        Vector3Dbl _absoluteAngularVelocity;

        //

        bool _isSceneSpace;
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

        public Vector3 Position
        {
            get => this._rb.position;
            set
            {
                if( _isSceneSpace && (Math.Abs( value.x ) > PositionRange || Math.Abs( value.y ) > PositionRange || Math.Abs( value.z ) > PositionRange) )
                {
                    SwitchToAbsoluteMode();
                }
                // Set both absolute and rigidbody because the call might happen after physics/fixedupdate.
                _rb.position = value;
                transform.position = value;
                var absolutePos = SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( value );
                _actualAbsolutePosition = absolutePos;
                _requestedAbsolutePosition = absolutePos;

                OnAbsolutePositionChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get
            {
                if( _isSceneSpace )
                    return SceneReferenceFrameManager.ReferenceFrame.TransformPosition( _rb.position );
                else
                    return _actualAbsolutePosition;
            }
            set
            {
                var scenePos = SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( value );
                if( _isSceneSpace && (Math.Abs( scenePos.x ) > PositionRange || Math.Abs( scenePos.y ) > PositionRange || Math.Abs( scenePos.z ) > PositionRange) )
                {
                    SwitchToAbsoluteMode();
                }
                _actualAbsolutePosition = value;
                _requestedAbsolutePosition = value;
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, value );

                OnAbsolutePositionChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Quaternion Rotation
        {
            get => this._rb.rotation;
            set
            {
                // Set both absolute and rigidbody because the call might happen after physics/fixedupdate.
                _rb.rotation = value;
                transform.rotation = value;
                var absoluteRot = SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( value );
                _actualAbsoluteRotation = absoluteRot;
                _requestedAbsoluteRotation = absoluteRot;

                OnAbsoluteRotationChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get
            {
                if( _isSceneSpace )
                    return SceneReferenceFrameManager.ReferenceFrame.TransformRotation( _rb.rotation );
                else
                    return _actualAbsoluteRotation;
            }
            set
            {
                _actualAbsoluteRotation = value;
                _requestedAbsoluteRotation = value;
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, value );

                OnAbsoluteRotationChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3 Velocity
        {
            get => this._rb.velocity;
            set
            {
                if( _isSceneSpace && (Math.Abs( value.x ) > VelocityRange || Math.Abs( value.y ) > VelocityRange || Math.Abs( value.z ) > VelocityRange) )
                {
                    SwitchToAbsoluteMode();
                }

                if( _isSceneSpace )
                    _rb.velocity = value;
                _absoluteVelocity = SceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( value );

                OnAbsoluteVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get
            {
                if( _isSceneSpace )
                    return SceneReferenceFrameManager.ReferenceFrame.TransformVelocity( _rb.velocity );
                else
                    return _absoluteVelocity;
            }
            set
            {
                var sceneVel = SceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( value );
                if( _isSceneSpace && (Math.Abs( sceneVel.x ) > VelocityRange || Math.Abs( sceneVel.y ) > VelocityRange || Math.Abs( sceneVel.z ) > VelocityRange) )
                {
                    SwitchToAbsoluteMode();
                }

                _absoluteVelocity = value;
                if( _isSceneSpace )
                    ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( _rb, value );

                OnAbsoluteVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3 AngularVelocity
        {
            get => this._rb.angularVelocity;
            set
            {
                if( _isSceneSpace )
                    _rb.angularVelocity = value;
                _absoluteAngularVelocity = SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularVelocity( value );

                OnAbsoluteAngularVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get
            {
                if( _isSceneSpace )
                    return SceneReferenceFrameManager.ReferenceFrame.TransformAngularVelocity( _rb.angularVelocity );
                else
                    return _absoluteAngularVelocity;
            }
            set
            {
                _absoluteAngularVelocity = value;
                if( _isSceneSpace )
                    ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( _rb, value );

                OnAbsoluteAngularVelocityChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public Vector3 Acceleration => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAcceleration( _absoluteAcceleration );
        public Vector3Dbl AbsoluteAcceleration => _absoluteAcceleration;
        public Vector3 AngularAcceleration => (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularAcceleration( _absoluteAngularAcceleration );
        public Vector3Dbl AbsoluteAngularAcceleration => _absoluteAngularAcceleration;

        public event Action OnAbsolutePositionChanged;
        public event Action OnAbsoluteRotationChanged;
        public event Action OnAbsoluteVelocityChanged;
        public event Action OnAbsoluteAngularVelocityChanged;
        public event Action OnAnyValueChanged;

        //
        //
        //

        private float _mass; // rb.mass has internal limits to how big you can make it.
        public float Mass
        {
            get => this._mass;
            set
            {
                _mass = value;
                this._rb.mass = value;
            }
        }

        public Vector3 LocalCenterOfMass
        {
            get => this._rb.centerOfMass;
            set => this._rb.centerOfMass = value;
        }

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

        public void AddForce( Vector3 force )
        {
            _absoluteAcceleration += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );

            if( _isSceneSpace )
            {
                this._rb.AddForce( force, ForceMode.Force );
            }
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            Vector3Dbl torque = Vector3Dbl.Cross( force, leverArm );
            _absoluteAcceleration += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );
            _absoluteAngularAcceleration += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( torque / this.GetInertia( torque.NormalizeToVector3() ) );

            if( _isSceneSpace )
            {
                this._rb.AddForceAtPosition( force, position, ForceMode.Force );
            }
        }

        public void AddTorque( Vector3 torque )
        {
            _absoluteAngularAcceleration += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( (Vector3Dbl)torque / this.GetInertia( torque.normalized ) );

            if( _isSceneSpace )
            {
                this._rb.AddTorque( torque, ForceMode.Force );
            }
        }

        private void SwitchToAbsoluteMode()
        {
            IReferenceFrame sceneReferenceFrame = SceneReferenceFrameManager.ReferenceFrame;

            _absoluteVelocity = sceneReferenceFrame.TransformVelocity( _rb.velocity );
            _actualAbsolutePosition = sceneReferenceFrame.TransformPosition( _rb.position );
            _requestedAbsolutePosition = _actualAbsolutePosition + (_absoluteVelocity * TimeManager.FixedDeltaTime);

            _absoluteAngularVelocity = sceneReferenceFrame.TransformAngularVelocity( _rb.angularVelocity );
            _actualAbsoluteRotation = sceneReferenceFrame.TransformRotation( _rb.rotation );
            QuaternionDbl deltaRotation = QuaternionDbl.Euler( _absoluteAngularVelocity * TimeManager.FixedDeltaTime * 57.29577951308232 );
            _requestedAbsoluteRotation = deltaRotation * _actualAbsoluteRotation;

            _isSceneSpace = false;
            _rb.isKinematic = true;
        }

        private void SwitchToSceneMode()
        {
            IReferenceFrame sceneReferenceFrame = SceneReferenceFrameManager.ReferenceFrame;

            _isSceneSpace = true;
            _rb.isKinematic = false;

            _rb.velocity = (Vector3)sceneReferenceFrame.InverseTransformVelocity( _absoluteVelocity );
            Vector3 requestedPos = (Vector3)sceneReferenceFrame.InverseTransformPosition( _requestedAbsolutePosition );
            Quaternion requestedRot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( _requestedAbsoluteRotation );

            // set values immediately so that the returned AbsolutePosition is correct immediately after exiting this method.
            _rb.position = requestedPos;
            _rb.rotation = requestedRot;
        }

        protected virtual void Awake()
        {
            if( this.HasComponentOtherThan<IReferenceFrameTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( HybridReferenceFrameTransform )} to a game object that already has a {nameof( IReferenceFrameTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Continuous (in any of its flavors) "jumps" when sitting on top of something when reference frame switches.
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = !_isSceneSpace;
        }

        void FixedUpdate()
        {
            if( _allowSceneSimulation )
            {
                if( _isSceneSpace )
                {
                    Vector3 scenePos = _rb.position;
                    Vector3 sceneVel = _rb.velocity;

                    if( Mathf.Abs( scenePos.x ) > PositionRange || Mathf.Abs( scenePos.y ) > PositionRange || Mathf.Abs( scenePos.z ) > PositionRange
                     || Mathf.Abs( sceneVel.x ) > PositionRange || Mathf.Abs( sceneVel.y ) > PositionRange || Mathf.Abs( sceneVel.z ) > PositionRange
                     || TimeManager.TimeScale > MaxTimeScale )
                    {
                        SwitchToAbsoluteMode();
                    }
                }
                else
                {
                    Vector3 scenePos = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( _actualAbsolutePosition );
                    Vector3 sceneVel = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( _absoluteVelocity );

                    if( Mathf.Abs( scenePos.x ) <= PositionRange && Mathf.Abs( scenePos.y ) <= PositionRange && Mathf.Abs( scenePos.z ) <= PositionRange
                     && Mathf.Abs( sceneVel.x ) <= PositionRange && Mathf.Abs( sceneVel.y ) <= PositionRange && Mathf.Abs( sceneVel.z ) <= PositionRange
                     && TimeManager.TimeScale <= MaxTimeScale )
                    {
                        SwitchToSceneMode();
                    }
                }
            }

            if( _isSceneSpace )
            {
                if( SceneReferenceFrameManager.ReferenceFrame is INonInertialReferenceFrame frame )
                {
                    Vector3Dbl localPos = frame.InverseTransformPosition( this.AbsolutePosition );
                    Vector3Dbl localVel = this.Velocity;
                    Vector3Dbl localAngVel = this.AngularVelocity;
                    Vector3 linAcc = (Vector3)frame.GetFicticiousAcceleration( localPos, localVel );
                    Vector3 angAcc = (Vector3)frame.GetFictitiousAngularAcceleration( localPos, localAngVel );

                    this._rb.AddForce( linAcc, ForceMode.Acceleration );
                    this._rb.AddTorque( angAcc, ForceMode.Acceleration );
                }
            }
            else
            {
                IReferenceFrame sceneReferenceFrameAfterPhysicsProcessing = SceneReferenceFrameManager.ReferenceFrame.AtUT( TimeManager.UT );

                // `_actualAbsolutePosition` should be up to date due to the callback inside physics step, which was invoked in the previous frame.

                _requestedAbsolutePosition = _actualAbsolutePosition + _absoluteVelocity * TimeManager.FixedDeltaTime;
                QuaternionDbl deltaRotation = QuaternionDbl.Euler( _absoluteAngularVelocity * TimeManager.FixedDeltaTime * 57.29577951308232 );
                _requestedAbsoluteRotation = deltaRotation * _actualAbsoluteRotation;

                var requestedPos = (Vector3)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformPosition( _requestedAbsolutePosition );
                var requestedRot = (Quaternion)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformRotation( _requestedAbsoluteRotation );

                _rb.Move( requestedPos, requestedRot );
            }
        }

        public virtual void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            if( _isSceneSpace )
            {
                Vector3 scenePos = _rb.position;
                Vector3 sceneVel = _rb.velocity;
                var sceneReferenceFrame = data.OldFrame;
                _actualAbsolutePosition = sceneReferenceFrame.TransformPosition( scenePos );
                _actualAbsoluteRotation = sceneReferenceFrame.TransformRotation( _rb.rotation );
                _absoluteVelocity = sceneReferenceFrame.TransformVelocity( sceneVel );
                _absoluteAngularVelocity = sceneReferenceFrame.TransformAngularVelocity( _rb.angularVelocity );

                if( Mathf.Abs( scenePos.x ) > PositionRange || Mathf.Abs( scenePos.y ) > PositionRange || Mathf.Abs( scenePos.z ) > PositionRange
                 || Mathf.Abs( sceneVel.x ) > PositionRange || Mathf.Abs( sceneVel.y ) > PositionRange || Mathf.Abs( sceneVel.z ) > PositionRange )
                {
                    SwitchToAbsoluteMode();
                }

                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, _actualAbsolutePosition, data.NewFrame );
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, _actualAbsoluteRotation );
                ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( _rb, _absoluteVelocity );
                ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( _rb, _absoluteAngularVelocity );
            }
            else
            {
                Vector3Dbl absolutePosition = this.AbsolutePosition;
                Vector3 scenePos = (Vector3)SceneReferenceFrameManager.ReferenceFrame.InverseTransformPosition( absolutePosition );
                _rb.position = scenePos;
                transform.position = scenePos;
                _actualAbsolutePosition = absolutePosition;

                QuaternionDbl absoluteRotation = this.AbsoluteRotation;
                Quaternion sceneRot = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( absoluteRotation );
                _rb.rotation = sceneRot;
                transform.rotation = sceneRot;
                _actualAbsoluteRotation = absoluteRotation;
            }
        }

        void OnEnable()
        {
            _activeHybridTransforms.Add( this );
        }

        void OnDisable()
        {
            _activeHybridTransforms.Remove( this );
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


        public const string ADD_PLAYER_LOOP_SYSTEM = "76523523453544";

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

        static List<HybridReferenceFrameTransform> _activeHybridTransforms = new();

        static void InsidePhysicsStep()
        {
            // This is required to happen indide physics step to properly account for all forces added during fixedupdate IF THE OBJECT IS IN ABSOLUTE MODE.
            // Some calls to AddForce might happen after FixedUpdate for this component has been called, and thus would only be accounted for in the next frame.
            //   This is unacceptable.

            // Assume that other objects aren't allowed to get the absolute position/velocity during physics step, as it is undefined (changes) during it.
            foreach( var t in _activeHybridTransforms )
            {
                if( !t._isSceneSpace )
                {
                    t._absoluteVelocity += t._absoluteAcceleration * TimeManager.FixedDeltaTime;
                    t._absoluteAngularVelocity += t._absoluteAngularAcceleration * TimeManager.FixedDeltaTime;
                }

                t._absoluteAcceleration = Vector3Dbl.zero;
                t._absoluteAngularAcceleration = Vector3Dbl.zero;

                t._actualAbsolutePosition = t._requestedAbsolutePosition;
                t._actualAbsoluteRotation = t._requestedAbsoluteRotation;
            }
        }

        [MapsInheritingFrom( typeof( HybridReferenceFrameTransform ) )]
        public static SerializationMapping FreePhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<HybridReferenceFrameTransform>()
                .WithMember( "allow_scene_simulation", o => o.AllowSceneSimulation )
                .WithMember( "position_range", o => o.PositionRange )
                .WithMember( "velocity_range", o => o.VelocityRange )
                .WithMember( "max_timescale", o => o.MaxTimeScale )

                .WithMember( "mass", o => o.Mass )
                .WithMember( "local_center_of_mass", o => o.LocalCenterOfMass )

                .WithMember( "absolute_position", o => o.AbsolutePosition )
                .WithMember( "absolute_rotation", o => o.AbsoluteRotation )
                .WithMember( "absolute_velocity", o => o.AbsoluteVelocity )
                .WithMember( "absolute_angular_velocity", o => o.AbsoluteAngularVelocity );
        }
    }
}