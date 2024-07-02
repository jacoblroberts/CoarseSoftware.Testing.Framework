namespace CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration
{
    using CoarseSoftware.Testing.Framework.Examples.Test.System.Component.Manager.Administration;
    using CoarseSoftware.Testing.Framework.Examples.Test.System.iFX;

    public class AdministrationManager: IAdministrationManager
    {
        public async Task<Response<OnStepActivateBase>> FlowAsync(Request<OnStepCompleteBase> stepComplete, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return new Response<OnStepActivateBase>
            {
                Data = new OnStepActivateBase
                {
                    SomeBaseClassField = "Hello World"
                }
            };
        }
    }
}
