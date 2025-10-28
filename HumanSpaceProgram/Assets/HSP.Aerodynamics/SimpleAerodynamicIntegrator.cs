using HSP.ReferenceFrames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Aerodynamics
{
    /// <summary>
    /// Add this to a GameObject to enable simple aerodynamic force calculations.
    /// </summary>
    public class SimpleAerodynamicIntegrator : MonoBehaviour
    {
        public double DragCoefficient { get; set; }

        public double ReferenceArea { get; set; }

        IReferenceFrameTransform referenceFrameTransform;
        IPhysicsTransform physicsTransform;

        void Awake()
        {
            referenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
            physicsTransform = this.GetComponent<IPhysicsTransform>();
        }

        void FixedUpdate()
        {
            if( inAtmosphere )
            {
                Vector3 sceneForce = CalculateNetAerodynamicForce();
                physicsTransform.AddForce( sceneForce );
            }
        }
    }
}