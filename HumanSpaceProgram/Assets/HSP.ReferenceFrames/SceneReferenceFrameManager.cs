using HSP.Time;
using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityPlus;

namespace HSP.ReferenceFrames
{
    /// <summary>
    /// Manages the reference frame used by the unity scene.
    /// Ensures that the <see cref="TargetObject"/> remains close to the origin of the scene, and that it doesn't move too fast relative to the scene.
    /// </summary>
    /// <remarks>
    /// This class works together with <see cref="IReferenceFrameTransform"/> and <see cref="IReferenceFrameSwitchResponder"/> to ensure that the objects respond to reference frame switches correctly.
    /// </remarks>
    public class SceneReferenceFrameManager : SingletonMonoBehaviour<SceneReferenceFrameManager>
    {
        public struct ReferenceFrameSwitchData
        {
            /// <summary>
            /// The reference frame that was active before the switch (with up-to-date <see cref="IReferenceFrame.ReferenceUT"/>).
            /// </summary>
            public IReferenceFrame OldFrame { get; set; }
            /// <summary>
            /// The reference frame that becomes active after the switch (with up-to-date <see cref="IReferenceFrame.ReferenceUT"/>).
            /// </summary>
            public IReferenceFrame NewFrame { get; set; }
        }

        private IReferenceFrameTransform _targetObject;
        /// <summary>
        /// The object that the scene will follow, ensuring that it's always close to the scene origin. 
        /// </summary>
        /// <remarks>
        /// Changing this object may result in a reference frame switch happening (at the next available time), if the new object is too far away or moving too fast.
        /// </remarks>
        public static IReferenceFrameTransform TargetObject
        {
            get => instance._targetObject;
            set
            {
                instance._targetObject = value;
                EnsureTargetObjectInSceneBounds();
            }
        }

        /// <summary>
        /// Invoked immediately AFTER the scene's reference frame switches.
        /// </summary>
        public static event Action<ReferenceFrameSwitchData> OnAfterReferenceFrameSwitch;

        private IReferenceFrame _referenceFrame;
        /// <summary>
        /// The current reference frame used by the scene.
        /// </summary>
        /// <remarks>
        /// The reference frame is updated during PhysicsProcessing, which happens after FixedUpdate. <br/>
        /// Use `ReferenceFrame.AtUT( TimeManager.UT )`, if you want to access what the scene frame will equal at the end of the current frame (assuming it won't switch in the meantime).
        /// </remarks>
        public static IReferenceFrame ReferenceFrame
        {
            get => instance._referenceFrame;
            set => instance._referenceFrame = value;
        }
#warning TODO - Consider including a getter for the 'referenceframe at end of current frame', which could consider incoming switches and return the frame after the switch.

        public bool IsSwitchRequested => _frameToSwitchTo != null;

        private static IReferenceFrame _frameToSwitchTo = null;

        /// <summary>
        /// Requests a reference frame switch at the next available time.
        /// </summary>
        /// <param name="referenceFrame">The reference frame to switch to. <br/> 
        ///     Must have its <see cref="ReferenceFrame.ReferenceUT"/> equal to the current time (as indicated by <see cref="TimeManager.UT"/>).</param>
        public static void RequestSceneReferenceFrameSwitch( IReferenceFrame referenceFrame )
        {
            if( referenceFrame.ReferenceUT != TimeManager.UT )
                throw new ArgumentException( $"The reference frame must have its {nameof( ReferenceFrame.ReferenceUT )} equal to the current UT ({nameof( TimeManager )}.{nameof( TimeManager.UT )}).", nameof( referenceFrame ) );

            _frameToSwitchTo = referenceFrame;
        }

        /// <summary>
        /// Attempts to switch to the requested reference frame, if one is set.
        /// </summary>
        private static void TrySwitchToRequestedSceneReferenceFrame()
        {
            if( _frameToSwitchTo == null )
                return;

            if( _frameToSwitchTo.ReferenceUT > TimeManager.UT )
                throw new InvalidOperationException( $"The reference frame can't be in the future (must have its {nameof( ReferenceFrame.ReferenceUT )} less than or equal to the current UT)." );

            if( _frameToSwitchTo.ReferenceUT != TimeManager.UT )
                _frameToSwitchTo = _frameToSwitchTo.AtUT( TimeManager.UT ); // This is mostly true when switch is requested inside Update (as opposed to FixedUpdate).
                                                                            // Additionally, this will not fire if the method called in Update adds FixedDeltaTime to the current UT
                                                                            //   (assuming the timestep won't change before the frame ends).

            IReferenceFrame newFrame = _frameToSwitchTo;
            IReferenceFrame oldFrame = ReferenceFrame;
            _frameToSwitchTo = null;

            ReferenceFrame = newFrame;
            Debug.Log( $"Switching to a new reference frame. UT: {TimeManager.UT}" );

            try
            {
                // Both oldFrame and newFrame should have up-to-date UT here.
                OnAfterReferenceFrameSwitch?.Invoke( new ReferenceFrameSwitchData() { OldFrame = oldFrame, NewFrame = newFrame } );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"An exception occurred while changing the scene reference frame." );
                Debug.LogException( ex );
            }
        }

