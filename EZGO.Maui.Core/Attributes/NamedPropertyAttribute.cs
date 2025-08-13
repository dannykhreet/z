using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Attributes
{
    /// <summary>
    /// The attribute that associates a property with a name
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NamedPropertyAttribute : Attribute
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPropertyAttribute"/> 
        /// </summary>
        /// <param name="name">The name of this property</param>
        public NamedPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
