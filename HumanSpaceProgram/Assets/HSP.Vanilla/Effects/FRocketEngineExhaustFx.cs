using HSP.Effects;
using HSP.Vanilla.Components;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Effects
{
    public class FRocketEngineExhaustFx : MonoBehaviour
    {
        public IPropulsion Engine;

        public IEffectData IgnitionSystem;
        public IEffectData LoopSystem;
        public IEffectData ShutdownSystem;

        IEffectHandle _ignitionHandle;
        IEffectHandle _loopHandle;
        IEffectHandle _shutdownHandle;

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

            _ignitionHandle?.TryStop();
            if( IgnitionSystem != null )
                _ignitionHandle = IgnitionSystem.Play();

            _loopHandle?.TryStop();
            if( LoopSystem != null )
                _loopHandle = LoopSystem.Play();

            _shutdownHandle?.TryStop();
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

            _ignitionHandle?.TryStop();

            _loopHandle?.TryStop();

            _shutdownHandle?.TryStop();
            if( ShutdownSystem != null )
                _shutdownHandle = ShutdownSystem.Play();
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