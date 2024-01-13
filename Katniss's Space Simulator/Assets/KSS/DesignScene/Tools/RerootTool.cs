using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.DesignScene.Tools
{
    /// <summary>
    /// Allows to change the root of the design vessel.
    /// </summary>
    public class RerootTool : DesignSceneToolBase
    {
        [SerializeField]
        Camera _camera;

        void Awake()
        {
            _camera = GameObject.Find( "Near camera" ).GetComponent<Camera>();
        }

        void Update()
        {
            // click on part to set root.
            // Takes into account redirects ofc.
        }
    }
}