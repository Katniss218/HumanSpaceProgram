//using System;
//using UnityEngine;
//using UnityPlus.Serialization;

//namespace HSP.ResourceFlow
//{
//    /// <summary>
//    /// State information about a single resource.
//    /// </summary>
//    [Serializable]
//    public readonly struct SubstanceState
//    {
//        /// <summary>
//        /// The physical/chemical data about the specific resource.
//        /// </summary>
//        [field: SerializeField]
//        public Substance Substance { get; }

//        /// <summary>
//        /// Amount of substance, tracked using mass, in [kg].
//        /// </summary>
//        [field: SerializeField]
//        public float MassAmount { get; }


//        public SubstanceState( float massAmount, Substance resource )
//        {
//            this.MassAmount = massAmount;
//            this.Substance = resource;
//        }

//        public SubstanceState( SubstanceState original, float massAmount )
//        {
//            this.Substance = original.Substance;
//            this.MassAmount = massAmount;
//        }



//        [MapsInheritingFrom( typeof( SubstanceState ) )]
//        public static SerializationMapping SubstanceStateMapping()
//        {
//            return new MemberwiseSerializationMapping<SubstanceState>()
//                .WithMember( "substance", ObjectContext.Asset, o => o.Substance )
//                .WithMember( "mass_amount", o => o.MassAmount );
//        }
//    }
//}