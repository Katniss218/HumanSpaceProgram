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

        IPhysicsTransform _physicsTransform;
        public IPhysicsTransform PhysicsTransform
        {
            get
            {
                if( _physicsTransform.IsUnityNull() )
                    _physicsTransform = this.GetComponent<IPhysicsTransform>();
                return _physicsTransform;
            }
        }

        IReferenceFrameTransform _referenceFrameTransform;
        public IReferenceFrameTransform ReferenceFrameTransform
        {
            get
            {
                if( _referenceFrameTransform.IsUnityNull() )
                    _referenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
                return _referenceFrameTransform;
            }
        }

        void Start()
        {
            if( this.ID == null )
                Debug.LogError( $"Celestial body '{this.gameObject.name}' has not been assigned an ID." );
            //this.gameObject.SetLayer( (int)Layer.CELESTIAL_BODY, true );

            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_CELESTIAL_BODY_CREATED.ID, this );
            //this.gameObject.SetLayer( (int)Layer.CELESTIAL_BODY, true );
        }

        private void OnDestroy()
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
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_CELESTIAL_BODY_DESTROYED.ID, this );
        }
         
        [MapsInheritingFrom( typeof( CelestialBody ) )]
        public static SerializationMapping CelestialBodyMapping()
        {
            return new MemberwiseSerializationMapping<CelestialBody>()
                .WithMember( "id", o => o.ID )
                .WithMember( "display_name", o => o.DisplayName )
                .WithMember( "mass", o => o.Mass )
                .WithMember( "radius", o => o.Radius );
        }
    }
}