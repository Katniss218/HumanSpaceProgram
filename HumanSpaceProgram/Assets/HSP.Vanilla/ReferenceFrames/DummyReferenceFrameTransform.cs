using HSP.ReferenceFrames;
using System;
using UnityEngine;

namespace HSP.Vanilla.ReferenceFrames
{
    /// <summary>
    /// A reference frame transform that does nothing and calculates itself using the underlying UnityEngine transform.
    /// </summary>
    public sealed class DummyReferenceFrameTransform : MonoBehaviour, IReferenceFrameTransform
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

        public Vector3 Position
        {
            get => transform.position; set
            {
                transform.position = value;
            }
        }

        public Vector3Dbl AbsolutePosition
        {
            get => SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformPosition( transform.position );
            set
            {
                transform.position = (Vector3)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformPosition( value );
            }
        }

        public Quaternion Rotation
        {
            get => transform.rotation; set
            {
                transform.rotation = value;
            }
        }

        public QuaternionDbl AbsoluteRotation
        {
            get => SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformRotation( transform.rotation );
            set
            {
                transform.rotation = (Quaternion)SceneReferenceFrameProvider.GetSceneReferenceFrame().InverseTransformRotation( value );
            }
        }

        public Vector3 Velocity { get => Vector3.zero; set { } }

        public Vector3Dbl AbsoluteVelocity { get => SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformVelocity( Vector3.zero ); set { } }

        public Vector3 AngularVelocity { get => Vector3.zero; set { } }

        public Vector3Dbl AbsoluteAngularVelocity { get => SceneReferenceFrameProvider.GetSceneReferenceFrame().TransformAngularVelocity( Vector3.zero ); set { } }

        public Vector3 Acceleration => throw new NotImplementedException();

        public Vector3Dbl AbsoluteAcceleration => throw new NotImplementedException();

        public Vector3 AngularAcceleration => throw new NotImplementedException();

        public Vector3Dbl AbsoluteAngularAcceleration => throw new NotImplementedException();

        public event Action OnAbsolutePositionChanged;
        public event Action OnAbsoluteRotationChanged;
        public event Action OnAbsoluteVelocityChanged;
        public event Action OnAbsoluteAngularVelocityChanged;
        public event Action OnAnyValueChanged;

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            var oldAbsPos = data.OldFrame.TransformPosition( transform.position );
            var oldAbsRot = data.OldFrame.TransformRotation( transform.rotation );

            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( data.NewFrame, transform, null, oldAbsPos );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( data.NewFrame, transform, null, oldAbsRot );
        }

        void OnEnable()
        {
            _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
        }

        void OnDisable()
        {
            _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
        }
    }
}