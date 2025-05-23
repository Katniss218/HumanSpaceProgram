using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.EmissionShapes
{
    public sealed class BoxEmissionShape : IParticleEffectEmissionShape
    {
        public enum SpawnLocation
        {
            Volume,    // fill the volume (or face)
            Shell,     // emit from the shell/edge/surface
            Edge
        }

        /// <summary>
        /// Volume = fill the inside of the box
        /// Shell  = emit only from the six faces (thin shell)
        /// </summary>
        public SpawnLocation SpawnFrom { get; set; } = SpawnLocation.Shell;

        /// <summary>
        /// Half‐extents of the box along each axis (X, Y, Z).
        /// </summary>
        public ConstantEffectValue<float> Size { get; set; } = new( 1.0f );

        public void OnInit( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;
            shape.shapeType = SpawnFrom switch
            {
                SpawnLocation.Volume => ParticleSystemShapeType.Box,
                SpawnLocation.Shell => ParticleSystemShapeType.BoxShell,
                SpawnLocation.Edge => ParticleSystemShapeType.BoxEdge,
                _ => throw new ArgumentException( $"Invalid SpawnFrom '{SpawnFrom}'." )
            };

            if( Size != null )
            {
                float val = Size.Get();
                shape.scale = new Vector3( val, val, val );
            }
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;

            if( Size != null && Size.drivers != null )
            {
                float val = Size.Get();
                shape.scale = new Vector3( val, val, val );
            }
        }

        [MapsInheritingFrom( typeof( BoxEmissionShape ) )]
        public static SerializationMapping BoxEmissionShapeMapping()
        {
            return new MemberwiseSerializationMapping<BoxEmissionShape>()
                .WithMember( "spawn_from", o => o.SpawnFrom )
                .WithMember( "size", o => o.Size );
        }
    }
}