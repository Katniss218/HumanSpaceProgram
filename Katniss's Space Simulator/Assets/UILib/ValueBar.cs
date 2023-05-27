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
        private float _spacing;
        private float _paddingLeft;
        private float _paddingRight;

        // segments and their widths as percentage of full.
        private List<(ValueBarSegment, float)> _segments = new List<(ValueBarSegment, float)>();

        public void AddSegment( Sprite segmentSprite )
        {
            float rightOfCurrentSegments = _segments.Count == 0 ? 0.0f : _segments[_segments.Count - 1].Item2;

            var seg = ValueBarSegment.Create( (RectTransform)this.transform, segmentSprite, rightOfCurrentSegments, 1, _paddingLeft, _paddingRight );

            _segments.Add( (seg, Mathf.Min( 0, rightOfCurrentSegments - 1.0f )) );
        }

        public void SetWidth( ValueBarSegment elem, float width )
        {
            int index = _segments.FindIndex( s => s.Item1 == elem );

            _segments[index] = (_segments[index].Item1, width);
        }

        private void UpdateElements( int startingFrom )
        {
            // starting at the specified index (this element changed), update it and the elements after it.
        }

#if UNITY_EDITOR
        void OnValidate()
        {

        }
#endif
    }
}