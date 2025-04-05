using HSP.Settings;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Input.Bindings;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Settings
{
    public sealed class SettingsPage_Keybinds : SettingsPage<SettingsPage_Keybinds>, IGameSettingsPage
    {
        public IInputBinding CommonPrimaryDown { get; set; } = new KeyDownBinding( 0, KeyCode.Mouse0 );
        public IInputBinding CommonPrimary { get; set; } = new KeyHoldBinding( 0, KeyCode.Mouse0 );
        public IInputBinding CommonPrimaryUp { get; set; } = new KeyUpBinding( 0, KeyCode.Mouse0 );
        public IInputBinding CommonSecondaryDown { get; set; } = new KeyDownBinding( 0, KeyCode.Mouse1 );
        public IInputBinding CommonSecondary { get; set; } = new KeyHoldBinding( 0, KeyCode.Mouse1 );
        public IInputBinding CommonSecondaryUp { get; set; } = new KeyUpBinding( 0, KeyCode.Mouse1 );

        public IInputBinding Escape { get; set; } = new KeyDownBinding( 0, KeyCode.Escape );
        public IInputBinding ToggleUI { get; set; } = new KeyDownBinding( 0, KeyCode.F1 );
        public IInputBinding Screenshot { get; set; } = new KeyDownBinding( 0, KeyCode.F2 );

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

        public KeyCode GameplayControlDefaultSequencerKey { get; set; } = KeyCode.Space;

        public IInputBinding GameplayTimescaleIncrease { get; set; } = new KeyDownBinding( 1, KeyCode.Period );
        public IInputBinding GameplayTimescaleDecrease { get; set; } = new KeyDownBinding( -1, KeyCode.Comma );

        public IInputBinding DesignSave { get; set; } = new MultipleKeyDownBinding( 0, KeyCode.LeftControl, KeyCode.S );

        public IInputBinding ConstructPartRotateXn { get; set; } = new KeyDownBinding( -1, KeyCode.S );
        public IInputBinding ConstructPartRotateXp { get; set; } = new KeyDownBinding( 1, KeyCode.W );
        public IInputBinding ConstructPartRotateYn { get; set; } = new KeyDownBinding( -1, KeyCode.A );
        public IInputBinding ConstructPartRotateYp { get; set; } = new KeyDownBinding( 1, KeyCode.D );
        public IInputBinding ConstructPartRotateZn { get; set; } = new KeyDownBinding( -1, KeyCode.E );
        public IInputBinding ConstructPartRotateZp { get; set; } = new KeyDownBinding( 1, KeyCode.Q );

        protected override SettingsPage_Keybinds OnApply()
        {
            BindOrUnbind( Input.InputChannel.PRIMARY_DOWN, CommonPrimaryDown );
            BindOrUnbind( Input.InputChannel.PRIMARY, CommonPrimary );
            BindOrUnbind( Input.InputChannel.PRIMARY_UP, CommonPrimaryUp );
            BindOrUnbind( Input.InputChannel.SECONDARY_DOWN, CommonSecondaryDown );
            BindOrUnbind( Input.InputChannel.SECONDARY, CommonSecondary );
            BindOrUnbind( Input.InputChannel.SECONDARY_UP, CommonSecondaryUp );

            BindOrUnbind( InputChannel.ESCAPE, Escape );
            BindOrUnbind( InputChannel.TOGGLE_UI, ToggleUI );
            BindOrUnbind( InputChannel.SCREENSHOT, Screenshot );

            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, GameplayControlThrottleMin );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, GameplayControlThrottleMax );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_THROTTLE_UP, GameplayControlThrottleUp );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_THROTTLE_DOWN, GameplayControlThrottleDown );

            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_TRANSLATE_FORWARD, GameplayControlTranslateForward );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_TRANSLATE_BACKWARD, GameplayControlTranslateBackward );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_TRANSLATE_LEFT, GameplayControlTranslateLeft );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_TRANSLATE_RIGHT, GameplayControlTranslateRight );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_TRANSLATE_UP, GameplayControlTranslateUp );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_TRANSLATE_DOWN, GameplayControlTranslateDown );

            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_PITCH, GameplayControlPitch );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_PITCH_UP, GameplayControlPitchUp );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_PITCH_DOWN, GameplayControlPitchDown );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_YAW, GameplayControlYaw );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_YAW_LEFT, GameplayControlYawLeft );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_YAW_RIGHT, GameplayControlYawRight );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_ROLL, GameplayControlRoll );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_ROLL_LEFT, GameplayControlRollLeft );
            BindOrUnbind( InputChannel.GAMEPLAY_CONTROL_ROLL_RIGHT, GameplayControlRollRight );

            BindOrUnbind( InputChannel.GAMEPLAY_TIMESCALE_INCREASE, GameplayTimescaleIncrease );
            BindOrUnbind( InputChannel.GAMEPLAY_TIMESCALE_DECREASE, GameplayTimescaleDecrease );

            BindOrUnbind( InputChannel.DESIGN_SAVE, DesignSave );

            BindOrUnbind( InputChannel.CONSTRUCT_PART_ROTATE_XN, ConstructPartRotateXn );
            BindOrUnbind( InputChannel.CONSTRUCT_PART_ROTATE_XP, ConstructPartRotateXp );
            BindOrUnbind( InputChannel.CONSTRUCT_PART_ROTATE_YN, ConstructPartRotateYn );
            BindOrUnbind( InputChannel.CONSTRUCT_PART_ROTATE_YP, ConstructPartRotateYp );
            BindOrUnbind( InputChannel.CONSTRUCT_PART_ROTATE_ZN, ConstructPartRotateZn );
            BindOrUnbind( InputChannel.CONSTRUCT_PART_ROTATE_ZP, ConstructPartRotateZp );
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
                .WithMember( "common_primary_down", o => o.CommonPrimaryDown )
                .WithMember( "common_primary", o => o.CommonPrimary )
                .WithMember( "common_primary_up", o => o.CommonPrimaryUp )
                .WithMember( "common_secondary_down", o => o.CommonSecondaryDown )
                .WithMember( "common_secondary", o => o.CommonSecondary )
                .WithMember( "common_secondary_up", o => o.CommonSecondaryUp )

                .WithMember( "escape", o => o.Escape )
                .WithMember( "toggle_ui", o => o.ToggleUI )
                .WithMember( "screenshot", o => o.Screenshot )

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

                .WithMember( "gameplay_control_default_sequencer_key", o => o.GameplayControlDefaultSequencerKey )

                .WithMember( "gameplay_timescale_increase", o => o.GameplayTimescaleIncrease )
                .WithMember( "gameplay_timescale_decrease", o => o.GameplayTimescaleDecrease )

                .WithMember( "design_save", o => o.DesignSave )

                .WithMember( "construct_part_rotate_xn", o => o.ConstructPartRotateXn )
                .WithMember( "construct_part_rotate_xp", o => o.ConstructPartRotateXp )
                .WithMember( "construct_part_rotate_yn", o => o.ConstructPartRotateYn )
                .WithMember( "construct_part_rotate_yp", o => o.ConstructPartRotateYp )
                .WithMember( "construct_part_rotate_zn", o => o.ConstructPartRotateZn )
                .WithMember( "construct_part_rotate_zp", o => o.ConstructPartRotateZp );
        }
    }
}