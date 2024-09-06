namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Service
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;
    using Microsoft.Extensions.DependencyInjection;
    using JobAccessFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Access.Job.Interface;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Logging;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event.Model;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event;
    using CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface;

    public class AdministrationManager: IAdministrationManager
    {
        private readonly IServiceProvider serviceProvider;
        public AdministrationManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<Response<OnStepActivateBase>> FlowAsync(Request<OnStepCompleteBase> stepComplete, CancellationToken cancellationToken)
        {
            if (stepComplete.Data is CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete)
            {
                return new Response<OnStepActivateBase>
                {
                    Data = new OnStepActivateBase
                    {
                        SomeBaseClassField = "Types Tested",
                        Multidimensional = new List<IEnumerable<string>>
                        {
                            new List<string>
                            {
                                "item-1-1",
                                "item-1-2",
                            },
                            new List<string>
                            {
                                "item-2-1",
                                "item-2-2",
                            }
                        },
                        Strings = new List<string>
                        {
                            "item-1",
                            "item-2"
                        }
                    }
                };
            }

            var jobAccess = this.serviceProvider.GetService<JobAccessFacet.IJobAccess>();

            var jobAccessResponse = jobAccess.FilterAsync(new Request<JobAccessFacet.RequestBase>
            {
                Data = new JobAccessFacet.RequestBase
                { }
            }, cancellationToken);

            var logger = this.serviceProvider.GetService<ILogger>();
            logger.LogInformation("Logging some info");

            var messageEvent = serviceProvider.GetService<IMessageEvent<SomeCrossVolatilityMessageEvent>>();
            await messageEvent.PublishAsync(new SomeCrossVolatilityMessageEvent
            {
                Context = stepComplete.Context,
                SomeProperty = "a property value"
            }, cancellationToken);

            jobAccessResponse = jobAccess.FilterAsync(new Request<JobAccessFacet.RequestBase>
            {
                Context = stepComplete.Context,
                Data = new JobAccessFacet.RequestBase
                { }
            }, cancellationToken);

            return new Response<OnStepActivateBase>
            {
                Data = new OnStepActivateBase
                {
                    SomeBaseClassField = "Hello World"
                }
            };
        }
    }
}
