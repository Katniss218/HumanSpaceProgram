using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.Buildings
{
    public class LaunchSiteFactory
    {
        public GameObject Prefab { get; set; } // temp. replace with parameters and stuff, configs?

        public LaunchSite Create( CelestialBodySurface.FeatureGroup group, Vector3 localPosition, Quaternion localRotation )
        {
            GameObject launchSite = GameObject.Instantiate<GameObject>( Prefab, group.transform );
            launchSite.transform.localPosition = localPosition;
            launchSite.transform.localRotation = localRotation;

            LaunchSite ls = launchSite.GetComponent<LaunchSite>();
            return ls;
        }
    }
}