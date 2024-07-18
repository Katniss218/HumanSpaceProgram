using UnityEngine;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.UI
{
    public class UITextReadout_FPS : UIText
    {
        RollingStatistics _fpsAvg = new RollingStatistics( 128 );

        private static float GetFps()
        {
            return 1.0f / UnityEngine.Time.unscaledDeltaTime; // unscaled so timewarp / pausing doesn't fuck with it.
        }

        void Update()
        {
            _fpsAvg.AddSample( GetFps() );

            this.Text = $"FPS: {Mathf.CeilToInt( _fpsAvg.GetMean() )}";
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UITextReadout_FPS
        {
            return UIText.Create<T>( parent, layout, "<placeholder_text>" );
        }
    }

    public static class UITextReadout_FPS_Ex
    {
        public static UITextReadout_FPS AddTextReadout_FPS( this IUIElementContainer parent, UILayoutInfo layout )
        {
            return UITextReadout_FPS.Create<UITextReadout_FPS>( parent, layout );
        }
    }
}