#if !NETFRAMEWORK
namespace System.Runtime.Remoting.Messaging
{
    public static class CallContext
    {
        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext"/>.
        /// </summary>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or <see langword="null"/> if not found.</returns>
        public static object LogicalGetData(string name) => throw new NotSupportedException();
            
    }
}
#endif