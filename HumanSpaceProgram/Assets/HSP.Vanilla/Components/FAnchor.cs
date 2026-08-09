using HSP.Vessels;
using UnityEngine;

namespace HSP.Trajectories.Components
{
    /// <summary>
    /// Specifies that this object should be anchored to the ground, instead of following its own trajectory in the world.
    /// </summary>
    [DisallowMultipleComponent]
    public class FAnchor : FComponent
    {
        /// <summary>
        /// Checks if the object should be anchored.
        /// </summary>
        public static bool HasAnchor( Vessel partGraph )
        {
            return false;// partGraph.GetFComponents<FAnchor>().Count > 0;
        }
    }
}