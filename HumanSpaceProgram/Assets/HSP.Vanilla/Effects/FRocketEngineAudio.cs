using HSP.Effects;
using HSP.Effects.Audio;
using HSP.Vanilla.Components;
using HSP.Vessels;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Effects
{
    public sealed class RocketEngineThrustValueGetter : IValueGetter<float>
    {
        public FRocketEngine Engine { get; }

        public RocketEngineThrustValueGetter( FRocketEngine engine )
        {
            Engine = engine;
        }

        public float Get()
        {
            return Engine.Thrust / Engine.MaxThrust;
        }


        [MapsInheritingFrom( typeof( RocketEngineThrustValueGetter ) )]
        public static SerializationMapping RocketEngineThrustValueGetterMapping()
        {
            return new MemberwiseSerializationMapping<RocketEngineThrustValueGetter>()
                .WithReadonlyMember( "engine", ObjectContext.Ref, o => o.Engine )
                .WithFactory<FRocketEngine>( ( engine ) => new RocketEngineThrustValueGetter( engine ) );
        }
    }





    public class FRocketEngineAudio : MonoBehaviour
    {
        public IPropulsion Engine;

        public AudioEffectDefinition IgnitionAudio;
        public AudioEffectDefinition LoopAudio;
        public AudioEffectDefinition ShutdownAudio;

        AudioEffectHandle _ignitionHandle;
        AudioEffectHandle _loopHandle;
        AudioEffectHandle _shutdownHandle;

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
            Transform t = this.transform;
            if( IgnitionAudio != null && IgnitionAudio.TargetTransform == null )
                IgnitionAudio.TargetTransform = t;
            if( LoopAudio != null && LoopAudio.TargetTransform == null )
                LoopAudio.TargetTransform = t;
            if( ShutdownAudio != null && ShutdownAudio.TargetTransform == null )
                ShutdownAudio.TargetTransform = t;

            _ignitionHandle.TryStop();
            if( IgnitionAudio != null )
                _ignitionHandle = AudioEffectManager.Play( IgnitionAudio );

            _loopHandle.TryStop();
            if( LoopAudio != null )
                _loopHandle = AudioEffectManager.Play( LoopAudio );

            _shutdownHandle.TryStop();
        }

        void OnShutdown()
        {
            Transform t = this.transform;
            if( IgnitionAudio != null && IgnitionAudio.TargetTransform == null )
                IgnitionAudio.TargetTransform = t;
            if( LoopAudio != null && LoopAudio.TargetTransform == null )
                LoopAudio.TargetTransform = t;
            if( ShutdownAudio != null && ShutdownAudio.TargetTransform == null )
                ShutdownAudio.TargetTransform = t;

            _ignitionHandle.TryStop();

            _loopHandle.TryStop();

            _shutdownHandle.TryStop();
            if( ShutdownAudio != null )
                _shutdownHandle = AudioEffectManager.Play( ShutdownAudio );
        }


        [MapsInheritingFrom( typeof( FRocketEngineAudio ) )]
        public static SerializationMapping FRocketEngineAudioMapping()
        {
            return new MemberwiseSerializationMapping<FRocketEngineAudio>()
                .WithMember( "engine", ObjectContext.Ref, o => o.Engine )
                .WithMember( "ignition_audio", o => o.IgnitionAudio )
                .WithMember( "loop_audio", o => o.LoopAudio )
                .WithMember( "shutdown_audio", o => o.ShutdownAudio );
        }
    }
}