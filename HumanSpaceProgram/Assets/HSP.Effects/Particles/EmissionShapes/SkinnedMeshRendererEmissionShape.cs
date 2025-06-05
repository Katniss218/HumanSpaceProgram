using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.EmissionShapes
{
    public sealed class SkinnedMeshRendererEmissionShape : IParticleEffectEmissionShape
    {
        public enum SpawnLocation
        {
            Vertex,
            Edge,
            Triangle
        }

        public SpawnLocation SpawnFrom { get; set; } = SpawnLocation.Vertex;

        public SkinnedMeshRenderer SkinnedMeshRenderer { get; set; }

        public void OnInit( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;

            shape.shapeType = ParticleSystemShapeType.Mesh;
            shape.meshShapeType = SpawnFrom switch
            {
                SpawnLocation.Vertex => ParticleSystemMeshShapeType.Vertex,
                SpawnLocation.Edge => ParticleSystemMeshShapeType.Edge,
                SpawnLocation.Triangle => ParticleSystemMeshShapeType.Triangle,
                _ => throw new ArgumentException( $"Invalid SpawnFrom '{SpawnFrom}'." )
            };
            shape.skinnedMeshRenderer = SkinnedMeshRenderer;
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
        }

        [MapsInheritingFrom( typeof( SkinnedMeshRendererEmissionShape ) )]
        public static SerializationMapping SkinnedMeshRendererEmissionShapeMapping()
        {
            return new MemberwiseSerializationMapping<SkinnedMeshRendererEmissionShape>()
                .WithMember( "spawn_from", o => o.SpawnFrom )
                .WithMember( "skinned_renderer", o => o.SkinnedMeshRenderer );
        }
    }
}