using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityPlus.UILib
{
	/// <summary>
	/// Enables a <see cref="RectTransform"/> to be dragged around by the mouse.
	/// </summary>
	public class RectTransformDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
		[field: SerializeField]
		public RectTransform UITransform { get; set; }

		[field: SerializeField]
		public PointerEventData.InputButton MouseButton { get; set; } = PointerEventData.InputButton.Left;

		Vector2 _cursorOffset = Vector2.zero;

		public Action OnBeginDragging { get; set; }
		public Action OnDragging { get; set; }
		public Action OnEndDragging { get; set; }

        public void OnBeginDrag( PointerEventData eventData )
        {
            if( UITransform == null )
            {
                return;
            }

            if( eventData.button != MouseButton )
            {
                return;
            }

            _cursorOffset = new Vector2( UITransform.position.x, UITransform.position.y ) - eventData.position + (eventData.position - eventData.pressPosition);
            OnBeginDragging?.Invoke();
        }

        public void OnDrag( PointerEventData eventData )
        {
            if( UITransform == null )
            {
                return;
            }

                UITransform.position = new Vector2( Input.mousePosition.x, Input.mousePosition.y ) + _cursorOffset;
                OnDragging?.Invoke();
        }

        public void OnEndDrag( PointerEventData eventData )
        {
            if( UITransform == null )
            {
                return;
            }

            if( eventData.button != MouseButton )
            {
                return;
            }

            OnEndDragging?.Invoke();
        }
	}
}