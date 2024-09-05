using HSP.CelestialBodies;
using HSP.SceneManagement;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vessels;
using UnityEngine;

namespace HSP._DevUtils
{
    public class GravityApplier : MonoBehaviour
    {
#warning TODO - remove this once the proper trajectory code is done.
        public Vessel Vessel { get; set; }

        void FixedUpdate()
        {
           // Vector3Dbl airfGravityForce = GravityUtils.GetNBodyGravityForce( Vessel.ReferenceFrameTransform.AbsolutePosition, Vessel.PhysicsTransform.Mass );
           // Vessel.PhysicsTransform.AddForce( (Vector3)airfGravityForce );
        }

        [HSPEventListener( HSPEvent_ON_VESSEL_CREATED.ID, "add_gravity_applier" )]
        public static void AddGravityApplier( Vessel vessel )
        {
            if( SceneLoader.IsSceneLoaded( GameplaySceneManager.SCENE_NAME ) )
            {
                GravityApplier g = vessel.gameObject.AddComponent<GravityApplier>();
                g.Vessel = vessel;
            }
        }
    }
}