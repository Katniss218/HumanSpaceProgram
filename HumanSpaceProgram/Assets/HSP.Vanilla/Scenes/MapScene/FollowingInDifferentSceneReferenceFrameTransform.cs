using HSP.ReferenceFrames;
using System;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    /// <summary>
    /// A reference frame transform that follows a different reference frame transform, using a different scene reference frame.
    /// </summary>
    public class FollowingInDifferentSceneReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform, IPhysicsTransform
    {
        public ISceneReferenceFrameProvider SceneReferenceFrameProvider { get; set; }

        public IReferenceFrameTransform UnderlyingReferenceFrameTransform { get; set; }
        public IPhysicsTransform UnderlyingPhysicsTransform { get; set; }

        public Vector3 Position
        {
            get
            {
                return (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformPosition( UnderlyingReferenceFrameTransform.AbsolutePosition );
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof(Position)} of {nameof( FollowingInDifferentSceneReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Vector3Dbl AbsolutePosition
        {
            get
            {
                return UnderlyingReferenceFrameTransform.AbsolutePosition;
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( AbsolutePosition )} of {nameof( FollowingInDifferentSceneReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Quaternion Rotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public QuaternionDbl AbsoluteRotation
        {
            get
            {
                return UnderlyingReferenceFrameTransform.AbsoluteRotation;
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( AbsoluteRotation )} of {nameof( FollowingInDifferentSceneReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Vector3 Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector3Dbl AbsoluteVelocity
        {
            get
            {
                return UnderlyingReferenceFrameTransform.AbsoluteVelocity;
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( AbsoluteVelocity )} of {nameof( FollowingInDifferentSceneReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }
        public Vector3 AngularVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector3Dbl AbsoluteAngularVelocity
        {
            get
            {
                return UnderlyingReferenceFrameTransform.AbsoluteAngularVelocity;
            }
            set
            {
                throw new InvalidOperationException( $"Can't set {nameof( AbsoluteAngularVelocity )} of {nameof( FollowingInDifferentSceneReferenceFrameTransform )}. This transform always follows the reference object in another scene." );
            }
        }

        public Vector3 Acceleration => throw new NotImplementedException();

        public Vector3Dbl AbsoluteAcceleration => UnderlyingReferenceFrameTransform.AbsoluteAcceleration;

        public Vector3 AngularAcceleration => throw new NotImplementedException();

        public Vector3Dbl AbsoluteAngularAcceleration => UnderlyingReferenceFrameTransform.AbsoluteAngularAcceleration;

        public float Mass { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector3 LocalCenterOfMass { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector3 MomentsOfInertia { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Quaternion MomentsOfInertiaRotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsColliding => throw new NotImplementedException();

        public event Action OnAbsolutePositionChanged;
        public event Action OnAbsoluteRotationChanged;
        public event Action OnAbsoluteVelocityChanged;
        public event Action OnAbsoluteAngularVelocityChanged;
        public event Action OnAnyValueChanged;

        public void AddForce( Vector3 force )
        {
            throw new NotImplementedException();
        }

        public void AddForceAtPosition( Vector3 force, Vector3 position )
        {
            throw new NotImplementedException();
        }

        public void AddTorque( Vector3 torque )
        {
            throw new NotImplementedException();
        }

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            throw new NotImplementedException();
        }

        void FixedUpdate()
        {
            var position = (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformPosition( UnderlyingReferenceFrameTransform.AbsolutePosition );
            var rotation = (Quaternion)SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformRotation( UnderlyingReferenceFrameTransform.AbsoluteRotation );
            this.transform.SetPositionAndRotation( position, rotation );
        }
    }
}