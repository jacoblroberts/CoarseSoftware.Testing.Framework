namespace CoarseSoftware.BusinessSystem.Component.Engine.Validating.Service
{
    using CoarseSoftware.BusinessSystem.Component.Engine.Validating.Interface;
    using CoarseSoftware.BusinessSystem.iFX;

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
