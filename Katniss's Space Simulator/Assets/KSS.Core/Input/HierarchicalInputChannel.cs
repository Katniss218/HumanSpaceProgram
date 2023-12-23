using System;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Input.Bindings;

namespace KSS.Core.Input
{
    public static class HierarchicalInputChannel
    {
        // they could use namespaced IDs 🤔

        public const string VIEWPORT_LEFT_MOUSE_DOWN = "c.lmb_d";
        public const string VIEWPORT_LEFT_MOUSE = "c.lmb";
        public const string VIEWPORT_LEFT_MOUSE_UP = "c.lmb_u";

        public const string VIEWPORT_RIGHT_MOUSE_DOWN = "c.rmb_d";
        public const string VIEWPORT_RIGHT_MOUSE = "c.rmb";
        public const string VIEWPORT_RIGHT_MOUSE_UP = "c.rmb_u";

        public const string VIEWPORT_MIDDLE_MOUSE_DOWN = "c.mmb_d";
        public const string VIEWPORT_MIDDLE_MOUSE = "c.mmb";
        public const string VIEWPORT_MIDDLE_MOUSE_UP = "c.mmb_u";

        public const string COMMON_ESCAPE = "c.esc";

        public const string GAMEPLAY_CONTROL_THROTTLE_MIN = "vanilla.gameplayc_throttle_min";
        public const string GAMEPLAY_CONTROL_THROTTLE_MAX = "vanilla.gameplayc_throttle_max";
        public const string GAMEPLAY_CONTROL_THROTTLE_UP = "vanilla.gameplayc_throttle_up";
        public const string GAMEPLAY_CONTROL_THROTTLE_DOWN = "vanilla.gameplayc_throttle_down";

        public const string GAMEPLAY_CONTROL_TRANSLATE_FORWARD = "vanilla.gameplayc_translate_zp";
        public const string GAMEPLAY_CONTROL_TRANSLATE_BACKWARD = "vanilla.gameplayc_translate_zn";
        public const string GAMEPLAY_CONTROL_TRANSLATE_LEFT = "vanilla.gameplayc_translate_xn";
        public const string GAMEPLAY_CONTROL_TRANSLATE_RIGHT = "vanilla.gameplayc_translate_xp";
        public const string GAMEPLAY_CONTROL_TRANSLATE_UP = "vanilla.gameplayc_translate_yp";
        public const string GAMEPLAY_CONTROL_TRANSLATE_DOWN = "vanilla.gameplayc_translate_yn";

        public const string GAMEPLAY_CONTROL_PITCH_UP = "vanilla.gameplayc_rotate_xp";
        public const string GAMEPLAY_CONTROL_PITCH_DOWN = "vanilla.gameplayc_rotate_xn";
        public const string GAMEPLAY_CONTROL_ROLL_LEFT = "vanilla.gameplayc_rotate_zn";
        public const string GAMEPLAY_CONTROL_ROLL_RIGHT = "vanilla.gameplayc_rotate_zp";
        public const string GAMEPLAY_CONTROL_YAW_LEFT = "vanilla.gameplayc_rotate_yp";
        public const string GAMEPLAY_CONTROL_YAW_RIGHT = "vanilla.gameplayc_rotate_yn";

        public const string GAMEPLAY_CONTROL_SEQUENCER_ADVANCE = "vanilla.gameplayc_sequencer_adv";

        public const string GAMEPLAY_TIMESCALE_INCREASE = "vanilla.gameplay_timescale_up";
        public const string GAMEPLAY_TIMESCALE_DECREASE = "vanilla.gameplay_timescale_down";

        public const string DESIGN_SAVE = "vanilla.design_save";

        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_VANILLA + ".bind_input_channels" )]
        static void Event()
        {
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_LEFT_MOUSE_DOWN, new KeyDownBinding( KeyCode.Mouse0 ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_LEFT_MOUSE, new KeyHoldBinding( KeyCode.Mouse0 ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_LEFT_MOUSE_UP, new KeyUpBinding( KeyCode.Mouse0 ) );

            HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_RIGHT_MOUSE_DOWN, new KeyDownBinding( KeyCode.Mouse1 ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_RIGHT_MOUSE, new KeyHoldBinding( KeyCode.Mouse1 ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_RIGHT_MOUSE_UP, new KeyUpBinding( KeyCode.Mouse1 ) );

            HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_MIDDLE_MOUSE_DOWN, new KeyDownBinding( KeyCode.Mouse2 ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_MIDDLE_MOUSE, new KeyHoldBinding( KeyCode.Mouse2 ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_MIDDLE_MOUSE_UP, new KeyUpBinding( KeyCode.Mouse2 ) );

            HierarchicalInputManager.BindInput( HierarchicalInputChannel.COMMON_ESCAPE, new KeyUpBinding( KeyCode.Escape ) );

            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, new KeyDownBinding( KeyCode.X ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, new KeyDownBinding( KeyCode.Z ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_UP, new KeyDownBinding( KeyCode.LeftShift ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_UP, new KeyDownBinding( KeyCode.LeftControl ) );

            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_FORWARD, new KeyDownBinding( KeyCode.H ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_BACKWARD, new KeyDownBinding( KeyCode.N ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_LEFT, new KeyDownBinding( KeyCode.J ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_RIGHT, new KeyDownBinding( KeyCode.L ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_UP, new KeyDownBinding( KeyCode.K ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_DOWN, new KeyDownBinding( KeyCode.I ) );

            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_UP, new KeyDownBinding( KeyCode.S ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_DOWN, new KeyDownBinding( KeyCode.W ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_LEFT, new KeyDownBinding( KeyCode.A ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_RIGHT, new KeyDownBinding( KeyCode.D ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_LEFT, new KeyDownBinding( KeyCode.Q ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_RIGHT, new KeyDownBinding( KeyCode.E ) );

            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_SEQUENCER_ADVANCE, new KeyDownBinding( KeyCode.Space ) );

            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_INCREASE, new KeyDownBinding( KeyCode.Period ) );
            HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_DECREASE, new KeyDownBinding( KeyCode.Comma ) );

            HierarchicalInputManager.BindInput( HierarchicalInputChannel.DESIGN_SAVE, new MultipleKeyDownBinding( KeyCode.LeftControl, KeyCode.S ) );
        }
    }
}