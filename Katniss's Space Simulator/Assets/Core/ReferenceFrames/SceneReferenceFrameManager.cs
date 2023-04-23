using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.ReferenceFrames
{
    /// <remarks>
    /// This class is my implementation of scene-wide Floating Origin / Krakensbane.
    /// </remarks>
    public class SceneReferenceFrameManager : MonoBehaviour
    {
        // something to force single instance of this would be nice in the future.

        public struct ReferenceFrameSwitchData
        {
            public IReferenceFrame OldFrame { get; set; }
            public IReferenceFrame NewFrame { get; set; }
        }

        // Only one reference frame can be used by the scene at any given time.

        // "reference frame" is used to make the world space behave like the local space of said reference frame.

        static SceneReferenceFrameManager()
        {
            OnAfterReferenceFrameSwitch += ReferenceFrameSwitch_Objects;
            OnAfterReferenceFrameSwitch += ReferenceFrameSwitch_Trail;
        }

        /// <summary>
        /// Called when the scene's reference frame switches.
        /// </summary>
        public static event Action<ReferenceFrameSwitchData> OnAfterReferenceFrameSwitch;

        /// <summary>
        /// The reference frame that describes how to convert between Absolute Inertial Reference Frame and the scene's world space.
        /// </summary>
        public static IReferenceFrame SceneReferenceFrame { get; private set; } = new OffsetReferenceFrame( Vector3Dbl.zero );

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
#warning TODO - if something declares that it keeps a more accurate position in AIRF - use that (check for some interface, get airf, transform by new reference frame).
                obj.transform.position = ReferenceFrameUtils.GetNewPosition( data.OldFrame, data.NewFrame, obj.transform.position );

                // TODO - add rotations/scaling/etc later.
                // Add PhysicsObject integration for things that have to get their forces/velocities/angular velocities/etc recalculated.

                // trail/line renderers need to be updated too, if any exist. Possibly add an event here, so mods can hook into.
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

        /// <summary>
        /// The extents of the area aroundthe scene origin, in which the active vessel is permitted to exist.
        /// </summary>
        /// <remarks>
        /// If the active vessel moves outside of this range, an origin shift will happen.
        /// </remarks>
        public static float MaxFloatingOriginRange { get; set; } = 32767.0f;

        /// <summary>
        /// Checks whether the current active vessel is too far away from the scene's origin, and performs an origin shift if it is.
        /// </summary>
        public static void TryFixActiveVesselOutOfBounds()
        {
            float max = MaxFloatingOriginRange;
            float min = -max;

            Vector3 position = VesselManager.ActiveVessel.transform.position;
            if( position.x < min || position.x > max
             || position.y < min || position.y > max
             || position.z < min || position.z > max )
            {
                ChangeSceneReferenceFrame( SceneReferenceFrame.Shift( position ) );
            }
        }

        void FixedUpdate()
        {
            TryFixActiveVesselOutOfBounds();
        }
    }
}