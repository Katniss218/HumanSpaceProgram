using HSP.Core.SceneManagement;
using HSP.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSP.Vanilla
{
    public static class SaveMetadataEx
    {
        public static void LoadAsync( this SaveMetadata save )
        {
            //SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( GameplaySceneManager.SCENE_NAME, true, false, () =>
            SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( "Testing And Shit", true, false, () =>
            {
                TimelineManager.BeginLoadAsync( save.TimelineID, save.SaveID );
            } ) );
        }
    }
}