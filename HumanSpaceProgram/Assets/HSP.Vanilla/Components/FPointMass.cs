using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// Adds a point mass of the specified mass to the object.
    /// </summary>
    public class FPointMass : MonoBehaviour, IHasMass
    {
        [SerializeField]
        private float _mass;
        public float Mass
        {
            get => _mass;
            set
            {
                float oldMass = this._mass;
                this._mass = value;
                this.OnAfterMassChanged?.Invoke( this._mass - oldMass );
            }
        }

        public event IHasMass.MassChange OnAfterMassChanged = null;


        [MapsInheritingFrom( typeof( FPointMass ) )]
        public static SerializationMapping FPointMassMapping()
        {
            return new MemberwiseSerializationMapping<FPointMass>()
                .WithMember( "mass", o => o.Mass )
                .WithMember( "on_after_mass_changed", o => o.OnAfterMassChanged );
        }
    }
}