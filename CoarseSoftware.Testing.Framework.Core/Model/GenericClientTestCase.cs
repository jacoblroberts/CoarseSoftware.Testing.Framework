namespace CoarseSoftware.Testing.Framework.Core
{
    using CoarseSoftware.Testing.Framework.Core.Proxy;
#if NET8_0
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
#endif
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    /// <summary>
    /// Client test case
    /// </summary>
    public class GenericClientTestCase //<TProgram> where TProgram : class
    {
        public GenericClientTestCase(string description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Unique/constant test Id.
        /// Note: if this is not constant (ie; Guid.Parse(guidString)), it will cause test discovery to find new tests every time.  You will know this is happening if your successful tests flip to not ran.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Client name
        /// </summary>
        public string Client { get; set; }

        /// <summary>
        /// Describes the test.
        /// Note: Could be used to provide some context clues as to which UI page this request should produce.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Used to register services.
        /// Note: if you want to overwrite a facet (ie; mock resource), register it last.  This way, if anything registers an instance, the last registration will win.
        /// </summary>
        //public Action<IServiceCollection> ServiceRegistration { get; set; }

        /// <summary>
        /// Web App uses type: WebApplicationEntryPointWrapper<TProgram>
        /// ...
        /// </summary>
        public EntryPointWrapper EntryPoint { get; set; }

        public abstract class EntryPointWrapper 
        {
        }

#if NET8_0
        public class WebApplicationEntryPointWrapper<TProgram>: EntryPointWrapper where TProgram : class
        {
            public Func<WebApplicationFactory<TProgram>, object> EntryPoint { get; set; }

            /// <summary>
            /// this is invoked by the test runner, which will switch on type, instantiate what is needed for the delegate and invoke it.
            /// </summary>
            public object InvokeEntryPoint(
                Type? genericTestExpectationComparerType,
                IEnumerable<Type> explicitTestExpectationComparerTypes,
                ClientTestCase.Microservice microservice,
                TestRunnerConfiguration configuration
                )
            {
                var webApplicationFactory = new CustomWebApplicationFactory<TProgram>(
                    genericTestExpectationComparerType,
                    explicitTestExpectationComparerTypes,
                    microservice,
                    configuration
                    ) as WebApplicationFactory<TProgram>;
                return this.EntryPoint.Invoke(webApplicationFactory);
            }

            public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class 
            {
                private readonly Type? genericTestExpectationComparerType;
                private readonly IEnumerable<Type> explicitTestExpectationComparerTypes;
                private readonly ClientTestCase.Microservice microservice;
                private readonly TestRunnerConfiguration configuration;
                public CustomWebApplicationFactory(
                    Type? genericTestExpectationComparerType,
                    IEnumerable<Type> explicitTestExpectationComparerTypes,
                    ClientTestCase.Microservice microservice,
                    TestRunnerConfiguration configuration
                    )
                {
                    this.genericTestExpectationComparerType = genericTestExpectationComparerType;
                    this.explicitTestExpectationComparerTypes = explicitTestExpectationComparerTypes;
                    this.microservice = microservice;
                    this.configuration = configuration;
                    this.serviceStats = new List<ServiceStat>();
                }

                public IEnumerable<ServiceStat> serviceStats {  get; private set; }
                private Dictionary<int, Guid> serviceProviderScopes = new Dictionary<int, Guid>();

                protected override void ConfigureWebHost(IWebHostBuilder builder)
                {
                    builder.ConfigureServices(services =>
                    {
                        // scenerios
                        //   invoking service in a loop
                        //      solved by service proxy invocation queue/blocking
                        //   invoking 2 services at once where both services are different but invoke a similar service.
                        //      needs resolution
                        //          can we get caller member?  ie; if the caller member could be a key but then how 
                        //          we can use the methodinfo to get the caller member
                        //              info is used as a key to track where it reports back
                        //              so we can have multiple open channels with unique callerinfoKey.  
                        //                  when a child is invoked, how do we set that child stat with the callerInfoKey?
                        //                      we do this by simply always having a callerInfoKey and when we close a channel it passed the parent caller
                        //    so the key is the openchannel gets a key.  key - the service being invoked.
                        //          parent key is the service invoking, which is passed in both open and close
                        //   invoking services in a loop, creating a new scope
                        //      solved by checking the sp's parent providers and attaching the status based on that index

                        // if i use the service provider as a copy where that copy can get the parent id
                        //  dict<serviceProvider.Hash, parentKey>
                        //   when serviceProvider is used, the descriptor delegate will get the parent key from the dict using sp.getHash()
                        //      it can then track the parent.  The service proxy, being generated at the same time will get the parent key.
                        //          now, we get to treat everything, even though it may be scoped or singleton, transitive like.  meaning the descriptor delegate will be invoked each time a service is requested.
     
                        Func<Guid, IEnumerable<ServiceStat>, ServiceStat> findOpenedParent = null;
                        var openChannel = new Action<Guid, Guid, TestStatStore.ClientTestStat.Service>((parentKey, key, serviceStat) =>
                        {
                            // find a matching parent key
                            // if none, we can assume this is the top most trackable
                            var openedParent = findOpenedParent.Invoke(parentKey, this.serviceStats);
                            var child = new ServiceStat
                            {
                                Key = key,
                                MethodName = serviceStat.MethodName,
                                RequestTypeNames = serviceStat.RequestTypeNames,
                                TypeName = serviceStat.TypeName
                            };
                            if (openedParent != null)
                            {
                                openedParent.ChildServices = openedParent.ChildServices.Append(child);
                            }
                            else
                            {
                                this.serviceStats = this.serviceStats.Append(child);
                            }
                        });
                        findOpenedParent = new Func<Guid, IEnumerable<ServiceStat>, ServiceStat>((parentKey, serviceStats) =>
                        {
                            foreach (var serviceStat in serviceStats)
                            {
                                if (!string.IsNullOrEmpty(serviceStat.ResponseTypeName))
                                {
                                    // already closed
                                    continue;
                                }
                                if (serviceStat.Key == parentKey)
                                {
                                    return serviceStat;
                                }
                                if (serviceStat.ChildServices.Any())
                                {
                                    var openedParent = findOpenedParent.Invoke(parentKey, serviceStat.ChildServices);
                                    if (openedParent != null)
                                    {
                                        return openedParent;
                                    }
                                }
                            }
                            return null;
                        });

                        Func<Guid, Guid, IEnumerable<ServiceStat>, ServiceStat> findOpenedService = null;
                        var closeChannel = new Action<Guid, Guid, string>((parentKey, key, responseTypeName) =>
                        {
                            var openedService = findOpenedService.Invoke(parentKey, key, this.serviceStats);
                            if (openedService == null)
                            {
                                throw new Exception("openedService should never be null");
                            }
                            openedService.ResponseTypeName = responseTypeName;
                        });
                        findOpenedService = new Func<Guid, Guid, IEnumerable<ServiceStat>, ServiceStat>((parentKey, key, serviceStats) =>
                        {
                            foreach (var serviceStat in serviceStats)
                            {
                                if (!string.IsNullOrEmpty(serviceStat.ResponseTypeName))
                                {
                                    // already closed
                                    continue;
                                }
                                if (serviceStat.Key == parentKey)
                                {
                                    var childService = serviceStat.ChildServices.Where(c => c.Key == key && string.IsNullOrEmpty(c.ResponseTypeName)).FirstOrDefault();
                                    if (childService != null)
                                    {
                                        return childService;
                                    }
                                    // no matching child
                                    continue;
                                }
                                if (serviceStat.ChildServices.Any())
                                {
                                    var openedParent = findOpenedService.Invoke(parentKey, key, serviceStat.ChildServices);
                                    if (openedParent != null)
                                    {
                                        return openedParent;
                                    }
                                }
                            }
                            return null;
                        });

                        // will use this to look up 
                        //IServiceCollection originalServices = new ServiceCollection();
                        // build this at the end..
                        //IServiceProvider serviceProvider = null;
                        foreach (var service in services)
                        {
                            //originalServices.Add(service);

                            var descriptor = new ServiceDescriptor(
                                service.ServiceType,
                                (sp) =>
                                {
                                    // do we care about the first sp?
                                    //  if sp is not in dict, we can assume it is for the first children
                                    // the parent key is actually the one that created this 
                                    Guid parentKey = Guid.Empty;
                                    var spHash = sp.GetHashCode();
                                    if (!this.serviceProviderScopes.ContainsKey(spHash))
                                    {
                                        parentKey = Guid.NewGuid();
                                        this.serviceProviderScopes.Add(spHash, parentKey);
                                    }
                                    else
                                    {
                                        parentKey = this.serviceProviderScopes[spHash];
                                    }
                                    var getService = new Func<Guid, object>((key) =>
                                    {
                                        var registeredImplementationType = service.ImplementationType;
                                        var hasEmptyOrDefaultConstr =
                                          registeredImplementationType.GetConstructor(Type.EmptyTypes) != null ||
                                          registeredImplementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                                        .Any(x => x.GetParameters().All(p => p.IsOptional));

                                        var scopedServiceProvider = services.BuildServiceProvider();
                                    // the descriptor delegate needs the parent key created here..
                                    // this way it can 

                                    this.serviceProviderScopes.Add(scopedServiceProvider.GetHashCode(), key); // Guid.NewGuid()); //<---
                                        var activatedService = hasEmptyOrDefaultConstr
                                            ? Activator.CreateInstance(registeredImplementationType)
                                            : Activator.CreateInstance(registeredImplementationType, new[] { scopedServiceProvider as IServiceProvider });
                                        return activatedService;
                                    });
                                    var proxy = CoarseSoftware.Testing.Framework.Core.ProxyV2.Client.MockServiceProxy.Create(
                                        service.ServiceType,
                                        openChannel,
                                        closeChannel,
                                        getService,
                                        parentKey,
                                        genericTestExpectationComparerType,
                                        explicitTestExpectationComparerTypes,
                                        microservice,
                                        configuration
                                        );
                                    return proxy;
                                },
                                service.Lifetime);
                        }
                        // @TODO - remove and replace sc
                        //var dbContextDescriptor = services.SingleOrDefault(
                        //    d => d.ServiceType ==
                        //        typeof(DbContextOptions<ApplicationDbContext>));

                        //services.Remove(dbContextDescriptor);

                        //var dbConnectionDescriptor = services.SingleOrDefault(
                        //    d => d.ServiceType ==
                        //        typeof(DbConnection));

                        //services.Remove(dbConnectionDescriptor);

                        //// Create open SqliteConnection so EF won't automatically close it.
                        //services.AddSingleton<DbConnection>(container =>
                        //{
                        //    var connection = new SqliteConnection("DataSource=:memory:");
                        //    connection.Open();

                        //    return connection;
                        //});

                        //services.AddDbContext<ApplicationDbContext>((container, options) =>
                        //{
                        //    var connection = container.GetRequiredService<DbConnection>();
                        //    options.UseSqlite(connection);
                        //});
                    });

                    builder.UseEnvironment("Development");
                }

                public class ServiceStat
                {
                    public Guid Key { get; set; }
                    public string TypeName { get; set; }
                    public string MethodName { get; set; }
                    public IEnumerable<string> RequestTypeNames { get; set; }
                    public string ResponseTypeName { get; set; }
                    public IEnumerable<ServiceStat> ChildServices { get; set; }
                }
            }
        }
#endif
        public class EmptyEntryPointWrapper : EntryPointWrapper
        {
            public Func<object> EntryPoint { get; set; }

            /// <summary>
            /// this is invoked by the test runner, which will switch on type, instantiate what is needed for the delegate and invoke it.
            /// </summary>
            public object InvokeEntryPoint()
            {
                return this.EntryPoint.Invoke();
            }
        }
        public class ServiceCollectionEntryPointWrapper : EntryPointWrapper
        {
            public Func<IServiceCollection, object> EntryPoint { get; set; }

            /// <summary>
            /// this is invoked by the test runner, which will switch on type, instantiate what is needed for the delegate and invoke it.
            /// </summary>
            public object InvokeEntryPoint(IServiceCollection serviceDescriptors)
            {
                return this.EntryPoint.Invoke(serviceDescriptors);
            }
        }
        public class ServiceProviderEntryPointWrapper : EntryPointWrapper
        {
            public Func<IServiceProvider, object> EntryPoint { get; set; }

            /// <summary>
            /// this is invoked by the test runner, which will switch on type, instantiate what is needed for the delegate and invoke it.
            /// </summary>
            public object InvokeEntryPoint(IServiceProvider serviceProvider)
            {
                return this.EntryPoint.Invoke(serviceProvider);
            }
        }

        /// <summary>
        /// This is the expected response from the EntryPoint invocation
        /// </summary>
        public object ExpectedResponse { set; get; }
        public IEnumerable<string> IngoredExpectedResponsePropertyNames { get; set; }

        /// <summary>
        /// Expectations of the microservice that will be invoked.
        /// </summary>
        public Microservice Service { get; set; }

        public class Microservice
        {
            /// <summary>
            /// The microservice we intend to invoke.  This will be any Manager service.
            /// </summary>
            public Type FacetType { get; set; }

            /// <summary>
            /// The FacetType method that will be invoked
            /// </summary>
            public string ExpectedMethodName { get; set; }

            /// <summary>
            /// Expected request.  This is important because we want to make sure the process to create this model, works as expected.  ie; deseralizing, mapping from original request, etc...
            /// </summary>
            public object ExpectedRequest { get; set; }

            /// <summary>
            /// Mock Response
            /// </summary>
            public object MockResponse { get; set; }

            public IEnumerable<string> IngoredRequestPropertyNames { get; set; }
        }
    }
}
