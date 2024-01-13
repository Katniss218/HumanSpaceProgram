using KSS.Core;
using KSS.Core.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityPlus.AssetManagement;

namespace KSS.GameplayScene
{
    public class NavballManager : SingletonMonoBehaviour<NavballManager>
    {
        // navball needs to be spawned (from code)
        // navball camera (and pivots and shit) need to be created (from code).
        // rendertexture needs to be created and assigned to the camera.

        // then something needs to hook into the UI to render the thing.

        // NavballManager manages the navball render texture (and by extension, the navball and navball camera).

        RenderTexture _attitudeIndicatorRT;
        Transform _navball;
        Transform _cameraPivot;

        public static RenderTexture AttitudeIndicatorRT { get => instance._attitudeIndicatorRT; }

        /// <summary>
        /// Gets or sets the orientation of the vessel (in the same space as <see cref="NavballOrientation"/>).
        /// </summary>
        public static Quaternion VesselOrientation { get => instance._cameraPivot.rotation; set => instance._cameraPivot.rotation = value; }

        /// <summary>
        /// Gets or sets the orientation of the attitude indicator (in the same space as <see cref="VesselOrientation"/>).
        /// </summary>
        /// <remarks>
        /// `up` is Up, `forward` is North, and `right` is East.
        /// </remarks>
        public static Quaternion NavballOrientation { get => instance._navball.rotation; set => instance._navball.rotation = value; }

        public static RenderTexture ResetAttitudeIndicatorRT()
        {
            RenderTexture renderTexture = new RenderTexture( 256, 256, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D16_UNorm );
            renderTexture.Create();

            instance._attitudeIndicatorRT = renderTexture;

            return renderTexture;
        }

        private static void CreateNavball()
        {
            GameObject navballObj = new GameObject( "navball" );
            navballObj.transform.localScale = new Vector3( -1, 1, 1 );

            MeshFilter mf = navballObj.AddComponent<MeshFilter>();
            mf.sharedMesh = AssetRegistry.Get<Mesh>( "builtin::Resources/Meshes/attitude_indicator2" );

            MeshRenderer mr = navballObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = AssetRegistry.Get<Material>( "builtin::Resources/Materials/attitude_indicator" );
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = false;

            navballObj.SetLayer( (int)Layer.HIDDEN_SPECIAL_1 );
            instance._navball = navballObj.transform;
        }

        private static void CreateNavballCamera()
        {
            GameObject pivotObj = new GameObject( "navball camera pivot" );

            GameObject cameraObj = new GameObject( "navball camera" );
            cameraObj.transform.SetParent( pivotObj.transform );
            cameraObj.transform.SetPositionAndRotation( new Vector3( 0, 0, 0 ), Quaternion.Euler( 0, 0, 0 ) );

            Camera camera = cameraObj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 100f;
            camera.farClipPlane = 125f;
            camera.cullingMask = 1 << (int)Layer.HIDDEN_SPECIAL_1;
            camera.targetTexture = AttitudeIndicatorRT;
            instance._cameraPivot = pivotObj.transform;
        }

        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, "vanilla.spawn_navball" )]
        public static void OnGameplayEnter()
        {
            GameplaySceneManager.GameObject.AddComponent<NavballManager>();

            ResetAttitudeIndicatorRT();
            CreateNavball();
            CreateNavballCamera();
        }

        void LateUpdate()
        {
            if( ActiveObjectManager.ActiveObject != null )
            {
                Vessel v = ActiveObjectManager.ActiveObject.transform.GetVessel();
                Vector3 forward = (Vector3)CelestialBodyManager.CelestialBodies.First().AIRFRotation.GetForwardAxis();
                Vector3 gravity = -GravityUtils.GetNBodyGravityAcceleration( v.AIRFPosition ).NormalizeToVector3();
                forward = Vector3.ProjectOnPlane( forward, gravity );
                NavballOrientation = Quaternion.LookRotation( forward, gravity );
                VesselOrientation = (Quaternion)v.AIRFRotation;
            }
        }
    }
}