namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating;
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

    public class RegulatingEngine : IRegulatingEngine
    {
        public async Task<Response<ResponseBase>> ApplyAsync(Request<RequestBase> request, CancellationToken cancellationToken)
        {
            // strategy 
            await Task.Yield();
            return new Response<ResponseBase>
            {
                Data = new ResponseBase
                { }
            };
        }
    }
}
