 using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
using KSS.Core.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    public static class BuildingEx
    {
        public static bool IsRootOfBuilding( this Transform part )
        {
            if( part.root != part.parent )
                return false;
            Building b = part.parent.GetComponent<Building>();
            if( b == null )
                return false;
            return b.RootPart == part;
        }

        /// <summary>
        /// Gets the <see cref="Building"/> attached to this transform.
        /// </summary>
        /// <returns>The building. Null if the transform is not part of a building.</returns>
        public static Building GetBuilding( this Transform part )
        {
            return part.root.GetComponent<Building>();
        }
    }

    /// <summary>
    /// Buildings are a lot like <see cref="Vessel"/>s, but anchored to the planet.
    /// </summary>
    [RequireComponent( typeof( PhysicsObject ) )]
    [RequireComponent( typeof( RootObjectTransform ) )]
    public partial class Building : MonoBehaviour, IPartObject
    {
        [SerializeField]
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; this.gameObject.name = value; }
        }

        [field: SerializeField]
        public Transform RootPart { get; private set; }

        public PhysicsObject PhysicsObject { get; private set; }
        public RootObjectTransform RootObjTransform { get; private set; }

        CelestialBody _referenceBody = null;
        public CelestialBody ReferenceBody
        {
            get => _referenceBody;
            set { _referenceBody = value; RecalculatePosition(); }
        }

        Vector3Dbl _referencePosition = Vector3.zero;
        public Vector3Dbl ReferencePosition
        {
            get => _referencePosition;
            set { _referencePosition = value; RecalculatePosition(); }
        }

        QuaternionDbl _referenceRotation = QuaternionDbl.identity;
        public QuaternionDbl ReferenceRotation
        {
            get => _referenceRotation;
            set { _referenceRotation = value; RecalculatePosition(); }
        }

        /// <remarks>
        /// DO NOT USE. This is for internal use, and can produce an invalid state. Use <see cref="VesselHierarchyUtils.SetParent(Transform, Transform)"/> instead.
        /// </remarks>
        [Obsolete( "This is for internal use, and can produce an invalid state." )]
        internal void SetRootPart( Transform part )
        {
            if( part != null && part.GetBuilding() != this )
            {
                throw new ArgumentException( $"Can't set the part '{part}' from building '{part.GetBuilding()}' as root. The part is not a part of this building." );
            }

            RootPart = part;
        }

        void RecalculatePosition()
        {
            if( ReferenceBody == null )
                return;

            this.RootObjTransform.AIRFPosition = ReferenceBody.OrientedReferenceFrame.TransformPosition( ReferencePosition );
            this.RootObjTransform.AIRFRotation = ReferenceBody.OrientedReferenceFrame.TransformRotation( ReferenceRotation );
        }

        void Awake()
        {
            this.RootObjTransform = this.GetComponent<RootObjectTransform>();
            this.PhysicsObject = this.GetComponent<PhysicsObject>();
            this.PhysicsObject.IsKinematic = true;
        }

        void OnEnable()
        {
            BuildingManager.Register( this );
        }

        void OnDisable()
        {
            try
            {
                BuildingManager.Unregister( this );
            }
            catch( InvalidSceneManagerException )
            {
                // scene unloaded.
            }
        }
    }
}