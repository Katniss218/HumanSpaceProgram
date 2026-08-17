using HSP.Content;
using HSP.Content.Vessels;
using HSP.Content.Vessels.Serialization;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace HSP.Vessels
{
    /// <summary>
    /// A marker component to track parts.
    /// </summary>
    public class VesselPart : MonoBehaviour
    {
#warning TODO - something needs to set the vessel.
        /// <summary>
        /// Gets or sets the vessel that this part belongs to. 
        /// </summary>
        public IPartGraph Graph { get; internal set; }

        public IVessel Vessel => Graph as IVessel;

        /// <summary>
        /// The ID of this part type (not instance).
        /// </summary>
        public NamespacedID PartID { get; set; }

        public PartMetadata GetPartMetadata()
        {
            if( PartRegistry.TryLoadMetadata( this.PartID, out var metadata ) )
                return metadata;

            Debug.LogError( $"Failed to load part metadata for FPart with part ID '{this.PartID}'." );
            return null;
        }

        private FComponent[] _components = Array.Empty<FComponent>();
        public IReadOnlyList<FComponent> Components => _components;

        private readonly FComponentCache _componentCache = new FComponentCache();

        public void SetComponents( FComponent[] components )
        {
            if( _components != null )
            {
                foreach( var comp in _components )
                {
                    comp?.OnDisable();
                }
            }
            _components = components ?? Array.Empty<FComponent>();
            foreach( var comp in _components )
            {
                if( comp != null )
                {
                    comp.Part = this;
                    comp.transform = this.transform;
                    comp.gameObject = this.gameObject;
                    comp.OnEnable();
                }
            }
            _componentCache.Clear();
            _componentCache.AddRange( _components );
        }

        public IReadOnlyList<T> GetFComponents<T>() where T : class
        {
            return _componentCache.Get<T>();
        }

        public static VesselPart GetPart( Transform obj )
        {
            return obj.GetComponentInParent<VesselPart>();
        }


        [MapsInheritingFrom( typeof( VesselPart ) )]
        public static IDescriptor VesselPartMapping()
        {
            return new MemberwiseDescriptor<VesselPart>()
                .WithMember( "part_id", o => o.PartID )
                .WithMember( "components", o => (FComponent[])o._components, ( VesselPart o, FComponent[] c ) => o.SetComponents( c ) );
        }
    }
}