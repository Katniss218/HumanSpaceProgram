using HSP.ReferenceFrames;
using HSP.Time;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityPlus.PlayerLoop;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vanilla.ReferenceFrames
{
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class HybridReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        private ISceneReferenceFrameProvider _sceneReferenceFrameProvider;
        public ISceneReferenceFrameProvider SceneReferenceFrameProvider
        {
            get => _sceneReferenceFrameProvider;
            set
            {
                if( _sceneReferenceFrameProvider == value )
                    return;

                _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
                _sceneReferenceFrameProvider = value;
                _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            }
        }

        private bool _allowSceneSimulation = true;
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
        public float PositionRange { get; set; } = 1000;
        /// <summary>
        /// The allowed values for scene velocity, in [m/s]. <br/>
        /// Outside of this range the object will be simulated using absolute space.
        /// </summary>
        public float VelocityRange { get; set; } = 150;
        /// <summary>
        /// The maximum allowed timescale. <br/>
        /// When the timescale is higher than this value, the object will be simulated using absolute space.
        /// </summary>
        public float MaxTimeScale { get; set; } = 16;

        // absolute space simulation variables

        private KinematicState _state = KinematicState.AbsoluteIdentity;
        private KinematicState _requestedState = KinematicState.AbsoluteIdentity;

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

        public ref readonly KinematicState GetStateRef( out IReferenceFrame referenceFrame )
        {
            RecalculateCacheIfNeeded();
            referenceFrame = null;
            return ref _state;
        }

        public KinematicState GetState( IReferenceFrame requestedFrame )
        {
            RecalculateCacheIfNeeded();
            return _state.InFrame( requestedFrame );
        }

        public void SetState( in KinematicState state )
        {
            _state = state.InFrame( null );
            _requestedState = _state;
            if( _isSceneSpace )
            {
                var scenePos = SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( _state.Position );
                if( _isSceneSpace && (Math.Abs( scenePos.x ) > PositionRange || Math.Abs( scenePos.y ) > PositionRange || Math.Abs( scenePos.z ) > PositionRange) )
                {
                    SwitchToAbsoluteMode();
                }
                else
                {
                    ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Position );
                    ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Rotation );
                    ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), _rb, _state.Velocity );
                    ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), _rb, _state.AngularVelocity );
                }
            }
            MakeCacheValid();
            OnStateChanged?.Invoke();
        }

        public void ModifyState( IReferenceFrame requestedFrame, KinematicStateMutator mutator )
        {
            RecalculateCacheIfNeeded();
            if( requestedFrame == null )
            {
                mutator( ref _state );
            }
            else
            {
                var localState = _state.InFrame( requestedFrame );
                mutator( ref localState );
                _state = localState.InFrame( null );
            }
            _requestedState = _state;

            if( _isSceneSpace )
            {
                var scenePos = SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( _state.Position );
                if( _isSceneSpace && (Math.Abs( scenePos.x ) > PositionRange || Math.Abs( scenePos.y ) > PositionRange || Math.Abs( scenePos.z ) > PositionRange) )
                {
                    SwitchToAbsoluteMode();
                }
                else
                {
                    ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Position );
                    ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, _rb, _state.Rotation );
                    ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), _rb, _state.Velocity );
                    ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), _rb, _state.AngularVelocity );
                }
            }
            MakeCacheValid();
            OnStateChanged?.Invoke();
        }

        /// <summary> The scene frame in which the cached values are expressed. </summary>
        IReferenceFrame _cachedSceneReferenceFrame;

        Vector3 _lastCachedPosition = new Vector3( 0.21454141f, -23465435.352342f, 231.6354523f );
        Vector3 _lastCachedVelocity = new Vector3( 0.21454141f, -23465435.352342f, 231.6354523f );
        Quaternion _lastCachedRotation = new Quaternion( 0.21454141f, -23465435.352342f, 231.6354523f, 45.3412435f );
        Vector3 _lastCachedAngularVelocity = new Vector3( 0.21454141f, -23465435.352342f, 231.6354523f );

        protected void RecalculateCacheIfNeeded()
        {
            if( IsCacheValid() )
                return;

            RecalculateCache( SceneReferenceFrameProvider.GetSceneReferenceFrame() );
            MakeCacheValid();
        }

        protected void RecalculateCache( IReferenceFrame sceneReferenceFrame )
        {
            if( _isSceneSpace )
            {
                _state.Position = sceneReferenceFrame.TransformPosition( this.gameObject.activeInHierarchy ? _rb.position : transform.position );
                _state.Rotation = sceneReferenceFrame.TransformRotation( this.gameObject.activeInHierarchy ? _rb.rotation : transform.rotation );
                if( this.gameObject.activeInHierarchy )
                {
                    _state.Velocity = sceneReferenceFrame.TransformVelocity( _rb.velocity );
                    _state.AngularVelocity = sceneReferenceFrame.TransformAngularVelocity( _rb.angularVelocity );
                }
            }
            _cachedSceneReferenceFrame = sceneReferenceFrame;
        }

        protected virtual bool IsCacheValid()
        {
            if( _cachedSceneReferenceFrame == null ) return false;
            if( !SceneReferenceFrameProvider.GetSceneReferenceFrame().EqualsIgnoreUT( _cachedSceneReferenceFrame ) ) return false;

            if( _isSceneSpace )
            {
                return (_rb.position.x == _lastCachedPosition.x && _rb.position.y == _lastCachedPosition.y && _rb.position.z == _lastCachedPosition.z)
                    && (_rb.rotation.x == _lastCachedRotation.x && _rb.rotation.y == _lastCachedRotation.y && _rb.rotation.z == _lastCachedRotation.z && _rb.rotation.w == _lastCachedRotation.w)
                    && (_rb.velocity.x == _lastCachedVelocity.x && _rb.velocity.y == _lastCachedVelocity.y && _rb.velocity.z == _lastCachedVelocity.z)
                    && (_rb.angularVelocity.x == _lastCachedAngularVelocity.x && _rb.angularVelocity.y == _lastCachedAngularVelocity.y && _rb.angularVelocity.z == _lastCachedAngularVelocity.z);
            }
            return true;
        }

        protected virtual void MakeCacheValid()
        {
            if( _isSceneSpace )
            {
                _lastCachedPosition = _rb.position;
                _lastCachedRotation = _rb.rotation;
                _lastCachedVelocity = _rb.velocity;
                _lastCachedAngularVelocity = _rb.angularVelocity;
            }
        }

        protected virtual void MakeCacheInvalid() => _lastCachedPosition = _rb.position + new Vector3( 1234.56789f, 12345678.9f, 1.23456789f );

        public event Action OnStateChanged;

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
            if( force.sqrMagnitude < 1e-6 )
                return;

            _state.Acceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAcceleration( (Vector3Dbl)force / Mass );

            if( _isSceneSpace )
            {
                this._rb.AddForce( force, ForceMode.Force );
            }
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            _state.Acceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAcceleration( (Vector3Dbl)force / Mass );
            if( _isSceneSpace )
                this._rb.AddForce( force, ForceMode.Force );

            Vector3Dbl torque = Vector3Dbl.Cross( leverArm, force );
            if( torque.sqrMagnitude > 1e-6 )
            {
                _state.AngularAcceleration += torque / this.GetInertia( torque.NormalizeToVector3() );
                if( _isSceneSpace )
                    this._rb.AddTorque( (Vector3)torque, ForceMode.Force );
            }
        }

        public void AddTorque( Vector3 torque )
        {
            if( torque.sqrMagnitude < 1e-6 )
                return;

            _state.AngularAcceleration += SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAngularAcceleration( (Vector3Dbl)torque / this.GetInertia( torque.normalized ) );

            if( _isSceneSpace )
            {
                this._rb.AddTorque( torque, ForceMode.Force );
            }
        }

        public void AddAbsoluteForce( Vector3 force )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            _state.Acceleration += (Vector3Dbl)force / Mass;

            if( _isSceneSpace )
            {
                this._rb.AddForce( SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformDirection( force ), ForceMode.Force );
            }
        }

        public void AddAbsoluteForceAtPosition( Vector3 force, Vector3Dbl position )
        {
            if( force.sqrMagnitude < 1e-6 )
                return;

            var referenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame();
            _state.Acceleration += (Vector3Dbl)force / Mass;
            if( _isSceneSpace )
                this._rb.AddForce( referenceFrame.InverseTransformDirection( force ), ForceMode.Force );

            Vector3Dbl leverArm = position - referenceFrame.TransformPosition( this._rb.worldCenterOfMass );
            Vector3Dbl torque = Vector3Dbl.Cross( leverArm, force );
            if( torque.sqrMagnitude > 1e-6 )
            {
                _state.AngularAcceleration += torque / this.GetInertia( torque.NormalizeToVector3() );
                if( _isSceneSpace )
                    this._rb.AddTorque( (Vector3)referenceFrame.InverseTransformDirection( (Vector3)torque ), ForceMode.Force );
            }
        }

        public void AddAbsoluteTorque( Vector3 torque )
        {
            if( torque.sqrMagnitude < 1e-6 )
                return;

            _state.AngularAcceleration += (Vector3Dbl)torque / this.GetInertia( torque.normalized );

            if( _isSceneSpace )
            {
                this._rb.AddTorque( SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformDirection( torque ), ForceMode.Force );
            }
        }

        private void SwitchToAbsoluteMode()
        {
            IReferenceFrame sceneReferenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame();

            _state.Velocity = sceneReferenceFrame.TransformVelocity( _rb.velocity );
            _state.Position = sceneReferenceFrame.TransformPosition( _rb.position );
            _requestedState.Position = _state.Position + (_state.Velocity * TimeManager.FixedDeltaTime);

            _state.AngularVelocity = sceneReferenceFrame.TransformAngularVelocity( _rb.angularVelocity );
            _state.Rotation = sceneReferenceFrame.TransformRotation( _rb.rotation );
            QuaternionDbl deltaRotation = QuaternionDbl.AngleAxis( _state.AngularVelocity.magnitude * TimeManager.FixedDeltaTime * 57.29577951308232, _state.AngularVelocity );
            _requestedState.Rotation = deltaRotation * _state.Rotation;

            _isSceneSpace = false;
            _rb.isKinematic = true;
        }

        private void SwitchToSceneMode()
        {
            IReferenceFrame sceneReferenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame();

            _isSceneSpace = true;
            _rb.isKinematic = false;

            _rb.velocity = (Vector3)sceneReferenceFrame.InverseTransformVelocity( _state.Velocity );
            _rb.angularVelocity = (Vector3)sceneReferenceFrame.InverseTransformAngularVelocity( _state.AngularVelocity );
            Vector3 requestedPos = (Vector3)sceneReferenceFrame.InverseTransformPosition( _requestedState.Position );
            Quaternion requestedRot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( _requestedState.Rotation );

            // set values immediately so that the returned AbsolutePosition is correct immediately after exiting this method.
            _rb.position = requestedPos;
            _rb.rotation = requestedRot;
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
            _rb.isKinematic = !_isSceneSpace;
            _rb.drag = 0;
            _rb.angularDrag = 0;
            _rb.maxAngularVelocity = 9000;
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
                     || Mathf.Abs( sceneVel.x ) > VelocityRange || Mathf.Abs( sceneVel.y ) > VelocityRange || Mathf.Abs( sceneVel.z ) > VelocityRange
                     || TimeManager.TimeScale > MaxTimeScale )
                    {
                        SwitchToAbsoluteMode();
                    }
                }
                else
                {
                    var frame = SceneReferenceFrameProvider.GetSceneReferenceFrame();
                    Vector3 scenePos = (Vector3)frame.InverseTransformPosition( _state.Position );
                    Vector3 sceneVel = (Vector3)frame.InverseTransformVelocity( _state.Velocity );

                    if( Mathf.Abs( scenePos.x ) <= PositionRange && Mathf.Abs( scenePos.y ) <= PositionRange && Mathf.Abs( scenePos.z ) <= PositionRange
                     && Mathf.Abs( sceneVel.x ) <= VelocityRange && Mathf.Abs( sceneVel.y ) <= VelocityRange && Mathf.Abs( sceneVel.z ) <= VelocityRange
                     && TimeManager.TimeScale <= MaxTimeScale )
                    {
                        SwitchToSceneMode();
                    }
                }

                if( _isSceneSpace )
                {
                    // apply noninertial force.
                    if( SceneReferenceFrameProvider.GetSceneReferenceFrame() is INonInertialReferenceFrame frame )
                    {
                        Vector3Dbl localPos = frame.InverseTransformPosition( this.GetAbsolutePosition() );
                        Vector3Dbl localVel = this.GetAbsoluteVelocity();
                        Vector3Dbl localAngVel = this.GetAbsoluteAngularVelocity();
                        Vector3 linAcc = (Vector3)frame.GetFicticiousAcceleration( localPos, localVel );
                        Vector3 angAcc = (Vector3)frame.GetFictitiousAngularAcceleration( localPos, localAngVel );

                        _rb.AddForce( linAcc, ForceMode.Acceleration );
                        _rb.AddTorque( angAcc, ForceMode.Acceleration );
                    }
                }
            }
        }

        public virtual void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            if( _isSceneSpace )
            {
                Vector3 scenePos = _rb.position;
                Vector3 sceneVel = _rb.velocity;
                var oldFrame = data.OldFrame;
                _state.Position = oldFrame.TransformPosition( scenePos );
                _state.Rotation = oldFrame.TransformRotation( _rb.rotation );
                _state.Velocity = oldFrame.TransformVelocity( sceneVel );
                _state.AngularVelocity = oldFrame.TransformAngularVelocity( _rb.angularVelocity );

                if( Mathf.Abs( scenePos.x ) > PositionRange || Mathf.Abs( scenePos.y ) > PositionRange || Mathf.Abs( scenePos.z ) > PositionRange
                 || Mathf.Abs( sceneVel.x ) > VelocityRange || Mathf.Abs( sceneVel.y ) > VelocityRange || Mathf.Abs( sceneVel.z ) > VelocityRange )
                {
                    SwitchToAbsoluteMode();
                }

                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( data.NewFrame, transform, _rb, _state.Position );
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( data.NewFrame, transform, _rb, _state.Rotation );
                ReferenceFrameTransformUtils.SetSceneVelocityFromAbsolute( data.NewFrame, _rb, _state.Velocity );
                ReferenceFrameTransformUtils.SetSceneAngularVelocityFromAbsolute( data.NewFrame, _rb, _state.AngularVelocity );
            }
            else
            {
                Vector3Dbl absolutePosition = _state.Position;
                Vector3 scenePos = (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( absolutePosition );
                _rb.position = scenePos;
                transform.position = scenePos;
                _state.Position = absolutePosition;

                QuaternionDbl absoluteRotation = _state.Rotation;
                Quaternion sceneRot = (Quaternion)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformRotation( absoluteRotation );
                _rb.rotation = sceneRot;
                transform.rotation = sceneRot;
                _state.Rotation = absoluteRotation;
            }
        }

        protected virtual void OnEnable()
        {
            _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
            _activeHybridTransforms.Add( this );
        }

        protected virtual void OnDisable()
        {
            _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
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


        static List<HybridReferenceFrameTransform> _activeHybridTransforms = new();

        [PlayerLoopSystem( typeof( UnityPlus.PlayerLoop.Phases.PostFixedUpdate ) )] // we update it here (after all fixed behaviour updates)
                                                                                    // because otherwise the execution order might fuck things,
                                                                                    // and I don't want to change the order manually.
        public sealed class HybridReferenceFrameTransformFixedUpdateSystem : IPlayerLoopSystem
        {
            public void Run()
            {
                foreach( var t in _activeHybridTransforms )
                {
                    if( !t._isSceneSpace )
                    {
                        IReferenceFrame sceneReferenceFrameAfterPhysicsProcessing = t.SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT );

                        // `_state.Position` should be up to date due to the callback inside physics step, which was invoked in the previous frame.

                        var vel = t._state.Velocity + t._state.Acceleration * TimeManager.FixedDeltaTime;
                        var angvel = t._state.AngularVelocity + t._state.AngularAcceleration * TimeManager.FixedDeltaTime;

                        t._requestedState.Position = t._state.Position + vel * TimeManager.FixedDeltaTime;
                        QuaternionDbl deltaRotation = QuaternionDbl.AngleAxis( angvel.magnitude * TimeManager.FixedDeltaTime * 57.29577951308232, angvel );
                        t._requestedState.Rotation = deltaRotation * t._state.Rotation;

                        var requestedPos = (Vector3)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformPosition( t._requestedState.Position );
                        var requestedRot = (Quaternion)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformRotation( t._requestedState.Rotation );

                        t._rb.Move( requestedPos, requestedRot );
                    }
                }
            }
        }

        [PlayerLoopSystem( typeof( UnityPlus.PlayerLoop.Phases.PhysicsStep ), Before = new[] { typeof( HSP.Trajectories.TrajectoryManager.TrajectoryManagerPostPhysicsStepSystem ) } )] // InsidePhysicsStep
        public sealed class HybridReferenceFrameTransformSystem : IPlayerLoopSystem
        {
            public void Run()
            {
                // This is required to happen indide physics step to properly account for all forces added during fixedupdate IF THE OBJECT IS IN ABSOLUTE MODE.
                // Some calls to AddForce might happen after FixedUpdate for this component has been called, and thus would only be accounted for in the next frame.
                //   This is unacceptable.

                // Assume that other objects aren't allowed to get the absolute position/velocity during physics step, as it is undefined (changes) during it.
                foreach( var t in _activeHybridTransforms )
                {
                    if( !t._isSceneSpace )
                    {
                        t._state.Velocity += t._state.Acceleration * TimeManager.FixedDeltaTime;
                        t._state.AngularVelocity += t._state.AngularAcceleration * TimeManager.FixedDeltaTime;
                    }

                    t._state.Acceleration = Vector3Dbl.zero;
                    t._state.AngularAcceleration = Vector3Dbl.zero;

                    t._state.Position = t._requestedState.Position;
                    t._state.Rotation = t._requestedState.Rotation;
                }
            }
        }

        [MapsInheritingFrom( typeof( HybridReferenceFrameTransform ) )]
        public static IDescriptor HybridReferenceFrameTransformMapping()
        {
            return new MemberwiseDescriptor<HybridReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "allow_scene_simulation", o => o.AllowSceneSimulation )
                .WithMember( "position_range", o => o.PositionRange )
                .WithMember( "velocity_range", o => o.VelocityRange )
                .WithMember( "max_timescale", o => o.MaxTimeScale )

                .WithMember( "mass", o => o.Mass )
                .WithMember( "local_center_of_mass", o => o.LocalCenterOfMass )

                .WithMember( "absolute_position", o => o.GetAbsolutePosition(), ( o, v ) => o.SetAbsolutePosition( v ) )
                .WithMember( "absolute_rotation", o => o.GetAbsoluteRotation(), ( o, v ) => o.SetAbsoluteRotation( v ) )
                .WithMember( "absolute_velocity", o => o.GetAbsoluteVelocity(), ( o, v ) => o.SetAbsoluteVelocity( v ) )
                .WithMember( "absolute_angular_velocity", o => o.GetAbsoluteAngularVelocity(), ( o, v ) => o.SetAbsoluteAngularVelocity( v ) );
        }
    }
}