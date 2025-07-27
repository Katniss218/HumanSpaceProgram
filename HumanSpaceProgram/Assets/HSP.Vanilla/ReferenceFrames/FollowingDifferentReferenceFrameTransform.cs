using HSP.ReferenceFrames;
using HSP.Time;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Scenes.MapScene
{
    /// <summary>
    /// A reference frame transform that follows some other reference frame transform, potentially also using a different scene reference frame.
    /// </summary>
    public class FollowingDifferentReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform
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

        public IReferenceFrameTransform TargetTransform { get; set; }

        public Vector3 Position
        {
            get
            {
                return (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( TargetTransform.AbsolutePosition );
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( Position )} of {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Vector3Dbl AbsolutePosition
        {
            get
            {
                return TargetTransform.AbsolutePosition;
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( AbsolutePosition )} of {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return (Quaternion)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformRotation( TargetTransform.AbsoluteRotation );
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( Rotation )} of {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public QuaternionDbl AbsoluteRotation
        {
            get
            {
                return TargetTransform.AbsoluteRotation;
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( AbsoluteRotation )} of {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Vector3 Velocity
        {
            get
            {
                return (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformVelocity( TargetTransform.AbsoluteVelocity );
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( Velocity )} of {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Vector3Dbl AbsoluteVelocity
        {
            get
            {
                return TargetTransform.AbsoluteVelocity;
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( AbsoluteVelocity )} of {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Vector3 AngularVelocity
        {
            get
            {
                return (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformAngularVelocity( TargetTransform.AbsoluteAngularVelocity );
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( AngularVelocity )} of {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Vector3Dbl AbsoluteAngularVelocity
        {
            get
            {
                return TargetTransform.AbsoluteAngularVelocity;
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( AbsoluteAngularVelocity )} of {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }

        public Vector3 Acceleration => throw new NotImplementedException();

        public Vector3Dbl AbsoluteAcceleration => TargetTransform.AbsoluteAcceleration;

        public Vector3 AngularAcceleration => throw new NotImplementedException();

        public Vector3Dbl AbsoluteAngularAcceleration => TargetTransform.AbsoluteAngularAcceleration;

        public event Action OnAbsolutePositionChanged;
        public event Action OnAbsoluteRotationChanged;
        public event Action OnAbsoluteVelocityChanged;
        public event Action OnAbsoluteAngularVelocityChanged;
        public event Action OnAnyValueChanged;


        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            var position = (Vector3)data.NewFrame.InverseTransformPosition( TargetTransform.AbsolutePosition );
            var rotation = (Quaternion)data.NewFrame.InverseTransformRotation( TargetTransform.AbsoluteRotation );
            this.transform.SetPositionAndRotation( position, rotation );
        }

        void FixedUpdate()
        {
            var position = (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT ).InverseTransformPosition( TargetTransform.AbsolutePosition );
            var rotation = (Quaternion)SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT ).InverseTransformRotation( TargetTransform.AbsoluteRotation );
            this.transform.SetPositionAndRotation( position, rotation );
        }

        void OnEnable()
        {
            _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
        }

        void OnDisable()
        {
            _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
        }

        [MapsInheritingFrom( typeof( FollowingDifferentReferenceFrameTransform ) )]
        public static SerializationMapping FollowingDifferentReferenceFrameTransformMapping()
        {
            return new MemberwiseSerializationMapping<FollowingDifferentReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "target_transform", o => o.TargetTransform );
        }
    }
}