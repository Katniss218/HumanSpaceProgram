using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UILib
{
    /// <summary>
    /// A multi-segmented value (progress) bar.
    /// </summary>
    public class ValueBar : MonoBehaviour
    {
        [Serializable]
        public class Element
        {
            public ValueBarSegment seg;
            public float width;
        }

        [SerializeField]
        private float _spacing;
        [SerializeField]
        private float _paddingLeft;
        [SerializeField]
        private float _paddingRight;

        RectTransform _self;
        float paddedWidth => _self.rect.width - (PaddingLeft + PaddingRight);

        public float Spacing
        {
            get { return _spacing; }
            set
            {
                _spacing = value;
                SyncElements( 0 );
            }
        }
        
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

        /// <summary>
        /// Gets the segment at the specified index.
        /// </summary>
        public ValueBarSegment GetSegment( int index )
        {
            return _segments[index].seg;
        }

        /// <summary>
        /// Adds a segment of the specified width to the end of the bar.
        /// </summary>
        /// <param name="width">The width. Clamped to [0..RemainingWidth].</param>
        /// <returns>The newly added segment.</returns>
        public ValueBarSegment AddSegment( float width )
        {
            return InsertSegment( _segments.Count, width );
        }

        /// <summary>
        /// Inserts a segment of the specified width at the specified index.
        /// </summary>
        /// <param name="width">The width. Clamped to [0..RemainingWidth].</param>
        /// <returns>The newly added segment.</returns>
        public ValueBarSegment InsertSegment( int index, float width )
        {
            float totalWidthPx = this.paddedWidth;
            float remainingWidth = 1.0f - GetWidth();

            float segWidthPerc = Mathf.Clamp( width, 0, remainingWidth );

            ValueBarSegment seg = ValueBarSegment.Create( (RectTransform)this.transform, totalWidthPx );

            _segments.Insert( index, new Element() { seg = seg, width = segWidthPerc } );

            SyncElements( index );

            return seg;
        }

        public void RemoveSegment( int index )
        {
            var seg = _segments[index];
            _segments.RemoveAt( index );

            Destroy( seg.seg.gameObject );

            SyncElements( index );
        }

        public void ClearSegments()
        {
            foreach( var seg in _segments )
            {
                Destroy( seg.seg.gameObject );
            }
            _segments.Clear();
            // everything removed so nothing left to sync.
        }

        public void SetWidth( int index, float width )
        {
            float remainingWidth = 1.0f - GetWidthExcluding( index );
            float segWidthPerc = Mathf.Clamp( width, 0, remainingWidth );

            _segments[index].width = segWidthPerc;

            SyncElements( index );
        }

        private float GetWidth()
        {
            return _segments.Sum( s => s.width );
        }

        private float GetWidthExcluding( int index )
        {
            float sum = 0.0f;
            for( int i = 0; i < _segments.Count; i++ )
            {
                if( i == index ) continue;

                sum += _segments[i].width;
            }
            return sum;
        }

        private float GetWidthBefore( int index )
        {
            float sum = 0.0f;
            for( int i = 0; i < index; i++ )
            {
                sum += _segments[i].width;
            }
            return sum;
        }

        private void SyncElements( int startingFrom )
        {
            float halfSpacing = this.Spacing / 2f;
            float totalWidthPx = this.paddedWidth;
            float rightOfCurrentSegment = GetWidthBefore( startingFrom );

            // starting at the specified index (this element changed), update it and the elements after it.
            for( int i = startingFrom; i < _segments.Count; i++ )
            {
                float extraPaddingLeft = i == 0 ? 0.0f : halfSpacing;
                float extraPaddingRight = i == _segments.Count ? 0.0f : halfSpacing;

                var elem = _segments[i];
                float segLeft = totalWidthPx * rightOfCurrentSegment + PaddingLeft + extraPaddingLeft;

                elem.seg.transform.SetSiblingIndex( i );

                elem.seg.TotalWidth = totalWidthPx;
                elem.seg.ForegroundOffsetX = PaddingLeft;
                elem.seg.Left = segLeft;
                elem.seg.Width = Mathf.Max( 0, totalWidthPx * elem.width - extraPaddingLeft - extraPaddingRight );
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