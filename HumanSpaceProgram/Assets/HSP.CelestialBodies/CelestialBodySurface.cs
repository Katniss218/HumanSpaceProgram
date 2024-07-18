using System.Collections.Generic;
using UnityEngine;

namespace HSP.CelestialBodies
{
    [RequireComponent( typeof( CelestialBody ) )]
    public class CelestialBodySurface : MonoBehaviour
    {
        public class FeatureGroup : MonoBehaviour
        {
            // can be used for LODs of a group of objects instead of a single object?
        }


        private List<FeatureGroup> _groups = new List<FeatureGroup>();

        [field: SerializeField]
        public CelestialBody CelestialBody { get; private set; }

        public FeatureGroup SpawnGroup( string name, float latitude, float longitude, float altitude )
        {
            Vector3 localPos = CoordinateUtils.GeodeticToEuclidean( latitude, longitude, altitude );

            GameObject go = new GameObject( $"group, '{name}'" );
            go.transform.SetParent( this.transform );
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.FromToRotation( Vector3.up, localPos.normalized ); // in the future, would be good to use surface normal instead.

            FeatureGroup group = go.AddComponent<FeatureGroup>();

            _groups.Add( group );
            return group;
        }

        private void Awake()
        {
            this.CelestialBody = this.GetComponent<CelestialBody>();
        }
    }
}