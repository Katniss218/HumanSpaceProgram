using HSP.Time;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public abstract class SceneReferenceFrameManager : MonoBehaviour
    {
        public struct ReferenceFrameSwitchData
        {
            /// <summary>
            /// The reference frame that was active before the switch (with up-to-date <see cref="IReferenceFrame.ReferenceUT"/> matching <see cref="TimeManager.UT"/>).
            /// </summary>
            public IReferenceFrame OldFrame { get; set; }
            /// <summary>
            /// The reference frame that becomes active after the switch (with up-to-date <see cref="IReferenceFrame.ReferenceUT"/> matching <see cref="TimeManager.UT"/>).
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
        public IReferenceFrameTransform targetObject
        {
            get => _targetObject;
            set
            {
                _targetObject = value;
                EnsureTargetObjectInSceneBounds();
            }
        }

        private HashSet<IReferenceFrameSwitchResponder> _responders = new();

        /// <summary>
        /// Invoked immediately AFTER the scene's reference frame switches.
        /// </summary>
        /// <remarks>
        /// NOTE TO IMPLEMENTERS: <br/>
        ///     This event is invoked 'during' (immediately after) unity physics step.
        /// </remarks>
        public event Action<ReferenceFrameSwitchData> OnAfterReferenceFrameSwitch;

        public void Subscribe( IReferenceFrameSwitchResponder responder )
        {
            if( responder == null )
                throw new ArgumentNullException( nameof( responder ), $"[{this.GetType().Name}] Can't subscribe a null responder." );

            _responders.Add( responder );
        }

        public void Unsubscribe( IReferenceFrameSwitchResponder responder )
        {
            if( responder == null )
                throw new ArgumentNullException( nameof( responder ), $"[{this.GetType().Name}] Can't unsubscribe a null responder." );

            _responders.Remove( responder );
        }

        /// <summary>
        /// The current reference frame used by the scene.
        /// </summary>
        /// <remarks>
        /// NOTE TO IMPLEMENTERS: <br/>
        ///     The reference frame is updated 'during' (immediately after) unity physics step, which happens after FixedUpdate. <br/>
        ///     If you want to access what the reference frame will be at the end of the frame from e.g. FixedUpdate, use `ReferenceFrame.AtUT( TimeManager.UT )`.
        /// </remarks>
        public IReferenceFrame referenceFrame { get; protected set; } = new CenteredReferenceFrame( 0, Vector3Dbl.zero );
        /*public static IReferenceFrame ReferenceFrame
        {
            get => instance._referenceFrame;
            set => instance._referenceFrame = value;
        }*/
#warning TODO - Consider including a getter for the 'referenceframe at end of current frame', which could consider incoming switches and return the frame after the switch.

        /// <summary>
        /// Returns true if a new reference frame is queued for switch at the next available time.
        /// </summary>
        public bool IsSwitchRequested => _frameToSwitchTo != null;

        private IReferenceFrame _frameToSwitchTo = null;

        /// <summary>
        /// Requests a reference frame switch at the next available time.
        /// </summary>
        /// <param name="referenceFrame">The reference frame to switch to. <br/> 
        ///     Must have its <see cref="ReferenceFrame.ReferenceUT"/> equal to the current time (as indicated by <see cref="TimeManager.UT"/>).</param>
        public void RequestReferenceFrameSwitch( IReferenceFrame referenceFrame )
        {
            if( referenceFrame.ReferenceUT != TimeManager.UT )
                throw new ArgumentException( $"[{this.GetType().Name}] The reference frame must have its {nameof( SceneReferenceFrameManager.referenceFrame.ReferenceUT )} equal to the current UT ({nameof( Time.TimeManager )}.{nameof( TimeManager.UT )}).", nameof( referenceFrame ) );

            Vector3 scenePosition = targetObject.Position;
            Vector3 sceneVelocity = targetObject.Velocity;
            Debug.Log( $"[{this.GetType().Name}] requesting switch " + scenePosition + " : " + sceneVelocity );
            _frameToSwitchTo = referenceFrame;
        }

        /// <summary>
        /// Attempts to immediately switch to the requested reference frame, if one is set. <br/>
        /// Only to be used after unity physics step
        /// </summary>
        private void TrySwitchToRequestedReferenceFrame()
        {
            if( _frameToSwitchTo == null )
                return;

            if( _frameToSwitchTo.ReferenceUT > TimeManager.UT )
                throw new InvalidOperationException( $"[{this.GetType().Name}] The reference frame can't be in the future (must have its {nameof( referenceFrame.ReferenceUT )} less than or equal to the current UT)." );

            if( _frameToSwitchTo.ReferenceUT != TimeManager.UT )
                _frameToSwitchTo = _frameToSwitchTo.AtUT( TimeManager.UT ); // This is mostly true when switch is requested inside Update (as opposed to FixedUpdate).
                                                                            // Additionally, this will not fire if the method called in Update adds FixedDeltaTime to the current UT
                                                                            //   (assuming the timestep won't change before the frame ends).

            // Both oldFrame and newFrame should now have UT that matches TimeManager.UT.

            IReferenceFrame newFrame = _frameToSwitchTo;
            IReferenceFrame oldFrame = referenceFrame;
            _frameToSwitchTo = null;

            referenceFrame = newFrame;
            Debug.Log( $"[{this.GetType().Name}] Switching to a new reference frame. UT: {TimeManager.UT}" );

            try
            {
                foreach( var responder in _responders )
                {
                    responder.OnSceneReferenceFrameSwitch( new ReferenceFrameSwitchData() { OldFrame = oldFrame, NewFrame = newFrame } );
                }
                OnAfterReferenceFrameSwitch?.Invoke( new ReferenceFrameSwitchData() { OldFrame = oldFrame, NewFrame = newFrame } );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"[{this.GetType().Name}] An exception occurred while changing the scene reference frame." );
                Debug.LogException( ex );
            }
        }

        /// <summary>
        /// The maximum distance from scene origin that the <see cref="TargetObject"/> can reach before a reference frame switch will happen.
        /// </summary>
        /// <remarks>
        /// Larger values may make the vessel's parts appear spread apart (because of limited number of possible scene space positions to map to), and make shadows appear too soft, among other things.
        /// </remarks>
        public float MaxRelativePosition { get; set; } = 500f;

        /// <summary>
        /// The maximum scene-space velocity that the <see cref="TargetObject"/> can reach before a reference frame switch will happen.
        /// </summary>
        public float MaxRelativeVelocity { get; set; } = 100f;

        /// <summary>
        /// Ensures that the <see cref="TargetObject"/> is within the allowed values for position and velocity. Requests a reference frame switch if required.
        /// </summary>
        public void EnsureTargetObjectInSceneBounds()
        {
            if( targetObject == null )
            {
                return;
            }

            Vector3 scenePosition = targetObject.Position;
            Vector3 sceneVelocity = targetObject.Velocity;
            if( sceneVelocity.magnitude > MaxRelativeVelocity || scenePosition.magnitude > MaxRelativePosition )
            {
                // Zero both position and velocity at the same time - most efficient in terms of number of frame switches.
                // Future available optimizations to further limit how often a switch needs to occur:
                // - Set the new frame's position to ahead of the object, instead of at the object.
                // - Use non-inertial reference frames.
                RequestReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT, targetObject.AbsolutePosition, targetObject.AbsoluteVelocity ) );
            }
        }

        static List<SceneReferenceFrameManager> _managers = new();

        protected virtual void Awake()
        {
            // This can now be inside Awake because the frame is switched during unity physics step, and not immediately.
            OnAfterReferenceFrameSwitch = null;
            //OnAfterReferenceFrameSwitch += ReferenceFrameSwitch_Responders;
        }

        protected virtual void OnEnable()
        {
            //PlayerLoopUtils.InsertSystemAfter<FixedUpdate>( in _playerLoopSystem, typeof( FixedUpdate.PhysicsFixedUpdate ) );
            _managers.Add( this );
            if( _managers.Count == 1 )
            {
                PlayerLoopUtils.AddSystem<FixedUpdate, FixedUpdate.PhysicsFixedUpdate>( in _playerLoopSystem );
            }
        }

        protected virtual void OnDisable()
        {
            _managers.Remove( this );
            if( _managers.Count == 0 )
            {
                PlayerLoopUtils.RemoveSystem<FixedUpdate, FixedUpdate.PhysicsFixedUpdate>( in _playerLoopSystem );
            }
        }

        private static PlayerLoopSystem _playerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( SceneReferenceFrameManager ),
            updateDelegate = ImmediatelyAfterUnityPhysicsStep,
            subSystemList = null
        };

        private static void ImmediatelyAfterUnityPhysicsStep()
        {
            if( !_managers.Any() )
                return;

            foreach( var manager in _managers )
            {
                // IMPORTANT: Do not put any code before the reference frame is updated.
                //   If you do, things can desync - You'll be using updated rigidbody values alongside a non-updated reference frame.
                manager.referenceFrame = manager.referenceFrame.AtUT( TimeManager.UT );

                // Checking object in scene bounds after unity physics step ensures that the new frame will match where the object is.
                manager.EnsureTargetObjectInSceneBounds();

                manager.TrySwitchToRequestedReferenceFrame();
            }
        }
    }
}