using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Audio
{

    public class ConstantEffectValue<T>
    {
        public float value;

        public EffectValueDriver<T>? driver;

        public ConstantEffectValue() { }

        public ConstantEffectValue( float value )
        {
            this.value = value;
        }

        public float Get()
        {
            // TODO - get the value from the driver
            throw new NotImplementedException();
        }


        [MapsInheritingFrom( typeof( ConstantEffectValue<> ) )]
        public static SerializationMapping ConstantEffectValueMapping<T>()
        {
            return new MemberwiseSerializationMapping<ConstantEffectValue<T>>()
                .WithMember( "value", o => o.value )
                .WithMember( "driver", o => o.driver );
        }
    }

    public class MinMaxEffectValue<T>
    {
        public float min;
        public float max;

        public EffectValueDriver<T>? driver;

        public MinMaxEffectValue() { }

        public MinMaxEffectValue( float value )
        {
            this.min = value;
            this.max = value;
        }

        public MinMaxEffectValue( float min, float max )
        {
            this.min = min;
            this.max = max;
        }

        public float GetMin()
        {
            // TODO - get the value from the driver
            throw new NotImplementedException();
        }

        public float GetMax()
        {
            // TODO - get the value from the driver
            throw new NotImplementedException();
        }

        public (float min, float max) Get()
        {
            // driver drives both min and max together.

            // TODO - get the value from the driver
            throw new NotImplementedException();
        }


        [MapsInheritingFrom( typeof( MinMaxEffectValue<> ) )]
        public static SerializationMapping MinMaxEffectValueMapping<T>()
        {
            return new MemberwiseSerializationMapping<MinMaxEffectValue<T>>()
                .WithMember( "min", o => o.min )
                .WithMember( "max", o => o.max )
                .WithMember( "driver", o => o.driver );
        }
    }

    public struct EffectValueDriver<T>
    {
        // translate them to builtin unity particle system stuff.
        public IValueGetter<T> Getter { get; set; }
        public AnimationCurve Curve { get; set; }

        [MapsInheritingFrom( typeof( EffectValueDriver<> ) )]
        public static SerializationMapping ParticleEffectShaperMapping<T>()
        {
            return new MemberwiseSerializationMapping<EffectValueDriver<T>>()
                .WithMember( "getter", o => o.Getter )
                .WithMember( "curve", o => o.Curve );
        }
    }

    /*
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
    }*/
}