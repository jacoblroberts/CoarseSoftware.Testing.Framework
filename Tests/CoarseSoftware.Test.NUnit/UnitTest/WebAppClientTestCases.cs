namespace CoarseSoftware.Test.NUnit.UnitTest
{
    using CoarseSoftware.BusinessSystem.iFX;
    using CoarseSoftware.Testing.Framework.Core;
    using System.Collections;

    using DashboardManagerFacet = CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface;

    public class WebAppClientTestCases : IEnumerable<GenericClientTestCase>
    {
        public IEnumerator<GenericClientTestCase> GetEnumerator()
        {
            yield return new GenericClientTestCase("Weather forcast - get request test case")
            {
                Id = Guid.Parse("113ec18d-0001-47d2-ac05-ffb9f022c60b"),
                Client = "Web Api",
                ExpectedResponse = string.Empty,
                Service = new GenericClientTestCase.Microservice
                {
                    FacetType = typeof(DashboardManagerFacet.IDashboardManager),
                    ExpectedMethodName = "FlowAsync",
                    ExpectedRequest = new Request<DashboardManagerFacet.OnStepCompleteBase>
                    {
                        Data = new DashboardManagerFacet.OnStepCompleteBase
                        { }
                    },
                    MockResponse = new Response<DashboardManagerFacet.OnStepActivateBase>
                    {
                        Data = new DashboardManagerFacet.DerivedOnStepActivate
                        {
                            StaticId = "IdNumber1"
                        }
                    }
                },
                EntryPoint = new GenericClientTestCase.WebApplicationEntryPointWrapper<CoarseSoftware.Client.WebApi.Program>
                {
                    EntryPoint = (webAppFactory) =>
                    {
                        var client = webAppFactory.CreateClient();
                        var responseTask = client.GetAsync("/Dashboard/Flow");
                        responseTask.Wait();
                        var response = responseTask.Result;
                        return response;
                    }
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
