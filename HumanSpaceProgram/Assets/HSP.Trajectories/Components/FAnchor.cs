﻿using UnityEngine;

namespace HSP.Trajectories.Components
{
    /// <summary>
    /// Specifies that this object should be anchored to the ground, instead of following its own trajectory in the world.
    /// </summary>
    [DisallowMultipleComponent]
    public class FAnchor : MonoBehaviour
    {


        /// <summary>
        /// Checks if the object should be anchored.
        /// </summary>
        public static bool IsAnchored( Transform transform )
        {
            return transform.gameObject.HasComponentInChildren<FAnchor>();
        }

        /// <summary>
        /// Checks if the root of the object should be anchored.
        /// </summary>
        public static bool IsRootAnchored( Transform transform )
        {
            return transform.root.gameObject.HasComponentInChildren<FAnchor>();
        }
    }
}