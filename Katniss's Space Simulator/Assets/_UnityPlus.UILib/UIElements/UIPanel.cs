using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a section of the canvas, or of a different UI element.
    /// </summary>
    public class UIPanel : UIElement
    {
        internal readonly UnityEngine.UI.Image backgroundComponent;

        public UIPanel( RectTransform transform, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            this.backgroundComponent = backgroundComponent;
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }
    }
}