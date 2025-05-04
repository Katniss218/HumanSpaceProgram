using HSP.Audio;
using HSP.Vanilla.Components;
using HSP.Vessels;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Effects
{
    public class FRocketEngineAudio : MonoBehaviour
    {
        FRocketEngine _engine;

        IAudioHandle _ignitionAudio;
        IAudioHandle _shutdownAudio;
        IAudioHandle _loopAudio;

        void OnEnable()
        {
            if( _engine == null )
                _engine = this.GetComponent<FRocketEngine>();

            _engine.OnAfterIgnite += OnIgnite;
            _engine.OnAfterShutdown += OnShutdown;
            _engine.OnAfterThrustChanged += OnThrustChanged;
        }

        void OnDisable()
        {
            if( _engine == null )
                return;

            _engine.OnAfterIgnite -= OnIgnite;
            _engine.OnAfterShutdown -= OnShutdown;
            _engine.OnAfterThrustChanged -= OnThrustChanged;
        }

        void OnIgnite()
        {
            if( _engine == null )
                return;

            _ignitionAudio?.TryStop();
            _ignitionAudio = AudioManager.PlayInWorld( AssetRegistry.Get<AudioClip>( "Vanilla::Assets/sounds/sound_liq8_enhanced" ), VesselManager.LoadedVessels.Skip( 1 ).First().ReferenceTransform, false, AudioChannel.Main_3D, 6 );

            _loopAudio?.TryStop();
            _loopAudio = AudioManager.PrepareInWorld( AssetRegistry.Get<AudioClip>( "Vanilla::Assets/sounds/kero_loop_hard" ), VesselManager.LoadedVessels.Skip( 1 ).First().ReferenceTransform, true, AudioChannel.Main_3D, (_engine.Thrust / _engine.MaxThrust) * 6 );
            _loopAudio.Play( 3, 6 );

            _shutdownAudio?.TryStop();
            _shutdownAudio = null;
        }

        void OnShutdown()
        {
            if( _engine == null )
                return;

            _ignitionAudio?.TryStop();
            _ignitionAudio = null;

            _loopAudio?.TryStop( 0, 0.5f );
            _loopAudio = null;

            _shutdownAudio?.TryStop();
            _shutdownAudio = AudioManager.PlayInWorld( AssetRegistry.Get<AudioClip>( "Vanilla::Assets/sounds/kero_flameout_hard" ), VesselManager.LoadedVessels.Skip( 1 ).First().ReferenceTransform, false, AudioChannel.Main_3D, 6 );
        }

        void OnThrustChanged()
        {
            if( _engine == null )
                return;

            if( _loopAudio != null )
                _loopAudio.Volume = (_engine.Thrust / _engine.MaxThrust) * 6;
        }


        [MapsInheritingFrom( typeof( FRocketEngineAudio ) )]
        public static SerializationMapping FRocketEngineAudioMapping()
        {
            return new MemberwiseSerializationMapping<FRocketEngineAudio>();
        }
    }
}