namespace CoarseSoftware.Testing.Framework.Core
{
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;

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
        public class WebApplicationEntryPointWrapper<TProgram>: EntryPointWrapper where TProgram : class
        {
            public Func<WebApplicationFactory<TProgram>, object> EntryPoint { get; set; }

            /// <summary>
            /// this is invoked by the test runner, which will switch on type, instantiate what is needed for the delegate and invoke it.
            /// </summary>
            public object InvokeEntryPoint()
            { 
                var webApplicationFactory = new WebApplicationFactory<TProgram>();
                return this.EntryPoint.Invoke(webApplicationFactory);
            }
        }
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
