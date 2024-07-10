using HSP.Core.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSP.Core.Serialization
{
    public static class SaveMetadataEx
    {
        public static void LoadAsync( this SaveMetadata save )
        {
            SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( GameplaySceneManager.SCENE_NAME, true, false, () =>
            {
                TimelineManager.BeginLoadAsync( save.TimelineID, save.SaveID );
            } ) );
        }
    }
}