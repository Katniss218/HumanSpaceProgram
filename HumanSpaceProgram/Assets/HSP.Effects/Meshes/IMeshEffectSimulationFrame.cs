
namespace HSP.Effects.Meshes
{
    public interface IMeshEffectSimulationFrame
    {
        public void OnInit( MeshEffectHandle handle );
        public void OnUpdate( MeshEffectHandle handle );
    }
}