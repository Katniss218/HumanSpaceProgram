using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace HSP.UI.Canvases
{
    public sealed class UIStaticCanvas : UICanvas
    {


        public static new UIStaticCanvas Create( Scene scene, string id )
        {
            var x = Create<UIStaticCanvas>( scene, id );

            x.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            x.canvas.pixelPerfect = false;
            x.canvas.sortingOrder = 5;

            x.canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            x.canvasScaler.referenceResolution = new Vector2( 1920, 1080 );
            x.canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            x.canvasScaler.matchWidthOrHeight = 0.0f; // 0 - width, 1 - height
            x.canvasScaler.referencePixelsPerUnit = 50.0f;

            return x.ui;
        }
    }
}