using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    /// <summary>
    /// Contains the data about symmetry in vessels/buildings.
    /// </summary>
    public class SymmetryDataContainer : MonoBehaviour, IPersistent
    {
        private Dictionary<GameObject, List<GameObject>> _groups = new Dictionary<GameObject, List<GameObject>>();

        // The dictionary is set up in such a way that every symmetry object in a symmetry "group" is assigned the same list.
        // And it's using a dictionary because otherwise we'd have a list of lists, and searching would be O(n) or O(n^2), depending on implementation, instead of O(1).

        /// <summary>
        /// Returns every object in the same symmetry group as the given object, including the specified object.
        /// </summary>
        public GameObject[] GetCounterparts( GameObject obj )
        {
            // safe this later.
            return _groups[obj].ToArray(); // copy owned data.
        }

        /// <summary>
        /// Removes the specified objects from any existing groups and assigns them to a new group.
        /// </summary>
        /// <param name="objs"></param>
        public void ResetSymmetryGroup( IEnumerable<GameObject> objs )
        {
            // remove every param object if it's already added to a group.
            List<GameObject> objList = new List<GameObject>( objs ); // owns the data.
            foreach( var obj in objList )
            {
                _groups[obj] = objList;
            }
        }

        /// <summary>
        /// Adds a new symmetry counterpart to the collection <paramref name="pivot"/> is a part of.
        /// </summary>
        public void AddCounterpart( GameObject pivot, GameObject newObj )
        {
            _groups[pivot].Add( newObj ); // since the list is the same instance, only need to add once.
            _groups[newObj] = _groups[pivot]; // assign existing reference.
        }
        
        /// <summary>
        /// Removes an object from symmetry.
        /// </summary>
        public void RemoveCounterpart( GameObject existing )
        {
            _groups[existing].Remove( existing ); // Same referenced instance = removes from all keys.
            _groups.Remove(existing);
        }

        public SerializedData GetData( ISaver s )
        {
            throw new System.NotImplementedException();
        }

        public void SetData( ILoader l, SerializedData data )
        {
            throw new System.NotImplementedException();
        }
    }
}