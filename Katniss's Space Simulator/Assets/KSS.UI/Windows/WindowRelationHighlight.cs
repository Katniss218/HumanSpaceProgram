using KSS.Cameras;
using UnityPlus.UILib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace KSS.UI.Windows
{
    /// <summary>
    /// A script that draws a graphical relationship between a UI element, and a scene object, when the UI element is hovered over.
    /// </summary>
    public class WindowRelationHighlight : EventTrigger
    {
        /// <summary>
        /// The UI element
        /// </summary>
        [field: SerializeField]
        public RectTransform UITransform { get; set; }

        /// <summary>
        /// The scene object to relate to.
        /// </summary>
        [field: SerializeField]
        public Transform ReferenceTransform { get; set; }


        RectTransform _highlightedObject = null;

        public override void OnPointerEnter( PointerEventData eventData )
        {
            if( ReferenceTransform == null )
            {
                return;
            }
            if( _highlightedObject != null )
            {
                return;
            }

            (GameObject highlighterGO, RectTransform rt) = UIElement.CreateUIGameObject( (RectTransform)this.transform, "relation highlight", new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (10, 10) ) );
            _highlightedObject = rt;

            Image exitImage = highlighterGO.AddComponent<Image>();
            exitImage.raycastTarget = true;
            exitImage.color = Color.green;

            base.OnPointerEnter( eventData );
        }

        public override void OnPointerExit( PointerEventData eventData )
        {
            if( ReferenceTransform == null )
            {
                return;
            }
            if( _highlightedObject == null )
            {
                return;
            }

            Destroy( _highlightedObject.gameObject );
            _highlightedObject = null;

            base.OnPointerExit( eventData );
        }

        void LateUpdate()
        {
            if( ReferenceTransform == null )
            {
                return;
            }

            if( _highlightedObject != null )
            {
                Vector3 targetScreenPos = GameplayCameraController.MainCamera.WorldToScreenPoint( ReferenceTransform.position );
                targetScreenPos.z = 0.0f;

                _highlightedObject.position = targetScreenPos;
            }
        }
    }

    public static class UIRelationHighlightEx
    {
        public static UIWindow WithRelationHightlight( this UIWindow window, out WindowRelationHighlight relationHightlight )
        {
            relationHightlight = window.gameObject.AddComponent<WindowRelationHighlight>();
            relationHightlight.UITransform = window.rectTransform;

            return window;
        }
    }
}