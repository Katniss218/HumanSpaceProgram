using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.EmissionShapes
{
    public sealed class DonutEmissionShape : IParticleEffectEmissionShape
    {
        public enum SpawnLocation
        {
            Volume,    // fill the volume (or face)
            Shell,     // emit from the shell/edge/surface
        }

        public SpawnLocation SpawnFrom { get; set; } = SpawnLocation.Shell;

        public ConstantEffectValue<float> MajorRadius { get; set; } = new( 1f );
        public ConstantEffectValue<float> MinorRadius { get; set; } = new( 0.25f );
        /// <summary>
        /// 0 = only the outer ring emits; 1 = the entire torus volume emits.
        /// </summary>
        public ConstantEffectValue<float> RadiusThickness { get; set; } = new( 1f );

        public void OnInit( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Donut;

            if( MajorRadius != null )
            {
                MajorRadius.InitDrivers( handle );
                shape.radius = MajorRadius.Get();
            }
            if( MinorRadius != null )
            {
                MinorRadius.InitDrivers( handle );
                shape.donutRadius = MinorRadius.Get();
            }
            if( SpawnFrom == SpawnLocation.Volume && RadiusThickness != null )
            {
                RadiusThickness.InitDrivers( handle );
                shape.radiusThickness = Mathf.Clamp01( RadiusThickness.Get() );
            }
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;

            if( MajorRadius != null && MajorRadius.drivers != null )
                shape.radius = MajorRadius.Get();
            if( MinorRadius != null && MinorRadius.drivers != null )
                shape.donutRadius = MinorRadius.Get();
            if( SpawnFrom == SpawnLocation.Volume && RadiusThickness != null && RadiusThickness.drivers != null )
                shape.radiusThickness = Mathf.Clamp01( RadiusThickness.Get() );
        }

        [MapsInheritingFrom( typeof( DonutEmissionShape ) )]
        public static SerializationMapping DonutEmissionShapeMapping()
        {
            return new MemberwiseSerializationMapping<DonutEmissionShape>()
                .WithMember( "spawn_from", o => o.SpawnFrom )
                .WithMember( "major_radius", o => o.MajorRadius )
                .WithMember( "minor_radius", o => o.MinorRadius )
                .WithMember( "radius_thickness", o => o.RadiusThickness );
        }
    }
}
