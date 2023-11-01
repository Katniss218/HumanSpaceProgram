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

        public string _id;
        /// <summary>
        /// Gets the id of the celestial body.
        /// </summary>
        public string ID
        {
            get => _id;
            set
            {
                if( value == null ) throw new ArgumentNullException( "value", "Can't assign a null ID." );

                if( _id != null )
                    CelestialBodyManager.Unregister( _id );
                _id = value;
                CelestialBodyManager.Register( this );
            }
        }

        /// <summary>
        /// Gets or sets the display name of the celestial body.
        /// </summary>
        public string DisplayName { get; set; }
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

        void Start()
        {
            if( this.ID == null )
                Debug.LogError( $"Celestial body '{this.gameObject.name}' has not been assigned an ID." );
        }

        void OnDisable()
        {
            if( this.ID != null )
            {
                try
                {
                    CelestialBodyManager.Unregister( this.ID );
                }
                catch( InvalidOperationException ex )
                {
                    // OnDisable was called when scene was unloaded, ignore.
                }
            }
        }

        /// <summary>
        /// Constructs the reference frame centered on this body, with axes aligned with the AIRF frame.
        /// </summary>
        public IReferenceFrame CenteredReferenceFrame => new CenteredReferenceFrame( this.AIRFPosition );

        /// <summary>
        /// Constructs the reference frame centered on this body, with axes aligned with the body.
        /// </summary>
        public IReferenceFrame OrientedReferenceFrame => new OrientedReferenceFrame( this.AIRFPosition, this.AIRFRotation );

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
