using HSP.ReferenceFrames;
using HSP.Time;
using System;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vanilla.ReferenceFrames
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

        public void SetState( in KinematicState state ) => throw new InvalidOperationException( $"Can't modify {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );

        public void ModifyState( IReferenceFrame requestedFrame, KinematicStateMutator mutator ) => throw new InvalidOperationException( $"Can't modify {nameof( FollowingDifferentReferenceFrameTransform )}. This transform always follows the reference object in another scene." );

        protected void RecalculateCacheIfNeeded()
        {
            if( IsCacheValid() )
                return;

            RecalculateCache( SceneReferenceFrameProvider.GetSceneReferenceFrame() );
            MakeCacheValid();
        }

        protected void RecalculateCache( IReferenceFrame sceneFrame )
        {
            _cachedState = TargetTransform.GetState( null );
            _cachedSceneReferenceFrame = sceneFrame;
        }

        protected bool IsCacheValid() => _cachedSceneReferenceFrame != null
            && TimeManager.UT == _cachedAtUT
            && SceneReferenceFrameProvider.GetSceneReferenceFrame().EqualsIgnoreUT( _cachedSceneReferenceFrame );

        protected void MakeCacheValid()
        {
            _cachedAtUT = TimeManager.UT;
        }

        protected void MakeCacheInvalid() => _cachedAtUT = -1;

        public void OnSceneReferenceFrameSwitch( SceneReferenceFrameManager.ReferenceFrameSwitchData data )
        {
            var position = (Vector3)data.NewFrame.InverseTransformPosition( GetState( null ).Position );
            var rotation = (Quaternion)data.NewFrame.InverseTransformRotation( GetState( null ).Rotation );
            this.transform.SetPositionAndRotation( position, rotation );
        }

        void FixedUpdate()
        {
            if( TargetTransform == null )
                return;

            IReferenceFrame sceneReferenceFrame = SceneReferenceFrameProvider.GetSceneReferenceFrame().AtUT( TimeManager.UT );
            Vector3 pos = (Vector3)sceneReferenceFrame.InverseTransformPosition( TargetTransform.GetAbsolutePosition() );
            Quaternion rot = (Quaternion)sceneReferenceFrame.InverseTransformRotation( TargetTransform.GetAbsoluteRotation() );

            transform.position = pos;
            transform.rotation = rot;
        }

        public event Action OnStateChanged;

        void OnEnable()
        {
            _sceneReferenceFrameProvider?.SubscribeIfNotSubscribed( this );
        }

        void OnDisable()
        {
            _sceneReferenceFrameProvider?.UnsubscribeIfSubscribed( this );
        }

        [MapsInheritingFrom( typeof( FollowingDifferentReferenceFrameTransform ) )]
        public static IDescriptor FollowingDifferentReferenceFrameTransformMapping()
        {
            return new MemberwiseDescriptor<FollowingDifferentReferenceFrameTransform>()
                .WithMember( "scene_reference_frame_provider", o => o.SceneReferenceFrameProvider )
                .WithMember( "target_transform", typeof( Ctx.Ref ), o => o.TargetTransform );
        }
    }
}