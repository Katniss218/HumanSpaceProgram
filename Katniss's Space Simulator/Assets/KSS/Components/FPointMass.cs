using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
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
            {
                ("mass", new Member<FPointMass, float>( o => o.Mass )),
                ("on_after_mass_changed", new Member<FPointMass, IHasMass.MassChange>( o => o.OnAfterMassChanged ))
            };
        }
    }
}