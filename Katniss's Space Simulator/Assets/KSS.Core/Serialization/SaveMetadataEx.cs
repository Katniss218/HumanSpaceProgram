﻿using KSS.Core.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core.Serialization
{
    public static class SaveMetadataEx
    {
        public static void LoadAsync( this SaveMetadata save )
        {
            SceneLoader.UnloadActiveSceneAsync( () => SceneLoader.LoadSceneAsync( "Testing And Shit", true, false, () =>
            {
                TimelineManager.BeginLoadAsync( save.TimelineID, save.SaveID );
            } ) );
        }
    }
}