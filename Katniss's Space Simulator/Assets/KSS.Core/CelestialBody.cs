using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    [RequireComponent( typeof( UnityPlus.Serialization.PreexistingReference ) )]
    [RequireComponent( typeof( RootObjectTransform ) )]
    public class CelestialBody : MonoBehaviour, IPersistent
    {
        public Vector3Dbl AIRFPosition { get => this._rootTransform.AIRFPosition; set => this._rootTransform.AIRFPosition = value; }
        public QuaternionDbl AIRFRotation { get => this._rootTransform.AIRFRotation; set => this._rootTransform.AIRFRotation = value; }

        public string Name { get; set; }
        public double Mass { get; set; }
        public double Radius { get; set; }

        RootObjectTransform _rootTransform;

        void Awake()
        {
            _rootTransform = this.GetComponent<RootObjectTransform>();
        }

        void OnEnable()
        {
            CelestialBodyManager.RegisterCelestialBody( this );
        }

        void OnDisable()
        {
            CelestialBodyManager.UnregisterCelestialBody( this );
        }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {

            };
            // save cb data itself.
        }

        public void SetData( ILoader l, SerializedData data )
        {

        }
    }
}
