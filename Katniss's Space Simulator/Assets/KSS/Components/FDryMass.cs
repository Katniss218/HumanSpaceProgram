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
    public class FDryMass : MonoBehaviour, IPersistent, IHasMass
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
                this.OnAfterMassChanged( this._mass - oldMass );
            }
        }

        public event IHasMass.MassChange OnAfterMassChanged;

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {
                { "mass", this.Mass },
                { "on_after_mass_changed", s.WriteDelegate( this.OnAfterMassChanged ) }
            };
        }

        public void SetData( ILoader l, SerializedData data )
        {
            if( data.TryGetValue( "on_after_mass_changed", out var onAfterMassChanged ) )
                this.OnAfterMassChanged = (IHasMass.MassChange)l.ReadDelegate( onAfterMassChanged );

            if( data.TryGetValue( "mass", out var mass ) )
                this.Mass = (float)mass;
        }
    }
}