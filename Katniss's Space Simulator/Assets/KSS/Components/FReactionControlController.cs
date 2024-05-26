using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    [Obsolete("Not implemented yet.")] // TODO - add actual functionality.
	public class FReactionControlController : MonoBehaviour
	{









        // RCS controllers can either control for desired angular accelerations, or linear accelerations.
        // There should be *one* controller active at any given time, or something else to sync them if two are needed.

        [SerializationMappingProvider( typeof( FReactionControlController ) )]
        public static SerializationMapping FReactionControlControllerMapping()
        {
            return new CompoundSerializationMapping<FReactionControlController>()
            {
            }
            .IncludeMembers<Behaviour>()
            .UseBaseTypeFactory();
        }
        /*
		public SerializedData GetData( IReverseReferenceMap s )
		{

            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            //ret.AddAll( new SerializedObject()

			return ret;
        }

		public void SetData( SerializedData data, IForwardReferenceMap l )
		{
			IPersistent_Behaviour.SetData( this, data, l );
		}*/
    }
}