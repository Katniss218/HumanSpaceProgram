using HSP.Core;
using UnityEngine;

namespace HSP.Vessels
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