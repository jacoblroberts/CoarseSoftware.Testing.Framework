namespace CoarseSoftware.Testing.Framework.Core.Proxy.ClientTest
{
    using System.Reflection;

    internal class MockServiceProxy : DispatchProxy
    {
        private Action<TestStatStore.ClientTestStat.Service> onChildStatComplete { get; set; }
        private Type? genericTestExpectationComparerType { get; set; }
        private IEnumerable<Type> explicitTestExpectationComparerTypes { get; set; }
        private Type serviceType { get; set; }
        private object service {  get; set; }
        private ClientTestCase.Microservice microservice { get; set; }
        private TestRunnerConfiguration configuration { get; set; }
        //private Func<ClientTestCase.ClientTestStats.Service> createActiveChildStat { get; set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var childStat = new TestStatStore.ClientTestStat.Service(); //this.createActiveChildStat();
            if (serviceType.Equals(microservice.FacetType) && targetMethod.Name == microservice.ExpectedMethodName)
            {
                object? requestData = args.First(); // null;
                if (args.Any() && args.First() != null)
                {
                    //var firstArgType = args.First().GetType();

                    //var isRequestWrapped = configuration.RequestWrapper != null
                    //    && firstArgType.IsGenericType
                    //    && firstArgType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;

                    //requestData = isRequestWrapped
                    //     ? firstArgType.GetProperty(configuration.RequestWrapper.DtoPropertyName).GetValue(args.First())
                    //     : args.First();

                    Helpers.CompareExpectedToActual(microservice.ExpectedRequest, requestData, microservice.IngoredRequestPropertyNames, explicitTestExpectationComparerTypes, genericTestExpectationComparerType);

                    var convert_method = typeof(MockServiceProxy).GetMethod("ConvertToTaskHack").MakeGenericMethod(microservice.MockResponse.GetType());

                    var result = convert_method.Invoke(null, new object[] { microservice.MockResponse });

                    childStat.MethodName = microservice.ExpectedMethodName;
                    childStat.TypeName = microservice.FacetType.FullName;
                    childStat.RequestTypeNames = new List<string> { microservice.ExpectedRequest.GetType().GetProperty(configuration.RequestWrapper.DtoPropertyName).GetValue(microservice.ExpectedRequest).GetType().FullName };
                    childStat.ResponseTypeName = microservice.MockResponse.GetType().GetProperty(configuration.ResponseWrapper.DtoPropertyName).GetValue(microservice.MockResponse).GetType().FullName;

                    onChildStatComplete.Invoke(childStat);
                    
                    return result;
                }
            }

            var task = targetMethod.Invoke(service, args);
            var isTask = targetMethod.ReturnType.Equals(typeof(Task)) || (targetMethod.ReturnType.IsGenericType && targetMethod.ReturnType.GetGenericTypeDefinition().Equals(typeof(Task<>)));
            object response = null;
            //firstArgType.GetGenericTypeDefinition() == configuration.RequestWrapper.OpenWrapperType;
            if (isTask)
            { 
                ((Task)task).Wait();

                var resultProperty = task.GetType().GetProperty("Result");
                response = resultProperty.GetValue(task);
            }
            else
            {
                response = task;
            }

            // i think response could be null for Task/void methods

            childStat.MethodName = targetMethod.Name;
            childStat.TypeName = targetMethod.DeclaringType.FullName;
            childStat.RequestTypeNames = args.Select(a => a.GetType().FullName).ToList();
            childStat.ResponseTypeName = response.GetType().FullName;
            onChildStatComplete.Invoke(childStat);

            if (isTask)
            {
                var convert_method = typeof(IntegratedServiceProxy).GetMethod("ConvertToTaskHack").MakeGenericMethod(response.GetType());
                var result = convert_method.Invoke(null, new object[] { response });
                return result;
            }
            return response;
        }

        public static Task<T> ConvertToTaskHack<T>(T value)
        {
            return Task.FromResult<T>((T)value);
        }

        public static object Create(object service, Type serviceType, Action<TestStatStore.ClientTestStat.Service> onChildStatComplete, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes, ClientTestCase.Microservice microservice, TestRunnerConfiguration configuration)
        {
#if NET8_0
            var proxy = DispatchProxy.Create(serviceType, typeof(MockServiceProxy));
#elif NET6_0
            var dispatchProxyCreateMethod = typeof(DispatchProxy).GetMethod("Create").MakeGenericMethod(serviceType, typeof(MockServiceProxy));
            var proxy = dispatchProxyCreateMethod.Invoke(null, null);
#endif
            var proxyTypes = (MockServiceProxy)proxy;
            proxyTypes.serviceType = serviceType;
            proxyTypes.service = service;
            proxyTypes.onChildStatComplete = onChildStatComplete;
            proxyTypes.genericTestExpectationComparerType = genericTestExpectationComparerType;
            proxyTypes.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
            proxyTypes.microservice = microservice;
            proxyTypes.configuration = configuration;
            return proxy;
        }
    }
}
