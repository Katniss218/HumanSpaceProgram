using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <remarks>
    /// This class is my implementation of scene-wide Floating Origin / Krakensbane.
    /// </remarks>
    public static class ReferenceFrameManager
    {
        // Only one reference frame can be used by the scene at any given time.

        // "reference frame" is used to make the world space behave like the local space of said reference frame.



        public static IReferenceFrame CurrentReferenceFrame { get; private set; } = new DefaultFrame( Vector3Large.zero );

        public static void SwitchReferenceFrame( IReferenceFrame newFrame )
        {
            foreach( var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects() )
            {
                Vector3Large globalPosition = CurrentReferenceFrame.InverseTransformPosition( obj.transform.position );
                Vector3 newScenePosition = newFrame.TransformPosition( globalPosition );
                obj.transform.position = newScenePosition;

                // TODO - add rotations/scaling/etc later.
                // Add PhysicsObject integration for things that have to get their forces/velocities/angular velocities/etc recalculated.
            }
            CurrentReferenceFrame = newFrame;
            // calculate new scene positions for every root object.
        }
    }
}
