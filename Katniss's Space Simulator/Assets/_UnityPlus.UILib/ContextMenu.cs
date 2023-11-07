using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.UILib
{
    public class ContextMenu : MonoBehaviour
    {
#warning TODO - join this with UIContextMenu.
        public RectTransform Pivot { get; set; }

        RectTransform _target;
        public RectTransform Target
        {
            get => _target;
            set
            {
                _target = value;
                RecalculateOffset();
            }
        }

        Vector2 _targetOffset;

        public void RecalculateOffset()
        {
            _targetOffset = new Vector2( _target.position.x, _target.position.y ) - new Vector2( Pivot.position.x, Pivot.position.y );
        }

        void LateUpdate()
        {
            Pivot.position = new Vector2( Pivot.position.x, Pivot.position.y ) + _targetOffset;
        }
    }
}