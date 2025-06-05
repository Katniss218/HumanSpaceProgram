using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.RenderModes
{
    public enum BillboardAlignMode
    {
        /// <summary>
        /// Faces the camera head-on.
        /// </summary>
        FaceCamera,
        /// <summary>
        /// Faces the camera, but is always perpendicular to emitter's forward direction.
        /// </summary>
        FaceCameraNoTilt,
        /// <summary>
        /// Faces the emitter's forward direction.
        /// </summary>
        FaceEmitterForward,
    }

    /// <summary>
    /// Renders the particles as a flat quad aligned in the specified direction.
    /// </summary>
    public sealed class BillboardRenderMode : IParticleEffectRenderMode
    {
        public BillboardAlignMode Alignment { get; set; } = BillboardAlignMode.FaceCamera;

        public void OnInit( ParticleEffectHandle handle )
        {
            handle.poolItem.renderer.renderMode = Alignment switch
            {
                BillboardAlignMode.FaceCamera => ParticleSystemRenderMode.Billboard,
                BillboardAlignMode.FaceEmitterForward => ParticleSystemRenderMode.HorizontalBillboard,
                BillboardAlignMode.FaceCameraNoTilt => ParticleSystemRenderMode.VerticalBillboard,
                _ => throw new ArgumentException($"Invalid AlignMode '{Alignment}'.")
            };
            handle.poolItem.renderer.enableGPUInstancing = true;
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            // Do nothing
        }


        [MapsInheritingFrom( typeof( BillboardRenderMode ) )]
        public static SerializationMapping BillboardRenderModeMapping()
        {
            return new MemberwiseSerializationMapping<BillboardRenderMode>()
                .WithMember( "alignment", o => o.Alignment );
        }
    }
}