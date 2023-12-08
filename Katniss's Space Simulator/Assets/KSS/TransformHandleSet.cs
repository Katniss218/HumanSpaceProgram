using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS
{
    /// <summary>
    /// Controls an entire set of transform handles.
    /// </summary>
    [DisallowMultipleComponent]
    public class TransformHandleSet : MonoBehaviour
    {
        [SerializeField] Transform _target;
        public Transform Target
        {
            get => _target;
            set
            {
                _target = value;
                foreach( var h in _handles )
                    h.Target = value;
            }
        }

        [SerializeField] Camera _camera;
        public Camera Camera
        {
            get => _camera;
            set
            {
                _camera = value;
                foreach( var h in _handles )
                    h.RaycastCamera = value;
            }
        }

        private List<TransformHandle> _handles = new List<TransformHandle>();

        // move parent by the sum of deltas of move handles.
        // don't rotate by rotation delta.

        // scale to keep arrows at constant size relative to the camera.

        // orientation of 'this.transform' dictates the orientation of the entire set of handles.

        void OnAfterTranslate( Vector3 worldSpaceDelta )
        {
            this.transform.position += worldSpaceDelta;
        }

        public static void Create3Handles<T>( Vector3 position, Quaternion rotation, Transform target, Camera camera, Mesh mesh, Material material, Action<GameObject> colliderConfigurator ) where T : TransformHandle
        {
            GameObject go = new GameObject();
            go.transform.SetPositionAndRotation( position, rotation );
            TransformHandleSet comp = go.AddComponent<TransformHandleSet>();
            comp.Target = target;
            comp.Camera = camera;

            comp.Create3Handles<T>( mesh, material, colliderConfigurator );
        }

        public void Create3Handles<T>( Mesh mesh, Material material, Action<GameObject> colliderConfigurator ) where T : TransformHandle
        {
            foreach( var h in _handles )
            {
                Destroy( h.gameObject );
            }
            _handles.Clear();

            _handles.Add( CreateHandle<T>( this.transform, this.Target, this.Camera, Vector3.right, mesh, material, colliderConfigurator ) );
            _handles.Add( CreateHandle<T>( this.transform, this.Target, this.Camera, Vector3.up, mesh, material, colliderConfigurator ) );
            _handles.Add( CreateHandle<T>( this.transform, this.Target, this.Camera, Vector3.forward, mesh, material, colliderConfigurator ) );
        }

        private static T CreateHandle<T>( Transform parent, Transform target, Camera camera, Vector3 localForward, Mesh mesh, Material material, Action<GameObject> colliderConfigurator ) where T : TransformHandle
        {
            GameObject go = new GameObject( localForward.ToString() );
            go.transform.SetParent( parent );
            go.transform.localRotation = Quaternion.LookRotation( localForward, Vector3.Cross( localForward, Vector3.one ) );

            /*c = goZ.AddComponent<CapsuleCollider>();
            c.radius = 0.375f;
            c.height = 2.75f;
            c.direction = 2;
            c.center = new Vector3( 0, 0, 1.375f );*/
            colliderConfigurator.Invoke( go );

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;

            var tt = go.AddComponent<T>();
            tt.Target = target;
            tt.RaycastCamera = camera;

            return tt;
        }
    }
}