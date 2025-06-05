using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.RenderModes
{
    /// <summary>
    /// Renders the particles as a mesh.
    /// </summary>
    public sealed class MeshRenderMode : IParticleEffectRenderMode
    {
        /// <summary>
        /// The meshes to use to draw the particles.
        /// </summary>
        /// <remarks>
        /// Each particle is assigned a mesh randomly.
        /// </remarks>
        public Mesh[] Meshes { get; set; }

        public void OnInit( ParticleEffectHandle handle )
        {
            handle.poolItem.renderer.renderMode = ParticleSystemRenderMode.Mesh;
            handle.poolItem.renderer.SetMeshes( Meshes );
            handle.poolItem.renderer.enableGPUInstancing = true;
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            // Do nothing
        }


        [MapsInheritingFrom( typeof( MeshRenderMode ) )]
        public static SerializationMapping MeshRenderModeMapping()
        {
            return new MemberwiseSerializationMapping<MeshRenderMode>()
                .WithMember( "meshes", ArrayContext.Assets, o => o.Meshes );
        }
    }
}