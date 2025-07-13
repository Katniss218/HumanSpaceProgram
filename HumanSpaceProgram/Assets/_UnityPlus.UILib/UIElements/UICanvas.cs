using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A component that identifies a given canvas. Works in tandem with the <see cref="CanvasManager"/>.
    /// </summary>
    [RequireComponent( typeof( Canvas ) )]
    public class UICanvas : UIElement, IUIElementContainer, IUILayoutDriven
    {
        public RectTransform contents => base.rectTransform;

        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        /// <summary>
        /// A unique identifier of the canvas. Should be unique among all canvases in all concurrently loaded scenes.
        /// </summary>
        [field: SerializeField]
        public string ID { get; private set; } = null;

        public static UICanvas Create( Scene scene, string id )
        {
            var x = Create<UICanvas>( scene, id );

            x.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            x.canvas.pixelPerfect = false;
            x.canvas.sortingOrder = 95;

            x.canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            x.canvasScaler.referenceResolution = new Vector2( 1920, 1080 );
            x.canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            x.canvasScaler.matchWidthOrHeight = 0.0f; // 0 - width, 1 - height
            x.canvasScaler.referencePixelsPerUnit = 50.0f;

            return x.ui;
        }

        /// <summary>
        /// Creates a dummy canvas and returns it along with the Unity components backing it.
        /// </summary>
        /// <param name="scene">The scene in which the canvas will be placed.</param>
        /// <param name="id">The unique ID of the canvas.</param>
        protected internal static (T ui, Canvas canvas, CanvasScaler canvasScaler) Create<T>( Scene scene, string id ) where T : UICanvas
        {
            GameObject canvasObject = new GameObject( $"Canvas - {typeof( T ).Name}, '{id}'" );
            SceneManager.MoveGameObjectToScene( canvasObject, scene );

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            GraphicRaycaster graphicRaycaster = canvasObject.AddComponent<GraphicRaycaster>();

            T uiCanvas = canvasObject.AddComponent<T>();
            uiCanvas.ID = id;

            return (uiCanvas, canvas, canvasScaler);
        }
    }
}