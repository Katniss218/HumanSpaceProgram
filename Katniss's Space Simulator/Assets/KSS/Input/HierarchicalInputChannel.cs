using KSS.Core;
using System;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Input.Bindings;

namespace KSS.Input
{
	public static class HierarchicalInputChannel
	{
		// they could use namespaced IDs 🤔

		public const string VIEWPORT_PRIMARY_DOWN = "c.lmb_d";
		public const string VIEWPORT_PRIMARY = "c.lmb";
		public const string VIEWPORT_PRIMARY_UP = "c.lmb_u";

		public const string VIEWPORT_SECONDARY_DOWN = "c.rmb_d";
		public const string VIEWPORT_SECONDARY = "c.rmb";
		public const string VIEWPORT_SECONDARY_UP = "c.rmb_u";

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

		public const string GAMEPLAY_CONTROL_PITCH = "vanilla.gameplayc_rotate_x";
		public const string GAMEPLAY_CONTROL_PITCH_UP = "vanilla.gameplayc_rotate_xp";
		public const string GAMEPLAY_CONTROL_PITCH_DOWN = "vanilla.gameplayc_rotate_xn";
		public const string GAMEPLAY_CONTROL_YAW = "vanilla.gameplayc_rotate_y";
		public const string GAMEPLAY_CONTROL_YAW_LEFT = "vanilla.gameplayc_rotate_yp";
		public const string GAMEPLAY_CONTROL_YAW_RIGHT = "vanilla.gameplayc_rotate_yn";
		public const string GAMEPLAY_CONTROL_ROLL = "vanilla.gameplayc_rotate_z";
		public const string GAMEPLAY_CONTROL_ROLL_LEFT = "vanilla.gameplayc_rotate_zn";
		public const string GAMEPLAY_CONTROL_ROLL_RIGHT = "vanilla.gameplayc_rotate_zp";

		public const string GAMEPLAY_CONTROL_SEQUENCER_ADVANCE = "vanilla.gameplayc_sequencer_adv";

		public const string GAMEPLAY_TIMESCALE_INCREASE = "vanilla.gameplay_timescale_up";
		public const string GAMEPLAY_TIMESCALE_DECREASE = "vanilla.gameplay_timescale_down";

		public const string DESIGN_SAVE = "vanilla.design_save";

		public const string DESIGN_PART_ROTATE_XP = "vanilla.gameplayc_rotate_xp";
		public const string DESIGN_PART_ROTATE_XN = "vanilla.gameplayc_rotate_xn";
		public const string DESIGN_PART_ROTATE_YP = "vanilla.gameplayc_rotate_yp";
		public const string DESIGN_PART_ROTATE_YN = "vanilla.gameplayc_rotate_yn";
		public const string DESIGN_PART_ROTATE_ZP = "vanilla.gameplayc_rotate_zp";
		public const string DESIGN_PART_ROTATE_ZN = "vanilla.gameplayc_rotate_zn";

		[HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, HSPEvent.NAMESPACE_VANILLA + ".bind_input_channels" )]
		static void Event()
		{
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_PRIMARY_DOWN, new KeyDownBinding( 0, KeyCode.Mouse0 ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_PRIMARY, new KeyHoldBinding( 0, KeyCode.Mouse0 ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_PRIMARY_UP, new KeyUpBinding( 0, KeyCode.Mouse0 ) );

			HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_SECONDARY_DOWN, new KeyDownBinding( 0, KeyCode.Mouse1 ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_SECONDARY, new KeyHoldBinding( 0, KeyCode.Mouse1 ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.VIEWPORT_SECONDARY_UP, new KeyUpBinding( 0, KeyCode.Mouse1 ) );

			HierarchicalInputManager.BindInput( HierarchicalInputChannel.COMMON_ESCAPE, new KeyUpBinding( 0, KeyCode.Escape ) );

			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MIN, new KeyDownBinding( 0, KeyCode.X ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_MAX, new KeyDownBinding( 0, KeyCode.Z ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_UP, new KeyDownBinding( 0, KeyCode.LeftShift ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_THROTTLE_DOWN, new KeyDownBinding( 0, KeyCode.LeftControl ) );

			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_FORWARD, new KeyDownBinding( 1, KeyCode.H ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_BACKWARD, new KeyDownBinding( -1, KeyCode.N ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_LEFT, new KeyDownBinding( -1, KeyCode.J ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_RIGHT, new KeyDownBinding( 1, KeyCode.L ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_UP, new KeyDownBinding( -1, KeyCode.K ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_TRANSLATE_DOWN, new KeyDownBinding( 1, KeyCode.I ) );

			// When looking from positive towards negative (along the axis), `1` is counterclockwise, `-1` is clockwise.
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH, new AxisBinding( new KeyHoldBinding( -1, KeyCode.S ), new KeyHoldBinding( 1, KeyCode.W ) ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_UP, new KeyDownBinding( -1, KeyCode.S ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_PITCH_DOWN, new KeyDownBinding( 1, KeyCode.W ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW, new AxisBinding( new KeyHoldBinding( 1, KeyCode.A ), new KeyHoldBinding( -1, KeyCode.D ) ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_LEFT, new KeyDownBinding( 1, KeyCode.A ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_YAW_RIGHT, new KeyDownBinding( -1, KeyCode.D ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL, new AxisBinding( new KeyHoldBinding( -1, KeyCode.Q ), new KeyHoldBinding( 1, KeyCode.E ) ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_LEFT, new KeyDownBinding( -1, KeyCode.Q ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_ROLL_RIGHT, new KeyDownBinding( 1, KeyCode.E ) );

			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_CONTROL_SEQUENCER_ADVANCE, new KeyDownBinding( 0, KeyCode.Space ) );

			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_INCREASE, new KeyDownBinding( 1, KeyCode.Period ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.GAMEPLAY_TIMESCALE_DECREASE, new KeyDownBinding( -1, KeyCode.Comma ) );

			HierarchicalInputManager.BindInput( HierarchicalInputChannel.DESIGN_SAVE, new MultipleKeyDownBinding( 0, KeyCode.LeftControl, KeyCode.S ) );

			HierarchicalInputManager.BindInput( HierarchicalInputChannel.DESIGN_PART_ROTATE_XP, new KeyDownBinding( 1, KeyCode.W ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.DESIGN_PART_ROTATE_XN, new KeyDownBinding( -1, KeyCode.S ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.DESIGN_PART_ROTATE_YP, new KeyDownBinding( 1, KeyCode.D ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.DESIGN_PART_ROTATE_YN, new KeyDownBinding( -1, KeyCode.A ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.DESIGN_PART_ROTATE_ZP, new KeyDownBinding( 1, KeyCode.Q ) );
			HierarchicalInputManager.BindInput( HierarchicalInputChannel.DESIGN_PART_ROTATE_ZN, new KeyDownBinding( -1, KeyCode.E ) );
		}
	}
}