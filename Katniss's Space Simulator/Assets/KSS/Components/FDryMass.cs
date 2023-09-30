using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Components
{
    public class FDryMass : MonoBehaviour, IHasMass
    {
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
    }
}