        private static void ReferenceFrameSwitch_Objects( ReferenceFrameSwitchData data )
        {
            foreach( var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects() )
            {
                if( obj.TryGetComponent<IReferenceFrameSwitchResponder>( out var referenceFrameSwitch ) )
                {
                    referenceFrameSwitch.OnSceneReferenceFrameSwitch( data );
                }
            }
        }

        private static void ReferenceFrameSwitch_Trail( ReferenceFrameSwitchData data )
        {
            // WARN: This is very expensive to run when the trail has a lot of vertices.
            // When moving fast, don't use the default trail parameters, it produces too many vertices.

            // Trails don't work properly when the reference frame is moving anyway, they're in scene frame, not in any sort of useful frame.

            foreach( var trail in FindObjectsOfType<TrailRenderer>() ) // this is slow. It would be beneficial to keep a cache, or wrap this in a module.
            {
                for( int i = 0; i < trail.positionCount; i++ )
                {
                    trail.SetPosition( i, (Vector3)ReferenceFrameUtils.GetNewPosition( data.OldFrame, data.NewFrame, trail.GetPosition( i ) ) );
                }
            }
        }

        //
        //
        // Floating origin and active vessel following part of the class below:

        /// <summary>
        /// The maximum distance from scene origin that the <see cref="TargetObject"/> can reach before a reference frame switch will happen.
        /// </summary>
        /// <remarks>
        /// Larger values may make the vessel's parts appear spread apart (because of limited number of possible scene space positions to map to), and make shadows appear too soft, among other things.
        /// </remarks>
        public static float MaxRelativePosition { get; set; } = 1024.0f;

        /// <summary>
        /// The maximum scene-space velocity that the <see cref="TargetObject"/> can reach before a reference frame switch will happen.
        /// </summary>
        public static float MaxRelativeVelocity { get; set; } = 64.0f;

        /// <summary>
        /// Ensures that the <see cref="TargetObject"/> is within the allowed values for position and velocity. Requests a reference frame switch if required.
        /// </summary>
        public static void EnsureTargetObjectInSceneBounds()
        {
            if( TargetObject == null )
            {
                return;
            }

            Vector3 scenePosition = TargetObject.Position;
            Vector3 sceneVelocity = TargetObject.Velocity;
            if( sceneVelocity.magnitude > MaxRelativeVelocity || scenePosition.magnitude > MaxRelativePosition )
            {
                // Zero both position and velocity at the same time - most efficient in terms of number of frame switches.
                // Future available optimizations to further limit how often a switch needs to occur:
                // - Set the new frame's position to ahead of the object, instead of at the object.
                // - Use non-inertial reference frames.
                RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, TargetObject.AbsolutePosition, TargetObject.AbsoluteVelocity ) );
            }
        }

        static SceneReferenceFrameManager() // If this is set in awake, the atmosphere renderer and other things might get desynced.
        {
            OnAfterReferenceFrameSwitch = null;
            OnAfterReferenceFrameSwitch += ReferenceFrameSwitch_Objects;
            OnAfterReferenceFrameSwitch += ReferenceFrameSwitch_Trail;
        }

        void Awake()
        {
            instance._referenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );
        }


        void OnEnable()
        {
            PlayerLoopUtils.InsertSystemAfter<FixedUpdate>( in _playerLoopSystem, typeof( FixedUpdate.PhysicsFixedUpdate ) );
        }

        void OnDisable()
        {
            PlayerLoopUtils.RemoveSystem<FixedUpdate>( in _playerLoopSystem );
        }

        private static PlayerLoopSystem _playerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( SceneReferenceFrameManager ),
            updateDelegate = PlayerLoopFixedUpdate,
            subSystemList = null
        };

        private static void PlayerLoopFixedUpdate()
        {
            if( !instanceExists )
                return;

            // IMPORTANT: Do not put code above this line.
            // Otherwise things can desync - you'll be using rigidbody positions after they've been updated
            //                               alongside a reference frame that hasn't, breaking the contract.
            instance._referenceFrame = instance._referenceFrame.AtUT( TimeManager.UT );

            // Checking for out of bounds objects AFTER they have updated their positions (as opposed to inside FixedUpdate)
            //   prevents moving objects from appearing offset from the center.
            EnsureTargetObjectInSceneBounds();

            TrySwitchToRequestedSceneReferenceFrame();
        }
    }
}