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
        /// <summary>
        /// Gets the current global position of the celestial body.
        /// </summary>
        public Vector3Dbl AIRFPosition { get => this._rootTransform.AIRFPosition; set => this._rootTransform.AIRFPosition = value; }
        /// <summary>
        /// Gets the current global rotation of the celestial body.
        /// </summary>
        public QuaternionDbl AIRFRotation { get => this._rootTransform.AIRFRotation; set => this._rootTransform.AIRFRotation = value; }

        /// <summary>
        /// Gets or sets the name of the celestial body.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets the mass of the celestial body.
        /// </summary>
        public double Mass { get; internal set; }
        /// <summary>
        /// Gets the radius of the celestial body.
        /// </summary>
        public double Radius { get; internal set; }

        RootObjectTransform _rootTransform;
        PreexistingReference _ref;

        void Awake()
        {
            _rootTransform = this.GetComponent<RootObjectTransform>();
            _ref = this.GetComponent<PreexistingReference>();
        }

        void OnEnable()
        {
            CelestialBodyManager.RegisterCelestialBody( _ref.GetPersistentGuid(), this );
        }

        void OnDisable()
        {
            CelestialBodyManager.UnregisterCelestialBody( _ref.GetPersistentGuid() );
        }

        public SerializedData GetData( ISaver s )
        {
            // save cb data itself.
            return new SerializedObject()
            {

            };
        }

        public void SetData( ILoader l, SerializedData data )
        {

        }
    }
}
