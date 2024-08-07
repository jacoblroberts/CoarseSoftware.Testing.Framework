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
            this.MessageListeners = new List<MessageListener>();
            this.ClientTestStats = new List<ClientTestStat>();
            this.ServiceStats = new List<ServiceStat>();
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

        public IEnumerable<ClientTestStat> ClientTestStats { get; set; }

        public IEnumerable<ServiceStat> ServiceStats {  get; set; }
        /// <summary>
        /// All listeners and where they are registerd.
        /// </summary>
        public IEnumerable<MessageListener> MessageListeners { get; set; }

        public class InvokedService
        {
            public InvokedService()
            {
                this.Services = new List<Type>();
            }
            public string Context { get; set; }
            public IEnumerable<Type> Services { get; set; }
        }

        public class MessageListener
        {
            public MessageListener()
            {
                this.RegisteredListeners = new List<string>();
            }
            public string ManagerTypeName { get; set; }
            public IEnumerable<string> RegisteredListeners { get; set; }
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
            //public IEnumerable<string> Reasons { get; set; }
            public IEnumerable<ForkReason> Reasons { get; set; }

            /// <summary>
            /// The invoked operations
            /// </summary>
            public IEnumerable<Operation> Operations { get; set; }

            public class ForkReason
            {
                public string Reason { get; set; }
                public Operation Operation { get; set; }
            }

            public class Operation
            {
                /// <summary>
                /// The invoked operation type.  engine, access, resource, utility...
                /// </summary>
                public string TypeName { get; set; }

                public string Method { get; set; }

                public string RequestTypeName { get; set; }

                /// <summary>
                /// some things like logging, publishing... have no response
                /// </summary>
                public string? ResponseTypeName { get; set; }
            }
        }

        public class ClientTestStat
        {
            public ClientTestStat()
            {
                this.Services = new List<Service>();
            }
            public Guid Id { get; set; }
            public string ClientName { get; set; }
            public string Description { get; set; }
            public IEnumerable<Service> Services { get; set; }
            public class Service
            {
                public string TypeName { get; set; }
                public string MethodName { get; set; }
                public IEnumerable<string> RequestTypeNames { get; set; }
                public string ResponseTypeName { get; set; }
                public IEnumerable<Service> ChildServices { get; set; }
            }


        }

        /// <summary>
        /// This is the most generic service invocation graph
        /// </summary>
        public class ServiceStat
        {
            public ServiceStat()
            {
                this.ChildServices = new List<ServiceStat>();
            }
            public Guid Key { get; set; }
            public string TypeName { get; set; }
            public string MethodName { get; set; }
            public IEnumerable<string> RequestTypeNames { get; set; }
            public string ResponseTypeName { get; set; }
            public IEnumerable<ServiceStat> ChildServices { get; set; }
        }
    }
}
