using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public class BuildingManager : SerializedManager, IPersistent
    {
        static List<Building> _loadedBuildings;

        public static Building[] GetLoadedBuildings()
        {
            return _loadedBuildings.ToArray();
        }

        internal static void Register( Building vessel )
        {
            if( _loadedBuildings == null )
                _loadedBuildings = new List<Building>();

            _loadedBuildings.Add( vessel );
        }

        internal static void Unregister( Building vessel )
        {
            if( _loadedBuildings != null )
                _loadedBuildings.Remove( vessel );
        }

        internal static GameObject[] GetAllRootGameObjects()
        {

            GameObject[] gos = new GameObject[_loadedBuildings.Count];
            for( int i = 0; i < _loadedBuildings.Count; i++ )
            {
                gos[i] = _loadedBuildings[i].gameObject;
            }
            return gos;
        }

        public SerializedData GetData( ISaver s )
        {
            return new SerializedObject()
            {

            };
        }

        public void SetData( ILoader l, SerializedData data )
        {

        }
    }
}