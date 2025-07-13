
namespace HSP.Vanilla
{
    public static class InputChannel
    {
        public const string ESCAPE = "vanilla.esc";
        public const string TOGGLE_UI = "vanilla.toggle_ui";
        public const string SCREENSHOT = "vanilla.screenshot";

        // universal - pick tool, construct tool, etc
        public const string CONSTRUCT_PART_ROTATE_XP = "vanilla.construct_rotate_xp";
        public const string CONSTRUCT_PART_ROTATE_XN = "vanilla.construct_rotate_xn";
        public const string CONSTRUCT_PART_ROTATE_YP = "vanilla.construct_rotate_yp";
        public const string CONSTRUCT_PART_ROTATE_YN = "vanilla.construct_rotate_yn";
        public const string CONSTRUCT_PART_ROTATE_ZP = "vanilla.construct_rotate_zp";
        public const string CONSTRUCT_PART_ROTATE_ZN = "vanilla.construct_rotate_zn";
        public const string CONSTRUCT_IGNORE_SURFACE_ATTACH = "vanilla.construct_ignore_srf";

        // control system related
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

        // gameplay scene
        public const string GAMEPLAY_TIMESCALE_INCREASE = "vanilla.gameplay_timescale_up";
        public const string GAMEPLAY_TIMESCALE_DECREASE = "vanilla.gameplay_timescale_down";
        public const string GAMEPLAY_TOGGLE_MAP_VIEW = "vanilla.gameplay_map_toggle";

        // design scene
        public const string DESIGN_SAVE = "vanilla.design_save";
    }
}
