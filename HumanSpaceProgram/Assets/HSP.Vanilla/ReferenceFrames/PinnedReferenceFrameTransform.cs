﻿using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Time;
using System;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    /// <remarks>
    /// A physics transform that is pinned to a fixed pos/rot in the local coordinate system of a celestial body.
    /// </remarks>
	[RequireComponent( typeof( Rigidbody ) )]
    [DisallowMultipleComponent]
    public class PinnedReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
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
                    MakeCacheInvalid();
                    ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
                    ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
                    SetPositionAndRotation();
                    OnAbsolutePositionChanged?.Invoke();
                    OnAbsoluteRotationChanged?.Invoke();
                    OnAnyValueChanged?.Invoke();
                }
            }
        }

        public Vector3Dbl ReferencePosition
        {
            get => _referencePosition;
            set
            {
                _referencePosition = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
                SetPositionAndRotation();
                OnAbsolutePositionChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public QuaternionDbl ReferenceRotation
        {
            get => _referenceRotation;
            set
            {
                _referenceRotation = value;
                MakeCacheInvalid();
                ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
                SetPositionAndRotation();
                OnAbsoluteRotationChanged?.Invoke();
                OnAnyValueChanged?.Invoke();
            }
        }

        public void SetReference( CelestialBody referenceBody, Vector3Dbl referencePosition, QuaternionDbl referenceRotation )
        {
            _referenceBody = referenceBody;
            _referencePosition = referencePosition;
            _referenceRotation = referenceRotation;
            MakeCacheInvalid();
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( transform, _rb, AbsolutePosition );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( transform, _rb, AbsoluteRotation );
            OnAbsolutePositionChanged?.Invoke();
            OnAbsoluteRotationChanged?.Invoke();
            OnAnyValueChanged?.Invoke();
        }


        public Vector3 Position
        {
            get
            {
                return _rb.position; // rb.position should be correct, since it's updated during unityphysics step
            }
            set
            {
                Vector3Dbl absolutePosition = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( value );
                ReferencePosition = _referenceBody.ReferenceFrameTransform.OrientedReferenceFrame().InverseTransformPosition( absolutePosition );
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAbsolutePosition;
            }
            set
            {
                ReferencePosition = _referenceBody.ReferenceFrameTransform.OrientedReferenceFrame().InverseTransformPosition( value );
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return _rb.rotation; // rb.rotation should be correct, since it's updated during unityphysics step
            }
            set
            {
                QuaternionDbl absoluteRotation = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( value );
                ReferenceRotation = _referenceBody.ReferenceFrameTransform.OrientedReferenceFrame().InverseTransformRotation( absoluteRotation );
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAbsoluteRotation;
            }
            set
            {
                ReferenceRotation = _referenceBody.ReferenceFrameTransform.OrientedReferenceFrame().InverseTransformRotation( value );
            }
        }


        public Vector3 Velocity
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedVelocity;
            }
            set { }
        }

        public Vector3Dbl AbsoluteVelocity
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAbsoluteVelocity;
            }
            set { }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAngularVelocity;
            }
            set { }
        }

        public Vector3Dbl AbsoluteAngularVelocity
        {
            get
            {
                RecalculateCacheIfNeeded();
                return _cachedAbsoluteAngularVelocity;
            }
            set { }
        }

        public Vector3 Acceleration => _cachedAcceleration;
        public Vector3Dbl AbsoluteAcceleration => _cachedAbsoluteAcceleration;
        public Vector3 AngularAcceleration => _cachedAngularAcceleration;
        public Vector3Dbl AbsoluteAngularAcceleration => _cachedAbsoluteAngularAcceleration;

        /// <summary> The scene frame in which the cached values are expressed. </summary>
        IReferenceFrame _cachedSceneReferenceFrame;
        IReferenceFrame _cachedBodyReferenceFrame;
        Vector3Dbl _cachedAbsolutePosition;
        QuaternionDbl _cachedAbsoluteRotation = QuaternionDbl.identity;
        Vector3 _cachedVelocity;
        Vector3Dbl _cachedAbsoluteVelocity;
        Vector3 _cachedAngularVelocity;
        Vector3Dbl _cachedAbsoluteAngularVelocity;
        Vector3 _cachedAcceleration;
        Vector3Dbl _cachedAbsoluteAcceleration;
        Vector3 _cachedAngularAcceleration;
        Vector3Dbl _cachedAbsoluteAngularAcceleration;

        Vector3Dbl _absoluteAccelerationSum = Vector3.zero;
        Vector3Dbl _absoluteAngularAccelerationSum = Vector3.zero;

        public event Action OnAbsolutePositionChanged;
        public event Action OnAbsoluteRotationChanged;
        public event Action OnAbsoluteVelocityChanged;
        public event Action OnAbsoluteAngularVelocityChanged;
        public event Action OnAnyValueChanged;

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
                if (___rb == null)
                    ___rb = this.GetComponent<Rigidbody>();
                return ___rb;
            }
        }

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

        private void SetPositionAndRotation()
        {
            if( ReferenceBody == null )
                return;

            IReferenceFrame sceneReferenceFrame = SceneReferenceFrameManager.ReferenceFrame;
            IReferenceFrame bodyFrame = ReferenceBody.ReferenceFrameTransform.OrientedInertialReferenceFrame();
            Vector3 pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( bodyFrame.TransformPosition( _referencePosition ) );
            Quaternion rot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( bodyFrame.TransformRotation( _referenceRotation ) );

            _rb.position = pos;
            transform.position = pos;

            _rb.rotation = rot;
            transform.rotation = rot;
        }

        private void RecalculateCacheIfNeeded()
        {
            if( IsCacheValid() )
                return;

            RecalculateCache( SceneReferenceFrameManager.ReferenceFrame );
        }

        private void RecalculateCache( IReferenceFrame sceneReferenceFrame )
        {
            if( _referenceBody == null )
                return;

            INonInertialReferenceFrame bodyFrame = _referenceBody.ReferenceFrameTransform.NonInertialReferenceFrame();
            _cachedAbsolutePosition = bodyFrame.TransformPosition( _referencePosition );
            _cachedAbsoluteRotation = bodyFrame.TransformRotation( _referenceRotation );
            _cachedAbsoluteVelocity = bodyFrame.TransformVelocity( Vector3Dbl.zero );
            _cachedAbsoluteAngularVelocity = bodyFrame.TransformAngularVelocity( Vector3Dbl.zero );

            _cachedAbsoluteVelocity += bodyFrame.GetTangentialVelocity( _referencePosition );

            _cachedVelocity = (Vector3)sceneReferenceFrame.InverseTransformVelocity( _cachedAbsoluteVelocity );
            _cachedAngularVelocity = (Vector3)sceneReferenceFrame.InverseTransformAngularVelocity( _cachedAbsoluteVelocity );

            _cachedAbsoluteAcceleration = bodyFrame.TransformAcceleration( Vector3Dbl.zero );
            _cachedAbsoluteAngularAcceleration = bodyFrame.TransformAngularAcceleration( Vector3Dbl.zero );
            _cachedAcceleration = (Vector3)sceneReferenceFrame.InverseTransformAcceleration( _cachedAbsoluteAcceleration );
            _cachedAngularAcceleration = (Vector3)sceneReferenceFrame.InverseTransformAngularAcceleration( _cachedAbsoluteAngularAcceleration );
            _cachedSceneReferenceFrame = sceneReferenceFrame;
            _cachedBodyReferenceFrame = bodyFrame;
        }

        // Exact comparison of the axes catches the most cases (and it's gonna be set to match exactly so it's okay)
        // Vector3's `==` operator does approximate comparison.
        private bool IsCacheValid() => false;// SceneReferenceFrameManager.ReferenceFrame.EqualsIgnoreUT( _cachedSceneReferenceFrame )
                                             // && _referenceBody.OrientedInertialReferenceFrame.EqualsIgnoreUT( _cachedBodyReferenceFrame );

        //private void MakeCacheValid() => ; cache validates itself when the frames are set

        private void MakeCacheInvalid() => _cachedBodyReferenceFrame = null;

        protected virtual void Awake()
        {
            if( this.HasComponentOtherThan<IReferenceFrameTransform>( this ) )
            {
                Debug.LogWarning( $"Tried to add a {nameof( PinnedReferenceFrameTransform )} to a game object that already has a {nameof( IReferenceFrameTransform )}. This is not allowed. Remove the previous physics object first." );
                Destroy( this );
                return;
            }

            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rb.interpolation = RigidbodyInterpolation.None; // DO NOT INTERPOLATE. Doing so will desync `rigidbody.position` and `transform.position`.
            _rb.isKinematic = true;
        }

        protected virtual void FixedUpdate()
        {
            if( ReferenceBody == null )
                return;

            // ReferenceFrame.AtUT is used because we want to access the frame for the end of the frame, and FixedUpdate (caller) is called before ReferenceFrame updates.
            var sceneReferenceFrame = SceneReferenceFrameManager.ReferenceFrame.AtUT( TimeManager.UT );
            var bodyFrame = ReferenceBody.ReferenceFrameTransform.NonInertialReferenceFrame().AtUT( TimeManager.UT );

            Vector3 pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( bodyFrame.TransformPosition( _referencePosition ) );
            Quaternion rot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( bodyFrame.TransformRotation( _referenceRotation ) );
            _rb.Move( pos, rot );
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            // `_referenceBody.OrientedReferenceFrame` Guarantees up-to-date reference frame, regardless of update order.

            SetPositionAndRotation();
        }

        protected virtual void OnEnable()
        {
            _rb.isKinematic = true; // Force kinematic.
        }

        protected virtual void OnDisable()
        {
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

        [MapsInheritingFrom( typeof( PinnedReferenceFrameTransform ) )]
        public static SerializationMapping PinnedPhysicsObjectMapping()
        {
            return new MemberwiseSerializationMapping<PinnedReferenceFrameTransform>()
                .WithMember( "mass", o => o.Mass )
                .WithMember( "local_center_of_mass", o => o.LocalCenterOfMass )

                .WithMember( "DO_NOT_TOUCH", o => true, ( o, value ) => o._rb.isKinematic = true )

                .WithMember( "reference_body", o => o.ReferenceBody == null ? null : o.ReferenceBody.ID, ( o, value ) => o.ReferenceBody = value == null ? null : CelestialBodyManager.Get( value ) )
                .WithMember( "reference_position", o => o.ReferencePosition )
                .WithMember( "reference_rotation", o => o.ReferenceRotation );
        }
    }
}