using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <summary>
    /// A class grouping methods related specifically to coordinates that are relevant to the terrain system.
    /// </summary>
    public static class CoordinateUtils
    {
        /// <summary>
        /// Z+ points towards the north pole (north is up).
        /// </summary>
        public static Vector3 GeodeticToEuclidean( float latitude, float longitude, float altitude )
        {
            // ECEF but missing the oblateness.
            float latRad = latitude * Mathf.Deg2Rad;
            float lonRad = longitude * Mathf.Deg2Rad;

            float x = -(altitude * Mathf.Cos( latRad ) * Mathf.Cos( lonRad ));
            float y = altitude * Mathf.Cos( latRad ) * Mathf.Sin( lonRad );
            float z = altitude * Mathf.Sin( latRad );

            return new Vector3( x, y, z );
        }

        /// <summary>
        /// Z+ points towards the north pole (north is up).
        /// </summary>
        public static (float latitude, float longitude, float altitude) EuclideanToGeodetic( Vector3 v )
        {
            float x = v.x;
            float y = v.y;
            float z = v.z;

            float altitude = Mathf.Sqrt( x * x + y * y + z * z );

            float theta = -Mathf.Atan2( y, x ) + Mathf.PI;
            float phi = -Mathf.Acos( z / altitude );

            return (theta * Mathf.Rad2Deg, phi * Mathf.Rad2Deg, altitude);
        }
    }
}