﻿using KSS.Core;
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
    public class FPointMass : MonoBehaviour, IPersistent, IHasMass
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
            return new SerializedObject()
            {
                { "mass", this._mass },
                { "on_after_mass_changed", s.WriteDelegate( this.OnAfterMassChanged ) }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "mass", out var mass ) )
                this._mass = (float)mass;

            if( data.TryGetValue( "on_after_mass_changed", out var onAfterMassChanged ) )
                this.OnAfterMassChanged = (IHasMass.MassChange)l.ReadDelegate( onAfterMassChanged );
        }
    }
}