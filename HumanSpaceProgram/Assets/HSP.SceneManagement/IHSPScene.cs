using UnityEngine.SceneManagement;

namespace HSP.SceneManagement
{
    public interface IHSPScene
    {
        Scene UnityScene { get; set; }
        void _onload();
        void _onunload();
        void _onactivate();
        void _ondeactivate();
    }
}