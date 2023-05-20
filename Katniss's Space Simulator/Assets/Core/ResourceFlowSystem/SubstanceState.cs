using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core.ResourceFlowSystem
{
    /// <summary>
    /// State information about a single resource.
    /// </summary>
    [Serializable]
    public struct SubstanceState
    {
        /// <summary>
        /// The physical/chemical data about the specific resource.
        /// </summary>
        [field: SerializeField]
        public Substance Data { get; private set; } // private setter for Unity inspector

        /// <summary>
        /// Amount of substance, tracked using mass, in [kg].
        /// </summary>
        [field: SerializeField]
        public float MassAmount { get; private set; } // private setter for Unity inspector. Could also be in moles instead hahaha.

        public SubstanceState( float massAmount, Substance resource )
        {
            this.MassAmount = massAmount;
            this.Data = resource;
        }

        public SubstanceState( SubstanceState original, float massAmount )
        {
            this.Data = original.Data;
            this.MassAmount = massAmount;
        }
    }
}