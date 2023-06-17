using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Manages the task of serialization.
    /// </summary>
    public class Saver
    {
        // Core ideas:
        /*
        
        Saving is split into 2 main steps:
        
        1. Save object data.
            Loop through every object and save its persistent data.
            When serializing a reference, ask what the ID of that object is by passing the object to the `RegisterOrGet` method.

        2. Save object instances (references).
            Loop through these objects again, and save them, along with their IDs (if referenced by anything).
            Use the object registry to get the IDs of objects that have been assigned to them in step 1.

        Benefit: We know what is referenced before we save the objects (all of which could potentially be referenced).

        */

        /// <summary>
        /// Specifies where to save the data.
        /// </summary>
        public string SaveDirectory { get; set; }

        List<Action<Saver>> _dataActions = new List<Action<Saver>>();
        List<Action<Saver>> _objectActions = new List<Action<Saver>>();

        Dictionary<object, Guid> _objectToGuid = new Dictionary<object, Guid>();

        // ---

        public Saver( string saveDirectory )
        {
            this.SaveDirectory = saveDirectory;
        }

        public Saver( string saveDirectory, Action<Saver>[] dataActions, Action<Saver>[] objectActions )
        {
            this.SaveDirectory = saveDirectory;

            foreach( var action in objectActions )
            {
                this._objectActions.Add( action );
            }
            foreach( var action in dataActions )
            {
                this._dataActions.Add( action );
            }
        }

        // ---

        private void ClearReferenceRegistry()
        {
            _objectToGuid.Clear();
        }

        /// <summary>
        /// Adds a new data action (used to save persistent data about instances of objects). <br />
        /// See also: <see cref="AddObjectAction"/>
        /// </summary>
        public void AddDataAction( Action<Saver> action )
        {
            this._dataActions.Add( action );
        }

        /// <summary>
        /// Adds a new object action (used to persist an object instance, so that the references can be reconstructed using a loader's data action). <br />
        /// See also: <see cref="AddDataAction"/>
        /// </summary>
        public void AddObjectAction( Action<Saver> action )
        {
            this._objectActions.Add( action );
        }

        /// <summary>
        /// Registers the specified object in the registry (if not registered already) and returns its reference ID.
        /// </summary>
        /// <remarks>
        /// Call this to map an object to an ID when saving an object reference.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Guid GetID( object obj )
        {
            if( _objectToGuid.TryGetValue( obj, out Guid id ) )
            {
                return id;
            }

            Guid newID = Guid.NewGuid();
            _objectToGuid.Add( obj, newID );
            return newID;
        }

        // ---

        /// <summary>
        /// Performs a save to the current path, and with the current save actions.
        /// </summary>
        public void Save()
        {
            ClearReferenceRegistry();

            foreach( var action in _objectActions )
            {
                action?.Invoke( this );
            }

            foreach( var action in _dataActions )
            {
                action?.Invoke( this );
            }

            ClearReferenceRegistry();
        }


        // save data (with things that can be referenced)
        // save referencable objects (gameobjects, additional eg dialogues).
        // - save ID if thing is in cache.


        // load referencable objects
        // load data (with references) once referencables are loaded.


        // dialogues are maps of references between dialogue options. load options first, then load dialogues. this separation *can* be done inside a single load action.
    }
}