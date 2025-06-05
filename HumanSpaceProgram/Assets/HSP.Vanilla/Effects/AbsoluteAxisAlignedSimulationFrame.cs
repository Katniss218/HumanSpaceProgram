using HSP.Effects.Particles;
using HSP.ReferenceFrames;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Effects
{
    public sealed class AbsoluteAxisAlignedSimulationFrame : HSP.Effects.Particles.SimulationFrames.CustomSimulationFrame
    {
        protected override void OnUpdateInternal( ParticleEffectHandle handle, Transform customFrame )
        {
            if( SceneReferenceFrameManager.ReferenceFrame != null )
            {
                customFrame.rotation = (Quaternion)SceneReferenceFrameManager.ReferenceFrame.InverseTransformRotation( QuaternionDbl.identity );
            }
        }


        [MapsInheritingFrom( typeof( AbsoluteAxisAlignedSimulationFrame ) )]
        public static SerializationMapping AbsoluteAxisAlignedSimulationFrameMapping()
        {
            return new MemberwiseSerializationMapping<AbsoluteAxisAlignedSimulationFrame>();
        }
    }
}