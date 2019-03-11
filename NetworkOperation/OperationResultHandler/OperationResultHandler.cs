using System;
using System.Globalization;
using NetworkOperation.StatusCodes;

namespace NetworkOperation.OperationResultHandler
{
    public struct OperationResultHandler<T>
    {
        internal OperationResultHandler(OperationResult<T> result, bool isHandled)
        {
            _result = result;
            _isHandled = isHandled;
        }

        private readonly OperationResult<T> _result;
        private readonly bool _isHandled;

        public OperationResultHandler<T> Success(Action<T> action)
        {
            if (_isHandled) return new OperationResultHandler<T>(default, true); 
            
            if (!_isHandled && _result.StatusCode == (uint) BuiltInOperationState.Success)
            {
                action(_result.Result); 
                return new OperationResultHandler<T>(default, true);
            }
            return new OperationResultHandler<T>(_result, false); 
        }

        public OperationResultHandler<T> BuiltInCode(BuiltInOperationState status, Action action)
        {
            if (_isHandled) return new OperationResultHandler<T>(default, true); 
            
            if (_result.StatusCode == (uint)status)
            {
                action();
                return new OperationResultHandler<T>(default, true);
            }
            return new OperationResultHandler<T>(_result, false); 
        }
        
        public OperationResultHandler<T> CustomCode<TEnum>(TEnum status, Action action) where TEnum : IConvertible
        {
            if (_isHandled) return new OperationResultHandler<T>(default, true); 
            
            if (StatusEncoding.IsValidValue<TEnum>(_result.StatusCode) && _result.StatusCode == status.ToUInt32(CultureInfo.InvariantCulture))
            {
                action();
                return new OperationResultHandler<T>(default, true);
            }
            return new OperationResultHandler<T>(_result, false); 
        }
        
    }
}