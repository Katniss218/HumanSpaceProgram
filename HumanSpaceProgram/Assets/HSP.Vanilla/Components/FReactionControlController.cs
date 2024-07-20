using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    [Obsolete("Not implemented yet.")] // TODO - add actual functionality.
	public class FReactionControlController : MonoBehaviour
	{









        // RCS controllers can either control for desired angular accelerations, or linear accelerations.
        // There should be *one* controller active at any given time, or something else to sync them if two are needed.

        [MapsInheritingFrom( typeof( FReactionControlController ) )]
        public static SerializationMapping FReactionControlControllerMapping()
        {
            return new MemberwiseSerializationMapping<FReactionControlController>()
            {
            };
        }
    }
}