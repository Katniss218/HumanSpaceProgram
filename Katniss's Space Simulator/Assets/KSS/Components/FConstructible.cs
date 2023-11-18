using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    /// <summary>
    /// A data container for an object that can be constructed.
    /// </summary>
    public class FConstructible : MonoBehaviour, IPersistent
    {
        /// <summary>
        /// One build point at 1x build speed takes 1 [s] to build.
        /// </summary>
        public float MaxBuildPoints { get; set; }

        public Dictionary<string, float> Conditions { get; set; } // condition id and value to enable construction.

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