using HSP.Content.Migrations;
using HSP.Time;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Migrations
{
    internal class VanillaMigration_0_0_to_1_0
    {
        [StructuralMigration( "Vanilla", "0.0", "1.0", Description = "Add TimeManager json" )]
        private static void Migration_0_0_to_1_0_Structural( IMigrationContext context )
        {
            SerializedData data = new SerializedObject()
            {
                { "$type", typeof(TimeManager).SerializeType() },
                { "ut", (SerializedPrimitive)0 },
            };

            context.WriteFile( "TimeManager.json", data );
        }
    }
}