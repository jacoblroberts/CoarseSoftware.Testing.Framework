namespace CoarseSoftware.BusinessSystem.Client
{
    using Microsoft.Extensions.DependencyInjection;

    public class SomeClientEntryPoint
    {
        private readonly IServiceProvider serviceProvider;

        public SomeClientEntryPoint(
            IServiceProvider serviceProvider
            )
        {
            this.serviceProvider = serviceProvider;        
        }

        public async Task<object> Start()
        {
            var dashboard = serviceProvider.GetService<CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface.IDashboardManager>();
            //var dashboard2 = serviceProvider.GetService(typeof(CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.IDashboardManager));

            var response = await dashboard.FlowAsync(new CoarseSoftware.BusinessSystem.iFX.Request<CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface.OnStepCompleteBase>
            {
                Data = new CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface.OnStepCompleteBase
                { }
            }, CancellationToken.None);

            return new SomeClientEntryResponse();
        }

        public class SomeClientEntryResponse
        {

        }
    }
}
