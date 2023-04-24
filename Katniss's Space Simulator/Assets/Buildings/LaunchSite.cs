using KatnisssSpaceSimulator.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KatnisssSpaceSimulator.Buildings
{
    public class LaunchSite : MonoBehaviour
    {
        [field: SerializeField]
        public Transform Spawner { get; set; }

        public Vector3Dbl GetSpawnerAIRFPosition()
        {
            return SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( this.Spawner.position ); // TODO - precision, transform local position using celestial body reference frame.
        }
    }
}