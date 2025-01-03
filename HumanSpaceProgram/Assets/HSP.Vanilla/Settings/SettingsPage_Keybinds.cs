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
        public IInputBinding ViewportPrimaryDown { get; set; } = new KeyDownBinding( 0, KeyCode.Mouse0 );
        public IInputBinding ViewportPrimary { get; set; } = new KeyHoldBinding( 0, KeyCode.Mouse0 );
        public IInputBinding ViewportPrimaryUp { get; set; } = new KeyUpBinding( 0, KeyCode.Mouse0 );
        public IInputBinding ViewportSecondaryDown { get; set; } = new KeyDownBinding( 0, KeyCode.Mouse1 );
        public IInputBinding ViewportSecondary { get; set; } = new KeyHoldBinding( 0, KeyCode.Mouse1 );
        public IInputBinding ViewportSecondaryUp { get; set; } = new KeyUpBinding( 0, KeyCode.Mouse1 );

        public IInputBinding CommonEscape { get; set; } = new KeyDownBinding( 0, KeyCode.Escape );
        public IInputBinding CommonToggleUI { get; set; } = new KeyDownBinding( 0, KeyCode.F1 );
        public IInputBinding CommonScreenshot { get; set; } = new KeyDownBinding( 0, KeyCode.F2 );

        public IInputBinding GameplayControlThrottleMin { get; set; } = new KeyDownBinding( 1, KeyCode.X );
        public IInputBinding GameplayControlThrottleMax { get; set; } = new KeyDownBinding( -1, KeyCode.Z );
        public IInputBinding GameplayControlThrottleUp { get; set; } = new KeyDownBinding( 1, KeyCode.LeftShift );
        public IInputBinding GameplayControlThrottleDown { get; set; } = new KeyDownBinding( -1, KeyCode.LeftControl );

        public IInputBinding GameplayControlTranslateForward { get; set; } = new KeyDownBinding( 1, KeyCode.H );
        public IInputBinding GameplayControlTranslateBackward { get; set; } = new KeyDownBinding( -1, KeyCode.N );
        public IInputBinding GameplayControlTranslateLeft { get; set; } = new KeyDownBinding( -1, KeyCode.J );
        public IInputBinding GameplayControlTranslateRight { get; set; } = new KeyDownBinding( 1, KeyCode.L );
        public IInputBinding GameplayControlTranslateUp { get; set; } = new KeyDownBinding( -1, KeyCode.K );
        public IInputBinding GameplayControlTranslateDown { get; set; } = new KeyDownBinding( 1, KeyCode.I );

        public IInputBinding GameplayControlPitch { get; set; } = new AxisBinding( new KeyHoldBinding( 1, KeyCode.S ), new KeyHoldBinding( -1, KeyCode.W ) );
        public IInputBinding GameplayControlPitchUp { get; set; } = new KeyDownBinding( 1, KeyCode.S );
        public IInputBinding GameplayControlPitchDown { get; set; } = new KeyDownBinding( -1, KeyCode.W );
        public IInputBinding GameplayControlYaw { get; set; } = new AxisBinding( new KeyHoldBinding( -1, KeyCode.A ), new KeyHoldBinding( 1, KeyCode.D ) );
        public IInputBinding GameplayControlYawLeft { get; set; } = new KeyDownBinding( -1, KeyCode.A );
        public IInputBinding GameplayControlYawRight { get; set; } = new KeyDownBinding( 1, KeyCode.D );
        public IInputBinding GameplayControlRoll { get; set; } = new AxisBinding( new KeyHoldBinding( 1, KeyCode.Q ), new KeyHoldBinding( -1, KeyCode.E ) );
        public IInputBinding GameplayControlRollLeft { get; set; } = new KeyDownBinding( 1, KeyCode.Q );
        public IInputBinding GameplayControlRollRight { get; set; } = new KeyDownBinding( -1, KeyCode.E );

        public IInputBinding GameplayTimescaleIncrease { get; set; } = new KeyDownBinding( 1, KeyCode.Period );
        public IInputBinding GameplayTimescaleDecrease { get; set; } = new KeyDownBinding( -1, KeyCode.Comma );

        protected override SettingsPage_Keybinds OnApply()
        {
            BindOrUnbind( HierarchicalInputChannel.VIEWPORT_PRIMARY_DOWN, ViewportPrimaryDown );
            BindOrUnbind( HierarchicalInputChannel.VIEWPORT_PRIMARY, ViewportPrimary );
            BindOrUnbind( HierarchicalInputChannel.VIEWPORT_PRIMARY_UP, ViewportPrimaryUp );
            BindOrUnbind( HierarchicalInputChannel.VIEWPORT_SECONDARY_DOWN, ViewportSecondaryDown );
            BindOrUnbind( HierarchicalInputChannel.VIEWPORT_SECONDARY, ViewportSecondary );
            BindOrUnbind( HierarchicalInputChannel.VIEWPORT_SECONDARY_UP, ViewportSecondaryUp );

            BindOrUnbind( HierarchicalInputChannel.COMMON_ESCAPE, CommonEscape );
            BindOrUnbind( HierarchicalInputChannel.COMMON_TOGGLE_UI, CommonToggleUI );
            BindOrUnbind( HierarchicalInputChannel.COMMON_SCREENSHOT, CommonScreenshot );

            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, GameplayControlThrottleMin );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, GameplayControlThrottleMax );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_UP, GameplayControlThrottleUp );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_DOWN, GameplayControlThrottleDown );

            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_FORWARD, GameplayControlTranslateForward );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_BACKWARD, GameplayControlTranslateBackward );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_LEFT, GameplayControlTranslateLeft );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_RIGHT, GameplayControlTranslateRight );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_UP, GameplayControlTranslateUp );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_DOWN, GameplayControlTranslateDown );

            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH, GameplayControlPitch );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_UP, GameplayControlPitchUp );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_DOWN, GameplayControlPitchDown );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW, GameplayControlYaw );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_LEFT, GameplayControlYawLeft );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_RIGHT, GameplayControlYawRight );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL, GameplayControlRoll );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_LEFT, GameplayControlRollLeft );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_RIGHT, GameplayControlRollRight );

            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_INCREASE, GameplayTimescaleIncrease );
            BindOrUnbind( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_DECREASE, GameplayTimescaleDecrease );
            return this;
        }

        private static void BindOrUnbind( string channel, IInputBinding binding )
        {
            if( binding == null )
            {
                HierarchicalInputManager.UnbindInput( channel );
            }
            else
            {
                HierarchicalInputManager.BindInput( channel, binding );
            }
        }


        [MapsInheritingFrom( typeof( SettingsPage_Keybinds ) )]
        public static SerializationMapping SettingsPage_KeybindsMapping()
        {
            return new MemberwiseSerializationMapping<SettingsPage_Keybinds>()
                .WithMember( "viewport_primary_down", o => o.ViewportPrimaryDown )
                .WithMember( "viewport_primary", o => o.ViewportPrimary )
                .WithMember( "viewport_primary_up", o => o.ViewportPrimaryUp )
                .WithMember( "viewport_secondary_down", o => o.ViewportSecondaryDown )
                .WithMember( "viewport_secondary", o => o.ViewportSecondary )
                .WithMember( "viewport_secondary_up", o => o.ViewportSecondaryUp )

                .WithMember( "common_escape", o => o.CommonEscape )
                .WithMember( "common_toggle_ui", o => o.CommonToggleUI )
                .WithMember( "common_screenshot", o => o.CommonScreenshot )

                .WithMember( "gameplay_control_throttle_min", o => o.GameplayControlThrottleMin )
                .WithMember( "gameplay_control_throttle_max", o => o.GameplayControlThrottleMax )
                .WithMember( "gameplay_control_throttle_up", o => o.GameplayControlThrottleUp )
                .WithMember( "gameplay_control_throttle_down", o => o.GameplayControlThrottleDown )

                .WithMember( "gameplay_control_translate_forward", o => o.GameplayControlTranslateForward )
                .WithMember( "gameplay_control_translate_backward", o => o.GameplayControlTranslateBackward )
                .WithMember( "gameplay_control_translate_left", o => o.GameplayControlTranslateLeft )
                .WithMember( "gameplay_control_translate_right", o => o.GameplayControlTranslateRight )
                .WithMember( "gameplay_control_translate_up", o => o.GameplayControlTranslateUp )
                .WithMember( "gameplay_control_translate_down", o => o.GameplayControlTranslateDown )

                .WithMember( "gameplay_control_pitch", o => o.GameplayControlPitch )
                .WithMember( "gameplay_control_pitch_up", o => o.GameplayControlPitchUp )
                .WithMember( "gameplay_control_pitch_down", o => o.GameplayControlPitchDown )
                .WithMember( "gameplay_control_yaw", o => o.GameplayControlYaw )
                .WithMember( "gameplay_control_yaw_left", o => o.GameplayControlYawLeft )
                .WithMember( "gameplay_control_yaw_right", o => o.GameplayControlYawRight )
                .WithMember( "gameplay_control_roll", o => o.GameplayControlRoll )
                .WithMember( "gameplay_control_roll_left", o => o.GameplayControlRollLeft )
                .WithMember( "gameplay_control_roll_right", o => o.GameplayControlRollRight )

                .WithMember( "gameplay_timescale_increase", o => o.GameplayTimescaleIncrease )
                .WithMember( "gameplay_timescale_decrease", o => o.GameplayTimescaleDecrease );
        }
    }
}