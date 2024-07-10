using KSS.Control.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core.ResourceFlowSystem
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
        public Substance Substance { get; private set; } // private setter for Unity inspector

        /// <summary>
        /// Amount of substance, tracked using mass, in [kg].
        /// </summary>
        [field: SerializeField]
        public float MassAmount { get; private set; } // private setter for Unity inspector. Could also be in moles instead hahaha.

        public SubstanceState( float massAmount, Substance resource )
        {
            this.MassAmount = massAmount;
            this.Substance = resource;
        }

        public SubstanceState( SubstanceState original, float massAmount )
        {
            this.Substance = original.Substance;
            this.MassAmount = massAmount;
        }


        [MapsInheritingFrom( typeof( SubstanceState ) )]
        public static SerializationMapping SubstanceStateMapping()
        {
            return new MemberwiseSerializationMapping<SubstanceState>()
            {
                ("substance", new Member<SubstanceState, Substance>( ObjectContext.Asset, o => o.Substance )),
                ("mass_amount", new Member<SubstanceState, float>( o => o.MassAmount ))
            };
        }
        /*
        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "substance", s.WriteAssetReference( Substance ) },
                { "mass_amount", MassAmount.AsSerialized() }
            };
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            Substance = l.ReadAssetReference<Substance>( data["substance"] );
            MassAmount = data["mass_amount"].AsFloat();
        }*/
    }
}