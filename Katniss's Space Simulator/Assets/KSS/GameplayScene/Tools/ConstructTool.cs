using KSS.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.GameplayScene.Tools
{
    /// <summary>
    /// Allows to detach and attach parts.
    /// </summary>
    public class ConstructTool : GameplaySceneToolBase
    {
        Transform _heldPart = null;
        /// <summary>
        /// Gets or sets the part that's currently "held" by the cursor.
        /// </summary>
        public Transform HeldPart
        {
            get => _heldPart;
            set
            {
                if( _heldPart != null )
                {
                    Destroy( _heldPart.gameObject );
                }
                _heldPart = value;
                _heldOffset = Vector3.zero;
            }
        }

        Vector3 _heldOffset;

        FAttachNode[] _nodes;

        public bool SnappingEnabled { get; set; }
        public float SnapAngle { get; set; }

        Camera _camera;
        FAttachNode snappedToNode = null;

        void Awake()
        {
            _camera = GameObject.Find( "Near camera" ).GetComponent<Camera>();
        }

        void Update()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return;

            // construct tool gets assigned a ghost, and its job is to place it. So it's basically a "place ghost" tool.
            // adjusting the placed ghost can be done by the move/rotate tools.

            // placed ghost can be adjusted, this resets all build points (or can't be adjusted if it has any build points accumulated)

            // adjustment is done by a different tool.

        }

        void OnDisable() // if tool switched while trying to place new construction ghost
        {
            if( _heldPart != null )
            {
                Destroy( _heldPart.gameObject );
            }
        }

        void Place()
        {
            // place.

            GameplaySceneToolManager.UseTool<DefaultTool>();
        }
    }
}