using HSP.Vessels;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla
{
    public static class HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED
    {
        /// <summary>
        /// Invoked after the active object changes.
        /// </summary>
        public const string ID = HSPEvent.NAMESPACE_HSP + ".after_active_object_changed";
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

#warning TODO - add an interface for types that can be selected by the player? could be a vessel, but you could select part of the vessel to (to launch a specific sub-vessel or something).

        /// <summary>
        /// Gets or sets the object that is currently being 'controlled' or viewed by the player.
        /// </summary>
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
                HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED.ID );
            }
        }

        public static Vessel ActiveVessel
        {
            get => instance._activeVessel;
            set
            {
                if( value == instance._activeVessel )
                    return;

                instance._activeVessel = value;
                instance._activeObject = value == null ? null : value.ReferenceTransform;
                HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED.ID );
            }
        }

        [MapsInheritingFrom( typeof( ActiveVesselManager ) )]
        public static SerializationMapping ActiveObjectManagerMapping()
        {
            return new MemberwiseSerializationMapping<ActiveVesselManager>()
            {
                ("active_object", new Member<ActiveVesselManager, Transform>( ObjectContext.Ref, o => ActiveVesselManager.ActiveObject, (o, value) => ActiveVesselManager.ActiveObject = value ))
            };
        }
    }
}