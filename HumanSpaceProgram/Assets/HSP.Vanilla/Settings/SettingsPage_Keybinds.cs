using HSP.Input;
using HSP.Settings;
using System;
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
            return this;
        }

        private static void BindOrUnbind( string channel, IInputBinding binding )
        {
            if( binding == null )
            {
                throw new NotImplementedException();
#warning TODO - implement this method.
                // HierarchicalInputManager.UnbindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH );
            }
            else
            {
                HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH, binding );
            }
        }


        [MapsInheritingFrom( typeof( SettingsPage_Keybinds ) )]
        public static SerializationMapping SettingsPage_KeybindsMapping()
        {
            return new MemberwiseSerializationMapping<SettingsPage_Keybinds>()
                .WithMember( "control_pitch", o => o.ControlPitch );
        }
    }
}