namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.UnitTest
{
    using global::System.Collections;
    using CoarseSoftware.Testing.Framework.Core;
    using Microsoft.Extensions.DependencyInjection;
    using DashboardManagerFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Dashboard.Interface;
 
    using RegulatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating.Interface;
    using ValidatingEngineFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating.Interface;

    using JobAccessFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Access.Job.Interface;

    using JobResourceFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Resource.Job.Interface;
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

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
                    serviceCollection.AddScoped<DashboardManagerFacet.IDashboardManager, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Dashboard.Service.DashboardManager>();
                    serviceCollection.AddScoped<RegulatingEngineFacet.IRegulatingEngine, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating.Service.RegulatingEngine>();
                    serviceCollection.AddScoped<ValidatingEngineFacet.IValidatingEngine, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating.Service.ValidatingEngine>();
                    serviceCollection.AddScoped<JobAccessFacet.IJobAccess, CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Access.Job.Service.JobAccess>();
                    serviceCollection.AddScoped<JobResourceFacet.IJobResource, CoarseSoftware.Testing.Framework.Examples.Test.System.Resource.Job.Service.JobResource>();
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
                    ExpectedRequest = new Test.System.iFX.Request<Test.System.Component.Manager.Dashboard.Interface.OnStepCompleteBase>
                    {
                        Data = new Test.System.Component.Manager.Dashboard.Interface.OnStepCompleteBase
                        {

                        }
                    },
                    MockResponse = new Test.System.iFX.Response<DashboardManagerFacet.OnStepActivateBase>
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
            var microservice = serviceProvider.GetService<Examples.Test.System.Component.Manager.Dashboard.Interface.IDashboardManager>();
            await microservice.FlowAsync(new Test.System.iFX.Request<Test.System.Component.Manager.Dashboard.Interface.OnStepCompleteBase>
            {
                Data = new Test.System.Component.Manager.Dashboard.Interface.OnStepCompleteBase
                {

                }
            }, CancellationToken.None);

            return true;
        }
    }
}
