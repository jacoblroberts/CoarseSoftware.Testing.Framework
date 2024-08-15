namespace CoarseSoftware.Testing.Framework.Core.Proxy
{
    using System.Reflection;
    using static CoarseSoftware.Testing.Framework.Core.TestCasesRunner;

    internal class MockProxy : DispatchProxy
    {
        public MockProxy()
        {
            this.responseQueue = new List<Response>();
            this.OrderedInvocations = new List<Response>();
        }
        public TestStats TestStats { get; set; }
        private Type? genericTestExpectationComparerType { get; set; }
        private IEnumerable<Type> explicitTestExpectationComparerTypes { get; set; }
        private TestRunnerConfiguration configuration {  get; set; }
        /// <summary>
        /// this is used for tracking purposes
        /// </summary>
        public IEnumerable<Response> OrderedInvocations { get; private set; }
        private IEnumerable<Response> responseQueue { get; set; }
        public void Enqueue(Response response)
        {
            this.responseQueue = this.responseQueue.Append(response).ToList();
            this.OrderedInvocations = this.OrderedInvocations.Append(response).ToList();
        }

        private Response dequeue()
        {
            var nextResponse = this.responseQueue.First();
            this.responseQueue = this.responseQueue.Skip(1).ToList();
            return nextResponse;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var nextResponse = this.dequeue();

            // testing order of operation
            TestStats.InvocationCount++;
            // NOTE: The expected type would be nice here but we need to pass the operations.  Future improvement.
            if (TestStats.InvocationCount != nextResponse.ExpectedInvocationOrder)
            {
                throw new Exception($"Order of operation was not as expected.  Expected service {targetMethod.DeclaringType.FullName} but {nextResponse.Item.GetType().FullName} invoked. TestStats.InvocationCount {TestStats.InvocationCount} != ExpectedInvocationOrder {nextResponse.ExpectedInvocationOrder}");
            }

            // we need a way to automatically include or exclude wrapper classes
            //  we can do it by getting the generic arg for teh request, recursively until we find a match.  if no match, use the default
            var firstArgType = args.First().GetType();

            var isWrapped = configuration.RequestWrapper != null
                    && firstArgType.IsGenericType
                    && firstArgType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;

            var requestData = isWrapped
                ? firstArgType.GetProperty(configuration.RequestWrapper.DtoPropertyName).GetValue(args.First())
                : args.First();
            var expectedRequestData = isWrapped
                ? nextResponse.ExpectedRequest.GetType().GetProperty(configuration.RequestWrapper.DtoPropertyName).GetValue(nextResponse.ExpectedRequest)
                : nextResponse.ExpectedRequest;
            /// from here down is ITestExpectationComparer implementation details
            // we need to know which one to activate since they are done differently and are invoked differently

            // testing types match
            if (firstArgType.FullName != nextResponse.ExpectedRequest.GetType().FullName)
            {
                throw new Exception($"Request was not wrapped in a Request<T> object. Comparing {firstArgType.FullName} to {nextResponse.ExpectedRequest.GetType().FullName}");
            }
            if (requestData.GetType().FullName != expectedRequestData.GetType().FullName)
            {
                throw new Exception($"The request type: {requestData.GetType().FullName} does not match the expected request type: {expectedRequestData.GetType().FullName}");
            }

            //Helpers.CompareExpectedToActual(expectedRequestData, requestData, nextResponse.IngoredPropertyNames, explicitTestExpectationComparerTypes, genericTestExpectationComparerType);
            Helpers.CompareExpectedToActual(nextResponse.ExpectedRequest, args.First(), nextResponse.IngoredPropertyNames, explicitTestExpectationComparerTypes, genericTestExpectationComparerType);

            if (nextResponse.Item is not null)
            {
                // must return Task<Repsonse<T>>
                // this is a hack to wrap the response in a task
                var convert_method = typeof(MockProxy).GetMethod("ConvertToTaskHack").MakeGenericMethod(nextResponse.Item.GetType());

                var result = convert_method.Invoke(null, new object[] { nextResponse.Item });

                return result;
            }
            return Task.CompletedTask;
        }

        public static Task<T> ConvertToTaskHack<T>(T value)
        {
            return Task.FromResult<T>((T)value);
        }

        public MockProxy Clone(Type facet, TestStats testStats)
        {
            var proxy = MockProxy.Create(facet, testStats, genericTestExpectationComparerType, explicitTestExpectationComparerTypes, configuration) as MockProxy;

            proxy.responseQueue = this.responseQueue.Select(q => new Response
            {
                ExpectedInvocationOrder = q.ExpectedInvocationOrder,
                ExpectedRequest = q.ExpectedRequest,
                IngoredPropertyNames = q.IngoredPropertyNames,
                Item = q.Item,
                ExpectedMethod = q.ExpectedMethod
            }).ToList();

            proxy.OrderedInvocations = this.OrderedInvocations.Select(q => new Response
            {
                ExpectedInvocationOrder = q.ExpectedInvocationOrder,
                ExpectedRequest = q.ExpectedRequest,
                IngoredPropertyNames = q.IngoredPropertyNames,
                Item = q.Item,
                ExpectedMethod = q.ExpectedMethod
            }).ToList();

            return proxy;
        }

        public static object Create(Type interfaceType, TestStats testStats, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes, TestRunnerConfiguration configuration)
        {
#if NET8_0
            var proxy = DispatchProxy.Create(interfaceType, typeof(MockProxy));
#elif NET6_0
            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(interfaceType, typeof(MockProxy));
            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
#endif
            var proxyTypes = (MockProxy)proxy;
            proxyTypes.TestStats = testStats;
            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
            proxyTypes.configuration = configuration;
            return proxy;
        }

        public class Response
        {
            public int ExpectedInvocationOrder { get; set; }
            public object Item { get; set; }
            public object ExpectedRequest { get; set; }
            public string ExpectedMethod { get; set; }
            public IEnumerable<string> IngoredPropertyNames { get; set; }
        }
    }
}
