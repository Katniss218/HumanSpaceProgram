using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    /// <summary>
    /// Manages loading, unloading, switching, etc of vessels.
    /// </summary>
    public class VesselManager : SerializedManager, IPersistent
    {
        public static Vessel ActiveVessel { get; set; }

        static List<Vessel> Vessels { get; set; }

        public static Vessel[] GetVessels()
        {
            return Vessels.ToArray();
        }

        public static void RegisterVessel( Vessel vessel )
        {
            if( Vessels == null )
                Vessels = new List<Vessel>();

            Vessels.Add( vessel );
        }

        public static void UnregisterVessel( Vessel vessel )
        {
            if( Vessels != null )
                Vessels.Remove( vessel );
        }

#warning TODO - implement custom save/load methods for the managers, aggregate them into the file. Need to save the active vessel.
        internal static GameObject[] GetAllRootGameObjects()
        {

            GameObject[] gos = new GameObject[Vessels.Count];
            for( int i = 0; i < Vessels.Count; i++ )
            {
                gos[i] = Vessels[i].gameObject;
            }
            return gos;
        }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {
                { "active_vessel", s.WriteObjectReference( ActiveVessel ) }
            };
        }

        public void SetData( ILoader l, SerializedData data )
        {
            if( data.TryGetValue( "active_vessel", out var activeVessel ) )
                ActiveVessel = (Vessel)l.ReadObjectReference( activeVessel );
            throw new NotImplementedException();
        }
    }
}