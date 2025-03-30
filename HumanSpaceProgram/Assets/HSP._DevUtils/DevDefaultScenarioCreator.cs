using HSP.CelestialBodies;
using HSP.Content.Vessels;
using HSP.Content;
using HSP.Timelines;
using HSP.Timelines.Serialization;
using HSP.Vanilla;
using HSP.Vessels;
using UnityEngine;

namespace HSP._DevUtils
{
    public class DevDefaultScenarioCreator
    {
        /*[HSPEventListener( HSPEvent_AFTER_TIMELINE_NEW.ID, CREATE_PLACEHOLDER_UNIVERSE )]
        private static void OnAfterCreateDefault()
        {
            CelestialBody body = CelestialBodyManager.Get( "main" );
            Vector3 localPos = CoordinateUtils.GeodeticToEuclidean( 28.5857702f, -80.6507262f, (float)(body.Radius + 12.5) );

            launchSite = VesselFactory.CreatePartless( Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero );
            launchSite.gameObject.name = "launchsite";
            launchSite.Pin( body, localPos, Quaternion.FromToRotation( Vector3.up, localPos.normalized ) );

            GameObject root = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::testlaunchsite" ), launchSite.transform, Vector3.zero, Quaternion.identity );
            launchSite.RootPart = root.transform;

            _vessel = CreateVessel( launchSite );

            ActiveVesselManager.ActiveObject = _vessel.RootPart.GetVessel().gameObject.transform;
        }

        /// <summary>
        /// Builds a loadable scenario directory with all the files and shit.
        /// </summary>
        public static void CreateScenario()
        {
            ScenarioMetadata scenario = new ScenarioMetadata( NamespacedID.Parse( "Vanilla::default" ) )
            {
                Name = "The default scenario",
                Description = "",
                Author = "Katniss"
            };

            LoadGameplayScene();

            CreatePlanetarySystem();

            CreateVessels();

            Serialize();
        }*/
    }
}