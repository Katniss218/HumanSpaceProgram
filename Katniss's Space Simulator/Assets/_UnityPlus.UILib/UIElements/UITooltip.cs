using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    public partial class UITooltip : UIElement, IUIElementContainer, IUIElementChild
    {
        protected RectTransformTrackCursor trackerComponent;
        protected RectTransform track;
        protected Image backgroundComponent;
        public virtual RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        void LateUpdate()
        {
            var pt = new PointerEventData( EventSystem.current );
            pt.position = Input.mousePosition;

            var results = new List<RaycastResult>();

            EventSystem.current.RaycastAll( pt, results );

            foreach( var r in results )
            {
                if( (RectTransform)r.gameObject.transform == track )
                {
                    return;
                }
            }
            this.Destroy();
        }

        protected internal static T Create<T>( RectTransform track, IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite background ) where T : UITooltip
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiTooltip) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layoutInfo );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Tiled;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            RectTransformTrackCursor cursorTracker = rootGameObject.AddComponent<RectTransformTrackCursor>();
            cursorTracker.Offset = new Vector2( -5, -20 );

            uiTooltip.track = track;
            uiTooltip.trackerComponent = cursorTracker;
            uiTooltip.backgroundComponent = backgroundComponent;
            return uiTooltip;
        }
    }
}