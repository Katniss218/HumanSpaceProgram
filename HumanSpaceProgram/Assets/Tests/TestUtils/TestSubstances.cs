using HSP.ResourceFlow;
using UnityEngine;

namespace HSP_Tests
{
    public static class TestSubstances
    {
        public static readonly Substance Water = new Substance( "water" )
        {
            DisplayName = "Water",
            Phase = SubstancePhase.Liquid,
            ReferenceDensity = 1000f,
            ReferencePressure = 101325f,
            BulkModulus = 2.2e9f,
            DisplayColor = Color.blue
        };

        public static readonly ISubstance Air = new Substance( "air" )
        {
            DisplayName = "Air",
            Phase = SubstancePhase.Gas,
            MolarMass = 0.0289647,
            DisplayColor = Color.clear
        };

        public static readonly ISubstance Oil = new Substance( "oil" )
        {
            DisplayName = "Oil",
            Phase = SubstancePhase.Liquid,
            ReferenceDensity = 800f,
            ReferencePressure = 101325f,
            BulkModulus = 1.5e9f,
            DisplayColor = Color.yellow
        };

        public static readonly ISubstance Kerosene = new Substance( "fuel" )
        {
            DisplayName = "Kerosene",
            Phase = SubstancePhase.Liquid,
            ReferenceDensity = 800f,
            ReferencePressure = 101325f,
            BulkModulus = 1.5e9f,
            DisplayColor = Color.yellow
        };

        public static readonly ISubstance Mercury = new Substance( "mercury" )
        {
            DisplayName = "Mercury",
            Phase = SubstancePhase.Liquid,
            ReferenceDensity = 13500f,
            ReferencePressure = 101325f,
            BulkModulus = 2.85e9f,
            DisplayColor = Color.gray
        };
    }
}