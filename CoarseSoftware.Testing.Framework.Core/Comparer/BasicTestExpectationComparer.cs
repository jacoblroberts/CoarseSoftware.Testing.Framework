namespace CoarseSoftware.Testing.Framework.Core.Comparer
{
    using KellermanSoftware.CompareNetObjects;
    using KellermanSoftware.CompareNetObjects.TypeComparers;
    using NUnit.Framework;
    using System.Collections;

    public class BasicTestExpectationComparer
    {
        public void Compare(object expected, object actual, IEnumerable<string> ignoredPropertyNames)
        {
            var comparerConfig = new ComparisonConfig();
            comparerConfig.MembersToIgnore.AddRange(ignoredPropertyNames ?? Array.Empty<string>());
            comparerConfig.CustomComparers.Add(new GenericCollectionTypeComparer());

            var comparer = new CompareLogic(comparerConfig);
            var result = comparer.Compare(expected, actual);
            if (!result.AreEqual)
            {
                Assert.Fail($@"Failed when comparing expected to actual. 
Type: {expected.GetType().FullName}
{result.DifferencesString}");
            }
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
