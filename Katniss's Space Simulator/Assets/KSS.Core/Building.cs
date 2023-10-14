using KSS.Core.Physics;
using KSS.Core.ReferenceFrames;
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
    /// Buildings are a lot like vessels, but anchored to the planet.
    /// </summary>
    [RequireComponent( typeof( PhysicsObject ) )]
    public class Building : MonoBehaviour, IPartObject
    {

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
        Vector3Dbl _localPosition = Vector3.zero;
        public Vector3Dbl LocalPosition
        {
            get => _localPosition;
            set { _localPosition = value; RecalculatePosition(); }
        }
        QuaternionDbl _localRotation = QuaternionDbl.identity;
        public QuaternionDbl LocalRotation
        {
            get => _localRotation;
            set { _localRotation = value; RecalculatePosition(); }
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

            this.RootObjTransform.AIRFPosition = ReferenceBody.OrientedReferenceFrame.TransformPosition( LocalPosition );
            this.RootObjTransform.AIRFRotation = ReferenceBody.OrientedReferenceFrame.TransformRotation( LocalRotation );
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
            BuildingManager.Unregister( this );
        }
    }
}
