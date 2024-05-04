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
    public class FPointMass : MonoBehaviour, IPersistsData, IHasMass
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

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "mass", _mass.GetData() },
                { "on_after_mass_changed", OnAfterMassChanged.GetData( s ) }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "mass", out var mass ) )
                _mass = mass.AsFloat();

            if( data.TryGetValue( "on_after_mass_changed", out var onAfterMassChanged ) )
                OnAfterMassChanged = (IHasMass.MassChange)onAfterMassChanged.AsDelegate( l );
        }
    }
}