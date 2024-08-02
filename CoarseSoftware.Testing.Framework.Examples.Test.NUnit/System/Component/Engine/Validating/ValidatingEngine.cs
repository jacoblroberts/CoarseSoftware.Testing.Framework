namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating.Service
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Validating.Interface;
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

    public class ValidatingEngine : IValidatingEngine
    {
        public async Task<Response<ResponseBase>> ValidateAsync(Request<RequestBase> request, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return new Response<ResponseBase>
            {
                Data = new ResponseBase
                {
                    IsValid = true
                }
            };
        }
    }
}
