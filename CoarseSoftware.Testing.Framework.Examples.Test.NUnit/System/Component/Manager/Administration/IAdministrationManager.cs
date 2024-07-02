namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;
    
    public interface IAdministrationManager
    {
        Task<Response<OnStepActivateBase>> FlowAsync(Request<OnStepCompleteBase> stepComplete, CancellationToken cancellationToken);
    }

    public class OnStepCompleteBase { }
    public class OnStepActivateBase
    {
        public string SomeBaseClassField { get; set; } = "Hello from base class";
    }
}
