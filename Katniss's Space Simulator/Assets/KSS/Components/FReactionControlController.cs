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
	public class FReactionControlController : MonoBehaviour, IPersistsData
	{









		// RCS controllers can either control for desired angular accelerations, or linear accelerations.
		// There should be *one* controller active at any given time, or something else to sync them if two are needed.

		public SerializedData GetData( IReverseReferenceMap s )
		{
			throw new NotImplementedException();
		}

		public void SetData( IForwardReferenceMap l, SerializedData data )
		{
			throw new NotImplementedException();
		}
	}
}