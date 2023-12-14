using KSS.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace KSS.DesignScene.Tools
{
    /// <summary>
    /// Allows to move a selected part after placing.
    /// </summary>
    public class TranslateTool : DesignSceneToolBase
    {
        private bool _snappingEnabled;
        public bool SnappingEnabled
        {
            get => _snappingEnabled;
            set
            {
                _snappingEnabled = value;
                foreach( var handle in _handles.GetHandles<TranslationTransformHandle>() )
                {
                    handle.SnappingEnabled = value;
                }
            }
        }

        private float _snappingInterval = 0.25f;
        public float SnappingInterval
        {
            get => _snappingInterval;
            set
            {
                _snappingInterval = value;
                foreach( var handle in _handles.GetHandles<TranslationTransformHandle>() )
                {
                    handle.SnappingInterval = value;
                }
            }
        }

        private TransformHandleSet _handles;

        void Update()
        {
            if( _handles == null )
            {
                return;
            }

            if( Input.GetKeyDown( KeyCode.LeftShift ) )
            {
                SnappingEnabled = true;
            }
            if( Input.GetKeyUp( KeyCode.LeftShift ) )
            {
                SnappingEnabled = false;
            }

            Ray ray = _handles.RaycastCamera.ScreenPointToRay( Input.mousePosition );
            if( Input.GetKeyDown( KeyCode.Mouse0 ) )
            {
#warning TODO - raycast priority for handles - if clicked on handle, don't try to reattach them to a different object, or click on anything else.
                if( UnityEngine.Physics.Raycast( ray, out RaycastHit hitInfo, 8192, int.MaxValue ) )
                {
                    Transform clickedObj = hitInfo.collider.transform;

                    FClickInteractionRedirect r = clickedObj.GetComponent<FClickInteractionRedirect>();
                    if( r != null && r.Target != null )
                    {
                        clickedObj = r.Target.transform;
                    }

                    if( DesignObjectManager.IsLooseOrPartOfDesignObject( clickedObj ) )
                    {
                        if( _handles == null )
                            CreateHandles();

                        _handles.Target = clickedObj;
                        _handles.transform.position = clickedObj.position;
                        return;
                    }
                }
            }
        }

        void CreateHandles()
        {
            _handles = TransformHandleSet.Create( Vector3.zero, Quaternion.identity, null, null );
            _handles.CreateXYZHandles<TranslationTransformHandle>(
                AssetRegistry.Get<Mesh>( $"builtin::Resources/translate_handle_1d" ),
                AssetRegistry.Get<Material>( $"builtin::Resources/Materials/axis" ),
                go =>
                {
                    CapsuleCollider c = go.AddComponent<CapsuleCollider>();
                    c.radius = 0.3f;
                    c.height = 3.5f;
                    c.direction = 2;
                    c.center = new Vector3( 0, 0, c.height / 2 );
                } );
            _handles.RaycastCamera = GameObject.Find( "UI Camera" ).GetComponent<Camera>();
        }

        void OnEnable()
        {
            if( DesignObjectManager.DesignObject != null )
            {
                CreateHandles();
                var target = DesignObjectManager.DesignObject.RootPart;
                _handles.Target = target;
                _handles.transform.position = target.position;
            }
        }

        void OnDisable()
        {
            if( _handles != null )
            {
                _handles.Destroy();
                _handles = null;
            }
        }
    }
}