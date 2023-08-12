using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnityPlus.UILib.UIElements
{
    public class UIButton : UIElement
    {
        internal readonly UnityEngine.UI.Button buttonComponent;
        internal readonly UnityEngine.UI.Image backgroundComponent;

        public UIButton( RectTransform transform, UnityEngine.UI.Button buttonComponent, UnityEngine.UI.Image backgroundComponent ) : base( transform )
        {
            this.buttonComponent = buttonComponent;
            this.backgroundComponent = backgroundComponent;
        }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public UnityEvent onClick => buttonComponent.onClick;
    }
}