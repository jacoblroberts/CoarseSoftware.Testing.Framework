namespace CoarseSoftware.Client.WebApi.Controllers
{
    using CoarseSoftware.BusinessSystem.iFX;
    using Microsoft.AspNetCore.Mvc;
    using DashboardManagerFacet = CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface;

    [ApiController]
    [Route("[controller]/[action]")]
    public class DashboardController : ControllerBase
    {
        private readonly IServiceProvider serviceProvider;

        public DashboardController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<DashboardManagerFacet.OnStepActivateBase> FlowAsync(CancellationToken cancellationToken)
        {
            var dashboardManager = serviceProvider.GetService<DashboardManagerFacet.IDashboardManager>();
            var response = await dashboardManager.FlowAsync(new Request<DashboardManagerFacet.OnStepCompleteBase>
            {
                Data = new DashboardManagerFacet.OnStepCompleteBase
                { }
            }, cancellationToken);
            return response.Data;
            //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            //    TemperatureC = Random.Shared.Next(-20, 55),
            //    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            //})
            //.ToArray();
        }
    }
}
