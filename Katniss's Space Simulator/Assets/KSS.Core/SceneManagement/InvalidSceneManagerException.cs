using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSS.Core.SceneManagement
{
    /// <summary>
    /// Thrown when a manager access is invalid for the current scene.
    /// </summary>
    public class InvalidSceneManagerException : Exception
    {
        public InvalidSceneManagerException()
        {

        }

        public InvalidSceneManagerException( string message ) : base( message )
        {

        }

        public InvalidSceneManagerException( string message, Exception inner ) : base( message, inner )
        {

        }
    }
}