using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.EmissionShapes
{
    public sealed class MeshEmissionShape : IParticleEffectEmissionShape
    {
        public enum SpawnLocation
        {
            Vertex,
            Edge,
            Triangle
        }

        public SpawnLocation SpawnFrom { get; set; } = SpawnLocation.Vertex;

        public Mesh Mesh { get; set; }

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
            shape.mesh = Mesh;
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
        }

        [MapsInheritingFrom( typeof( MeshEmissionShape ) )]
        public static SerializationMapping MeshEmissionShapeMapping()
        {
            return new MemberwiseSerializationMapping<MeshEmissionShape>()
                .WithMember( "spawn_from", o => o.SpawnFrom )
                .WithMember( "mesh", o => o.Mesh );
        }
    }
}