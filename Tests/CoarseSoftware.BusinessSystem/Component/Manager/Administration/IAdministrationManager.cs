namespace CoarseSoftware.BusinessSystem.Component.Manager.Administration.Interface
{
    using CoarseSoftware.BusinessSystem.iFX;
    
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

namespace CoarseSoftware.BusinessSystem.Component.Manager.Administration.Interface.UseCases.SomeContext
{
    public class DerivedOnStepComplete: OnStepCompleteBase
    {
        public DateTimeOffset DateTimeOffset { get; set; }
        public DateTime DateTime {  get; set; }
        public bool Bool {  get; set; }
        public int Int { get; set; }
        public decimal Decimal { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public string String { get; set; }
        public IEnumerable<string> Strings { get; set; }
        public IEnumerable<bool> Lists {  get; set; }
        public IEnumerable<SubClass> SubClasses { get; set; }
        public SubClass SubClassWithValue { get; set; }
        public SubClass SubClassNullValue { get; set; }
        public IEnumerable<SubClassWithDate> SubClassWithDates { get; set; }
        public SubClassToIgnore SubClassIgnore { get; set; }

        public class SubClassToIgnore
        {
            public string PropertyToIgnore { get; set; }
        }

        public class SubClass
        {
            public SomeEnum SomeEnum {  get; set; }
            public SomeEnum SomeEnumNull { get; set; }
        }

        public class SubClassWithDate
        {
            public DateTimeOffset Date { get; set; }
        }

        public enum SomeEnum
        {
            None,
            Value
        }
    }
}
