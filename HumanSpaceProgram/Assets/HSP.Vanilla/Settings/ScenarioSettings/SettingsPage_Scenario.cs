using HSP.Settings;
using HSP.Timelines;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Settings
{
    public sealed class SettingsPage_Scenario : SettingsPage<SettingsPage_Scenario>, IScenarioSettingsPage
    {
        protected override SettingsPage_Scenario OnApply()
        {
            // Apply externally since the multiplier needs to be in HSP.Vessels.Construction which doesn't have access to this page.
            return this;
        }


        [MapsInheritingFrom( typeof( SettingsPage_Scenario ) )]
        public static SerializationMapping SettingsPage_ScenarioMapping()
        {
            return new MemberwiseSerializationMapping<SettingsPage_Scenario>()
                ;//.WithMember( "construction_speed_multiplier", o => o.ConstructionSpeedMultiplier );
        }
    }
}