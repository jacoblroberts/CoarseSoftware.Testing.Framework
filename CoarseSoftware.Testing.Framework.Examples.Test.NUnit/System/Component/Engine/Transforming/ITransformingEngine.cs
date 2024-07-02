namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Transforming
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;
    public interface ITransformingEngine
    {
        Task<Response<ResponseBase>> TransformAsync(Request<RequestBase> request, CancellationToken cancellationToken);
    }

    public class ResponseBase
    { }

    public class RequestBase
    { }
}
