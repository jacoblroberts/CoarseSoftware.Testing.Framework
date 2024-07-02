namespace CoarseSoftware.Testing.Framework.Core
{
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;
    using CoarseSoftware.Testing.Framework.Core.Proxy;
    using System.Linq;
    using System.Text.Json;
    using CoarseSoftware.Testing.Framework.Core.Comparer;
    using NUnit.Framework;

    [TestFixture]
    [Ignore("Abstract")]
    public abstract class TestCasesRunner
    {
        [TestCaseSource(nameof(TestCaseData))]
        public async Task<bool> RunTests(TestCaseDataRequest testCase, TestStatStore testStatStore)
        {
            return await RunTestCase(testCase, testStatStore);
        }

        public class TestCaseDataRequest
        {
            public string TestName { get; set; }

            public TestCase TestCase { get; set; }
            public IServiceCollection ServiceCollection { get; set; }
            public string Category { get; set; }
            public object? ExpectedResponse { get; set; }
            public IEnumerable<string> IngoredExpectedResponsePropertyNames { get; set; }
        }

        private class CreateTestCaseResponse
        {
            public Guid TestId { get; set; }

            public IEnumerable<PreregisteredDependency> PreregisteredDependencies { get; set; }
            public int DependencyCount { get; set; }

            /// <summary>
            /// This is only nullable to allow for recursion.  Any response outside of a recursive loop is expected to have a value.
            /// </summary>
            public object? ExpectedResponse { get; set; }

            public IEnumerable<string> IngoredExpectedResponsePropertyNames { get; set; }

            public IEnumerable<string> ForkReasons { get; set; }
        }

        private class PreregisteredDependency
        {
            public Type FacetType { get; set; }
            public MockProxy Proxy { get; set; }
        }

        private static IEnumerable<CreateTestCaseResponse> createTestCase(IEnumerable<TestCase.Operation> operations, IEnumerable<PreregisteredDependency> preregisteredDependencies, int dependencyCount, TestCase managerTestCase, IEnumerable<string> forkReasons, TestStats testStats, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes)
        {

            var expectedResponseFound = false;
            foreach (var operation in operations)
            {
                if (expectedResponseFound)
                {
                    throw new Exception($"No operation should come after a {typeof(TestCase.ExpectedResponseOperation).Name}.");
                }
                switch (operation)
                {
                    case TestCase.ServiceOperation op:
                        {
                            dependencyCount++;

                            var preregisteredDependency = preregisteredDependencies.Where(s => s.FacetType.FullName == op.FacetType.FullName).FirstOrDefault();
                            if (preregisteredDependency == null)
                            {
                                var mockService = MockProxy.Create(op.FacetType, testStats, genericTestExpectationComparerType, explicitTestExpectationComparerTypes) as MockProxy;
                                preregisteredDependency = new PreregisteredDependency
                                {
                                    FacetType = op.FacetType,
                                    Proxy = mockService
                                };
                                preregisteredDependencies = preregisteredDependencies.Append(preregisteredDependency).ToList();
                            }
                            switch (op.Response)
                            {
                                case TestCase.Response.ForkingResponse forkingResponse:
                                    {
                                        //need to create copies first because we run the left side first, which could change the dependencies
                                        var copyOfTestStats = testStats.Clone();
                                        var copyOfPreregisteredDependencies = preregisteredDependencies.Select(p => new PreregisteredDependency
                                        {
                                            FacetType = p.FacetType,
                                            Proxy = p.Proxy.Clone(p.FacetType, copyOfTestStats)
                                        }).ToList();
                                        var copyOfForkReasons = forkReasons.ToList();

                                        var copyOfDependencyCount = dependencyCount;

                                        preregisteredDependency.Proxy.Enqueue(new MockProxy.Response
                                        {
                                            ExpectedInvocationOrder = dependencyCount,
                                            Item = forkingResponse.LeftFork.MockResponse,
                                            ExpectedRequest = op.ExpectedRequest,
                                            IngoredPropertyNames = op.IngoredExpectedRequestPropertyNames
                                        });

                                        // @TODO - do something with reason like add it to the json output.
                                        //forkingResponse.LeftFork.Reason
                                        forkReasons = forkReasons.Append(forkingResponse.LeftFork.Reason);
                                        var completed = false;
                                        foreach (var v in createTestCase(forkingResponse.LeftFork.Operations, preregisteredDependencies, dependencyCount, managerTestCase, forkReasons.ToList(), testStats, genericTestExpectationComparerType, explicitTestExpectationComparerTypes))
                                        {
                                            if (completed)
                                            {
                                                throw new Exception("Only the last response will have a null TestCaseData");
                                            }

                                            if (v.ExpectedResponse != null)
                                            {
                                                yield return v;
                                            }
                                            else
                                            {
                                                dependencyCount = v.DependencyCount;
                                                preregisteredDependencies = v.PreregisteredDependencies;
                                                forkReasons = v.ForkReasons;
                                                completed = true;
                                            }
                                        }

                                        // handle the RightFork
                                        // if the right fork is a MockResponse, we can handle it here
                                        //      if it is a ForkingResponse, we need to invoke create.
                                        var preregisteredDependencyFromCopy = copyOfPreregisteredDependencies.Where(s => s.FacetType.FullName == op.FacetType.FullName).FirstOrDefault();
                                        if (preregisteredDependencyFromCopy == null)
                                        {
                                            throw new Exception("this should already be set before the copy");
                                        }
                                        preregisteredDependencyFromCopy.Proxy.Enqueue(new MockProxy.Response
                                        {
                                            ExpectedInvocationOrder = copyOfDependencyCount,
                                            Item = forkingResponse.RightFork.Response,
                                            ExpectedRequest = op.ExpectedRequest,
                                            IngoredPropertyNames = op.IngoredExpectedRequestPropertyNames
                                        });
                                        // @TODO - do something with reason,  write to json output
                                        copyOfForkReasons.Add(forkingResponse.RightFork.Reason);
                                        completed = false;
                                        foreach (var v in createTestCase(forkingResponse.RightFork.Operations, copyOfPreregisteredDependencies, copyOfDependencyCount, managerTestCase, copyOfForkReasons, copyOfTestStats, genericTestExpectationComparerType, explicitTestExpectationComparerTypes))
                                        {
                                            if (completed)
                                            {
                                                throw new Exception("Only the last response will have a null TestCaseData");
                                            }

                                            if (v.ExpectedResponse != null)
                                            {
                                                yield return v;
                                            }
                                            else
                                            {
                                                //set the cound and serviceCollection
                                                copyOfDependencyCount = v.DependencyCount;
                                                preregisteredDependencies = v.PreregisteredDependencies;
                                                forkReasons = v.ForkReasons;
                                                completed = true;
                                            }
                                        }
                                        break;
                                    }
                                case TestCase.Response.MockResponse mockResponse:
                                    {
                                        preregisteredDependency.Proxy.Enqueue(new MockProxy.Response
                                        {
                                            ExpectedInvocationOrder = dependencyCount,
                                            Item = mockResponse.Response,
                                            ExpectedRequest = op.ExpectedRequest,
                                            IngoredPropertyNames = op.IngoredExpectedRequestPropertyNames
                                        });
                                        break;
                                    }
                                default:
                                    {
                                        if (op.Response != null)
                                        {
                                            throw new Exception($"Unhandled switch for typeof: {op.Response.GetType().FullName}");
                                        }
                                        preregisteredDependency.Proxy.Enqueue(new MockProxy.Response
                                        {
                                            ExpectedInvocationOrder = dependencyCount,
                                            Item = null,
                                            ExpectedRequest = op.ExpectedRequest,
                                            IngoredPropertyNames = op.IngoredExpectedRequestPropertyNames
                                        });
                                        break;
                                    }
                            }


                            break;
                        }
                    case TestCase.ExpectedResponseOperation op:
                        {
                            expectedResponseFound = true;

                            yield return new CreateTestCaseResponse
                            {
                                TestId = op.Id,
                                DependencyCount = dependencyCount,
                                PreregisteredDependencies = preregisteredDependencies,
                                ExpectedResponse = op.ExpectedResponse ?? new VoidResponse(),
                                ForkReasons = forkReasons.ToList(),
                                IngoredExpectedResponsePropertyNames = op.IngoredExpectedResponsePropertyNames
                            };
                            break;
                        }
                }
            }

            // once we get through all operations, there may have not been a ExpectedResponseOperation.
            // we still need to return a value
            if (!expectedResponseFound)
            {
                yield return new CreateTestCaseResponse
                {
                    DependencyCount = dependencyCount,
                    PreregisteredDependencies = preregisteredDependencies,
                    ForkReasons = forkReasons.ToList()
                };
            }
        }

        /// <summary>
        /// Used by explicit tests
        /// </summary>
        /// <param name="testCases"></param>
        /// <returns></returns>
        public static Task<bool[]> RunTestCases(IEnumerable<TestCase> testCases)
        {
            // invoke buildTestCaseDataRequests
            // run test per item
            //foreach (var testCaseDataRequest in buildTestCaseDataRequests(testCases))
            //{

            //    yield return await runTestCase(testCaseDataRequest);
            //}

            return Task.WhenAll(
                buildTestCaseDataRequests(testCases).Select(testCaseDataRequest => RunTestCase(testCaseDataRequest))
            );
        }

        public static async Task<bool> RunTestCase(TestCaseDataRequest testCaseDataRequest, TestStatStore? testStatStore = null)
        {
            //DEV NOTE: removing the Hosting.Register.  this invocation will be required in the test case.
            var conceptInterfaceTypeName = testCaseDataRequest.TestCase.ConceptInterfaceType.FullName; // $"Examples.Manager.WhatEver.Interface.IWhateverManager";
            //var conceptInterfaceNamespace = testCaseDataRequest.TestCase.ConceptInterfaceType.Namespace; // $"Examples.Manager.WhatEver.Interface";
            //var conceptServiceNamespace = conceptInterfaceNamespace.Replace("Interface", "Service"); // $"Examples.Manager.WhatEver.Service";

            //var conceptAssembly = Assembly.Load(conceptServiceNamespace);
            //// @TODO: we can remove Hosting.Register invocation and force it to be supplied in the TestCase.RegisterDependency delegate
            //var conceptHostingRegisterMethod = conceptAssembly.GetTypes().Where(t => t.Name == "Hosting").First().GetMethod("Register", BindingFlags.Public | BindingFlags.Static);

            //var conceptHostingRegisterArgs = new List<object> { testCaseDataRequest.ServiceCollection };
            //conceptHostingRegisterArgs.AddRange(testCaseDataRequest.TestCase.OptionalHostingArgs);
            //conceptHostingRegisterMethod.Invoke(null, conceptHostingRegisterArgs.ToArray());

            // build the service provider
            var serviceProvider = testCaseDataRequest.ServiceCollection.BuildServiceProvider();
            var componentService = testCaseDataRequest.ServiceCollection.Where(serviceDescriptor =>
            {
                return serviceDescriptor.ServiceType.FullName == conceptInterfaceTypeName;
            }).First();
            //var facet = serviceProvider.GetService(componentService.ServiceType);
            //if (facet == null)
            //{
            //    throw new Exception("Service not registered. Perhaps missing a call to Hosting.Register?");
            //}
            // getting the interface load the registered service and invoke
            //var managerAssembly = Assembly.Load(conceptInterfaceNamespace);
            //var facet = managerAssembly.GetType(conceptInterfaceTypeName);
            var method = componentService.ServiceType.GetMethods().Where(m => m.Name == testCaseDataRequest.TestCase.Method).First();

            var facetMethod = method;
            // @TODO - methodArgType could be used to test request type with the method args (either explicit or derived)
            //var methodArgType = facetMethod.GetParameters()[0].ParameterType.GenericTypeArguments[0];

            // invoke operation
            var manager = serviceProvider.GetService(componentService.ServiceType);
            var task = (Task)facetMethod.Invoke(manager, new object[] { testCaseDataRequest.TestCase.Request, CancellationToken.None });
            await task.ConfigureAwait(false);

            // get response
            var resultProperty = task.GetType().GetProperty("Result");
            var response = resultProperty.GetValue(task);
            if (response.GetType().FullName != "System.Threading.Tasks.VoidTaskResult")
            {
                var responseData = response.GetType().GetProperty("Data").GetValue(response);

                var expectedResponseData = testCaseDataRequest.ExpectedResponse.GetType().GetProperty("Data").GetValue(testCaseDataRequest.ExpectedResponse);

                Assert.AreEqual(expectedResponseData.GetType().FullName, responseData.GetType().FullName);

                // running compare logic
                var comparerTypes = getComparerTypes();
                var explicitTestExpectationComparer = comparerTypes.Item2.Where(i => i.GetGenericArguments()[0] == expectedResponseData.GetType()).SingleOrDefault();
                if (explicitTestExpectationComparer != null)
                {
                    // an explicit comparer was found for the response so we use that to compere
                    var explicitTestInstance = Activator.CreateInstance(explicitTestExpectationComparer);
                    var explicitCompareMethod = explicitTestExpectationComparer.GetMethod("Compare");
                    explicitCompareMethod.Invoke(explicitTestInstance,
                        new[] { expectedResponseData, responseData, testCaseDataRequest.IngoredExpectedResponsePropertyNames });
                }
                else
                {
                    // runs the generic test comparer wich does a deep compare of each property to property
                    var genericTestInstance = Activator.CreateInstance(comparerTypes.Item1);
                    var genericCompareMethod = comparerTypes.Item1.GetMethod("Compare");
                    genericCompareMethod.Invoke(genericTestInstance,
                        new[] { expectedResponseData, responseData, testCaseDataRequest.IngoredExpectedResponsePropertyNames });
                }
            }
            else
            {
                Assert.True(testCaseDataRequest.ExpectedResponse is VoidResponse);
            }

            if (testStatStore != null)
            {
                // @TODO - set the request type and response type, if not in the list in testStatStore.
                // used to click through use cases in the call chain
            }

            return true;
        }

        private static IEnumerable<TestCaseDataRequest> buildTestCaseDataRequests(IEnumerable<TestCase> testCases)
        {
            foreach (var testCase in testCases)
            {
                var testCaseDatas = buildTestCaseData(
                    testCase,
                    new TestStats(),
                    new TestStatStore(),
                    null,
                    new List<Type>()
                );

                foreach (var testCaseData in testCaseDatas)
                {
                    yield return testCaseData.Arguments[0] as TestCaseDataRequest;
                }
            }
        }

        /// <summary>
        /// the caller will iterate the tuples, using service to invoke hosting.  it will return the IServiceProvider
        /// </summary>
        /// <param name="managerTestCase"></param>
        /// <returns></returns>
        public static IEnumerable<TestCaseData> buildTestCaseData(TestCase managerTestCase, TestStats testStats, TestStatStore testStatStore, Type? genericTestExpectationComparerType, IEnumerable<Type> explicitTestExpectationComparerTypes)
        {
            var requestType = managerTestCase.Request.GetType().FullName.Contains("Request") ? managerTestCase.Request.GetType().GetProperty("Data").GetValue(managerTestCase.Request).GetType() : managerTestCase.Request.GetType();
            var context = requestType.Namespace.Contains("UseCases.") ? requestType.Namespace.Substring(requestType.Namespace.IndexOf("UseCases.")).Replace("UseCases.", "") : "Default";

            var createTestCaseData = new Func<TestCase, IServiceCollection, CreateTestCaseResponse, TestCaseData>((managerTestCase, serviceCollection, createTestCaseResponse) =>
            {
                var expectedResponseData = createTestCaseResponse.ExpectedResponse is not VoidResponse ? createTestCaseResponse.ExpectedResponse.GetType().GetProperty("Data").GetValue(createTestCaseResponse.ExpectedResponse) : typeof(VoidResponse);

                var testCaseDataRequest = new TestCaseDataRequest
                {
                    TestCase = managerTestCase,
                    ServiceCollection = serviceCollection,
                    Category = $"Manager.{managerTestCase.ConceptInterfaceType}.{context}.{requestType.Name}.{expectedResponseData.GetType().Name}",
                    ExpectedResponse = createTestCaseResponse.ExpectedResponse,
                    TestName = createTestCaseResponse.ForkReasons.Any()
                        ? $"{managerTestCase.ConceptInterfaceType}.UseCases.{context}.{expectedResponseData.GetType().Name} Under Condition; {string.Join(" | ", createTestCaseResponse.ForkReasons)}  ID: {createTestCaseResponse.TestId}"
                        : $"{managerTestCase.ConceptInterfaceType}.UseCases.{context}.{expectedResponseData.GetType().Name} ID: {createTestCaseResponse.TestId}",
                    IngoredExpectedResponsePropertyNames = createTestCaseResponse.IngoredExpectedResponsePropertyNames
                };

                var testCaseData = new TestCaseData(testCaseDataRequest, testStatStore);

                testCaseData.SetCategory($"{context}.{requestType.Name}");
                testCaseData.SetName(testCaseDataRequest.TestName);
                testCaseData.Returns(true);

                // now set the UseCaseTracking on the TestStatStore
                var operations = new List<TestStatStore.UseCaseTracking.Operation>();
                var testId = string.Empty;
                foreach (var prd in createTestCaseResponse.PreregisteredDependencies)
                {
                    var index = operations.Where(o => o.TypeName == prd.FacetType.FullName).Count();
                    var mockResponse = prd.Proxy.OrderedInvocations.ElementAt(index);
                    operations.Add(new TestStatStore.UseCaseTracking.Operation
                    {
                        TypeName = prd.FacetType.FullName,
                        RequestTypeName = mockResponse.ExpectedRequest.GetType().FullName.Contains("Request") ? mockResponse.ExpectedRequest.GetType().GetProperty("Data").GetValue(mockResponse.ExpectedRequest).GetType().FullName : mockResponse.ExpectedRequest.GetType().FullName, // mockResponse.ExpectedRequest.GetType().FullName,
                        ResponseTypeName = mockResponse.Item is not null
                            ? mockResponse.Item.GetType().FullName.Contains("Response") ? mockResponse.Item.GetType().GetProperty("Data").GetValue(mockResponse.Item).GetType().FullName : mockResponse.Item.GetType().FullName
                            : string.Empty
                    });
                }

                testStatStore.UseCaseTrackings = testStatStore.UseCaseTrackings.Append(new TestStatStore.UseCaseTracking
                {
                    TestId = createTestCaseResponse.TestId.ToString(),
                    ConceptTypeName = managerTestCase.ConceptInterfaceType.FullName,
                    Context = context,
                    RequestTypeName = managerTestCase.Request.GetType().FullName.Contains("Request") ? managerTestCase.Request.GetType().GetProperty("Data").GetValue(managerTestCase.Request).GetType().FullName : managerTestCase.Request.GetType().FullName,
                    ResponseTypeName = expectedResponseData.GetType().FullName,
                    Reasons = createTestCaseResponse.ForkReasons.ToList(),
                    Operations = operations
                });

                return testCaseData;
            });

            foreach (var testCase in createTestCase(managerTestCase.Operations, new List<PreregisteredDependency>(), 0, managerTestCase, new List<string>(), testStats, genericTestExpectationComparerType, explicitTestExpectationComparerTypes))
            {
                //if (!testStatStore.InvokedServices.ContainsKey(managerTestCase.ConceptInterfaceType.FullName))
                //{
                //    testStatStore.InvokedServices.Add(managerTestCase.ConceptInterfaceType.FullName, new List<TestStatStore.InvokedService>());
                //}

                if (testCase.ExpectedResponse == null)
                {
                    //@TODO - VoidResponse should be set when an expected operation is found with a null response.
                    // find anywhere looking for null or setting a VoidResponse.
                    continue;
                }

                var serviceCollection = new ServiceCollection();
                managerTestCase.ServiceRegistration?.Invoke(serviceCollection);

                foreach (var svc in testCase.PreregisteredDependencies)
                {
                    serviceCollection.AddScoped(svc.FacetType, sp => svc.Proxy);
                }
                yield return createTestCaseData(managerTestCase, serviceCollection, testCase);
            }
        }

        public class TestStats
        {
            /// <summary>
            /// The number of times any service was invoked during a test.
            /// </summary>
            public int InvocationCount { get; set; }

            public TestStats Clone()
            {
                var testStats = new TestStats();
                testStats.InvocationCount = InvocationCount;
                return testStats;
            }
        }
        // Category is Manager, Name is Volatility, Description is $"{requestType} to {responseType} for reasons, {reason chain}"
        public static IEnumerable<TestCaseData> TestCaseData()
        {
            var testStatStore = new TestStatStore();
            // need to set up the singleton store...
            // we can instantiate it here and pass it along

            // to set up the singleton store, we are going to use a single mock for all services.
            // that mock will have the store.  then we can find all request/repsonses and set that in the store.
            // then when the mock is invoked, it can track invocation index and add its request/response types
            // hmm...
            //      the end of the test needs to test all request responses... how do we deal with that?
            //>>>

            // how does the 'AfterAll' part of the tests get the store?

            // we need a testStatStore for every type being invoked so it can track each
            // this doesn't work for the count because the count is per test.
            // we can wrap it... 

            // string type if used for the interface fullname
            var discoveredInterfaceDtosKvp = new Dictionary<string, IEnumerable<Type>>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var testCaseTypes = getTestCaseTypes(); // new List<Type>();
            var comparerTypes = getComparerTypes();
            Type? genericTestModelComparerType = comparerTypes.Item1; //ITestModelComparer
            var explicitTestModelComparerTypess = comparerTypes.Item2; //ITestModelComparer<T>

            foreach (var testCaseType in testCaseTypes)
            {
                var testCaseEnumerable = Activator.CreateInstance(testCaseType) as IEnumerable<TestCase>;
                if (testCaseEnumerable == null)
                {
                    throw new Exception($"Could not create instance for type {testCaseType.FullName}");
                }

                foreach (var item in testCaseEnumerable)
                {
                    if (!discoveredInterfaceDtosKvp.ContainsKey(item.ConceptInterfaceType.FullName))
                    {
                        // get all request and response using the base classes by checking all classes (recursively) for matching base
                        // @TODO

                        //testStatStore = discoveredInterfaceDtosKvp[item.ConceptInterfaceType.FullName];
                    }

                    foreach (var testCase in buildTestCaseData(item, new TestStats(), testStatStore, genericTestModelComparerType, explicitTestModelComparerTypess))
                    {
                        yield return testCase;
                    }
                }
            }

            // @TODO - all tests have run.  Now we can compare the dtos.


            // this is supposed to get the system report... however, something is missing.
            // condition reasons
            // the request 
            // the response
            /*
             i should expect to see;
                AdministrationManager
                    Web
                        Reqeusts: [
                          RequestA
                            { condition: "because", operations: [...], response: ResponseA }
                            { condition: "not because", operations: [...],, response: ResponseB }
                        ]
                        Listeners: [
                            MessageA
                                { operations: [...] }
                        ]

            */
            var outputResultsFilePath = Environment.GetEnvironmentVariable("CoarseSoftwareSystemResults");

            if (!string.IsNullOrEmpty(outputResultsFilePath))
            {
                var systemResults = new SystemResults
                {
                    UseCaseTrackings = testStatStore.UseCaseTrackings
                };


                // writing the json
                //string assemblyPath = Assembly.GetExecutingAssembly().Location;
                //string assemblyDirectory = Path.GetDirectoryName(assemblyPath);
                //string textPath = Path.Combine(assemblyDirectory, "CoarseSoftwareSystemResults.json");
                string textPath = outputResultsFilePath;

                string json =
                    JsonSerializer.Serialize(systemResults, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(textPath, $"{json}");
            }
        }

        private static IEnumerable<Type> getTestCaseTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("System") || assembly.FullName.Contains("Microsoft"))
                {
                    continue;
                }
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName.Contains("System") || type.FullName.Contains("Microsoft"))
                    {
                        continue;
                    }
                    var testCaseInterfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).Where(i => i.GetGenericArguments()[0] == typeof(TestCase)).ToList();
                    if (testCaseInterfaces.Any())
                    {
                        yield return type;
                        continue;
                    }
                }
            }
        }

        private static Tuple<Type, IEnumerable<Type>> getComparerTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type? genericTestModelComparerType = null; //ITestModelComparer
            var explicitTestModelComparerTypess = new List<Type>(); //ITestModelComparer<T>
            foreach (var assembly in assemblies)
            {
                if (assembly.FullName.Contains("System") || assembly.FullName.Contains("Microsoft"))
                {
                    continue;
                }
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName.Contains("System") || type.FullName.Contains("Microsoft"))
                    {
                        continue;
                    }

                    if (genericTestModelComparerType == null)
                    {
                        //look for ITestModelComparer
                        if (type.GetInterfaces().Where(i => i == typeof(ITestExpectationComparer)).Any())
                        {
                            genericTestModelComparerType = type;
                            continue;
                        }
                    }
                    //look for ITestModelComparer<T>
                    if (type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITestExpectationComparer<>)).Any())
                    {
                        explicitTestModelComparerTypess.Add(type);
                    }
                }
            }

            if (genericTestModelComparerType == null)
            {
                // no ITestModelComparer implementation found so we default to the basic
                genericTestModelComparerType = typeof(CoarseSoftware.Testing.Framework.Core.Comparer.BasicTestExpectationComparer);
            }

            return new Tuple<Type, IEnumerable<Type>>(genericTestModelComparerType, explicitTestModelComparerTypess);
        }

        public class SystemResults
        {
            public SystemResults()
            {
                this.UseCaseTrackings = new List<TestStatStore.UseCaseTracking>();
                this.ManagerListeners = new List<Listeners>();
            }
            public IEnumerable<TestStatStore.UseCaseTracking> UseCaseTrackings { get; set; }

            public IEnumerable<Listeners> ManagerListeners { get; set; }

            public class Listeners
            {
                public string ManagerTypeName { get; set; }
                public IEnumerable<string> RegisteredListeners { get; set; }
            }
        }

        public class VoidResponse { }
    }
}
