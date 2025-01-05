using HSP.Settings;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Settings
{
    public enum TextureScale
    {
        Full = 0,
        Half = 1,
        Quarter = 2,
        Eighth = 3
    }

    public sealed class SettingsPage_Graphics : SettingsPage<SettingsPage_Graphics>
    {
        public int HorizontalResolution { get; set; } = Screen.currentResolution.width;
        public int VerticalResolution { get; set; } = Screen.currentResolution.height;
        public FullScreenMode FullScreenMode { get; set; } = FullScreenMode.FullScreenWindow;
        public int TargetFramerate { get; set; } = 120;

        public TextureScale TextureScale { get; set; } = TextureScale.Full;
        public int PixelLightCount { get; set; } = 32;

        protected override SettingsPage_Graphics OnApply()
        {
#if !UNITY_EDITOR
            Screen.SetResolution( HorizontalResolution, VerticalResolution, FullScreenMode );
#endif
            Application.targetFrameRate = TargetFramerate;

            QualitySettings.masterTextureLimit = (int)TextureScale;
            QualitySettings.pixelLightCount = PixelLightCount;

            return this;
        }


        [MapsInheritingFrom( typeof( SettingsPage_Graphics ) )]
        public static SerializationMapping SettingsPage_GraphicsMapping()
        {
            return new MemberwiseSerializationMapping<SettingsPage_Graphics>()
                .WithMember( "horizontal_resolution", o => o.HorizontalResolution )
                .WithMember( "vertical_resolution", o => o.VerticalResolution )
                .WithMember( "full_screen_mode", o => o.FullScreenMode )
                .WithMember( "target_framerate", o => o.TargetFramerate )

                .WithMember( "texture_scale", o => o.TextureScale )
                .WithMember( "pixel_light_count", o => o.PixelLightCount );
        }
    }
}