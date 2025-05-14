using HSP.Audio;
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

    

    

    public class ParticleSystemEffectShaper
    {
        // particle plumes
    }

    public class MeshEffectShaper
    {
        // mesh plumes
    }

    public class FRocketEngineAudio : MonoBehaviour
    {
        public IPropulsion Engine;

        public AudioEffectDefinition IgnitionAudio;
        public AudioEffectDefinition LoopAudio;
        public AudioEffectDefinition ShutdownAudio;

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
            if( Engine == null )
                return;

            Transform t = this.transform.GetVessel().ReferenceTransform;

            IgnitionAudio?.TryStop();
            IgnitionAudio?.PlayInWorld( t );

            LoopAudio?.TryStop();
            LoopAudio?.PlayInWorld( t );

            ShutdownAudio?.TryStop();
        }

        void OnShutdown()
        {
            if( Engine == null )
                return;

            IgnitionAudio?.TryStop();

            LoopAudio?.TryStop();

            ShutdownAudio?.TryStop();
            ShutdownAudio?.PlayInWorld( this.transform.GetVessel().ReferenceTransform );
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