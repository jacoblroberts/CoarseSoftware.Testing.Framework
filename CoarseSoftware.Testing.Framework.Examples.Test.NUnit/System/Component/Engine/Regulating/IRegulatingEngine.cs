namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Engine.Regulating.Interface
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

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
