using System;
using UnityEngine;

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

        private IReferenceFrame _sceneReferenceFrame;
        /// <summary>
        /// The reference frame that describes how to convert between Absolute Inertial Reference Frame and the scene's world space.
        /// </summary>
        public static IReferenceFrame SceneReferenceFrame
        {
            get => instance._sceneReferenceFrame;
            set => instance._sceneReferenceFrame = value;
        }

        /// <summary>
        /// Sets the scene's reference frame to the specified frame, and calls out a frame switch event.
        /// </summary>
        public static void ChangeSceneReferenceFrame( IReferenceFrame newFrame )
        {
            IReferenceFrame old = SceneReferenceFrame;
            SceneReferenceFrame = newFrame;

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

            foreach( var trail in FindObjectsOfType<TrailRenderer>() ) // this is slow. It would be beneficial to keep a cache, or wrap this in a module.
            {
                for( int i = 0; i < trail.positionCount; i++ )
                {
                    trail.SetPosition( i, ReferenceFrameUtils.GetNewPosition( data.OldFrame, data.NewFrame, trail.GetPosition( i ) ) );
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
        public static float MaxFloatingOriginRange { get; set; } = 8192.0f;

        /// <summary>
        /// Checks whether the current active vessel is too far away from the scene's origin, and performs an origin shift if it is.
        /// </summary>
        public static void TryFixActiveObjectOutOfBounds()
        {
            if( ActiveObjectManager.ActiveObject == null )
            {
                return;
            }

            Vector3 position = ActiveObjectManager.ActiveObject.transform.position;
            if( position.magnitude > MaxFloatingOriginRange )
            {
                ChangeSceneReferenceFrame( SceneReferenceFrame.Shift( position ) );
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
            SceneReferenceFrame = new CenteredReferenceFrame( Vector3Dbl.zero );
        }

        void FixedUpdate()
        {
            TryFixActiveObjectOutOfBounds();
        }
    }
}