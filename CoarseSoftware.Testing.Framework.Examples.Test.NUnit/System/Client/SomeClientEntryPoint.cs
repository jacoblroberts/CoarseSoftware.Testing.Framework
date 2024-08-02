namespace CoarseSoftware.Testing.Framework.Examples.Test.NUnit.System.Client
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
            var dashboard = serviceProvider.GetService<CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Dashboard.Interface.IDashboardManager>();
            //var dashboard2 = serviceProvider.GetService(typeof(CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Dashboard.IDashboardManager));

            var response = await dashboard.FlowAsync(new Test.System.iFX.Request<Test.System.Component.Manager.Dashboard.Interface.OnStepCompleteBase>
            {
                Data = new Test.System.Component.Manager.Dashboard.Interface.OnStepCompleteBase
                { }
            }, CancellationToken.None);

            return new SomeClientEntryResponse();
        }

        public class SomeClientEntryResponse
        {

        }
    }
}
