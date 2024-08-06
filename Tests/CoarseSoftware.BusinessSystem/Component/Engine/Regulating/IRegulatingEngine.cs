namespace CoarseSoftware.BusinessSystem.Component.Engine.Regulating.Interface
{
    using CoarseSoftware.BusinessSystem.iFX;

    public interface IRegulatingEngine
    {
        Task<Response<ResponseBase>> ApplyAsync(Request<RequestBase> request, CancellationToken cancellationToken);
    }

    public class ResponseBase
    {
        public int Number {  get; set; }
    }

    public class RequestBase
    {
        public IEnumerable<int> Numbers { get; set; }
    }
}
