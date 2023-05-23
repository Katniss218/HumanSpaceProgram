using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    [RequireComponent( typeof( RootObjectTransform ) )]
    public class CelestialBody : MonoBehaviour
    {
        public Vector3Dbl AIRFPosition { get => this._rootTransform.GetAIRFPosition(); set => this._rootTransform.SetAIRFPosition( value ); }

        public double Mass { get; set; }
        public double Radius { get; set; }

        RootObjectTransform _rootTransform;

        private void Awake()
        {
            _rootTransform = this.GetComponent<RootObjectTransform>();
        }
    }
}
