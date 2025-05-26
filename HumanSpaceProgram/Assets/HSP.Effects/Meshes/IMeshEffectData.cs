
namespace HSP.Effects.Meshes
{
    public interface IMeshEffectData : IEffectData<MeshEffectHandle>
    {
        public BoneData[] Bones { get; }
    }
}