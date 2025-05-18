using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.EmissionShapes
{
    public sealed class MeshRendererEmissionShape : IParticleEffectEmissionShape
    {
        public enum SpawnLocation
        {
            Vertex,
            Edge,
            Triangle
        }

        public SpawnLocation SpawnFrom { get; set; } = SpawnLocation.Vertex;

        public MeshRenderer MeshRenderer { get; set; }

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
            shape.meshRenderer = MeshRenderer;
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;
        }

        [MapsInheritingFrom( typeof( MeshRendererEmissionShape ) )]
        public static SerializationMapping MeshRendererEmissionShapeMapping()
        {
            return new MemberwiseSerializationMapping<MeshRendererEmissionShape>()
                .WithMember( "spawn_from", o => o.SpawnFrom )
                .WithMember( "mesh_renderer", o => o.MeshRenderer );
        }
    }
}