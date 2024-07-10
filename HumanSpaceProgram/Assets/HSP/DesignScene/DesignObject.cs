using HSP.Core;
using HSP.Core.Physics;
using HSP.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.DesignScene
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

        public Transform ReferenceTransform => this.transform;

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