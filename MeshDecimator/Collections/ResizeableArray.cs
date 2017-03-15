using System;

namespace MeshDecimator.Collections
{
    /// <summary>
    /// A resizable array.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    internal sealed class ResizableArray<T>
    {
        #region Consts
        private const int DefaultCapacity = 8;
        #endregion

        #region Fields
        private T[] items = null;
        private int length = 0;

        private static T[] emptyArr = new T[0];
        #endregion

        #region Properties
        /// <summary>
        /// Gets the length of this array.
        /// </summary>
        public int Length
        {
            get { return length; }
        }

        /// <summary>
        /// Gets the internal data buffer for this array.
        /// </summary>
        public T[] Data
        {
            get { return items; }
        }

        /// <summary>
        /// Gets or sets the element value at a specific index.
        /// </summary>
        /// <param name="index">The element index.</param>
        /// <returns>The element value.</returns>
        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new resizable array.
        /// </summary>
        public ResizableArray()
            : this(DefaultCapacity)
        {

        }

        /// <summary>
        /// Creates a new resizable array.
        /// </summary>
        /// <param name="capacity">The initial array capacity.</param>
        public ResizableArray(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");

            if (capacity > 0)
                items = new T[capacity];
            else
                items = emptyArr;

            length = 0;
        }
        #endregion

        #region Private Methods
        private void IncreaseCapacity(int capacity)
        {
            T[] newItems = new T[capacity];
            Array.Copy(items, 0, newItems, 0, System.Math.Min(length, capacity));
            items = newItems;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Clears this array.
        /// </summary>
        public void Clear()
        {
            Array.Clear(items, 0, length);
            length = 0;
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        /// <param name="capacity">The new capacity.</param>
        public void Resize(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");

            if (capacity > items.Length)
            {
                IncreaseCapacity(capacity);
            }
            else if (capacity < length)
            {
                //Array.Clear(items, capacity, length - capacity);
            }
            length = capacity;
        }

        /// <summary>
        /// Trims any excess memory for this array.
        /// </summary>
        public void TrimExcess()
        {
            if (items.Length == length) // Nothing to do
                return;

            T[] newItems = new T[length];
            Array.Copy(items, 0, newItems, 0, length);
            items = newItems;
        }

        /// <summary>
        /// Adds a new item to the end of this array.
        /// </summary>
        /// <param name="item">The new item.</param>
        public void Add(T item)
        {
            if (length >= items.Length)
            {
                IncreaseCapacity(items.Length << 1);
            }

            items[length++] = item;
        }
        #endregion
    }
}