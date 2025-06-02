
using System.Collections.Generic;

namespace HSP.Effects.Meshes
{
    public interface IMeshEffectData : IEffectData<MeshEffectHandle>
    {
        public bool IsSkinned { get; }
        public IReadOnlyList<BindPose> BoneBindPoses { get; }
    }
}