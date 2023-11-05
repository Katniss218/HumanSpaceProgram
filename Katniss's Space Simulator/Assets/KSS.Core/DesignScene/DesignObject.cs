using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.DesignScene
{
    public class DesignObject : MonoBehaviour, IPartObject
    {
        [SerializeField]
        Transform _rootPart;
        public Transform RootPart
        {
            get => _rootPart;
            set
            {
                if( _rootPart != null )
                    _rootPart.SetParent( null );
                _rootPart = value;
                if( _rootPart != null )
                    _rootPart.SetParent( this.transform );
            }
        }

        public PhysicsObject PhysicsObject => throw new NotSupportedException( $"Design vessels can't move using the physics system." );
        public RootObjectTransform RootObjTransform { get; private set; }

        void Awake()
        {
            this.RootObjTransform = this.GetComponent<RootObjectTransform>();
        }
    }
}