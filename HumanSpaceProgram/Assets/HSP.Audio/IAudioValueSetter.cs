using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Audio
{
    public interface IAudioValueSetter<T>
    {
        void Set( IAudioHandle audioHandle, T value );
    }

    public sealed class AudioPitchSetter : IAudioValueSetter<float>
    {
        public void Set( IAudioHandle audioHandle, float value )
        {
            audioHandle.Pitch = value;
        }


        [MapsInheritingFrom( typeof( AudioPitchSetter ) )]
        public static SerializationMapping AudioPitchSetterMapping()
        {
            return new MemberwiseSerializationMapping<AudioPitchSetter>();
        }
    }

    public sealed class AudioVolumeSetter : IAudioValueSetter<float>
    {
        public void Set( IAudioHandle audioHandle, float value )
        {
            audioHandle.Volume = value;
        }


        [MapsInheritingFrom( typeof( AudioVolumeSetter ) )]
        public static SerializationMapping AudioVolumeSetterMapping()
        {
            return new MemberwiseSerializationMapping<AudioVolumeSetter>();
        }
    }
}