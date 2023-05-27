using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UILib
{
    public class ValueBar : MonoBehaviour
    {
        [Serializable]
        public class Element
        {
            public ValueBarSegment seg;
            public float width;
        }

        //private float _spacing; 0
        [SerializeField]
        private float _paddingLeft;
        [SerializeField]
        private float _paddingRight;

        RectTransform _self;
        float paddedWidth => _self.rect.width - (PaddingLeft + PaddingRight);

        public float PaddingLeft
        {
            get { return _paddingLeft; }
            set
            {
                _paddingLeft = value;
                SyncElements( 0 );
            }
        }

        public float PaddingRight
        {
            get { return _paddingRight; }
            set
            {
                _paddingRight = value;
                SyncElements( 0 );
            }
        }

        void Awake()
        {
            _self = (RectTransform)this.transform;
        }

        // segments and their widths as percentage of full.
        [SerializeField]
        [HideInInspector]
        private List<Element> _segments = new List<Element>();

        public int SegmentCount => _segments.Count;

        public ValueBarSegment GetSegment( int index )
        {
            return _segments[index].seg;
        }

        public ValueBarSegment AddSegment( float width )
        {
            return InsertSegment( _segments.Count, width );
        }

        public ValueBarSegment InsertSegment( int index, float width )
        {
            float totalWidthPx = this.paddedWidth;
            float rightOfCurrentSegment = GetWidthBefore( index );

            float segWidthPerc = Mathf.Clamp( width, 0, 1.0f - rightOfCurrentSegment );
            float segWidth = Mathf.Clamp( width, 0, 1.0f - rightOfCurrentSegment ) * totalWidthPx;

            float segLeft = totalWidthPx * rightOfCurrentSegment + PaddingLeft;

            ValueBarSegment seg = ValueBarSegment.Create( (RectTransform)this.transform, totalWidthPx, segLeft, segWidth, PaddingLeft );

            _segments.Insert( index, new Element() { seg = seg, width = segWidthPerc } );

            return seg;
        }

        public void SetWidth( ValueBarSegment elem, float width )
        {
            int index = _segments.FindIndex( s => s.seg == elem );

            float rightOfCurrentSegment = GetWidthBefore( index );
            float segWidthPerc = Mathf.Clamp( width, 0, 1.0f - rightOfCurrentSegment );

            _segments[index].width = segWidthPerc;

            SyncElements( index );
        }

        private float GetWidthBefore( int index )
        {
            float rightOfCurrentSegment = 0;
            if( _segments.Count != 0 && index > 0 )
            {
                rightOfCurrentSegment = _segments[index - 1].width;
            }
            return rightOfCurrentSegment;
        }

        private void SyncElements( int startingFrom )
        {
            float totalWidthPx = this.paddedWidth;
            float rightOfCurrentSegment = GetWidthBefore( startingFrom );

            // starting at the specified index (this element changed), update it and the elements after it.
            for( int i = startingFrom; i < _segments.Count; i++ )
            {
                var elem = _segments[i];
                float segLeft = totalWidthPx * rightOfCurrentSegment + PaddingLeft;

                elem.seg.transform.SetSiblingIndex( i );

                elem.seg.WidthParent = totalWidthPx;
                elem.seg.ForegroundOffsetX = PaddingLeft;
                elem.seg.Left = segLeft;
                elem.seg.Width = totalWidthPx * elem.width;
                rightOfCurrentSegment += elem.width;
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            SyncElements( 0 );
        }
#endif
    }
}