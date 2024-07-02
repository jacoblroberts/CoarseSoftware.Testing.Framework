namespace CoarseSoftware.Testing.Framework.Core.Comparer
{
    /// <summary>
    /// Used to compare the expected object with the actual object
    /// </summary>
    public interface ITestExpectationComparer
    {
        void Compare(object expected, object actual, IEnumerable<string> ignoredPropertyNames);
    }

    /// <summary>
    /// Used to overwrite default ITestExpectationComparer behavior.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITestExpectationComparer<T>
    {
        // this wouldn't work because we could know the generated value.  
        // ie; during create data, the id is generated so we cant know.
        //     during view data, we send the id and know it.
        //     both of these return a view model.
        // how do we solve for this?
        // we need to be able to set up a value with new Generated<T>() : where T is the property type
        void Compare(T expected, T actual, IEnumerable<string> ignoredPropertyNames);
    }
}
