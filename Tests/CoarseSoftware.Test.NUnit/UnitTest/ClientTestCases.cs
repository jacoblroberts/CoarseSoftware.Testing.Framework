namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.UnitTest
{
    using global::System.Collections;
    using CoarseSoftware.Testing.Framework.Core;
    using Microsoft.Extensions.DependencyInjection;
    using DashboardManagerFacet = CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface;
 
    using RegulatingEngineFacet = CoarseSoftware.BusinessSystem.Component.Engine.Regulating.Interface;
    using ValidatingEngineFacet = CoarseSoftware.BusinessSystem.Component.Engine.Validating.Interface;

    using JobAccessFacet = CoarseSoftware.BusinessSystem.Component.Access.Job.Interface;

    using JobResourceFacet = CoarseSoftware.BusinessSystem.Resource.Job.Interface;
    using CoarseSoftware.BusinessSystem.iFX;

    public class ClientTestCases : IEnumerable<ClientTestCase>
    {
        public IEnumerator<ClientTestCase> GetEnumerator()
        {
            yield return new ClientTestCase("Some client test case")
            {
                Client = "Test Client",
                Id = Guid.Parse("f179283b-e339-4e59-9af8-e3e7c3211d8d"),
                ServiceRegistration = serviceCollection =>
                {
                    serviceCollection.AddScoped<IWorker, Worker>();
                    serviceCollection.AddScoped<DashboardManagerFacet.IDashboardManager, CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Service.DashboardManager>();
                    serviceCollection.AddScoped<RegulatingEngineFacet.IRegulatingEngine, CoarseSoftware.BusinessSystem.Component.Engine.Regulating.Service.RegulatingEngine>();
                    serviceCollection.AddScoped<ValidatingEngineFacet.IValidatingEngine, CoarseSoftware.BusinessSystem.Component.Engine.Validating.Service.ValidatingEngine>();
                    serviceCollection.AddScoped<JobAccessFacet.IJobAccess, CoarseSoftware.BusinessSystem.Component.Access.Job.Service.JobAccess>();
                    serviceCollection.AddScoped<JobResourceFacet.IJobResource, CoarseSoftware.BusinessSystem.Resource.Job.Service.JobResource>();
                },
                EntryPoint = sp =>
                {
                    var start = new Start(sp);
                    var task = start.DoWork();
                    task.Wait();
                    return task.Result;
                },
                ExpectedResponse = true,
                Service = new ClientTestCase.Microservice
                {
                    FacetType = typeof(DashboardManagerFacet.IDashboardManager),
                    ExpectedMethodName = "FlowAsync",
                    ExpectedRequest = new CoarseSoftware.BusinessSystem.iFX.Request<CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface.OnStepCompleteBase>
                    {
                        Data = new CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface.OnStepCompleteBase
                        {

                        }
                    },
                    MockResponse = new CoarseSoftware.BusinessSystem.iFX.Response<DashboardManagerFacet.OnStepActivateBase>
                    {
                        Data = new DashboardManagerFacet.DerivedOnStepActivate
                        {
                            StaticId = "123"
                        }
                    }
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Start
    {
        private readonly IServiceProvider serviceProvider;
        public Start(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<bool> DoWork()
        {
            var worker = serviceProvider.GetService<IWorker>();
            await worker.DoWork();

            return true;
        }
    }

    public interface IWorker
    {
        Task<bool> DoWork();
    }
    public class Worker: IWorker
    {
        private readonly IServiceProvider serviceProvider;
        public Worker(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<bool> DoWork()
        {
            var microservice = serviceProvider.GetService<CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface.IDashboardManager>();
            await microservice.FlowAsync(new CoarseSoftware.BusinessSystem.iFX.Request<CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface.OnStepCompleteBase>
            {
                Data = new CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface.OnStepCompleteBase
                {

                }
            }, CancellationToken.None);

            return true;
        }
    }
}
