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
    /// Allows the user to rotate the object around
    /// </summary>
    public class RotationTransformHandle : TransformHandle
    {
        public event Action<Quaternion> OnAfterRotate;

        Vector3 _lastCursorPos;

        protected override void StartTransformation()
        {
            _handleStartPosition = this.transform.position;
            _handleStartForward = this.transform.forward;
            _startLocalToWorld = Matrix4x4.TRS( this.transform.position, this.transform.rotation, Vector3.one ); // Calculate ourselves, otherwise the scale of the handle would mess it up.
            _startWorldToLocal = _startLocalToWorld.inverse;

            _lastCursorPos = ProjectCursor( RaycastCamera, Mode.Planar );
            _isHeld = true;
        }

        protected override void ContinueTransformation()
        {
            Vector3 cursorHitPoint = ProjectCursor( RaycastCamera, Mode.Planar );

            // If the handle is moved to a nan, abort that axis.
            if( float.IsNaN( cursorHitPoint.x ) )
                cursorHitPoint.x = _lastCursorPos.x;
            if( float.IsNaN( cursorHitPoint.y ) )
                cursorHitPoint.y = _lastCursorPos.y;
            if( float.IsNaN( cursorHitPoint.z ) )
                cursorHitPoint.z = _lastCursorPos.z;

            float angle = Vector3.SignedAngle( _lastCursorPos, cursorHitPoint, Vector3.forward );
            if( angle < 0.0f )
            {
                angle += 360.0f;
            }

            if( Input.GetKey( KeyCode.LeftShift ) )
            {
                angle = RoundToMultiple( angle, 22.5f );
            }

            Quaternion currentFrameDelta = Quaternion.AngleAxis( angle, _handleStartForward );
            Target.transform.rotation = currentFrameDelta * Target.transform.rotation;

            _lastCursorPos = cursorHitPoint;
            OnAfterRotate?.Invoke( currentFrameDelta );
        }

        protected override void EndTransformation()
        {
            ContinueTransformation(); // Makes sure the final position is correct.
            _isHeld = false;
        }
    }
}