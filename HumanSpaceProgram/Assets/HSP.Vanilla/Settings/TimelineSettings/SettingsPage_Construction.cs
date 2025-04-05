using HSP.Settings;
using HSP.Timelines;
using HSP.Vessels.Construction;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Settings
{
    public sealed class SettingsPage_Construction : SettingsPage<SettingsPage_Construction>, ITimelineSettingsPage
    {
        public float ConstructionSpeedMultiplier { get; set; } = 1.0f;

        protected override SettingsPage_Construction OnApply()
        {
            // Apply externally since the multiplier needs to be in HSP.Vessels.Construction which doesn't have access to this page.
            return this;
        }


        [MapsInheritingFrom( typeof( SettingsPage_Construction ) )]
        public static SerializationMapping SettingsPage_ConstructionMapping()
        {
            return new MemberwiseSerializationMapping<SettingsPage_Construction>()
                .WithMember( "construction_speed_multiplier", o => o.ConstructionSpeedMultiplier );
        }
    }
}