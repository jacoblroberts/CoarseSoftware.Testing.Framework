namespace CoarseSoftware.BusinessSystem.Component.Manager.Dashboard.Interface
{
    using CoarseSoftware.BusinessSystem.iFX;

    public interface IDashboardManager
    {
        Task<Response<OnStepActivateBase>> FlowAsync(Request<OnStepCompleteBase> stepComplete, CancellationToken cancellationToken);
    }

    public class OnStepCompleteBase { }
    public class OnStepActivateBase { }

    public class DerivedOnStepActivate: OnStepActivateBase
    {
        public string RandomId { get; set; }
        public string StaticId { get; set; }
    }
}
