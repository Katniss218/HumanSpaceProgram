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

        public static readonly Substance Air = new Substance( "air" )
        {
            DisplayName = "Air",
            Phase = SubstancePhase.Gas,
            MolarMass = 0.0289647,
            DisplayColor = Color.clear
        };

        public static readonly Substance Oil = new Substance( "oil" )
        {
            DisplayName = "Oil",
            Phase = SubstancePhase.Liquid,
            ReferenceDensity = 800f,
            ReferencePressure = 101325f,
            BulkModulus = 1.5e9f,
            DisplayColor = Color.yellow
        };

        public static readonly Substance Kerosene = new Substance( "fuel" )
        {
            DisplayName = "Kerosene",
            Phase = SubstancePhase.Liquid,
            ReferenceDensity = 800f,
            ReferencePressure = 101325f,
            BulkModulus = 1.5e9f,
            DisplayColor = Color.yellow
        };

        public static readonly Substance Lox = new Substance( "lox" )
        {
            DisplayName = "Liquid Oxygen",
            Phase = SubstancePhase.Liquid,
            MolarMass = 0.0319988,
            ReferenceDensity = 1141f,
            ReferencePressure = 101325f,
            BulkModulus = 0.95e9f,
            DisplayColor = Color.cyan
        };

        public static readonly Substance Mercury = new Substance( "mercury" )
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