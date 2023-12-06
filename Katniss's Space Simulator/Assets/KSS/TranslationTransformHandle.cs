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
    public class TranslationTransformHandle : TransformHandle
    {
        public Vector3 Delta { get; private set; }

        Mode _mode;

        Vector3 _targetStartPosition;
        /// <summary>
        /// The offset between the initial click point and the origin of the handle (in local space).
        /// </summary>
        Vector3 _clickOffsetLocalSpace;

        protected override void StartTransformation()
        {
            _handleStartPosition = this.transform.position;
            _handleStartForward = this.transform.forward;
            _targetStartPosition = Target.transform.position;
            _startLocalToWorld = Matrix4x4.TRS( this.transform.position, this.transform.rotation, Vector3.one ); // Calculate ourselves, otherwise the scale of the handle would mess it up.
            _startWorldToLocal = _startLocalToWorld.inverse;

            _clickOffsetLocalSpace = ProjectCursor( RaycastCamera, _mode );
            _isHeld = true;
            Delta = Vector3.zero;
        }

        protected override void ContinueTransformation()
        {
            Vector3 cursorHitPoint = ProjectCursor( RaycastCamera, _mode );

            // If the handle is moved to a nan, abort that axis.
            if( float.IsNaN( cursorHitPoint.x ) )
                cursorHitPoint.x = _clickOffsetLocalSpace.x;
            if( float.IsNaN( cursorHitPoint.y ) )
                cursorHitPoint.y = _clickOffsetLocalSpace.y;
            if( float.IsNaN( cursorHitPoint.z ) )
                cursorHitPoint.z = _clickOffsetLocalSpace.z;

            Vector3 targetPositionBeforeUpdate = Target.transform.position;

            if( _mode == Mode.Linear )
            {
                cursorHitPoint -= _clickOffsetLocalSpace;

                if( Input.GetKey( KeyCode.LeftShift ) )
                {
                    cursorHitPoint = RoundToMultiple( cursorHitPoint, 0.25f );
                }

                cursorHitPoint.x = 0.0f;
                cursorHitPoint.y = 0.0f;

                Vector3 worldHitPoint = _startLocalToWorld * cursorHitPoint;

                Vector3 targetPosition = _targetStartPosition + worldHitPoint;

                Target.transform.position = _targetStartPosition + worldHitPoint; // Origin is at the arrow, but the transformation has to be relative to the origin of the transformee.
                Delta = targetPosition - targetPositionBeforeUpdate;
                return;
            }
            if( _mode == Mode.Planar )
            {
                cursorHitPoint -= _clickOffsetLocalSpace;

                if( Input.GetKey( KeyCode.LeftShift ) )
                {
                    cursorHitPoint = RoundToMultiple( cursorHitPoint, 0.25f );
                }

                cursorHitPoint.z = 0.0f;

                Vector3 worldHitPoint = _startLocalToWorld * cursorHitPoint;

                Vector3 targetPosition = _targetStartPosition + worldHitPoint;

                Target.transform.position = _targetStartPosition + worldHitPoint; // Origin is at the arrow, but the transformation has to be relative to the origin of the transformee.
                Delta = targetPosition - targetPositionBeforeUpdate;
                return;
            }
        }

        protected override void EndTransformation()
        {
            ContinueTransformation(); // Makes sure the final position is correct.
            _isHeld = false;
            Delta = Vector3.zero;
        }

        public static void DestroyHandles( Transform parent )
        {
            foreach( Transform child in parent )
            {
                Destroy( child );
            }
        }

        public static void Create3Handles( Transform parent, Camera camera, Transform target, Quaternion orientation )
        {
            parent.rotation = orientation;

            GameObject goX = new GameObject( "X" );
            goX.transform.SetParent( parent );
            goX.transform.localRotation = Quaternion.Euler( 0, 90, 0 );

            CapsuleCollider c = goX.AddComponent<CapsuleCollider>();
            c.radius = 0.375f;
            c.height = 2.75f;
            c.direction = 2;
            c.center = new Vector3( 0, 0, 1.375f );

            MeshFilter mf = goX.AddComponent<MeshFilter>();
            mf.sharedMesh = AssetRegistry.Get<Mesh>( $"builtin::Resources/translate_handle_1d" );

            MeshRenderer mr = goX.AddComponent<MeshRenderer>();
            mr.sharedMaterial = AssetRegistry.Get<Material>( $"builtin::Resources/Materials/axis_x" );

            TranslationTransformHandle tt = goX.AddComponent<TranslationTransformHandle>();
            tt.Target = target;
            tt.RaycastCamera = camera;

            GameObject goY = new GameObject( "Y" );
            goY.transform.SetParent( parent );
            goY.transform.localRotation = Quaternion.Euler( -90, 0, 0 );

            c = goY.AddComponent<CapsuleCollider>();
            c.radius = 0.375f;
            c.height = 2.75f;
            c.direction = 2;
            c.center = new Vector3( 0, 0, 1.375f );

            mf = goY.AddComponent<MeshFilter>();
            mf.sharedMesh = AssetRegistry.Get<Mesh>( $"builtin::Resources/translate_handle_1d" );

            mr = goY.AddComponent<MeshRenderer>();
            mr.sharedMaterial = AssetRegistry.Get<Material>( $"builtin::Resources/Materials/axis_y" );

            tt = goY.AddComponent<TranslationTransformHandle>();
            tt.Target = target;
            tt.RaycastCamera = camera;

            GameObject goZ = new GameObject( "Z" );
            goZ.transform.SetParent( parent );
            goZ.transform.localRotation = Quaternion.Euler( 0, 0, 0 );

            c = goZ.AddComponent<CapsuleCollider>();
            c.radius = 0.375f;
            c.height = 2.75f;
            c.direction = 2;
            c.center = new Vector3( 0, 0, 1.375f );

            mf = goZ.AddComponent<MeshFilter>();
            mf.sharedMesh = AssetRegistry.Get<Mesh>( $"builtin::Resources/translate_handle_1d" );

            mr = goZ.AddComponent<MeshRenderer>();
            mr.sharedMaterial = AssetRegistry.Get<Material>( $"builtin::Resources/Materials/axis_z" );

            tt = goZ.AddComponent<TranslationTransformHandle>();
            tt.Target = target;
            tt.RaycastCamera = camera;
        }
    }
}