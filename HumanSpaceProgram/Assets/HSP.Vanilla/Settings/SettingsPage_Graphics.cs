using HSP.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            {
                ("target_framerate", new Member<SettingsPage_Graphics, int>( o => o.TargetFramerate )),
            };
        }
    }
}