using HSP.Effects.Audio;
using HSP.Effects.Particles;
using HSP.Vanilla.Components;
using HSP.Vessels;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Effects
{
    public class FRocketEngineExhaustFx : MonoBehaviour
    {
        public IPropulsion Engine;

        public ParticleEffectDefinition IgnitionSystem;
        public ParticleEffectDefinition LoopSystem;
        public ParticleEffectDefinition ShutdownSystem;

        ParticleEffectHandle _ignitionHandle;
        ParticleEffectHandle _loopHandle;
        ParticleEffectHandle _shutdownHandle;

        void OnEnable()
        {
            if( Engine == null )
                Engine = this.GetComponent<IPropulsion>();

            Engine.OnAfterIgnite += OnIgnite;
            Engine.OnAfterShutdown += OnShutdown;
        }

        void OnDisable()
        {
            if( Engine == null )
                return;

            Engine.OnAfterIgnite -= OnIgnite;
            Engine.OnAfterShutdown -= OnShutdown;
        }

        void OnIgnite()
        {
            Transform t = Engine.ThrustTransform;
            if( IgnitionSystem != null && IgnitionSystem.TargetTransform == null )
                IgnitionSystem.TargetTransform = t;
            if( LoopSystem != null && LoopSystem.TargetTransform == null )
                LoopSystem.TargetTransform = t;
            if( ShutdownSystem != null && ShutdownSystem.TargetTransform == null )
                ShutdownSystem.TargetTransform = t;

            _ignitionHandle.TryStop();
            if( IgnitionSystem != null )
                _ignitionHandle = ParticleEffectManager.Play( IgnitionSystem );

            _loopHandle.TryStop();
            if( LoopSystem != null )
                _loopHandle = ParticleEffectManager.Play( LoopSystem );

            _shutdownHandle.TryStop();
        }

        void OnShutdown()
        {
            Transform t = Engine.ThrustTransform;
            if( IgnitionSystem != null && IgnitionSystem.TargetTransform == null )
                IgnitionSystem.TargetTransform = t;
            if( LoopSystem != null && LoopSystem.TargetTransform == null )
                LoopSystem.TargetTransform = t;
            if( ShutdownSystem != null && ShutdownSystem.TargetTransform == null )
                ShutdownSystem.TargetTransform = t;

            _ignitionHandle.TryStop();

            _loopHandle.TryStop();

            _shutdownHandle.TryStop();
            if( ShutdownSystem != null )
                _shutdownHandle = ParticleEffectManager.Play( ShutdownSystem );
        }


        [MapsInheritingFrom( typeof( FRocketEngineExhaustFx ) )]
        public static SerializationMapping FRocketEngineExhaustFxMapping()
        {
            return new MemberwiseSerializationMapping<FRocketEngineExhaustFx>()
                .WithMember( "engine", ObjectContext.Ref, o => o.Engine )
                .WithMember( "ignition_system", o => o.IgnitionSystem )
                .WithMember( "loop_system", o => o.LoopSystem )
                .WithMember( "shutdown_system", o => o.ShutdownSystem );
        }
    }
}