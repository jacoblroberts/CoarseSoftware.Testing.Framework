namespace CoarseSoftware.Testing.Framework.Core.Proxy
{
    /*
     These are only used in the test itself... the discovered are found but the invocations happen in the test..
     how would we get all invoced?
        this can be a singleton where we load them all across use cases.

    this needs to store ALL of the data we will use to build the json output

    a) for the concept facet as a key, we store engine, access and utitily invocations (across all tests)
            for managers, we need to track message listeners
     */
    public class TestStatStore
    {
        public TestStatStore()
        {
            this.DiscoveredDtoTypes = new List<Type>();
            this.InvokedDtoTypes = new List<Type>();

            this.UseCaseTrackings = new List<UseCaseTracking>();
        }

        /// <summary>
        /// The request and respoinse objects found for a particular service.
        /// </summary>
        public IEnumerable<Type> DiscoveredDtoTypes { get; set; }

        /// <summary>
        /// The request and response objects used for a test.
        /// </summary>
        public IEnumerable<Type> InvokedDtoTypes { get; set; }

        /// <summary>
        /// string is the concept facet, type is the invoked concept or utility
        /// this doesn't work.. we need it per concept, per use case.
        /// </summary>
        //public Dictionary<string, IEnumerable<InvokedService>> InvokedServices { get; set; }
        public IEnumerable<UseCaseTracking> UseCaseTrackings { get; set; }
        public class InvokedService
        {
            public InvokedService()
            {
                this.Services = new List<Type>();
            }
            public string Context { get; set; }
            public IEnumerable<Type> Services { get; set; }
        }

        /// <summary>
        /// One of these will be created for EVERY test created
        /// this is used to dervice the wiki and diagrams
        /// </summary>
        public class UseCaseTracking
        {
            /// <summary>
            /// Matches the test case
            /// </summary>
            public string TestId { get; set; }
            /// <summary>
            /// like; Examples.Manager.Dashboard.Interface.IDashboardManager
            /// </summary>
            public string ConceptTypeName { get; set; }

            /// <summary>
            /// like; Web, Phone...
            /// </summary>
            public string Context { get; set; }

            /// <summary>
            /// The request that was sent to the use case
            /// </summary>
            public string RequestTypeName { get; set; }

            /// <summary>
            /// The response
            /// </summary>
            public string ResponseTypeName { get; set; }

            /// <summary>
            /// Ordered fork reasons
            /// </summary>
            public IEnumerable<string> Reasons { get; set; }

            /// <summary>
            /// The invoked operations
            /// </summary>
            public IEnumerable<Operation> Operations { get; set; }

            public class Operation
            {
                /// <summary>
                /// The invoked operation type.  engine, access, resource, utility...
                /// </summary>
                public string TypeName { get; set; }

                //the below props don't work yet... need to get the upstream, which is annoying. 
                // if the typeName contains a message type utility, then we can use the ReqeustTypeName to track message publishing
                public string RequestTypeName { get; set; }

                /// <summary>
                /// some things like logging, publishing... have not response
                /// </summary>
                public string? ResponseTypeName { get; set; }
            }
        }
    }
}
