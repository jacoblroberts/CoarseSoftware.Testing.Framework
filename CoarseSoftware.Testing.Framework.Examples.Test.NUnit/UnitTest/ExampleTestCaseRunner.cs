namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.UnitTest
{
    using CoarseSoftware.Testing.Framework.Core;
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;
    using global::NUnit.Framework;

    public class ExampleTestCaseRunner
    {
        [TestFixture]
        public class Testing : TestCasesRunner
        { }

        public class Configuration: TestRunnerConfiguration
        {
            public Configuration()
              : base(
                    breakOnActualDumpBreakPoint: ActualDumpBreakType.Always, 
                    allowNullResponseData: true, 
                    maxDifferencesBeforeFailing: 50, 
                    systemResultsOutputPath: "C:\\SourceCode",
                    wildcard: new ServiceTypeWildcard
                    {
                        ManagerFacetWildCards = new List<string> { "System.Component.Manager" },
                        EngineFacetWildCards = new List<string> { "System.Component.Engine" },
                        AccessFacetWildCards = new List<string> { "System.Component.Access" },
                        ResourceFacetWildCards = new List<string> { "System.Resource" },
                        UtilityFacetWildCards = new List<string> { "System.iFX" },
                    },
                    requestWrapper: new DtoWrapper
                    {
                        OpenWrapperType = typeof(Request<>),
                        DtoPropertyName = "Data"
                    },
                    responseWrapper: new DtoWrapper
                    {
                        OpenWrapperType = typeof(Response<>),
                        DtoPropertyName = "Data"
                    },
                    ignoreDateTimeComparison: true
                )
            {
                
            }
        }
    }
}
