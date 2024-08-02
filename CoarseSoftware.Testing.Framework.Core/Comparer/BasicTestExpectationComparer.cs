namespace CoarseSoftware.Testing.Framework.Core.Comparer
{
    using CoarseSoftware.Testing.Framework.Core;
    using CoarseSoftware.Testing.Framework.Core.TypeComparers;
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Data;

    public class BasicTestExpectationComparer
    {
        public void Compare(object expected, object actual, IEnumerable<string> ignoredPropertyNames)
        {
            var comparerConfig = new ComparisonConfig();
            comparerConfig.MembersToIgnore.AddRange(ignoredPropertyNames ?? Array.Empty<string>());
            comparerConfig.CustomComparers.Add(new GenericCollectionTypeComparer());
            comparerConfig.MaxDifferences = InternalTestRunnerConfiguration.MaxDifferencesBeforeFailing;
            comparerConfig.ShowBreadcrumb = false;
            comparerConfig.CompareBackingFields = false;
            comparerConfig.IgnoreDateTimeOffsetTimezones = true;
            comparerConfig.IgnoreDateTimeComparison = InternalTestRunnerConfiguration.IgnoreDateTimeComparison;

            comparerConfig.CompareChildren = true;
            comparerConfig.CompareProperties = true;
            comparerConfig.Caching = true;
            
            var comparer = new CompareLogic(comparerConfig);
            var result = comparer.Compare(expected, actual);
            if (!result.AreEqual)
            {
                var actualDump = $@"new {actual.GetType().FullName.Replace("+", ".").Replace(" ", "")}
{this.printProperties(actual, 0)}";
                if (
                    (InternalTestRunnerConfiguration.BreakOnActualDumpBreakPoint == TestRunnerConfiguration.ActualDumpBreakType.Debugging
                        && System.Diagnostics.Debugger.IsAttached)
                    || InternalTestRunnerConfiguration.BreakOnActualDumpBreakPoint == TestRunnerConfiguration.ActualDumpBreakType.Always)
                {
                    System.Diagnostics.Debugger.Break();
                }

                Assert.Fail($@"Failed when comparing expected to actual. 
Expected Type: {expected.GetType().FullName}
Actual Type: {actual.GetType().FullName}

{result.DifferencesString}

-==Actual Dump==-

{actualDump}

-===============-
");
            }
        }

        private string printProperties(object obj, int indent)
        {
            if (obj == null)
                return string.Empty;

            string indentString = new string(' ', indent);

            Type objType = obj.GetType();

            var objectResponse = new List<string>();
            objectResponse.Add($"{indentString}{{");

            PropertyInfo[] properties = objType.GetProperties();
            var propertyEntries = new List<string>();
            foreach (PropertyInfo property in properties)
            {
                if (property.GetSetMethod() == null)
                {
                    continue;
                }

                var internalIndent = indent + 4;
                string internalIndentString = new string(' ', internalIndent);
                object propValue;

                if (property.PropertyType.IsArray)
                    propValue = (Array)property.GetValue(obj);
                else
                    propValue = property.GetValue(obj, null);

                if (propValue == null)
                {
                    //$"{indentString}{property.Name} = null"
                    continue;
                }

                if (!typeof(string).Equals(property.PropertyType) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    var ienum1 = propValue as IEnumerable;

                    List<object> list1 = new List<object>();

                    foreach (var item in ienum1)
                        list1.Add(item);

                    propValue = list1;
                }

                var elems = propValue as IList;

                if (elems != null)
                {
                    var sb = new List<string>();
                    Type itemType = property.PropertyType.GetGenericArguments()[0];
                    var listTypeName = string.Empty;
                    if (itemType.Equals(typeof(string)))
                    {
                        listTypeName = "string";
                    }
                    else if (itemType.Equals(typeof(bool))) 
                    {
                        listTypeName = "bool";
                    }
                    else if (itemType.Equals(typeof(decimal)))
                    {
                        listTypeName = "decimal";
                    }
                    else if (itemType.Equals(typeof(float)))
                    {
                        listTypeName = "float";
                    }
                    else if (itemType.Equals(typeof(int)))
                    {
                        listTypeName = "int";
                    }
                    else if (itemType.Equals(typeof(double)))
                    {
                        listTypeName = "double";
                    }
                    else
                    {
                        listTypeName = itemType.FullName.Replace("+", ".").Replace(" ", "");
                    }
                    sb.Add($"{internalIndentString}{property.Name} = new List<{listTypeName}>");
                    sb.Add($"{internalIndentString}{{");
                    var listIndent = internalIndent + 4;
                    var listIndentString = new string(' ', listIndent);
                    var items = new List<string>();
                    for (int i = 0; i < elems.Count; ++i)
                    {
                        var elem = elems[i];

                        switch (elem)
                        {
                            case string:
                                items.Add($"{listIndentString}\"{elem}\"");
                                break;
                            case int:
                            case long:
                                items.Add($"{listIndentString}{elem}");
                                break;
                            case float:
                                items.Add($"{listIndentString}{elem}f");
                                break;
                            case double:
                                items.Add($"{listIndentString}{elem}d");
                                break;
                            case decimal:
                                items.Add($"{listIndentString}{elem}m");
                                break;
                            case bool:
                                propertyEntries.Add($"{listIndentString}{elem.ToString().ToLower()}");
                                break;
                            case DateTime:
                                items.Add($"{listIndentString}DateTime.Parse(\"{elem}\")");
                                break;
                            case DateTimeOffset dto:
                                items.Add($"{listIndentString}DateTimeOffset.Parse(\"{dto.ToUniversalTime().ToString("o")}\")");
                                break;
                            default:
                                {
                                    if (elem.GetType().IsEnum)
                                    {
                                        items.Add($"{listIndentString}{elem.GetType().FullName.Replace("+", ".").Replace(" ", "")}.{elem}");
                                        break;
                                    }
                                    //objectResponse.AppendLine($"{indentString}new {objType.FullName.Replace("+", ".").Replace(" ", "")}");
                                    items.Add($@"{listIndentString}new {elem.GetType().FullName.Replace("+", ".").Replace(" ", "")}
{printProperties(elem, listIndent)}");
                                    break;
                                }
                        }
                    }
                    var itemIndex = 1;
                    items = items.Where(i => !string.IsNullOrEmpty(i)).ToList();
                    foreach (var item in items)
                    {
                        var isLast = itemIndex == items.Count;
                        var ending = isLast ? string.Empty : ",";
                        sb.Add($"{item}{ending}");
                        itemIndex++;
                    }
                    sb.Add($"{internalIndentString}}}");

                    propertyEntries.Add(string.Join("\n", sb));
                }
                else
                {
                    switch (propValue)
                    {
                        case string:
                            propertyEntries.Add($"{internalIndentString}{property.Name} = \"{propValue}\"");
                            break;
                        case int:
                        case long:
                            propertyEntries.Add($"{internalIndentString}{property.Name} = {propValue}");
                            break;
                        case float:
                            propertyEntries.Add($"{internalIndentString}{property.Name} = {propValue}f");
                            break;
                        case double:
                            propertyEntries.Add($"{internalIndentString}{property.Name} = {propValue}d");
                            break;
                        case decimal:
                            propertyEntries.Add($"{internalIndentString}{property.Name} = {propValue}m");
                            break;
                        case bool:
                            propertyEntries.Add($"{internalIndentString}{property.Name} = {propValue.ToString().ToLower()}");
                            break;
                        case DateTime:
                            propertyEntries.Add($"{internalIndentString}{property.Name} = DateTime.Parse(\"{propValue}\")");
                            break;
                        case DateTimeOffset dto:
                            //  think about this.  would it be more of an annoyance?  can it be smarter?  ie; pass in the failed diffs and if this is a failed diff, then add the warning.  warning is a /* potentially generated */ note after property.Name =
                            //var warning = dto <= DateTimeOffset.UtcNow.AddMinutes(1) && dto >= DateTimeOffset.UtcNow.AddMinutes(-5);
                            propertyEntries.Add($"{internalIndentString}{property.Name} = DateTimeOffset.Parse(\"{dto.ToUniversalTime().ToString("o")}\")");
                            break;
                        default:
                            {
                                if (propValue.GetType().IsEnum)
                                {
                                    propertyEntries.Add($"{internalIndentString}{property.Name} = {propValue.GetType().FullName.Replace("+", ".").Replace(" ", "")}.{propValue}");
                                    break;
                                }
                                //objectResponse.AppendLine($"{indentString}new {objType.FullName.Replace("+", ".").Replace(" ", "")}");
                                propertyEntries.Add($@"{internalIndentString}{property.Name} = new {propValue.GetType().FullName.Replace("+", ".").Replace(" ", "")}
{printProperties(propValue, internalIndent)}");
                                break;
                            }
                    }
                }

            }

            var propertyEntryIndex = 1;
            foreach(var propertyEntry  in propertyEntries)
            {
                var isLastPropertyEntry = propertyEntryIndex == propertyEntries.Count;
                var lineEnding = isLastPropertyEntry ? string.Empty : ",";
                objectResponse.Add($"{propertyEntry}{lineEnding}");
                propertyEntryIndex++;
            }
            //objectResponse.AppendLine($"{indentString}{string.Join(",", propertyEntries)}");
            objectResponse.Add($"{indentString}}}");

            var response = string.Join("\n", objectResponse);
            return response;
        }
    }

    public class GenericCollectionTypeComparer : BaseTypeComparer
    {
        private readonly ListComparer _compareIList;

        public GenericCollectionTypeComparer(RootComparer rootComparer) : base(rootComparer)
        {
            _compareIList = new ListComparer(rootComparer);
        }

        public GenericCollectionTypeComparer() : this(RootComparerFactory.GetRootComparer())
        {
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            if (typeof(string).Equals(type1) &&  typeof(string).Equals(type2))
                return false;

            if (typeof(IEnumerable).IsAssignableFrom(type1) && typeof(IEnumerable).IsAssignableFrom(type2))
                return true;

            return false;
        }

        public override void CompareType(CompareParms parms)
        {
            var ienum1 = parms.Object1 as IEnumerable;
            var ienum2 = parms.Object2 as IEnumerable;

            var oldObject1 = parms.Object1;
            var oldObject2 = parms.Object2;
            try
            {
                parms.Result.AddParent(parms.Object1);
                parms.Result.AddParent(parms.Object2);

                List<object> list1 = new List<object>();
                List<object> list2 = new List<object>();

                // possibly compare each item as we iterate?
                foreach (var item in ienum1)
                    list1.Add(item);

                foreach (var item in ienum2)
                    list2.Add(item);

                parms.Object1 = list1;
                parms.Object2 = list2;

                _compareIList.CompareType(parms);
            }
            finally
            {
                parms.Result.RemoveParent(oldObject1);
                parms.Result.RemoveParent(oldObject2);
            }
        }
    }
}
