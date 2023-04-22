using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.Managers
{
    /// <remarks>
    /// This class is my implementation of scene-wide Floating Origin / Krakensbane.
    /// </remarks>
    public class ReferenceFrameManager : MonoBehaviour
    {
        public struct ReferenceFrameSwitchData
        {
            public IReferenceFrame OldFrame { get; set; }
            public IReferenceFrame NewFrame { get; set; }
        }

        // Only one reference frame can be used by the scene at any given time.

        // "reference frame" is used to make the world space behave like the local space of said reference frame.

        static ReferenceFrameManager()
        {
            OnReferenceFrameSwitch += ReferenceFrameSwitch_Objects;
            OnReferenceFrameSwitch += ReferenceFrameSwitch_Trail;
        }


        public static IReferenceFrame CurrentReferenceFrame { get; private set; } = new DefaultFrame( Vector3Large.zero );

        public static event Action<ReferenceFrameSwitchData> OnReferenceFrameSwitch;

        public static void SwitchReferenceFrame( IReferenceFrame newFrame )
        {
            IReferenceFrame old = CurrentReferenceFrame;
            CurrentReferenceFrame = newFrame;

            OnReferenceFrameSwitch?.Invoke( new ReferenceFrameSwitchData() { OldFrame = old, NewFrame = newFrame } );
        }

        public static Vector3 GetNewPosition( IReferenceFrame oldFrame, IReferenceFrame newFrame, Vector3 oldPosition )
        {
            // If both frames are inertial and not rotated, and scaled equally, it's enough to calculate the difference between any position.

            Vector3Large globalPosition = oldFrame.TransformPosition( oldPosition );
            Vector3 newPosition = newFrame.InverseTransformPosition( globalPosition );
            return newPosition;
        }

        private static void ReferenceFrameSwitch_Objects( ReferenceFrameSwitchData data )
        {
            foreach( var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects() )
            {
                /*Rigidbody rb = obj.GetComponent<Rigidbody>();
                if( rb != null )
                {
                    rb.position = GetNewPosition( data.OldFrame, data.NewFrame, obj.transform.position );
                }
                else
                {
                */
                    obj.transform.position = GetNewPosition( data.OldFrame, data.NewFrame, obj.transform.position );
               // }

                // TODO - add rotations/scaling/etc later.
                // Add PhysicsObject integration for things that have to get their forces/velocities/angular velocities/etc recalculated.

                // trail/line renderers need to be updated too, if any exist. Possibly add an event here, so mods can hook into.
            }
        }

        private static void ReferenceFrameSwitch_Trail( ReferenceFrameSwitchData data )
        {
            foreach( var trail in FindObjectsOfType<TrailRenderer>() )
            {
                for( int i = 0; i < trail.positionCount; i++ )
                {
                    trail.SetPosition( i, GetNewPosition( data.OldFrame, data.NewFrame, trail.GetPosition( i ) ) );
                }
            }
        }

        // something to force single instance would be nice in the future.

        void FixedUpdate()
        {
            const float MaxFloatingOriginRange = 10000.0f;

            float max = MaxFloatingOriginRange;
            float min = -max;

            if( VesselManager.ActiveVessel.transform.position.x < min || VesselManager.ActiveVessel.transform.position.x > max
             || VesselManager.ActiveVessel.transform.position.y < min || VesselManager.ActiveVessel.transform.position.y > max
             || VesselManager.ActiveVessel.transform.position.z < min || VesselManager.ActiveVessel.transform.position.z > max )
            {
                SwitchReferenceFrame( CurrentReferenceFrame.Shift( VesselManager.ActiveVessel.transform.position ) );
            }
        }
    }
}