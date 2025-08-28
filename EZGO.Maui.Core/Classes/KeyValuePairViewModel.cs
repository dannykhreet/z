using Newtonsoft.Json.Schema;
using Syncfusion.Maui.GridCommon.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Classes
{
    /// <summary>
    /// Notify property changed capable model that mimics <see cref="KeyValuePair"/>
    /// </summary>
    /// <typeparam name="TKey">Type of key</typeparam>
    /// <typeparam name="TValue">Type of value</typeparam>
    public class KeyValuePairViewModel<TKey, TValue> : NotifyPropertyChanged, IEquatable<KeyValuePairViewModel<TKey, TValue>>
    {
        #region Public Properties

        /// <summary>
        /// The key of this item
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// Display text for the key
        /// </summary>
        public string KeyDisplay { get; set; }

        /// <summary>
        /// The value of this item
        /// </summary>
        public TValue Value { get; set; }

        /// <summary>
        /// Display text for the value
        /// </summary>
        public string ValueDisplay { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance with default values
        /// </summary>
        public KeyValuePairViewModel()
        { }

        /// <summary>
        /// Creates a new instance from a given <see cref="KeyValuePair"/>
        /// </summary>
        /// <param name="pair">The pair to copy the values from</param>
        public KeyValuePairViewModel(KeyValuePair<TKey, TValue> pair)
        {
            Key = pair.Key;
            Value = pair.Value;
        }

        /// <summary>
        /// Creates a new instance with given values
        /// </summary>
        /// <param name="key">Value for the key</param>
        /// <param name="value">Value for the value</param>
        public KeyValuePairViewModel(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Creates a new instance with given values
        /// </summary>
        /// <param name="key">Value for the key</param>
        /// <param name="value">Value for the value</param>
        /// <param name="displayKey">Display text for the key</param>
        /// <param name="displayValue">Display text for the value</param>
        public KeyValuePairViewModel(TKey key, TValue value, string displayKey, string displayValue)
        {
            Key = key;
            KeyDisplay = displayKey;
            Value = value;
            ValueDisplay = displayValue;
        }

        #endregion

        #region IEquatable

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="other"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public bool Equals(KeyValuePairViewModel<TKey, TValue> other)
        {
            return Key.Equals(other.Key);
        }

        #endregion

    }
}
