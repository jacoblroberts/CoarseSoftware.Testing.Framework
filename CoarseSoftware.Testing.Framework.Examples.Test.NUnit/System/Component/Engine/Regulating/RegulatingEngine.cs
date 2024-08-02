namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating.Service
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating.Interface;
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;
    using Microsoft.Extensions.DependencyInjection;
    using JobAccessFacet = CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Access.Job.Interface;

    public class RegulatingEngine : IRegulatingEngine
    {
        private readonly IServiceProvider serviceProvider;

        public RegulatingEngine(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;    
        }

        public async Task<Response<ResponseBase>> ApplyAsync(Request<RequestBase> request, CancellationToken cancellationToken)
        {
            var jobAccess = serviceProvider.GetService<JobAccessFacet.IJobAccess>();
            var jobAccessResponse = await jobAccess.FilterAsync(new Request<JobAccessFacet.RequestBase>
            {
                Data = new JobAccessFacet.RequestBase
                { }
            }, cancellationToken);
            return new Response<ResponseBase>
            {
                Data = new ResponseBase
                { }
            };
        }
    }
}
