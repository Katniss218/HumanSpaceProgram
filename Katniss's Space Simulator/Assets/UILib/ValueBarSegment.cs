using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UILib
{
    public class ValueBarSegment : MonoBehaviour
    {
        [SerializeField]
        private float _left;
        [SerializeField]
        private float _right;

        public float Left
        {
            get { return _left; }
            set
            {
                _left = value;
                OnLeftOrRightChanged();
            }
        }

        public float Right
        {
            get { return _right; }
            set
            {
                _right = value;
                OnLeftOrRightChanged();
            }
        }

        [SerializeField]
        RectTransform _image;

        RectTransform _parent;
        RectTransform _mask;

        float width => _parent.rect.width;

        [SerializeField]
        private float _leftPadding;
        [SerializeField]
        private float _rightPadding;
        public float LeftPadding
        {
            get { return _leftPadding; }
            set
            {
                _leftPadding = value;
                OnPaddingChanged();
            }
        }

        public float RightPadding
        {
            get { return _rightPadding; }
            set
            {
                _rightPadding = value;
                OnPaddingChanged();
            }
        }

        void RecacheParent()
        {
            _parent = (RectTransform)this.transform.parent;

            if( _parent == null )
            {
                Debug.LogError( $"Can't add {nameof( ValueBarSegment )} to a root object." );
                Destroy( this );
            }
        }

        void Awake()
        {
            RecacheParent();
            _mask = (RectTransform)this.transform;
        }

        public void SetImage( RectTransform image )
        {
            _image = image;
        }

#warning TODO - text.

        void OnLeftOrRightChanged()
        {
            // mask and image are always anchored on the left, and have pivots on the left too.

            float leftPos = (width * Left);
            float widthPx = (width * (1 - Right - Left)) - (LeftPadding + RightPadding);
            _mask.sizeDelta = new Vector2( widthPx, 0 );
            _mask.anchoredPosition = new Vector2( leftPos + LeftPadding, 0 );
            _image.anchoredPosition = new Vector2( -leftPos, 0 );
        }

        void OnPaddingChanged()
        {
            float widthPx = width - (LeftPadding + RightPadding);
            _image.sizeDelta = new Vector2( widthPx, 0 );
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if( this.transform.parent != _parent )
            {
                RecacheParent();
            }

            if( _image != null ) // IsValidate can be called before a suitable image exists.
            {
                OnLeftOrRightChanged();
                OnPaddingChanged();
            }
        }
#endif

        internal static ValueBarSegment Create( RectTransform parent, Sprite foregroundSprite, float left, float right, float paddingLeft, float paddingRight )
        {
            (GameObject maskGO, RectTransform maskT) = UIHelper.CreateUI( parent, "mask", new UILayoutInfo( new Vector2( 0, 0 ), new Vector2( 0, 1 ), Vector2.zero, new Vector2( parent.rect.width, 0 ) ) );

            Image maskImage = maskGO.AddComponent<Image>();
            maskImage.raycastTarget = false;
            maskImage.sprite = null;

            Mask mask = maskGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject imgGO, RectTransform imgT) = UIHelper.CreateUI( maskT, "foreground", new UILayoutInfo( new Vector2( 0, 0 ), new Vector2( 0, 1 ), Vector2.zero, new Vector2( parent.rect.width, 0 ) ) );

            Image foregroundImage = imgGO.AddComponent<Image>();
            foregroundImage.raycastTarget = false;
            foregroundImage.sprite = foregroundSprite;
            foregroundImage.type = Image.Type.Sliced;

            ValueBarSegment barSegment = maskGO.AddComponent<ValueBarSegment>();

            barSegment.SetImage( imgT );
            barSegment.LeftPadding = paddingLeft;
            barSegment.RightPadding = paddingRight;
            barSegment.Left = left;
            barSegment.Right = right;

            return barSegment;
        }
    }
}