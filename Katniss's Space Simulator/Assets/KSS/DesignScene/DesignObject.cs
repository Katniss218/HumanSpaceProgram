using KSS.Core;
using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.DesignScene
{
    //[RequireComponent( typeof( RootObjectTransform ) )]
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

        public IPhysicsObject PhysicsObject => throw new NotSupportedException( $"Design objects can't move using the physics system." );
        public RootObjectTransform RootObjTransform => throw new NotSupportedException( $"Design objects can't use the reference frames." );

        void Awake()
        {
           // this.RootObjTransform = this.GetComponent<RootObjectTransform>();
        }
    }
}