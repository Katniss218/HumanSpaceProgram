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
    public class RectTransformDragResize : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// Determines which point on the window will move with the mouse.
        /// </summary>
        public enum ResizeDragMode : byte
        {
            None = 0,

            TopLeft, Top, TopRight,
            Left, /*no center*/ Right,
            BottomLeft, Bottom, BottomRight
        }

        /// <summary>
        /// The width (in pixels) of the draggable area.
        /// </summary>
        public float Padding { get; set; } = 16f;

        [field: SerializeField]
        public PointerEventData.InputButton MouseButton { get; set; } = PointerEventData.InputButton.Left;

        private ResizeDragMode _dragState = ResizeDragMode.None;

        private Vector2 _initialPosition = Vector2.zero;
        private Vector2 _initialSize = Vector2.zero;

        public Action OnBeginDragging { get; set; }
        public Action OnDragging { get; set; }
        public Action OnEndDragging { get; set; }

        public void OnBeginDrag( PointerEventData eventData )
        {
            if( eventData.button != MouseButton )
            {
                return;
            }

            RectTransform rectTransform = (RectTransform)this.transform;

            if( rectTransform.FillsWidth() || rectTransform.FillsHeight() )
            {
                Debug.LogWarning( $"{nameof( RectTransformDragResize )} - Tried to resize a RectTransform that fills either height or width." );
                return;
            }

            Vector2 localPos = rectTransform.InverseTransformPoint( eventData.pressPosition );

            Rect rect = rectTransform.rect;
            _initialSize = rect.size;
            _initialPosition = rectTransform.anchoredPosition;

            float leftBoundary = rect.xMin + Padding;
            float rightBoundary = rect.xMax - Padding;
            float topBoundary = rect.yMax - Padding;
            float bottomBoundary = rect.yMin + Padding;

            // No need to test within the rect, since OnBeginDrag won't fire unless the cursor click starts within the rect.

            if( localPos.x < leftBoundary )
            {
                if( localPos.y > topBoundary )
                {
                    _dragState = ResizeDragMode.TopLeft;
                }
                else if( localPos.y < bottomBoundary )
                {
                    _dragState = ResizeDragMode.BottomLeft;
                }
                else
                {
                    _dragState = ResizeDragMode.Left;
                }
            }
            else if( localPos.x > rightBoundary )
            {
                if( localPos.y > topBoundary )
                {
                    _dragState = ResizeDragMode.TopRight;
                }
                else if( localPos.y < bottomBoundary )
                {
                    _dragState = ResizeDragMode.BottomRight;
                }
                else
                {
                    _dragState = ResizeDragMode.Right;
                }
            }
            else
            {
                if( localPos.y > topBoundary )
                {
                    _dragState = ResizeDragMode.Top;
                }
                else if( localPos.y < bottomBoundary )
                {
                    _dragState = ResizeDragMode.Bottom;
                }
                else
                {
                    _dragState = ResizeDragMode.None; // center, but there is no center drag.
                }
            }

            if( _dragState != ResizeDragMode.None )
            {
                OnBeginDragging?.Invoke();
            }
        }

        public void OnDrag( PointerEventData eventData )
        {
            RectTransform rectTransform = (RectTransform)this.transform;

            Vector2 cursorOffset = eventData.position - eventData.pressPosition;

            switch( _dragState )
            {
                case ResizeDragMode.TopLeft:
                {
                    rectTransform.sizeDelta = _initialSize + new Vector2( -cursorOffset.x, cursorOffset.y );
                    rectTransform.anchoredPosition = _initialPosition + new Vector2( cursorOffset.x / 2, cursorOffset.y / 2 );
                    break;
                }
                case ResizeDragMode.Top:
                {
                    rectTransform.sizeDelta = _initialSize + new Vector2( 0, cursorOffset.y );
                    rectTransform.anchoredPosition = _initialPosition + new Vector2( 0, cursorOffset.y / 2 );
                    break;
                }
                case ResizeDragMode.TopRight:
                {
                    rectTransform.sizeDelta = _initialSize + new Vector2( cursorOffset.x, cursorOffset.y );
                    rectTransform.anchoredPosition = _initialPosition + new Vector2( cursorOffset.x / 2, cursorOffset.y / 2 );
                    break;
                }
                case ResizeDragMode.Left:
                {
                    rectTransform.sizeDelta = _initialSize + new Vector2( -cursorOffset.x, 0 );
                    rectTransform.anchoredPosition = _initialPosition + new Vector2( cursorOffset.x / 2, 0 );
                    break;
                }
                case ResizeDragMode.Right:
                {
                    rectTransform.sizeDelta = _initialSize + new Vector2( cursorOffset.x, 0 );
                    rectTransform.anchoredPosition = _initialPosition + new Vector2( cursorOffset.x / 2, 0 );
                    break;
                }
                case ResizeDragMode.BottomLeft:
                {
                    rectTransform.sizeDelta = _initialSize + new Vector2( -cursorOffset.x, -cursorOffset.y );
                    rectTransform.anchoredPosition = _initialPosition + new Vector2( cursorOffset.x / 2, cursorOffset.y / 2 );
                    break;
                }
                case ResizeDragMode.Bottom:
                {
                    rectTransform.sizeDelta = _initialSize + new Vector2( 0, -cursorOffset.y );
                    rectTransform.anchoredPosition = _initialPosition + new Vector2( 0, cursorOffset.y / 2 );
                    break;
                }
                case ResizeDragMode.BottomRight:
                {
                    rectTransform.sizeDelta = _initialSize + new Vector2( cursorOffset.x, -cursorOffset.y );
                    rectTransform.anchoredPosition = _initialPosition + new Vector2( cursorOffset.x / 2, cursorOffset.y / 2 );
                    break;
                }
                default:
                {
                    // For None case, no resizing logic is needed
                    break;
                }
            }

            float min = 2 * Padding;
            Vector2 size = rectTransform.sizeDelta;
            size.x = size.x < min ? min : size.x;
            size.y = size.y < min ? min : size.y;
            rectTransform.sizeDelta = size;

            if( _dragState != ResizeDragMode.None )
            {
                OnDragging?.Invoke();
            }
        }

        public void OnEndDrag( PointerEventData eventData )
        {
            if( eventData.button != MouseButton )
            {
                return;
            }

            if( _dragState != ResizeDragMode.None )
            {
                OnEndDragging?.Invoke();
                _dragState = ResizeDragMode.None;
            }
        }
    }
}