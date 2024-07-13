using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.Core
{
    public sealed class SceneCamera : SingletonMonoBehaviour<SceneCamera>
    {
        [SerializeField]
        Camera _camera;

        /// <summary>
        /// Use this if you need to do camera projection transformations in the current scene.
        /// </summary>
        public static Camera Camera => instance._camera;
    }
}