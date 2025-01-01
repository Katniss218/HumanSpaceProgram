using HSP.Vessels;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    public static class HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED
    {
        /// <summary>
        /// Invoked after the active vessel changes.
        /// </summary>
        public const string ID = HSPEvent.NAMESPACE_HSP + ".after_active_vessel_changed";
    }

    /// <summary>
    /// Manages the vessel that's currently selected by the player (active).
    /// </summary>
    public class ActiveVesselManager : SingletonMonoBehaviour<ActiveVesselManager>
    {
        [SerializeField]
        private Vessel _activeVessel;

        [SerializeField]
        private Transform _activeObject;

        /// <summary>
        /// Gets or sets the active vessel.
        /// </summary>
        /// <remarks>
        /// The active object will be set to the reference transform of the active vessel.
        /// </remarks>
        public static Vessel ActiveVessel
        {
            get => instance._activeVessel;
            set
            {
                if( value == instance._activeVessel )
                    return;

                instance._activeVessel = value;
                instance._activeObject = value == null ? null : value.ReferenceTransform;
                HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED.ID );
            }
        }

        /// <summary>
        /// Gets or sets the part of the active vessel that is currently being 'controlled' or viewed by the player.
        /// </summary>
        /// <remarks>
        /// If the new active object is not part of the active vessel, the active vessel will also be set.
        /// </remarks>
        public static Transform ActiveObject
        {
            get => instance._activeObject;
            set
            {
                if( value == instance._activeObject )
                    return;

                Vessel vessel = null;
                if( value != null )
                {
                    vessel = value.GetVessel();
                    if( vessel == null )
                        throw new InvalidOperationException( $"Tried to set the active vessel to an object that's not part of any vessel." );
                }

                instance._activeVessel = vessel;
                instance._activeObject = value;
                HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED.ID );
            }
        }

        [MapsInheritingFrom( typeof( ActiveVesselManager ) )]
        public static SerializationMapping ActiveObjectManagerMapping()
        {
            return new MemberwiseSerializationMapping<ActiveVesselManager>()
                .WithMember( "active_object", ObjectContext.Ref, o => ActiveVesselManager.ActiveObject, ( o, value ) => ActiveVesselManager.ActiveObject = value );
        }
    }
}