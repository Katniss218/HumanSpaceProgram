using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus
{
    /// <summary>
    /// A generic class implementing an object pool pattern, for Unity objects.
    /// </summary>
    /// <remarks>
    /// This class is NOT thread-safe, and intended for use with GameObjects only.
    /// </remarks>
    /// <typeparam name="TItem">The type of the pooled component.</typeparam>
    /// <typeparam name="TItemData">The type of data to apply when initializing the pooled component.</typeparam>
    public class ObjectPool<TItem, TItemData> where TItem : Component
    {
        /// <summary>
        /// Optional, used to create a new item in the pool.
        /// </summary>
        /// <remarks>
        /// If left unset, the pool will create an empty gameobject and add a <see cref="TItem"/> component to it.
        /// </remarks>
        private readonly Func<TItem> _itemFactory;

        /// <summary>
        /// Used to set the data for the item when it is created or reused.
        /// </summary>
        private readonly Action<TItem, TItemData> _initializeItem; // allows relaxing the assumption that the pool item must implement this method.
        /// <summary>
        /// Optional, checks whether an item can currently be released back into the pool. <br/>
        /// Used for auto-releasing old items.
        /// </summary>
        private readonly Func<TItem, bool> _isExpired;

        /// <summary>
        /// Optional, invoked before an item is released back into the pool, to dispose of it.
        /// </summary>
        private readonly Action<TItem> _disposeItem;

        /// <summary>
        /// The number of items that are currently allocated to the pool.
        /// </summary>
        public int Count => _allItems.Count;

        private readonly Stack<TItem> _available = new();
        private readonly HashSet<TItem> _allItems = new();

        private Transform _itemContainer;

        /// <param name="initialize">The delegate to use to initialize the pooled items.</param>
        public ObjectPool( Action<TItem, TItemData> initialize )
        {
            this._initializeItem = initialize;
        }

        /// <param name="initialize">The delegate to use to initialize the pooled items.</param>
        /// <param name="isExpired">The delegate to use when checking if the pooled item can be automatically released back into the pool.</param>
        public ObjectPool( Action<TItem, TItemData> initialize, Func<TItem, bool> isExpired )
        {
            this._isExpired = isExpired;
            this._initializeItem = initialize;
        }

        /// <param name="createItem">The delegate to use as a factory for the pooled items.</param>
        /// <param name="initialize">The delegate to use to initialize the pooled items.</param>
        /// <param name="isExpired">The delegate to use when checking if the pooled item can be automatically released back into the pool.</param>
        public ObjectPool( Func<TItem> createItem, Action<TItem, TItemData> initialize, Func<TItem, bool> isExpired )
        {
            this._itemFactory = createItem;
            this._isExpired = isExpired;
            this._initializeItem = initialize;
        }

        /// <param name="createItem">The delegate to use as a factory for the pooled items.</param>
        /// <param name="initialize">The delegate to use to initialize the pooled items.</param>
        /// <param name="dispose">The delegate to use to dispose of the pooled items when releasing them.</param>
        /// <param name="isExpired">The delegate to use when checking if the pooled item can be automatically released back into the pool.</param>
        public ObjectPool( Func<TItem> createItem, Action<TItem, TItemData> initialize, Action<TItem> dispose, Func<TItem, bool> isExpired )
        {
            this._itemFactory = createItem;
            this._isExpired = isExpired;
            this._initializeItem = initialize;
            this._disposeItem = dispose;
        }

        /// <summary>
        /// Preallocates the specific number of items to the pool.
        /// </summary>
        public void Preallocate( int count )
        {
            if( count <= 0 )
                throw new ArgumentOutOfRangeException( nameof( count ), "Count must be greater than 0." );

            for( int i = 0; i < count; i++ )
            {
                TItem item = CreateNewItem();
                item.gameObject.SetActive( false );
                _available.Push( item );
            }
        }

        /// <summary>
        /// Retrieves an item, initialized with the specified data, from the pool.
        /// </summary>
        public TItem Get( TItemData data )
        {
            // Fill the stack for the next time using items that have finished being used.
            // `_available` is guaranteed to be empty here.
            if( _available.Count == 0 && _isExpired != null )
            {
                ReleaseExpired_Fast();
            }

            TItem item = _available.Count > 0
                ? _available.Pop()
                : CreateNewItem();

            item.gameObject.SetActive( true );
            _initializeItem.Invoke( item, data );
            return item;
        }

        /// <summary>
        /// Releases an item back into the pool.
        /// </summary>
        public void Release( TItem item )
        {
            if( !_allItems.Contains( item ) )
                throw new InvalidOperationException( "The specified item does not belong to this pool." );

            _disposeItem?.Invoke( item );
            item.gameObject.SetActive( false );
            _available.Push( item );
        }

        /// <summary>
        /// Releases all expired items back into the pool.
        /// </summary>
        public void ReleaseExpired()
        {
            if( _isExpired == null )
                throw new InvalidOperationException( "This object pool is not recyclable. Make sure to use a constructor that supports item recycling." );

            foreach( var item in _allItems )
            {
                if( _isExpired.Invoke( item ) && !_available.Contains( item ) )
                {
                    _disposeItem?.Invoke( item );
                    item.gameObject.SetActive( false );
                    _available.Push( item );
                }
            }
        }

        /// <summary>
        /// Destroys all pooled items and clears the pool. <br/>
        /// Also destroys the container object used to hold the pooled items.
        /// </summary>
        public void Clear()
        {
            foreach( var item in _allItems )
            {
                if( item != null && item.gameObject != null )
                {
                    _disposeItem?.Invoke( item );
                    UnityEngine.Object.Destroy( item.gameObject );
                }
            }

            _available.Clear();
            _allItems.Clear();

            if( _itemContainer != null )
            {
                UnityEngine.Object.Destroy( _itemContainer.gameObject );
            }
        }

        private TItem CreateNewItem()
        {
            TItem item = (_itemFactory == null)
                ? DefaultFactory()
                : _itemFactory.Invoke();

            if( _itemContainer == null )
            {
                _itemContainer = new GameObject( "Object Pool Parent" ).transform;
            }

            item.gameObject.transform.SetParent( _itemContainer, false );
            _allItems.Add( item );

            return item;
        }

        private TItem DefaultFactory()
        {
            GameObject gameObject = new GameObject( $"Object Pool Item - {typeof( TItem ).Name}" );
            return gameObject.AddComponent<TItem>();
        }

        private void ReleaseExpired_Fast()
        {
            // Assumes that '_available' is empty.
            foreach( var item in _allItems )
            {
                if( _isExpired.Invoke( item ) )
                {
                    _disposeItem?.Invoke( item );
                    item.gameObject.SetActive( false );
                    _available.Push( item );
                }
            }
        }
    }
}