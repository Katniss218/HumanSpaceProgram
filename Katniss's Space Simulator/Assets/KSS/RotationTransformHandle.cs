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
        Vector3 _lastClickOffset;

        public event Action<Quaternion> OnAfterRotate;

        protected override void StartTransformation()
        {
            _handleStartPosition = this.transform.position;
            _handleStartForward = this.transform.forward;
            _startLocalToWorld = Matrix4x4.TRS( this.transform.position, this.transform.rotation, Vector3.one ); // Calculate ourselves, otherwise the scale of the handle would mess it up.
            _startWorldToLocal = _startLocalToWorld.inverse;

            _lastClickOffset = ProjectCursor( RaycastCamera, Mode.Planar );
            _isHeld = true;
        }

        protected override void ContinueTransformation()
        {
            Vector3 cursorHitPoint = ProjectCursor( RaycastCamera, Mode.Planar );

            // If the handle is moved to a nan, abort that axis.
            if( float.IsNaN( cursorHitPoint.x ) )
                cursorHitPoint.x = _lastClickOffset.x;
            if( float.IsNaN( cursorHitPoint.y ) )
                cursorHitPoint.y = _lastClickOffset.y;
            if( float.IsNaN( cursorHitPoint.z ) )
                cursorHitPoint.z = _lastClickOffset.z;

            float angle = Vector3.SignedAngle( _lastClickOffset, cursorHitPoint, Vector3.forward );
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

            _lastClickOffset = cursorHitPoint;
            OnAfterRotate?.Invoke( currentFrameDelta );
        }

        protected override void EndTransformation()
        {
            ContinueTransformation(); // Makes sure the final position is correct.
            _isHeld = false;
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

            BoxCollider c = goX.AddComponent<BoxCollider>();
            c.size = new Vector3( 3f, 3f, 0.1f );

            MeshFilter mf = goX.AddComponent<MeshFilter>();
            mf.sharedMesh = AssetRegistry.Get<Mesh>( $"builtin::Resources/rotate_handle_1d" );

            MeshRenderer mr = goX.AddComponent<MeshRenderer>();
            mr.sharedMaterial = AssetRegistry.Get<Material>( $"builtin::Resources/Materials/axis_x" );

            RotationTransformHandle tt = goX.AddComponent<RotationTransformHandle>();
            tt.Target = target;
            tt.RaycastCamera = camera;

            GameObject goY = new GameObject( "Y" );
            goY.transform.SetParent( parent );
            goY.transform.localRotation = Quaternion.Euler( -90, 0, 0 );

            c = goY.AddComponent<BoxCollider>();
            c.size = new Vector3( 3f, 3f, 0.1f );

            mf = goY.AddComponent<MeshFilter>();
            mf.sharedMesh = AssetRegistry.Get<Mesh>( $"builtin::Resources/rotate_handle_1d" );

            mr = goY.AddComponent<MeshRenderer>();
            mr.sharedMaterial = AssetRegistry.Get<Material>( $"builtin::Resources/Materials/axis_y" );

            tt = goY.AddComponent<RotationTransformHandle>();
            tt.Target = target;
            tt.RaycastCamera = camera;

            GameObject goZ = new GameObject( "Z" );
            goZ.transform.SetParent( parent );
            goZ.transform.localRotation = Quaternion.Euler( 0, 0, 0 );

            c = goZ.AddComponent<BoxCollider>();
            c.size = new Vector3( 3f, 3f, 0.1f );

            mf = goZ.AddComponent<MeshFilter>();
            mf.sharedMesh = AssetRegistry.Get<Mesh>( $"builtin::Resources/rotate_handle_1d" );

            mr = goZ.AddComponent<MeshRenderer>();
            mr.sharedMaterial = AssetRegistry.Get<Material>( $"builtin::Resources/Materials/axis_z" );

            tt = goZ.AddComponent<RotationTransformHandle>();
            tt.Target = target;
            tt.RaycastCamera = camera;
        }
    }
}