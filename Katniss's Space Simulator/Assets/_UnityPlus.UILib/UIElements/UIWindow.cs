using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a window, which is a defined section of the canvas.
    /// </summary>
    public class UIWindow : UIElement
    {
        internal readonly UnityEngine.UI.Image backgroundComponent;

        public UIWindow( RectTransform transform, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            this.backgroundComponent = backgroundComponent;
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }
    }
}