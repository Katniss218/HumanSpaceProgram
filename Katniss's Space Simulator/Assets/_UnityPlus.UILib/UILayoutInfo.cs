using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.UILib
{
    public struct UIAnchor
    {
        public float x, y;

        /// <summary>
        /// (x = 0, y = 1)
        /// </summary>
        public static UIAnchor TopLeft => new UIAnchor() { x = 0, y = 1 };
        /// <summary>
        /// (_____, y = 1), a.k.a. TopCenter
        /// </summary>
        public static UIAnchorVertical Top => new UIAnchorVertical() { y = 1 };
        /// <summary>
        /// (x = 1, y = 1)
        /// </summary>
        public static UIAnchor TopRight => new UIAnchor() { x = 1, y = 1 };

        /// <summary>
        /// (x = 0, _____), a.k.a. MiddleLeft
        /// </summary>
        public static UIAnchorHorizontal Left => new UIAnchorHorizontal() { x = 0 };
        /// <summary>
        /// (x = 0.5, y = 0.5)
        /// </summary>
        public static UIAnchor Center => new UIAnchor() { x = 0.5f, y = 0.5f };
        /// <summary>
        /// (x = 1, _____), a.k.a. MiddleRight
        /// </summary>
        public static UIAnchorHorizontal Right => new UIAnchorHorizontal() { x = 1 };

        /// <summary>
        /// (x = 0, y = 0)
        /// </summary>
        public static UIAnchor BottomLeft => new UIAnchor() { x = 0, y = 0 };
        /// <summary>
        /// (_____, y = 0), a.k.a. BottomCenter
        /// </summary>
        public static UIAnchorVertical Bottom => new UIAnchorVertical() { y = 0 };
        /// <summary>
        /// (x = 1, y = 0)
        /// </summary>
        public static UIAnchor BottomRight => new UIAnchor() { x = 1, y = 0 };

        public static implicit operator UIAnchor( (float x, float y) pos ) => new UIAnchor()
        {
            x = pos.x,
            y = pos.y
        };

        public static explicit operator UIAnchor( Vector2 pos ) => new UIAnchor()
        {
            x = pos.x,
            y = pos.y
        };

        public static implicit operator UIAnchorHorizontal( UIAnchor anchor ) => new UIAnchorHorizontal()
        {
            x = anchor.x
        };

        public static implicit operator UIAnchorVertical( UIAnchor anchor ) => new UIAnchorVertical()
        {
            y = anchor.y
        };
    }

    public struct UIAnchorHorizontal
    {
        public float x;

        public static implicit operator UIAnchor( UIAnchorHorizontal anchor ) => new UIAnchor()
        {
            x = anchor.x,
            y = 0.5f
        };
    }

    public struct UIAnchorVertical
    {
        public float y;

        public static implicit operator UIAnchor( UIAnchorVertical anchor ) => new UIAnchor()
        {
            x = 0.5f,
            y = anchor.y
        };
    }

    public struct UIFill
    {
        public float left, right;
        public float top, bottom;

        public static UIFill Fill() => new UIFill();

        public static UIFill Fill( float left, float right, float top, float bottom ) => new UIFill()
        {
            left = left,
            right = right,
            top = top,
            bottom = bottom
        };

        public static UIFillPercent FillPercent( float left, float right, float top, float bottom ) => new UIFillPercent()
        {
            left = left,
            right = right,
            top = top,
            bottom = bottom
        };

        public static UIFillHorizontal Horizontal() => new UIFillHorizontal();

        public static UIFillHorizontal Horizontal( float marginLeft, float marginRight ) => new UIFillHorizontal()
        {
            left = marginLeft,
            right = marginRight
        };

        public static UIFillPercentHorizontal HorizontalPercent( float marginLeft, float marginRight ) => new UIFillPercentHorizontal()
        {
            left = marginLeft,
            right = marginRight
        };

        public static UIFillVertical Vertical() => new UIFillVertical();

        public static UIFillVertical Vertical( float marginTop, float marginBottom ) => new UIFillVertical()
        {
            top = marginTop,
            bottom = marginBottom
        };

        public static UIFillPercentVertical VerticalPercent( float marginTop, float marginBottom ) => new UIFillPercentVertical()
        {
            top = marginTop,
            bottom = marginBottom
        };
    }

    public struct UIFillHorizontal
    {
        public float left, right;

        public static implicit operator UIFill( UIFillHorizontal fill ) => new UIFill()
        {
            left = fill.left,
            right = fill.right,
            top = 0,
            bottom = 0
        };
    }

    public struct UIFillVertical
    {
        public float top, bottom;

        public static implicit operator UIFill( UIFillVertical fill ) => new UIFill()
        {
            left = 0,
            right = 0,
            top = fill.top,
            bottom = fill.bottom
        };
    }

    public struct UIFillPercent
    {
        public float left, right;
        public float top, bottom;
    }

    public struct UIFillPercentHorizontal
    {
        public float left, right;

        public static implicit operator UIFillPercent( UIFillPercentHorizontal fill ) => new UIFillPercent()
        {
            left = fill.left,
            right = fill.right,
            top = 0,
            bottom = 0
        };
    }

    public struct UIFillPercentVertical
    {
        public float top, bottom;

        public static implicit operator UIFillPercent( UIFillPercentVertical fill ) => new UIFillPercent()
        {
            left = 0,
            right = 0,
            top = fill.top,
            bottom = fill.bottom
        };
    }

    public struct UIPosition
    {
        public float x, y;

        public static UIPosition TopLeft => new UIPosition() { x = 0, y = 1 };
        public static float Top => 1f;
        public static UIPosition TopRight => new UIPosition() { x = 1, y = 1 };

        public static float Left => 0f;
        public static UIPosition Middle => new UIPosition() { x = 0.5f, y = 0.5f };
        public static float Right => 1f;

        public static UIPosition BottomLeft => new UIPosition() { x = 0, y = 0 };
        public static float Bottom => 0f;
        public static UIPosition BottomRight => new UIPosition() { x = 1, y = 0 };

        public static implicit operator UIPosition( float xy ) => new UIPosition()
        {
            x = xy,
            y = xy
        };

        public static implicit operator UIPosition( (float x, float y) pos ) => new UIPosition()
        {
            x = pos.x,
            y = pos.y
        };

        public static explicit operator UIPosition( Vector2 pos ) => new UIPosition()
        {
            x = pos.x,
            y = pos.y
        };
    }

    public struct UISize
    {
        public float x, y;

        public static implicit operator UISize( (float x, float y) pos ) => new UISize()
        {
            x = pos.x,
            y = pos.y
        };

        public static explicit operator UISize( Vector2 pos ) => new UISize()
        {
            x = pos.x,
            y = pos.y
        };
    }

    public struct UILayoutInfo
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;

        public UILayoutInfo( Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta )
        {
            this.anchorMin = anchorMin;
            this.anchorMax = anchorMax;
            this.pivot = pivot;
            this.anchoredPosition = anchoredPosition;
            this.sizeDelta = sizeDelta;
        }

        public UILayoutInfo( UIAnchor anchor, UIPosition pos, UISize size )
        {
            this.anchorMin = new Vector2( anchor.x, anchor.y );
            this.anchorMax = new Vector2( anchor.x, anchor.y );
            this.pivot = new Vector2( anchor.x, anchor.y );
            this.anchoredPosition = new Vector2( pos.x, pos.y );
            this.sizeDelta = new Vector2( size.x, size.y );
        }

        public UILayoutInfo( UIFill fill )
        {
            anchorMin = Vector2.zero;
            anchorMax = Vector2.one;
            pivot = new Vector2( 0.5f, 0.5f );
            anchoredPosition = new Vector2( (fill.left - fill.right) / 2, (fill.bottom - fill.top) / 2 );
            sizeDelta = new Vector2( -(fill.left + fill.right), -(fill.top + fill.bottom) );
        }

        public UILayoutInfo( UIAnchorHorizontal anchorHorizontal, UIFillVertical fillVertical, float posX, float width )
        {
            anchorMin = new Vector2( anchorHorizontal.x, 0.0f );
            anchorMax = new Vector2( anchorHorizontal.x, 1.0f );
            pivot = new Vector2( anchorHorizontal.x, 0.5f );
            anchoredPosition = new Vector2( posX, (fillVertical.bottom - fillVertical.top) / 2 );
            sizeDelta = new Vector2( width, -(fillVertical.top + fillVertical.bottom) );
        }

        public UILayoutInfo( UIFillHorizontal fillHorizontal, UIAnchorVertical anchorVertical, float posY, float height )
        {
            anchorMin = new Vector2( 0.0f, anchorVertical.y );
            anchorMax = new Vector2( 1.0f, anchorVertical.y );
            pivot = new Vector2( 0.5f, anchorVertical.y );
            anchoredPosition = new Vector2( (fillHorizontal.left - fillHorizontal.right) / 2, posY );
            sizeDelta = new Vector2( -(fillHorizontal.left + fillHorizontal.right), height );
        }

        public UILayoutInfo( UIFillPercent fill )
        {
            anchorMin = new Vector2( fill.left, fill.bottom );
            anchorMax = new Vector2( 1.0f - fill.right, 1.0f - fill.top ); // since we want to input percent from each edge, we need to invert the max anchor.
            pivot = new Vector2( 0.5f, 0.5f );
            anchoredPosition = Vector2.zero;
            sizeDelta = Vector2.zero;
        }

        public UILayoutInfo( UIAnchorHorizontal anchorHorizontal, UIFillPercentVertical fillVertical, float posX, float width )
        {
            anchorMin = new Vector2( anchorHorizontal.x, fillVertical.bottom );
            anchorMax = new Vector2( anchorHorizontal.x, 1.0f - fillVertical.top ); // since we want to input percent from each edge, we need to invert the max anchor.
            pivot = new Vector2( anchorHorizontal.x, 0.5f );
            anchoredPosition = new Vector2( posX, 0.0f );
            sizeDelta = new Vector2( width, 0.0f );
        }

        public UILayoutInfo( UIFillPercentHorizontal fillHorizontal, UIAnchorVertical anchorVertical, float posY, float height )
        {
            anchorMin = new Vector2( fillHorizontal.left, anchorVertical.y );
            anchorMax = new Vector2( 1.0f - fillHorizontal.right, anchorVertical.y ); // since we want to input percent from each edge, we need to invert the max anchor.
            pivot = new Vector2( 0.5f, anchorVertical.y );
            anchoredPosition = new Vector2( 0.0f, posY );
            sizeDelta = new Vector2( 0.0f, height );
        }

        public UILayoutInfo( UIFillHorizontal fillHorizontal, UIFillPercentVertical fillVertical )
        {
            anchorMin = new Vector2( 0.0f, fillVertical.bottom );
            anchorMax = new Vector2( 1.0f, 1.0f - fillVertical.top ); // since we want to input percent from each edge, we need to invert the max anchor.
            pivot = new Vector2( 0.5f, 0.5f );
            anchoredPosition = new Vector2( (fillHorizontal.left - fillHorizontal.right) / 2, 0.0f );
            sizeDelta = new Vector2( -(fillHorizontal.left + fillHorizontal.right), 0.0f );
        }

        public UILayoutInfo( UIFillPercentHorizontal fillHorizontal, UIFillVertical fillVertical )
        {
            anchorMin = new Vector2( fillHorizontal.left, 0.0f );
            anchorMax = new Vector2( 1.0f - fillHorizontal.right, 1.0f ); // since we want to input percent from each edge, we need to invert the max anchor.
            pivot = new Vector2( 0.5f, 0.5f );
            anchoredPosition = new Vector2( 0.0f, (fillVertical.bottom - fillVertical.top) / 2 );
            sizeDelta = new Vector2( 0.0f, -(fillVertical.top + fillVertical.bottom) );
        }
    }
}