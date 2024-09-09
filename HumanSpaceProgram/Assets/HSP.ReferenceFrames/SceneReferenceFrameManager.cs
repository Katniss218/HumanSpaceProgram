using HSP.Time;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

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

        private IReferenceFrameTransform _targetObject;
        public static IReferenceFrameTransform TargetObject
        {
            get => instance._targetObject;
            set
            {
                instance._targetObject = value;
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
            get => GetReferenceFrame();
            set => instance._referenceFrame = value;
        }

        private double cachedUT;

        public static IReferenceFrame GetReferenceFrame()
        {
            if( instance.cachedUT != TimeManager.UT )
            {
                instance._referenceFrame = instance._referenceFrame.AtUT( TimeManager.UT );
                instance.cachedUT = TimeManager.UT;
            }
            return instance._referenceFrame;
        }

        public static IReferenceFrame GetReferenceFrameAtUT( double ut )
        {
            return instance._referenceFrame.AtUT( ut );
        }

        private static IReferenceFrame _toChange = null;

        public static void RequestChangeSceneReferenceFrame( IReferenceFrame referenceFrame )
        {
            _toChange = referenceFrame;
        }

        /// <summary>
        /// Sets the scene's reference frame to the specified frame, and calls out a frame switch event.
        /// </summary>
        private static void ChangeSceneReferenceFrame()
        {
            if( _toChange == null )
                return;

            IReferenceFrame newFrame = _toChange;
            IReferenceFrame oldFrame = ReferenceFrame;
            ReferenceFrame = newFrame;
            _toChange = null;

            try
            {
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
        public static float MaxRelativePosition { get; set; } = 1024.0f;
        public static float MaxRelativeVelocity { get; set; } = 64.0f;

        /// <summary>
        /// Checks whether the current active vessel is too far away from the scene's origin, and performs an origin shift if it is.
        /// </summary>
        public static void TryFixActiveObjectOutOfBounds()
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
                // Future available optimizations:
                // - non-inertial frames (include acceleration).
                // - switch the position to ahead of where the object is accelerating instead of the center of the object.
                RequestChangeSceneReferenceFrame( new CenteredInertialReferenceFrame( TimeManager.UT, TargetObject.AbsolutePosition, TargetObject.AbsoluteVelocity ) );
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
        }

        void FixedUpdate()
        {
            TryFixActiveObjectOutOfBounds();
        }


        private void OnEnable()
        {
            AddFrameSwitcherToPlayerLoop();
        }

        private void OnDisable()
        {
            RemoveFrameSwitcherFromPlayerLoop();
        }




        public static void PlayerLoop_AfterPhysics()
        {
            if( !instanceExists )
                return;

            ChangeSceneReferenceFrame();
        }

        static PlayerLoopSystem _playerLoopSystem = new PlayerLoopSystem()
        {
            type = typeof( SceneReferenceFrameManager ),
            updateDelegate = PlayerLoop_AfterPhysics,
            subSystemList = null
        };

        private static void AddFrameSwitcherToPlayerLoop()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopUtils.InsertSystem<FixedUpdate>( ref playerLoop, in _playerLoopSystem, 12 );
            PlayerLoop.SetPlayerLoop( playerLoop );
        }

        private static void RemoveFrameSwitcherFromPlayerLoop()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopUtils.RemoveSystem<FixedUpdate>( ref playerLoop, in _playerLoopSystem );
            PlayerLoop.SetPlayerLoop( playerLoop );
        }
    }



#warning TODO - clean up. Also, add a way to insert 'before' / 'after' a specified system type when inserting into a subsystem list.

    public class PlayerLoopUtils
    {
        // https://github.com/adammyhre/Unity-Improved-Timers/blob/master/Runtime/PlayerLoopUtils.cs

        public static void RemoveSystem<T>( ref PlayerLoopSystem loop, in PlayerLoopSystem systemToRemove )
        {
            if( loop.subSystemList == null )
                return;

            var playerLoopSystemList = new List<PlayerLoopSystem>( loop.subSystemList );
            for( int i = 0; i < playerLoopSystemList.Count; ++i )
            {
                if( playerLoopSystemList[i].type == systemToRemove.type && playerLoopSystemList[i].updateDelegate == systemToRemove.updateDelegate )
                {
                    playerLoopSystemList.RemoveAt( i );
                    loop.subSystemList = playerLoopSystemList.ToArray();
                }
            }

            HandleSubSystemLoopForRemoval<T>( ref loop, systemToRemove );
        }

        static void HandleSubSystemLoopForRemoval<T>( ref PlayerLoopSystem loop, PlayerLoopSystem systemToRemove )
        {
            if( loop.subSystemList == null )
                return;

            for( int i = 0; i < loop.subSystemList.Length; ++i )
            {
                RemoveSystem<T>( ref loop.subSystemList[i], systemToRemove );
            }
        }

        public static bool InsertSystem<T>( ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index )
        {
            if( loop.type != typeof( T ) )
                return HandleSubSystemLoop<T>( ref loop, systemToInsert, index );

            var playerLoopSystemList = new List<PlayerLoopSystem>();
            if( loop.subSystemList != null )
                playerLoopSystemList.AddRange( loop.subSystemList );
            playerLoopSystemList.Insert( index, systemToInsert );
            loop.subSystemList = playerLoopSystemList.ToArray();
            return true;
        }

        static bool HandleSubSystemLoop<T>( ref PlayerLoopSystem loop, in PlayerLoopSystem systemToInsert, int index )
        {
            if( loop.subSystemList == null )
                return false;

            for( int i = 0; i < loop.subSystemList.Length; ++i )
            {
                if( !InsertSystem<T>( ref loop.subSystemList[i], in systemToInsert, index ) )
                    continue;
                return true;
            }

            return false;
        }

        public static void PrintPlayerLoop( PlayerLoopSystem loop )
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine( "Unity Player Loop" );
            foreach( PlayerLoopSystem subSystem in loop.subSystemList )
            {
                PrintSubsystem( subSystem, sb, 0 );
            }
            Debug.Log( sb.ToString() );
        }

        static void PrintSubsystem( PlayerLoopSystem system, StringBuilder sb, int level )
        {
            sb.Append( ' ', level * 2 ).AppendLine( system.type.ToString() );
            if( system.subSystemList == null || system.subSystemList.Length == 0 ) return;

            foreach( PlayerLoopSystem subSystem in system.subSystemList )
            {
                PrintSubsystem( subSystem, sb, level + 1 );
            }
        }
    }
}