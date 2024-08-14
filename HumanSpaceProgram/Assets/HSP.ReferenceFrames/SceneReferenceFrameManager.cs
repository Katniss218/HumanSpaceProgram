using HSP.Time;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HSP.ReferenceFrames
{
    /// <remarks>
    /// This is basically a scene-wide Floating Origin / Krakensbane, based on reference frames that follow the <see cref="ActiveObjectManager.ActiveObject"/>.
    /// </remarks>
    public class SceneReferenceFrameManager : SingletonMonoBehaviour<SceneReferenceFrameManager>
    {
        public struct ReferenceFrameSwitchData
        {
            public IReferenceFrame OldFrame { get; set; }
            public IReferenceFrame NewFrame { get; set; }
        }

        // Only one reference frame can be used by the scene at any given time.

        // "reference frame" is used to make the world space behave like the local space of said reference frame.

        /// <summary>
        /// Called when the scene's reference frame switches.
        /// </summary>
        public static event Action<ReferenceFrameSwitchData> OnAfterReferenceFrameSwitch;

        private IReferenceFrame _referenceFrame;
        /// <summary>
        /// The reference frame that describes how to convert between Absolute Inertial Reference Frame and the scene's world space.
        /// </summary>
        public static IReferenceFrame ReferenceFrame
        {
            get => instance._referenceFrame;
            set => instance._referenceFrame = value;
        }

        /// <summary>
        /// Sets the scene's reference frame to the specified frame, and calls out a frame switch event.
        /// </summary>
        public static void ChangeSceneReferenceFrame( IReferenceFrame newFrame )
        {
            IReferenceFrame old = ReferenceFrame;
            ReferenceFrame = newFrame;

            OnAfterReferenceFrameSwitch?.Invoke( new ReferenceFrameSwitchData() { OldFrame = old, NewFrame = newFrame } );
        }

        private static void ReferenceFrameSwitch_Objects( ReferenceFrameSwitchData data )
        {
            foreach( var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects() )
            {
                IReferenceFrameSwitchResponder referenceFrameSwitch = obj.GetComponent<IReferenceFrameSwitchResponder>();
                if( referenceFrameSwitch != null )
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
        /// The extents of the area aroundthe scene origin, in which the active vessel is permitted to exist.
        /// </summary>
        /// <remarks>
        /// If the active vessel moves outside of this range, an origin shift will happen. <br/>
        /// Larger values of MaxFloatingOriginRange can make the vessel's parts appear spread apart (because of limited number of possible world positions to map to).
        /// </remarks>
        public static float MaxRelativePosition { get; set; } = 512.0f;
        public static float MaxRelativeVelocity { get; set; } = 32.0f;

        /// <summary>
        /// Checks whether the current active vessel is too far away from the scene's origin, and performs an origin shift if it is.
        /// </summary>
        public static void TryFixActiveObjectOutOfBounds()
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                return;
            }

#warning TODO - this needs to be a IReferenceFrameTransform, not active object. Which is related to the fact it should be split off.
            Vector3 scenePosition = ActiveObjectManager.ActiveObject.position;
            if( scenePosition.magnitude > MaxRelativePosition )
            {
#warning TODO - we might want to control the parameters more directly. Also, needs to switch based on object velocity as well (direction and magnitude) relative to frame velocity.

                ChangeSceneReferenceFrame( ReferenceFrame.Shift( scenePosition ) );
            }
            IReferenceFrameTransform trans = ActiveObjectManager.ActiveObject.GetComponent<IReferenceFrameTransform>();
            if( trans != null )
            {
                Vector3 sceneVelocity = trans.Velocity;
                if( sceneVelocity.magnitude > MaxRelativeVelocity )
                {
                    //ChangeSceneReferenceFrame( new CenteredInertialReferenceFrame( ReferenceFrame.Position, (Vector3Dbl)sceneVelocity, Vector3Dbl.zero ) );
                }
            }
        }

        static SceneReferenceFrameManager() // If this is set in awake, the atmosphere renderer and other might get detached.
        {
            OnAfterReferenceFrameSwitch = null;
            OnAfterReferenceFrameSwitch += ReferenceFrameSwitch_Objects;
            OnAfterReferenceFrameSwitch += ReferenceFrameSwitch_Trail;
        }

        void Awake()
        {
            ReferenceFrame = new CenteredReferenceFrame( Vector3Dbl.zero );

            ChangeSceneReferenceFrame( new CenteredInertialReferenceFrame( Vector3Dbl.zero, new Vector3Dbl( 0, 25, 25 ).normalized * 500.0, Vector3Dbl.zero ) );
        }

        void FixedUpdate()
        {
#warning TODO - non-rest reference frames need to be updated, and they will need to be continuous, not discrete. Able to be sampled at a given UT offset.
            // to do this, non-rest reference frames need to know the UT at which they were created.
            ReferenceFrame = ReferenceFrame.AddUT( TimeManager.FixedDeltaTime );

            TryFixActiveObjectOutOfBounds();
        }
    }
}