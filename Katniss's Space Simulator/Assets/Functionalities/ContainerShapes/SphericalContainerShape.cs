using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Functionalities.ContainerShapes
{
    public class SphericalContainerShape : FBulkContainer.IShape
    {
        /// <summary>
        /// Calculates the height of a truncated unit sphere with the given volume as a [0..1] percentage of the unit sphere's volume.
        /// </summary>
        public static float SolveHeightOfTruncatedSphere( float volumePercent )
        {
            // https://math.stackexchange.com/questions/2364343/height-of-a-spherical-cap-from-volume-and-radius

            const float UnitSphereVolume = 4.18879020479f; // 4/3 * pi     -- radius=1
            const float TwoPi = 6.28318530718f;            // 2 * pi       -- radius=1
            const float Sqrt3 = 1.73205080757f;

            float Volume = UnitSphereVolume * volumePercent;

            float A = 1.0f - ((3.0f * Volume) / TwoPi); // A is a coefficient, [-1..1] for volumePercent in [0..1]
            float OneThirdArccosA = 0.333333333f * Mathf.Acos( A );
            float height = Sqrt3 * Mathf.Sin( OneThirdArccosA ) - Mathf.Cos( OneThirdArccosA ) + 1.0f;
            return height;
        }

        public FBulkContainer.IShape.SampleData Sample( Vector3 localPosition, Vector3 localAcceleration, float holeCrossSection, FBulkContainer container )
        {
            if( container.Contents.ResourceCount > 1 )
            {
                throw new NotImplementedException( "Handling multiple fluids isn't implemented yet." );
            }

            float heightOfLiquid = SolveHeightOfTruncatedSphere( container.Contents.Volume / container.MaxVolume );

            throw new NotImplementedException();
            // pressure in [Pa]
            float pressure = localAcceleration.magnitude * container.Contents.GetResources()[0].fluid.Density * heightOfLiquid;
            //return pressure;
        }
    }
}