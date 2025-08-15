using HSP.Effects.Particles;
using HSP.ReferenceFrames;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Effects
{
    public sealed class AbsoluteAxisAlignedSimulationFrame : HSP.Effects.Particles.SimulationFrames.CustomSimulationFrame
    {
        public ISceneReferenceFrameProvider SceneReferenceFrameProvider { get; set; }

        protected override void OnUpdateInternal( ParticleEffectHandle handle, Transform customFrame )
        {
            var sceneReferenceFrame = SceneReferenceFrameProvider?.GetSceneReferenceFrame();
            if( sceneReferenceFrame != null )
            {
                customFrame.rotation = (Quaternion)sceneReferenceFrame.InverseTransformRotation( QuaternionDbl.identity );
            }
        }


        [MapsInheritingFrom( typeof( AbsoluteAxisAlignedSimulationFrame ) )]
        public static SerializationMapping AbsoluteAxisAlignedSimulationFrameMapping()
        {
            return new MemberwiseSerializationMapping<AbsoluteAxisAlignedSimulationFrame>()
                .WithMember("scene_reference_Frame_provider", o => o.SceneReferenceFrameProvider );
        }
    }
}