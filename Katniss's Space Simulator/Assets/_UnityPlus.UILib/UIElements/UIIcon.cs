using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a simple icon UI element.
    /// </summary>
    public class UIIcon : UIElement
    {
        internal readonly UnityEngine.UI.Image imageComponent;

        public UIIcon( RectTransform transform, UnityEngine.UI.Image imageComponent ) : base( transform )
        {
            this.imageComponent = imageComponent;
        }

        public Sprite Sprite { get => imageComponent.sprite; set => imageComponent.sprite = value; }
    }
}