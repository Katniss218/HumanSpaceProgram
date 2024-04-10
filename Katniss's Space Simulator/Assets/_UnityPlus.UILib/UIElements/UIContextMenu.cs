using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A context menu contains a list of items.
    /// </summary>
    public class UIContextMenu : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        protected internal RectTransformTracker trackerComponent;
        protected internal RectTransformDestroyOnLeave destroyOnLeaveComponent;
        protected internal Image backgroundComponent;
        public virtual RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        protected internal static T Create<T>( RectTransform track, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background ) where T : UIContextMenu
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiContextMenu) = UIElement.CreateUIGameObject<T>( contextMenuCanvas, $"uilib-{nameof( T )}", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            RectTransformTracker trackerComponent = rootGameObject.AddComponent<RectTransformTracker>();
            trackerComponent.Target = track;
            
            RectTransformDestroyOnLeave destroyOnLeaveComponent = rootGameObject.AddComponent<RectTransformDestroyOnLeave>();

            uiContextMenu.trackerComponent = trackerComponent;
            uiContextMenu.destroyOnLeaveComponent = destroyOnLeaveComponent;
            uiContextMenu.backgroundComponent = backgroundComponent;
            return uiContextMenu;
        }
    }
}