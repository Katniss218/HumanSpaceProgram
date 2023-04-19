using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <summary>
    /// A class responsible for instantiating a part from a source (save file, picked in VAB, etc).
    /// </summary>
    public class PartFactory
    {
        // Add source for the part's persistent data and modules.

        const string name = "tempname_part";

        public Part CreateRoot( Vessel vessel )
        {
            if( vessel.RootPart != null )
            {
                throw new InvalidOperationException( $"The vessel '{vessel}' already has a root part." );
            }

            Part part = Create( vessel.transform, Vector3.zero, Quaternion.identity );
            vessel.SetRootPart( part );

            return part;
        }

        public Part Create( Part parent )
        {
            Part part = Create( parent.Vessel.transform, Vector3.zero, Quaternion.identity );

            part.SetParent( parent );
            return part;
        }

        private Part Create( Transform parent, Vector3 localPosition, Quaternion localRotation )
        {
            GameObject partGO = new GameObject( $"Part, '{name}'" );
            partGO.transform.SetParent( parent );
            partGO.transform.localPosition = localPosition;
            partGO.transform.localRotation = localRotation;

            Part part = partGO.AddComponent<Part>();
            part.DisplayName = name;

            // Add modules, etc.

            return part;
        }

        public static void Destroy( Part part, bool keepChildren = false )
        {
            if( keepChildren )
            {
                foreach( var cp in part.Children )
                {
                    cp.SetParent( part.Parent );
                }
            }

            UnityEngine.Object.Destroy( part.gameObject );
        }
    }
}
