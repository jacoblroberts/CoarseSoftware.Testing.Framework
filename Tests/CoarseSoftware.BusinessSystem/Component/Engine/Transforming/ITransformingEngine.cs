namespace CoarseSoftware.BusinessSystem.Component.Engine.Transforming.Interface
{
    using CoarseSoftware.BusinessSystem.iFX;
    public interface ITransformingEngine
    {
        Task<Response<ResponseBase>> TransformAsync(Request<RequestBase> request, CancellationToken cancellationToken);
    }

    public class ResponseBase
    { }

    public class RequestBase
    { }
}
