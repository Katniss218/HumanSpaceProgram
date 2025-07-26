using HSP.ReferenceFrames;
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
        public ISceneReferenceFrameProvider SceneReferenceFrameProvider { get; set; }

        public IReferenceFrameTransform TargetTransform { get; set; }

        public Vector3 Position
        {
            get
            {
                return (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformPosition( TargetTransform.AbsolutePosition );
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof(Position)} of {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
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
        public Quaternion Rotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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
        public Vector3 Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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
        public Vector3 AngularVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
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

        public bool IsColliding => throw new NotImplementedException();

        public event Action OnAbsolutePositionChanged;
        public event Action OnAbsoluteRotationChanged;
        public event Action OnAbsoluteVelocityChanged;
        public event Action OnAbsoluteAngularVelocityChanged;
        public event Action OnAnyValueChanged;


        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            throw new NotImplementedException();
        }

        void FixedUpdate()
        {
            var position = (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformPosition( TargetTransform.AbsolutePosition );
            var rotation = (Quaternion)SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformRotation( TargetTransform.AbsoluteRotation );
            this.transform.SetPositionAndRotation( position, rotation );
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