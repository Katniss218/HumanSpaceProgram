using HSP.ReferenceFrames;
using System;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

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

        private KinematicState _cachedState;
        private IReferenceFrame _cachedSceneReferenceFrame = null;
        private double _cachedAtUT = -1;

        public KinematicState GetState( IReferenceFrame requestedFrame )
        {
            RecalculateCacheIfNeeded();
            return _cachedState.InFrame( requestedFrame );
        }

        public ref readonly KinematicState GetStateRef( out IReferenceFrame referenceFrame )
        {
            RecalculateCacheIfNeeded();
            referenceFrame = null;
            return ref _cachedState;
        }

        public void SetState( in KinematicState state )
        {
            var absoluteState = state.InFrame( null );
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, null, absoluteState.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, null, absoluteState.Rotation );
            MakeCacheInvalid();
            OnStateChanged?.Invoke();
        }

        public void ModifyState( IReferenceFrame requestedFrame, KinematicStateMutator mutator )
        {
            RecalculateCacheIfNeeded();
            if( requestedFrame == null )
            {
                mutator( ref _cachedState );
            }
            else
            {
                var localState = _cachedState.InFrame( requestedFrame );
                mutator( ref localState );
                _cachedState = localState.InFrame( null );
            }
            ReferenceFrameTransformUtils.SetScenePositionFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, null, _cachedState.Position );
            ReferenceFrameTransformUtils.SetSceneRotationFromAbsolute( SceneReferenceFrameProvider.GetSceneReferenceFrame(), transform, null, _cachedState.Rotation );
            MakeCacheInvalid();
            OnStateChanged?.Invoke();
        }

        protected void RecalculateCacheIfNeeded()
        {
            if( IsCacheValid() )
                return;

            RecalculateCache( SceneReferenceFrameProvider.GetSceneReferenceFrame() );
            MakeCacheValid();
        }

        protected void RecalculateCache( IReferenceFrame sceneFrame )
        {
            _cachedState = new KinematicState(
                null,
                sceneFrame.TransformPosition( transform.position ),
                sceneFrame.TransformRotation( transform.rotation ),
                sceneFrame.TransformVelocity( Vector3.zero ),
                sceneFrame.TransformAngularVelocity( Vector3.zero ),
                Vector3Dbl.zero,
                Vector3Dbl.zero
            );
            _cachedSceneReferenceFrame = sceneFrame;
        }

        protected bool IsCacheValid() => _cachedSceneReferenceFrame != null
            && HSP.Time.TimeManager.UT == _cachedAtUT
            && SceneReferenceFrameProvider.GetSceneReferenceFrame().EqualsIgnoreUT( _cachedSceneReferenceFrame );

        protected void MakeCacheValid()
        {
            _cachedAtUT = HSP.Time.TimeManager.UT;
        }

        protected void MakeCacheInvalid() => _cachedAtUT = -1;

        public event Action OnStateChanged;

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

        [MapsInheritingFrom( typeof( DummyReferenceFrameTransform ) )]
        public static IDescriptor DummyReferenceFrameTransformMapping()
        {
            return new MemberwiseDescriptor<DummyReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "absolute_position", o => o.GetAbsolutePosition(), ( o, v ) => o.SetAbsolutePosition( v ) )
                .WithMember( "absolute_rotation", o => o.GetAbsoluteRotation(), ( o, v ) => o.SetAbsoluteRotation( v ) )
                .WithMember( "absolute_velocity", o => o.GetAbsoluteVelocity(), ( o, v ) => o.SetAbsoluteVelocity( v ) )
                .WithMember( "absolute_angular_velocity", o => o.GetAbsoluteAngularVelocity(), ( o, v ) => o.SetAbsoluteAngularVelocity( v ) );
        }
    }
}