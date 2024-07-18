using HSP.Core;
using HSP.Core.ReferenceFrames;
using System;
using UnityEngine;

namespace HSP.Vessels
{
    public class DesignVessel : MonoBehaviour, IVessel
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

        public Transform ReferenceTransform => this.transform;

        public IPhysicsObject PhysicsObject { get => throw new NotImplementedException( "implement via a nonmoving phys object." ); set => throw new NotImplementedException( "implement via a nonmoving phys object." ); }

        public RootObjectTransform RootObjTransform { get => throw new NotImplementedException( "implement via a nonmoving phys object." ); }

        void Awake()
        {
            this.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );
        }

        public void RecalculatePartCache()
        {
            // do nothing (for now)
        }
    }
}