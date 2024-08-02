namespace CoarseSoftware.Testing.Framework.Core
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Client test case
    /// </summary>
    public class ClientTestCase
    {
        public ClientTestCase(string description)
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
        public Action<IServiceCollection> ServiceRegistration { get; set; }

        /// <summary>
        /// This is used to setup and run the client entry points.
        /// This can be used to test full middleware or invoking the controller directly.  Possibly instantiating your MVVM and invoking a Command.
        /// Generics - response is the expected response.  Possibly null.
        /// </summary>
        public Func<IServiceProvider, object> EntryPoint { get; set; }

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

        //public class ClientTestStats
        //{
        //    public IEnumerable<Service> Services { get; set; }
        //    public class Service
        //    {
        //        public string TypeName { get; set; }
        //        public string MethodName { get; set; }
        //        public IEnumerable<string> RequestTypeNames { get; set; }
        //        public string ResponseTypeName { get; set; }
        //        public IEnumerable<Service> ChildServices { get; set; }
        //    }
            

        //}
    }
}
