using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a section of the canvas, or of a different UI element.
    /// </summary>
    public sealed class UIMask : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        internal Image backgroundComponent;
        public RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        public Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public static UIMask Create( IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite mask )
        {
            if( mask == null )
            {
                throw new ArgumentNullException( nameof( mask ), $"Mask can't be null." );
            }

            (GameObject rootGameObject, RectTransform rootTransform, UIMask uiMask) = UIElement.CreateUIGameObject<UIMask>( parent, "uilib-mask", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = mask;
            backgroundComponent.type = Image.Type.Simple;

            uiMask.backgroundComponent = backgroundComponent;

            Mask maskC = rootGameObject.AddComponent<Mask>();
            maskC.showMaskGraphic = false;

            return uiMask;
        }
    }
}