using UnityEngine;

namespace HSP.Effects.Particles
{
    public static class EffectValue_Ex
    {
        public static ParticleSystem.MinMaxCurve GetMinMaxCurve( this ConstantEffectValue<float> value )
        {
            return new ParticleSystem.MinMaxCurve( value.Get() );
        }

        public static ParticleSystem.MinMaxCurve GetMinMaxCurve( this MinMaxEffectValue<float> value )
        {
            var minMax = value.Get();
            return new ParticleSystem.MinMaxCurve( minMax.min, minMax.max );
        }

        public static ParticleSystem.MinMaxGradient GetMinMaxGradient( this ConstantEffectValue<Color> value )
        {
            return new ParticleSystem.MinMaxGradient( value.Get() );
        }

        public static ParticleSystem.MinMaxGradient GetMinMaxGradient( this MinMaxEffectValue<Color> value )
        {
            var minMax = value.Get();
            return new ParticleSystem.MinMaxGradient( minMax.min, minMax.max );
        }
    }
}