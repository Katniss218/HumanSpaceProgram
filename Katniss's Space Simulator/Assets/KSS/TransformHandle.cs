using KSS.Core;
using KSS.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;

namespace KSS
{
    [DisallowMultipleComponent]
    [RequireComponent( typeof( Collider ) )]
    public abstract class TransformHandle : MonoBehaviour
    {
        protected enum Mode
        {
            Linear,
            Planar
        }

        Collider _raycastCollider;

        /// <summary>
        /// The camera used to cast rays to grab the collider.
        /// </summary>
        [field: SerializeField]
        public Camera RaycastCamera { get; set; }

        /// <summary>
        /// The target of the transformation.
        /// </summary>
        [field: SerializeField]
        public Transform Target { get; set; }

        bool _isHeld;

        /// <summary>
        /// The position of the handle at the start of the transformation (in scene/world space).
        /// </summary>
        protected Vector3 _handleStartPosition;
        /// <summary>
        /// The 'forward' direction of the handle at the start of the transformation (in scene/world space).
        /// </summary>
        protected Vector3 _handleStartForward;

        protected Matrix4x4 _startLocalToWorld;
        protected Matrix4x4 _startWorldToLocal;

        const int HANDLE_COLLIDER_LAYER = 5;

        void Awake()
        {
            _raycastCollider = this.GetComponent<Collider>();
            _raycastCollider.gameObject.layer = HANDLE_COLLIDER_LAYER;
            _raycastCollider.isTrigger = true;
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.VIEWPORT_PRIMARY_DOWN, HierarchicalInputPriority.HIGH, Input_MouseDown );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.VIEWPORT_PRIMARY, HierarchicalInputPriority.HIGH, Input_MouseHeld );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.VIEWPORT_PRIMARY_UP, HierarchicalInputPriority.HIGH, Input_MouseUp );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.VIEWPORT_PRIMARY_DOWN, Input_MouseDown );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.VIEWPORT_PRIMARY, Input_MouseHeld );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.VIEWPORT_PRIMARY_UP, Input_MouseUp );
        }

        private bool Input_MouseDown()
        {
            if( _isHeld )
                return false;

            Ray ray = this.RaycastCamera.ScreenPointToRay( HierarchicalInputManager.CurrentState.MousePosition );

            // arrows get drawn on top of other objects.
            if( Physics.Raycast( ray, out RaycastHit hitInfo, float.MaxValue, 1 << HANDLE_COLLIDER_LAYER ) )
            {
                if( hitInfo.collider == _raycastCollider )
                {
                    StartTransformation();
                    _isHeld = true;
                    return true;
                }
            }
            return false;
        }

        private bool Input_MouseHeld()
        {
            if( !_isHeld ) return false;

            ContinueTransformation();
            return false;
        }

        private bool Input_MouseUp()
        {
            if( !_isHeld ) return false;

            EndTransformation();
            _isHeld = false;
            return false;
        }

        protected abstract void StartTransformation();
        protected abstract void ContinueTransformation();
        protected abstract void EndTransformation();


        public static Vector3 RoundToMultiple( Vector3 value, float multiple )
        {
            return new Vector3(
                multiple * Mathf.Round( value.x / multiple ),
                multiple * Mathf.Round( value.y / multiple ),
                multiple * Mathf.Round( value.z / multiple )
            );
        }

        public static float RoundToMultiple( float value, float multiple )
        {
            return multiple * Mathf.Round( value / multiple );
        }

        protected Plane GetRaycastPlane( Camera camera, Mode mode )
        {
            Vector3 normal;
            if( mode == Mode.Linear )
            {
                // In linear mode (single-axis transformation),
                //   the plane should be pointed towards the camera, but constrained such that the plane always contains the 'forward' axis of the handle.

                normal = (camera.transform.position - _handleStartPosition).normalized;
                normal = Vector3.ProjectOnPlane( normal, _handleStartForward );
                normal.Normalize();

                return new Plane( normal, _handleStartPosition );
            }
            if( mode == Mode.Planar )
            {
                // In planar mode (2-axis transformation),
                //   the plane should be oriented normal to the forward direction of the handle.

                return new Plane( this.transform.forward, _handleStartPosition );
            }
            throw new InvalidOperationException( $"Unknown handle mode '{mode}'." );
        }

        /// <summary>
        /// Calculates the local-space position of the cursor.
        /// </summary>
        protected Vector3 ProjectCursor( Camera camera, Mode mode )
        {
            Plane plane = GetRaycastPlane( camera, mode );
            Ray ray = camera.ScreenPointToRay( HierarchicalInputManager.CurrentState.MousePosition );

            if( plane.Raycast( ray, out float hitDistance ) )
            {
                Vector3 hitPoint = ray.GetPoint( hitDistance );

                hitPoint -= _handleStartPosition;

                hitPoint = _startWorldToLocal * hitPoint;

                return hitPoint;
            }
            else // This will only happen if we're looking at a very acute angle along or opposite to the arrow (or if we somehow can glitch the camera behind the starting point).
            {
                return new Vector3( float.NaN, float.NaN, float.NaN );
            }
        }
    }
}