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

        private static IReferenceFrameTransform _targetObject;
        public static IReferenceFrameTransform TargetObject
        {
            get => _targetObject;
            set
            {
                _targetObject = value;
                TryFixActiveObjectOutOfBounds();
            }
        }

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
#warning TODO - remove this getter - accessors should not return a modified reference frame. Use GetReferenceFrame.
            //get => instance._referenceFrame;
            get => instance._referenceFrame.AtUT( TimeManager.UT );
            set => instance._referenceFrame = value;
        }

        public static IReferenceFrame GetReferenceFrame()
        {
            return instance._referenceFrame.AtUT( TimeManager.UT );
        }

        public static IReferenceFrame GetReferenceFrameAtUT( double ut )
        {
            return instance._referenceFrame.AtUT( ut );
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
            if( TargetObject == null )
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
            else
            {
                IReferenceFrameTransform trans = ActiveObjectManager.ActiveObject.GetComponent<IReferenceFrameTransform>();
                if( trans != null )
                {
                    Vector3 sceneVelocity = trans.Velocity;
                    if( sceneVelocity.magnitude > MaxRelativeVelocity )
                    {
                        if( ReferenceFrame is CenteredReferenceFrame rf )
                            ChangeSceneReferenceFrame( new CenteredInertialReferenceFrame( TimeManager.UT, rf.Position, (Vector3Dbl)sceneVelocity, Vector3Dbl.zero ) );
                        else if( ReferenceFrame is CenteredInertialReferenceFrame rf2 )
                            ChangeSceneReferenceFrame( new CenteredInertialReferenceFrame( TimeManager.UT, rf2.Position, (Vector3Dbl)sceneVelocity, Vector3Dbl.zero ) );
                    }
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
            ReferenceFrame = new CenteredReferenceFrame( TimeManager.UT, Vector3Dbl.zero );

            //ChangeSceneReferenceFrame( new CenteredInertialReferenceFrame( TimeManager.UT, Vector3Dbl.zero, new Vector3Dbl( 0, 25, 25 ).normalized * 50.0, Vector3Dbl.zero ) );
        }

        void FixedUpdate()
        {
            TryFixActiveObjectOutOfBounds();
        }
    }
}