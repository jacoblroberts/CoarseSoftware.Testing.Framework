namespace CoarseSoftware.BusinessSystem.Component.Manager.Administration.Service
{
    using CoarseSoftware.BusinessSystem.iFX;
    using Microsoft.Extensions.DependencyInjection;
    using JobAccessFacet = CoarseSoftware.BusinessSystem.Component.Access.Job.Interface;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Logging;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event.Model;
    using CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.iFX.Event;
    using CoarseSoftware.BusinessSystem.Component.Manager.Administration.Interface;

    public class AdministrationManager: IAdministrationManager
    {
        private readonly IServiceProvider serviceProvider;
        public AdministrationManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<Response<OnStepActivateBase>> FlowAsync(Request<OnStepCompleteBase> stepComplete, CancellationToken cancellationToken)
        {
            if (stepComplete.Data is CoarseSoftware.BusinessSystem.Component.Manager.Administration.Interface.UseCases.SomeContext.DerivedOnStepComplete)
            {
                return new Response<OnStepActivateBase>
                {
                    Data = new OnStepActivateBase
                    {
                        SomeBaseClassField = "Types Tested"
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
