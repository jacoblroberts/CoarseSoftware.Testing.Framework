namespace CoarseSoftware.Testing.Framework.Core
{
    internal class Helpers
    {
        public static TestRunnerConfiguration GetTestRunnerConfiguration()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("System") || assembly.FullName.Contains("Microsoft") || assembly.FullName.Contains("CoarseSoftware.Testing.Framework.Core"))
                {
                    continue;
                }
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName.Contains("System") || type.FullName.Contains("Microsoft") || assembly.FullName.Contains("CoarseSoftware.Testing.Framework.Core"))
                    {
                        continue;
                    }

                    if (type.BaseType is not null && type.BaseType.Equals(typeof(TestRunnerConfiguration)))
                    {
                        // found derived config so we return that.
                        return Activator.CreateInstance(type) as TestRunnerConfiguration;
                    }
                }
            }
            // returning default config
            return new TestRunnerConfiguration();
        }

        public static void CompareExpectedToActual(object expected, object actual, IEnumerable<string> ignoredProperties, IEnumerable<Type> explicitComparerTypes, Type basicComparerType)
        {
            var explicitTestExpectationComparer = explicitComparerTypes.Where(i => i.GetGenericArguments()[0] == actual.GetType()).SingleOrDefault();
            if (explicitTestExpectationComparer != null)
            {
                //need to use reflection to invoke explicitTestExpectationComparer Compare method
                //  passing in expected (expectedRequestData) and actual (requestData)
                var explicitTestInstance = Activator.CreateInstance(explicitTestExpectationComparer);
                var explicitCompareMethod = explicitTestExpectationComparer.GetMethod("Compare");
                explicitCompareMethod.Invoke(explicitTestInstance,
                    new[] { expected, actual, ignoredProperties });
            }
            else
            {
                // DESIGN NOTE:  if this works as is, then we can test both the same.
                var genericTestInstance = Activator.CreateInstance(basicComparerType);
                var genericCompareMethod = basicComparerType.GetMethod("Compare");
                genericCompareMethod.Invoke(genericTestInstance,
                    new[] { expected, actual, ignoredProperties });
            }
        }
    }
}
