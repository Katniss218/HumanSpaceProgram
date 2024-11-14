using HSP.ReferenceFrames;
using HSP.Time;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityPlus;

namespace HSP.Vanilla
{
    [RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class HybridReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        private bool _collisionResponseEnabled = false;
        /// <summary>
        /// If true, the object is allowed to simulate using scene space, allowing for collisions, when the position and velocity are within the range allowed for scene space simulation.
        /// </summary>
        public bool AllowCollisionResponse // a.k.a. allow simulation
        {
            get => _collisionResponseEnabled;
            set
            {
                _collisionResponseEnabled = value;
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
        /// The allowed values for scene position where the object can have collision response, in [m].
        /// </summary>
        public float PositionRange { get; set; }
        /// <summary>
        /// The allowed values for scene velocity where the object can have collision response, in [m/s].
        /// </summary>
        public float VelocityRange { get; set; }
        /// <summary>
        /// The maximum allowed timescale where the object can have collision response.
        /// </summary>
        public float MaxTimeScale { get; set; }

        Rigidbody _rb;

        // absolute mode variables

        Vector3Dbl _requestedAbsolutePosition;
        Vector3Dbl _actualAbsolutePosition;
        QuaternionDbl _requestedAbsoluteRotation = QuaternionDbl.identity;
        QuaternionDbl _actualAbsoluteRotation = QuaternionDbl.identity;

        Vector3Dbl _absoluteAcceleration;
        Vector3Dbl _absoluteAngularAcceleration;

        Vector3Dbl _absoluteVelocity;
        Vector3Dbl _absoluteAngularVelocity;

        //

        private bool _isSceneSpace;

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
                var absoluteVel = SceneReferenceFrameManager.ReferenceFrame.InverseTransformVelocity( value );
                _absoluteVelocity = absoluteVel;

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
                var absoluteAngVel = SceneReferenceFrameManager.ReferenceFrame.InverseTransformAngularVelocity( value );
                _absoluteAngularVelocity = absoluteAngVel;

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
#warning TODO - worldCoM is not accurate enough for absolute mode.
#warning TODO - force and position are not accurate enough either.
            Vector3 leverArm = position - this._rb.worldCenterOfMass;
            _absoluteAcceleration += SceneReferenceFrameManager.ReferenceFrame.TransformAcceleration( (Vector3Dbl)force / Mass );
            _absoluteAngularAcceleration += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( Vector3Dbl.Cross( force, leverArm ) / Mass );

            // T = torque??
            // Quaternion q = transform.rotation * rigidbody.inertiaTensorRotation; // q is rotation of inertia tensor in world space
            // T = q * Vector3.Scale(rigidbody.inertiaTensor, (Quaternion.Inverse(q) * w));

#warning TODO - take into account the moment of inertia?
            if( _isSceneSpace )
            {
                this._rb.AddForceAtPosition( force, position, ForceMode.Force );
            }
        }

        public void AddTorque( Vector3 torque )
        {
            _absoluteAngularAcceleration += SceneReferenceFrameManager.ReferenceFrame.TransformAngularAcceleration( (Vector3Dbl)torque / Mass );

            if( _isSceneSpace )
            {
                this._rb.AddTorque( torque, ForceMode.Force );
            }
        }

        private void SwitchToAbsoluteMode()
        {
            IReferenceFrame sceneReferenceFrame = SceneReferenceFrameManager.ReferenceFrame;

            _absoluteVelocity = sceneReferenceFrame.TransformVelocity( _rb.velocity );
            _actualAbsolutePosition = sceneReferenceFrame.TransformVelocity( _rb.position );
            _requestedAbsolutePosition = _actualAbsolutePosition + (_absoluteVelocity * TimeManager.FixedDeltaTime);

            _absoluteAngularVelocity = sceneReferenceFrame.TransformAngularVelocity( _rb.angularVelocity );
            _actualAbsoluteRotation = sceneReferenceFrame.TransformRotation( _rb.rotation );
            QuaternionDbl deltaRotation = QuaternionDbl.Euler( _absoluteAngularVelocity * TimeManager.FixedDeltaTime * 57.2957795131 );
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

            //_rb.Move( requestedPos, requestedRot ); // sets the ground truth values.
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

            _rb = this.GetComponent<Rigidbody>();

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Continuous (in any of its flavors) "jumps" when sitting on top of something when reference frame switches.
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = !_isSceneSpace;
        }

        void FixedUpdate()
        {
            // Only try toggling in and out of scene mode when collisions are desired.
            if( _collisionResponseEnabled )
            {
                if( _isSceneSpace )
                {
                    Vector3 scenePos = _rb.position;
                    Vector3 sceneVel = _rb.velocity;

                    // Ensure that the condition switch to one doesn't have overlap with the condition to switch back, as this would be silly.
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

                    // Ensure that the condition switch to one doesn't have overlap with the condition to switch back, as this would be silly.
                    if( Mathf.Abs( scenePos.x ) <= PositionRange && Mathf.Abs( scenePos.y ) <= PositionRange && Mathf.Abs( scenePos.z ) <= PositionRange
                     && Mathf.Abs( sceneVel.x ) <= PositionRange && Mathf.Abs( sceneVel.y ) <= PositionRange && Mathf.Abs( sceneVel.z ) <= PositionRange
                     && TimeManager.TimeScale <= MaxTimeScale )
                    {
                        SwitchToSceneMode();
                    }
                }
            }

            if( !_isSceneSpace )
            {
                //
                // Simulate absolute space \/
                IReferenceFrame sceneReferenceFrameAfterPhysicsProcessing = SceneReferenceFrameManager.ReferenceFrame.AtUT( TimeManager.UT );

                // `_actualAbsolutePosition` should be up to date due to the callback inside physics step, which was invoked in the previous frame.

                _requestedAbsolutePosition = _actualAbsolutePosition + _absoluteVelocity * TimeManager.FixedDeltaTime;
                QuaternionDbl deltaRotation = QuaternionDbl.Euler( _absoluteAngularVelocity * TimeManager.FixedDeltaTime * 57.2957795131 );
                _requestedAbsoluteRotation = deltaRotation * _actualAbsoluteRotation;

                var requestedPos = (Vector3)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformPosition( _requestedAbsolutePosition );
                var requestedRot = (Quaternion)sceneReferenceFrameAfterPhysicsProcessing.InverseTransformRotation( _requestedAbsoluteRotation );

                _rb.Move( requestedPos, requestedRot );
            }

            //
            // Rigidbody simulates itself during scene mode.
        }

        public virtual void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
#warning TODO - switch immediately to absolute if needed to prevent loss of precision.

            if( _isSceneSpace )
            {
                var sceneReferenceFrame = data.OldFrame;
                _actualAbsolutePosition = sceneReferenceFrame.TransformPosition( _rb.position );
                _actualAbsoluteRotation = sceneReferenceFrame.TransformRotation( _rb.rotation );
                _absoluteVelocity = sceneReferenceFrame.TransformVelocity( _rb.velocity );
                _absoluteAngularVelocity = sceneReferenceFrame.TransformAngularVelocity( _rb.angularVelocity );

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
                //_actualPosition = scenePos;
                _actualAbsolutePosition = absolutePosition;
                _requestedAbsolutePosition = absolutePosition;

                QuaternionDbl absoluteRotation = this.AbsoluteRotation;
                Quaternion sceneRot = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( absoluteRotation );
                _rb.rotation = sceneRot;
                transform.rotation = sceneRot;
                //_actualRotation = sceneRot;
                _actualAbsoluteRotation = absoluteRotation;
                _requestedAbsoluteRotation = absoluteRotation;
            }
        }

        void OnEnable()
        {
            _transforms.Add( this );
        }

        void OnDisable()
        {
            _transforms.Remove( this );
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

        // Imo it's kind of ugly using HSPEvent_STARTUP_IMMEDIATELY to mess with player loop, but it is what it is.
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, "a" )]
        static void A()
        {
            PlayerLoopUtils.AddSystem<FixedUpdate, FixedUpdate.PhysicsFixedUpdate>( in _playerLoopSystem );
        }

        private static PlayerLoopSystem _playerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( SceneReferenceFrameManager ),
            updateDelegate = InsidePhysicsStep,
            subSystemList = null
        };

        static HashSet<HybridReferenceFrameTransform> _transforms = new();

        static void InsidePhysicsStep()
        {
            // This is required to happen indide physics step to properly account for all forces added during fixedupdate IF THE OBJECT IS IN ABSOLUTE MODE.
            // Some calls to AddForce might happen after FixedUpdate for this component has been called, and thus would only be accounted for in the next frame.
            //   This is unacceptable.

            // Assume that other objects aren't allowed to get the absolute position/velocity during physics step, as it is undefined (changes) during it.
            foreach( var t in _transforms )
            {
                if( !t._isSceneSpace )
                {
                    t._absoluteVelocity += t._absoluteAcceleration * TimeManager.FixedDeltaTime;
                    t._absoluteAngularVelocity += t._absoluteAngularAcceleration * TimeManager.FixedDeltaTime;
                }

                t._absoluteAcceleration = Vector3Dbl.zero;
                t._absoluteAngularAcceleration = Vector3Dbl.zero;

                t._actualAbsolutePosition = t._requestedAbsolutePosition;
            }
        }
    }
}