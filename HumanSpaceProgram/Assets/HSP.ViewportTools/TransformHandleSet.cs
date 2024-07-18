using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ViewportTools
{
    /// <summary>
    /// Controls an entire set of transform handles.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TransformHandleSet : MonoBehaviour
    {
        private static readonly Vector3[] XYZ_HANDLE_FORWARDS = new Vector3[]
        {
            Vector3.right,
            Vector3.up,
            Vector3.forward
        };

        [SerializeField] Transform _target;
        /// <summary>
        /// Gets or sets the target of the transform handles.
        /// </summary>
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

        [SerializeField] Camera _raycastCamera;
        /// <summary>
        /// Gets or sets the raycasting camera of the transform handles.
        /// </summary>
        public Camera RaycastCamera
        {
            get => _raycastCamera;
            set
            {
                _raycastCamera = value;
                foreach( var h in _handles )
                    h.RaycastCamera = value;
            }
        }

        List<TransformHandle> _handles = new List<TransformHandle>();

        /// <summary>
        /// Gets the underlying transform handles of the specified type.
        /// </summary>
        /// <remarks>
        /// Returns an empty collection if there are no handles of the given type.
        /// </remarks>
        public IEnumerable<T> GetHandles<T>() where T : TransformHandle
        {
            // NOTE: The handle set might contain handles of different types at once (e.g. both translation and rotation handles).
            return _handles.Where( h => h is T ).Cast<T>();
        }

        /// <summary>
        /// Creates a new set of transform handles.
        /// </summary>
        /// <param name="position">The position of the handle set.</param>
        /// <param name="rotation">The orientation of the handle set.</param>
        /// <param name="target">The target to affect when the handles are held.</param>
        /// <param name="raycastCamera">The camera used for raycasting.</param>
        /// <returns></returns>
        public static TransformHandleSet Create( Vector3 position, Quaternion rotation, Transform target, Camera raycastCamera )
        {
            GameObject rootGameObject = new GameObject();
            rootGameObject.transform.SetPositionAndRotation( position, rotation );

            TransformHandleSet handleSet = rootGameObject.AddComponent<TransformHandleSet>();
            handleSet.Target = target;
            handleSet.RaycastCamera = raycastCamera;

            return handleSet;
        }

        /// <summary>
        /// Destroys the transform handle set.
        /// </summary>
        public void Destroy()
        {
            Destroy( this.gameObject );
        }

        /// <summary>
        /// Creates a new set of 3 handles pointing along the local XYZ axes of the handle set.
        /// </summary>
        /// <typeparam name="T">The type of the handle to create.</typeparam>
        /// <param name="mesh">The mesh to use when rendering the handles.</param>
        /// <param name="material">The material to use when rendering the handles.</param>
        /// <param name="colliderConfigurator">Use to add the appropriate collider.</param>
        public void CreateXYZHandles<T>( Mesh mesh, Material material, Action<GameObject> colliderConfigurator ) where T : TransformHandle
        {
            // TODO - the collider adder thing being a delegate is kinda ugly and prone to abuse.

            foreach( var h in _handles )
            {
                Destroy( h.gameObject );
            }
            _handles.Clear();

            foreach( var dir in XYZ_HANDLE_FORWARDS )
            {
                T handle = CreateHandle<T>( dir, mesh, material, colliderConfigurator );
                if( handle is TranslationTransformHandle translationHandle )
                {
                    translationHandle.OnAfterTranslate += OnAfterTranslate;
                }
                _handles.Add( handle );
            }
        }

        private void OnAfterTranslate( Vector3 worldSpaceDelta )
        {
            this.transform.position += worldSpaceDelta;
        }

        private T CreateHandle<T>( Vector3 localForward, Mesh mesh, Material material, Action<GameObject> colliderConfigurator ) where T : TransformHandle
        {
            GameObject gameObject = new GameObject( localForward.ToString() );
            gameObject.transform.SetParent( this.transform );
            gameObject.transform.localRotation = Quaternion.LookRotation( localForward, Vector3.Cross( localForward, Vector3.one ) );

            colliderConfigurator.Invoke( gameObject );

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.material.SetColor( "_Color", new Color( localForward.x * 255f, localForward.y * 255f, localForward.z * 255f, 1f ) );

            T handle = gameObject.AddComponent<T>();
            handle.Target = this.Target;
            handle.RaycastCamera = this.RaycastCamera;

            return handle;
        }
    }
}