using HSP.Input;
using HSP.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Input.Bindings;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Settings
{
    public sealed class SettingsPage_Keybinds : SettingsPage<SettingsPage_Keybinds>
    {
        public IInputBinding ControlPitch { get; set; } = new AxisBinding( new KeyHoldBinding( 1, KeyCode.S ), new KeyHoldBinding( -1, KeyCode.W ) );

        protected override SettingsPage_Keybinds OnApply()
        {
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_SEQUENCER_ADVANCE, ControlPitch );
            Debug.Log( ((KeyDownBinding)ControlPitch).Key );
            return this;
        }


        [MapsInheritingFrom( typeof( SettingsPage_Keybinds ) )]
        public static SerializationMapping SettingsPage_KeybindsMapping()
        {
            return new MemberwiseSerializationMapping<SettingsPage_Keybinds>()
            {
                ("control_pitch", new Member<SettingsPage_Keybinds, IInputBinding>( o => o.ControlPitch )),
            };
        }
    }
}