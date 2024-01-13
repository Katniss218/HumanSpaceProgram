using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace KSS
{
    /// <summary>
    /// Allows the user to move the object around
    /// </summary>
    public sealed class TranslationTransformHandle : TransformHandle
    {
        /// <summary>
        /// world space delta
        /// </summary>
        public event Action<Vector3> OnAfterTranslate;

        Mode _mode;

        Vector3 _lastCursorPos;

        public bool SnappingEnabled { get; set; } = false;
        public float SnappingInterval { get; set; } = 0.25f;

        protected override void StartTransformation()
        {
            _handleStartPosition = this.transform.position;
            _handleStartForward = this.transform.forward;
            _startLocalToWorld = Matrix4x4.TRS( this.transform.position, this.transform.rotation, Vector3.one ); // Calculate ourselves, otherwise the scale of the handle would mess it up.
            _startWorldToLocal = _startLocalToWorld.inverse;

            _lastCursorPos = ProjectCursor( RaycastCamera, _mode );
            switch( _mode )
            {
                case Mode.Linear:
                    _lastCursorPos.x = 0.0f;
                    _lastCursorPos.y = 0.0f;
                    break;
                case Mode.Planar:
                    _lastCursorPos.z = 0.0f;
                    break;
                default:
                    throw new InvalidOperationException( $"Unknown mode '{_mode}'." );
            }

        }

        protected override void ContinueTransformation()
        {
            Vector3 cursorHitPoint = ProjectCursor( RaycastCamera, _mode );
            switch( _mode )
            {
                case Mode.Linear:
                    cursorHitPoint.x = 0.0f;
                    cursorHitPoint.y = 0.0f;
                    break;
                case Mode.Planar:
                    cursorHitPoint.z = 0.0f;
                    break;
                default:
                    throw new InvalidOperationException( $"Unknown mode '{_mode}'." );
            }

            // If the handle is moved to a nan, abort that axis.
            if( float.IsNaN( cursorHitPoint.x ) )
                cursorHitPoint.x = _lastCursorPos.x;
            if( float.IsNaN( cursorHitPoint.y ) )
                cursorHitPoint.y = _lastCursorPos.y;
            if( float.IsNaN( cursorHitPoint.z ) )
                cursorHitPoint.z = _lastCursorPos.z;

            if( SnappingEnabled )
            {
                cursorHitPoint = RoundToMultiple( cursorHitPoint, SnappingInterval );
            }

            Vector3 positionDelta = cursorHitPoint - _lastCursorPos;
            positionDelta = _startLocalToWorld * positionDelta;

            Target.transform.position += positionDelta;

            _lastCursorPos = cursorHitPoint;
            OnAfterTranslate?.Invoke( positionDelta );
        }

        protected override void EndTransformation()
        {
            ContinueTransformation(); // Makes sure the final position is correct.
        }
    }
}