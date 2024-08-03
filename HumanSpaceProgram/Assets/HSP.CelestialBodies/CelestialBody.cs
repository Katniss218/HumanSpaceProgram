using HSP.ReferenceFrames;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.CelestialBodies
{
    public static class HSPEvent_AFTER_CELESTIAL_BODY_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".celestial_body_created.after";
    }

    public static class HSPEvent_AFTER_CELESTIAL_BODY_DESTROYED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".celestial_body_destroyed.after";
    }

    public class CelestialBody : MonoBehaviour
    {
        public string _id;
        /// <summary>
        /// Gets the id of the celestial body.
        /// </summary>
        public string ID
        {
            get => _id;
            set
            {
                if( value == null )
                    throw new ArgumentNullException( "value", "Can't assign a null ID." );

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
        public double Mass { get; internal set; } // TODO - add setters that fire events.

        /// <summary>
        /// Gets the radius of the celestial body.
        /// </summary>
        public double Radius { get; internal set; }

        public IPhysicsTransform PhysicsTransform { get; set; }
        public IReferenceFrameTransform ReferenceFrameTransform { get; set; }

        void Awake()
        {
            this.ReferenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
            this.PhysicsTransform = this.GetComponent<IPhysicsTransform>();
        }

        void Start()
        {
            this.ReferenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
            this.PhysicsTransform = this.GetComponent<IPhysicsTransform>();

            if( this.ID == null )
                Debug.LogError( $"Celestial body '{this.gameObject.name}' has not been assigned an ID." );
            //this.gameObject.SetLayer( (int)Layer.CELESTIAL_BODY, true );

            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_CELESTIAL_BODY_CREATED.ID, this );
            //this.gameObject.SetLayer( (int)Layer.CELESTIAL_BODY, true );
        }

        private void OnDestroy()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_CELESTIAL_BODY_DESTROYED.ID, this );
        }

        void OnDisable()
        {
            if( this.ID != null )
            {
                try
                {
                    CelestialBodyManager.Unregister( this.ID );
                }
                catch( SingletonInstanceException )
                {
                    // OnDisable was called when scene was unloaded, ignore.
                }
            }
        }

        /// <summary>
        /// Constructs the reference frame centered on this body, with axes aligned with the AIRF frame.
        /// </summary>
        public IReferenceFrame CenteredReferenceFrame => new CenteredReferenceFrame( this.ReferenceFrameTransform.AbsolutePosition );

        /// <summary>
        /// Constructs the reference frame centered on this body, with axes aligned with the body (i.e. local body space).
        /// </summary>
        public IReferenceFrame OrientedReferenceFrame => new OrientedReferenceFrame( this.ReferenceFrameTransform.AbsolutePosition, this.ReferenceFrameTransform.AbsoluteRotation );

        [MapsInheritingFrom( typeof( CelestialBody ) )]
        public static SerializationMapping CelestialBodyMapping()
        {
            return new MemberwiseSerializationMapping<CelestialBody>()
            {
            };
        }
    }
}
