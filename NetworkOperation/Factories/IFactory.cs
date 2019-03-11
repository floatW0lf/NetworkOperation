using System.Runtime.CompilerServices;

namespace NetworkOperation.Factories
{
    public interface IFactory<in TArg, out TInstance>
    {
        TInstance Create(TArg arg); 
    }
    
    public interface IFactory<in TArg0, in TArg1, out TInstance>
    {
        TInstance Create(TArg0 arg, TArg1 arg1); 
    }
}