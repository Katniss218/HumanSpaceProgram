using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.EmissionShapes
{
    public sealed class ConeEmissionShape : IParticleEffectEmissionShape
    {
        public ParticleEffectSpawnLocation SpawnFrom { get; set; } = ParticleEffectSpawnLocation.Base;

        public ConstantEffectValue<float> Radius { get; set; } = new( 1 );
        public ConstantEffectValue<float> Height { get; set; } = null;
        public ConstantEffectValue<float> Angle { get; set; } = new( 0 );
        public ConstantEffectValue<float> InnerAngle { get; set; } = null;

        public void OnInit( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;
            shape.shapeType = SpawnFrom switch
            {
                ParticleEffectSpawnLocation.Base => ParticleSystemShapeType.Cone,
                ParticleEffectSpawnLocation.Volume => ParticleSystemShapeType.ConeVolume,
                _ => throw new ArgumentException( $"Invalid SpawnFrom '{SpawnFrom}'." )
            };

            if( Radius != null )
                shape.radius = Radius.Get();
            if( Height != null )
                shape.length = Height.Get();
            if( Angle != null )
                shape.angle = Angle.Get();
            // shape.radiusThickness = 1.0f - (InnerAngle.Get() / shape.angle);
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;

            if( Radius != null )
                shape.radius = Radius.Get();
            if( Height != null )
                shape.length = Height.Get();
            if( Angle != null )
                shape.angle = Angle.Get();
            // shape.radiusThickness = 1.0f - (InnerAngle.Get() / shape.angle);
        }


        [MapsInheritingFrom( typeof( ConeEmissionShape ) )]
        public static SerializationMapping ConeEmissionShapeMapping()
        {
            return new MemberwiseSerializationMapping<ConeEmissionShape>()
                .WithMember( "spawn_from", o => o.SpawnFrom )
                .WithMember( "radius", o => o.Radius )
                .WithMember( "height", o => o.Height )
                .WithMember( "angle", o => o.Angle )
                .WithMember( "inner_angle", o => o.InnerAngle );
        }
    }
}