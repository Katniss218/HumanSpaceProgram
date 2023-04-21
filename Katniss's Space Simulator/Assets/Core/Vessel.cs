using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    public class Vessel : MonoBehaviour
    {
        [SerializeField]
        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set { _displayName = value; this.gameObject.name = value; }
        }

        [field: SerializeField]
        public Part RootPart { get; private set; }

        [field: SerializeField]
        public IEnumerable<Part> Parts { get; private set; }

        [field: SerializeField]
        public int PartCount { get; private set; }

        /// <remarks>
        /// DO NOT USE. This is for internal use, and can produce an invalid state. Use <see cref="VesselStateUtils.SetParent(Part, Part)"/> instead.
        /// </remarks>
        internal void SetRootPart( Part part )
        {
            /*if( part == null )
            {
                throw new ArgumentNullException( nameof(part), "Can't mark a null part as root" );
            }*/
            if( part != null && part.Vessel != this )
            {
                throw new ArgumentException( $"Can't set the part '{part}' from vessel '{part.Vessel}' as root. The part is not a part of this vessel." );
            }

            RootPart = part;
        }

        public void RecalculateParts()
        {
            // take root and traverse its hierarchy to add up all parts.
            //throw new NotImplementedException();
        }

        void Start()
        {

        }

        void Update()
        {

        }
    }
}