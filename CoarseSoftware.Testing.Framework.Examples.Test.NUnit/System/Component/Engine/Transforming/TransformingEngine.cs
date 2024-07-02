namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Transforming
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;
    public class TransformingEngine : ITransformingEngine
    {
        public async Task<Response<ResponseBase>> TransformAsync(Request<RequestBase> request, CancellationToken cancellationToken)
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
