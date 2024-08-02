namespace CoarseSoftware.Testing.Framework.Core
{
    public class TestRunnerConfiguration
    {
        public TestRunnerConfiguration(
            int maxDifferencesBeforeFailing = 20,
            bool allowNullResponseData = false,
            string systemResultsOutputPath = "",
            ServiceTypeWildcard wildcard = null,
            DtoWrapper requestWrapper = null,
            DtoWrapper responseWrapper = null,
            ActualDumpBreakType breakOnActualDumpBreakPoint = 0,
            bool ignoreDateTimeComparison = false
            )
        {
            this.MaxDifferencesBeforeFailing = maxDifferencesBeforeFailing;
            this.AllowNullResponseData = allowNullResponseData;
            this.SystemResultsOutputPath = systemResultsOutputPath;
            this.Wildcard = wildcard ?? new ServiceTypeWildcard();
            this.RequestWrapper = requestWrapper;
            this.ResponseWrapper = responseWrapper;
            this.BreakOnActualDumpBreakPoint = breakOnActualDumpBreakPoint;
            this.IgnoreDateTimeComparison = ignoreDateTimeComparison;

            InternalTestRunnerConfiguration.MaxDifferencesBeforeFailing = maxDifferencesBeforeFailing;
            InternalTestRunnerConfiguration.SystemResultsOutputPath = systemResultsOutputPath;
            InternalTestRunnerConfiguration.BreakOnActualDumpBreakPoint = breakOnActualDumpBreakPoint;
            InternalTestRunnerConfiguration.IgnoreDateTimeComparison = ignoreDateTimeComparison;
        }

        /// <summary>
        /// If a large data object tuncates, you can use this to break on the dump object to copy the entire output.
        /// </summary>
        public ActualDumpBreakType BreakOnActualDumpBreakPoint { get; private set; }
        /// <summary>
        /// Defaults to 20.  
        /// </summary>
        public int MaxDifferencesBeforeFailing { get; private set; }

        public bool AllowNullResponseData { get; private set; }

        public string SystemResultsOutputPath {  get; private set; }

        /// <summary>
        /// ignores DateTime and DateTimeOffset comparison.  
        /// Needing this is indicitive of mis-handling DateTimes.  ie; mapping from DateTimeOffset to DateTime uses the system TZ, when it needs to be customer or venue TZ.
        /// </summary>
        public bool IgnoreDateTimeComparison { get; private set; }

        /// <summary>
        /// used to pull out the dto type name from the request object.  Ie; Request<T> { T Data; } where Data is our dto
        /// </summary>
        public DtoWrapper RequestWrapper {  get; private set; }

        /// <summary>
        /// used to pull out the dto type name from the response object.  Ie; Response<T> { T Data; } where Data is our dto
        /// </summary>
        public DtoWrapper ResponseWrapper { get; private set; }

        /// <summary>
        /// These are used to determine if a service being invoked is a system component
        /// Values can be service facet full names (namespace.facet) or wildcards (Component.Concept)
        /// </summary>
        public ServiceTypeWildcard Wildcard { get; private set; }

        public enum ActualDumpBreakType
        {
            Never,
            Debugging,
            Always
        }

        public class DtoWrapper
        {
            /// <summary>
            /// ie; typeof(Request<>)
            /// </summary>
            public Type OpenWrapperType { get; set; }
            public string DtoPropertyName { get; set; }
        }

        public class ServiceTypeWildcard
        {
            public ServiceTypeWildcard()
            {
                this.ManagerFacetWildCards = new List<string>();
                this.EngineFacetWildCards = new List<string>();
                this.AccessFacetWildCards = new List<string>();
                this.ResourceFacetWildCards = new List<string>();
                this.UtilityFacetWildCards = new List<string>();
            }

            public IEnumerable<string> ManagerFacetWildCards { get; set; }
            public IEnumerable<string> EngineFacetWildCards { get; set; }
            public IEnumerable<string> AccessFacetWildCards { get; set; }
            public IEnumerable<string> ResourceFacetWildCards { get; set; }
            public IEnumerable<string> UtilityFacetWildCards { get; set; }
        }
    }

    /// <summary>
    /// this is used to simplify the needs for config values so an instance doesn't need to be created each time.
    /// </summary>
    internal static class InternalTestRunnerConfiguration
    {
        public static int MaxDifferencesBeforeFailing { get; set; }
        public static string SystemResultsOutputPath { get; set; }
        public static TestRunnerConfiguration.ActualDumpBreakType BreakOnActualDumpBreakPoint { get; set; }
        public static bool IgnoreDateTimeComparison { get; set; }
    }
}
