﻿﻿#pragma warning disable CS1570 // XML comment has badly formed XML
using System;
using CoarseSoftware.Testing.Framework.Core.TypeComparers;

namespace CoarseSoftware.Testing.Framework.Core
{
    /// <summary>
    /// Compare Parameters
    /// </summary>
    public class CompareParms
    {
        /// <summary>
        /// The configuration settings
        /// </summary>
        public ComparisonConfig Config { get; set; }

        /// <summary>
        /// The type of the first object
        /// </summary>
        public Type Object1Type { get; set; }

        /// <summary>
        /// The type of the second object
        /// </summary>
        public Type Object2Type { get; set; }

        /// <summary>
        /// The declared type of the first object in its parent. e.g. IList<T>
        /// </summary>
        public Type Object1DeclaredType { get; set; }

        /// <summary>
        /// The declared type of the second object in its parent. e.g. IList<T>
        /// </summary>
        public Type Object2DeclaredType { get; set; }

        /// <summary>
        /// Details about the comparison
        /// </summary>
        public ComparisonResult Result { get; set; }

        /// <summary>
        /// A reference to the parent object1
        /// </summary>
        public object ParentObject1 { get; set; }

        /// <summary>
        /// A reference to the parent object2
        /// </summary>
        public object ParentObject2 { get; set; }

        /// <summary>
        /// The first object to be compared
        /// </summary>
        public object Object1 { get; set; }
        
        /// <summary>
        /// The second object to be compared
        /// </summary>
        public object Object2 { get; set; }

        /// <summary>
        /// The breadcrumb in the tree
        /// </summary>
        public string BreadCrumb { get; set; }

        /// <summary>
        /// Custom comparer used to assert <para>Object1</para>
        /// </summary>
        public BaseTypeComparer CustomPropertyComparer { get; set; }
    }
}
#pragma warning restore CS1570 // XML comment has badly formed XML