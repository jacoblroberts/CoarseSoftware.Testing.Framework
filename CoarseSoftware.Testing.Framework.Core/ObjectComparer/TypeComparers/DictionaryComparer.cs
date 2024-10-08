﻿using System;
using System.Collections;
using System.Globalization;
using CoarseSoftware.Testing.Framework.Core.IgnoreOrderTypes;

namespace CoarseSoftware.Testing.Framework.Core.TypeComparers
{
    /// <summary>
    /// Logic to compare two dictionaries
    /// </summary>
    public class DictionaryComparer : BaseTypeComparer
    {
        /// <summary>
        /// Constructor that takes a root comparer
        /// </summary>
        /// <param name="rootComparer"></param>
        public DictionaryComparer(RootComparer rootComparer) : base(rootComparer)
        {
        }

        /// <summary>
        /// Returns true if both types are dictionaries
        /// </summary>
        /// <param name="type1">The type of the first object</param>
        /// <param name="type2">The type of the second object</param>
        /// <returns></returns>
        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return ((TypeHelper.IsIDictionary(type1) || type1 == null) &&
                    (TypeHelper.IsIDictionary(type2) || type2 == null) &&
                    !(type1 == null && type2 == null));
        }

        /// <summary>
        /// Compare two dictionaries
        /// </summary>
        public override void CompareType(CompareParms parms)
        {
            try
            {
                parms.Result.AddParent(parms.Object1);
                parms.Result.AddParent(parms.Object2);

                //Objects must be the same length
                bool countsDifferent = DictionaryCountsDifferent(parms);

                if (countsDifferent && parms.Result.ExceededDifferences)
                    return;

                bool shouldCompareByKeys = ShouldCompareByKeys(parms);

                if (shouldCompareByKeys)
                {
                    CompareByKeys(parms);
                }
                else
                {
                    if (parms.Config.IgnoreCollectionOrder)
                    {
                        IgnoreOrderLogic logic = new IgnoreOrderLogic(RootComparer);
                        logic.CompareEnumeratorIgnoreOrder(parms, countsDifferent);
                    }
                    else
                    {
                        CompareByEnumerator(parms);
                    }
                }
            }
            finally
            {
                parms.Result.RemoveParent(parms.Object1);
                parms.Result.RemoveParent(parms.Object2);
            }
        }

        /// <summary>
        /// This is to handle funky situation of having a complex object as a key
        /// (In this case a dictionary as a key)
        /// https://github.com/GregFinzer/Compare-Net-Objects/issues/222
        /// </summary>
        /// <param name="parms"></param>
        /// <returns></returns>
        private static bool ShouldCompareByKeys(CompareParms parms)
        {
            bool shouldCompareByKeys = true;

            if (parms.Object1 != null)
            {
                var dict1 = ((IDictionary) parms.Object1);
                
                if (dict1.Keys.Count > 0)
                {
                    var enumerator1 = ((IDictionary) parms.Object1).GetEnumerator();
                    enumerator1.MoveNext();
                    shouldCompareByKeys =
                        enumerator1.Key != null && TypeHelper.IsSimpleType(enumerator1.Key.GetType());
                }
            }

            return shouldCompareByKeys;
        }


        private void CompareByKeys(CompareParms parms)
        {
            var dict1 = ((IDictionary)parms.Object1);
            var dict2 = ((IDictionary)parms.Object2);

            if (dict1 != null)
            {
                foreach (var key in dict1.Keys)
                {
                    string currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, "[" +key.ToString()+ "].Value");

                    CompareParms childParms = new CompareParms
                    {
                        Result = parms.Result,
                        Config = parms.Config,
                        ParentObject1 = parms.Object1,
                        ParentObject2 = parms.Object2,
                        Object1 = dict1[key],
                        Object2 = (dict2 != null) && dict2.Contains(key) ? dict2[key] : null,
                        BreadCrumb = currentBreadCrumb
                    };

                    RootComparer.Compare(childParms);

                    if (parms.Result.ExceededDifferences)
                        return;
                }
            }

            if (dict2 != null)
            {
                foreach (var key in dict2.Keys)
                {
                    if (dict1 != null && dict1.Contains(key))
                        continue;

                    var currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb,
                        "[" + key.ToString() + "].Value");

                    var childParms = new CompareParms
                    {
                        Result = parms.Result,
                        Config = parms.Config,
                        ParentObject1 = parms.Object1,
                        ParentObject2 = parms.Object2,
                        Object1 = null,
                        Object2 = dict2[key],
                        BreadCrumb = currentBreadCrumb
                    };

                    RootComparer.Compare(childParms);

                    if (parms.Result.ExceededDifferences)
                        return;
                    
                }
            }
        }

        private void CompareByEnumerator(CompareParms parms)
        {
            var enumerator1 = ((IDictionary)parms.Object1).GetEnumerator();
            var enumerator2 = ((IDictionary)parms.Object2).GetEnumerator();

            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                string currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, "Key");

                CompareParms childParms = new CompareParms
                {
                    Result = parms.Result,
                    Config = parms.Config,
                    ParentObject1 = parms.Object1,
                    ParentObject2 = parms.Object2,
                    Object1 = enumerator1.Key,
                    Object2 = enumerator2.Key,
                    BreadCrumb = currentBreadCrumb
                };

                RootComparer.Compare(childParms);

                if (parms.Result.ExceededDifferences)
                    return;

                currentBreadCrumb = AddBreadCrumb(parms.Config, parms.BreadCrumb, "Value");

                childParms = new CompareParms
                {
                    Result = parms.Result,
                    Config = parms.Config,
                    ParentObject1 = parms.Object1,
                    ParentObject2 = parms.Object2,
                    Object1 = enumerator1.Value,
                    Object2 = enumerator2.Value,
                    BreadCrumb = currentBreadCrumb
                };

                RootComparer.Compare(childParms);

                if (parms.Result.ExceededDifferences)
                    return;
            }
        }

        private bool DictionaryCountsDifferent(CompareParms parms)
        {
            IDictionary iDict1 = parms.Object1 as IDictionary;
            IDictionary iDict2 = parms.Object2 as IDictionary;

            int iDict1Count = (iDict1 == null) ? 0 : iDict1.Count;
            int iDict2Count = (iDict2 == null) ? 0 : iDict2.Count;

            if (iDict1Count == iDict2Count)
                return false;

            Difference difference = new Difference
            {
                ParentObject1 = parms.ParentObject1,
                ParentObject2 = parms.ParentObject2,
                PropertyName = parms.BreadCrumb,
                Object1Value = iDict1Count.ToString(CultureInfo.InvariantCulture),
                Object2Value = iDict2Count.ToString(CultureInfo.InvariantCulture),
                ChildPropertyName = "Count",
                Object1 = iDict1,
                Object2 = iDict2
            };

            AddDifference(parms.Result, difference);

            return true;
        }
    }
}
