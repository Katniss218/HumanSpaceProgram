using HSP.Settings;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Settings
{
    public sealed class SettingsPage_Graphics : SettingsPage<SettingsPage_Graphics>
    {
        public int TargetFramerate { get; set; } = 120;

        protected override SettingsPage_Graphics OnApply()
        {
            Application.targetFrameRate = TargetFramerate;

            return this;
        }


        [MapsInheritingFrom( typeof( SettingsPage_Graphics ) )]
        public static SerializationMapping SettingsPage_GraphicsMapping()
        {
            return new MemberwiseSerializationMapping<SettingsPage_Graphics>()
                .WithMember( "target_framerate", o => o.TargetFramerate );
        }
    }
}