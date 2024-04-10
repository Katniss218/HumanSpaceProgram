using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a generic window, which is a defined section of the canvas.
    /// </summary>
    public partial class UIWindow : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        protected Image backgroundComponent;
        public virtual RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        protected internal static T Create<T>( UICanvas parent, UILayoutInfo layoutInfo, Sprite background ) where T : UIWindow
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiWindow) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{nameof( T )}", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            uiWindow.backgroundComponent = backgroundComponent;
            return uiWindow;
        }
    }
}